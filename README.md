# JobHunter

JobHunter is a backend for a Telegram bot that helps users find relevant job vacancies. The bot collects vacancies from four popular Ukrainian job boards, scores them according to the user's preferences, and sends new matching vacancies as Telegram messages.

The sources are checked automatically approximately once an hour. Users can also start a check manually from the bot menu.

## Features

- searches for vacancies on Djinni, Work.ua, Robota.ua, and DOU;
- configures keywords, positions, work type, English level, and experience through Telegram;
- scores vacancies according to the user's preferences;
- sends new vacancies without duplicate notifications;
- sends up to five top vacancies after the initial setup;
- provides an HTTP API for retrieving found vacancies;
- stores vacancies, users, and notification history in PostgreSQL.

## Tech stack

- .NET 10 / ASP.NET Core Web API;
- Entity Framework Core 10;
- PostgreSQL;
- Telegram.Bot;
- AngleSharp;
- Microsoft Playwright for loading Work.ua pages.

## Requirements

- .NET SDK 10;
- PostgreSQL;
- a Telegram bot and its token from [@BotFather](https://t.me/BotFather);
- Chromium for Playwright.

## Configuration

The project uses .NET User Secrets, so Telegram tokens and database passwords are not stored in Git.

Run the following commands from the project root:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=JobHunter;Username=postgres;Password=YOUR_PASSWORD"
dotnet user-secrets set "Telegram:BotToken" "YOUR_TELEGRAM_BOT_TOKEN"
```

Make sure PostgreSQL is running. You may create the `JobHunter` database in advance or change the connection string as needed.

The Entity Framework Core CLI is required to apply migrations. Install it if it is not already available:

```bash
dotnet tool install --global dotnet-ef
```

Install the Playwright browser:

```bash
dotnet build
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
```

## Running the application

```bash
dotnet restore
dotnet ef database update
dotnet run
```

In the Development environment, OpenAPI is available at `http://localhost:5216/openapi/v1.json` after the application starts.

Find your bot in Telegram and send `/start`. The bot will guide you through configuring your search preferences and will then start sending new matching vacancies.

## API

Retrieve vacancies using the following request:

```http
POST /api/Vacancies
Content-Type: application/json

{
  "keywords": [".NET", "C#"],
  "positions": ["Backend Developer"],
  "workType": "Remote",
  "englishLevel": "B2",
  "experienceYears": 2
}
```

## Project structure

- `Controllers/` — HTTP controllers;
- `Services/` — Telegram bot, search, parsers, scoring, and background services;
- `Models/` — domain models;
- `Data/` — Entity Framework Core context;
- `Migrations/` — database migrations;
- `BrowserService/` — Playwright integration.

## Notes

Job boards may change their HTML markup, APIs, or access rules. The corresponding parser may need to be updated when a source changes.
