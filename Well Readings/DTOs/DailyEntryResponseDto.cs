using Microsoft.AspNetCore.Mvc;

namespace Well_Readings.DTOs
{
    public class DailyEntryResponseDto
    {
        public Guid Id { get; set; }
        public DateOnly EntryDate { get; set; }
        public TimeOnly EntryTime { get; set; }

        public List<WellReadingResponseDto> WellReadings { get; set; } = new();
    }

}
