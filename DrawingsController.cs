using DrawingApp.Data;
using DrawingApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace DrawingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DrawingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DrawingsController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        private bool IsAdmin()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            return roleClaim != null && roleClaim.Value == "Admin";
        }

        [HttpPost]
        public async Task<IActionResult> SaveDrawing([FromBody] DrawingDto drawingDto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var drawing = new Drawing
            {
                Name = drawingDto.Name,
                GeoJson = drawingDto.GeoJson,
                CreatedByUserId = userId,
                CreatedAt = System.DateTime.UtcNow
            };

            _context.Drawings.Add(drawing);
            await _context.SaveChangesAsync();
            return Ok(drawing);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (IsAdmin())
            {
                var allDrawings = await _context.Drawings.Include(d => d.Owner).ToListAsync();
                return Ok(allDrawings);
            }
            else
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                    return Unauthorized();

                var userDrawings = await _context.Drawings
                    .Where(d => d.CreatedByUserId == userId)
                    .Include(d => d.Owner)
                    .ToListAsync();

                return Ok(userDrawings);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var drawing = await _context.Drawings.FindAsync(id);
            if (drawing == null)
                return NotFound();

            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            if (drawing.CreatedByUserId != userId && !IsAdmin())
                return Forbid();

            _context.Drawings.Remove(drawing);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DrawingDto drawingDto)
        {
            var existing = await _context.Drawings.FindAsync(id);
            if (existing == null)
                return NotFound();

            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            if (existing.CreatedByUserId != userId && !IsAdmin())
                return Forbid();

            existing.Name = drawingDto.Name;
            existing.GeoJson = drawingDto.GeoJson;
            await _context.SaveChangesAsync();

            return Ok(existing);
        }
    }
}
