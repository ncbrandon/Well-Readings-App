using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Well_Readings.Data;
using Well_Readings.Hubs;
using Well_Readings.Models;

namespace Well_Readings.Services
{
    public class ScadaRealtimeService
    {
        private readonly IHubContext<ScadaHub> _hub;
        private readonly AppDbContext _db;

        public ScadaRealtimeService(
            IHubContext<ScadaHub> hub,
            AppDbContext db)
        {
            _hub = hub;
            _db = db;
        }

        public async Task PushUpdateAsync(object data)
        {
            await _hub.Clients.All.SendAsync("scadaUpdate", data);
        }

        public async Task PushAlarmAsync(object alarm)
        {
            await _hub.Clients.All.SendAsync("scadaAlarm", alarm);
        }

        // ✅ THIS IS WHAT YOU WERE MISSING
        public async Task EvaluateAndBroadcastAsync()
        {
            var readings = await _db.WellReadings
                .Include(r => r.Well)
                .Include(r => r.DailyEntry)
                .ToListAsync();

            var configs = await _db.WellAlarmConfigs.ToListAsync();

            var data = readings
                .Where(r => r.Well != null && r.DailyEntry != null)
                .GroupBy(r => r.Well.Id)
                .Select(g =>
                {
                    var config = configs.FirstOrDefault(c => c.WellId == g.Key)
                                 ?? new WellAlarmConfig { WellId = g.Key };

                    var last = g
                        .OrderByDescending(x => x.DailyEntry.EntryDate)
                        .ThenByDescending(x => x.DailyEntry.EntryTime)
                        .FirstOrDefault();

                    var lastValue = last?.MeterReading ?? 0;

                    var isAlarm = lastValue > config.HighThreshold;

                    if (isAlarm)
                    {
                        config.IsAcknowledged = false;
                        config.LastAlarmTime = DateTime.Now;
                    }

                    return new
                    {
                        wellId = g.Key,
                        wellName = g.First().Well.Name,
                        lastReading = lastValue,
                        totalGallons = g.Sum(x => x.MeterReading),
                        isAlarm = isAlarm,
                        isAcknowledged = config.IsAcknowledged,
                        threshold = config.HighThreshold
                    };
                })
                .ToList();

            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("scadaFullUpdate", data);
        }

    }
}
