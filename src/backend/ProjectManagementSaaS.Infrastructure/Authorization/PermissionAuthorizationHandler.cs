using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ProjectManagementSaaS.Application.Abstractions.Identity;
using ProjectManagementSaaS.Application.Common.Security;

namespace ProjectManagementSaaS.Infrastructure.Authorization;

internal sealed class PermissionAuthorizationHandler(IUserPermissionService userPermissionService)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue(ApplicationClaimTypes.UserId);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return;
        }

        var permissions = await userPermissionService.GetPermissionCodesAsync(userId, CancellationToken.None);
        if (permissions.Contains(requirement.Permission, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }
    }
}
