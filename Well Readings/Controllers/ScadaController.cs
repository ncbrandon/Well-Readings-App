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
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            var meterColumns = new[]
            {
        "Reeves Well",
        "Reeves Well A",
        "Park Well",
        "Park Well A",
        "Park Well B",
        "Woods",
        "Catawissa",
        "New",
        "Oakwood",
        "Ray",
        "Filter Plant"
    };

            var wells = new List<object>();

            foreach (var well in meterColumns)
            {
                var gallonsYesterday = await GetDeltaForDate(well, "Meter Reading", yesterday);
                var gallonsToday = await GetDeltaForDate(well, "Meter Reading", today);

                wells.Add(new
                {
                    wellName = well,
                    gallonsYesterday,
                    gallonsToday,
                    isAlarm = false
                });
            }

            var pumpNames = new[]
            {
        "Beaver Creek Pump 1",
        "Beaver Creek Pump 2",
        "Beaver Creek Generator",
        "Greenfield Pump 1",
        "Greenfield Pump 2",
        "Greenfield Generator",
        "Helen Blevins Pump 1",
        "Helen Blevins Pump 2",
        "Dogget Pump 1",
        "Dogget Pump 2"
    };

            var pumpStations = new List<object>();

            foreach (var pump in pumpNames)
            {
                var hoursYesterday = await GetDeltaForDate(pump, "Pump Status", yesterday);

                if (hoursYesterday == 0)
                    hoursYesterday = await GetDeltaForDate(pump, "Generator Status", yesterday);

                var hoursToday = await GetDeltaForDate(pump, "Pump Status", today);

                if (hoursToday == 0)
                    hoursToday = await GetDeltaForDate(pump, "Generator Status", today);

                pumpStations.Add(new
                {
                    name = pump,
                    hoursYesterday,
                    hoursToday,
                    flashRed = hoursToday == 0
                });
            }

            var pumpedYesterday = wells.Sum(x => (decimal)x.GetType().GetProperty("gallonsYesterday")!.GetValue(x)!);
            var pumpedToday = wells.Sum(x => (decimal)x.GetType().GetProperty("gallonsToday")!.GetValue(x)!);

            return Ok(new
            {
                wells,
                pumpStations,
                pumpedYesterday,
                pumpedToday,
                activeAlarms = 0
            });
        }

        private async Task<decimal> GetDeltaForDate(string location, string metricType, DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            var current = await _context.ScadaHistoryPoints
                .Where(x =>
                    x.Location == location &&
                    x.MetricType == metricType &&
                    x.Timestamp >= start &&
                    x.Timestamp < end)
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefaultAsync();

            var previous = await _context.ScadaHistoryPoints
                .Where(x =>
                    x.Location == location &&
                    x.MetricType == metricType &&
                    x.Timestamp < start)
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefaultAsync();

            if (current?.Value == null || previous?.Value == null)
                return 0;

            var delta = current.Value.Value - previous.Value.Value;

            return delta < 0 ? 0 : delta;
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

            var submittedTime = readings.First().Timestamp;

            if (submittedTime == default)
                submittedTime = DateTime.Now;

            var targetDate = submittedTime.Date;

            var existingForDate = await _context.ScadaHistoryPoints
                .Where(x => x.Timestamp.Date == targetDate)
                .ToListAsync();

            _context.ScadaHistoryPoints.RemoveRange(existingForDate);

            ApplyMeterTimestampSpacing(readings, submittedTime);

            foreach (var reading in readings)
            {
                reading.Id = Guid.NewGuid();

                if (reading.Timestamp == default)
                    reading.Timestamp = submittedTime;

                if (string.IsNullOrWhiteSpace(reading.SourceColumn))
                    reading.SourceColumn = $"{reading.Location} - {reading.MetricType}";
            }

            _context.ScadaHistoryPoints.AddRange(readings);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                saved = readings.Count,
                overwrittenDate = targetDate
            });
        }

        [HttpGet("entry-by-date")]
        public async Task<IActionResult> GetEntryByDate(DateTime date)
        {
            var targetDate = date.Date;

            var entries = await _context.ScadaHistoryPoints
                .Where(x => x.Timestamp.Date == targetDate)
                .Select(x => new
                {
                    x.Location,
                    x.MetricType,
                    x.SourceColumn,
                    x.Value,
                    x.Timestamp
                })
                .ToListAsync();

            return Ok(entries);
        }


        [HttpGet("daily-entries")]
        public async Task<IActionResult> GetDailyEntries()
        {
            var entries = _context.ScadaHistoryPoints
                .AsEnumerable()
                .GroupBy(x => x.Timestamp.Date)
                .Select(g => new
                {
                    date = g.Key,
                    timestamp = g.Max(x => x.Timestamp),
                    count = g.Count()
                })
                .OrderByDescending(x => x.timestamp)
                .ToList();

            return Ok(entries);
        }

        [HttpGet("daily-entry")]
        public async Task<IActionResult> GetDailyEntry(DateTime timestamp)
        {
            var date = timestamp.Date;

            var entries = await _context.ScadaHistoryPoints
                .Where(x => x.Timestamp.Date == date)
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

            var originalDate = request.OriginalTimestamp == default
                ? request.Timestamp.Date
                : request.OriginalTimestamp.Date;

            var existing = await _context.ScadaHistoryPoints
                .Where(x => x.Timestamp.Date == originalDate)
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

            ApplyMeterTimestampSpacing(newEntries, request.Timestamp);

            _context.ScadaHistoryPoints.AddRange(newEntries);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                updated = true,
                saved = newEntries.Count
            });
        }

        [HttpPost("daily-entry/delete")]
        public async Task<IActionResult> DeleteDailyEntries([FromBody] DeleteDailyEntriesRequest request)
        {
            if (request == null || request.Dates == null || request.Dates.Count == 0)
                return BadRequest("No dates were selected.");

            foreach (var date in request.Dates)
            {
                var targetDate = date.Date;

                var entries = await _context.ScadaHistoryPoints
                    .Where(x => x.Timestamp.Date == targetDate)
                    .ToListAsync();

                _context.ScadaHistoryPoints.RemoveRange(entries);
            }

            await _context.SaveChangesAsync();

            return Ok(new { deleted = true });
        }

        public class DeleteDailyEntriesRequest
        {
            public List<DateTime> Dates { get; set; } = new();
        }

        [HttpGet("monthly-site-report")]
        public async Task<IActionResult> GetMonthlySiteReport(string site, DateTime startDate, DateTime endDate)
        {
            endDate = endDate.Date.AddDays(1).AddTicks(-1);

            var points = await _context.ScadaHistoryPoints
                .Where(x => x.Timestamp <= endDate)
                .OrderBy(x => x.Timestamp)
                .ToListAsync();

            var reportSites = GetReportSites();

            var selectedSites = site == "All Sites"
                ? reportSites.Keys.ToList()
                : new List<string> { site };

            var rows = new List<object>();
            var summaryRows = new List<object>();

            foreach (var selectedSite in selectedSites)
            {
                if (!reportSites.ContainsKey(selectedSite))
                    continue;

                var config = reportSites[selectedSite];

                foreach (var meter in config.Meters)
                {
                    var meterPoints = points
                        .Where(x => x.Location == meter.Location && x.MetricType == meter.MetricType)
                        .OrderBy(x => x.Timestamp)
                        .ToList();

                    var dailyRows = new List<(DateTime Date, decimal Gallons)>();

                    foreach (var current in meterPoints.Where(x =>
                        x.Timestamp.Date >= startDate.Date &&
                        x.Timestamp.Date <= endDate.Date))
                    {
                        var previous = meterPoints
                            .Where(x => x.Timestamp < current.Timestamp)
                            .OrderByDescending(x => x.Timestamp)
                            .FirstOrDefault();

                        if (previous == null || current.Value == null || previous.Value == null)
                            continue;

                        var gallons = current.Value.Value - previous.Value.Value;

                        if (gallons < 0)
                            gallons = 0;

                        dailyRows.Add((current.Timestamp, gallons));

                        rows.Add(new
                        {
                            date = current.Timestamp,
                            site = selectedSite,
                            name = meter.DisplayName,
                            gallonsPumped = gallons,
                            chlorine = GetDailyValue(points, config.ChemistryLocations, "Chlorine", current.Timestamp.Date),
                            phosphate = GetDailyValue(points, config.ChemistryLocations, "Phosphate", current.Timestamp.Date),
                            ph = GetDailyValue(points, config.ChemistryLocations, "pH", current.Timestamp.Date),
                            temperature = selectedSite == "Filter Plant"
                                ? GetDailyValue(points, config.ChemistryLocations, "Temperature", current.Timestamp.Date)
                                : null
                        });
                    }

                    summaryRows.Add(new
                    {
                        site = selectedSite,
                        name = meter.DisplayName,
                        daysPumped = dailyRows.Count(x => x.Gallons > 0),
                        maxGallonsPumped = dailyRows.Any() ? dailyRows.Max(x => x.Gallons) : 0
                    });
                }
            }

            return Ok(new
            {
                rows,
                summary = summaryRows
            });
        }

        [HttpGet("meter-reading-total-report")]
        public async Task<IActionResult> GetMeterReadingTotalReport(DateTime startDate, DateTime endDate)
        {
            endDate = endDate.Date.AddDays(1).AddTicks(-1);

            var points = await _context.ScadaHistoryPoints
                .Where(x => x.Timestamp <= endDate)
                .OrderBy(x => x.Timestamp)
                .ToListAsync();

            decimal totalGallons = 0;

            foreach (var site in GetReportSites().Values)
            {
                foreach (var meter in site.Meters)
                {
                    var meterPoints = points
                        .Where(x => x.Location == meter.Location && x.MetricType == meter.MetricType)
                        .OrderBy(x => x.Timestamp)
                        .ToList();

                    foreach (var current in meterPoints.Where(x =>
                        x.Timestamp.Date >= startDate.Date &&
                        x.Timestamp.Date <= endDate.Date))
                    {
                        var previous = meterPoints
                            .Where(x => x.Timestamp < current.Timestamp)
                            .OrderByDescending(x => x.Timestamp)
                            .FirstOrDefault();

                        if (previous == null || current.Value == null || previous.Value == null)
                            continue;

                        var gallons = current.Value.Value - previous.Value.Value;

                        if (gallons > 0)
                            totalGallons += gallons;
                    }
                }
            }

            return Ok(new
            {
                startDate,
                endDate,
                totalGallonsPumped = totalGallons
            });
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

        [HttpGet("pump-station-report")]
        public async Task<IActionResult> GetPumpStationReport(string station, DateTime startDate, DateTime endDate)
        {
            endDate = endDate.Date.AddDays(1).AddTicks(-1);

            var pumpNames = station switch
            {
                "Beaver Creek" => new[] { "Beaver Creek Pump 1", "Beaver Creek Pump 2", "Beaver Creek Generator" },
                "Greenfield" => new[] { "Greenfield Pump 1", "Greenfield Pump 2", "Greenfield Generator" },
                "Helen Blevins" => new[] { "Helen Blevins Pump 1", "Helen Blevins Pump 2" },
                "Dogget" => new[] { "Dogget Pump 1", "Dogget Pump 2" },
                _ => Array.Empty<string>()
            };

            if (!pumpNames.Any())
                return BadRequest("Invalid pump station.");

            var allPoints = await _context.ScadaHistoryPoints
                .Where(x => pumpNames.Contains(x.Location) && x.Timestamp <= endDate)
                .OrderBy(x => x.Timestamp)
                .ToListAsync();

            var rows = new List<object>();

            foreach (var pump in pumpNames)
            {
                var pumpPoints = allPoints
                    .Where(x => x.Location == pump)
                    .OrderBy(x => x.Timestamp)
                    .ToList();

                var reportPoints = pumpPoints
                    .Where(x => x.Timestamp.Date >= startDate.Date && x.Timestamp.Date <= endDate.Date)
                    .ToList();

                foreach (var current in reportPoints)
                {
                    var previous = pumpPoints
                        .Where(x => x.Timestamp < current.Timestamp)
                        .OrderByDescending(x => x.Timestamp)
                        .FirstOrDefault();

                    decimal hoursRun = 0;

                    if (current.Value.HasValue && previous?.Value != null)
                    {
                        hoursRun = current.Value.Value - previous.Value.Value;

                        if (hoursRun < 0)
                            hoursRun = 0;
                    }

                    rows.Add(new
                    {
                        date = current.Timestamp,
                        station,
                        pump,
                        reading = current.Value ?? 0,
                        hoursRun
                    });
                }
            }

            return Ok(rows.OrderBy(x => x.GetType().GetProperty("date")!.GetValue(x)).ToList());
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

        private static void ApplyMeterTimestampSpacing(List<ScadaHistoryPoint> readings, DateTime submittedTime)
        {
            var roundedStartTime = RoundToNearestFiveMinutes(submittedTime);

            var siteOrder = new List<string>
    {
        "Reeves Well Site",
        "Park Well Site",
        "Woods",
        "Catawissa",
        "New",
        "Oakwood",
        "Ray",
        "Filter Plant",
        "Pump Stations"
    };

            string GetSiteGroup(ScadaHistoryPoint reading)
            {
                return reading.Location switch
                {
                    "Reeves Well" or "Reeves Well A" or "Reeves Well Site" => "Reeves Well Site",

                    "Park Well" or "Park Well A" or "Park Well B" or "Park Well Site" => "Park Well Site",

                    "Filter Plant" or "Mt. Jefferson" or "Filter 1" or "Filter 2" => "Filter Plant",

                    "Beaver Creek Pump 1" or "Beaver Creek Pump 2" or "Beaver Creek Generator"
                        or "Greenfield Pump 1" or "Greenfield Pump 2" or "Greenfield Generator"
                        or "Helen Blevins Pump 1" or "Helen Blevins Pump 2"
                        or "Dogget Pump 1" or "Dogget Pump 2" => "Pump Stations",

                    _ => reading.Location
                };
            }

            var groups = readings
                .GroupBy(GetSiteGroup)
                .OrderBy(g =>
                {
                    var index = siteOrder.IndexOf(g.Key);
                    return index == -1 ? 999 : index;
                })
                .ToList();

            for (int i = 0; i < groups.Count; i++)
            {
                var groupTime = roundedStartTime.AddMinutes(i * 10);

                foreach (var reading in groups[i])
                {
                    reading.Timestamp = groupTime;
                }
            }
        }

        private static DateTime RoundToNearestFiveMinutes(DateTime dateTime)
        {
            var ticks = TimeSpan.FromMinutes(5).Ticks;
            return new DateTime(((dateTime.Ticks + ticks / 2) / ticks) * ticks);
        }

        private static decimal? GetDailyValue(
            List<ScadaHistoryPoint> points,
            List<string> locations,
            string metricType,
            DateTime date)
        {
            return points
                .Where(x =>
                    locations.Contains(x.Location) &&
                    x.MetricType == metricType &&
                    x.Timestamp.Date == date.Date)
                .OrderByDescending(x => x.Timestamp)
                .Select(x => x.Value)
                .FirstOrDefault();
        }

        private static Dictionary<string, MonthlyReportSiteConfig> GetReportSites()
        {
            return new Dictionary<string, MonthlyReportSiteConfig>
            {
                ["Reeves Well Site"] = new MonthlyReportSiteConfig
                {
                    ChemistryLocations = new List<string> { "Reeves Well Site", "Reeves" },
                    Meters = new List<MonthlyReportMeterConfig>
                    {
                        new MonthlyReportMeterConfig { DisplayName = "Reeves Well", Location = "Reeves Well", MetricType = "Meter Reading" },
                        new MonthlyReportMeterConfig { DisplayName = "Reeves Well A", Location = "Reeves Well A", MetricType = "Meter Reading" }
                    }
                },

                ["Park Well Site"] = new MonthlyReportSiteConfig
                {
                    ChemistryLocations = new List<string> { "Park Well Site", "Park" },
                    Meters = new List<MonthlyReportMeterConfig>
                    {
                        new MonthlyReportMeterConfig { DisplayName = "Park Well", Location = "Park Well", MetricType = "Meter Reading" },
                        new MonthlyReportMeterConfig { DisplayName = "Park Well A", Location = "Park Well A", MetricType = "Meter Reading" },
                        new MonthlyReportMeterConfig { DisplayName = "Park Well B", Location = "Park Well B", MetricType = "Meter Reading" }
                    }
                },

                ["Woods"] = new MonthlyReportSiteConfig
                {
                    ChemistryLocations = new List<string> { "Woods" },
                    Meters = new List<MonthlyReportMeterConfig>
                    {
                        new MonthlyReportMeterConfig { DisplayName = "Woods", Location = "Woods", MetricType = "Meter Reading" }
                    }
                },

                ["Catawissa"] = new MonthlyReportSiteConfig
                {
                    ChemistryLocations = new List<string> { "Catawissa" },
                    Meters = new List<MonthlyReportMeterConfig>
                    {
                        new MonthlyReportMeterConfig { DisplayName = "Catawissa", Location = "Catawissa", MetricType = "Meter Reading" }
                    }
                },

                ["New"] = new MonthlyReportSiteConfig
                {
                    ChemistryLocations = new List<string> { "New" },
                    Meters = new List<MonthlyReportMeterConfig>
                    {
                        new MonthlyReportMeterConfig { DisplayName = "New", Location = "New", MetricType = "Meter Reading" }
                    }
                },

                ["Oakwood"] = new MonthlyReportSiteConfig
                {
                    ChemistryLocations = new List<string> { "Oakwood" },
                    Meters = new List<MonthlyReportMeterConfig>
                    {
                        new MonthlyReportMeterConfig { DisplayName = "Oakwood", Location = "Oakwood", MetricType = "Meter Reading" }
                    }
                },

                ["Ray"] = new MonthlyReportSiteConfig
                {
                    ChemistryLocations = new List<string> { "Ray" },
                    Meters = new List<MonthlyReportMeterConfig>
                    {
                        new MonthlyReportMeterConfig { DisplayName = "Ray", Location = "Ray", MetricType = "Meter Reading" }
                    }
                },

                ["Filter Plant"] = new MonthlyReportSiteConfig
                {
                    ChemistryLocations = new List<string> { "Filter Plant" },
                    Meters = new List<MonthlyReportMeterConfig>
                    {
                        new MonthlyReportMeterConfig { DisplayName = "Filter Plant", Location = "Filter Plant", MetricType = "Meter Reading" },
                        new MonthlyReportMeterConfig { DisplayName = "Filter 1", Location = "Filter 1", MetricType = "Total Filtration Flow Yesterday" },
                        new MonthlyReportMeterConfig { DisplayName = "Filter 2", Location = "Filter 2", MetricType = "Total Filtration Flow Yesterday" }
                    }
                }
            };
        }

        private static string GetLocation(string sourceColumn)
        {
            return sourceColumn switch
            {
                "Reeves" => "Reeves Well",
                "Reeves A" => "Reeves Well A",
                "Reeves Well" => "Reeves Well",
                "Reeves Well A" => "Reeves Well A",

                "Park" => "Park Well",
                "Park A" => "Park Well A",
                "Park B" => "Park Well B",
                "Park Well" => "Park Well",
                "Park Well A" => "Park Well A",
                "Park Well B" => "Park Well B",

                "Woods" => "Woods",
                "Catawissa" => "Catawissa",
                "New" => "New",
                "Oakwood" => "Oakwood",
                "Ray" => "Ray",
                "Filter Plant" => "Filter Plant",
                "Mt. Jefferson" => "Mt. Jefferson",

                "Cl-R" or "PO4-R" or "pH-R" => "Reeves Well Site",
                "Cl-P" or "PO4-P" or "pH-P" => "Park Well Site",
                "Cl-W" or "PO4-W" or "pH-W" => "Woods",
                "Cl-C" or "PO4-C" or "pH-C" => "Catawissa",
                "Cl-N" or "PO4-N" or "pH-N" => "New",
                "Cl-O" or "PO4-O" or "pH-O" => "Oakwood",
                "Cl-Ray" or "PO4-Ray" or "pH-Ray" => "Ray",

                _ when sourceColumn.Contains("Filter 1", StringComparison.OrdinalIgnoreCase) => "Filter 1",
                _ when sourceColumn.Contains("Filter 2", StringComparison.OrdinalIgnoreCase) => "Filter 2",
                _ when sourceColumn.Contains("Filter", StringComparison.OrdinalIgnoreCase) => "Filter Plant",
                _ when sourceColumn.EndsWith("-F", StringComparison.OrdinalIgnoreCase) => "Filter Plant",

                _ => sourceColumn
            };
        }

        private static string GetMetricType(string sourceColumn)
        {
            if (sourceColumn is "Reeves" or "Reeves A" or "Reeves Well" or "Reeves Well A"
                or "Park" or "Park A" or "Park B" or "Park Well" or "Park Well A" or "Park Well B"
                or "Woods" or "Catawissa" or "New" or "Oakwood" or "Ray"
                or "Filter Plant" or "Mt. Jefferson")
                return "Meter Reading";

            if (sourceColumn.Contains("Total Filtration Flow Yesterday", StringComparison.OrdinalIgnoreCase))
                return "Total Filtration Flow Yesterday";

            if (sourceColumn.StartsWith("Cl", StringComparison.OrdinalIgnoreCase))
                return "Chlorine";

            if (sourceColumn.StartsWith("PO4", StringComparison.OrdinalIgnoreCase))
                return "Phosphate";

            if (sourceColumn.StartsWith("pH", StringComparison.OrdinalIgnoreCase))
                return "pH";

            if (sourceColumn.Contains("Temp", StringComparison.OrdinalIgnoreCase))
                return "Temperature";

            if (sourceColumn.Contains("Feed Flow", StringComparison.OrdinalIgnoreCase))
                return "Feed Flow";

            if (sourceColumn.Contains("Filtrate Flow", StringComparison.OrdinalIgnoreCase))
                return "Filtrate Flow";

            if (sourceColumn.Contains("Flow", StringComparison.OrdinalIgnoreCase))
                return "Flow";

            if (sourceColumn.Contains("Pressure", StringComparison.OrdinalIgnoreCase))
                return sourceColumn;

            if (sourceColumn.Contains("TMP", StringComparison.OrdinalIgnoreCase))
                return "TMP";

            if (sourceColumn.Contains("Turb", StringComparison.OrdinalIgnoreCase))
                return "Turbidity";

            return sourceColumn;
        }

        public class DailyEntryUpdateRequest
        {
            public DateTime OriginalTimestamp { get; set; }
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

        public class MonthlyReportSiteConfig
        {
            public List<string> ChemistryLocations { get; set; } = new();
            public List<MonthlyReportMeterConfig> Meters { get; set; } = new();
        }

        public class MonthlyReportMeterConfig
        {
            public string DisplayName { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public string MetricType { get; set; } = string.Empty;
        }
    }
}