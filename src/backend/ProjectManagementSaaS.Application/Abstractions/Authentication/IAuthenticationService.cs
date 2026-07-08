using ProjectManagementSaaS.Application.Features.Authentication.Contracts;

namespace ProjectManagementSaaS.Application.Abstractions.Authentication;

public interface IAuthenticationService
{
    Task<AuthenticationSession> SignInAsync(
        SignInRequest request,
        string ipAddress,
        CancellationToken cancellationToken);

    Task<AuthenticationSession> RefreshSessionAsync(
        RefreshTokenRequest request,
        string ipAddress,
        CancellationToken cancellationToken);

    Task RevokeRefreshTokenAsync(
        RevokeRefreshTokenRequest request,
        string ipAddress,
        CancellationToken cancellationToken);

    Task<CurrentUserResponse> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
