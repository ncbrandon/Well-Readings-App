using Microsoft.AspNetCore.Mvc;

namespace Well_Readings.DTOs.API
{
    public class MonthlyWellReportRowDto
    {
        public DateOnly Date { get; set; }

        public string Site { get; set; } = string.Empty;

        public string WellName { get; set; } = string.Empty;

        public decimal TotalGallons { get; set; }
    }
}

