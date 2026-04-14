
namespace Well_Readings.DTOs
{
    public class WellReadingDto
    {
        public string WellName { get; set; }
        public decimal MeterReading { get; set; }
        public decimal? Chlorine { get; set; }
        public decimal? Phosphate { get; set; }
        public decimal? Ph { get; set; }
    }
}
