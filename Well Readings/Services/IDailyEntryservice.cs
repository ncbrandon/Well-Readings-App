using Well_Readings.DTOs;

namespace Well_Readings.Services
{
    public interface IDailyEntryService
    {
        Task<DailyEntryResponseDto> CreateOrUpdateAsync(DailyEntryRequestDto request);

        Task<List<DailyEntryResponseDto>> GetAllAsync();

        Task<DailyEntryResponseDto?> GetByIdAsync(Guid id);

        Task<DailyEntryResponseDto?> GetTodayAsync();

        Task<List<WellDto>> GetWellsAsync();

        Task<bool> DeleteAsync(Guid id);
    }
}

