using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Well_Readings.Data;

namespace Well_Readings.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var readings = await _context.WellReadings
            .Include(r => r.Well)
            .Include(r => r.DailyEntry)
            .ToListAsync();

        var data = readings
            .Where(r => r.Well != null && r.DailyEntry != null)
            .GroupBy(r => r.Well.Name)
            .Select(g => new
            {
                WellName = g.Key,
                TotalReadings = g.Count(),
                TotalGallons = g.Sum(x => x.MeterReading)
            })
            .ToList();

        return Ok(data);
    }
}
