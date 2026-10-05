
namespace VacancyApi.Models;

public class SearchPreferences
{
  public int Id { get; set; }
  public List<string> Keywords { get; set; } = [];

  public List<string> Positions { get; set; } = [];
  public string? WorkType { get; set; }
  public string? EnglishLevel { get; set; }
  public int? ExperienceYears { get; set; }

  public int UserId { get; set; }

  public User User { get; set; } = null!;
}