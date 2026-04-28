using Well_Readings.DTOs.API;

namespace Well_Readings.Services
{
    public interface IReportsClient
    {
        Task<List<MonthlyWellReportRowDto>> GetMonthly(int year, int month);
        Task<List<RangeSummaryRowDto>> GetRange(DateOnly start, DateOnly end);
    }
}
