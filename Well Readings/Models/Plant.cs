namespace Well_Readings.Models
{
    public class Plant
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";

        public List<WellReading> Readings { get; set; } = new();
    }
}
