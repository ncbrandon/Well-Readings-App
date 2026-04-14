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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- Well readings ----
            modelBuilder.Entity<WellReading>(entity =>
            {
                entity.Property(e => e.MeterReading)
                      .HasPrecision(9, 0);

                entity.Property(e => e.Chlorine)
                      .HasPrecision(1, 1);

                entity.Property(e => e.Phosphate)
                      .HasPrecision(1, 1);

                entity.Property(e => e.Ph)
                      .HasPrecision(2, 1);
            });

            // ---- Filtration plant ----
            modelBuilder.Entity<FiltrationPlantReading>(entity =>
            {
                entity.Property(e => e.FilterPlantMeterReading)
                      .HasPrecision(18, 0);

                entity.Property(e => e.MtJeffersonMeterReading)
                      .HasPrecision(18, 0);

                entity.Property(e => e.Chlorine)
                      .HasPrecision(4, 1);

                entity.Property(e => e.Phosphate)
                      .HasPrecision(4, 1);

                entity.Property(e => e.Ph)
                      .HasPrecision(3, 1);

                entity.Property(e => e.Temperature)
                      .HasPrecision(4, 1);
            });
        }
    }
}
