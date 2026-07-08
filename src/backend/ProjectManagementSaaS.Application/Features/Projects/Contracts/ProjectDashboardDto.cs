using ProjectManagementSaaS.Domain.Projects;

namespace ProjectManagementSaaS.Application.Features.Projects.Contracts;

public sealed record ProjectDashboardDto(
    ProjectSummaryDto Summary,
    int MemberCount,
    int TeamCount,
    int TaskCount,
    decimal CompletionPercentage,
    IReadOnlyCollection<ProjectDeadlineDto> UpcomingDeadlines,
    IReadOnlyCollection<ProjectActivityDto> Activity,
    IReadOnlyCollection<RecentProjectMemberDto> RecentMembers);

public sealed record ProjectSummaryDto(
    Guid Id,
    string Name,
    string Code,
    ProjectStatus Status,
    ProjectPriority Priority,
    DateOnly? StartDate,
    DateOnly? EndDate,
    decimal? EstimatedBudget,
    string OrganizationName,
    string? ClientName,
    string? ProjectManagerName);

public sealed record ProjectDeadlineDto(
    string Title,
    DateOnly DueDate,
    string Category);

public sealed record ProjectActivityDto(
    string EntityName,
    string Action,
    string ChangedBy,
    DateTimeOffset ChangedOn);

public sealed record RecentProjectMemberDto(
    Guid UserId,
    string DisplayName,
    ProjectMemberRole RoleInProject,
    DateTimeOffset JoinedDate);
