using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using Well_Readings.DTOs;

namespace Well_Readings.Pages
{
    public class DailyEntryModel : PageModel
    {
        // This is the object bound to the form
        [BindProperty]
        public DailyEntryRequestDto Entry { get; set; } = new();

        // Master list of wells (single source of truth)
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

        // Constructor: initialize one WellReadingDto per well
        public DailyEntryModel()
        {
            foreach (var well in Wells)
            {
                Entry.WellReadings.Add(new WellReadingDto
                {
                    WellName = well
                });
            }
        }

        // Handles POST from the form
        public async Task<IActionResult> OnPostAsync()
        {
            using var client = new HttpClient();

            var json = JsonSerializer.Serialize(Entry);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "https://localhost:7090/api/daily-entries",
                content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to save daily entry. Please check the values and try again."
                );
                return Page();
            }

            // Reload the page clean after success
            return RedirectToPage("DailyEntry");
        }
    }
}