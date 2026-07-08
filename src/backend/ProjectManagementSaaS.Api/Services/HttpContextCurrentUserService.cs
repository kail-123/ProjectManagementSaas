using System.Security.Claims;
using ProjectManagementSaaS.Application.Abstractions.Common;
using ProjectManagementSaaS.Application.Common.Security;

namespace ProjectManagementSaaS.Api.Services;

internal sealed class HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User?.FindFirstValue(ApplicationClaimTypes.UserId);

            return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
        }
    }

    public string? UserName => User?.Identity?.Name
        ?? User?.FindFirstValue(ClaimTypes.Email);

    public IReadOnlyCollection<string> Roles =>
        User?.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.Ordinal)
            .ToArray()
        ?? Array.Empty<string>();

    public bool IsInRole(string roleName)
    {
        return User?.IsInRole(roleName) == true
            || Roles.Contains(roleName, StringComparer.Ordinal);
    }
}
