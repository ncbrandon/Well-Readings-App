namespace Well_Readings.DTOs.API
{
    public class RangeSummaryRowDto
    {
        public string WellName { get; set; } = string.Empty;
        public decimal TotalProduction { get; set; }
        public decimal MinReading { get; set; }
        public decimal MaxReading { get; set; }
    }
}
