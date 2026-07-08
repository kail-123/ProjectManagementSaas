using ProjectManagementSaaS.Application.Abstractions.Common;
using ProjectManagementSaaS.Application.Abstractions.Persistence;
using ProjectManagementSaaS.Application.Abstractions.Projects;
using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Security;
using ProjectManagementSaaS.Domain.Projects;

namespace ProjectManagementSaaS.Application.Features.Projects;

internal sealed class ProjectAccessService(
    IRepository<Project> projectRepository,
    IPaginationService paginationService,
    ICurrentUserService currentUserService)
    : IProjectAccessService
{
    private IReadOnlyCollection<Guid>? _accessibleProjectIds;

    public bool CanAccessAllProjects => currentUserService.IsInRole(ApplicationRoles.SystemAdministrator);

    public Guid CurrentUserId => currentUserService.UserId
        ?? throw new ApplicationUnauthorizedException("Authenticated user id claim is missing or invalid.");

    public IQueryable<Project> ApplyProjectFilter(IQueryable<Project> query)
    {
        if (CanAccessAllProjects)
        {
            return query;
        }

        var userId = CurrentUserId;
        return query.Where(project => project.Members.Any(member => member.UserId == userId && member.IsActive));
    }

    public async Task<IReadOnlyCollection<Guid>> GetAccessibleProjectIdsAsync(CancellationToken cancellationToken)
    {
        if (_accessibleProjectIds is not null)
        {
            return _accessibleProjectIds;
        }

        _accessibleProjectIds = await paginationService.ToListAsync(
            ApplyProjectFilter(projectRepository.Query()).Select(project => project.Id),
            cancellationToken);

        return _accessibleProjectIds;
    }

    public async Task EnsureCanAccessProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var exists = await paginationService.SingleOrDefaultAsync(
            projectRepository.Query()
                .Where(project => project.Id == projectId)
                .Select(project => (Guid?)project.Id),
            cancellationToken);

        if (!exists.HasValue)
        {
            throw new ApplicationNotFoundException("Project was not found.");
        }

        if (CanAccessAllProjects)
        {
            return;
        }

        var accessibleProjectIds = await GetAccessibleProjectIdsAsync(cancellationToken);
        if (!accessibleProjectIds.Contains(projectId))
        {
            throw new ApplicationForbiddenException("You do not have access to this project.");
        }
    }

    public async Task EnsureCanAccessProjectsAsync(IReadOnlyCollection<Guid> projectIds, CancellationToken cancellationToken)
    {
        var requestedProjectIds = projectIds.Distinct().ToArray();
        if (requestedProjectIds.Length == 0)
        {
            return;
        }

        var existingProjectIds = await paginationService.ToListAsync(
            projectRepository.Query()
                .Where(project => requestedProjectIds.Contains(project.Id))
                .Select(project => project.Id),
            cancellationToken);

        if (existingProjectIds.Count != requestedProjectIds.Length)
        {
            throw new ApplicationNotFoundException("One or more projects were not found.");
        }

        if (CanAccessAllProjects)
        {
            return;
        }

        var accessibleProjectIds = (await GetAccessibleProjectIdsAsync(cancellationToken)).ToHashSet();
        if (requestedProjectIds.Any(projectId => !accessibleProjectIds.Contains(projectId)))
        {
            throw new ApplicationForbiddenException("You do not have access to one or more selected projects.");
        }
    }
}
