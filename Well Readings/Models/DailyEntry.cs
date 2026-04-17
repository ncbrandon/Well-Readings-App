using System.ComponentModel.DataAnnotations;

namespace Well_Readings.Models
{
    public class DailyEntry
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DateOnly EntryDate { get; set; }

        [Required]
        public TimeOnly EntryTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<WellReading> WellReadings { get; set; } = new List<WellReading>();
        public FiltrationPlantReading? FiltrationPlantReading { get; set; }
    }
}
