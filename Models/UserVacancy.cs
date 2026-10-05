namespace VacancyApi.Models;

public class UserVacancy
{
  public int VacancyId { get; set; }
  public int UserId { get; set; }

  public DateTime SendAt { get; set; }

  public User User { get; set; } = null!;
  public Vacancy Vacancy { get; set; } = null!;
}