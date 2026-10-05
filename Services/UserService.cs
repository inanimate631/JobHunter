using Microsoft.EntityFrameworkCore;
using VacancyApi.Data;
using VacancyApi.Models;

namespace VacancyApi.Services;

public class UserService
{
  private readonly VacancyDbContext _dbContext;

  public UserService(VacancyDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<User> GetOrCreateUserAsync(long telegramChatId)
  {
    var user = await _dbContext.Users
      .Include(user => user.SearchPreferences)
      .FirstOrDefaultAsync(u => u.TelegramChatId == telegramChatId);

    if (user == null)
    {
      user = new User
      {
        TelegramChatId = telegramChatId,
        SearchPreferences = new SearchPreferences()
      };

      _dbContext.Users.Add(user);
      await _dbContext.SaveChangesAsync();
    }

    return user;
  }

  public async Task<SearchPreferences?> GetUserPreferencesAsync(long telegramChatId)
  {
    var user = await _dbContext.Users
      .Include(u => u.SearchPreferences)
      .FirstOrDefaultAsync(u => u.TelegramChatId == telegramChatId);

    if (user == null)
    {
      return null;
    }

    return user.SearchPreferences;
  }

  public async Task<User> SaveUserPreferencesAsync(long telegramChatId, SearchPreferences preferences)
  {
    var user = await _dbContext.Users
      .Include(u => u.SearchPreferences)
      .FirstOrDefaultAsync(u => u.TelegramChatId == telegramChatId);

    if (user == null)
    {
      user = new User
      {
        TelegramChatId = telegramChatId,
        SearchPreferences = preferences
      };

      _dbContext.Users.Add(user);
      await _dbContext.SaveChangesAsync();
    }
    else
    {
      user.SearchPreferences = preferences;
      await _dbContext.SaveChangesAsync();
    }
    return  user;
  }

  public async Task<List<User>> GetAllUsersWithPreferencesAsync()
  {
    var userWithPreferences = await _dbContext.Users.Include(u => u.SearchPreferences)
      .ToListAsync();

    return userWithPreferences;
  }
}