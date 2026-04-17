using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Well_Readings.Models
{

    public class WellReading
    {
        public Guid Id { get; set; }

        public Guid DailyEntryId { get; set; }
        public DailyEntry DailyEntry { get; set; } = null!;

        public Guid WellId { get; set; }
        public Well Well { get; set; } = null!;

        public decimal MeterReading { get; set; }
        public decimal? Chlorine { get; set; }
        public decimal? Phosphate { get; set; }
        public decimal? Ph { get; set; }
    }

}
