namespace Well_Readings.Models
{
    public class MaintenancePumpInstall
    {
        public Guid Id { get; set; }

        public string SiteName { get; set; } = string.Empty;

        public string PumpType { get; set; } = string.Empty;

        public DateTime InstalledDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}