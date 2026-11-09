using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Authorization;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayContext _context;

    public CategoriesController(RaceDayContext context)
    {
        _context = context;
    }

    // GET /api/categories
    [HttpGet("categories")]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _context.Categories.ToListAsync();
        return Ok(categories.Select(c => new CategoryResponse(c.Id, c.Name)));
    }

    // POST /api/categories
    [HttpPost("categories")]
    [RequireSession("Organiser")]
    public async Task<IActionResult> Create(CreateCategoryRequest request)
    {
        var exists = await _context.Categories.AnyAsync(c => c.Name == request.Name);
        if (exists) return Conflict(new { message = "A category with this name already exists." });

        var category = new Category { Name = request.Name };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return StatusCode(201, new CategoryResponse(category.Id, category.Name));
    }

    // GET /api/events/{eventId}/categories
    [HttpGet("events/{eventId}/categories")]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        var exists = await _context.Events.AnyAsync(e => e.Id == eventId);
        if (!exists) return NotFound(new { message = "Event not found." });

        var categories = await _context.EventCategories
            .Include(ec => ec.Category)
            .Where(ec => ec.EventId == eventId)
            .ToListAsync();

        var response = categories.Select(ec => new EventCategoryResponse(ec.Id, ec.Category.Id, ec.Category.Name));
        return Ok(response);
    }

    // POST /api/events/{eventId}/categories
    [HttpPost("events/{eventId}/categories")]
    [RequireSession("Organiser")]
    public async Task<IActionResult> AttachToEvent(int eventId, AttachCategoryRequest request)
    {
        var organiserId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);

        if (ev == null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != organiserId) return Forbid();

        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId);
        if (category == null) return NotFound(new { message = "Category not found." });

        var alreadyLinked = await _context.EventCategories
            .AnyAsync(ec => ec.EventId == eventId && ec.CategoryId == request.CategoryId);
        if (alreadyLinked) return Conflict(new { message = "This category is already attached to the event." });

        var link = new EventCategory { EventId = eventId, CategoryId = request.CategoryId };
        _context.EventCategories.Add(link);
        await _context.SaveChangesAsync();

        return StatusCode(201, new EventCategoryResponse(link.Id, category.Id, category.Name));
    }
}
