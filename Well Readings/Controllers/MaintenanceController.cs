using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Well_Readings.Data;
using Well_Readings.Models;

namespace Well_Readings.Controllers
{
    [ApiController]
    [Route("api/maintenance")]
    public class MaintenanceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MaintenanceController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("pump-install")]
        public async Task<IActionResult> SavePumpInstall([FromBody] MaintenancePumpInstall request)
        {
            if (request == null)
                return BadRequest("No maintenance data was submitted.");

            if (string.IsNullOrWhiteSpace(request.SiteName))
                return BadRequest("Site name is required.");

            if (string.IsNullOrWhiteSpace(request.PumpType))
                return BadRequest("Pump type is required.");

            if (request.InstalledDate == default)
                return BadRequest("Installed date is required.");

            request.Id = Guid.NewGuid();
            request.CreatedAt = DateTime.Now;

            _context.MaintenancePumpInstalls.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                saved = true,
                request.SiteName,
                request.PumpType,
                request.InstalledDate
            });
        }

        [HttpGet("pump-installs")]
        public async Task<IActionResult> GetPumpInstalls()
        {
            var installs = await _context.MaintenancePumpInstalls
                .OrderByDescending(x => x.InstalledDate)
                .ToListAsync();

            return Ok(installs);
        }
    }
}