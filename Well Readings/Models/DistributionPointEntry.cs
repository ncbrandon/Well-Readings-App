namespace Well_Readings.Models
{
    public class DistributionPointEntry
    {
        public Guid Id { get; set; }
        public DateTime EntryDate { get; set; }

        public int Code { get; set; }
        public string Location { get; set; } = string.Empty;

        public decimal? Chlorine { get; set; }
        public decimal? Phosphate { get; set; }
        public decimal? Ph { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}