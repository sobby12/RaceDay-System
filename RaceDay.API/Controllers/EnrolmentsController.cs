
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(value, out int userId)
                ? userId
                : null;
        }

        private bool IsAdministrator()
        {
            return User.IsInRole("Admin") ||
                   User.IsInRole("Administrator");
        }

        // GET: api/Enrolments
        // Only administrators can view all enrolments.
        [HttpGet]
        [Authorize(Roles = "Admin,Administrator")]
        public async Task<ActionResult<IEnumerable<EventEnrollment>>> GetEnrolments()
        {
            var enrolments = await _context.EventEnrollments
                .AsNoTracking()
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/Enrolments/5
        // Participants can view their own enrolment.
        // Administrators can view any enrolment.
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventEnrollment>> GetEnrolment(int id)
        {
            var enrolment = await _context.EventEnrollments
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            if (!IsAdministrator() &&
                enrolment.UserId != GetCurrentUserId())
            {
                return Forbid();
            }

            return Ok(enrolment);
        }

        // POST: api/Events/5/Enrolments
        [HttpPost("/api/Events/{eventId:int}/Enrolments")]
        [Authorize(Roles = "Participant")]
        public async Task<ActionResult<EventEnrollment>> CreateEnrolment(
            int eventId,
            [FromBody] EventEnrollment enrolment)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            if (eventId <= 0 || enrolment.EventId != eventId)
            {
                return BadRequest(new
                {
                    message = "The event ID in the URL and request body must match."
                });
            }

            if (enrolment.EnrollmentId != 0 ||
                enrolment.CategoryId <= 0)
            {
                return BadRequest(new
                {
                    message = "Provide a valid category ID and do not provide an enrolment ID."
                });
            }

            // Never trust the submitted UserId.
            enrolment.UserId = currentUserId.Value;

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventItem == null)
                return NotFound(new { message = "Event not found." });

            var categoryExists = await _context.Categories.AnyAsync(c =>
                c.EventId == eventId &&
                c.CategoryId == enrolment.CategoryId);

            if (!categoryExists)
            {
                return BadRequest(new
                {
                    message = "The category does not belong to this event."
                });
            }

            var alreadyEnrolled = await _context.EventEnrollments.AnyAsync(e =>
                e.EventId == eventId &&
                e.UserId == currentUserId.Value &&
                e.Status != "Cancelled");

            if (alreadyEnrolled)
            {
                return Conflict(new
                {
                    message = "You are already enrolled in this event."
                });
            }

            var participantCount = await _context.EventEnrollments.CountAsync(e =>
                e.EventId == eventId &&
                e.Status != "Cancelled");

            if (participantCount >= eventItem.MaxParticipants)
            {
                return Conflict(new
                {
                    message = "This event has reached its participant limit."
                });
            }

            enrolment.Status = "Registered";
            enrolment.EnrollmentDate = DateTime.Now;

            _context.EventEnrollments.Add(enrolment);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "The enrolment could not be saved. Check the event and category details."
                });
            }

            return CreatedAtAction(
                nameof(GetEnrolment),
                new { id = enrolment.EnrollmentId },
                enrolment);
        }

        // GET: api/Events/5/Enrolments
        // Only administrators can view all enrolments for an event.
        [HttpGet("/api/Events/{eventId:int}/Enrolments")]
        [Authorize(Roles = "Admin,Administrator")]
        public async Task<ActionResult<IEnumerable<EventEnrollment>>> GetEventEnrolments(
            int eventId)
        {
            var eventExists = await _context.Events
                .AnyAsync(e => e.EventId == eventId);

            if (!eventExists)
                return NotFound(new { message = "Event not found." });

            var enrolments = await _context.EventEnrollments
                .AsNoTracking()
                .Where(e => e.EventId == eventId)
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/Enrolments/Me
        // Participants can only view their own enrolments.
        [HttpGet("Me")]
        [Authorize(Roles = "Participant")]
        public async Task<ActionResult<IEnumerable<EventEnrollment>>> GetMyEnrolments()
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var enrolments = await _context.EventEnrollments
                .AsNoTracking()
                .Where(e => e.UserId == currentUserId.Value)
                .ToListAsync();

            return Ok(enrolments);
        }

        // DELETE: api/Enrolments/5
        // Participants can cancel their own enrolment.
        // Administrators can cancel any enrolment.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            var enrolment = await _context.EventEnrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            if (!IsAdministrator() &&
                enrolment.UserId != GetCurrentUserId())
            {
                return Forbid();
            }

            if (enrolment.Status == "Cancelled")
            {
                return Conflict(new
                {
                    message = "This enrolment has already been cancelled."
                });
            }

            enrolment.Status = "Cancelled";

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "The enrolment could not be cancelled."
                });
            }

            return NoContent();
        }
    }
}

