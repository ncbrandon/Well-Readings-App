namespace Well_Readings.DTOs
{
    public class DailyTotalDto
    {
        public DateOnly Date { get; set; }
        public decimal Gallons { get; set; }
        public decimal CumulativeGallons { get; set; }
    }
}

