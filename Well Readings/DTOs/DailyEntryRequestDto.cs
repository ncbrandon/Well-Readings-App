namespace Well_Readings.DTOs
{
    public class DailyEntryRequestDto
    {
        public Guid? Id { get; set; }

        public DateOnly EntryDate { get; set; }

        public TimeOnly EntryTime { get; set; }

        public List<WellReadingDto> WellReadings { get; set; } = new();

        // Add this property to support Filtration Plant data
        public FiltrationPlantReadingDto? FiltrationPlant { get; set; }
    }
}
