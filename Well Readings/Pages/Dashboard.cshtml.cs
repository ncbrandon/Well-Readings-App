using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Well_Readings.Pages
{
    public class DashboardModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public DateOnly Start { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateOnly End { get; set; }

        public List<DashboardWell> Wells { get; set; } = new();

        public async Task OnGetAsync()
        {
            if (Start == default)
                Start = DateOnly.FromDateTime(DateTime.Today.AddDays(-7));

            if (End == default)
                End = DateOnly.FromDateTime(DateTime.Today);

            using var client = new HttpClient();

            var url = $"https://localhost:7090/api/daily-entries/reports/dashboard?start={Start}&end={End}";

            var res = await client.GetAsync(url);

            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                throw new Exception(json);

            Wells = JsonSerializer.Deserialize<List<DashboardWell>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }

        public class DashboardWell
        {
            public string Well { get; set; } = "";
            public decimal Total { get; set; }
            public decimal Max { get; set; }
            public List<Daily> Data { get; set; } = new();
        }

        public class Daily
        {
            public DateOnly Date { get; set; }
            public decimal Gallons { get; set; }
            public bool IsAnomaly { get; set; }
        }
    }
}
