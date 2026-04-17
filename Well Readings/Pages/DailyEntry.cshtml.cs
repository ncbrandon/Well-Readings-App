using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using Well_Readings.DTOs;

namespace Well_Readings.Pages
{
    public class DailyEntryModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public DailyEntryRequestDto Entry { get; set; } = new();

        public List<string> Wells { get; } = new()
        {
            "Reeves Well A",
            "Reeves Well B",
            "Park Well",
            "Park Well A",
            "Park Well B",
            "Woods Well",
            "Catawissa Well",
            "New Well",
            "Oakwood Well",
            "Ray Well"
        };

        // GET
        public async Task OnGetAsync(Guid? id)
        {
            using var client = new HttpClient();
            HttpResponseMessage response;

            if (id.HasValue)
            {
                response = await client.GetAsync(
                    $"https://localhost:7090/api/daily-entries/{id}");
            }
            else
            {
                response = await client.GetAsync(
                    "https://localhost:7090/api/daily-entries/today");

                // ✅ NEW entry → ensure ID is null
                Entry.Id = null;
            }

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                Entry = JsonSerializer.Deserialize<DailyEntryRequestDto>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    })!;
            }
            else
            {
                // ✅ Defaults for new entry
                Entry.EntryDate = DateOnly.FromDateTime(DateTime.Today);
                Entry.EntryTime = TimeOnly.FromDateTime(DateTime.Now);
                Entry.Id = null;
            }

            // ✅ Normalize wells AFTER Entry is loaded
            var normalizedWells = new List<WellReadingDto>();

            foreach (var well in Wells)
            {
                var match = Entry.WellReadings
                    .FirstOrDefault(w => w.WellName == well);

                normalizedWells.Add(match ?? new WellReadingDto
                {
                    WellName = well
                });
            }

            Entry.WellReadings = normalizedWells;
        }

        // POST
        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine($"POSTED ENTRY ID = {Entry.Id}");

            using var client = new HttpClient();

            var json = JsonSerializer.Serialize(Entry);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "https://localhost:7090/api/daily-entries",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    $"Save failed: {error}"
                );

                return Page();
            }

            return RedirectToPage("DailyEntry");
        }
    }
}