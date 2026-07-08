namespace ProjectManagementSaaS.Application.Features.Organizations.Contracts;

public sealed record DepartmentDto(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    string? CreatedBy,
    DateTimeOffset CreatedOn,
    string? UpdatedBy,
    DateTimeOffset? UpdatedOn);
