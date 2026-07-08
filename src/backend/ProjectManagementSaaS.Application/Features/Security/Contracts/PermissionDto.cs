namespace ProjectManagementSaaS.Application.Features.Security.Contracts;

public sealed record PermissionDto(
    Guid Id,
    string Module,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    string? CreatedBy,
    DateTimeOffset CreatedOn,
    string? UpdatedBy,
    DateTimeOffset? UpdatedOn);
