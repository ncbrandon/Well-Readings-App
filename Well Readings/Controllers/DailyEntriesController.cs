using Microsoft.AspNetCore.Mvc;
using Well_Readings.DTOs;
using Well_Readings.Services;

namespace Well_Readings.Controllers
{
    [ApiController]
    [Route("api/daily-entries")]
    public class DailyEntriesController : ControllerBase
    {
        private readonly IDailyEntryService _service;

        public DailyEntriesController(IDailyEntryService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DailyEntryRequestDto request)
        {
            var result = await _service.CreateOrUpdateAsync(request);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
            => Ok(await _service.GetByIdAsync(id));

        [HttpGet("today")]
        public async Task<IActionResult> Today()
            => Ok(await _service.GetTodayAsync());

        [HttpGet("wells")]
        public async Task<IActionResult> Wells()
            => Ok(await _service.GetWellsAsync());

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _service.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
