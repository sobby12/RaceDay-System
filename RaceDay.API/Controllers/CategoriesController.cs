
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/Events/5/Categories
        [HttpGet("/api/Events/{eventId:int}/Categories")]
        public async Task<ActionResult<IEnumerable<Category>>> GetEventCategories(
            int eventId)
        {
            var eventExists = await _context.Events
                .AnyAsync(e => e.EventId == eventId);

            if (!eventExists)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.EventId == eventId)
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/Events/5/Categories
        [HttpPost("/api/Events/{eventId:int}/Categories")]
        public async Task<ActionResult<Category>> CreateCategory(
            int eventId,
            [FromBody] Category category)
        {
            if (eventId <= 0 || category.EventId != eventId)
            {
                return BadRequest(new
                {
                    message = "The event ID in the URL and request body must match."
                });
            }

            if (category.CategoryId != 0)
            {
                return BadRequest(new
                {
                    message = "Do not provide a category ID when creating a category."
                });
            }

            if (string.IsNullOrWhiteSpace(category.CategoryName) ||
                string.IsNullOrWhiteSpace(category.CategoryType))
            {
                return BadRequest(new
                {
                    message = "Provide a category name and category type."
                });
            }

            category.CategoryName = category.CategoryName.Trim();
            category.CategoryType = category.CategoryType.Trim();

            if (category.CategoryType != "Distance" &&
                category.CategoryType != "Age")
            {
                return BadRequest(new
                {
                    message = "Category type must be Distance or Age."
                });
            }

            if (category.MinAge < 0 ||
                category.MaxAge < 0 ||
                (category.MinAge.HasValue &&
                 category.MaxAge.HasValue &&
                 category.MinAge > category.MaxAge) ||
                (category.CategoryDistanceKm.HasValue &&
                 category.CategoryDistanceKm <= 0))
            {
                return BadRequest(new
                {
                    message = "Check the age range and category distance values."
                });
            }

            var eventExists = await _context.Events
                .AnyAsync(e => e.EventId == eventId);

            if (!eventExists)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            var duplicateCategory = await _context.Categories.AnyAsync(c =>
                c.EventId == eventId &&
                c.CategoryName == category.CategoryName);

            if (duplicateCategory)
            {
                return Conflict(new
                {
                    message = "A category with this name already exists for this event."
                });
            }

            category.EventId = eventId;

            _context.Categories.Add(category);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "The category could not be saved. Check the category values and database constraints."
                });
            }

            return CreatedAtAction(
                nameof(GetEventCategories),
                new { eventId = category.EventId },
                category);
        }

        // PUT: api/Categories/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(
            int id,
            [FromBody] Category updatedCategory)
        {
            if (id != updatedCategory.CategoryId)
            {
                return BadRequest(new
                {
                    message = "The category ID in the URL and request body must match."
                });
            }

            if (updatedCategory.EventId <= 0 ||
                string.IsNullOrWhiteSpace(updatedCategory.CategoryName) ||
                string.IsNullOrWhiteSpace(updatedCategory.CategoryType))
            {
                return BadRequest(new
                {
                    message = "Provide a valid event ID, category name and category type."
                });
            }

            updatedCategory.CategoryName =
                updatedCategory.CategoryName.Trim();

            updatedCategory.CategoryType =
                updatedCategory.CategoryType.Trim();

            if (updatedCategory.CategoryType != "Distance" &&
                updatedCategory.CategoryType != "Age")
            {
                return BadRequest(new
                {
                    message = "Category type must be Distance or Age."
                });
            }

            if (updatedCategory.MinAge < 0 ||
                updatedCategory.MaxAge < 0 ||
                (updatedCategory.MinAge.HasValue &&
                 updatedCategory.MaxAge.HasValue &&
                 updatedCategory.MinAge > updatedCategory.MaxAge) ||
                (updatedCategory.CategoryDistanceKm.HasValue &&
                 updatedCategory.CategoryDistanceKm <= 0))
            {
                return BadRequest(new
                {
                    message = "Check the age range and category distance values."
                });
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            var eventExists = await _context.Events
                .AnyAsync(e => e.EventId == updatedCategory.EventId);

            if (!eventExists)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            var duplicateCategory = await _context.Categories.AnyAsync(c =>
                c.EventId == updatedCategory.EventId &&
                c.CategoryName == updatedCategory.CategoryName &&
                c.CategoryId != id);

            if (duplicateCategory)
            {
                return Conflict(new
                {
                    message = "A category with this name already exists for this event."
                });
            }

            category.EventId = updatedCategory.EventId;
            category.CategoryName = updatedCategory.CategoryName;
            category.CategoryType = updatedCategory.CategoryType;
            category.MinAge = updatedCategory.MinAge;
            category.MaxAge = updatedCategory.MaxAge;
            category.CategoryDistanceKm =
                updatedCategory.CategoryDistanceKm;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "The category could not be updated. Check the category values and database constraints."
                });
            }

            return NoContent();
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            _context.Categories.Remove(category);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "This category cannot be deleted because it may be linked to existing enrolments."
                });
            }

            return NoContent();
        }
    }
}

