using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Results;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.API.Tests;

public class ResultsControllerTests
{
    private static RaceDayDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    return new RaceDayDbContext(options);
    }

    private static async Task SeedEnrolmentAsync(
        RaceDayDbContext context,
        int enrolmentId = 1)
    {
        context.Enrolments.Add(new Enrolment
        {
            EnrolmentID = enrolmentId,
            ParticipantID = 1005,
            EventID = 1,
            CategoryID = 1,
            EnrolmentDate = new DateTime(2026, 1, 10),
            Enrolment_status = "Confirmed"
        });

        await context.SaveChangesAsync();
    }

    private static async Task SeedResultAsync(
        RaceDayDbContext context)
    {
        await SeedEnrolmentAsync(context);

        context.Results.Add(new Result
        {
            ResultsID = 1,
            EnrolmentID = 1,
            FinishTime = new TimeSpan(0, 45, 32),
            FinishPosition = 1
        });

        await context.SaveChangesAsync();
    }

    // GET: api/Results

    [Fact]
    public async Task GetResults_WhenResultsExist_ReturnsOkWithResults()
    {
        using var context = CreateContext();
        await SeedResultAsync(context);

        var controller = new ResultsController(context);

        var response = await controller.GetResults();

        var okResult = Assert.IsType<OkObjectResult>(response);
        var results = Assert.IsAssignableFrom<IEnumerable>(okResult.Value);

        Assert.Single(results.Cast<object>());
    }

    [Fact]
    public async Task GetResults_WhenNoResultsExist_ReturnsEmptyList()
    {
        using var context = CreateContext();

        var controller = new ResultsController(context);

        var response = await controller.GetResults();

        var okResult = Assert.IsType<OkObjectResult>(response);
        var results = Assert.IsAssignableFrom<IEnumerable>(okResult.Value);

        Assert.Empty(results.Cast<object>());
    }

    // GET: api/Results/{id}

    [Fact]
    public async Task GetResultById_WhenResultExists_ReturnsOk()
    {
        using var context = CreateContext();
        await SeedResultAsync(context);

        var controller = new ResultsController(context);

        var response = await controller.GetResultById(1);

        Assert.IsType<OkObjectResult>(response);
    }

    [Fact]
    public async Task GetResultById_WhenResultDoesNotExist_ReturnsNotFound()
    {
        using var context = CreateContext();

        var controller = new ResultsController(context);

        var response = await controller.GetResultById(999);

        Assert.IsType<NotFoundObjectResult>(response);
    }

    // POST: api/Results

    [Fact]
    public async Task CreateResult_WithValidRequest_CreatesResult()
    {
        using var context = CreateContext();
        await SeedEnrolmentAsync(context);

        var controller = new ResultsController(context);

        var request = new CreateResultRequest
        {
            EnrolmentID = 1,
            FinishTime = "00:45:32",
            FinishPosition = 1
        };

        var response = await controller.CreateResult(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(response);

        Assert.Equal(nameof(ResultsController.GetResultById), createdResult.ActionName);

        var savedResult = await context.Results.SingleAsync();

        Assert.Equal(1, savedResult.EnrolmentID);
        Assert.Equal(new TimeSpan(0, 45, 32), savedResult.FinishTime);
        Assert.Equal(1, savedResult.FinishPosition);
    }

    [Fact]
    public async Task CreateResult_WhenFinishTimeIsInvalid_ReturnsBadRequest()
    {
        using var context = CreateContext();
        await SeedEnrolmentAsync(context);

        var controller = new ResultsController(context);

        var request = new CreateResultRequest
        {
            EnrolmentID = 1,
            FinishTime = "not-a-time",
            FinishPosition = 1
        };

        var response = await controller.CreateResult(request);

        Assert.IsType<BadRequestObjectResult>(response);
        Assert.Empty(await context.Results.ToListAsync());
    }

    [Fact]
    public async Task CreateResult_WhenFinishTimeExceedsOneDay_ReturnsBadRequest()
    {
        using var context = CreateContext();
        await SeedEnrolmentAsync(context);

        var controller = new ResultsController(context);

        var request = new CreateResultRequest
        {
            EnrolmentID = 1,
            FinishTime = "24:00:00",
            FinishPosition = 1
        };

        var response = await controller.CreateResult(request);

        Assert.IsType<BadRequestObjectResult>(response);
        Assert.Empty(await context.Results.ToListAsync());
    }

    [Fact]
    public async Task CreateResult_WhenEnrolmentDoesNotExist_ReturnsNotFound()
    {
        using var context = CreateContext();

        var controller = new ResultsController(context);

        var request = new CreateResultRequest
        {
            EnrolmentID = 999,
            FinishTime = "00:45:32",
            FinishPosition = 1
        };

        var response = await controller.CreateResult(request);

        Assert.IsType<NotFoundObjectResult>(response);
        Assert.Empty(await context.Results.ToListAsync());
    }

    [Fact]
    public async Task CreateResult_WhenResultAlreadyExists_ReturnsConflict()
    {
        using var context = CreateContext();
        await SeedResultAsync(context);

        var controller = new ResultsController(context);

        var request = new CreateResultRequest
        {
            EnrolmentID = 1,
            FinishTime = "00:50:00",
            FinishPosition = 2
        };

        var response = await controller.CreateResult(request);

        Assert.IsType<ConflictObjectResult>(response);
        Assert.Single(await context.Results.ToListAsync());
    }

}
