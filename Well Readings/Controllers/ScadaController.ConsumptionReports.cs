using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Well_Readings.Models;

namespace Well_Readings.Controllers
{
    public partial class ScadaController
    {
        [HttpGet("consumption-report")]
        public async Task<IActionResult> GetConsumptionReport(
            DateTime lastReadDate,
            DateTime currentReadDate,
            decimal consumedGallons,
            string? notes = null)
        {
            if (currentReadDate.Date < lastReadDate.Date)
            {
                return BadRequest("Current meter read date must be on or after last meter read date.");
            }

            var report = await BuildConsumptionReportAsync(
                lastReadDate,
                currentReadDate,
                consumedGallons,
                notes,
                null);

            return Ok(report);
        }

        [HttpDelete("consumption-report/{id:guid}")]
        public async Task<IActionResult> DeleteConsumptionReport(Guid id)
        {
            var existing = await _context.ConsumptionReports
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existing == null)
            {
                return NotFound("Consumption report was not found.");
            }

            _context.ConsumptionReports.Remove(existing);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                deleted = true,
                id
            });
        }

        [HttpPost("consumption-report/save")]
        public async Task<IActionResult> SaveConsumptionReport([FromBody] SaveConsumptionReportRequest request)
        {
            if (request == null)
            {
                return BadRequest("No report data was submitted.");
            }

            if (request.CurrentReadDate.Date < request.LastReadDate.Date)
            {
                return BadRequest("Current meter read date must be on or after last meter read date.");
            }

            var reportDto = await BuildConsumptionReportAsync(
                request.LastReadDate,
                request.CurrentReadDate,
                request.WaterConsumed,
                request.Notes,
                request.WaterPumpedOverride);

            if (!string.IsNullOrWhiteSpace(request.PeriodLabel))
            {
                reportDto.PeriodLabel = request.PeriodLabel;
            }

            var existing = await _context.ConsumptionReports
                .FirstOrDefaultAsync(x =>
                    x.LastReadDate.Date == reportDto.LastReadDate.Date &&
                    x.CurrentReadDate.Date == reportDto.CurrentReadDate.Date);

            if (existing == null)
            {
                existing = new ConsumptionReport
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.ConsumptionReports.Add(existing);
            }

            existing.PeriodLabel = reportDto.PeriodLabel;
            existing.LastReadDate = reportDto.LastReadDate;
            existing.CurrentReadDate = reportDto.CurrentReadDate;
            existing.BillingDays = reportDto.BillingDays;
            existing.WaterPumped = reportDto.WaterPumped;
            existing.WaterConsumed = reportDto.WaterConsumed;
            existing.WaterLoss = reportDto.WaterLoss;
            existing.LossPercent = reportDto.LossPercent;
            existing.PumpedAveragePerDay = reportDto.PumpedAveragePerDay;
            existing.ConsumedAveragePerDay = reportDto.ConsumedAveragePerDay;
            existing.Notes = reportDto.Notes ?? "";
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                saved = true,
                id = existing.Id,
                report = reportDto
            });
        }

        [HttpGet("consumption-report-history")]
        public async Task<IActionResult> GetConsumptionReportHistory(
            int? year = null,
            int? startMonth = null,
            int? endMonth = null)
        {
            var query = _context.ConsumptionReports.AsQueryable();

            if (year.HasValue)
            {
                query = query.Where(x => x.CurrentReadDate.Year == year.Value);
            }

            if (startMonth.HasValue)
            {
                query = query.Where(x => x.CurrentReadDate.Month >= startMonth.Value);
            }

            if (endMonth.HasValue)
            {
                query = query.Where(x => x.CurrentReadDate.Month <= endMonth.Value);
            }

            var rows = await query
                .OrderBy(x => x.CurrentReadDate)
                .Select(x => new
                {
                    x.Id,
                    x.PeriodLabel,
                    x.LastReadDate,
                    x.CurrentReadDate,
                    x.BillingDays,
                    x.WaterPumped,
                    x.WaterConsumed,
                    x.WaterLoss,
                    x.LossPercent,
                    x.PumpedAveragePerDay,
                    x.ConsumedAveragePerDay,
                    x.Notes
                })
                .ToListAsync();

            return Ok(rows);
        }

        [HttpPost("consumption-report-import")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportConsumptionReports(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No spreadsheet was uploaded.");
            }

            var imported = 0;
            var skipped = 0;
            var errors = new List<string>();

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);

            var worksheet = workbook.Worksheets.FirstOrDefault(x =>
                x.Name.Equals("Consumption Import", StringComparison.OrdinalIgnoreCase))
                ?? workbook.Worksheets.First();

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            if (lastRow < 2)
            {
                return BadRequest("The spreadsheet does not contain any import rows.");
            }

            for (var row = 2; row <= lastRow; row++)
            {
                try
                {
                    var periodLabel = worksheet.Cell(row, 1).GetString().Trim();

                    var lastReadDateCell = worksheet.Cell(row, 2);
                    var currentReadDateCell = worksheet.Cell(row, 3);
                    var consumerGallonsCell = worksheet.Cell(row, 4);
                    var waterPumpedOverrideCell = worksheet.Cell(row, 5);
                    var notes = worksheet.Cell(row, 6).GetString().Trim();

                    if (!lastReadDateCell.TryGetValue<DateTime>(out var lastReadDate) ||
                        !currentReadDateCell.TryGetValue<DateTime>(out var currentReadDate))
                    {
                        skipped++;
                        errors.Add($"Row {row}: Missing or invalid dates.");
                        continue;
                    }

                    if (!consumerGallonsCell.TryGetValue<decimal>(out var consumerGallons))
                    {
                        skipped++;
                        errors.Add($"Row {row}: Missing or invalid consumer gallons.");
                        continue;
                    }

                    decimal? pumpedOverride = null;

                    if (!waterPumpedOverrideCell.IsEmpty() &&
                        waterPumpedOverrideCell.TryGetValue<decimal>(out var overrideValue))
                    {
                        pumpedOverride = overrideValue;
                    }

                    var reportDto = await BuildConsumptionReportAsync(
                        lastReadDate,
                        currentReadDate,
                        consumerGallons,
                        notes,
                        pumpedOverride);

                    if (!string.IsNullOrWhiteSpace(periodLabel))
                    {
                        reportDto.PeriodLabel = periodLabel;
                    }

                    var existing = await _context.ConsumptionReports
                        .FirstOrDefaultAsync(x =>
                            x.LastReadDate.Date == reportDto.LastReadDate.Date &&
                            x.CurrentReadDate.Date == reportDto.CurrentReadDate.Date);

                    if (existing == null)
                    {
                        existing = new ConsumptionReport
                        {
                            Id = Guid.NewGuid(),
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.ConsumptionReports.Add(existing);
                    }

                    existing.PeriodLabel = reportDto.PeriodLabel;
                    existing.LastReadDate = reportDto.LastReadDate;
                    existing.CurrentReadDate = reportDto.CurrentReadDate;
                    existing.BillingDays = reportDto.BillingDays;
                    existing.WaterPumped = reportDto.WaterPumped;
                    existing.WaterConsumed = reportDto.WaterConsumed;
                    existing.WaterLoss = reportDto.WaterLoss;
                    existing.LossPercent = reportDto.LossPercent;
                    existing.PumpedAveragePerDay = reportDto.PumpedAveragePerDay;
                    existing.ConsumedAveragePerDay = reportDto.ConsumedAveragePerDay;
                    existing.Notes = reportDto.Notes ?? "";
                    existing.UpdatedAt = DateTime.UtcNow;

                    imported++;
                }
                catch (Exception ex)
                {
                    skipped++;
                    errors.Add($"Row {row}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                imported,
                skipped,
                errors
            });
        }

        [HttpGet("consumption-report-export")]
        public async Task<IActionResult> ExportSingleConsumptionReport(
            DateTime lastReadDate,
            DateTime currentReadDate,
            decimal consumedGallons,
            string? notes = null)
        {
            if (currentReadDate.Date < lastReadDate.Date)
            {
                return BadRequest("Current meter read date must be on or after last meter read date.");
            }

            var report = await BuildConsumptionReportAsync(
                lastReadDate,
                currentReadDate,
                consumedGallons,
                notes,
                null);

            return ExportConsumptionWorkbook(
                new List<ConsumptionReportDto> { report },
                $"Consumption_Report_{report.LastReadDate:yyyyMMdd}_to_{report.CurrentReadDate:yyyyMMdd}.xlsx");
        }

        [HttpGet("consumption-report-history-export")]
        public async Task<IActionResult> ExportConsumptionReportHistory(
            int? year = null,
            int? startMonth = null,
            int? endMonth = null)
        {
            var query = _context.ConsumptionReports.AsQueryable();

            if (year.HasValue)
            {
                query = query.Where(x => x.CurrentReadDate.Year == year.Value);
            }

            if (startMonth.HasValue)
            {
                query = query.Where(x => x.CurrentReadDate.Month >= startMonth.Value);
            }

            if (endMonth.HasValue)
            {
                query = query.Where(x => x.CurrentReadDate.Month <= endMonth.Value);
            }

            var reports = await query
                .OrderBy(x => x.CurrentReadDate)
                .Select(x => new ConsumptionReportDto
                {
                    PeriodLabel = x.PeriodLabel,
                    LastReadDate = x.LastReadDate,
                    CurrentReadDate = x.CurrentReadDate,
                    BillingDays = x.BillingDays,
                    WaterPumped = x.WaterPumped,
                    WaterConsumed = x.WaterConsumed,
                    WaterLoss = x.WaterLoss,
                    LossPercent = x.LossPercent,
                    PumpedAveragePerDay = x.PumpedAveragePerDay,
                    ConsumedAveragePerDay = x.ConsumedAveragePerDay,
                    Notes = x.Notes
                })
                .ToListAsync();

            var fileName = year.HasValue
                ? $"Consumption_History_{year}.xlsx"
                : "Consumption_History.xlsx";

            return ExportConsumptionWorkbook(reports, fileName);
        }

        private async Task<ConsumptionReportDto> BuildConsumptionReportAsync(
            DateTime lastReadDate,
            DateTime currentReadDate,
            decimal consumedGallons,
            string? notes,
            decimal? waterPumpedOverride)
        {
            var billingDays = (currentReadDate.Date - lastReadDate.Date).Days;

            if (billingDays <= 0)
            {
                billingDays = 1;
            }

            var waterPumped = waterPumpedOverride
                ?? await GetPumpedTotalForInclusivePeriodAsync(lastReadDate, currentReadDate);

            var waterConsumed = consumedGallons;
            var waterLoss = waterPumped - waterConsumed;

            var lossPercent = waterPumped > 0
                ? waterLoss / waterPumped
                : 0;

            var periodLabel =
                $"{lastReadDate:MM/dd/yyyy} - {currentReadDate:MM/dd/yyyy}";

            return new ConsumptionReportDto
            {
                PeriodLabel = periodLabel,
                LastReadDate = lastReadDate.Date,
                CurrentReadDate = currentReadDate.Date,
                BillingDays = billingDays,
                WaterPumped = waterPumped,
                WaterConsumed = waterConsumed,
                WaterLoss = waterLoss,
                LossPercent = lossPercent,
                PumpedAveragePerDay = waterPumped / billingDays,
                ConsumedAveragePerDay = waterConsumed / billingDays,
                Notes = notes ?? ""
            };
        }

        private async Task<decimal> GetPumpedTotalForInclusivePeriodAsync(
            DateTime startDate,
            DateTime endDate)
        {
            var validLocations = await _context.ValidMeterLocations
                .Select(x => x.Location)
                .ToListAsync();

            decimal total = 0;

            foreach (var location in validLocations)
            {
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    var gallons = await GetDeltaForDate(location, "Meter Reading", date);
                    total += gallons;
                }
            }

            return total;
        }

        //private async Task<decimal> GetMeterUsageForPeriodAsync(
        //    string location,
        //    DateTime startBoundary,
        //    DateTime endBoundary)
        //{
        //    var startReading = await _context.ScadaHistoryPoints
        //        .Where(x =>
        //            x.Location == location &&
        //            x.MetricType == "Meter Reading" &&
        //            x.Timestamp < startBoundary &&
        //            x.Value != null)
        //        .OrderByDescending(x => x.Timestamp)
        //        .FirstOrDefaultAsync();

        //    var endReading = await _context.ScadaHistoryPoints
        //        .Where(x =>
        //            x.Location == location &&
        //            x.MetricType == "Meter Reading" &&
        //            x.Timestamp < endBoundary &&
        //            x.Value != null)
        //        .OrderByDescending(x => x.Timestamp)
        //        .FirstOrDefaultAsync();

        //    if (startReading?.Value == null || endReading?.Value == null)
        //    {
        //        return 0;
        //    }

        //    var simpleDelta = endReading.Value.Value - startReading.Value.Value;

        //    if (simpleDelta >= 0)
        //    {
        //        return simpleDelta;
        //    }

        //    var replacements = await _context.MeterReplacements
        //        .Where(x =>
        //            x.Location == location &&
        //            x.ReplacementDate >= startReading.Timestamp.Date &&
        //            x.ReplacementDate <= endReading.Timestamp.Date)
        //        .OrderBy(x => x.ReplacementDate)
        //        .ToListAsync();

        //    if (!replacements.Any())
        //    {
        //        return 0;
        //    }

        //    decimal total = 0;
        //    var previousReading = startReading.Value.Value;

        //    foreach (var replacement in replacements)
        //    {
        //        var oldMeterUsage = replacement.OldMeterFinalReading - previousReading;

        //        if (oldMeterUsage > 0)
        //        {
        //            total += oldMeterUsage;
        //        }

        //        previousReading = replacement.NewMeterStartingReading;
        //    }

        //    var finalNewMeterUsage = endReading.Value.Value - previousReading;

        //    if (finalNewMeterUsage > 0)
        //    {
        //        total += finalNewMeterUsage;
        //    }

        //    return total;
        //}

        private IActionResult ExportConsumptionWorkbook(
    List<ConsumptionReportDto> reports,
    string fileName)
        {
            using var workbook = new XLWorkbook();

            var summary = workbook.Worksheets.Add("Summary");
            var charts = workbook.Worksheets.Add("Charts");
            var data = workbook.Worksheets.Add("Saved Reports");

            // =========================
            // Summary Sheet
            // =========================

            summary.Cell("A1").Value = "Consumption Report Summary";
            summary.Range("A1:F1").Merge();
            summary.Cell("A1").Style.Font.Bold = true;
            summary.Cell("A1").Style.Font.FontSize = 16;
            summary.Cell("A1").Style.Fill.BackgroundColor = XLColor.FromHtml("#1f2937");
            summary.Cell("A1").Style.Font.FontColor = XLColor.White;
            summary.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var totalPumped = reports.Sum(x => x.WaterPumped);
            var totalConsumed = reports.Sum(x => x.WaterConsumed);
            var totalLoss = reports.Sum(x => x.WaterLoss);
            var totalLossPercent = totalPumped > 0 ? totalLoss / totalPumped : 0;

            summary.Cell("A3").Value = "Reports";
            summary.Cell("B3").Value = reports.Count;

            summary.Cell("A4").Value = "Total Water Pumped";
            summary.Cell("B4").Value = totalPumped;

            summary.Cell("A5").Value = "Total Water Consumed";
            summary.Cell("B5").Value = totalConsumed;

            summary.Cell("A6").Value = "Total Unaccounted For";
            summary.Cell("B6").Value = totalLoss;

            summary.Cell("A7").Value = "Total Loss %";
            summary.Cell("B7").Value = totalLossPercent;

            summary.Range("A3:A7").Style.Font.Bold = true;
            summary.Range("A3:B7").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            summary.Range("A3:B7").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            summary.Range("B4:B6").Style.NumberFormat.Format = "#,##0";
            summary.Cell("B7").Style.NumberFormat.Format = "0.00%";

            // =========================
            // Charts Sheet
            // =========================

            charts.Cell("A1").Value = "Consumption Charts";
            charts.Range("A1:J1").Merge();
            charts.Cell("A1").Style.Font.Bold = true;
            charts.Cell("A1").Style.Font.FontSize = 16;
            charts.Cell("A1").Style.Fill.BackgroundColor = XLColor.FromHtml("#1f2937");
            charts.Cell("A1").Style.Font.FontColor = XLColor.White;
            charts.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            charts.Cell("A3").Value = "Period";
            charts.Cell("B3").Value = "Water Pumped";
            charts.Cell("C3").Value = "Water Consumed";
            charts.Cell("D3").Value = "Unaccounted For";
            charts.Cell("E3").Value = "Loss %";
            charts.Cell("F3").Value = "Pumped Bar";
            charts.Cell("G3").Value = "Consumed Bar";
            charts.Cell("H3").Value = "Loss Bar";
            charts.Cell("I3").Value = "Loss % Status";

            charts.Range("A3:I3").Style.Font.Bold = true;
            charts.Range("A3:I3").Style.Fill.BackgroundColor = XLColor.FromHtml("#dbeafe");
            charts.Range("A3:I3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            charts.Range("A3:I3").Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            var maxGallons = reports.Any()
                ? Math.Max(
                    reports.Max(x => x.WaterPumped),
                    Math.Max(reports.Max(x => x.WaterConsumed), reports.Max(x => x.WaterLoss)))
                : 0;

            if (maxGallons <= 0)
            {
                maxGallons = 1;
            }

            for (var i = 0; i < reports.Count; i++)
            {
                var report = reports[i];
                var row = i + 4;

                charts.Cell(row, 1).Value = report.PeriodLabel;
                charts.Cell(row, 2).Value = report.WaterPumped;
                charts.Cell(row, 3).Value = report.WaterConsumed;
                charts.Cell(row, 4).Value = report.WaterLoss;
                charts.Cell(row, 5).Value = report.LossPercent;

                charts.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
                charts.Cell(row, 3).Style.NumberFormat.Format = "#,##0";
                charts.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                charts.Cell(row, 5).Style.NumberFormat.Format = "0.00%";

                var pumpedBar = MakeTextBar(report.WaterPumped, maxGallons);
                var consumedBar = MakeTextBar(report.WaterConsumed, maxGallons);
                var lossBar = MakeTextBar(report.WaterLoss, maxGallons);

                charts.Cell(row, 6).Value = pumpedBar;
                charts.Cell(row, 7).Value = consumedBar;
                charts.Cell(row, 8).Value = lossBar;

                charts.Cell(row, 6).Style.Font.FontColor = XLColor.FromHtml("#2563eb");
                charts.Cell(row, 7).Style.Font.FontColor = XLColor.FromHtml("#059669");

                if (report.LossPercent >= 0.30m)
                {
                    charts.Cell(row, 8).Style.Font.FontColor = XLColor.FromHtml("#dc2626");
                    charts.Cell(row, 9).Value = "High";
                    charts.Cell(row, 9).Style.Fill.BackgroundColor = XLColor.FromHtml("#fee2e2");
                    charts.Cell(row, 9).Style.Font.FontColor = XLColor.FromHtml("#991b1b");
                }
                else if (report.LossPercent >= 0.20m)
                {
                    charts.Cell(row, 8).Style.Font.FontColor = XLColor.FromHtml("#f59e0b");
                    charts.Cell(row, 9).Value = "Watch";
                    charts.Cell(row, 9).Style.Fill.BackgroundColor = XLColor.FromHtml("#fef3c7");
                    charts.Cell(row, 9).Style.Font.FontColor = XLColor.FromHtml("#92400e");
                }
                else
                {
                    charts.Cell(row, 8).Style.Font.FontColor = XLColor.FromHtml("#64748b");
                    charts.Cell(row, 9).Value = "OK";
                    charts.Cell(row, 9).Style.Fill.BackgroundColor = XLColor.FromHtml("#dcfce7");
                    charts.Cell(row, 9).Style.Font.FontColor = XLColor.FromHtml("#166534");
                }

                charts.Cell(row, 9).Style.Font.Bold = true;
                charts.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            var lastChartRow = reports.Count + 3;

            if (reports.Count > 0)
            {
                charts.Range(3, 1, lastChartRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                charts.Range(3, 1, lastChartRow, 9).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            charts.Cell("A" + (lastChartRow + 3)).Value = "Legend";
            charts.Cell("A" + (lastChartRow + 3)).Style.Font.Bold = true;

            charts.Cell("A" + (lastChartRow + 4)).Value = "Blue";
            charts.Cell("B" + (lastChartRow + 4)).Value = "Water Pumped";

            charts.Cell("A" + (lastChartRow + 5)).Value = "Green";
            charts.Cell("B" + (lastChartRow + 5)).Value = "Water Consumed";

            charts.Cell("A" + (lastChartRow + 6)).Value = "Gray / Orange / Red";
            charts.Cell("B" + (lastChartRow + 6)).Value = "Unaccounted-for water / Loss";

            charts.Cell("A" + (lastChartRow + 8)).Value = "Loss % Thresholds";
            charts.Cell("A" + (lastChartRow + 8)).Style.Font.Bold = true;

            charts.Cell("A" + (lastChartRow + 9)).Value = "OK";
            charts.Cell("B" + (lastChartRow + 9)).Value = "Under 20%";

            charts.Cell("A" + (lastChartRow + 10)).Value = "Watch";
            charts.Cell("B" + (lastChartRow + 10)).Value = "20% to 29.99%";

            charts.Cell("A" + (lastChartRow + 11)).Value = "High";
            charts.Cell("B" + (lastChartRow + 11)).Value = "30% or higher";

            charts.Columns("A:I").AdjustToContents();
            charts.Column("F").Width = 35;
            charts.Column("G").Width = 35;
            charts.Column("H").Width = 35;

            charts.SheetView.FreezeRows(3);

            // =========================
            // Saved Reports Sheet
            // =========================

            var headers = new[]
            {
        "Period Label",
        "Last Read Date",
        "Current Read Date",
        "Billing Days",
        "Water Pumped",
        "Water Consumed",
        "Unaccounted For",
        "Loss %",
        "Pumped Avg/Day",
        "Consumed Avg/Day",
        "Notes"
    };

            for (var i = 0; i < headers.Length; i++)
            {
                data.Cell(1, i + 1).Value = headers[i];
            }

            data.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;
            data.Range(1, 1, 1, headers.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#dbeafe");

            for (var i = 0; i < reports.Count; i++)
            {
                var row = i + 2;
                var report = reports[i];

                data.Cell(row, 1).Value = report.PeriodLabel;
                data.Cell(row, 2).Value = report.LastReadDate;
                data.Cell(row, 3).Value = report.CurrentReadDate;
                data.Cell(row, 4).Value = report.BillingDays;
                data.Cell(row, 5).Value = report.WaterPumped;
                data.Cell(row, 6).Value = report.WaterConsumed;
                data.Cell(row, 7).Value = report.WaterLoss;
                data.Cell(row, 8).Value = report.LossPercent;
                data.Cell(row, 9).Value = report.PumpedAveragePerDay;
                data.Cell(row, 10).Value = report.ConsumedAveragePerDay;
                data.Cell(row, 11).Value = report.Notes;
            }

            data.Columns().AdjustToContents();

            data.RangeUsed()?.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            data.RangeUsed()?.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

            data.Columns(2, 3).Style.DateFormat.Format = "mm/dd/yyyy";
            data.Columns(5, 7).Style.NumberFormat.Format = "#,##0";
            data.Column(8).Style.NumberFormat.Format = "0.00%";
            data.Columns(9, 10).Style.NumberFormat.Format = "#,##0";

            summary.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        private static string MakeTextBar(decimal value, decimal maxValue)
        {
            if (value <= 0 || maxValue <= 0)
            {
                return "";
            }

            const int maxBlocks = 25;

            var blocks = (int)Math.Round((value / maxValue) * maxBlocks);

            if (blocks < 1)
            {
                blocks = 1;
            }

            if (blocks > maxBlocks)
            {
                blocks = maxBlocks;
            }

            return new string('█', blocks);
        }

        public class SaveConsumptionReportRequest
        {
            public string PeriodLabel { get; set; } = string.Empty;

            public DateTime LastReadDate { get; set; }

            public DateTime CurrentReadDate { get; set; }

            public decimal WaterConsumed { get; set; }

            public decimal? WaterPumpedOverride { get; set; }

            public string Notes { get; set; } = string.Empty;
        }

        public class ConsumptionReportDto
        {
            public string PeriodLabel { get; set; } = string.Empty;

            public DateTime LastReadDate { get; set; }

            public DateTime CurrentReadDate { get; set; }

            public int BillingDays { get; set; }

            public decimal WaterPumped { get; set; }

            public decimal WaterConsumed { get; set; }

            public decimal WaterLoss { get; set; }

            public decimal LossPercent { get; set; }

            public decimal PumpedAveragePerDay { get; set; }

            public decimal ConsumedAveragePerDay { get; set; }

            public string Notes { get; set; } = string.Empty;
        }
    }
}