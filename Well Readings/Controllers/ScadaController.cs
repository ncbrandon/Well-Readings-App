using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Well_Readings.Data;

namespace Well_Readings.Controllers
{
    [ApiController]
    [Route("api/scada")]
    public class ScadaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ScadaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var wells = await _context.WellReadings
                .Include(x => x.Well)
                .ToListAsync();

            var wellData = wells
                .GroupBy(x => x.Well.Name)
                .Select(g => new
                {
                    wellName = g.Key,

                    lastReading = g
                        .OrderByDescending(x => x.Timestamp)
                        .Select(x => x.MeterReading)
                        .FirstOrDefault(),

                    totalGallons = g.Sum(x => x.MeterReading),

                    isAlarm = g.Any(x => x.IsAlarm)
                })
                .ToList();

            var plant = await _context.FiltrationPlantReadings
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefaultAsync();

            var plantTile = new
            {
                plantName = "Filtration Plant",
                flowRate = plant?.FlowRate ?? 0,
                turbidity = plant?.Turbidity ?? 0,
                chlorine = plant?.Chlorine ?? 0,
                ph = plant?.Ph ?? 0,
                isAlarm = plant?.IsAlarm ?? false
            };

            return Ok(new
            {
                wells = wellData,
                plant = plantTile
            });
        }
    }
}
