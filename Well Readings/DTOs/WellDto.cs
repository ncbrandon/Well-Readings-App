using Microsoft.AspNetCore.Mvc;

namespace Well_Readings.DTOs
{
    public class WellDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
