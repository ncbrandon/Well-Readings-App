namespace Well_Readings.Models
{
    public class MaintenanceSiteStatus
    {
        public Guid Id { get; set; }
        public string SiteName { get; set; } = string.Empty;
        public bool IsOn { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}