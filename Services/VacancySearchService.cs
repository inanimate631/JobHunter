
using VacancyApi.Models;

namespace VacancyApi.Services;

public class VacancySearchService
{

  private readonly DjinniParser _djinniParser;
  private readonly WorkUaParser _workUaParser;
  private readonly RobotaUaParser _robotaUaParser;
  private readonly DouParser _douParser;
  private readonly VacancyScoringService _vacancyScoringService;

  private readonly VacancyService _vacancyService;

  public VacancySearchService(
    DjinniParser djinniParser,
    VacancyScoringService vacancyScoringService,
    VacancyService vacancyService,
    WorkUaParser workUaParser,
    RobotaUaParser robotaUaParser,
    DouParser douParser
  )
  {
    _djinniParser = djinniParser;
    _vacancyScoringService = vacancyScoringService;
    _vacancyService = vacancyService;
    _workUaParser = workUaParser;
    _robotaUaParser = robotaUaParser;
    _douParser = douParser;
  }

  public async Task<List<ScoredVacancy>> GetVacanciesList(SearchPreferences searchPreferences)
  {
    var keywords = string.Join(
     " ",
     searchPreferences.Keywords.Select(Uri.EscapeDataString));
    var workUaKeywords = string.Join(
     "+",
     searchPreferences.Keywords.Select(Uri.EscapeDataString));
    var robotaUaKeywords = string.Join(
         "-",
         searchPreferences.Keywords.Select(Uri.EscapeDataString));
    var douKeywords = string.Join(
             "+",
             searchPreferences.Keywords.Select(Uri.EscapeDataString));

    var djinniUrl = $"https://djinni.co/jobs/?all_keywords={keywords}&search_type=basic-search";
    var workUaUrl = $"https://www.work.ua/jobs-{workUaKeywords}";
    var robotaUaUrl = $"https://api.rabota.ua/vacancy/search?keyWords={robotaUaKeywords}";
    var douUrl = $"https://jobs.dou.ua/vacancies/?search={douKeywords}";

    var djinniHtml = _djinniParser.GetPageAsync(djinniUrl);
    var workUaHtml = _workUaParser.GetPageAsync(workUaUrl);
    var robotaUaHtml = _robotaUaParser.GetPageAsync(robotaUaUrl);
    var douHtml = _douParser.GetPageAsync(douUrl);

    await Task.WhenAll(
        djinniHtml,
        workUaHtml,
        robotaUaHtml,
        douHtml
    );
    var vacancies = _djinniParser.GetVacancies(await djinniHtml).ToList();
    vacancies.AddRange(_workUaParser.GetVacancies(await workUaHtml));
    vacancies.AddRange(_robotaUaParser.GetVacancies(await robotaUaHtml));
    vacancies.AddRange(_douParser.GetVacancies(await douHtml));

    var scoredVacancies = new List<ScoredVacancy>();

    foreach (var vacancy in vacancies)
    {
      var existingVacancy = await _vacancyService.GetByExternalIdAsync(
          vacancy.Source,
          vacancy.ExternalId);

      var vacancyToReturn = existingVacancy;

      if (existingVacancy == null)
      {
        vacancyToReturn = await _vacancyService.AddVacancy(vacancy);
      }

      var score = _vacancyScoringService.CalculateScore(
          vacancyToReturn,
          searchPreferences);

      if (existingVacancy == null)
      {
        score += 2;
      }

      var scored = new ScoredVacancy
      {
        Vacancy = vacancyToReturn,
        Score = score
      };

      scoredVacancies.Add(scored);
    }
    return scoredVacancies;
  }
}