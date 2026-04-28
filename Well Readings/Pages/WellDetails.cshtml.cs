using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace Well_Readings.Pages;

public class WellDetailsModel : PageModel
{
    private readonly IHttpClientFactory _http;

    public WellDetailsModel(IHttpClientFactory http)
    {
        _http = http;
    }

    [BindProperty(SupportsGet = true)]
    public string Name { get; set; } = "";

    public List<ReadingDto> Readings { get; set; } = new();

    public async Task OnGetAsync()
    {
        var client = _http.CreateClient();

        Readings = await client.GetFromJsonAsync<List<ReadingDto>>(
            $"https://localhost:7090/api/scada/well?name={Name}"
        ) ?? new();
    }

    public class ReadingDto
    {
        public DateOnly Date { get; set; }
        public decimal Gallons { get; set; }
    }
}
