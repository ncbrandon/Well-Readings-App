namespace Well_Readings.Models.ViewModels
{
    public class RangeSummaryViewModel
    {
        public DateOnly Start { get; set; }
        public DateOnly End { get; set; }

        public List<RangeSummaryRowViewModel> Rows { get; set; } = new();

        public decimal TotalGallonsAllSites { get; set; }
    }

    public class RangeSummaryRowViewModel
    {
        public string WellName { get; set; } = string.Empty;
        public decimal TotalProduction { get; set; }
        public decimal MinReading { get; set; }
        public decimal MaxReading { get; set; }
    }
}
