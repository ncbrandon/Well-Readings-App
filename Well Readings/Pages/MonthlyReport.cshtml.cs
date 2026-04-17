using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Well_Readings.DTOs;

namespace Well_Readings.Pages
{
    public class MonthlyReportModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MonthlyReportModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty(SupportsGet = true)]
        public int Year { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Month { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Site { get; set; }

        public List<MonthlyWellReportRowDto> Rows { get; set; } = new();

        public List<string> AvailableSites { get; } = new()
        {
            "", // All Sites
            "Reeves Well",
            "Park Well",
            "Woods Well",
            "Catawissa Well",
            "New Well",
            "Oakwood Well",
            "Ray Well"
        };

        public Dictionary<string, decimal> TotalGallonsPerSite { get; set; } = new();
        public Dictionary<string, decimal> MaxDailyGallonsPerSite { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Default to current month
            if (Year == 0 || Month == 0)
            {
                var today = DateTime.Today;
                Year = today.Year;
                Month = today.Month;
            }

            using var client = _httpClientFactory.CreateClient();

            var url = $"https://localhost:7090/api/daily-entries/reports/monthly?year={Year}&month={Month}";

            if (!string.IsNullOrEmpty(Site))
            {
                url += $"&site={Uri.EscapeDataString(Site)}";
            }

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return;

            var json = await response.Content.ReadAsStringAsync();

            Rows = JsonSerializer.Deserialize<List<MonthlyWellReportRowDto>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new();

            ComputeAggregates();
        }

        public string GetSiteFromWell(string wellName)
        {
            if (string.IsNullOrWhiteSpace(wellName))
                return wellName ?? "";

            var name = wellName.ToLowerInvariant();
            if (name.StartsWith("reeves well"))
                return "Reeves Well";
            if (name.StartsWith("park well"))
                return "Park Well";
            if (name.StartsWith("reeves"))
                return "Reeves Well";
            if (name.StartsWith("park"))
                return "Park Well";

            var parts = wellName.Trim().Split(' ');
            if (parts.Length > 1 && parts[^1].Length == 1)
                return string.Join(' ', parts.Take(parts.Length - 1));
            return wellName;
        }

        private void ComputeAggregates()
        {
            var perReadingGallons = new List<(DateOnly Date, string Well, string Site, decimal Gallons)>();

            var groupedByWell = Rows
                .GroupBy(r => r.WellName)
                .ToDictionary(g => g.Key, g => g.OrderBy(r => r.Date).ThenBy(r => r.EntryTime).ToList());

            foreach (var kvp in groupedByWell)
            {
                var well = kvp.Key;
                var ordered = kvp.Value;
                decimal? previousMeter = null;

                foreach (var r in ordered)
                {
                    decimal gallons = 0;
                    if (previousMeter.HasValue)
                    {
                        gallons = r.MeterReading - previousMeter.Value;
                        if (gallons < 0) gallons = 0;
                    }
                    previousMeter = r.MeterReading;
                    perReadingGallons.Add((r.Date, well, GetSiteFromWell(well), gallons));
                }
            }

            // Aggregate per-site per-day
            var dailySiteTotals = perReadingGallons
                .GroupBy(x => x.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(x => x.Site)
                          .ToDictionary(sg => sg.Key, sg => sg.Sum(x => x.Gallons))
                );

            // Totals and maxima per site
            var siteTotals = new Dictionary<string, decimal>();
            var siteMaxDaily = new Dictionary<string, decimal>();

            foreach (var dateKvp in dailySiteTotals)
            {
                foreach (var siteKvp in dateKvp.Value)
                {
                    var site = siteKvp.Key;
                    var daily = siteKvp.Value;

                    if (!siteTotals.ContainsKey(site))
                        siteTotals[site] = 0;
                    siteTotals[site] += daily;

                    if (!siteMaxDaily.ContainsKey(site) || daily > siteMaxDaily[site])
                        siteMaxDaily[site] = daily;
                }
            }

            TotalGallonsPerSite = siteTotals;
            MaxDailyGallonsPerSite = siteMaxDaily;
        }
    }
}
