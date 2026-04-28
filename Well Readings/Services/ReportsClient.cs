using System.Net.Http.Json;
using Well_Readings.DTOs.API;

namespace Well_Readings.Services
{
    public class ReportsClient : IReportsClient
    {
        private readonly HttpClient _http;

        public ReportsClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<MonthlyWellReportRowDto>> GetMonthly(int year, int month)
        {
            return await _http.GetFromJsonAsync<List<MonthlyWellReportRowDto>>(
                $"/api/reports/monthly?year={year}&month={month}")
                ?? new();
        }

        public async Task<List<RangeSummaryRowDto>> GetRange(DateOnly start, DateOnly end)
        {
            return await _http.GetFromJsonAsync<List<RangeSummaryRowDto>>(
                $"/api/reports/range-summary?start={start}&end={end}")
                ?? new();
        }
    }
}
