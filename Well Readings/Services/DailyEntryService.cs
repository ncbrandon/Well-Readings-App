using Microsoft.EntityFrameworkCore;
using Well_Readings.Data;
using Well_Readings.DTOs;
using Well_Readings.Models;

namespace Well_Readings.Services
{
    public class DailyEntryService : IDailyEntryService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<DailyEntryService> _logger;
        private readonly ScadaRealtimeService _realtime;

        public DailyEntryService(
            AppDbContext db,
            ILogger<DailyEntryService> logger,
            ScadaRealtimeService realtime)
        {
            _db = db;
            _logger = logger;
            _realtime = realtime;
        }

        public async Task<DailyEntryResponseDto> CreateOrUpdateAsync(DailyEntryRequestDto request)
        {
            if (request.WellReadings == null || request.WellReadings.Count == 0)
                throw new InvalidOperationException("At least one well reading is required.");

            await using var tx = await _db.Database.BeginTransactionAsync();

            DailyEntry entry;

            if (request.Id.HasValue)
            {
                entry = await _db.DailyEntries
                    .FirstOrDefaultAsync(x => x.Id == request.Id.Value)
                    ?? throw new Exception("Entry not found");

                entry.EntryDate = request.EntryDate;
                entry.EntryTime = request.EntryTime;

                await _db.WellReadings
                    .Where(x => x.DailyEntryId == entry.Id)
                    .ExecuteDeleteAsync();
            }
            else
            {
                entry = new DailyEntry
                {
                    Id = Guid.NewGuid(),
                    EntryDate = request.EntryDate,
                    EntryTime = request.EntryTime,
                    CreatedAt = DateTime.UtcNow
                };

                _db.DailyEntries.Add(entry);
            }

            var validWells = await _db.Wells.Select(x => x.Id).ToListAsync();

            foreach (var r in request.WellReadings)
            {
                if (!validWells.Contains(r.WellId))
                    throw new Exception($"Invalid well {r.WellId}");

                _db.WellReadings.Add(new WellReading
                {
                    Id = Guid.NewGuid(),
                    DailyEntryId = entry.Id,
                    WellId = r.WellId,
                    MeterReading = r.MeterReading ?? 0m,
                    Chlorine = r.Chlorine ?? 0m,
                    Phosphate = r.Phosphate ?? 0m,
                    Ph = r.Ph ?? 0m
                });
                //Change Alarm Threshold to 60000 for testing purposes, can be changed back to 50000 later
                if ((r.MeterReading ?? 0) > 10000)
                {
                    await _realtime.PushAlarmAsync(new
                    {
                        WellId = r.WellId,
                        Value = r.MeterReading,
                        Message = "HIGH FLOW ALARM"
                    });
                }

            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            // 🔥 PUSH LIVE UPDATE TO SCADA
            await _realtime.PushUpdateAsync(new
            {
                message = "New data received",
                time = DateTime.Now
            });


            // 🔥 SCADA REAL-TIME PUSH
            await _realtime.EvaluateAndBroadcastAsync();


            return await GetByIdAsync(entry.Id)
                   ?? throw new Exception("Failed to load result");
        }

        public async Task<List<DailyEntryResponseDto>> GetAllAsync()
        {
            return await _db.DailyEntries
                .AsNoTracking()
                .Select(x => new DailyEntryResponseDto
                {
                    Id = x.Id,
                    EntryDate = x.EntryDate,
                    EntryTime = x.EntryTime,
                    WellReadings = new()
                })
                .ToListAsync();
        }

        public async Task<DailyEntryResponseDto?> GetByIdAsync(Guid id)
        {
            return await _db.DailyEntries
                .AsNoTracking()
                .Include(x => x.WellReadings)
                .ThenInclude(x => x.Well)
                .Where(x => x.Id == id)
                .Select(x => new DailyEntryResponseDto
                {
                    Id = x.Id,
                    EntryDate = x.EntryDate,
                    EntryTime = x.EntryTime,
                    WellReadings = x.WellReadings.Select(w => new WellReadingResponseDto
                    {
                        WellName = w.Well.Name,
                        MeterReading = w.MeterReading,
                        Chlorine = w.Chlorine,
                        Phosphate = w.Phosphate,
                        Ph = w.Ph
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<DailyEntryResponseDto?> GetTodayAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var entry = await _db.DailyEntries
                .FirstOrDefaultAsync(x => x.EntryDate == today);

            return entry == null ? null : await GetByIdAsync(entry.Id);
        }

        public async Task<List<WellDto>> GetWellsAsync()
        {
            return await _db.Wells
                .Select(x => new WellDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entry = await _db.DailyEntries
                .Include(x => x.WellReadings)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entry == null) return false;

            _db.WellReadings.RemoveRange(entry.WellReadings);
            _db.DailyEntries.Remove(entry);

            await _db.SaveChangesAsync();

            await _realtime.EvaluateAndBroadcastAsync();

            return true;
        }
    }
}
