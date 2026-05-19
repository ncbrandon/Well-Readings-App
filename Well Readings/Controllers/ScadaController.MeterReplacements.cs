using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Well_Readings.Models;

namespace Well_Readings.Controllers
{
    public partial class ScadaController
    {
        [HttpGet("meter-replacements")]
        public async Task<IActionResult> GetMeterReplacements()
        {
            var rows = await _context.MeterReplacements
                .OrderByDescending(x => x.ReplacementDate)
                .ThenBy(x => x.Location)
                .Select(x => new
                {
                    x.Id,
                    x.Location,
                    x.ReplacementDate,
                    x.OldMeterFinalReading,
                    x.NewMeterStartingReading,
                    x.Notes,
                    x.CreatedAt,
                    x.UpdatedAt
                })
                .ToListAsync();

            return Ok(rows);
        }

        [HttpPost("meter-replacements")]
        public async Task<IActionResult> SaveMeterReplacement([FromBody] SaveMeterReplacementRequest request)
        {
            if (request == null)
            {
                return BadRequest("No meter replacement data was submitted.");
            }

            if (string.IsNullOrWhiteSpace(request.Location))
            {
                return BadRequest("Location is required.");
            }

            if (request.ReplacementDate == default)
            {
                return BadRequest("Replacement date is required.");
            }

            var existing = await _context.MeterReplacements
                .FirstOrDefaultAsync(x =>
                    x.Location == request.Location &&
                    x.ReplacementDate.Date == request.ReplacementDate.Date);

            if (existing == null)
            {
                existing = new MeterReplacement
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.MeterReplacements.Add(existing);
            }

            existing.Location = request.Location.Trim();
            existing.ReplacementDate = request.ReplacementDate.Date;
            existing.OldMeterFinalReading = request.OldMeterFinalReading;
            existing.NewMeterStartingReading = request.NewMeterStartingReading;
            existing.Notes = request.Notes ?? string.Empty;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                saved = true,
                id = existing.Id
            });
        }

        [HttpDelete("meter-replacements/{id:guid}")]
        public async Task<IActionResult> DeleteMeterReplacement(Guid id)
        {
            var existing = await _context.MeterReplacements
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existing == null)
            {
                return NotFound();
            }

            _context.MeterReplacements.Remove(existing);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                deleted = true
            });
        }

        public class SaveMeterReplacementRequest
        {
            public string Location { get; set; } = string.Empty;

            public DateTime ReplacementDate { get; set; }

            public decimal OldMeterFinalReading { get; set; }

            public decimal NewMeterStartingReading { get; set; }

            public string Notes { get; set; } = string.Empty;
        }
    }
}