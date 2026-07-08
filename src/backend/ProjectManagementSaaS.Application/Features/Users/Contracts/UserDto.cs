namespace ProjectManagementSaaS.Application.Features.Users.Contracts;

public sealed record UserDto(
    Guid Id,
    string Email,
    string? DisplayName,
    bool IsActive,
    IReadOnlyCollection<string> Roles,
    UserProfileDto? Profile);
