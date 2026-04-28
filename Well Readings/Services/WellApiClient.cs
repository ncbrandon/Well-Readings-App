using Microsoft.AspNetCore.Mvc;

using System.Net.Http.Json;
using Well_Readings.DTOs;

namespace Well_Readings.Services
{
    public class WellApiClient
    {
        private readonly HttpClient _http;

        public WellApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<DailyEntryResponseDto?> GetTodayAsync()
            => await _http.GetFromJsonAsync<DailyEntryResponseDto>("api/daily-entries/today");

        public async Task<List<WellDto>?> GetWellsAsync()
            => await _http.GetFromJsonAsync<List<WellDto>>("api/daily-entries/wells");

        public async Task<DailyEntryResponseDto?> GetByIdAsync(Guid id)
            => await _http.GetFromJsonAsync<DailyEntryResponseDto>($"api/daily-entries/{id}");

        public async Task<HttpResponseMessage> SaveAsync(DailyEntryRequestDto dto)
            => await _http.PostAsJsonAsync("api/daily-entries", dto);
    }

    public class WellDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
