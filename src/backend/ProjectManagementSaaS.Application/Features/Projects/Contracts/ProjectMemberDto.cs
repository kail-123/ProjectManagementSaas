using ProjectManagementSaaS.Domain.Projects;

namespace ProjectManagementSaaS.Application.Features.Projects.Contracts;

public sealed record ProjectMemberDto(
    Guid Id,
    Guid ProjectId,
    Guid UserId,
    Guid? UserProfileId,
    string UserDisplayName,
    string EmployeeCode,
    string? Designation,
    ProjectMemberRole RoleInProject,
    DateTimeOffset JoinedDate,
    bool IsActive,
    string? AddedBy);
