using VacancyApi.Models;
using AngleSharp.Dom;

namespace VacancyApi.Interfaces;

public interface ICharacterService
{
  Task<IDocument> GetPageAsync(string url);

  IEnumerable<Vacancy> GetVacancies(IDocument document);
}