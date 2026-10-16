
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Categories;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public CategoriesController(RaceDayDbContext context)
    {
        _context = context;
    }

    // 1. GET: api/Categories
    // Retrieve all categories
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .Select(c => new
            {
                c.CategoryID,
                c.CategoryName,
                c.CategoryType,
                c.EventID
            })
            .ToListAsync();

        return Ok(categories);
    }
  

    // 2. GET: api/Categories/{id}
    // Retrieve one category by ID
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Where(c => c.CategoryID == id)
            .Select(c => new
            {
                c.CategoryID,
                c.CategoryName,
                c.CategoryType,
                c.EventID
            })
            .FirstOrDefaultAsync();

        if (category is null)
        {
            return NotFound(new
            {
                message = $"Category with ID {id} was not found."
            });
        }

        return Ok(category);
    }

// 3. POST: api/Categories
// Create a category (Organiser only)
[HttpPost]
[Authorize(Roles = "Organiser")]
public async Task<IActionResult> CreateCategory(
    [FromBody] CreateCategoryRequest request)
    {
        var eventExists = await _context.Events
            .AnyAsync(e => e.EventID == request.EventID);

        if (!eventExists)
        {
            return NotFound(new
            {
                message = $"Event with ID {request.EventID} was not found."
            });
        }

        var category = new Category
        {
            CategoryName = request.CategoryName.Trim(),
            CategoryType = request.CategoryType.Trim(),
            EventID = request.EventID
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Category created successfully.",
            categoryId = category.CategoryID,
            categoryName = category.CategoryName,
            categoryType = category.CategoryType,
            eventId = category.EventID
        });
    }



}

