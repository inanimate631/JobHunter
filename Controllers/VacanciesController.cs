using Microsoft.AspNetCore.Mvc;
using VacancyApi.Models;
using VacancyApi.Services;

namespace VacancyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VacanciesController : ControllerBase
{
  private readonly VacancySearchService _vacancySearchService;

  public VacanciesController(
    VacancySearchService vacancySearchService
  )
  {
    _vacancySearchService = vacancySearchService;
  }

  [HttpPost]
  public async Task<IActionResult> GetVacanciesList(SearchPreferences searchPreferences)
  {
    var scoredVacancies = await _vacancySearchService.GetVacanciesList(searchPreferences);

    return Ok(scoredVacancies);
  }
}