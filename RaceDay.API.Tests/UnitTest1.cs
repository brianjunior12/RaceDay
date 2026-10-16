
using RaceDay.API.Services;

namespace RaceDay.API.Tests;

public class PasswordServiceTests
{
    private readonly PasswordService _passwordService = new();

    [Fact]
    public void HashPassword_AndVerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "RaceDayTest2026!";
        var passwordHash = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(password, passwordHash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "RaceDayTest2026!";
        var passwordHash = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(
            "WrongPassword2026!",
            passwordHash);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("", "some-hash")]
    [InlineData("password", "")]
    [InlineData("", "")]
    public void VerifyPassword_WithEmptyInput_ReturnsFalse(
        string password,
        string passwordHash)
    {
        // Act
        var result = _passwordService.VerifyPassword(password, passwordHash);

        // Assert
        Assert.False(result);
    }
}