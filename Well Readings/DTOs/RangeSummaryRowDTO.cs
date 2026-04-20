namespace Well_Readings.DTOs
{
    public class RangeSummaryRowDto
    {
        public string Site { get; set; } = "";

        public string WellName { get; set; } = string.Empty;

        public decimal TotalGallons { get; set; }
        public decimal MaxDailyGallons { get; set; }

        public List<DailyTotalDto> DailyTotals { get; set; } = new();


    }
}
