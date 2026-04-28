using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Well_Readings.Data;
using Well_Readings.Models;

[ApiController]
[Route("api/alarms")]
public class AlarmController : ControllerBase
{
    private readonly AppDbContext _db;

    public AlarmController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _db.WellAlarms.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Save(WellAlarm alarm)
    {
        var existing = await _db.WellAlarms
            .FirstOrDefaultAsync(x => x.WellId == alarm.WellId);

        if (existing == null)
            _db.WellAlarms.Add(alarm);
        else
        {
            existing.HighLimit = alarm.HighLimit;
            existing.LowLimit = alarm.LowLimit;
            existing.Enabled = alarm.Enabled;
        }

        await _db.SaveChangesAsync();
        return Ok();
    }
}
