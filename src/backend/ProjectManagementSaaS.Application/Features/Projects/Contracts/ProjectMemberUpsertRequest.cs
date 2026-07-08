using ProjectManagementSaaS.Domain.Projects;

namespace ProjectManagementSaaS.Application.Features.Projects.Contracts;

public sealed record ProjectMemberUpsertRequest(
    IReadOnlyCollection<Guid> UserIds,
    ProjectMemberRole RoleInProject);
