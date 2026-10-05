using Microsoft.Playwright;

namespace VacancyApi.Services;

public class BrowserService : IAsyncDisposable
{
  private readonly IPlaywright _playwright;

  public IBrowser Browser { get; }

  private BrowserService(
      IPlaywright playwright,
      IBrowser browser)
  {
    _playwright = playwright;
    Browser = browser;
  }

  public static async Task<BrowserService> CreateAsync()
  {
    var playwright = await Playwright.CreateAsync();

    var browser = await playwright.Chromium.LaunchAsync(
        new BrowserTypeLaunchOptions
        {
          Headless = true
        });

    return new BrowserService(playwright, browser);
  }

  public async Task<string> GetPageHtmlAsync(string url)
  {
    var page = await Browser.NewPageAsync();

    try
    {
      await page.GotoAsync(
          url,
          new PageGotoOptions
          {
            WaitUntil = WaitUntilState.DOMContentLoaded
          });

      return await page.ContentAsync();
    }
    finally
    {
      await page.CloseAsync();
    }
  }

  public async ValueTask DisposeAsync()
  {
    await Browser.CloseAsync();
    _playwright.Dispose();
  }
}