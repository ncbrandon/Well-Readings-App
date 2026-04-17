namespace Well_Readings.DTOs
{
    public class DailyEntryRequestDto
    {
        public DateOnly EntryDate { get; set; }
        public TimeOnly EntryTime { get; set; }
        public Guid? Id { get; set; }

        public List<WellReadingDto> WellReadings { get; set; } = new();
        public FiltrationPlantReadingDto? FiltrationPlant { get; set; }
    }

    public class FiltrationPlantEntryDto
    {
        public string? UnitSelection { get; set; } // "Unit1", "Unit2", "Both"
        public List<FiltrationUnitEntryDto> Units { get; set; } = new();
    }

    public class FiltrationUnitEntryDto
    {
        public decimal? FeedPressure { get; set; }
        public decimal? FeedFlow { get; set; }
        public decimal? FiltratePressure { get; set; }
        public decimal? FiltrateFlow { get; set; }
        public decimal? TMP { get; set; }
        public decimal? TotalFilterRunTime { get; set; }
        public decimal? TotalFiltrationFlowYesterday { get; set; }
        public decimal? PressureDecay { get; set; }
    }
}
