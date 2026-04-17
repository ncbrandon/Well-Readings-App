using System.Collections.Generic;

namespace Well_Readings.DTOs
{
    public class FiltrationPlantReadingDto
    {
        public string? UnitSelection { get; set; } // "Unit1", "Unit2", "Both"
        public List<FiltrationUnitReadingDto> Units { get; set; } = new();

        public decimal FilterPlantMeterReading { get; set; }
        public decimal MtJeffersonMeterReading { get; set; }

        public decimal? Chlorine { get; set; }
        public decimal? Phosphate { get; set; }
        public decimal? Ph { get; set; }
        public decimal? Temperature { get; set; }
    }

    public class FiltrationUnitReadingDto
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