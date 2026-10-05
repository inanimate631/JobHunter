using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using VacancyApi.Models;
using VacancyApi.Interfaces;

namespace VacancyApi.Services;

public class DjinniParser : ICharacterService
{
  private readonly HttpClient _httpClient;
  public async Task<IDocument> GetPageAsync(string url)
  {
    var response = await _httpClient.GetAsync(url);
    response.EnsureSuccessStatusCode();
    var parser = new HtmlParser();
    var html = await response.Content.ReadAsStringAsync();

    var document = await parser.ParseDocumentAsync(html);

    return document;
  }

  public IEnumerable<Vacancy> GetVacancies(IDocument document)
  {
    var jobs = document.QuerySelectorAll(".job-item");

    var vacancies = new List<Vacancy>();

    foreach (var job in jobs)
    {
      var externalId = int.Parse(job.Id.Replace("job-item-", ""));
      var title = job.QuerySelector(".job-item__position").TextContent.Trim();
      var company = job.QuerySelector(".small.text-gray-800.opacity-75.font-weight-500").TextContent.Trim();
      var link = "https://djinni.co" + job.QuerySelector("a.job_item__header-link").GetAttribute("href");

      var spans = job.QuerySelectorAll("span.text-nowrap");

      var experience = spans.FirstOrDefault(span => span.TextContent.Contains("досвіду"))?.TextContent.Trim();
      var englishLevel = spans.FirstOrDefault(span => span.TextContent.Contains("Англійська"))?.TextContent.Trim();
      var source = "DJINNI";

      var workType = spans
     .FirstOrDefault(span =>
         span.TextContent.Contains("віддалено", StringComparison.OrdinalIgnoreCase) ||
         span.TextContent.Contains("офіс", StringComparison.OrdinalIgnoreCase) ||
         span.TextContent.Contains("гібрид", StringComparison.OrdinalIgnoreCase))
     ?.TextContent.Trim();

      var vacancy = new Vacancy(externalId, title, company, link, experience, englishLevel, source, workType);

      vacancies.Add(vacancy);
    }

    return vacancies;
  }

  public DjinniParser(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }
}