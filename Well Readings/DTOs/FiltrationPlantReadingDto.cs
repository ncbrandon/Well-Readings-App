namespace Well_Readings.DTOs
{
    public class FiltrationPlantReadingDto
    {
        public string? UnitSelection { get; set; }

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
