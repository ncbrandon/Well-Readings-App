namespace Well_Readings.DTOs
{
    public class FiltrationPlantReadingDto
    {
        public decimal FilterPlantMeterReading { get; set; }
        public decimal MtJeffersonMeterReading { get; set; }

        public decimal? Chlorine { get; set; }
        public decimal? Phosphate { get; set; }
        public decimal? Ph { get; set; }
        public decimal? Temperature { get; set; }
    }
}