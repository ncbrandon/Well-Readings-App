namespace Well_Readings.DTOs
{
    public class DailyEntryRequestDto
    {
        public DateOnly EntryDate { get; set; }
        public TimeOnly EntryTime { get; set; }

        public List<WellReadingDto> WellReadings { get; set; } = new();
        public FiltrationPlantReadingDto FiltrationPlant { get; set; }
    }
}
