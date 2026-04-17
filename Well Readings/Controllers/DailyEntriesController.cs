using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Well_Readings.Data;
using Well_Readings.DTOs;
using Well_Readings.Models;

namespace Well_Readings.Controllers
{
    [ApiController]
    [Route("api/daily-entries")]
    public class DailyEntriesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DailyEntriesController(AppDbContext db)
        {
            _db = db;
        }

        // =========================
        // CREATE / UPDATE
        // =========================
        [HttpPost]
        public async Task<IActionResult> CreateDailyEntry(
    [FromBody] DailyEntryRequestDto request)
        {
            // ✅ Normalize Guid.Empty
            if (request.Id == Guid.Empty)
                request.Id = null;

            if (request.EntryDate == default)
                return BadRequest("Entry date is required.");

            var wellIdLookup = await _db.Wells
                .ToDictionaryAsync(w => w.Name, w => w.Id);

            DailyEntry dailyEntry;

            if (request.Id.HasValue)
            {
                // ✅ UPDATE
                dailyEntry = await _db.DailyEntries
                    .Include(d => d.WellReadings)
                    .Include(d => d.FiltrationPlantReading)
                    .FirstOrDefaultAsync(d => d.Id == request.Id.Value);

                if (dailyEntry == null)
                    return NotFound("Daily entry not found.");

                dailyEntry.EntryDate = request.EntryDate;
                dailyEntry.EntryTime = request.EntryTime;

                // ✅ CRITICAL FIX
                _db.WellReadings.RemoveRange(dailyEntry.WellReadings);
                dailyEntry.WellReadings.Clear();
            }
            else
            {
                // ✅ CREATE (one per date)
                var exists = await _db.DailyEntries
                    .AnyAsync(d => d.EntryDate == request.EntryDate);

                if (exists)
                    return BadRequest("An entry already exists for this date.");

                dailyEntry = new DailyEntry
                {
                    Id = Guid.NewGuid(),
                    EntryDate = request.EntryDate,
                    EntryTime = request.EntryTime,
                    CreatedAt = DateTime.UtcNow
                };

                _db.DailyEntries.Add(dailyEntry);
            }

            // ✅ Re-add wells
            foreach (var wr in request.WellReadings)
            {
                dailyEntry.WellReadings.Add(new WellReading
                {
                    Id = Guid.NewGuid(),
                    WellId = wellIdLookup[wr.WellName],
                    MeterReading = wr.MeterReading,
                    Chlorine = wr.Chlorine,
                    Phosphate = wr.Phosphate,
                    Ph = wr.Ph
                });
            }

            await _db.SaveChangesAsync();
            return Ok(new { dailyEntry.Id });
        }

        // =========================
        // GET ALL (DailyLog)
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entries = await _db.DailyEntries
                .OrderByDescending(d => d.EntryDate)
                .ThenByDescending(d => d.EntryTime)
                .Select(d => new
                {
                    d.Id,
                    d.EntryDate,
                    d.EntryTime,
                    WellCount = d.WellReadings.Count
                })
                .ToListAsync();

            return Ok(entries);
        }


        // =========================
        // GET TODAY
        // =========================
        [HttpGet("today")]
        public async Task<IActionResult> GetToday()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var entry = await _db.DailyEntries
                .Where(d => d.EntryDate == today)
                .Select(d => new DailyEntryRequestDto
                {
                    Id = d.Id,
                    EntryDate = d.EntryDate,
                    EntryTime = d.EntryTime,
                    WellReadings = d.WellReadings.Select(w => new WellReadingDto
                    {
                        WellName = w.Well.Name,
                        MeterReading = w.MeterReading,
                        Chlorine = w.Chlorine,
                        Phosphate = w.Phosphate,
                        Ph = w.Ph
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (entry == null)
                return NotFound();

            return Ok(entry);
        }

        // =========================
        // GET BY ID (FIXED)
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entry = await _db.DailyEntries
                .Where(d => d.Id == id)
                .Select(d => new DailyEntryRequestDto
                {
                    Id = d.Id,
                    EntryDate = d.EntryDate,
                    EntryTime = d.EntryTime,
                    WellReadings = d.WellReadings.Select(w => new WellReadingDto
                    {
                        WellName = w.Well.Name,
                        MeterReading = w.MeterReading,
                        Chlorine = w.Chlorine,
                        Phosphate = w.Phosphate,
                        Ph = w.Ph
                    }).ToList()
                })
                .FirstOrDefaultAsync();   // ✅ FIXED

            if (entry == null)
                return NotFound();

            return Ok(entry);
        }
    }
}