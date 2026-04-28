namespace Well_Readings.Pages.ViewModels
{
    public class MonthlyReportRowViewModel
    {
        public string WellName { get; set; } = string.Empty;
        public string Site { get; set; } = string.Empty;

        public decimal TotalProduction { get; set; }
        public decimal AverageProduction { get; set; }
        public int DaysReported { get; set; }

        public bool IsAnomaly => TotalProduction > 10000;
    }
}
