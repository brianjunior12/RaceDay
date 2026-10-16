
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Tests;

public class RaceDayWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestJwtKey =
        "RaceDay-Integration-Tests-Key-2026-Use-32-Bytes-Minimum!";

    private readonly string _databaseName =
        $"RaceDayIntegrationTests_{Guid.NewGuid():N}";

    public RaceDayWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__Key", TestJwtKey);
        Environment.SetEnvironmentVariable("Jwt__Issuer", "RaceDay.API");
        Environment.SetEnvironmentVariable("Jwt__Audience", "RaceDay.Client");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<RaceDayDbContext>();
            services.RemoveAll<DbContextOptions<RaceDayDbContext>>();

            services.AddDbContext<RaceDayDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<RaceDayDbContext>();

        var passwordService = scope.ServiceProvider
            .GetRequiredService<PasswordService>();

        await db.Database.EnsureCreatedAsync();

        if (await db.Roles.AnyAsync())
            return;

        db.Roles.AddRange(
            new Role { RoleID = 1, RoleName = "Organiser" },
            new Role { RoleID = 2, RoleName = "Participant" });

        db.Users.AddRange(
            new User
            {
                UserId = 1001,
                FirstName = "Test",
                LastName = "Organiser",
                Email = "organiser.integration@example.com",
                PasswordHash = passwordService.HashPassword("OrganiserTest2026!"),
                RoleID = 1
            },
            new User
            {
                UserId = 1002,
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant.integration@example.com",
                PasswordHash = passwordService.HashPassword("ParticipantTest2026!"),
                RoleID = 2
            });

        await db.SaveChangesAsync();
    }
}
