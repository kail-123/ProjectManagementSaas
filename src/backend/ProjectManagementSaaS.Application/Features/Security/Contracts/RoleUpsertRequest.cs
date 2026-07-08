namespace ProjectManagementSaaS.Application.Features.Security.Contracts;

public sealed record RoleUpsertRequest(string Name, string? Description);
