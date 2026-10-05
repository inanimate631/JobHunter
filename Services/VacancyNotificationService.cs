using VacancyApi.Data;
using VacancyApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace VacancyApi.Services;

public class VacancyNotificationService
{
  private readonly TelegramService _telegramService;
  private readonly VacancyDbContext _dbContext;

  public VacancyNotificationService(TelegramService telegramService, VacancyDbContext dbContext)
  {
    _telegramService = telegramService;
    _dbContext = dbContext;
  }

  public async Task NotifyUsersAsync(User user, List<ScoredVacancy> scoredVacancies)
  {
    foreach (var scoredVacancy in scoredVacancies)
    {
      var isAlreadyNotified = await _dbContext.UserVacancies
        .AnyAsync(uv => uv.UserId == user.Id && uv.VacancyId == scoredVacancy.Vacancy.Id);

      if (!isAlreadyNotified)
      {
        var vacancy = scoredVacancy.Vacancy;

        await _telegramService.SendMessageAsync(
            user.TelegramChatId,
            BuildVacancyMessage(vacancy, scoredVacancy.Score));

        var userVacancy = new UserVacancy
        {
          UserId = user.Id,
          VacancyId = scoredVacancy.Vacancy.Id,
        };

        _dbContext.UserVacancies.Add(userVacancy);
        await _dbContext.SaveChangesAsync();
      }
    }
  }

  public async Task SendInitialVacanciesAsync(
     User user,
     List<ScoredVacancy> scoredVacancies)
  {
    var existingVacancyIds = await _dbContext.UserVacancies
    .Where(uv => uv.UserId == user.Id)
    .Select(uv => uv.VacancyId)
    .ToHashSetAsync();

    var newVacancies = scoredVacancies
        .Where(v => !existingVacancyIds.Contains(v.Vacancy.Id))
        .ToList();

    foreach (var scoredVacancy in newVacancies)
    {
      _dbContext.UserVacancies.Add(new UserVacancy
      {
        UserId = user.Id,
        VacancyId = scoredVacancy.Vacancy.Id,
        SendAt = DateTime.UtcNow
      });
    }

    await _dbContext.SaveChangesAsync();

    var topVacancies = newVacancies
        .OrderByDescending(v => v.Score)
        .Take(5)
        .ToList();

    var message = new StringBuilder();

    message.AppendLine("🔥 ТОП-5 вакансій");
    message.AppendLine();

    foreach (var scoredVacancy in topVacancies)
    {
      message.AppendLine(
          BuildVacancyMessage(
              scoredVacancy.Vacancy,
              scoredVacancy.Score));

      message.AppendLine("━━━━━━━━━━━━");
      message.AppendLine();
    }

    await _telegramService.SendMessageAsync(
        user.TelegramChatId,
        message.ToString());
  }

  private string BuildVacancyMessage(Vacancy vacancy, int score)
  {
    var message = new StringBuilder();

    message.AppendLine($"🔥 {vacancy.Title}");
    message.AppendLine();
    message.AppendLine($"🏢 {vacancy.Company}");

    if (vacancy.Experience is not null)
      message.AppendLine($"💼 Досвід: {vacancy.Experience}");

    if (vacancy.EnglishLevel is not null)
      message.AppendLine($"🇬🇧 Англійська: {vacancy.EnglishLevel}");

    if (vacancy.WorkType is not null)
      message.AppendLine($"💻 Тип роботи: {vacancy.WorkType}");

    message.AppendLine($"🌐 Джерело: {vacancy.Source}");
    message.AppendLine($"⭐ Рейтинг: {score}");
    message.AppendLine();
    message.AppendLine($"🔗 {vacancy.Link}");

    return message.ToString();
  }
}