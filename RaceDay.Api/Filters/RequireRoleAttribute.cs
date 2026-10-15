using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Filters;

/// <summary>
/// Blocks a request unless the user is logged in (401) and, if roles are listed,
/// has one of those roles (403).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public RequireRoleAttribute(params string[] roles) => _roles = roles;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;
        var userId = session.GetInt32("UserId");
        var role = session.GetString("Role");

        if (userId == null || role == null)
        {
            context.Result = new UnauthorizedObjectResult("You must be logged in.");
            return;
        }

        if (_roles.Length > 0 && !_roles.Contains(role))
        {
            context.Result = new ObjectResult("You do not have permission for this action.")
            {
                StatusCode = 403
            };
        }
    }
}