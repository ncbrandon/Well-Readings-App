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

        [HttpPost]
        public async Task<IActionResult> CreateDailyEntry([FromBody] DailyEntryRequestDto request)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var dailyEntry = new DailyEntry
                {
                    Id = Guid.NewGuid(),
                    EntryDate = request.EntryDate,
                    EntryTime = request.EntryTime
                };

                _db.DailyEntries.Add(dailyEntry);
                await _db.SaveChangesAsync();

                foreach (var wr in request.WellReadings)
                {
                    _db.WellReadings.Add(new WellReading
                    {
                        Id = Guid.NewGuid(),
                        DailyEntryId = dailyEntry.Id,
                        WellName = wr.WellName,
                        MeterReading = wr.MeterReading,
                        Chlorine = wr.Chlorine,
                        Phosphate = wr.Phosphate,
                        Ph = wr.Ph
                    });
                }

                if (request.FiltrationPlant != null)
                {
                    _db.FiltrationPlantReadings.Add(new FiltrationPlantReading
                    {
                        Id = Guid.NewGuid(),
                        DailyEntryId = dailyEntry.Id,
                        FilterPlantMeterReading = request.FiltrationPlant.FilterPlantMeterReading,
                        MtJeffersonMeterReading = request.FiltrationPlant.MtJeffersonMeterReading,
                        Chlorine = request.FiltrationPlant.Chlorine,
                        Phosphate = request.FiltrationPlant.Phosphate,
                        Ph = request.FiltrationPlant.Ph,
                        Temperature = request.FiltrationPlant.Temperature
                    });
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { dailyEntry.Id });
            }

            catch (Exception)
            {
                await transaction.RollbackAsync();
                return BadRequest("Unable to save daily entry. Please verify the data and try again.");
            }

        }
    }
}