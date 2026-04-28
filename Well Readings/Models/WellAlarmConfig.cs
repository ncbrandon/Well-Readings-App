using System.ComponentModel.DataAnnotations;

namespace Well_Readings.Models
{
    public class WellAlarmConfig
    {
        [Key]
        public Guid WellId { get; set; }

        public decimal HighThreshold { get; set; } = 10000;

        public bool IsAcknowledged { get; set; } = true;

        public DateTime? LastAlarmTime { get; set; }
    }
}
