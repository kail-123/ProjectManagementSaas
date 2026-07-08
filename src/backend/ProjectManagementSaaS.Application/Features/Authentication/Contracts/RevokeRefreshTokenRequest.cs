namespace ProjectManagementSaaS.Application.Features.Authentication.Contracts;

public sealed record RevokeRefreshTokenRequest(string? RefreshToken);
