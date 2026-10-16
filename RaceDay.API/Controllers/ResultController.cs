
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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
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

        // GET: api/Results
        // Administrators can view all results.
        [HttpGet]
        [Authorize(Roles = "Admin,Administrator")]
        public async Task<ActionResult<IEnumerable<RaceDay.API.Models.Result>>>
            GetResults()
        {
            var results = await _context.Result
                .AsNoTracking()
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/Results/5
        // Participants can view their own results.
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RaceDay.API.Models.Result>>
            GetResult(int id)
        {
            var result = await _context.Result
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound(new { message = "Result not found." });

            if (!IsAdministrator())
            {
                var userId = GetCurrentUserId();

                if (userId == null)
                    return Unauthorized();

                var isOwner = await _context.EventEnrollments.AnyAsync(e =>
                    e.EnrollmentId == result.EnrollmentId &&
                    e.UserId == userId.Value);

                if (!isOwner)
                    return Forbid();
            }

            return Ok(result);
        }

        // POST: api/Enrolments/5/Result
        // Only administrators can record results.
        [HttpPost("/api/Enrolments/{enrolmentId:int}/Result")]
        [Authorize(Roles = "Admin,Administrator")]
        public async Task<ActionResult<RaceDay.API.Models.Result>>
            CreateResult(
                int enrolmentId,
                [FromBody] RaceDay.API.Models.Result result)
        {
            if (enrolmentId <= 0 ||
                result.ResultId != 0 ||
                result.EnrollmentId != enrolmentId)
            {
                return BadRequest(new
                {
                    message = "Provide a valid enrolment ID and do not provide a result ID."
                });
            }

            if (result.FinishPosition.HasValue &&
                result.FinishPosition.Value <= 0)
            {
                return BadRequest(new
                {
                    message = "Finish position must be greater than zero."
                });
            }

            if (result.FinishTimeSeconds.HasValue &&
                result.FinishTimeSeconds.Value <= 0)
            {
                return BadRequest(new
                {
                    message = "Finish time must be greater than zero seconds."
                });
            }

            result.ResultStatus = string.IsNullOrWhiteSpace(result.ResultStatus)
                ? "Official"
                : result.ResultStatus.Trim();

            if (result.ResultStatus != "Official" &&
                result.ResultStatus != "Pending" &&
                result.ResultStatus != "Disqualified")
            {
                return BadRequest(new
                {
                    message = "Result status must be Official, Pending or Disqualified."
                });
            }

            var enrolment = await _context.EventEnrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrolmentId);

            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            if (enrolment.Status == "Cancelled")
            {
                return BadRequest(new
                {
                    message = "A result cannot be recorded for a cancelled enrolment."
                });
            }

            var resultExists = await _context.Result
                .AnyAsync(r => r.EnrollmentId == enrolmentId);

            if (resultExists)
            {
                return Conflict(new
                {
                    message = "A result already exists for this enrolment."
                });
            }

            _context.Result.Add(result);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "The result could not be saved. Check the enrolment and result values."
                });
            }

            return CreatedAtAction(
                nameof(GetResult),
                new { id = result.ResultId },
                result);
        }

        // GET: api/Events/5/Results
        // Only administrators can view all results for an event.
        [HttpGet("/api/Events/{eventId:int}/Results")]
        [Authorize(Roles = "Admin,Administrator")]
        public async Task<ActionResult<IEnumerable<RaceDay.API.Models.Result>>>
            GetEventResults(int eventId)
        {
            var eventExists = await _context.Events
                .AnyAsync(e => e.EventId == eventId);

            if (!eventExists)
                return NotFound(new { message = "Event not found." });

            var results = await _context.Result
                .AsNoTracking()
                .Join(
                    _context.EventEnrollments,
                    result => result.EnrollmentId,
                    enrolment => enrolment.EnrollmentId,
                    (result, enrolment) => new { result, enrolment })
                .Where(x => x.enrolment.EventId == eventId)
                .Select(x => x.result)
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/Results/Me
        // Participants can retrieve their own results only.
        [HttpGet("Me")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var results = await _context.Result
                .AsNoTracking()
                .Join(
                    _context.EventEnrollments,
                    result => result.EnrollmentId,
                    enrolment => enrolment.EnrollmentId,
                    (result, enrolment) => new { result, enrolment })
                .Where(x => x.enrolment.UserId == userId.Value)
                .Select(x => x.result)
                .ToListAsync();

            return Ok(results);
        }

        // PUT: api/Results/5
        // Only administrators can update results.
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Administrator")]
        public async Task<IActionResult> UpdateResult(
            int id,
            [FromBody] RaceDay.API.Models.Result updatedResult)
        {
            if (id != updatedResult.ResultId)
            {
                return BadRequest(new
                {
                    message = "The result ID in the URL and request body must match."
                });
            }

            if (updatedResult.FinishPosition.HasValue &&
                updatedResult.FinishPosition.Value <= 0)
            {
                return BadRequest(new
                {
                    message = "Finish position must be greater than zero."
                });
            }

            if (updatedResult.FinishTimeSeconds.HasValue &&
                updatedResult.FinishTimeSeconds.Value <= 0)
            {
                return BadRequest(new
                {
                    message = "Finish time must be greater than zero seconds."
                });
            }

            updatedResult.ResultStatus =
                updatedResult.ResultStatus?.Trim() ?? "";

            if (updatedResult.ResultStatus != "Official" &&
                updatedResult.ResultStatus != "Pending" &&
                updatedResult.ResultStatus != "Disqualified")
            {
                return BadRequest(new
                {
                    message = "Result status must be Official, Pending or Disqualified."
                });
            }

            var result = await _context.Result
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound(new { message = "Result not found." });

            if (result.EnrollmentId != updatedResult.EnrollmentId)
            {
                return BadRequest(new
                {
                    message = "The enrolment linked to a result cannot be changed."
                });
            }

            result.FinishPosition = updatedResult.FinishPosition;
            result.FinishTimeSeconds = updatedResult.FinishTimeSeconds;
            result.ResultStatus = updatedResult.ResultStatus;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "The result could not be updated."
                });
            }

            return NoContent();
        }

        // DELETE: api/Results/5
        // Only administrators can delete results.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Administrator")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var result = await _context.Result
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound(new { message = "Result not found." });

            _context.Result.Remove(result);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "The result could not be deleted."
                });
            }

            return NoContent();
        }
    }
}

