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

        [HttpGet("site-statuses")]
        public async Task<IActionResult> GetSiteStatuses()
        {
            var statuses = await _context.MaintenanceSiteStatuses
                .OrderBy(x => x.SiteName)
                .ToListAsync();

            return Ok(statuses);
        }

        [HttpPost("site-status")]
        public async Task<IActionResult> SaveSiteStatus([FromBody] MaintenanceSiteStatus request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.SiteName))
                return BadRequest("Site name is required.");

            var existing = await _context.MaintenanceSiteStatuses
                .FirstOrDefaultAsync(x => x.SiteName == request.SiteName);

            if (existing == null)
            {
                request.Id = Guid.NewGuid();
                request.UpdatedAt = DateTime.Now;
                _context.MaintenanceSiteStatuses.Add(request);
            }
            else
            {
                existing.IsOn = request.IsOn;
                existing.NeedChlorine = request.NeedChlorine;
                existing.NeedPhosphate = request.NeedPhosphate;
                existing.NeedInjector = request.NeedInjector;
                existing.NeedChemicalPump = request.NeedChemicalPump;   
                existing.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Ok(new { saved = true });
        }
    }
}