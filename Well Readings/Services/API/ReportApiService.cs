using System.Net.Http.Json;
using Well_Readings.DTOs.API;

namespace Well_Readings.Services.Api;

public class ReportApiService
{
    private readonly HttpClient _client;

    public ReportApiService(HttpClient client)
    {
        _client = client;
    }

    public async Task<List<RangeSummaryRowDto>> GetRangeSummary(DateOnly start, DateOnly end)
    {
        return await _client.GetFromJsonAsync<List<RangeSummaryRowDto>>(
            $"api/reports/range-summary?start={start}&end={end}"
        ) ?? new();
    }

    public async Task<List<MonthlyWellReportRowDto>> GetMonthly(int year, int month)
    {
        return await _client.GetFromJsonAsync<List<MonthlyWellReportRowDto>>(
            $"api/reports/monthly?year={year}&month={month}"
        ) ?? new();
    }
}

