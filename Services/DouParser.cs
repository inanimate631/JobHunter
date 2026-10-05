using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using VacancyApi.Models;
using VacancyApi.Interfaces;

namespace VacancyApi.Services;

public class DouParser : ICharacterService
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
    var jobs = document.QuerySelectorAll(".l-vacancy");

    var vacancies = new List<Vacancy>();

    foreach (var job in jobs)
    {
      // Link
      var link = job
          .QuerySelector("a.vt")
          ?.GetAttribute("href");

      if (string.IsNullOrWhiteSpace(link))
      {
        continue;
      }

      // ExternalId
      var url = new Uri(link);

      var externalIdString = url
          .AbsolutePath
          .TrimEnd('/')
          .Split('/')
          .Last();

      if (!int.TryParse(externalIdString, out var externalId))
      {
        continue;
      }

      // Title
      var title = job
          .QuerySelector("a.vt")
          ?.TextContent
          .Trim();

      if (string.IsNullOrWhiteSpace(title))
      {
        continue;
      }

      // Company
      var company = job
          .QuerySelector("a.company")
          ?.TextContent
          .Trim();

      if (string.IsNullOrWhiteSpace(company))
      {
        continue;
      }

      // Location
      var location = job
          .QuerySelector(".cities")
          ?.TextContent
          .Trim();

      // Work type
      string? workType = null;

      if (!string.IsNullOrWhiteSpace(location))
      {
        if (location.Contains("віддалено", StringComparison.OrdinalIgnoreCase))
        {
          workType = "Віддалено";
        }
        else if (location.Contains("гібрид", StringComparison.OrdinalIgnoreCase))
        {
          workType = "Гібрид";
        }
        else if (location.Contains("офіс", StringComparison.OrdinalIgnoreCase))
        {
          workType = "Офіс";
        }
      }

      // DOU doesn't provide experience
      // and English level as separate fields in this list.
      var experience = "";
      var englishLevel = "";

      var source = "DOU";

      var vacancy = new Vacancy(
          externalId,
          title,
          company,
          link,
          experience,
          englishLevel,
          source,
          workType ?? ""
      );

      vacancies.Add(vacancy);
    }

    return vacancies;
  }

  public DouParser(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }
}