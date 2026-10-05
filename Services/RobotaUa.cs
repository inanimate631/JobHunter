
using System.Text.Json;
using VacancyApi.Models;

namespace VacancyApi.Services;

public class RobotaUaParser
{
  private readonly HttpClient _httpClient;

  public RobotaUaParser(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task<string> GetPageAsync(string url)
  {
    var response = await _httpClient.GetAsync(url);

    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsStringAsync();
  }

  public IEnumerable<Vacancy> GetVacancies(string json)
  {
    var data = JsonSerializer.Deserialize<RobotaSearchResponse>(
        json,
        new JsonSerializerOptions
        {
          PropertyNameCaseInsensitive = true
        });

    if (data == null)
      return [];

    var vacancies = new List<Vacancy>();

    foreach (var item in data.Documents)
    {
      vacancies.Add(
          new Vacancy(
             item.Id,
            item.Name,
            item.CompanyName,
            $"https://robota.ua/company{item.NotebookId}/vacancy{item.Id}",
            "",
            "",
            "ROBOTA_UA",
            ""
          )
      );
    }

    return vacancies;
  }
}