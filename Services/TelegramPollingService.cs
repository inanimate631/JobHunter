using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using AppUser = VacancyApi.Models.User;

namespace VacancyApi.Services;

public class TelegramPollingService : BackgroundService
{
  private readonly TelegramBotClient _botClient;
  private readonly IServiceScopeFactory _scopeFactory;

  public TelegramPollingService(
      TelegramBotClient botClient,
      IServiceScopeFactory scopeFactory)
  {
    _botClient = botClient;
    _scopeFactory = scopeFactory;
  }

  protected override async Task ExecuteAsync(
      CancellationToken stoppingToken)
  {
    var receiverOptions = new ReceiverOptions
    {
      AllowedUpdates = [UpdateType.Message]
    };

    _botClient.StartReceiving(
        HandleUpdateAsync,
        HandleErrorAsync,
        receiverOptions,
        stoppingToken);

    var me = await _botClient.GetMe(stoppingToken);

    Console.WriteLine(
        $"Telegram bot started: @{me.Username}");
  }

  private async Task HandleUpdateAsync(
      ITelegramBotClient bot,
      Update update,
      CancellationToken cancellationToken)
  {
    if (update.Message?.Text == null)
      return;

    var chatId = update.Message.Chat.Id;
    var message = update.Message.Text;

    var keyboard = CreateMainKeyboard();

    using var scope = _scopeFactory.CreateScope();

    var userService =
        scope.ServiceProvider.GetRequiredService<UserService>();

    if (message == "/start")
    {
      await HandleStartAsync(
          bot,
          chatId,
          userService,
          keyboard,
          cancellationToken);

      return;
    }

    var menuHandled = await HandleMenuAsync(
        bot,
        chatId,
        message,
        userService,
        scope,
        keyboard,
        cancellationToken);

    if (menuHandled)
      return;

    await HandleOnboardingAsync(
        bot,
        chatId,
        message,
        userService,
        scope,
        keyboard,
        cancellationToken);
  }

  private ReplyKeyboardMarkup CreateMainKeyboard()
  {
    return new ReplyKeyboardMarkup([
        ["🔎 Налаштувати пошук", "📋 Мої налаштування"],
            ["🔄 Перевірити зараз", "ℹ️ Як це працює"]
    ])
    {
      ResizeKeyboard = true
    };
  }

  private async Task HandleStartAsync(
      ITelegramBotClient bot,
      long chatId,
      UserService userService,
      ReplyKeyboardMarkup keyboard,
      CancellationToken cancellationToken)
  {
    await userService.GetOrCreateUserAsync(chatId);

    await bot.SendMessage(
        chatId,
        "🐩 Gaf! Я Ричик — твій персональний шукач вакансій!\n\n" +
        "Я шукаю вакансії на Djinni, Work.ua, Robota.ua та DOU.\n\n" +
        "Обери, що хочеш зробити 👇",
        replyMarkup: keyboard,
        cancellationToken: cancellationToken);
  }

  private async Task<bool> HandleMenuAsync(
      ITelegramBotClient bot,
      long chatId,
      string message,
      UserService userService,
      IServiceScope scope,
      ReplyKeyboardMarkup keyboard,
      CancellationToken cancellationToken)
  {
    if (message == "🔎 Налаштувати пошук")
    {
      await ResetSearchAsync(
          bot,
          chatId,
          userService,
          keyboard,
          cancellationToken);

      return true;
    }

    if (message == "📋 Мої налаштування")
    {
      await HandleMySettingsAsync(
          bot,
          chatId,
          userService,
          keyboard,
          cancellationToken);

      return true;
    }

    if (message == "🔄 Перевірити зараз")
    {
      await HandleCheckNowAsync(
          bot,
          chatId,
          userService,
          scope,
          keyboard,
          cancellationToken);

      return true;
    }

    if (message == "ℹ️ Як це працює")
    {
      await HandleHowItWorksAsync(
          bot,
          chatId,
          keyboard,
          cancellationToken);

      return true;
    }

    return false;
  }

  private async Task ResetSearchAsync(
      ITelegramBotClient bot,
      long chatId,
      UserService userService,
      ReplyKeyboardMarkup keyboard,
      CancellationToken cancellationToken)
  {
    var preferences =
        await userService.GetUserPreferencesAsync(chatId);

    if (preferences == null)
    {
      await bot.SendMessage(
          chatId,
          "🐩 Gaf! Не вдалося отримати твої налаштування.",
          replyMarkup: keyboard,
          cancellationToken: cancellationToken);

      return;
    }

    preferences.Keywords.Clear();
    preferences.Positions.Clear();
    preferences.WorkType = null;
    preferences.EnglishLevel = null;
    preferences.ExperienceYears = null;

    await userService.SaveUserPreferencesAsync(
        chatId,
        preferences);

    await bot.SendMessage(
        chatId,
        "🐩 Gaf! Давай налаштуємо пошук заново.\n\n" +
        "Введи ключові слова для пошуку вакансій.",
       replyMarkup: new ReplyKeyboardRemove(),
        cancellationToken: cancellationToken);
  }

  private async Task HandleMySettingsAsync(
      ITelegramBotClient bot,
      long chatId,
      UserService userService,
      ReplyKeyboardMarkup keyboard,
      CancellationToken cancellationToken)
  {
    var preferences =
        await userService.GetUserPreferencesAsync(chatId);

    if (preferences == null)
    {
      await bot.SendMessage(
          chatId,
          "🐩 Gaf! Не вдалося отримати твої налаштування.",
          replyMarkup: keyboard,
          cancellationToken: cancellationToken);

      return;
    }

    var keywords = preferences.Keywords.Count > 0
        ? string.Join(", ", preferences.Keywords)
        : "не вказано";

    var positions = preferences.Positions.Count > 0
        ? string.Join(", ", preferences.Positions)
        : "не вказано";

    await bot.SendMessage(
        chatId,
        $"📋 Твої налаштування:\n\n" +
        $"🔎 Ключові слова: {keywords}\n" +
        $"💼 Посади: {positions}\n" +
        $"💻 Тип роботи: {preferences.WorkType ?? "не вказано"}\n" +
        $"🇬🇧 Англійська: {preferences.EnglishLevel ?? "не вказано"}\n" +
        $"🧑‍💻 Досвід: {preferences.ExperienceYears?.ToString() ?? "не вказано"} років",
        replyMarkup: keyboard,
        cancellationToken: cancellationToken);
  }

  private ReplyKeyboardMarkup CreateWorkTypeKeyboard()
  {
    return new ReplyKeyboardMarkup([
        ["🏢 Офіс", "🏠 Віддалено"],
        ["🔀 Гібрид"]
    ])
    {
      ResizeKeyboard = true,
      OneTimeKeyboard = true
    };
  }
  private ReplyKeyboardMarkup CreateEnglishKeyboard()
  {
    return new ReplyKeyboardMarkup([
        ["A1", "A2", "B1"],
        ["B2", "C1", "C2"]
    ])
    {
      ResizeKeyboard = true,
      OneTimeKeyboard = true
    };
  }

  private async Task HandleCheckNowAsync(
      ITelegramBotClient bot,
      long chatId,
      UserService userService,
      IServiceScope scope,
      ReplyKeyboardMarkup keyboard,
      CancellationToken cancellationToken)
  {
    var user =
        await userService.GetOrCreateUserAsync(chatId);

    if (!IsSearchConfigured(user))
    {
      await bot.SendMessage(
          chatId,
          "🐩 Спочатку потрібно налаштувати пошук.",
          replyMarkup: keyboard,
          cancellationToken: cancellationToken);

      return;
    }

    await bot.SendMessage(
        chatId,
        "🔄 Перевіряю вакансії на Djinni, Work.ua, Robota.ua та DOU...",
        replyMarkup: keyboard,
        cancellationToken: cancellationToken);

    var vacancySearchService =
        scope.ServiceProvider
            .GetRequiredService<VacancySearchService>();

    var vacancyNotificationService =
        scope.ServiceProvider
            .GetRequiredService<VacancyNotificationService>();

    var vacancies =
        await vacancySearchService.GetVacanciesList(
            user.SearchPreferences);

    Console.WriteLine(
        $"Всего scoredVacancies: {vacancies.Count}");

    await vacancyNotificationService.NotifyUsersAsync(
        user,
        vacancies);

    await bot.SendMessage(
        chatId,
        "🐩 Gaf! Перевірка завершена.",
        replyMarkup: keyboard,
        cancellationToken: cancellationToken);
  }

  private async Task HandleHowItWorksAsync(
      ITelegramBotClient bot,
      long chatId,
      ReplyKeyboardMarkup keyboard,
      CancellationToken cancellationToken)
  {
    await bot.SendMessage(
        chatId,
        "🐩 Я шукаю вакансії на:\n\n" +
        "• Djinni\n" +
        "• Work.ua\n" +
        "• Robota.ua\n" +
        "• DOU\n\n" +
        "Ти задаєш параметри пошуку, а я періодично " +
        "перевіряю нові вакансії та повідомляю тебе " +
        "про підходящі. 🔥",
        replyMarkup: keyboard,
        cancellationToken: cancellationToken);
  }

  private async Task HandleOnboardingAsync(
      ITelegramBotClient bot,
      long chatId,
      string message,
      UserService userService,
      IServiceScope scope,
      ReplyKeyboardMarkup keyboard,
      CancellationToken cancellationToken)
  {
    var preferences =
        await userService.GetUserPreferencesAsync(chatId);

    if (preferences == null)
    {
      await bot.SendMessage(
          chatId,
          "🐩 Gaf! Сталася помилка при отриманні ваших налаштувань.",
          replyMarkup: keyboard,
          cancellationToken: cancellationToken);

      return;
    }

    if (preferences.Keywords.Count == 0)
    {
      preferences.Keywords = message
          .Split(",")
          .Select(k => k.Trim())
          .Where(k => !string.IsNullOrWhiteSpace(k))
          .ToList();

      await userService.SaveUserPreferencesAsync(
          chatId,
          preferences);

      await bot.SendMessage(
          chatId,
          "🐩 Gaf! Ваші ключові слова збережено.\n\n" +
          "Введи бажану посаду, наприклад: Frontend Developer",
         replyMarkup: new ReplyKeyboardRemove(),
          cancellationToken: cancellationToken);

      return;
    }

    if (preferences.Positions.Count == 0)
    {
      preferences.Positions = message
          .Split(",")
          .Select(k => k.Trim())
          .Where(k => !string.IsNullOrWhiteSpace(k))
          .ToList();

      await userService.SaveUserPreferencesAsync(
          chatId,
          preferences);

      await bot.SendMessage(
          chatId,
          "🐩 Gaf! Посаду збережено.\n\n" +
          "Введи тип роботи, яку шукаєш: Офіс / Віддалено",
           replyMarkup: CreateWorkTypeKeyboard(),
          cancellationToken: cancellationToken);

      return;
    }

    if (preferences.WorkType is null)
    {
      preferences.WorkType = message.Trim();

      await userService.SaveUserPreferencesAsync(
          chatId,
          preferences);

      await bot.SendMessage(
          chatId,
          "🐩 Gaf! Тип роботи збережено.\n\n" +
          "Введи рівень англійської: A2, B1, B2 або C1",
          replyMarkup: CreateEnglishKeyboard(),
          cancellationToken: cancellationToken);

      return;
    }

    if (preferences.EnglishLevel is null)
    {
      preferences.EnglishLevel = message.Trim();

      await userService.SaveUserPreferencesAsync(
          chatId,
          preferences);

      await bot.SendMessage(
          chatId,
          "🐩 Gaf! Рівень англійської збережено.\n\n" +
          "Введи досвід у роках цифрою.\n" +
          "Наприклад: 3\n" +
          "Або 0, якщо досвіду немає.",
         replyMarkup: new ReplyKeyboardRemove(),
          cancellationToken: cancellationToken);

      return;
    }

    if (preferences.ExperienceYears is null)
    {
      if (!int.TryParse(message.Trim(), out var years) ||
          years < 0)
      {
        await bot.SendMessage(
            chatId,
            "🐩 Gaf! Введи кількість років цифрою.\n\n" +
            "Наприклад: 3 або 0",
           replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);

        return;
      }

      preferences.ExperienceYears = years;

      var user =
          await userService.SaveUserPreferencesAsync(
              chatId,
              preferences);

      await bot.SendMessage(
          chatId,
          "🐶 Gaf! Налаштування завершено!\n\n" +
          "Я перевірю вакансії та буду повідомляти " +
          "тебе про нові підходящі вакансії.\n\n" +
          "Показую найкращі 5 🔥",
           replyMarkup: CreateMainKeyboard(),
          cancellationToken: cancellationToken);

      var vacancySearchService =
          scope.ServiceProvider
              .GetRequiredService<VacancySearchService>();

      var vacancyNotificationService =
          scope.ServiceProvider
              .GetRequiredService<VacancyNotificationService>();

      var vacancies =
          await vacancySearchService.GetVacanciesList(
              user.SearchPreferences);

      Console.WriteLine(
          $"Всего scoredVacancies: {vacancies.Count}");

      await vacancyNotificationService
          .SendInitialVacanciesAsync(
              user,
              vacancies);

      return;
    }
  }

  private bool IsSearchConfigured(AppUser user)
  {
    var preferences = user.SearchPreferences;

    return preferences.Keywords.Count > 0 &&
           preferences.Positions.Count > 0 &&
           preferences.WorkType is not null &&
           preferences.EnglishLevel is not null &&
           preferences.ExperienceYears is not null;
  }

  private Task HandleErrorAsync(
      ITelegramBotClient bot,
      Exception exception,
      CancellationToken cancellationToken)
  {
    Console.WriteLine(
        $"Telegram error: {exception}");

    return Task.CompletedTask;
  }
}