using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;
using System.Security.Claims;
using RaceDay.API.DTOs.Events;

namespace RaceDay.API.Tests;

public class EventsControllerTests
{
    [Fact]
    public async Task GetEvents_ReturnsOkWithEvents()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayDbContext(options);

        context.Events.Add(new Event
        {
            EventID = 1,
            EventName = "Test Marathon",
            Event_Discrip = "A test running event",
            EventDate = new DateTime(2027, 5, 1),
            Distance = 42.20m,
            EventType = "Running",
            Location = "Johannesburg",
            OrganiserID = 1004
        });

        await context.SaveChangesAsync();

        var controller = new EventsController(context);

        // Act
        var result = await controller.GetEvents();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Single(context.Events);
    }

    [Fact]
    public async Task GetEventById_WhenEventDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayDbContext(options);

        var controller = new EventsController(context);

        // Act
        var result = await controller.GetEventById(9999);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }


    [Fact]
    public async Task GetEventById_WhenEventExists_ReturnsOk()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayDbContext(options);

        context.Events.Add(new Event
        {
            EventID = 1,
            EventName = "Test Marathon",
            Event_Discrip = "A test running event",
            EventDate = new DateTime(2027, 5, 1),
            Distance = 42.20m,
            EventType = "Running",
            Location = "Johannesburg",
            OrganiserID = 1004
        });

        await context.SaveChangesAsync();

        var controller = new EventsController(context);

        var result = await controller.GetEventById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.NotNull(okResult.Value);
    }


    [Fact]
    public async Task CreateEvent_WithValidRequest_CreatesEvent()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayDbContext(options);

        var controller = new EventsController(context);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                        new Claim(ClaimTypes.NameIdentifier, "1004"),
                        new Claim(ClaimTypes.Role, "Organiser")
                        },
                        "TestAuthentication"))
            }
        };

        var request = new CreateEventRequest
        {
            EventName = "Test Road Race",
            Event_Discrip = "Unit test event",
            EventDate = new DateTime(2027, 6, 15),
            Distance = 10.00m,
            EventType = "Running",
            Location = "Johannesburg"
        };

        var result = await controller.CreateEvent(request);

        var created = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, created.StatusCode);

        var savedEvent = await context.Events
            .SingleAsync(e => e.EventName == "Test Road Race");

        Assert.Equal(1004, savedEvent.OrganiserID);
        Assert.Equal(10.00m, savedEvent.Distance);
    }


    [Fact]
    public async Task UpdateEvent_WhenEventBelongsToAnotherOrganiser_ReturnsForbidden()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayDbContext(options);

        var eventItem = new Event
        {
            EventID = 1,
            EventName = "Original Event",
            Event_Discrip = "Original description",
            EventDate = new DateTime(2027, 5, 1),
            Distance = 10.00m,
            EventType = "Running",
            Location = "Johannesburg",
            OrganiserID = 2000
        };

        context.Events.Add(eventItem);
        await context.SaveChangesAsync();

        var controller = new EventsController(context);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                        new Claim(ClaimTypes.NameIdentifier, "1004"),
                        new Claim(ClaimTypes.Role, "Organiser")
                        },
                        "TestAuthentication"))
            }
        };

        var request = new UpdateEventRequest
        {
            EventName = "Changed Event",
            Event_Discrip = "Changed description",
            EventDate = new DateTime(2027, 6, 15),
            Distance = 21.00m,
            EventType = "Running",
            Location = "Pretoria"
        };

        var result = await controller.UpdateEvent(1, request);

        Assert.IsType<ForbidResult>(result);

        var unchangedEvent = await context.Events.SingleAsync(e => e.EventID == 1);
        Assert.Equal("Original Event", unchangedEvent.EventName);
        Assert.Equal(2000, unchangedEvent.OrganiserID);
    }


    [Fact]
    public async Task DeleteEvent_WhenEventBelongsToAnotherOrganiser_ReturnsForbidden()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayDbContext(options);

        context.Events.Add(new Event
        {
            EventID = 1,
            EventName = "Protected Event",
            Event_Discrip = "Owned by another organiser",
            EventDate = new DateTime(2027, 5, 1),
            Distance = 10.00m,
            EventType = "Running",
            Location = "Johannesburg",
            OrganiserID = 2000
        });

        await context.SaveChangesAsync();

        var controller = new EventsController(context);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                        new Claim(ClaimTypes.NameIdentifier, "1004"),
                        new Claim(ClaimTypes.Role, "Organiser")
                        },
                        "TestAuthentication"))
            }
        };

        var result = await controller.DeleteEvent(1);

        Assert.IsType<ForbidResult>(result);

        var remainingEvent = await context.Events
            .SingleOrDefaultAsync(e => e.EventID == 1);

        Assert.NotNull(remainingEvent);
        Assert.Equal("Protected Event", remainingEvent.EventName);
        Assert.Equal(2000, remainingEvent.OrganiserID);
    }


    [Fact]
    public async Task UpdateEvent_WhenEventBelongsToOrganiser_UpdatesEvent()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayDbContext(options);

        context.Events.Add(new Event
        {
            EventID = 1,
            EventName = "Original Event",
            Event_Discrip = "Original description",
            EventDate = new DateTime(2027, 5, 1),
            Distance = 10.00m,
            EventType = "Running",
            Location = "Johannesburg",
            OrganiserID = 1004
        });

        await context.SaveChangesAsync();

        var controller = new EventsController(context);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                        new Claim(ClaimTypes.NameIdentifier, "1004"),
                        new Claim(ClaimTypes.Role, "Organiser")
                        },
                        "TestAuthentication"))
            }
        };

        var request = new UpdateEventRequest
        {
            EventName = "Updated Event",
            Event_Discrip = "Updated description",
            EventDate = new DateTime(2027, 6, 15),
            Distance = 21.00m,
            EventType = "Running",
            Location = "Pretoria"
        };

        var result = await controller.UpdateEvent(1, request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var updatedEvent = await context.Events.SingleAsync(e => e.EventID == 1);

        Assert.Equal("Updated Event", updatedEvent.EventName);
        Assert.Equal("Updated description", updatedEvent.Event_Discrip);
        Assert.Equal(21.00m, updatedEvent.Distance);
        Assert.Equal("Pretoria", updatedEvent.Location);
        Assert.Equal(1004, updatedEvent.OrganiserID);
    }


    [Fact]
    public async Task DeleteEvent_WhenEventBelongsToOrganiser_DeletesEvent()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayDbContext(options);

        context.Events.Add(new Event
        {
            EventID = 1,
            EventName = "Event To Delete",
            Event_Discrip = "Temporary test event",
            EventDate = new DateTime(2027, 5, 1),
            Distance = 10.00m,
            EventType = "Running",
            Location = "Johannesburg",
            OrganiserID = 1004
        });

        await context.SaveChangesAsync();

        var controller = new EventsController(context);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                        new Claim(ClaimTypes.NameIdentifier, "1004"),
                        new Claim(ClaimTypes.Role, "Organiser")
                        },
                        "TestAuthentication"))
            }


        };

        var result = await controller.DeleteEvent(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var deletedEvent = await context.Events
            .SingleOrDefaultAsync(e => e.EventID == 1);

        Assert.Null(deletedEvent);
    }


    [Fact]
    public async Task UpdateEvent_WhenEventDoesNotExist_ReturnsNotFound()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayDbContext(options);

        var controller = new EventsController(context);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                        new Claim(ClaimTypes.NameIdentifier, "1004"),
                        new Claim(ClaimTypes.Role, "Organiser")
                        },
                        "TestAuthentication"))
            }
        };

        var request = new UpdateEventRequest
        {
            EventName = "Updated Event",
            Event_Discrip = "Updated description",
            EventDate = new DateTime(2027, 6, 15),
            Distance = 21.00m,
            EventType = "Running",
            Location = "Pretoria"
        };

        var result = await controller.UpdateEvent(9999, request);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }


    [Fact]
    public async Task DeleteEvent_WhenEventDoesNotExist_ReturnsNotFound()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayDbContext(options);

        var controller = new EventsController(context);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                        new Claim(ClaimTypes.NameIdentifier, "1004"),
                        new Claim(ClaimTypes.Role, "Organiser")
                        },
                        "TestAuthentication"))
            }
        };

        var result = await controller.DeleteEvent(9999);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }


    [Fact]
    public async Task CreateEvent_WhenOrganiserIdClaimIsMissing_ReturnsUnauthorized()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayDbContext(options);

        var controller = new EventsController(context);

        // Authenticated identity, but no user ID claim.
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                        new Claim(ClaimTypes.Role, "Organiser")
                        },
                        "TestAuthentication"))
            }
        };

        var request = new CreateEventRequest
        {
            EventName = "Test Road Race",
            Event_Discrip = "Unit test event",
            EventDate = new DateTime(2027, 6, 15),
            Distance = 10.00m,
            EventType = "Running",
            Location = "Johannesburg"
        };

        var result = await controller.CreateEvent(request);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorized.StatusCode);

        Assert.Empty(context.Events);
    }


    [Fact]
    public async Task CreateEvent_WhenOrganiserIdClaimIsInvalid_ReturnsUnauthorized()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayDbContext(options);

        var controller = new EventsController(context);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                        new Claim(ClaimTypes.NameIdentifier, "abc"),
                        new Claim(ClaimTypes.Role, "Organiser")
                        },
                        "TestAuthentication"))
            }
        };

        var request = new CreateEventRequest
        {
            EventName = "Test Road Race",
            Event_Discrip = "Unit test event",
            EventDate = new DateTime(2027, 6, 15),
            Distance = 10.00m,
            EventType = "Running",
            Location = "Johannesburg"
        };

        var result = await controller.CreateEvent(request);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorized.StatusCode);
        Assert.Empty(context.Events);
    }
}