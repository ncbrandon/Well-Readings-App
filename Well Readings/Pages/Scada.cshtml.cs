using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Well_Readings.Data;

namespace Well_Readings.Pages
{
    public class ScadaModel : PageModel
    {
        private readonly AppDbContext _context;

        public ScadaModel(AppDbContext context)
        {
            _context = context;
        }

        private static readonly string[] MeterColumns =
        {
            "Reeves Well",
            "Reeves Well A",
            "Park Well",
            "Park Well A",
            "Park Well B",
            "Woods",
            "Catawissa",
            "New",
            "Oakwood",
            "Ray",
            "Filter Plant"
        };

        public void OnGet()
        {
        }

        public async Task<JsonResult> OnGetPumpedHistoryAsync(string mode)
        {
            if (string.Equals(mode, "yearsToday", StringComparison.OrdinalIgnoreCase))
            {
                var yearsToday = await GetPreviousYearsTodayPumpedAsync();
                return new JsonResult(yearsToday);
            }

            var previous30Days = await GetPrevious30DaysPumpedAsync();
            return new JsonResult(previous30Days);
        }

        private async Task<List<PumpedHistoryRowDto>> GetPrevious30DaysPumpedAsync()
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(-30);
            var endDate = today.AddDays(-1);

            var results = new List<PumpedHistoryRowDto>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var gallons = await GetTotalGallonsPumpedForDateAsync(date);

                results.Add(new PumpedHistoryRowDto
                {
                    Date = date,
                    DayName = date.ToString("dddd", CultureInfo.InvariantCulture),
                    DisplayDate = date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                    GallonsPumped = gallons
                });
            }

            return results
                .OrderBy(x => x.Date)
                .ToList();
        }

        private async Task<List<PumpedHistoryRowDto>> GetPreviousYearsTodayPumpedAsync()
        {
            var today = DateTime.Today;
            var month = today.Month;
            var day = today.Day;
            var currentYear = today.Year;

            var earliestScadaDate = await _context.ScadaHistoryPoints
                .OrderBy(x => x.Timestamp)
                .Select(x => (DateTime?)x.Timestamp)
                .FirstOrDefaultAsync();

            if (earliestScadaDate == null)
            {
                return new List<PumpedHistoryRowDto>();
            }

            var firstYear = earliestScadaDate.Value.Year;
            var results = new List<PumpedHistoryRowDto>();

            for (var year = firstYear; year < currentYear; year++)
            {
                DateTime date;

                try
                {
                    date = new DateTime(year, month, day);
                }
                catch
                {
                    continue;
                }

                var gallons = await GetTotalGallonsPumpedForDateAsync(date);

                if (gallons <= 0)
                {
                    continue;
                }

                results.Add(new PumpedHistoryRowDto
                {
                    Date = date,
                    DayName = date.ToString("dddd", CultureInfo.InvariantCulture),
                    DisplayDate = date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                    GallonsPumped = gallons
                });
            }

            return results
                .OrderBy(x => x.Date)
                .ToList();
        }

        private async Task<decimal> GetTotalGallonsPumpedForDateAsync(DateTime date)
        {
            decimal total = 0;

            foreach (var meter in MeterColumns)
            {
                total += await GetDeltaForDateAsync(meter, "Meter Reading", date);
            }

            return total;
        }

        private async Task<decimal> GetDeltaForDateAsync(string location, string metricType, DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            var current = await _context.ScadaHistoryPoints
                .Where(x =>
                    x.Location == location &&
                    x.MetricType == metricType &&
                    x.Timestamp >= start &&
                    x.Timestamp < end)
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefaultAsync();

            var previous = await _context.ScadaHistoryPoints
                .Where(x =>
                    x.Location == location &&
                    x.MetricType == metricType &&
                    x.Timestamp < start)
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefaultAsync();

            if (current?.Value == null || previous?.Value == null)
            {
                return 0;
            }

            var delta = current.Value.Value - previous.Value.Value;

            return delta < 0 ? 0 : delta;
        }

        public class PumpedHistoryRowDto
        {
            public string DayName { get; set; } = string.Empty;

            public string DisplayDate { get; set; } = string.Empty;

            public DateTime Date { get; set; }

            public decimal GallonsPumped { get; set; }
        }
    }
}