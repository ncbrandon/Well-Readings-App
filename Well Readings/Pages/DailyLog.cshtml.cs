using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace Well_Readings.Pages
{
    public class DailyLogModel : PageModel
    {
        public List<DailyLogItem> Entries { get; set; } = new();

        public async Task OnGetAsync()
        {
            using var client = new HttpClient();

            var response = await client.GetAsync("https://localhost:7090/api/daily-entries");

            if (!response.IsSuccessStatusCode)
                return;

            var json = await response.Content.ReadAsStringAsync();

            Entries = JsonSerializer.Deserialize<List<DailyLogItem>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new();
        }

        

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            using var client = new HttpClient();

            var response = await client.DeleteAsync(
                $"https://localhost:7090/api/daily-entries/{id}");

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Delete failed. The entry was not removed from the database."
                );

                // Reload the list so the row reappears
                await OnGetAsync();
                return Page();
            }

            return RedirectToPage();
        }


    }

    public class DailyLogItem
    {
        public Guid Id { get; set; }
        public DateOnly EntryDate { get; set; }
        public TimeOnly EntryTime { get; set; }
        public int WellCount { get; set; }
    }
}