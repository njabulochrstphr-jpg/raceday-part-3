using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Authorization;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public EventsController(RaceDayContext context)
    {
        _context = context;
    }

    // GET /api/events
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var events = await _context.Events.Include(e => e.Organiser).ToListAsync();
        return Ok(events.Select(MapToResponse));
    }

    // GET /api/events/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _context.Events
            .Include(e => e.Organiser)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev == null) return NotFound(new { message = "Event not found." });
        return Ok(MapToResponse(ev));
    }

    // POST /api/events
    [HttpPost]
    [RequireSession("Organiser")]
    public async Task<IActionResult> Create(CreateEventRequest request)
    {
        var organiserId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;

        var ev = new Event
        {
            OrganiserId = organiserId,
            Title = request.Title,
            Description = request.Description,
            EventDate = request.EventDate,
            Location = request.Location,
            Distance = request.Distance,
            EventType = request.EventType,
            Status = "Scheduled"
        };

        _context.Events.Add(ev);
        await _context.SaveChangesAsync();
        await _context.Entry(ev).Reference(e => e.Organiser).LoadAsync();

        return StatusCode(201, MapToResponse(ev));
    }

    // PUT /api/events/{id}
    [HttpPut("{id}")]
    [RequireSession("Organiser")]
    public async Task<IActionResult> Update(int id, UpdateEventRequest request)
    {
        var organiserId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
        var ev = await _context.Events.Include(e => e.Organiser).FirstOrDefaultAsync(e => e.Id == id);

        if (ev == null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != organiserId) return Forbid();

        if (request.Title != null) ev.Title = request.Title;
        if (request.Description != null) ev.Description = request.Description;
        if (request.EventDate.HasValue) ev.EventDate = request.EventDate.Value;
        if (request.Location != null) ev.Location = request.Location;
        if (request.Distance.HasValue) ev.Distance = request.Distance.Value;
        if (request.EventType != null) ev.EventType = request.EventType;
        if (request.Status != null) ev.Status = request.Status;

        await _context.SaveChangesAsync();
        return Ok(MapToResponse(ev));
    }

    // DELETE /api/events/{id}
    [HttpDelete("{id}")]
    [RequireSession("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var organiserId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);

        if (ev == null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != organiserId) return Forbid();

        _context.Events.Remove(ev);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // GET /api/events/{id}/enrolments
    [HttpGet("{id}/enrolments")]
    [RequireSession("Organiser")]
    public async Task<IActionResult> GetEnrolmentsForEvent(int id)
    {
        var organiserId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);

        if (ev == null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != organiserId) return Forbid();

        var enrolments = await _context.Enrolments
            .Include(en => en.Participant)
            .Include(en => en.EventCategory).ThenInclude(ec => ec.Category)
            .Where(en => en.EventCategory.EventId == id)
            .ToListAsync();

        var response = enrolments.Select(en => new
        {
            en.Id,
            Participant = en.Participant.Name,
            Category = en.EventCategory.Category.Name,
            en.EnrolmentDate,
            en.Status
        });

        return Ok(response);
    }

    // GET /api/events/{id}/results
    [HttpGet("{id}/results")]
    public async Task<IActionResult> GetResultsForEvent(int id)
    {
        var exists = await _context.Events.AnyAsync(e => e.Id == id);
        if (!exists) return NotFound(new { message = "Event not found." });

        var results = await _context.Results
            .Include(r => r.Enrolment).ThenInclude(e => e.Participant)
            .Include(r => r.Enrolment).ThenInclude(e => e.EventCategory).ThenInclude(ec => ec.Category)
            .Where(r => r.Enrolment.EventCategory.EventId == id)
            .OrderBy(r => r.Position)
            .ToListAsync();

        var response = results.Select(r => new
        {
            r.Id,
            Participant = r.Enrolment.Participant.Name,
            Category = r.Enrolment.EventCategory.Category.Name,
            r.FinishTime,
            r.Position
        });

        return Ok(response);
    }

    private static EventResponse MapToResponse(Event ev) => new(
        ev.Id,
        ev.Title,
        ev.Description,
        ev.EventDate,
        ev.Location,
        ev.Distance,
        ev.EventType,
        ev.Status,
        ev.Organiser?.Name ?? string.Empty);
}
