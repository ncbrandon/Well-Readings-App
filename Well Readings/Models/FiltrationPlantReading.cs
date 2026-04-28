using System.ComponentModel.DataAnnotations;

namespace Well_Readings.Models
{
    public class FiltrationPlantReading
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime Timestamp { get; set; }

        public decimal FlowRate { get; set; }
        public decimal Turbidity { get; set; }
        public decimal Chlorine { get; set; }
        public decimal Ph { get; set; }

        public decimal Temperature { get; set; }

        public bool IsAlarm { get; set; }
    }
}
