using ProjectManagementSaaS.Domain.Projects;

namespace ProjectManagementSaaS.Application.Abstractions.Projects;

public interface IProjectAccessService
{
    bool CanAccessAllProjects { get; }

    Guid CurrentUserId { get; }

    IQueryable<Project> ApplyProjectFilter(IQueryable<Project> query);

    Task<IReadOnlyCollection<Guid>> GetAccessibleProjectIdsAsync(CancellationToken cancellationToken);

    Task EnsureCanAccessProjectAsync(Guid projectId, CancellationToken cancellationToken);

    Task EnsureCanAccessProjectsAsync(IReadOnlyCollection<Guid> projectIds, CancellationToken cancellationToken);
}
