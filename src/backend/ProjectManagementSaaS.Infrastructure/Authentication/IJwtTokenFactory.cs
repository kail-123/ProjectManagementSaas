using ProjectManagementSaaS.Infrastructure.Persistence.Entities;

namespace ProjectManagementSaaS.Infrastructure.Authentication;

internal interface IJwtTokenFactory
{
    JwtToken CreateToken(
        ApplicationUser user,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions);
}
