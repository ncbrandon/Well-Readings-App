using Microsoft.EntityFrameworkCore;
using Well_Readings.Models;

namespace Well_Readings.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<DailyEntry> DailyEntries { get; set; }
        public DbSet<WellReading> WellReadings { get; set; }
        public DbSet<FiltrationPlantReading> FiltrationPlantReadings { get; set; }
        public DbSet<Well> Wells { get; set; }
        public DbSet<WellAlarmConfig> WellAlarmConfigs { get; set; }
        public DbSet<ScadaHistoryPoint> ScadaHistoryPoints { get; set; }
        public DbSet<MaintenancePumpInstall> MaintenancePumpInstalls { get; set; }
        public DbSet<Plant> Plants { get; set; }
        public DbSet<WellAlarm> WellAlarms => Set<WellAlarm>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WellReading>()
                .HasOne(w => w.DailyEntry)
                .WithMany(d => d.WellReadings)
                .HasForeignKey(w => w.DailyEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WellReading>()
                .HasOne(w => w.Well)
                .WithMany()
                .HasForeignKey(w => w.WellId);

            modelBuilder.Entity<WellReading>(entity =>
            {
                entity.Property(e => e.MeterReading).HasPrecision(18, 0);
                entity.Property(e => e.Chlorine).HasPrecision(3, 1);
                entity.Property(e => e.Phosphate).HasPrecision(3, 1);
                entity.Property(e => e.Ph).HasPrecision(3, 1);
            });

            modelBuilder.Entity<FiltrationPlantReading>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.FlowRate);
                entity.Property(x => x.Turbidity);
                entity.Property(x => x.Chlorine);
                entity.Property(x => x.Ph);
                entity.Property(x => x.Temperature);
                entity.Property(x => x.Timestamp);
            });

            modelBuilder.Entity<ScadaHistoryPoint>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Timestamp)
                    .IsRequired();

                entity.Property(x => x.Location)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.MetricType)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.SourceColumn)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Value)
                    .HasPrecision(18, 3);

                entity.HasIndex(x => new { x.Timestamp, x.Location, x.MetricType });
            });

            modelBuilder.Entity<MaintenancePumpInstall>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.SiteName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.PumpType)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.InstalledDate)
                    .IsRequired();

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.HasIndex(x => new { x.SiteName, x.PumpType, x.InstalledDate });
            });
        }
    }
}