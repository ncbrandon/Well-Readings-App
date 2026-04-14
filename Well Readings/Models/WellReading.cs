using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Well_Readings.Models
{
    public class WellReading
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid DailyEntryId { get; set; }

        [Required]
        public string WellName { get; set; } = string.Empty;

        [Required]
        public decimal MeterReading { get; set; }

        public decimal? Chlorine { get; set; }
        public decimal? Phosphate { get; set; }
        public decimal? Ph { get; set; }

        // Navigation property
        [ForeignKey(nameof(DailyEntryId))]
        public DailyEntry DailyEntry { get; set; }
    }
}
