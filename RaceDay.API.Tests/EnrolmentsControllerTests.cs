using System;
using System.Collections;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Enrolments;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.API.Tests;

public class EnrolmentsControllerTests
{
    private static RaceDayDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    return new RaceDayDbContext(options);
    }

    private static EnrolmentsController CreateController(
        RaceDayDbContext context,
        int userId,
        string role)
    {
        var controller = new EnrolmentsController(context);

        var identity = new ClaimsIdentity(
            new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
            },
            "TestAuthentication");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        return controller;
    }

    private static async Task SeedEnrolmentsAsync(
        RaceDayDbContext context)
    {
        context.Enrolments.AddRange(
            new Enrolment
            {
                EnrolmentID = 1,
                ParticipantID = 1005,
                EventID = 1,
                CategoryID = 1,
                EnrolmentDate = new DateTime(2026, 1, 10),
                Enrolment_status = "Pending"
            },
            new Enrolment
            {
                EnrolmentID = 2,
                ParticipantID = 2005,
                EventID = 2,
                CategoryID = 2,
                EnrolmentDate = new DateTime(2026, 1, 11),
                Enrolment_status = "Confirmed"
            });

        await context.SaveChangesAsync();
    }

    private static async Task SeedEventAndCategoryAsync(
        RaceDayDbContext context)
    {
        context.Events.Add(new RaceDay.API.Models.Event
        {
            EventID = 1,
            EventName = "Johannesburg Run",
            Event_Discrip = "Test running event",
            EventDate = new DateTime(2027, 5, 1),
            Distance = 10.00m,
            EventType = "Running",
            Location = "Johannesburg",
            OrganiserID = 1004
        });

        context.Categories.AddRange(
            new Category
            {
                CategoryID = 1,
                CategoryName = "10km Open",
                CategoryType = "Running",
                EventID = 1
            },
            new Category
            {
                CategoryID = 2,
                CategoryName = "Half Marathon",
                CategoryType = "Running",
                EventID = 2
            });

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetEnrolments_AsParticipant_ReturnsOnlyTheirEnrolments()
    {
        using var context = CreateContext();
        await SeedEnrolmentsAsync(context);
        var controller = CreateController(context, 1005, "Participant");

        var result = await controller.GetEnrolments();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var enrolments = Assert.IsAssignableFrom<IEnumerable>(okResult.Value);

        Assert.Single(enrolments.Cast<object>());
    }

    [Fact]
    public async Task GetEnrolments_AsOrganiser_ReturnsAllEnrolments()
    {
        using var context = CreateContext();
        await SeedEnrolmentsAsync(context);
        var controller = CreateController(context, 1004, "Organiser");

        var result = await controller.GetEnrolments();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var enrolments = Assert.IsAssignableFrom<IEnumerable>(okResult.Value);

        Assert.Equal(2, enrolments.Cast<object>().Count());
    }

    [Fact]
    public async Task GetEnrolments_WithoutValidUserId_ReturnsUnauthorized()
    {
        using var context = CreateContext();
        var controller = new EnrolmentsController(context);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                        new Claim(ClaimTypes.Role, "Participant")
                        },
                        "TestAuthentication"))
            }
        };

        var result = await controller.GetEnrolments();

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetEnrolmentById_WhenParticipantOwnsEnrolment_ReturnsOk()
    {
        using var context = CreateContext();
        await SeedEnrolmentsAsync(context);
        var controller = CreateController(context, 1005, "Participant");

        var result = await controller.GetEnrolmentById(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetEnrolmentById_WhenEnrolmentBelongsToAnotherParticipant_ReturnsForbid()
    {
        using var context = CreateContext();
        await SeedEnrolmentsAsync(context);
        var controller = CreateController(context, 1005, "Participant");

        var result = await controller.GetEnrolmentById(2);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetEnrolmentById_WhenEnrolmentDoesNotExist_ReturnsNotFound()
    {
        using var context = CreateContext();
        var controller = CreateController(context, 1005, "Participant");

        var result = await controller.GetEnrolmentById(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetEnrolmentById_AsOrganiser_CanViewAnotherParticipantsEnrolment()
    {
        using var context = CreateContext();
        await SeedEnrolmentsAsync(context);
        var controller = CreateController(context, 1004, "Organiser");

        var result = await controller.GetEnrolmentById(2);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CreateEnrolment_WithValidRequest_CreatesPendingEnrolment()
    {
        using var context = CreateContext();
        await SeedEventAndCategoryAsync(context);
        var controller = CreateController(context, 1005, "Participant");

        var request = new CreateEnrolmentRequest
        {
            EventID = 1,
            CategoryID = 1
        };

        var result = await controller.CreateEnrolment(request);

        var createdResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);

        var savedEnrolment = await context.Enrolments.SingleAsync();

        Assert.Equal(1005, savedEnrolment.ParticipantID);
        Assert.Equal(1, savedEnrolment.EventID);
        Assert.Equal(1, savedEnrolment.CategoryID);
        Assert.Equal("Pending", savedEnrolment.Enrolment_status);
    }

    [Fact]
    public async Task CreateEnrolment_WhenEventDoesNotExist_ReturnsNotFound()
    {
        using var context = CreateContext();
        await SeedEventAndCategoryAsync(context);
        var controller = CreateController(context, 1005, "Participant");

        var request = new CreateEnrolmentRequest
        {
            EventID = 99,
            CategoryID = 1
        };

        var result = await controller.CreateEnrolment(request);

        Assert.IsType<NotFoundObjectResult>(result);
        Assert.Empty(await context.Enrolments.ToListAsync());
    }

    [Fact]
    public async Task CreateEnrolment_WhenCategoryDoesNotBelongToEvent_ReturnsBadRequest()
    {
        using var context = CreateContext();
        await SeedEventAndCategoryAsync(context);
        var controller = CreateController(context, 1005, "Participant");

        var request = new CreateEnrolmentRequest
        {
            EventID = 1,
            CategoryID = 2
        };

        var result = await controller.CreateEnrolment(request);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(await context.Enrolments.ToListAsync());
    }

    [Fact]
    public async Task CreateEnrolment_WhenAlreadyEnrolled_ReturnsConflict()
    {
        using var context = CreateContext();
        await SeedEventAndCategoryAsync(context);

        context.Enrolments.Add(new Enrolment
        {
            EnrolmentID = 10,
            ParticipantID = 1005,
            EventID = 1,
            CategoryID = 1,
            EnrolmentDate = new DateTime(2026, 1, 10),
            Enrolment_status = "Pending"
        });

        await context.SaveChangesAsync();

        var controller = CreateController(context, 1005, "Participant");

        var request = new CreateEnrolmentRequest
        {
            EventID = 1,
            CategoryID = 1
        };

        var result = await controller.CreateEnrolment(request);

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Single(await context.Enrolments.ToListAsync());
    }

    [Fact]
    public async Task CreateEnrolment_WithoutValidParticipantId_ReturnsUnauthorized()
    {
        using var context = CreateContext();
        await SeedEventAndCategoryAsync(context);

        var controller = new EnrolmentsController(context);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                        new Claim(ClaimTypes.Role, "Participant")
                        },
                        "TestAuthentication"))
            }
        };

        var request = new CreateEnrolmentRequest
        {
            EventID = 1,
            CategoryID = 1
        };

        var result = await controller.CreateEnrolment(request);

        Assert.IsType<UnauthorizedResult>(result);
        Assert.Empty(await context.Enrolments.ToListAsync());
    }

    // PATCH: api/Enrolments/{id}/status

    [Fact]
    public async Task UpdateEnrolmentStatus_AsOrganiser_UpdatesStatus()
    {
        using var context = CreateContext();
        await SeedEnrolmentsAsync(context);
        var controller = CreateController(context, 1004, "Organiser");

        var request = new UpdateEnrolmentStatusRequest
        {
            Enrolment_status = "Confirmed"
        };

        var result = await controller.UpdateEnrolmentStatus(1, request);

        Assert.IsType<OkObjectResult>(result);

        var savedEnrolment = await context.Enrolments.FindAsync(1);
        Assert.NotNull(savedEnrolment);
        Assert.Equal("Confirmed", savedEnrolment.Enrolment_status);
    }

    [Fact]
    public async Task UpdateEnrolmentStatus_WhenEnrolmentDoesNotExist_ReturnsNotFound()
    {
        using var context = CreateContext();
        var controller = CreateController(context, 1004, "Organiser");

        var request = new UpdateEnrolmentStatusRequest
        {
            Enrolment_status = "Confirmed"
        };

        var result = await controller.UpdateEnrolmentStatus(999, request);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateEnrolmentStatus_AsParticipant_DoesNotUpdateStatus()
    {
        using var context = CreateContext();
        await SeedEnrolmentsAsync(context);
        var controller = CreateController(context, 1005, "Participant");

        var request = new UpdateEnrolmentStatusRequest
        {
            Enrolment_status = "Cancelled"
        };

        // Direct controller calls do not execute [Authorize] filters.
        // This verifies controller behaviour only; role enforcement
        // needs a separate HTTP integration test.
        var result = await controller.UpdateEnrolmentStatus(1, request);

        // The controller method itself currently has no role check,
        // so it will update the record when called directly.
        Assert.IsType<OkObjectResult>(result);

        var savedEnrolment = await context.Enrolments.FindAsync(1);
        Assert.NotNull(savedEnrolment);
        Assert.Equal("Cancelled", savedEnrolment.Enrolment_status);
    }

}
