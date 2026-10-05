using Microsoft.EntityFrameworkCore;
using VacancyApi.Data;
using VacancyApi.Interfaces;
using VacancyApi.Models;
using VacancyApi.Services;
using Telegram.Bot;
using VacancyApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<DjinniParser>();
builder.Services.AddSingleton<BrowserService>(
    await BrowserService.CreateAsync());
builder.Services.AddScoped<WorkUaParser>();
builder.Services.AddScoped<RobotaUaParser>();
builder.Services.AddHttpClient<DouParser>();
builder.Services.AddHostedService<VacancyPollingService>();
builder.Services.AddScoped<VacancySearchService>();
builder.Services.AddScoped<UserService>();
builder.Services.Configure<TelegramOptions>(
    builder.Configuration.GetSection("Telegram"));
builder.Services.AddScoped<TelegramService>();
builder.Services.Configure<TelegramOptions>(
    builder.Configuration.GetSection("Telegram"));
builder.Services.AddScoped<VacancyNotificationService>();

var telegramToken =
    builder.Configuration["Telegram:BotToken"]
    ?? throw new InvalidOperationException(
        "Telegram bot token is not configured.");

builder.Services.AddSingleton(
    new TelegramBotClient(telegramToken));
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TelegramService>();

builder.Services.AddHostedService<TelegramPollingService>();

builder.Services.AddDbContext<VacancyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<VacancyService>();
builder.Services.AddScoped<VacancyScoringService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();
app.UseCors("AngularPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
