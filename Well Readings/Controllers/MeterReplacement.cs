namespace Well_Readings.Models
{
    public class MeterReplacement
    {
        public Guid Id { get; set; }

        public string Location { get; set; } = string.Empty;

        public DateTime ReplacementDate { get; set; }

        public decimal OldMeterFinalReading { get; set; }

        public decimal NewMeterStartingReading { get; set; }

        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}