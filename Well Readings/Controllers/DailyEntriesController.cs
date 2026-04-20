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
        // RANGE SUMMARY (FIXED + LEAK DETECTION)
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
                .Where(w => w.DailyEntry.EntryDate >= start.AddDays(-1) &&
                            w.DailyEntry.EntryDate <= end)
                .Select(w => new
                {
                    WellName = w.Well.Name,
                    Date = w.DailyEntry.EntryDate,
                    MeterReading = w.MeterReading
                })
                .ToListAsync();

            var grouped = data
                // ✅ STEP 1: group by WELL
                .GroupBy(x => x.WellName)
                .Select(g =>
                {
                    // ✅ STEP 2: group by DATE (take MAX reading per day)
                    var dailyMax = g
                        .GroupBy(x => x.Date)
                        .Select(d => new
                        {
                            Date = d.Key,
                            MeterReading = d.Max(x => x.MeterReading)
                        })
                        .OrderBy(x => x.Date)
                        .ToList();

                    var daily = new List<DailyTotalDto>();

                    // ✅ STEP 3: calculate DAILY USAGE (difference)
                    for (int i = 0; i < dailyMax.Count; i++)
                    {
                        if (i == 0)
                        {
                            daily.Add(new DailyTotalDto
                            {
                                Date = dailyMax[i].Date,
                                Gallons = 0,
                                IsAnomaly = false
                            });
                            continue;
                        }

                        var prev = dailyMax[i - 1].MeterReading;
                        var current = dailyMax[i].MeterReading;

                        var gallons = current - prev;

                        // handle bad data / resets
                        if (gallons < 0)
                            gallons = 0;

                        daily.Add(new DailyTotalDto
                        {
                            Date = dailyMax[i].Date,
                            Gallons = gallons
                        });
                    }

                    // ✅ THIS IS THE CORRECT SPOT
                    daily = daily.Skip(1).ToList();

                    // ✅ STEP 4: anomaly detection (rolling avg)
                    for (int i = 0; i < daily.Count; i++)
                    {
                        var window = daily
                            .Skip(Math.Max(0, i - 3))
                            .Take(3)
                            .Where(d => d.Gallons > 0)
                            .ToList();

                        var avg = window.Any() ? window.Average(d => d.Gallons) : 0;

                        if (avg > 0 && daily[i].Gallons > avg * 1.5m)
                        {
                            daily[i].IsAnomaly = true;
                        }
                    }

                    // ✅ STEP 5: cumulative total
                    decimal cumulative = 0;

                    foreach (var d in daily)
                    {
                        cumulative += d.Gallons;
                        d.CumulativeGallons = cumulative;
                    }

                    return new RangeSummaryRowDto
                    {
                        WellName = g.Key,
                        TotalGallons = daily.Sum(d => d.Gallons),
                        MaxDailyGallons = daily.Any() ? daily.Max(d => d.Gallons) : 0,
                        DailyTotals = daily
                    };
                })
                .OrderBy(x => x.WellName)
                .ToList();

            return Ok(grouped);
        }


        // =====================================================
        // MONTHLY REPORT
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

        [HttpGet("reports/dashboard")]
        public async Task<IActionResult> GetDashboard(
    [FromQuery] DateOnly start,
    [FromQuery] DateOnly end)
        {
            if (start == default || end == default)
                return BadRequest("Start and End dates required.");

            var data = await _db.WellReadings
                .AsNoTracking()
                .Include(w => w.DailyEntry)
                .Include(w => w.Well)
                .Where(w => w.DailyEntry.EntryDate >= start.AddDays(-1) &&
                            w.DailyEntry.EntryDate <= end)
                .Select(w => new
                {
                    Well = w.Well.Name,
                    Date = w.DailyEntry.EntryDate,
                    Reading = w.MeterReading
                })
                .ToListAsync();

            var result = data
                .GroupBy(x => x.Well)
                .Select(g =>
                {
                    var dailyMax = g
                        .GroupBy(x => x.Date)
                        .Select(d => new
                        {
                            Date = d.Key,
                            Reading = d.Max(x => x.Reading)
                        })
                        .OrderBy(x => x.Date)
                        .ToList();

                    var daily = new List<DailyTotalDto>();

                    for (int i = 0; i < dailyMax.Count; i++)
                    {
                        if (i == 0)
                        {
                            daily.Add(new DailyTotalDto
                            {
                                Date = dailyMax[i].Date,
                                Gallons = 0
                            });
                            continue;
                        }

                        var gallons = dailyMax[i].Reading - dailyMax[i - 1].Reading;
                        if (gallons < 0) gallons = 0;

                        daily.Add(new DailyTotalDto
                        {
                            Date = dailyMax[i].Date,
                            Gallons = gallons
                        });
                    }

                    // remove baseline
                    daily = daily.Skip(1).ToList();

                    // anomaly detection
                    var avg = daily.Any() ? daily.Average(d => d.Gallons) : 0;

                    foreach (var d in daily)
                    {
                        d.IsAnomaly = avg > 0 && d.Gallons > avg * 1.5m;
                    }

                    return new
                    {
                        Well = g.Key,
                        Total = daily.Sum(d => d.Gallons),
                        Max = daily.Any() ? daily.Max(d => d.Gallons) : 0,
                        Data = daily
                    };
                })
                .OrderBy(x => x.Well)
                .ToList();

            return Ok(result);
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

