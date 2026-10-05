using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using VacancyApi.Models;

namespace VacancyApi.Services;

public class WorkUaParser
{
  private readonly BrowserService _browser;

  public WorkUaParser(BrowserService browser)
  {
    _browser = browser;
  }

  public async Task<IDocument> GetPageAsync(string url)
  {
    Console.WriteLine($"Opening Work.ua: {url}");

    var html = await _browser.GetPageHtmlAsync(url);

    Console.WriteLine($"Work.ua HTML length: {html.Length}");

    var parser = new HtmlParser();

    return await parser.ParseDocumentAsync(html);
  }

  public IEnumerable<Vacancy> GetVacancies(IDocument document)
  {
    var jobs = document.QuerySelectorAll(".job-link");

    Console.WriteLine($"Work.ua job elements: {jobs.Length}");

    var vacancies = new List<Vacancy>();

    foreach (var job in jobs)
    {
      var externalIdString = job.GetAttribute("data-id");

      if (!int.TryParse(externalIdString, out var externalId))
      {
        continue;
      }

      var titleElement = job.QuerySelector("h2 a");

      var title = titleElement?
          .TextContent
          .Trim();

      var link = titleElement?
          .GetAttribute("href");

      var company = job
          .QuerySelector(".glyphicon-company")
          ?.ParentElement?
          .QuerySelector(".strong-600")
          ?.TextContent
          .Trim();

      var workType = job
          .QuerySelector(".glyphicon-map-marker")
          ?.ParentElement?
          .TextContent
          .Trim();

      var experience = job
          .QuerySelector(".glyphicon-list")
          ?.ParentElement?
          .TextContent
          .Trim();


      const string englishLevel = "";
      const string source = "WORK_UA";

      if (string.IsNullOrWhiteSpace(title) ||
          string.IsNullOrWhiteSpace(link) ||
          string.IsNullOrWhiteSpace(company))
      {
        Console.WriteLine(
            $"Skipping Work.ua vacancy {externalId}: " +
            $"title={title}, company={company}, link={link}");

        continue;
      }

      if (link.StartsWith("/"))
      {
        link = "https://www.work.ua" + link;
      }

      var vacancy = new Vacancy(
          externalId,
          title,
          company,
          link,
          experience ?? "",
          englishLevel,
          source,
          workType ?? "");

      vacancies.Add(vacancy);
    }

    Console.WriteLine($"Work.ua vacancies parsed: {vacancies.Count}");

    return vacancies;
  }
}