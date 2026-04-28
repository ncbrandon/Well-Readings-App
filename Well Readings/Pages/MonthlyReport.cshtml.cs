using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Well_Readings.DTOs.API;
using Well_Readings.Pages.ViewModels;
using Well_Readings.Services;

namespace Well_Readings.Pages
{
    public class MonthlyReportModel : PageModel
    {
        private readonly IReportsClient _reportsClient;

        public MonthlyReportModel(IReportsClient reportsClient)
        {
            _reportsClient = reportsClient;
        }

        [BindProperty(SupportsGet = true)]
        public int Year { get; set; } = DateTime.Today.Year;

        [BindProperty(SupportsGet = true)]
        public int Month { get; set; } = DateTime.Today.Month;

        public List<MonthlyReportRowViewModel> Rows { get; set; } = new();

        public List<string> AvailableSites { get; set; } = new();

        public Dictionary<string, decimal> TotalGallonsPerSite { get; set; } = new();

        public Dictionary<string, decimal> MaxDailyGallonsPerSite { get; set; } = new();

        public async Task OnGetAsync()
        {
            // FIX: single source of data (NO HttpClientFactory, NO duplicate rawRows)
            var rawRows = await _reportsClient.GetMonthly(Year, Month)
                           ?? new List<MonthlyWellReportRowDto>();

            // safe fallback
            int totalDaysInMonth = DateTime.DaysInMonth(Year, Month);

            Rows = rawRows
                .GroupBy(r => new { r.WellName, r.Site })
                .Select(g => new MonthlyReportRowViewModel
                {
                    WellName = g.Key.WellName,
                    Site = g.Key.Site,

                    DaysReported = g.Count(),

                    TotalProduction = g.Sum(x => x.TotalGallons),

                    // FIX: correct average logic (avoid divide-by-zero)
                    AverageProduction = g.Count() == 0
                        ? 0
                        : g.Sum(x => x.TotalGallons) / g.Count()
                })
                .ToList();

            AvailableSites = Rows
                .Select(r => r.Site)
                .Distinct()
                .ToList();

            TotalGallonsPerSite = Rows
                .GroupBy(r => r.Site)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.TotalProduction)
                );

            MaxDailyGallonsPerSite = rawRows
                .GroupBy(r => r.Site)
                .ToDictionary(
                    g => g.Key,
                    g => g.Max(x => x.TotalGallons)
                );
        }
    }
}
