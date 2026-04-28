namespace Well_Readings.Models
{
    public class ScadaHistoryPoint
    {
        public Guid Id { get; set; }

        public string WellName { get; set; }

        public DateTime Timestamp { get; set; }

        public decimal Value { get; set; }

        public string MetricType { get; set; }
        // "Flow", "Chlorine", "Ph", "Phosphate", "Temperature", etc.
    }
}

