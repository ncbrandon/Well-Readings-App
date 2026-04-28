using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Well_Readings.Models
{
    public class WellReading
    {
        [Key]
        public Guid Id { get; set; }

        public Guid WellId { get; set; }
        public Well Well { get; set; }

        public Guid? DailyEntryId { get; set; }
        public DailyEntry? DailyEntry { get; set; }

        public DateTime Timestamp { get; set; }

        // SCADA VALUES
        public decimal MeterReading { get; set; }
        public decimal Chlorine { get; set; }
        public decimal Phosphate { get; set; }
        public decimal Ph { get; set; }

        // Alarm support
        public bool IsAlarm { get; set; }
        public decimal? AlarmThreshold { get; set; }
    }
}
