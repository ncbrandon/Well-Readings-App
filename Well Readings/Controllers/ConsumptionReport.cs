namespace Well_Readings.Models
{
    public class ConsumptionReport
    {
        public Guid Id { get; set; }

        public string PeriodLabel { get; set; } = string.Empty;

        public DateTime LastReadDate { get; set; }

        public DateTime CurrentReadDate { get; set; }

        public int BillingDays { get; set; }

        public decimal WaterPumped { get; set; }

        public decimal WaterConsumed { get; set; }

        public decimal WaterLoss { get; set; }

        public decimal LossPercent { get; set; }

        public decimal PumpedAveragePerDay { get; set; }

        public decimal ConsumedAveragePerDay { get; set; }

        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}