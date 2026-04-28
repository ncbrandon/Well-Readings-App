namespace Well_Readings.Models.ViewModels
{
    public class MonthlyReportViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }

        public List<MonthlyRowViewModel> Rows { get; set; } = new();

        public List<string> AvailableSites { get; set; } = new();

        public Dictionary<string, decimal> TotalGallonsPerSite { get; set; } = new();

        public Dictionary<string, decimal> MaxDailyGallonsPerSite { get; set; } = new();
    }

    public class MonthlyRowViewModel
    {
        public DateOnly Date { get; set; }
        public string Site { get; set; } = string.Empty;
        public string WellName { get; set; } = string.Empty;
        public decimal TotalGallons { get; set; }
    }
}
