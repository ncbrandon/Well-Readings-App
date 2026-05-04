namespace Well_Readings.Models
{
    public class MaintenanceSiteStatus
    {
        public Guid Id { get; set; }
        public string SiteName { get; set; } = string.Empty;
        public bool IsOn { get; set; }
        public bool NeedChlorine { get; set; }
        public bool NeedPhosphate { get; set; }
        public bool NeedInjector { get; set; }
        public bool NeedChemicalPump { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}