using VacancyApi.Models;
using VacancyApi.Interfaces;

namespace VacancyApi.Services;

public class VacancyScoringService()
{
  private static readonly string[] EnglishLevels =
[
    "A1",
    "A2",
    "B1",
    "B2",
    "C1",
    "C2"
];

  private string? ExtractEnglishLevel(string? value)
  {
    if (value == null)
    {
      return null;
    }
    return EnglishLevels.FirstOrDefault(level => value.Contains(level, StringComparison.OrdinalIgnoreCase));
  }

  private int GetEnglishLevelNumber(string level)
  {
    return Array.IndexOf(EnglishLevels, level) + 1;
  }

  private int CalculateEnglishScore(
    string? vacancyLevel,
    string? userLevel)
  {
    if (vacancyLevel == null || userLevel == null)
    {
      return 0;
    }

    var vacancyLevelNumber = GetEnglishLevelNumber(vacancyLevel);
    var userLevelNumber = GetEnglishLevelNumber(userLevel);

    if (vacancyLevelNumber == userLevelNumber)
    {
      return 1;
    }

    return vacancyLevelNumber < userLevelNumber ? 2 : 0;
  }

  public int? ExtractExperienceYears(string? value)
  {
    if (string.IsNullOrWhiteSpace(value))
      return null;

    int years;

    if (int.TryParse(value.Split(' ')[0], out years))
      return years;

    return 0;
  }

  public int CalculateExperienceScore(int? vacancyYears, int? userYears)
  {
    if (!vacancyYears.HasValue || !userYears.HasValue)
    {
      return 0;
    }

    if (vacancyYears == userYears)
    {
      return 1;
    }

    return vacancyYears < userYears ? 2 : 0;
  }

  public int CalculateScore(Vacancy vacancy, SearchPreferences preferences)
  {
    var score = 0;

    if (preferences.Positions.Any(position => vacancy.Title.Contains(position, StringComparison.OrdinalIgnoreCase)))
    {
      score += 2;
    }

    score += CalculateEnglishScore(ExtractEnglishLevel(vacancy.EnglishLevel), preferences.EnglishLevel);

    score += CalculateExperienceScore(ExtractExperienceYears(vacancy.Experience), preferences.ExperienceYears);

    return score;
  }
}