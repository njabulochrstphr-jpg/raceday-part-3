using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Authorization;

/// <summary>
/// Protects an endpoint using session state rather than token auth, as
/// required by the brief. With no arguments it just requires a logged-in
/// user. With role names passed in, it also requires the session's role
/// to be one of them - e.g. [RequireSession("Organiser")].
/// </summary>
public class RequireSessionAttribute : Attribute, IAsyncActionFilter
{
    private readonly string[] _allowedRoles;

    public RequireSessionAttribute(params string[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var session = context.HttpContext.Session;
        var userId = session.GetInt32(SessionKeys.UserId);

        if (userId == null)
        {
            context.Result = new ObjectResult(new { message = "You must be logged in to access this resource." })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        if (_allowedRoles.Length > 0)
        {
            var role = session.GetString(SessionKeys.UserRole);
            if (role == null || !_allowedRoles.Contains(role))
            {
                context.Result = new ObjectResult(new { message = "You do not have permission to access this resource." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }
        }

        await next();
    }
}
