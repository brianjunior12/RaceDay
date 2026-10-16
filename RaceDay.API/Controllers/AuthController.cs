
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Auth;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    private readonly PasswordService _passwordService;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        RaceDayDbContext context,
        PasswordService passwordService,
        JwtService jwtService,
        ILogger<AuthController> logger)
    {
        _context = context;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _logger = logger;
    }

    // POST: api/Auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _context.Users
            .AnyAsync(u => u.Email == email);

        if (existingUser)
        {
            return Conflict(new
            {
                message = "An account with this email already exists."
            });
        }

        var participantRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleName == "Participant");

        if (participantRole is null)
        {
            return StatusCode(500, new
            {
                message = "The Participant role is not configured."
            });
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash = _passwordService.HashPassword(
                request.Password),
            RoleID = participantRole.RoleID
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Registration successful.",
            userId = user.UserId,
            firstName = user.FirstName,
            lastName = user.LastName,
            email = user.Email,
            role = participantRole.RoleName
        });
    }

    // POST: api/Auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        // TEMPORARY DIAGNOSTICS:
        // Log the result without logging the password or hash.
        if (user is null)
        {
            _logger.LogWarning(
                "Login diagnostic: user not found for submitted email.");

            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var passwordMatches = _passwordService.VerifyPassword(
            request.Password,
            user.PasswordHash);

        _logger.LogInformation(
            "Login diagnostic: user found. Password verification result: {PasswordMatches}",
            passwordMatches);

        if (!passwordMatches)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        if (user.Role is null)
        {
            return Unauthorized(new
            {
                message = "The user's role is not configured."
            });
        }

        var response = _jwtService.GenerateToken(
            user,
            user.Role.RoleName);

        return Ok(response);
    }
}