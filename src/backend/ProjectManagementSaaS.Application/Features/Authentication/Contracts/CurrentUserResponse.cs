namespace ProjectManagementSaaS.Application.Features.Authentication.Contracts;

public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    string? DisplayName,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
