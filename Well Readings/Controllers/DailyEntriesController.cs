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

        // =====================================================
        // CREATE / UPDATE
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> CreateDailyEntry(
            [FromBody] DailyEntryRequestDto request)
        {
            if (request.EntryDate == default)
                return BadRequest("Entry date is required.");

            var wellIdLookup = await _db.Wells
                .ToDictionaryAsync(w => w.Name, w => w.Id);

            DailyEntry dailyEntry;

            // ================= UPDATE =================
            if (request.Id.HasValue && request.Id != Guid.Empty)
            {
                dailyEntry = await _db.DailyEntries
                    .FirstOrDefaultAsync(d => d.Id == request.Id.Value);

                if (dailyEntry == null)
                    return NotFound("Daily entry not found.");

                dailyEntry.EntryDate = request.EntryDate;
                dailyEntry.EntryTime = request.EntryTime;

                await _db.WellReadings
                    .Where(w => w.DailyEntryId == dailyEntry.Id)
                    .ExecuteDeleteAsync();
            }
            else
            {
                // ================= CREATE =================
                if (await _db.DailyEntries.AnyAsync(d => d.EntryDate == request.EntryDate))
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

            if (request.WellReadings?.Any() == true)
            {
                var newReadings = request.WellReadings.Select(wr => new WellReading
                {
                    Id = Guid.NewGuid(),
                    DailyEntryId = dailyEntry.Id,
                    WellId = wellIdLookup[wr.WellName],
                    MeterReading = wr.MeterReading,
                    Chlorine = wr.Chlorine,
                    Phosphate = wr.Phosphate,
                    Ph = wr.Ph
                });

                await _db.WellReadings.AddRangeAsync(newReadings);
            }

            await _db.SaveChangesAsync();
            return Ok(new { dailyEntry.Id });
        }

        // =====================================================
        // LIST (Daily Log)
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entries = await _db.DailyEntries
                .AsNoTracking()
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

        // =====================================================
        // TODAY
        // =====================================================
        [HttpGet("today")]
        public async Task<IActionResult> GetToday()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var entry = await _db.DailyEntries
                .AsNoTracking()
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

        // =====================================================
        // GET BY ID
        // =====================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entry = await _db.DailyEntries
                .AsNoTracking()
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
                .FirstOrDefaultAsync();

            if (entry == null)
                return NotFound();

            return Ok(entry);
        }

        // =====================================================
        // RANGE SUMMARY (FIXED + ACCURATE DAILY USAGE)
        // =====================================================
        [HttpGet("reports/range-summary")]
        public async Task<IActionResult> GetRangeSummary(
            [FromQuery] DateOnly start,
            [FromQuery] DateOnly end)
        {
            if (start == default || end == default)
                return BadRequest("Start and End dates are required.");

            if (end < start)
                return BadRequest("End date must be >= start date.");

            var data = await _db.WellReadings
                .AsNoTracking()
                .Include(w => w.DailyEntry)
                .Include(w => w.Well)
                .Where(w => w.DailyEntry.EntryDate >= start &&
                            w.DailyEntry.EntryDate <= end)
                .Select(w => new
                {
                    WellName = w.Well.Name,
                    Date = w.DailyEntry.EntryDate,
                    MeterReading = w.MeterReading
                })
                .ToListAsync();

            var grouped = data
                .GroupBy(x => x.WellName)
                .Select(g =>
                {
                    var ordered = g
                        .OrderBy(x => x.Date)
                        .ToList();

                    var daily = new List<DailyTotalDto>();
                    decimal cumulative = 0;

                    for (int i = 0; i < ordered.Count; i++)
                    {
                        var current = ordered[i];
                        var previous = i > 0 ? ordered[i - 1].MeterReading : 0;

                        var gallons = current.MeterReading - previous;

                        if (gallons < 0)
                            gallons = 0;

                        cumulative += gallons;

                        daily.Add(new DailyTotalDto
                        {
                            Date = current.Date,
                            Gallons = gallons,
                            CumulativeGallons = cumulative
                        });
                    }

                    return new RangeSummaryRowDto
                    {
                        WellName = g.Key,
                        TotalGallons = daily.Sum(d => d.Gallons),
                        DailyTotals = daily
                    };
                })
                .OrderBy(x => x.WellName)
                .ToList();

            return Ok(grouped);
        }

        // =====================================================
        // MONTHLY REPORT (FIXED NAMING)
        // =====================================================
        [HttpGet("reports/monthly")]
        public async Task<IActionResult> GetMonthlyReport(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] string? site)
        {
            if (year <= 0 || month <= 0)
                return BadRequest("Year and Month are required.");

            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var query = _db.WellReadings
                .AsNoTracking()
                .Include(w => w.DailyEntry)
                .Include(w => w.Well)
                .Where(w => w.DailyEntry.EntryDate >= startDate &&
                            w.DailyEntry.EntryDate <= endDate);

            if (!string.IsNullOrWhiteSpace(site))
            {
                query = query.Where(w => w.Well.Name.Contains(site));
            }

            var rows = await query
                .OrderBy(w => w.DailyEntry.EntryDate)
                .ThenBy(w => w.DailyEntry.EntryTime)
                .Select(w => new MonthlyWellReportRowDto
                {
                    Date = w.DailyEntry.EntryDate,
                    EntryTime = w.DailyEntry.EntryTime,
                    WellName = w.Well.Name,
                    MeterReading = w.MeterReading,
                    Chlorine = w.Chlorine,
                    Phosphate = w.Phosphate,
                    Ph = w.Ph
                })
                .ToListAsync();

            return Ok(rows);
        }

        // =====================================================
        // DELETE
        // =====================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entry = await _db.DailyEntries.FindAsync(id);

            if (entry == null)
                return NotFound();

            _db.DailyEntries.Remove(entry);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
