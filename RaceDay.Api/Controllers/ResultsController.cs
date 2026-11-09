using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Authorization;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public ResultsController(RaceDayContext context)
    {
        _context = context;
    }

    // POST /api/enrolments/{enrolmentId}/result
    [HttpPost("enrolments/{enrolmentId}/result")]
    [RequireSession("Organiser")]
    public async Task<IActionResult> Create(int enrolmentId, CreateResultRequest request)
    {
        var organiserId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;

        var enrolment = await _context.Enrolments
            .Include(e => e.EventCategory).ThenInclude(ec => ec.Event)
            .Include(e => e.EventCategory).ThenInclude(ec => ec.Category)
            .FirstOrDefaultAsync(e => e.Id == enrolmentId);

        if (enrolment == null) return NotFound(new { message = "Enrolment not found." });
        if (enrolment.EventCategory.Event.OrganiserId != organiserId) return Forbid();

        var existing = await _context.Results.AnyAsync(r => r.EnrolmentId == enrolmentId);
        if (existing) return Conflict(new { message = "A result has already been recorded for this enrolment." });

        var result = new Result
        {
            EnrolmentId = enrolmentId,
            FinishTime = request.FinishTime,
            Position = request.Position
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        var response = new ResultResponse(
            result.Id,
            enrolment.EventCategory.Event.Title,
            enrolment.EventCategory.Category.Name,
            result.FinishTime,
            result.Position);

        return StatusCode(201, response);
    }
}
