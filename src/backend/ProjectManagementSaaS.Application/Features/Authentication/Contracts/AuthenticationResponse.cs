namespace ProjectManagementSaaS.Application.Features.Authentication.Contracts;

public sealed record AuthenticationResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    CurrentUserResponse User);
