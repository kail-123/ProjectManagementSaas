namespace ProjectManagementSaaS.Application.Features.Organizations.Contracts;

public sealed record DepartmentUpsertRequest(
    Guid OrganizationId,
    string Name,
    string Code,
    string? Description,
    bool IsActive);
