using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Well_Readings.Models
{
    public class FiltrationPlantReading
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid DailyEntryId { get; set; }

        // Meter readings (cumulative)
        [Required]
        public decimal FilterPlantMeterReading { get; set; }

        [Required]
        public decimal MtJeffersonMeterReading { get; set; }

        // Water quality
        public decimal? Chlorine { get; set; }
        public decimal? Phosphate { get; set; }
        public decimal? Ph { get; set; }
        public decimal? Temperature { get; set; }

        // Navigation
        [ForeignKey(nameof(DailyEntryId))]
        public DailyEntry DailyEntry { get; set; }
    }
}