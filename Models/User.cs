
namespace VacancyApi.Models;

public class User
{
  public int Id { get; set; }
  public long TelegramChatId { get; set; }

  public SearchPreferences SearchPreferences { get; set; } = null!;
}