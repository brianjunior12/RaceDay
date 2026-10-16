
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Enrolments;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

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

    // GET: api/Enrolments
    [HttpGet]
    public async Task<IActionResult> GetEnrolments()
    {
        var isOrganiser = User.IsInRole("Organiser");
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var query = _context.Enrolments.AsNoTracking();

        if (!isOrganiser)
        {
            query = query.Where(e => e.ParticipantID == userId);
        }

        var enrolments = await query
            .Select(e => new
            {
                e.EnrolmentID,
                e.ParticipantID,
                e.EventID,
                e.CategoryID,
                e.EnrolmentDate,
                e.Enrolment_status
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    // GET: api/Enrolments/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEnrolmentById(int id)
    {
        var enrolment = await _context.Enrolments
            .AsNoTracking()
            .Where(e => e.EnrolmentID == id)
            .Select(e => new
            {
                e.EnrolmentID,
                e.ParticipantID,
                e.EventID,
                e.CategoryID,
                e.EnrolmentDate,
                e.Enrolment_status
            })
            .FirstOrDefaultAsync();

        if (enrolment is null)
            return NotFound(new { message = "Enrolment not found." });

        var isOrganiser = User.IsInRole("Organiser");
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized();

        if (!isOrganiser && enrolment.ParticipantID != userId)
            return Forbid();

        return Ok(enrolment);
    }

    // POST: api/Enrolments
    [HttpPost]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> CreateEnrolment(
        [FromBody] CreateEnrolmentRequest request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var participantId))
            return Unauthorized();

        var eventExists = await _context.Events
            .AnyAsync(e => e.EventID == request.EventID);

        if (!eventExists)
        {
            return NotFound(new
            {
                message = $"Event with ID {request.EventID} was not found."
            });
        }

        var categoryBelongsToEvent = await _context.Categories
            .AnyAsync(c =>
                c.CategoryID == request.CategoryID &&
                c.EventID == request.EventID);

        if (!categoryBelongsToEvent)
        {
            return BadRequest(new
            {
                message = "The selected category does not belong to the specified event."
            });
        }

        var alreadyEnrolled = await _context.Enrolments
            .AnyAsync(e =>
                e.ParticipantID == participantId &&
                e.EventID == request.EventID &&
                e.CategoryID == request.CategoryID);

        if (alreadyEnrolled)
        {
            return Conflict(new
            {
                message = "You are already enrolled in this category for this event."
            });
        }

        var enrolment = new Enrolment
        {
            ParticipantID = participantId,
            EventID = request.EventID,
            CategoryID = request.CategoryID,
            EnrolmentDate = DateTime.UtcNow,
            Enrolment_status = "Pending"
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Enrolment created successfully.",
            enrolment.EnrolmentID,
            enrolment.ParticipantID,
            enrolment.EventID,
            enrolment.CategoryID,
            enrolment.EnrolmentDate,
            enrolment.Enrolment_status
        });
    }

    // PATCH: api/Enrolments/5/status
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateEnrolmentStatus(
        int id,
        [FromBody] UpdateEnrolmentStatusRequest request)
    {
        var enrolment = await _context.Enrolments
            .FirstOrDefaultAsync(e => e.EnrolmentID == id);

        if (enrolment is null)
            return NotFound(new { message = "Enrolment not found." });

        enrolment.Enrolment_status = request.Enrolment_status;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Enrolment status updated successfully.",
            enrolment.EnrolmentID,
            enrolment.Enrolment_status
        });
    }
}

