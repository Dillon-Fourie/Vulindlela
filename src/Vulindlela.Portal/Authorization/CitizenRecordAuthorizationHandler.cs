
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Vulindlela.Portal.Authorization;

public class CitizenRecordAuthorizationHandler
    : AuthorizationHandler<CitizenRecordRequirement, string>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CitizenRecordRequirement requirement,
        string requestedCitizenId)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        if (context.User.IsInRole("Official"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var signedInCitizenId =
            context.User.FindFirstValue("CitizenId");

        if (context.User.IsInRole("Citizen") &&
            !string.IsNullOrWhiteSpace(signedInCitizenId) &&
            string.Equals(
                signedInCitizenId,
                requestedCitizenId,
                StringComparison.Ordinal))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
