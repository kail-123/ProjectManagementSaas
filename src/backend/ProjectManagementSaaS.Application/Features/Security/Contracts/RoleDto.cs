namespace ProjectManagementSaaS.Application.Features.Security.Contracts;

public sealed record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    IReadOnlyCollection<PermissionDto> Permissions);
