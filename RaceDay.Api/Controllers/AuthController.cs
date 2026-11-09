using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Authorization;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayContext _context;
    private readonly PasswordHasher<User> _hasher = new();

    public AuthController(RaceDayContext context)
    {
        _context = context;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Name, email, and password are required." });
        }

        // Role is validated against the fixed Roles table rather than
        // trusted blindly - a user can only ever become an Organiser or
        // a Participant, never anything else, since those are the only
        // two rows that exist and there is no endpoint to create more.
        if (request.Role != "Organiser" && request.Role != "Participant")
        {
            return BadRequest(new { message = "Role must be either 'Organiser' or 'Participant'." });
        }

        var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existing != null)
        {
            return Conflict(new { message = "A user with this email is already registered." });
        }

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.Role);
        if (role == null)
        {
            return BadRequest(new { message = "Invalid role specified." });
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            RoleId = role.Id
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return StatusCode(201, new { user.Id, user.Name, user.Email, Role = role.Name });
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var verification = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // Session management: the user's id, role, and name are stored in
        // server-side session state. The session cookie is sent back to
        // the browser automatically and must be included on every
        // subsequent request for the user to stay authenticated.
        HttpContext.Session.SetInt32(SessionKeys.UserId, user.Id);
        HttpContext.Session.SetString(SessionKeys.UserRole, user.Role.Name);
        HttpContext.Session.SetString(SessionKeys.UserName, user.Name);

        return Ok(new
        {
            message = "Login successful.",
            user = new { user.Id, user.Name, user.Email, Role = user.Role.Name }
        });
    }

    // POST /api/auth/logout
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logged out successfully." });
    }
}
