namespace ProjectManagementSaaS.Application.Features.Users.Contracts;

public sealed record AssignRolesRequest(IReadOnlyCollection<string> Roles);
