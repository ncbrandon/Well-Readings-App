using Microsoft.AspNetCore.Mvc;

namespace Well_Readings.DTOs
{
    public class WellReadingResponseDto
    {
        public string WellName { get; set; } = string.Empty;
        public decimal MeterReading { get; set; }
        public decimal? Chlorine { get; set; }
        public decimal? Phosphate { get; set; }
        public decimal? Ph { get; set; }
    }

}
