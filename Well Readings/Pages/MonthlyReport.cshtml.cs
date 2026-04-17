using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Well_Readings.DTOs;

namespace Well_Readings.Pages
{
    public class MonthlyReportModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int Year { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Month { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Site { get; set; }


        public List<MonthlyWellReportRowDto> Rows { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Default to current month
            if (Year == 0 || Month == 0)
            {
                var today = DateTime.Today;
                Year = today.Year;
                Month = today.Month;
            }

            using var client = new HttpClient();


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
        }
    }
}
