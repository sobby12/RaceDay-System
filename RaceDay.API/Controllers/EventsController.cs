
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Microsoft.AspNetCore.Authorization;




namespace RaceDay.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {



        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            var events = await _context.Events
                .AsNoTracking()
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/events/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var eventItem = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            return Ok(eventItem);
        }

        // POST: api/events
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event eventItem)
        {
            if (eventItem.EventId != 0)
            {
                return BadRequest(new
                {
                    message = "Do not provide an EventId when creating an event."
                });
            }

            if (eventItem.OrganizerId <= 0 ||
                string.IsNullOrWhiteSpace(eventItem.EventName) ||
                string.IsNullOrWhiteSpace(eventItem.Location) ||
                string.IsNullOrWhiteSpace(eventItem.EventType) ||
                eventItem.DistanceKm <= 0 ||
                eventItem.EntryFee < 0 ||
                eventItem.MaxParticipants <= 0)
            {
                return BadRequest(new
                {
                    message = "Provide a valid organizer, name, location, event type, distance, entry fee and participant limit."
                });
            }

            var eventType = eventItem.EventType.Trim();

            if (eventType != "Run" &&
                eventType != "Walk" &&
                eventType != "Cycle")
            {
                return BadRequest(new
                {
                    message = "EventType must be Run, Walk or Cycle."
                });
            }

            eventItem.EventType = eventType;
            eventItem.EventName = eventItem.EventName.Trim();
            eventItem.Location = eventItem.Location.Trim();

            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = eventItem.EventId },
                eventItem);
        }

        // PUT: api/events/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(int id, Event updatedEvent)
        {
            if (id != updatedEvent.EventId)
            {
                return BadRequest(new
                {
                    message = "The URL ID must match the EventId in the request body."
                });
            }

            if (updatedEvent.OrganizerId <= 0 ||
                string.IsNullOrWhiteSpace(updatedEvent.EventName) ||
                string.IsNullOrWhiteSpace(updatedEvent.Location) ||
                string.IsNullOrWhiteSpace(updatedEvent.EventType) ||
                updatedEvent.DistanceKm <= 0 ||
                updatedEvent.EntryFee < 0 ||
                updatedEvent.MaxParticipants <= 0)
            {
                return BadRequest(new
                {
                    message = "Provide valid event details."
                });
            }

            var eventType = updatedEvent.EventType.Trim();

            if (eventType != "Run" &&
                eventType != "Walk" &&
                eventType != "Cycle")
            {
                return BadRequest(new
                {
                    message = "EventType must be Run, Walk or Cycle."
                });
            }

            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (existingEvent == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            existingEvent.OrganizerId = updatedEvent.OrganizerId;
            existingEvent.EventName = updatedEvent.EventName.Trim();
            existingEvent.Description = updatedEvent.Description;
            existingEvent.EventDate = updatedEvent.EventDate;
            existingEvent.Location = updatedEvent.Location.Trim();
            existingEvent.DistanceKm = updatedEvent.DistanceKm;
            existingEvent.EventType = eventType;
            existingEvent.EntryFee = updatedEvent.EntryFee;
            existingEvent.MaxParticipants = updatedEvent.MaxParticipants;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/events/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

