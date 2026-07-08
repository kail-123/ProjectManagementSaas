namespace ProjectManagementSaaS.Infrastructure.Authentication;

internal sealed record JwtToken(string Value, DateTimeOffset ExpiresAtUtc);
