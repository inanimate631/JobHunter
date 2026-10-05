public class RobotaSearchResponse
{
    public int Took { get; set; }
    public int Start { get; set; }
    public int Count { get; set; }
    public int Total { get; set; }
    public int HasGeoCount { get; set; }
    public string? ErrorMessage { get; set; }

    public List<RobotaVacancyDto> Documents { get; set; } = [];
}

public class RobotaVacancyDto
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string CompanyName { get; set; } = "";

    public string CityName { get; set; } = "";

    public int CityId { get; set; }

    public DateTime Date { get; set; }

    public int Salary { get; set; }

    public int SalaryFrom { get; set; }

    public int SalaryTo { get; set; }

    public string SalaryComment { get; set; } = "";

    public string ShortDescription { get; set; } = "";

    public int NotebookId { get; set; }
}