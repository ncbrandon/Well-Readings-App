using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Well_Readings.DTOs;

namespace Well_Readings.Pages
{
    public class RangeSummaryModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public DateOnly Start { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateOnly End { get; set; }

        public List<RangeSummaryRowDto> Rows { get; set; } = new();

        public decimal TotalGallonsAllSites { get; set; }

        public async Task OnGetAsync()
        {
            if (Start == default)
                Start = DateOnly.FromDateTime(DateTime.Today.AddDays(-7));

            if (End == default)
                End = DateOnly.FromDateTime(DateTime.Today);

            using var client = new HttpClient();

            var response = await client.GetAsync(
                $"https://localhost:7090/api/daily-entries/reports/range-summary" +
                $"?start={Start:yyyy-MM-dd}&end={End:yyyy-MM-dd}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {response.StatusCode} - {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            Rows = JsonSerializer.Deserialize<List<RangeSummaryRowDto>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new();

            TotalGallonsAllSites = Rows.Sum(r => r.TotalGallons);
        }
    }
}
