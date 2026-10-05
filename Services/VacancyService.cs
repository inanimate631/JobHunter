using Microsoft.EntityFrameworkCore;
using VacancyApi.Data;
using VacancyApi.Models;
using VacancyApi.Interfaces;

namespace VacancyApi.Services;

public class VacancyService
{

  private readonly VacancyDbContext _context;

  public VacancyService(VacancyDbContext context)
  {
    _context = context;
  }

  public async Task<Vacancy> AddVacancy(Vacancy vacancy)
  {
    await _context.Vacancies.AddAsync(vacancy);
    await _context.SaveChangesAsync();

    return vacancy;
  }

  public async Task<Vacancy?> GetByExternalIdAsync(string source, int externalId)
  {
    return await _context.Vacancies
        .FirstOrDefaultAsync(vacancy =>
            vacancy.Source == source &&
            vacancy.ExternalId == externalId);
  }
}