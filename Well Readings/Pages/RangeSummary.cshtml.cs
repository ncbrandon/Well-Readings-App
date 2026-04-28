using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using Well_Readings.DTOs.API;
using Well_Readings.Services;

namespace Well_Readings.Pages
{
    public class RangeSummaryModel : PageModel
    {
        private readonly IReportsClient _reportsClient;

        public RangeSummaryModel(IReportsClient reportsClient)
        {
            _reportsClient = reportsClient;
        }

        [BindProperty(SupportsGet = true)]
        public DateOnly Start { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateOnly End { get; set; }

        public List<RangeSummaryRowDto> Rows { get; set; } = new();

        public decimal TotalGallonsAllSites { get; set; }

        public async Task OnGetAsync()
        {
            var rawRows = await _reportsClient.GetRange(Start, End);

            Rows = rawRows;

            TotalGallonsAllSites = Rows.Sum(r => r.TotalProduction);
        }
    }
}
