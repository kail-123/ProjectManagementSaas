namespace ProjectManagementSaaS.Application.Features.Authentication.Contracts;

public sealed record AuthenticationSession(
    AuthenticationResponse Response,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc);
