using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace Well_Readings.Pages
{
    public class ScadaModel : PageModel
    {
        private readonly IHttpClientFactory _http;

        public ScadaModel(IHttpClientFactory http)
        {
            _http = http;
        }

        public async Task OnGetAsync()
        {
            var client = _http.CreateClient();

            var data = await client.GetFromJsonAsync<ScadaResponse>(
                "https://localhost:7090/api/scada/dashboard"
            );

            Wells = data?.Wells ?? new();
            Plant = data?.Plant;
        }

        public List<ScadaWellDto> Wells { get; set; } = new();
        public ScadaPlantDto? Plant { get; set; }

        // ✅ MATCHES API RESPONSE
        public class ScadaResponse
        {
            public List<ScadaWellDto> Wells { get; set; } = new();
            public ScadaPlantDto? Plant { get; set; }
        }

        public class ScadaWellDto
        {
            public string WellName { get; set; } = "";
            public decimal LastReading { get; set; }
            public decimal TotalGallons { get; set; }
            public bool IsAlarm { get; set; }
        }

        public class ScadaPlantDto
        {
            public decimal FlowRate { get; set; }
            public decimal Turbidity { get; set; }
            public decimal Chlorine { get; set; }
            public decimal Ph { get; set; }
            public decimal Temperature { get; set; }
        }
    }
}
