using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace Well_Readings.Pages;

public class DashboardModel : PageModel
{
    private readonly IHttpClientFactory _http;

    public DashboardModel(IHttpClientFactory http)
    {
        _http = http;
    }
    [BindProperty(SupportsGet = true)]
    public DateTime? Timestamp { get; set; }

    // ===== KPI =====
    public decimal TotalToday { get; set; }
    public decimal TotalMonth { get; set; }
    public int ActiveWells { get; set; }

    // ===== Trend =====
    public List<string> Dates { get; set; } = new();
    public List<decimal> DailyTotals { get; set; } = new();

    // ===== SCADA =====
    public List<ScadaWellDto> Wells { get; set; } = new();

    public int AlarmCount => Wells.Count(x => x.IsAlarm);

    public async Task OnGetAsync()
    {
        if (Timestamp == null)
            Timestamp = DateTime.Now;

        var client = _http.CreateClient();

        // 🔹 PARALLEL CALLS (important for performance)
        var dashboardTask = client.GetFromJsonAsync<List<DashboardDto>>(
            "https://localhost:7090/api/dashboard"
        );

        var scadaTask = client.GetFromJsonAsync<List<ScadaWellDto>>(
            "https://localhost:7090/api/dashboard?timestamp={Timestamp:0}"
        );

        await Task.WhenAll(dashboardTask!, scadaTask!);

        var dashboard = dashboardTask!.Result ?? new();
        Wells = scadaTask!.Result ?? new();

        // ===== KPI =====
        TotalToday = dashboard.Sum(x => x.TodayTotal);
        TotalMonth = dashboard.Sum(x => x.MonthTotal);
        ActiveWells = Wells.Count;

        // ===== TREND =====
        Dates = dashboard
            .OrderBy(x => x.Date)
            .Select(x => x.Date.ToString("MM-dd"))
            .ToList();

        DailyTotals = dashboard
            .OrderBy(x => x.Date)
            .Select(x => x.Total)
            .ToList();
    }

    // ===== DTOs =====

    public class DashboardDto
    {
        public string WellName { get; set; } = "";
        public DateOnly Date { get; set; }
        public decimal TodayTotal { get; set; }
        public decimal MonthTotal { get; set; }
        public decimal Total { get; set; }
    }

    public class ScadaWellDto
    {
        public string WellName { get; set; } = "";
        public decimal LastReading { get; set; }
        public decimal TotalGallons { get; set; }
        public bool IsAlarm { get; set; }
    }
}
