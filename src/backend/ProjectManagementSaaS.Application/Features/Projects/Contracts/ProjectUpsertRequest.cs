using ProjectManagementSaaS.Domain.Projects;

namespace ProjectManagementSaaS.Application.Features.Projects.Contracts;

public sealed record ProjectUpsertRequest(
    Guid OrganizationId,
    Guid? ClientId,
    Guid? ProjectManagerUserProfileId,
    string Name,
    string Code,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    decimal? EstimatedBudget,
    ProjectStatus Status,
    ProjectPriority Priority,
    ProjectVisibility Visibility,
    IReadOnlyCollection<Guid> TeamIds);
