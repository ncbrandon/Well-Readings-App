using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Well_Readings.Data;
using Well_Readings.Models;

namespace Well_Readings.Controllers
{
    [ApiController]
    [Route("api/distribution-points")]
    public class DistributionPointsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DistributionPointsController(AppDbContext context)
        {
            _context = context;
        }

        private static readonly List<DistributionPointLocation> Locations = new()
        {
            new DistributionPointLocation { Code = 1, Location = "1 S. Jefferson Avenue" },
            new DistributionPointLocation { Code = 2, Location = "406 School Avenue" },
            new DistributionPointLocation { Code = 6, Location = "485 Beaver Creek School Road" },
            new DistributionPointLocation { Code = 7, Location = "4083 US Hwy 221 South" },
            new DistributionPointLocation { Code = 8, Location = "335 Clearwater Drive" },
            new DistributionPointLocation { Code = 12, Location = "1380 Mt. Jefferson Road" },
            new DistributionPointLocation { Code = 14, Location = "11 Ashemount Drive" },
            new DistributionPointLocation { Code = 15, Location = "432 E. Second Street" }
        };

        [HttpGet("locations")]
        public IActionResult GetLocations()
        {
            return Ok(Locations.OrderBy(x => x.Code));
        }

        [HttpPost("entry")]
        public async Task<IActionResult> SaveEntry([FromBody] DistributionPointEntry request)
        {
            if (request == null)
                return BadRequest("No entry was submitted.");

            var location = Locations.FirstOrDefault(x => x.Code == request.Code);

            if (location == null)
                return BadRequest("Invalid distribution point code.");

            if (request.EntryDate == default)
                request.EntryDate = DateTime.Today;

            request.Location = location.Location;

            var existing = await _context.DistributionPointEntries
                .FirstOrDefaultAsync(x =>
                    x.EntryDate.Date == request.EntryDate.Date &&
                    x.Code == request.Code);

            if (existing == null)
            {
                request.Id = Guid.NewGuid();
                request.CreatedAt = DateTime.Now;

                _context.DistributionPointEntries.Add(request);
            }
            else
            {
                existing.Location = location.Location;
                existing.Chlorine = request.Chlorine;
                existing.Phosphate = request.Phosphate;
                existing.Ph = request.Ph;
            }

            await _context.SaveChangesAsync();

            return Ok(new { saved = true });
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetReport(DateTime startDate, DateTime endDate)
        {
            var endExclusive = endDate.Date.AddDays(1);

            var entries = await _context.DistributionPointEntries
                .Where(x => x.EntryDate >= startDate.Date && x.EntryDate < endExclusive)
                .OrderByDescending(x => x.EntryDate)
                .ThenBy(x => x.Code)
                .ToListAsync();

            return Ok(entries);
        }

        [HttpGet("entry/{id}")]
        public async Task<IActionResult> GetEntry(Guid id)
        {
            var entry = await _context.DistributionPointEntries.FindAsync(id);

            if (entry == null)
                return NotFound();

            return Ok(entry);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateEntry([FromBody] DistributionPointEntry request)
        {
            var existing = await _context.DistributionPointEntries.FindAsync(request.Id);

            if (existing == null)
                return NotFound();

            var location = Locations.FirstOrDefault(x => x.Code == request.Code);

            if (location == null)
                return BadRequest("Invalid distribution point code.");

            existing.EntryDate = request.EntryDate.Date;
            existing.Code = request.Code;
            existing.Location = location.Location;
            existing.Chlorine = request.Chlorine;
            existing.Phosphate = request.Phosphate;
            existing.Ph = request.Ph;

            await _context.SaveChangesAsync();

            return Ok(new { updated = true });
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteEntry(Guid id)
        {
            var entry = await _context.DistributionPointEntries.FindAsync(id);

            if (entry == null)
                return NotFound();

            _context.DistributionPointEntries.Remove(entry);
            await _context.SaveChangesAsync();

            return Ok(new { deleted = true });
        }

        public class DistributionPointLocation
        {
            public int Code { get; set; }
            public string Location { get; set; } = string.Empty;
        }
    }
}