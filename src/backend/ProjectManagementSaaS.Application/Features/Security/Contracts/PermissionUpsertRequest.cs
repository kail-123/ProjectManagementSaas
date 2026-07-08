namespace ProjectManagementSaaS.Application.Features.Security.Contracts;

public sealed record PermissionUpsertRequest(
    string Module,
    string Name,
    string Code,
    string? Description,
    bool IsActive);
