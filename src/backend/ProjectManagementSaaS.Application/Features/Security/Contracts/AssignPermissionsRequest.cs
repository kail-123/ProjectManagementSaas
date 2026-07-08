namespace ProjectManagementSaaS.Application.Features.Security.Contracts;

public sealed record AssignPermissionsRequest(IReadOnlyCollection<Guid> PermissionIds);
