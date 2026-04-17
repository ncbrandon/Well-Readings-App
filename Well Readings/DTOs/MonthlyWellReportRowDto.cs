namespace Well_Readings.DTOs
{
    public class MonthlyWellReportRowDto
    {
        public DateOnly Date { get; set; }

        public TimeOnly EntryTime { get; set; }

        public string WellName { get; set; } = string.Empty;

        public decimal MeterReading { get; set; }

        public decimal? Chlorine { get; set; }

        public decimal? Phosphate { get; set; }

        public decimal? Ph { get; set; }

        public decimal? Temperature { get; set; } // <-- Add this line
    }
}
