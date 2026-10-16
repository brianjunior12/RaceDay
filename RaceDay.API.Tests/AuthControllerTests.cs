
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Auth;
using RaceDay.API.Models;
using RaceDay.API.Services;
using Xunit;

namespace RaceDay.API.Tests;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_WithIncorrectPassword_ReturnsUnauthorized()
    {
        // Arrange: create a separate in-memory database.
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayDbContext(options);

        var role = new Role
        {
            RoleID = 2,
            RoleName = "Participant"
        };

        var passwordService = new PasswordService();
        var user = new User
        {
            UserId = 1005,
            FirstName = "Test",
            LastName = "Participant",
            Email = "participant.test@example.com",
            PasswordHash = passwordService.HashPassword("CorrectPassword123!"),
            RoleID = role.RoleID,
            Role = role
        };

        context.Roles.Add(role);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var jwtSettings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "Test-Only-RaceDay-Key-2026-Long-Enough!",
            ["Jwt:Issuer"] = "RaceDay.API",
            ["Jwt:Audience"] = "RaceDay.Client"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(jwtSettings)
            .Build();

        var jwtService = new JwtService(configuration);
        using var loggerFactory = LoggerFactory.Create(builder => { });
        var logger = loggerFactory.CreateLogger<AuthController>();

        var controller = new AuthController(
            context,
            passwordService,
            jwtService,
            logger);

        // Act: attempt login with the wrong password.
        var request = new LoginRequest
        {
            Email = "participant.test@example.com",
            Password = "WrongPassword123!"
        };

        var result = await controller.Login(request);

        // Assert: invalid credentials should return HTTP 401.
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorized.StatusCode);
    }


    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsOkWithToken()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayDbContext(options);

        var role = new Role
        {
            RoleID = 2,
            RoleName = "Participant"
        };

        var passwordService = new PasswordService();

        var user = new User
        {
            UserId = 1005,
            FirstName = "Test",
            LastName = "Participant",
            Email = "participant.test@example.com",
            PasswordHash = passwordService.HashPassword("CorrectPassword123!"),
            RoleID = role.RoleID,
            Role = role
        };

        context.Roles.Add(role);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var jwtSettings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "Test-Only-RaceDay-Key-2026-Long-Enough!",
            ["Jwt:Issuer"] = "RaceDay.API",
            ["Jwt:Audience"] = "RaceDay.Client"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(jwtSettings)
            .Build();

        var jwtService = new JwtService(configuration);

        using var loggerFactory = LoggerFactory.Create(builder => { });
        var logger = loggerFactory.CreateLogger<AuthController>();

        var controller = new AuthController(
            context,
            passwordService,
            jwtService,
            logger);

        var request = new LoginRequest
        {
            Email = "participant.test@example.com",
            Password = "CorrectPassword123!"
        };

        var result = await controller.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var response = Assert.IsType<RaceDay.API.DTOs.Auth.LoginResponse>(
            okResult.Value);

        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal(user.UserId, response.UserId);
        Assert.Equal(user.Email, response.Email);
        Assert.Equal("Participant", response.Role);
        Assert.True(response.ExpiresAt > DateTime.UtcNow);
    }


    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayDbContext(options);

        var passwordService = new PasswordService();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "Test-Only-RaceDay-Key-2026-Long-Enough!",
                ["Jwt:Issuer"] = "RaceDay.API",
                ["Jwt:Audience"] = "RaceDay.Client"
            })
            .Build();

        var jwtService = new JwtService(configuration);

        using var loggerFactory = LoggerFactory.Create(builder => { });
        var logger = loggerFactory.CreateLogger<AuthController>();

        var controller = new AuthController(
            context,
            passwordService,
            jwtService,
            logger);

        var request = new LoginRequest
        {
            Email = "unknown.user@example.com",
            Password = "AnyPassword123!"
        };

        var result = await controller.Login(request);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorized.StatusCode);
    }


    [Fact]
    public async Task Register_WithExistingEmail_ReturnsConflict()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayDbContext(options);

        var passwordService = new PasswordService();

        context.Users.Add(new User
        {
            UserId = 1005,
            FirstName = "Existing",
            LastName = "Participant",
            Email = "participant.test@example.com",
            PasswordHash = passwordService.HashPassword("CorrectPassword123!"),
            RoleID = 2
        });

        await context.SaveChangesAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "Test-Only-RaceDay-Key-2026-Long-Enough!",
                ["Jwt:Issuer"] = "RaceDay.API",
                ["Jwt:Audience"] = "RaceDay.Client"
            })
            .Build();

        var jwtService = new JwtService(configuration);

        using var loggerFactory = LoggerFactory.Create(builder => { });
        var logger = loggerFactory.CreateLogger<AuthController>();

        var controller = new AuthController(
            context,
            passwordService,
            jwtService,
            logger);

        var request = new RegisterRequest
        {
            FirstName = "Another",
            LastName = "Participant",
            Email = "participant.test@example.com",
            Password = "AnotherPassword123!"
        };

        var result = await controller.Register(request);

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, conflict.StatusCode);
    }


    [Fact]
    public async Task Register_WithNewEmail_ReturnsCreatedAndAssignsParticipantRole()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayDbContext(options);

        context.Roles.Add(new Role
        {
            RoleID = 2,
            RoleName = "Participant"
        });

        await context.SaveChangesAsync();

        var passwordService = new PasswordService();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "Test-Only-RaceDay-Key-2026-Long-Enough!",
                ["Jwt:Issuer"] = "RaceDay.API",
                ["Jwt:Audience"] = "RaceDay.Client"
            })
            .Build();

        var jwtService = new JwtService(configuration);

        using var loggerFactory = LoggerFactory.Create(builder => { });
        var logger = loggerFactory.CreateLogger<AuthController>();

        var controller = new AuthController(
            context,
            passwordService,
            jwtService,
            logger);

        var request = new RegisterRequest
        {
            FirstName = "New",
            LastName = "Participant",
            Email = "new.participant@example.com",
            Password = "SecureTestPassword123!"
        };

        var result = await controller.Register(request);

        var created = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);

        var savedUser = await context.Users
            .SingleAsync(u => u.Email == "new.participant@example.com");

        Assert.Equal("New", savedUser.FirstName);
        Assert.Equal("Participant", savedUser.LastName);
        Assert.Equal(2, savedUser.RoleID);
        Assert.NotEqual("SecureTestPassword123!", savedUser.PasswordHash);
        Assert.True(passwordService.VerifyPassword(
            "SecureTestPassword123!",
            savedUser.PasswordHash));
    }


    [Fact]
    public async Task Register_WhenParticipantRoleIsMissing_ReturnsInternalServerError()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayDbContext(options);

        var passwordService = new PasswordService();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "Test-Only-RaceDay-Key-2026-Long-Enough!",
                ["Jwt:Issuer"] = "RaceDay.API",
                ["Jwt:Audience"] = "RaceDay.Client"
            })
            .Build();

        var jwtService = new JwtService(configuration);

        using var loggerFactory = LoggerFactory.Create(builder => { });
        var logger = loggerFactory.CreateLogger<AuthController>();

        var controller = new AuthController(
            context,
            passwordService,
            jwtService,
            logger);

        var request = new RegisterRequest
        {
            FirstName = "New",
            LastName = "Participant",
            Email = "missing.role@example.com",
            Password = "SecureTestPassword123!"
        };

        var result = await controller.Register(request);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            error.StatusCode);

        Assert.Empty(context.Users);
    }
}
