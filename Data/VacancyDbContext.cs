using Microsoft.EntityFrameworkCore;
using VacancyApi.Models;

namespace VacancyApi.Data;

public class VacancyDbContext : DbContext
{
    public DbSet<Vacancy> Vacancies => Set<Vacancy>();

    public DbSet<User> Users => Set<User>();
    public DbSet<SearchPreferences> SearchPreferences => Set<SearchPreferences>();

    public DbSet<UserVacancy> UserVacancies => Set<UserVacancy>();

    public VacancyDbContext(DbContextOptions<VacancyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vacancy>()
            .HasIndex(v => new { v.Source, v.ExternalId })
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.SearchPreferences)
            .WithOne(preferences => preferences.User)
            .HasForeignKey<SearchPreferences>(u => u.UserId);

        modelBuilder.Entity<User>()
          .HasIndex(u => u.TelegramChatId)
          .IsUnique();

        modelBuilder.Entity<UserVacancy>()
            .HasKey(uv => new { uv.UserId, uv.VacancyId });

        modelBuilder.Entity<UserVacancy>()
            .HasOne(uv => uv.User)
            .WithMany()
            .HasForeignKey(uv => uv.UserId);

        modelBuilder.Entity<UserVacancy>()
           .HasOne(uv => uv.Vacancy)
           .WithMany()
           .HasForeignKey(uv => uv.VacancyId);
    }
}