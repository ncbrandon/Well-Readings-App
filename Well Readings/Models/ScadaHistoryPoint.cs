namespace Well_Readings.Models
{
    public class ScadaHistoryPoint
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }

        public string Location { get; set; } = string.Empty;
        public string MetricType { get; set; } = string.Empty;
        public string SourceColumn { get; set; } = string.Empty;

        public decimal? Value { get; set; }
    }
}