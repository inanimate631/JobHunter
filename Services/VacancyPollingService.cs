namespace VacancyApi.Services;

public class VacancyPollingService : BackgroundService
{
  private readonly IServiceScopeFactory _scopeFactory;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      using var scope = _scopeFactory.CreateScope();
      var vacancySearchService = scope.ServiceProvider.GetRequiredService<VacancySearchService>();
      var userService = scope.ServiceProvider.GetRequiredService<UserService>();
      var vacancyNotificationService = scope.ServiceProvider.GetRequiredService<VacancyNotificationService>();

      var users = await userService.GetAllUsersWithPreferencesAsync();

      foreach (var user in users)
      {
        if (user.SearchPreferences.Keywords.Count == 0 || user.SearchPreferences.Positions.Count == 0 || user.SearchPreferences.WorkType is null || user.SearchPreferences.EnglishLevel is null || user.SearchPreferences.ExperienceYears is null)
        {
          continue;
        }
        var vacancies = await vacancySearchService.GetVacanciesList(
            user.SearchPreferences);
        await vacancyNotificationService.NotifyUsersAsync(user, vacancies);
      }

      await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
    }
  }


  public VacancyPollingService(
    IServiceScopeFactory scopeFactory
  )
  {
    _scopeFactory = scopeFactory;
  }

}