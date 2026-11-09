using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Authorization;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly RaceDayContext _context;

    public UsersController(RaceDayContext context)
    {
        _context = context;
    }

    // GET /api/users/me
    [HttpGet("me")]
    [RequireSession]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();

        return Ok(new UserProfileResponse(user.Id, user.Name, user.Email, user.Role.Name));
    }

    // PUT /api/users/me
    [HttpPut("me")]
    [RequireSession]
    public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request)
    {
        var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Name)) user.Name = request.Name;
        if (!string.IsNullOrWhiteSpace(request.Email)) user.Email = request.Email;

        await _context.SaveChangesAsync();
        return Ok(new UserProfileResponse(user.Id, user.Name, user.Email, user.Role.Name));
    }

    // GET /api/users/me/enrolments
    [HttpGet("me/enrolments")]
    [RequireSession("Participant")]
    public async Task<IActionResult> GetMyEnrolments()
    {
        var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;

        var enrolments = await _context.Enrolments
            .Include(e => e.EventCategory).ThenInclude(ec => ec.Event)
            .Include(e => e.EventCategory).ThenInclude(ec => ec.Category)
            .Where(e => e.ParticipantId == userId)
            .ToListAsync();

        var response = enrolments.Select(e => new EnrolmentResponse(
            e.Id, e.EventCategory.Event.Title, e.EventCategory.Category.Name, e.EnrolmentDate, e.Status));

        return Ok(response);
    }

    // GET /api/users/me/results
    [HttpGet("me/results")]
    [RequireSession("Participant")]
    public async Task<IActionResult> GetMyResults()
    {
        var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;

        var results = await _context.Results
            .Include(r => r.Enrolment).ThenInclude(e => e.EventCategory).ThenInclude(ec => ec.Event)
            .Include(r => r.Enrolment).ThenInclude(e => e.EventCategory).ThenInclude(ec => ec.Category)
            .Where(r => r.Enrolment.ParticipantId == userId)
            .ToListAsync();

        var response = results.Select(r => new ResultResponse(
            r.Id,
            r.Enrolment.EventCategory.Event.Title,
            r.Enrolment.EventCategory.Category.Name,
            r.FinishTime,
            r.Position));

        return Ok(response);
    }
}
