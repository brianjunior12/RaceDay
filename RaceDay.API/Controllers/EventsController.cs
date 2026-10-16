
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Events;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EventsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // 1. GET: api/Events
    // Retrieve all events
    [HttpGet]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Events
            .AsNoTracking()
            .Select(e => new
            {
                e.EventID,
                e.EventName,
                e.Event_Discrip,
                e.EventDate,
                e.Distance,
                e.EventType,
                e.Location,
                e.OrganiserID
            })
            .ToListAsync();

        return Ok(events);
    }

    // 2. GET: api/Events/{id}
    // Retrieve one event by ID
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEventById(int id)
    {
        var eventItem = await _context.Events
            .AsNoTracking()
            .Where(e => e.EventID == id)
            .Select(e => new
            {
                e.EventID,
                e.EventName,
                e.Event_Discrip,
                e.EventDate,
                e.Distance,
                e.EventType,
                e.Location,
                e.OrganiserID
            })
            .FirstOrDefaultAsync();

        if (eventItem is null)
        {
            return NotFound(new
            {
                message = $"Event with ID {id} was not found."
            });
        }

        return Ok(eventItem);
    }

    // 3. POST: api/Events
    // Create a new event (Organiser only)
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateEvent(
        [FromBody] CreateEventRequest request)
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var organiserId))
        {
            return Unauthorized(new
            {
                message = "The authenticated user's ID is missing or invalid."
            });
        }

        var newEvent = new RaceDay.API.Models.Event
        {
            EventName = request.EventName.Trim(),
            Event_Discrip = request.Event_Discrip?.Trim(),
            EventDate = request.EventDate!.Value,
            Distance = request.Distance,
            EventType = request.EventType.Trim(),
            Location = request.Location.Trim(),
            OrganiserID = organiserId
        };

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Event created successfully.",
            eventId = newEvent.EventID,
            eventName = newEvent.EventName
        });
    }

    // 4. PUT: api/Events/{id}
    // Update an event (owning Organiser only)
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateEvent(
        int id,
        [FromBody] UpdateEventRequest request)
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var organiserId))
        {
            return Unauthorized(new
            {
                message = "The authenticated user's ID is missing or invalid."
            });
        }

        var eventItem = await _context.Events
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (eventItem is null)
        {
            return NotFound(new
            {
                message = $"Event with ID {id} was not found."
            });
        }

        if (eventItem.OrganiserID != organiserId)
        {
            return Forbid();
        }

        eventItem.EventName = request.EventName.Trim();
        eventItem.Event_Discrip = request.Event_Discrip?.Trim();
        eventItem.EventDate = request.EventDate!.Value;
        eventItem.Distance = request.Distance;
        eventItem.EventType = request.EventType.Trim();
        eventItem.Location = request.Location.Trim();

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Event updated successfully.",
            eventId = eventItem.EventID,
            eventName = eventItem.EventName
        });
    }

    // 5. DELETE: api/Events/{id}
    // Delete an event (owning Organiser only)
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var organiserId))
        {
            return Unauthorized(new
            {
                message = "The authenticated user's ID is missing or invalid."
            });
        }

        var eventItem = await _context.Events
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (eventItem is null)
        {
            return NotFound(new
            {
                message = $"Event with ID {id} was not found."
            });
        }

        if (eventItem.OrganiserID != organiserId)
        {
            return Forbid();
        }

        _context.Events.Remove(eventItem);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Event deleted successfully.",
            eventId = id
        });
    }
}

