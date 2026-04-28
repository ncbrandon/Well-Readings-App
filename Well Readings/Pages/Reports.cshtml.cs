using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;
using System.Data;
using System.Text.RegularExpressions;

namespace Well_Readings.Pages
{
    public class ReportsModel : PageModel
    {
        public List<WellSummaryRow> WellSummary { get; set; } = new();

        public void OnGet()
        {
        }
    }

    public class WellSummaryRow
    {
        public string WellName { get; set; } = "";
        public decimal TotalProduction { get; set; }
        public int DaysPumped { get; set; }
    }

 }