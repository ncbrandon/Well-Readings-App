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
            new DistributionPointLocation { Code = "001", Location = "S. Jefferson Avenue" },
            new DistributionPointLocation { Code = "002", Location = "406 School Avenue" },
            new DistributionPointLocation { Code = "006", Location = "485 Beaver Creek School Road" },
            new DistributionPointLocation { Code = "007", Location = "4083 US Hwy 221 South" },
            new DistributionPointLocation { Code = "008", Location = "335 Clearwater Drive" },
            new DistributionPointLocation { Code = "012", Location = "1380 Mt. Jefferson Road" },
            new DistributionPointLocation { Code = "014", Location = "11 Ashemount Drive" },
            new DistributionPointLocation { Code = "015", Location = "432 E. Second Street" }
        };

        [HttpGet("locations")]
        public IActionResult GetLocations()
        {
            return Ok(Locations.OrderBy(x => x.Code));
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export(DateTime startDate, DateTime endDate)
        {
            var endExclusive = endDate.Date.AddDays(1);

            var entries = await _context.DistributionPointEntries
                .Where(x => x.EntryDate >= startDate.Date && x.EntryDate < endExclusive)
                .OrderBy(x => x.EntryDate)
                .ThenBy(x => x.Code)
                .ToListAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("DistributionPoints");

            ws.Cell(1, 1).Value = "EntryDate";
            ws.Cell(1, 2).Value = "Code";
            ws.Cell(1, 3).Value = "Location";
            ws.Cell(1, 4).Value = "Chlorine";
            ws.Cell(1, 5).Value = "Phosphate";
            ws.Cell(1, 6).Value = "pH";

            var row = 2;

            foreach (var entry in entries)
            {
                ws.Cell(row, 1).Value = entry.EntryDate;
                ws.Cell(row, 1).Style.DateFormat.Format = "yyyy-mm-dd";

                ws.Cell(row, 2).Value = entry.Code;
                ws.Cell(row, 3).Value = entry.Location;
                ws.Cell(row, 4).Value = entry.Chlorine;
                ws.Cell(row, 5).Value = entry.Phosphate;
                ws.Cell(row, 6).Value = entry.Ph;

                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"DistributionPoints_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx"
            );
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file was uploaded.");

            var imported = 0;
            var updated = 0;
            var skipped = 0;

            using var stream = file.OpenReadStream();
            using var workbook = new ClosedXML.Excel.XLWorkbook(stream);

            var ws =
                workbook.Worksheets.FirstOrDefault(x => x.Name.Equals("DistributionPoints", StringComparison.OrdinalIgnoreCase))
                ?? workbook.Worksheets.First();

            var lastRow = ws.LastRowUsed().RowNumber();

            for (var row = 2; row <= lastRow; row++)
            {
                if (!DateTime.TryParse(ws.Cell(row, 1).GetString(), out var entryDate))
                {
                    if (ws.Cell(row, 1).TryGetValue<DateTime>(out var excelDate))
                        entryDate = excelDate;
                    else
                    {
                        skipped++;
                        continue;
                    }
                }

                var code = ws.Cell(row, 2).GetString().Trim().PadLeft(3, '0');

                var location = Locations.FirstOrDefault(x => x.Code == code);

                if (location == null)
                {
                    skipped++;
                    continue;
                }

                decimal? chlorine = TryDecimal(ws.Cell(row, 4).GetString());
                decimal? phosphate = TryDecimal(ws.Cell(row, 5).GetString());
                decimal? ph = TryDecimal(ws.Cell(row, 6).GetString());

                var existing = await _context.DistributionPointEntries
                    .FirstOrDefaultAsync(x =>
                        x.EntryDate.Date == entryDate.Date &&
                        x.Code == code);

                if (existing == null)
                {
                    _context.DistributionPointEntries.Add(new DistributionPointEntry
                    {
                        Id = Guid.NewGuid(),
                        EntryDate = entryDate.Date,
                        Code = code,
                        Location = location.Location,
                        Chlorine = chlorine,
                        Phosphate = phosphate,
                        Ph = ph,
                        CreatedAt = DateTime.Now
                    });

                    imported++;
                }
                else
                {
                    existing.Location = location.Location;
                    existing.Chlorine = chlorine;
                    existing.Phosphate = phosphate;
                    existing.Ph = ph;

                    updated++;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                imported,
                updated,
                skipped
            });
        }

        private static decimal? TryDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return decimal.TryParse(value, out var result)
                ? result
                : null;
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
            public string Code { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
        }
    }
}