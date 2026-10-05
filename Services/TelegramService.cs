using Microsoft.Extensions.Options;
using Telegram.Bot;
using VacancyApi.Models;

namespace VacancyApi.Services;

public class TelegramService
{
  private readonly TelegramBotClient _botClient;

  public TelegramService(IOptions<TelegramOptions> options)
  {
    _botClient = new TelegramBotClient(options.Value.BotToken);
  }

  public async Task SendMessageAsync(
      long chatId,
      string message,
      CancellationToken cancellationToken = default)
  {
    await _botClient.SendMessage(
        chatId,
        message,
        cancellationToken: cancellationToken);
  }
}