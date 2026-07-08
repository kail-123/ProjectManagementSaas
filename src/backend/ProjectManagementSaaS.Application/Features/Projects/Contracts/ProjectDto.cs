using ProjectManagementSaaS.Domain.Projects;

namespace ProjectManagementSaaS.Application.Features.Projects.Contracts;

public sealed record ProjectDto(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    Guid? ClientId,
    string? ClientName,
    Guid? ProjectManagerUserProfileId,
    string? ProjectManagerName,
    string Name,
    string Code,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    decimal? EstimatedBudget,
    ProjectStatus Status,
    ProjectPriority Priority,
    ProjectVisibility Visibility,
    IReadOnlyCollection<Guid> TeamIds,
    int MemberCount,
    string? CreatedBy,
    DateTimeOffset CreatedOn,
    string? UpdatedBy,
    DateTimeOffset? UpdatedOn);
