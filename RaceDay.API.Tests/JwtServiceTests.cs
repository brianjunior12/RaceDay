
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using RaceDay.API.Models;
using RaceDay.API.Services;
using Xunit;

namespace RaceDay.API.Tests;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "Test-Only-RaceDay-Key-2026-Long-Enough!",
            ["Jwt:Issuer"] = "RaceDay.API",
            ["Jwt:Audience"] = "RaceDay.Client"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        _jwtService = new JwtService(configuration);
    }

    [Fact]
    public void GenerateToken_ReturnsExpectedUserDetails()
    {
        // Arrange
        var user = new User
        {
            UserId = 1004,
            Email = "raceday.organiser.test@example.com"
        };

        // Act
        var response = _jwtService.GenerateToken(user, "Organiser");

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal(1004, response.UserId);
        Assert.Equal(user.Email, response.Email);
        Assert.Equal("Organiser", response.Role);
        Assert.True(response.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public void GenerateToken_ContainsExpectedClaims()
    {
        // Arrange
        var user = new User
        {
            UserId = 1004,
            Email = "raceday.organiser.test@example.com"
        };

        // Act
        var response = _jwtService.GenerateToken(user, "Organiser");
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(response.Token);

        // Assert
        Assert.Equal("RaceDay.API", token.Issuer);
        Assert.Contains("RaceDay.Client", token.Audiences);
        Assert.Contains(token.Claims, c =>
            c.Type == ClaimTypes.NameIdentifier && c.Value == "1004");
        Assert.Contains(token.Claims, c =>
            c.Type == ClaimTypes.Role && c.Value == "Organiser");
        Assert.Contains(token.Claims, c =>
            c.Type == JwtRegisteredClaimNames.Email &&
            c.Value == user.Email);
    }

    [Fact]
    public void GenerateToken_WhenSigningKeyIsMissing_ThrowsException()
    {
        // Arrange
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "RaceDay.API",
                ["Jwt:Audience"] = "RaceDay.Client"
            })
            .Build();

        var service = new JwtService(configuration);
        var user = new User
        {
            UserId = 1004,
            Email = "raceday.organiser.test@example.com"
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(
            () => service.GenerateToken(user, "Organiser"));
    }

    [Fact]
    public void GenerateToken_WhenSigningKeyIsTooShort_ThrowsException()
    {
        // Arrange
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "too-short",
                ["Jwt:Issuer"] = "RaceDay.API",
                ["Jwt:Audience"] = "RaceDay.Client"
            })
            .Build();

        var service = new JwtService(configuration);
        var user = new User
        {
            UserId = 1004,
            Email = "raceday.organiser.test@example.com"
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(
            () => service.GenerateToken(user, "Organiser"));
    }
}