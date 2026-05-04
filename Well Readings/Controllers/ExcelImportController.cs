using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Well_Readings.Data;
using Well_Readings.Models;

namespace Well_Readings.Controllers
{
    [ApiController]
    [Route("api/excel-import")]
    public class ExcelImportController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExcelImportController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("scada")]
        public async Task<IActionResult> ImportScadaExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file was uploaded.");

            var imported = 0;
            var updated = 0;
            var skipped = 0;

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);

            var worksheet =
                workbook.Worksheets.FirstOrDefault(x => x.Name.Equals("Form Responses 1", StringComparison.OrdinalIgnoreCase))
                ?? workbook.Worksheets.FirstOrDefault(x => x.Name.Equals("Data", StringComparison.OrdinalIgnoreCase))
                ?? workbook.Worksheets.First();

            var headerRow = worksheet.Row(1);
            var lastColumn = worksheet.LastColumnUsed().ColumnNumber();
            var lastRow = worksheet.LastRowUsed().RowNumber();

            var headers = new Dictionary<int, string>();

            for (var col = 1; col <= lastColumn; col++)
            {
                var header = headerRow.Cell(col).GetString().Trim();

                if (!string.IsNullOrWhiteSpace(header))
                    headers[col] = header;
            }

            for (var row = 2; row <= lastRow; row++)
            {
                var timestampCell = worksheet.Row(row).Cell(1);

                if (!TryGetDateTime(timestampCell, out var timestamp))
                {
                    skipped++;
                    continue;
                }

                foreach (var headerPair in headers)
                {
                    var col = headerPair.Key;
                    var columnName = headerPair.Value;

                    if (columnName.Equals("Timestamp", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var mapping = GetMapping(columnName);

                    if (mapping == null)
                        continue;

                    var cell = worksheet.Row(row).Cell(col);

                    if (!TryGetDecimal(cell, out var value))
                        continue;

                    var existing = await _context.ScadaHistoryPoints.FirstOrDefaultAsync(x =>
                        x.Timestamp == timestamp &&
                        x.Location == mapping.Location &&
                        x.MetricType == mapping.MetricType &&
                        x.SourceColumn == mapping.SourceColumn);

                    if (existing == null)
                    {
                        _context.ScadaHistoryPoints.Add(new ScadaHistoryPoint
                        {
                            Id = Guid.NewGuid(),
                            Timestamp = timestamp,
                            Location = mapping.Location,
                            MetricType = mapping.MetricType,
                            SourceColumn = mapping.SourceColumn,
                            Value = value
                        });

                        imported++;
                    }
                    else
                    {
                        existing.Value = value;
                        updated++;
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                imported,
                updated,
                skipped,
                sheet = worksheet.Name
            });
        }

        private static ImportMapping? GetMapping(string column)
        {
            var c = column.Trim();

            return c switch
            {
                // Reeves
                "Reeves" => new("Reeves Well", "Meter Reading", "Reeves Well"),
                "Reeves A" => new("Reeves Well A", "Meter Reading", "Reeves Well A"),
                "Cl-R" or "C-R" => new("Reeves Well Site", "Chlorine", "Cl-R"),
                "PO4-R" or "P-R" => new("Reeves Well Site", "Phosphate", "PO4-R"),
                "pH-R" => new("Reeves Well Site", "pH", "pH-R"),

                // Park
                "Park" => new("Park Well", "Meter Reading", "Park Well"),
                "Park A" => new("Park Well A", "Meter Reading", "Park Well A"),
                "Park B" => new("Park Well B", "Meter Reading", "Park Well B"),
                "Cl-P" or "C-P" => new("Park Well Site", "Chlorine", "Cl-P"),
                "PO4-P" or "P-P" => new("Park Well Site", "Phosphate", "PO4-P"),
                "pH-P" => new("Park Well Site", "pH", "pH-P"),

                // Woods
                "Woods" => new("Woods", "Meter Reading", "Woods"),
                "Cl-W" or "C-W" => new("Woods", "Chlorine", "Cl-W"),
                "PO4-W" or "P-W" => new("Woods", "Phosphate", "PO4-W"),
                "pH-W" => new("Woods", "pH", "pH-W"),

                // Catawissa
                "Catawissa" or "Catawissa " => new("Catawissa", "Meter Reading", "Catawissa"),
                "Cl-C" or "C-C" => new("Catawissa", "Chlorine", "Cl-C"),
                "PO4-C" or "P-C" => new("Catawissa", "Phosphate", "PO4-C"),
                "pH-C" => new("Catawissa", "pH", "pH-C"),

                // New
                "New" => new("New", "Meter Reading", "New"),
                "Cl-N" or "C-N" => new("New", "Chlorine", "Cl-N"),
                "PO4-N" or "P-N" => new("New", "Phosphate", "PO4-N"),
                "pH-N" => new("New", "pH", "pH-N"),

                // Oakwood
                "Oakwood" => new("Oakwood", "Meter Reading", "Oakwood"),
                "Cl-O" or "C-O" => new("Oakwood", "Chlorine", "Cl-O"),
                "PO4-O" or "P-O" => new("Oakwood", "Phosphate", "PO4-O"),
                "pH-O" => new("Oakwood", "pH", "pH-O"),

                // Ray
                "Ray" => new("Ray", "Meter Reading", "Ray"),
                "Cl-Ray" or "C-Ray" => new("Ray", "Chlorine", "Cl-Ray"),
                "PO4-Ray" or "P-Ray" => new("Ray", "Phosphate", "PO4-Ray"),
                "pH-Ray" => new("Ray", "pH", "pH-Ray"),

                // Filter Plant
                "Filter Plant" => new("Filter Plant", "Meter Reading", "Filter Plant"),
                "Mt. Jefferson" => new("Mt. Jefferson", "Meter Reading", "Mt. Jefferson"),
                "Cl-F" or "C-F" => new("Filter Plant", "Chlorine", "Cl-F"),
                "PO4-F" or "P-F" => new("Filter Plant", "Phosphate", "PO4-F"),
                "pH-F" => new("Filter Plant", "pH", "pH-F"),
                "Temp-F" or "Temp-F.1" => new("Filter Plant", "Temperature", "Temp-F"),

                // Filter 1
                "Feed Pressure" => new("Filter 1", "Feed Pressure", "Filter 1 Feed Pressure"),
                "Feed Flow" => new("Filter 1", "Feed Flow", "Filter 1 Feed Flow"),
                "Filtrate Pressure" => new("Filter 1", "Filtrate Pressure", "Filter 1 Filtrate Pressure"),
                "Filtrate Flow" => new("Filter 1", "Filtrate Flow", "Filter 1 Filtrate Flow"),
                "TMP" => new("Filter 1", "TMP", "Filter 1 TMP"),
                "Total Filter Run Time" or "Total Run Time" => new("Filter 1", "Total Filter Run Time", "Filter 1 Total Filter Run Time"),
                "Total Filtration Flow Yesterday" or "Total Filtration Flow Yesterday " or "Total Flow Yesterday" => new("Filter 1", "Total Filtration Flow Yesterday", "Filter 1 Total Filtration Flow Yesterday"),
                "Pressure Decay" or "Pressure Decay " => new("Filter 1", "Pressure Decay", "Filter 1 Pressure Decay"),

                // Filter 2
                "Feed Pressure.1" => new("Filter 2", "Feed Pressure", "Filter 2 Feed Pressure"),
                "Feed Flow.1" => new("Filter 2", "Feed Flow", "Filter 2 Feed Flow"),
                "Filtrate Pressure.1" => new("Filter 2", "Filtrate Pressure", "Filter 2 Filtrate Pressure"),
                "Filtrate Flow.1" => new("Filter 2", "Filtrate Flow", "Filter 2 Filtrate Flow"),
                "TMP.1" => new("Filter 2", "TMP", "Filter 2 TMP"),
                "Total Filter Run Time.1" => new("Filter 2", "Total Filter Run Time", "Filter 2 Total Filter Run Time"),
                "Total Filtration Flow Yesterday .1" => new("Filter 2", "Total Filtration Flow Yesterday", "Filter 2 Total Filtration Flow Yesterday"),
                "Pressure Decay .1" => new("Filter 2", "Pressure Decay", "Filter 2 Pressure Decay"),

                // Pump Stations
                "Helen Blevins Pump 1" => new("Helen Blevins Pump 1", "Pump Status", "Helen Blevins Pump 1"),
                "Helen Blevins Pump 2" => new("Helen Blevins Pump 2", "Pump Status", "Helen Blevins Pump 2"),

                "Beaver Creek Pump 1" => new("Beaver Creek Pump 1", "Pump Status", "Beaver Creek Pump 1"),
                "Beaver Creek Pump 2" => new("Beaver Creek Pump 2", "Pump Status", "Beaver Creek Pump 2"),
                "Beaver Creek Generator" => new("Beaver Creek Generator", "Generator Status", "Beaver Creek Generator"),

                "Greenfield Pump 1" => new("Greenfield Pump 1", "Pump Status", "Greenfield Pump 1"),
                "Greenfield Pump 2" => new("Greenfield Pump 2", "Pump Status", "Greenfield Pump 2"),
                "Greenfield Generator" or "Greenfield  Generator" => new("Greenfield Generator", "Generator Status", "Greenfield Generator"),

                "Dogget Pump 1" => new("Dogget Pump 1", "Pump Status", "Dogget Pump 1"),
                "Dogget Pump 2" => new("Dogget Pump 2", "Pump Status", "Dogget Pump 2"),

                _ => null
            };
        }

        private static bool TryGetDateTime(IXLCell cell, out DateTime value)
        {
            value = default;

            if (cell.IsEmpty())
                return false;

            if (cell.DataType == XLDataType.DateTime)
            {
                value = cell.GetDateTime();
                return true;
            }

            if (DateTime.TryParse(cell.GetString(), out value))
                return true;

            return false;
        }

        private static bool TryGetDecimal(IXLCell cell, out decimal value)
        {
            value = 0;

            if (cell.IsEmpty())
                return false;

            if (cell.DataType == XLDataType.Boolean)
                return false;

            if (cell.TryGetValue<decimal>(out value))
                return true;

            var text = cell.GetString().Trim();

            if (string.IsNullOrWhiteSpace(text))
                return false;

            return decimal.TryParse(text, out value);
        }

        private record ImportMapping(string Location, string MetricType, string SourceColumn);
    }
}