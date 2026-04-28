using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace Well_Readings.Pages
{
    public class DailyEntryModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DailyEntryModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public DailyEntryDto Entry { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var client = _httpClientFactory.CreateClient();

            var result = await client.GetFromJsonAsync<DailyEntryDto>(
                $"https://localhost:7090/api/daily-entries/{id}"
            );

            if (result == null)
                return NotFound();

            Entry = result;
            return Page();
        }
    }

    public class DailyEntryDto
    {
        public Guid Id { get; set; }

        public DateOnly EntryDate { get; set; }

        public TimeOnly EntryTime { get; set; }

        public List<WellReadingDto> Wells { get; set; } = new();

        public decimal TotalGallons => Wells.Sum(x => x.Gallons);

        public decimal MaxGallons => Wells.Count == 0 ? 0 : Wells.Max(x => x.Gallons);
    }

    public class WellReadingDto
    {
        public string WellName { get; set; } = "";
        public decimal Gallons { get; set; }
    }

}
