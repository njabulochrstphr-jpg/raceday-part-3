using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Authorization;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public EnrolmentsController(RaceDayContext context)
    {
        _context = context;
    }

    // POST /api/event-categories/{eventCategoryId}/enrol
    [HttpPost("event-categories/{eventCategoryId}/enrol")]
    [RequireSession("Participant")]
    public async Task<IActionResult> Enrol(int eventCategoryId)
    {
        var participantId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;

        var eventCategory = await _context.EventCategories.FirstOrDefaultAsync(ec => ec.Id == eventCategoryId);
        if (eventCategory == null) return NotFound(new { message = "Event category not found." });

        var alreadyEnrolled = await _context.Enrolments
            .AnyAsync(e => e.ParticipantId == participantId && e.EventCategoryId == eventCategoryId);
        if (alreadyEnrolled) return Conflict(new { message = "You are already enrolled in this event category." });

        var enrolment = new Enrolment
        {
            ParticipantId = participantId,
            EventCategoryId = eventCategoryId
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        return StatusCode(201, new { enrolment.Id, message = "Enrolment successful." });
    }

    // DELETE /api/enrolments/{id}
    [HttpDelete("enrolments/{id}")]
    [RequireSession("Participant")]
    public async Task<IActionResult> Cancel(int id)
    {
        var participantId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
        var enrolment = await _context.Enrolments.FirstOrDefaultAsync(e => e.Id == id);

        if (enrolment == null) return NotFound(new { message = "Enrolment not found." });
        if (enrolment.ParticipantId != participantId) return Forbid();

        _context.Enrolments.Remove(enrolment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
