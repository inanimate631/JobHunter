namespace VacancyApi.Models;

public class Vacancy
{
  public int Id { get; private set; }
  public int ExternalId { get; private set; }
  public string Title { get; private set; }

  public string Company { get; private set; }

  public string Link { get; private set; }

  public string? Experience { get; private set; }

  public string? EnglishLevel { get; private set; }
  public string Source { get; private set; }

  public string? WorkType { get; private set; }

  public DateTime CreatedAt { get; private set; }
  public DateTime LastSeenAt { get; private set; }
  public bool IsActive { get; private set; }

  public Vacancy(int externalId, string title, string company, string link, string? experience, string? englishLevel, string source, string? workType)
  {
    ExternalId = externalId;
    Title = title;
    Company = company;
    Link = link;
    Experience = experience;
    EnglishLevel = englishLevel;
    Source = source;
    WorkType = workType;
  }
}