namespace Well_Readings.Models
{
    public class DistributionPointEntry
    {
        public Guid Id { get; set; }
        public DateTime EntryDate { get; set; }

        public string Code { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        public decimal? Chlorine { get; set; }
        public decimal? Phosphate { get; set; }
        public decimal? Ph { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}