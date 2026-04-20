using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Well_Readings.Pages
{
    public class DailyLogModel : PageModel
    {
        public List<DailyLogRow> Entries { get; set; } = new();

        public async Task OnGetAsync()
        {
            using var client = new HttpClient();

            var url = "https://localhost:7090/api/daily-entries";

            var response = await client.GetAsync(url);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API failed: {response.StatusCode} - {json}");
            }

            Entries = JsonSerializer.Deserialize<List<DailyLogRow>>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new();
        }

        public class DailyLogRow
        {
            public Guid Id { get; set; }
            public DateOnly EntryDate { get; set; }
            public TimeOnly EntryTime { get; set; }
            public int WellCount { get; set; }
        }
    }
}
