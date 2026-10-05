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

- Docker Engine 24+ and Docker Compose v2 (for server deployment);
- a Telegram bot and its token from [@BotFather](https://t.me/BotFather);
- a server with at least 2 GB RAM and enough disk space for PostgreSQL data.

The Docker image already contains Chromium for Playwright. The application
listens on port `8080` inside the container, and Compose starts PostgreSQL with
a persistent volume. Checked-in EF Core migrations are applied automatically
when the application starts.

## Deployment with Docker Compose

Copy the repository to the server, then run these commands from the project
directory:

```bash
cp .env.example .env
nano .env
docker compose up -d --build
docker compose ps
docker compose logs -f app
```

Set a long random value for `POSTGRES_PASSWORD` and paste the Telegram token
into `TELEGRAM_BOT_TOKEN`. Do not commit `.env` to Git. The application port
is bound to `127.0.0.1`, so it is not available directly from the internet.
The Telegram bot does not need an incoming port. The optional HTTP API can be
reached locally on the server at `http://127.0.0.1:8080`.

For a public production API, put Nginx or Caddy in front of the container,
terminate HTTPS there, and expose only ports 80/443 publicly. Keep PostgreSQL
unpublished; it is reachable by the application only through the internal
Compose network. Telegram polling does not require opening an inbound Telegram
webhook port.

Useful maintenance commands:

```bash
docker compose logs -f app
docker compose restart app
docker compose pull db && docker compose up -d
docker compose down                 # stops containers, keeps database volume
docker compose down -v              # also deletes the database volume
```

Back up PostgreSQL before server migration or volume removal:

```bash
docker compose exec -T db sh -c 'pg_dump -U "$POSTGRES_USER" "$POSTGRES_DB"' > jobhunter.sql
```

The command that removes stored database data is `docker compose down -v`; use
it only when you intentionally want to delete the PostgreSQL volume.

## Automatic deployment from GitHub

The repository contains `.github/workflows/deploy.yml`. It deploys every push
to the `main` branch and can also be started manually from the GitHub Actions
page. The workflow logs into the server over SSH, pulls the commit, rebuilds
the Docker image, and restarts the application. The existing Nginx
configuration is not changed.

Complete the one-time server setup first:

```bash
sudo apt update
sudo apt install -y git
sudo mkdir -p /opt
sudo chown "$USER:$USER" /opt
cd /opt
git clone git@github.com:OWNER/REPOSITORY.git JobHunter
cd JobHunter
cp .env.example .env
chmod 600 .env
nano .env
docker compose up -d --build
```

The server user must be able to run Docker without `sudo`:

```bash
sudo usermod -aG docker "$USER"
```

Log out and back in after running that command. Then create a separate SSH key
on your computer for GitHub Actions:

```bash
ssh-keygen -t ed25519 -C "github-actions-jobhunter" -f jobhunter_deploy
```

Append `jobhunter_deploy.pub` to the server user's
`~/.ssh/authorized_keys`. Add these repository secrets in GitHub under
`Settings → Secrets and variables → Actions`:

```text
SERVER_HOST          server IP or hostname
SERVER_USER          Linux user used for deployment
SERVER_PATH          /opt/JobHunter
SSH_PRIVATE_KEY      contents of jobhunter_deploy
SERVER_KNOWN_HOSTS   output of ssh-keyscan -H SERVER_HOST
```

If the repository is private, the server also needs a read-only GitHub Deploy
Key so that `git pull` can access the repository. Do not put `.env`, Telegram
tokens, database passwords, or private SSH keys into Git.

After this, a normal deployment is simply:

```bash
git add .
git commit -m "Update bot"
git push origin main
```

GitHub Actions will perform the server update automatically. The workflow uses
the `main` branch; change both `main` references in the workflow if the main
branch in the repository is named differently.

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
