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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- Well readings ----
            modelBuilder.Entity<WellReading>(entity =>
            {
                entity.Property(e => e.MeterReading)
                      .HasPrecision(18, 0);

                entity.Property(e => e.Chlorine)
                      .HasPrecision(3, 1);

                entity.Property(e => e.Phosphate)
                      .HasPrecision(3, 1);

                entity.Property(e => e.Ph)
                      .HasPrecision(3, 1);
            });

            // ---- Filtration plant ----
            modelBuilder.Entity<FiltrationPlantReading>(entity =>
            {
                entity.Property(e => e.FilterPlantMeterReading)
                      .HasPrecision(18, 0);

                entity.Property(e => e.MtJeffersonMeterReading)
                      .HasPrecision(18, 0);

                entity.Property(e => e.Chlorine)
                      .HasPrecision(3, 1);

                entity.Property(e => e.Phosphate)
                      .HasPrecision(3, 1);

                entity.Property(e => e.Ph)
                      .HasPrecision(3, 1);

                entity.Property(e => e.Temperature)
                      .HasPrecision(3, 1);
            });
        }
    }
}
