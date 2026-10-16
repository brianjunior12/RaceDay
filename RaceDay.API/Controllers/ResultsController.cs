
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Results;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

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

    // GET: api/Results
    [HttpGet]
    public async Task<IActionResult> GetResults()
    {
        var results = await _context.Results
            .AsNoTracking()
            .Select(r => new
            {
                r.ResultsID,
                r.EnrolmentID,
                r.FinishTime,
                r.FinishPosition
            })
            .ToListAsync();

        return Ok(results);
    }

    // GET: api/Results/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetResultById(int id)
    {
        var result = await _context.Results
            .AsNoTracking()
            .Where(r => r.ResultsID == id)
            .Select(r => new
            {
                r.ResultsID,
                r.EnrolmentID,
                r.FinishTime,
                r.FinishPosition
            })
            .FirstOrDefaultAsync();

        if (result is null)
        {
            return NotFound(new
            {
                message = "Result not found."
            });
        }

        return Ok(result);
    }

    // POST: api/Results
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateResult(
        [FromBody] CreateResultRequest request)
    {
        if (!TimeSpan.TryParseExact(
                request.FinishTime,
                new[] { @"hh\:mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss\.FFFFFFF" },
                CultureInfo.InvariantCulture,
                out var finishTime))
        {
            return BadRequest(new
            {
                message = "FinishTime must be a valid time in HH:mm:ss format."
            });
        }

        if (finishTime < TimeSpan.Zero ||
            finishTime >= TimeSpan.FromDays(1))
        {
            return BadRequest(new
            {
                message = "FinishTime must be between 00:00:00 and 23:59:59."
            });
        }

        var enrolmentExists = await _context.Enrolments
            .AnyAsync(e => e.EnrolmentID == request.EnrolmentID);

        if (!enrolmentExists)
        {
            return NotFound(new
            {
                message = $"Enrolment with ID {request.EnrolmentID} was not found."
            });
        }

        var resultExists = await _context.Results
            .AnyAsync(r => r.EnrolmentID == request.EnrolmentID);

        if (resultExists)
        {
            return Conflict(new
            {
                message = "A result already exists for this enrolment."
            });
        }

        var result = new Result
        {
            EnrolmentID = request.EnrolmentID,
            FinishTime = finishTime,
            FinishPosition = request.FinishPosition
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetResultById),
            new { id = result.ResultsID },
            new
            {
                result.ResultsID,
                result.EnrolmentID,
                result.FinishTime,
                result.FinishPosition
            });
    }
}
