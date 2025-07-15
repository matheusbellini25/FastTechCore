using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FastTech.Api.Authorization;

public class RolesAuthorizationHandler : AuthorizationHandler<RolesRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesRequirement requirement)
    {
        var roleClaim = context.User.FindFirst(c => c.Type == ClaimTypes.Role)?.Value;
        Console.WriteLine($"roleClaim: {roleClaim}, required: {(int)requirement.Permission}");

        if (!string.IsNullOrEmpty(roleClaim) && int.TryParse(roleClaim, out int roleValue))
        {
            if (roleValue == (int)requirement.Permission)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }

}
