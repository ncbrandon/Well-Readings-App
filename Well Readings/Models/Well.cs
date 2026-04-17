using Well_Readings.Models;

public class Well
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";

    public ICollection<WellReading> WellReadings { get; set; }
        = new List<WellReading>();
}