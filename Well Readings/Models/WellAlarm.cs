namespace Well_Readings.Models
{
    public class WellAlarm
    {
        public Guid Id { get; set; }
        public Guid WellId { get; set; }

        public decimal HighLimit { get; set; } = 10000;
        public decimal LowLimit { get; set; } = 0;
        public bool Enabled { get; set; } = true;
    }
}
