using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Well_Readings.Data;
using Well_Readings.Models;

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
            var meterColumns = new[]
            {
                "Reeves", "Reeves A", "Park", "Park A", "Park B",
                "Woods", "Catawissa", "New", "Oakwood", "Ray"
            };

            var wells = new List<object>();

            foreach (var well in meterColumns)
            {
                var readings = await _context.ScadaHistoryPoints
                    .Where(x => x.Location == well && x.MetricType == "Meter Reading")
                    .OrderBy(x => x.Timestamp)
                    .ToListAsync();

                if (!readings.Any())
                    continue;

                var first = readings.First().Value ?? 0;
                var last = readings.Last().Value ?? 0;

                wells.Add(new
                {
                    wellName = well,
                    lastReading = last,
                    totalGallons = last - first,
                    isAlarm = false
                });
            }

            var plantFlow = await GetLatestValue("Filter Plant", "Feed Flow");
            var plantPh = await GetLatestValue("Filter Plant", "pH");
            var plantChlorine = await GetLatestValue("Filter Plant", "Chlorine");
            var plantTemp = await GetLatestValue("Filter Plant", "Temperature");
            var plantTurbidity = await GetLatestValue("Filter Plant", "Turbidity");

            var plant = new
            {
                flowRate = plantFlow,
                turbidity = plantTurbidity,
                chlorine = plantChlorine,
                ph = plantPh,
                temperature = plantTemp
            };

            return Ok(new
            {
                wells,
                plant
            });
        }

        [HttpPost("daily-entry")]
        public async Task<IActionResult> SaveDailyEntry([FromBody] DailyEntryRequest request)
        {
            if (request == null || request.Readings == null || !request.Readings.Any())
                return BadRequest("No readings were submitted.");

            var entryDate = request.EntryDate.Date;

            var newPoints = request.Readings
                .Where(x => x.Value.HasValue)
                .Select(x => new ScadaHistoryPoint
                {
                    Id = Guid.NewGuid(),
                    Timestamp = entryDate,
                    Location = x.Location,
                    MetricType = x.MetricType,
                    SourceColumn = x.SourceColumn,
                    Value = x.Value
                })
                .ToList();

            var locations = newPoints.Select(x => x.Location).Distinct().ToList();

            var existing = await _context.ScadaHistoryPoints
                .Where(x => x.Timestamp == entryDate && locations.Contains(x.Location))
                .ToListAsync();

            _context.ScadaHistoryPoints.RemoveRange(existing);
            _context.ScadaHistoryPoints.AddRange(newPoints);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                saved = newPoints.Count,
                date = entryDate
            });
        }

        public class DailyEntryRequest
        {
            public DateTime EntryDate { get; set; }
            public List<DailyReadingDto> Readings { get; set; } = new();
        }

        public class DailyReadingDto
        {
            public string Location { get; set; } = string.Empty;
            public string MetricType { get; set; } = string.Empty;
            public string SourceColumn { get; set; } = string.Empty;
            public decimal? Value { get; set; }
        }

        [HttpPost("import-excel")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No Excel file was uploaded.");

            var points = new List<ScadaHistoryPoint>();

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var headerRow = worksheet.Row(1);
            var lastRow = worksheet.LastRowUsed().RowNumber();
            var lastColumn = worksheet.LastColumnUsed().ColumnNumber();

            for (int row = 2; row <= lastRow; row++)
            {
                var timestampCell = worksheet.Cell(row, 1);

                if (!timestampCell.TryGetValue<DateTime>(out var timestamp))
                    continue;

                for (int col = 2; col <= lastColumn; col++)
                {
                    var sourceColumn = headerRow.Cell(col).GetString().Trim();

                    if (string.IsNullOrWhiteSpace(sourceColumn))
                        continue;

                    if (sourceColumn.StartsWith("Unnamed", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var cell = worksheet.Cell(row, col);

                    if (!cell.TryGetValue<decimal>(out var value))
                        continue;

                    points.Add(new ScadaHistoryPoint
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = timestamp,
                        SourceColumn = sourceColumn,
                        Location = GetLocation(sourceColumn),
                        MetricType = GetMetricType(sourceColumn),
                        Value = value
                    });
                }
            }

            if (!points.Any())
                return BadRequest("No valid data was found in the Excel file.");

            var minDate = points.Min(x => x.Timestamp);
            var maxDate = points.Max(x => x.Timestamp);

            var existing = await _context.ScadaHistoryPoints
                .Where(x => x.Timestamp >= minDate && x.Timestamp <= maxDate)
                .ToListAsync();

            _context.ScadaHistoryPoints.RemoveRange(existing);
            _context.ScadaHistoryPoints.AddRange(points);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                imported = points.Count,
                startDate = minDate,
                endDate = maxDate
            });
        }

        [HttpPost("manual-entry")]
        public async Task<IActionResult> ManualEntry([FromBody] List<ScadaHistoryPoint> readings)
        {
            if (readings == null || readings.Count == 0)
                return BadRequest("No readings were submitted.");

            foreach (var reading in readings)
            {
                reading.Id = Guid.NewGuid();

                if (reading.Timestamp == default)
                    reading.Timestamp = DateTime.Today;

                if (string.IsNullOrWhiteSpace(reading.SourceColumn))
                    reading.SourceColumn = $"{reading.Location} - {reading.MetricType}";
            }

            _context.ScadaHistoryPoints.AddRange(readings);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                saved = readings.Count
            });
        }

        [HttpGet("daily-entries")]
        public async Task<IActionResult> GetDailyEntries()
        {
            var entries = await _context.ScadaHistoryPoints
                .GroupBy(x => x.Timestamp)
                .Select(g => new
                {
                    timestamp = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(x => x.timestamp)
                .ToListAsync();

            return Ok(entries);
        }

        [HttpGet("daily-entry")]
        public async Task<IActionResult> GetDailyEntry(DateTime timestamp)
        {
            var entries = await _context.ScadaHistoryPoints
                .Where(x => x.Timestamp == timestamp)
                .Select(x => new
                {
                    x.Id,
                    x.Timestamp,
                    x.Location,
                    x.MetricType,
                    x.SourceColumn,
                    x.Value
                })
                .ToListAsync();

            return Ok(entries);
        }

        [HttpPost("daily-entry/update")]
        public async Task<IActionResult> UpdateDailyEntry([FromBody] DailyEntryUpdateRequest request)
        {
            if (request == null)
                return BadRequest("No data was submitted.");

            if (request.Timestamp == default)
                return BadRequest("Timestamp is required.");

            var existing = await _context.ScadaHistoryPoints
                .Where(x => x.Timestamp == request.Timestamp)
                .ToListAsync();

            _context.ScadaHistoryPoints.RemoveRange(existing);

            var newEntries = request.Readings
                .Where(x => x.Value.HasValue)
                .Select(x => new ScadaHistoryPoint
                {
                    Id = Guid.NewGuid(),
                    Timestamp = request.Timestamp,
                    Location = x.Location,
                    MetricType = x.MetricType,
                    SourceColumn = x.SourceColumn,
                    Value = x.Value
                })
                .ToList();

            _context.ScadaHistoryPoints.AddRange(newEntries);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                updated = true,
                saved = newEntries.Count
            });
        }

        public class DailyEntryUpdateRequest
        {
            public DateTime Timestamp { get; set; }
            public List<DailyEntryReadingDto> Readings { get; set; } = new();
        }

        public class DailyEntryReadingDto
        {
            public string Location { get; set; } = string.Empty;
            public string MetricType { get; set; } = string.Empty;
            public string SourceColumn { get; set; } = string.Empty;
            public decimal? Value { get; set; }
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetReport(DateTime startDate, DateTime endDate)
        {
            var report = await _context.ScadaHistoryPoints
                .Where(x => x.Timestamp >= startDate && x.Timestamp <= endDate)
                .GroupBy(x => new { x.Location, x.MetricType })
                .Select(g => new
                {
                    location = g.Key.Location,
                    metricType = g.Key.MetricType,
                    count = g.Count(),
                    firstValue = g.OrderBy(x => x.Timestamp).Select(x => x.Value).FirstOrDefault(),
                    lastValue = g.OrderByDescending(x => x.Timestamp).Select(x => x.Value).FirstOrDefault(),
                    minimum = g.Min(x => x.Value),
                    maximum = g.Max(x => x.Value),
                    average = g.Average(x => x.Value)
                })
                .OrderBy(x => x.location)
                .ThenBy(x => x.metricType)
                .ToListAsync();

            return Ok(report);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(int days = 7)
        {
            var startDate = DateTime.Today.AddDays(-days);

            var history = await _context.ScadaHistoryPoints
                .Where(x => x.Timestamp >= startDate && x.MetricType == "Meter Reading")
                .GroupBy(x => x.Location)
                .Select(g => new
                {
                    wellName = g.Key,
                    points = g.OrderBy(x => x.Timestamp)
                        .Select(x => new
                        {
                            time = x.Timestamp,
                            value = x.Value
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(history);
        }

        private async Task<decimal> GetLatestValue(string location, string metricType)
        {
            return await _context.ScadaHistoryPoints
                .Where(x => x.Location == location && x.MetricType == metricType)
                .OrderByDescending(x => x.Timestamp)
                .Select(x => x.Value ?? 0)
                .FirstOrDefaultAsync();
        }

        private static string GetLocation(string sourceColumn)
        {
            return sourceColumn switch
            {
                "Reeves" => "Reeves",
                "Reeves A" => "Reeves A",
                "Park" => "Park",
                "Park A" => "Park A",
                "Park B" => "Park B",
                "Woods" => "Woods",
                "Catawissa" => "Catawissa",
                "New" => "New",
                "Oakwood" => "Oakwood",
                "Ray" => "Ray",

                "Cl-R" or "PO4-R" or "pH-R" => "Reeves",
                "Cl-P" or "PO4-P" or "pH-P" => "Park",
                "Cl-W" or "PO4-W" or "pH-W" => "Woods",
                "Cl-C" or "PO4-C" or "pH-C" => "Catawissa",
                "Cl-N" or "PO4-N" or "pH-N" => "New",
                "Cl-O" or "PO4-O" or "pH-O" => "Oakwood",
                "Cl-Ray" or "PO4-Ray" or "pH-Ray" => "Ray",

                _ when sourceColumn.Contains("Filter", StringComparison.OrdinalIgnoreCase) => "Filter Plant",
                _ when sourceColumn.Contains("Feed", StringComparison.OrdinalIgnoreCase) => "Filter Plant",
                _ when sourceColumn.Contains("Filtrate", StringComparison.OrdinalIgnoreCase) => "Filter Plant",
                _ when sourceColumn.Contains("TMP", StringComparison.OrdinalIgnoreCase) => "Filter Plant",
                _ when sourceColumn.EndsWith("-F", StringComparison.OrdinalIgnoreCase) => "Filter Plant",

                _ => sourceColumn
            };
        }

        private static string GetMetricType(string sourceColumn)
        {
            if (sourceColumn is "Reeves" or "Reeves A" or "Park" or "Park A" or "Park B"
                or "Woods" or "Catawissa" or "New" or "Oakwood" or "Ray"
                or "Filter Plant" or "Mt. Jefferson")
                return "Meter Reading";

            if (sourceColumn.StartsWith("Cl", StringComparison.OrdinalIgnoreCase))
                return "Chlorine";

            if (sourceColumn.StartsWith("PO4", StringComparison.OrdinalIgnoreCase))
                return "Phosphate";

            if (sourceColumn.StartsWith("pH", StringComparison.OrdinalIgnoreCase))
                return "pH";

            if (sourceColumn.Contains("Temp", StringComparison.OrdinalIgnoreCase))
                return "Temperature";

            if (sourceColumn.Contains("Flow", StringComparison.OrdinalIgnoreCase))
                return sourceColumn.Contains("Feed", StringComparison.OrdinalIgnoreCase)
                    ? "Feed Flow"
                    : "Flow";

            if (sourceColumn.Contains("Pressure", StringComparison.OrdinalIgnoreCase))
                return sourceColumn;

            if (sourceColumn.Contains("Turb", StringComparison.OrdinalIgnoreCase))
                return "Turbidity";

            return sourceColumn;
        }
    }
}