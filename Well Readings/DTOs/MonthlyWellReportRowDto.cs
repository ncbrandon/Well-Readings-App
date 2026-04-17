namespace Well_Readings.DTOs
{
    public class MonthlyWellReportRowDto
    {
        public string Site { get; set; } = "";
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }
        public decimal GallonsPumped { get; set; }
        public decimal? Chlorine { get; set; }
        public decimal? Phosphate { get; set; }
        public decimal? Ph { get; set; }
    }
}