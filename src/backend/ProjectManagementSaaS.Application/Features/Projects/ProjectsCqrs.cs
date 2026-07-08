using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using ProjectManagementSaaS.Application.Abstractions.Common;
using ProjectManagementSaaS.Application.Abstractions.Notifications;
using ProjectManagementSaaS.Application.Abstractions.Persistence;
using ProjectManagementSaaS.Application.Abstractions.Projects;
using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Features.Projects.Contracts;
using ProjectManagementSaaS.Application.Features.Work;
using ProjectManagementSaaS.Domain.Audit;
using ProjectManagementSaaS.Domain.Notifications;
using ProjectManagementSaaS.Domain.Organizations;
using ProjectManagementSaaS.Domain.Projects;
using ProjectManagementSaaS.Domain.Work;

namespace ProjectManagementSaaS.Application.Features.Projects;

public sealed record ListProjectsQuery(PagedRequest Request, Guid? OrganizationId, Guid? ClientId, ProjectStatus? Status) : IRequest<PagedResult<ProjectDto>>;

public sealed record ListMyProjectsQuery(PagedRequest Request, Guid? OrganizationId, Guid? ClientId, ProjectStatus? Status) : IRequest<PagedResult<ProjectDto>>;

public sealed record GetProjectByIdQuery(Guid Id) : IRequest<ProjectDto>;

public sealed record CreateProjectCommand(ProjectUpsertRequest Request) : IRequest<ProjectDto>;

public sealed record UpdateProjectCommand(Guid Id, ProjectUpsertRequest Request) : IRequest<ProjectDto>;

public sealed record DeleteProjectCommand(Guid Id) : IRequest;

public sealed record BulkDeleteProjectsCommand(BulkDeleteRequest Request) : IRequest;

public sealed record ListProjectMembersQuery(Guid ProjectId, PagedRequest Request) : IRequest<PagedResult<ProjectMemberDto>>;

public sealed record AddProjectMemberCommand(Guid ProjectId, ProjectMemberUpsertRequest Request) : IRequest<IReadOnlyCollection<ProjectMemberDto>>;

public sealed record DeleteProjectMemberCommand(Guid ProjectId, Guid UserId) : IRequest;

public sealed record BulkDeleteProjectMembersCommand(Guid ProjectId, BulkDeleteRequest Request) : IRequest;

public sealed record GetProjectSettingsQuery(Guid ProjectId) : IRequest<ProjectSettingsDto>;

public sealed record UpdateProjectSettingsCommand(Guid ProjectId, ProjectSettingsRequest Request) : IRequest<ProjectSettingsDto>;

public sealed record GetProjectDashboardQuery(Guid ProjectId) : IRequest<ProjectDashboardDto>;

internal sealed class ListProjectsQueryHandler(
    IRepository<Project> repository,
    IPaginationService paginationService,
    IMapper mapper,
    IProjectAccessService projectAccessService)
    : IRequestHandler<ListProjectsQuery, PagedResult<ProjectDto>>
{
    public Task<PagedResult<ProjectDto>> Handle(ListProjectsQuery request, CancellationToken cancellationToken)
    {
        var query = BuildQuery(repository.Query(), request.OrganizationId, request.ClientId, request.Status, request.Request, projectAccessService);

        return paginationService.CreateAsync(
            query.ProjectTo<ProjectDto>(mapper.ConfigurationProvider),
            request.Request.PageNumber,
            request.Request.PageSize,
            cancellationToken);
    }

    public static IQueryable<Project> BuildQuery(
        IQueryable<Project> query,
        Guid? organizationId,
        Guid? clientId,
        ProjectStatus? status,
        PagedRequest request,
        IProjectAccessService projectAccessService)
    {
        query = projectAccessService.ApplyProjectFilter(query);

        if (organizationId.HasValue)
        {
            query = query.Where(project => project.OrganizationId == organizationId.Value);
        }

        if (clientId.HasValue)
        {
            query = query.Where(project => project.ClientId == clientId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(project => project.Status == status.Value);
        }

        query = ApplySearch(query, request.Search);
        return ApplySort(query, request.SortBy, request.SortDirection);
    }

    private static IQueryable<Project> ApplySearch(IQueryable<Project> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var term = search.Trim();
        return query.Where(project =>
            project.Name.Contains(term)
            || project.Code.Contains(term)
            || (project.Description != null && project.Description.Contains(term))
            || project.Organization.Name.Contains(term)
            || (project.Client != null && project.Client.Name.Contains(term))
            || (project.ProjectManager != null
                && (project.ProjectManager.FirstName.Contains(term) || project.ProjectManager.LastName.Contains(term))));
    }

    private static IQueryable<Project> ApplySort(IQueryable<Project> query, string? sortBy, string? direction)
    {
        var descending = direction?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;

        return (sortBy?.ToLowerInvariant(), descending) switch
        {
            ("code", true) => query.OrderByDescending(project => project.Code),
            ("code", false) => query.OrderBy(project => project.Code),
            ("organizationname", true) => query.OrderByDescending(project => project.Organization.Name),
            ("organizationname", false) => query.OrderBy(project => project.Organization.Name),
            ("clientname", true) => query.OrderByDescending(project => project.Client == null ? string.Empty : project.Client.Name),
            ("clientname", false) => query.OrderBy(project => project.Client == null ? string.Empty : project.Client.Name),
            ("status", true) => query.OrderByDescending(project => project.Status),
            ("status", false) => query.OrderBy(project => project.Status),
            ("priority", true) => query.OrderByDescending(project => project.Priority),
            ("priority", false) => query.OrderBy(project => project.Priority),
            ("startdate", true) => query.OrderByDescending(project => project.StartDate),
            ("startdate", false) => query.OrderBy(project => project.StartDate),
            ("enddate", true) => query.OrderByDescending(project => project.EndDate),
            ("enddate", false) => query.OrderBy(project => project.EndDate),
            ("createdon", true) => query.OrderByDescending(project => project.CreatedOn),
            ("createdon", false) => query.OrderBy(project => project.CreatedOn),
            ("name", true) => query.OrderByDescending(project => project.Name),
            _ => query.OrderBy(project => project.Name)
        };
    }
}

internal sealed class ListMyProjectsQueryHandler(
    IRepository<Project> repository,
    IPaginationService paginationService,
    IMapper mapper,
    IProjectAccessService projectAccessService)
    : IRequestHandler<ListMyProjectsQuery, PagedResult<ProjectDto>>
{
    public Task<PagedResult<ProjectDto>> Handle(ListMyProjectsQuery request, CancellationToken cancellationToken)
    {
        var query = ListProjectsQueryHandler.BuildQuery(repository.Query(), request.OrganizationId, request.ClientId, request.Status, request.Request, projectAccessService);

        return paginationService.CreateAsync(
            query.ProjectTo<ProjectDto>(mapper.ConfigurationProvider),
            request.Request.PageNumber,
            request.Request.PageSize,
            cancellationToken);
    }
}

internal sealed class GetProjectByIdQueryHandler(
    IRepository<Project> repository,
    IPaginationService paginationService,
    IMapper mapper,
    IProjectAccessService projectAccessService)
    : IRequestHandler<GetProjectByIdQuery, ProjectDto>
{
    public async Task<ProjectDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        await projectAccessService.EnsureCanAccessProjectAsync(request.Id, cancellationToken);

        return await ProjectReadModel.ReadProjectDtoAsync(repository, paginationService, mapper, request.Id, cancellationToken);
    }
}

internal sealed class CreateProjectCommandHandler(
    IRepository<Organization> organizationRepository,
    IRepository<Client> clientRepository,
    IRepository<UserProfile> userProfileRepository,
    IRepository<Team> teamRepository,
    IRepository<Project> repository,
    IPaginationService paginationService,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        _ = await organizationRepository.GetByIdAsync(request.Request.OrganizationId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Organization was not found.");

        await ProjectRules.EnsureUniqueProjectCodeAsync(repository, paginationService, request.Request.OrganizationId, request.Request.Code, null, cancellationToken);
        await ProjectRules.EnsureClientBelongsToOrganizationAsync(clientRepository, request.Request.ClientId, request.Request.OrganizationId, cancellationToken);
        var projectManagerProfile = await ProjectRules.EnsureUserProfileBelongsToOrganizationAsync(userProfileRepository, request.Request.ProjectManagerUserProfileId, request.Request.OrganizationId, "Project manager", cancellationToken);
        await ProjectRules.EnsureTeamsBelongToOrganizationAsync(teamRepository, paginationService, request.Request.TeamIds, request.Request.OrganizationId, cancellationToken);

        var project = mapper.Map<Project>(request.Request);
        project.Id = Guid.NewGuid();
        project.Settings = new ProjectSettings { ProjectId = project.Id };
        var now = dateTimeProvider.UtcNow;

        ProjectRules.AddProjectTeams(
            project,
            request.Request.TeamIds,
            now,
            currentUserService.UserName);
        ProjectRules.AddProjectMembers(
            project,
            ProjectRules.BuildInitialProjectMemberIds(currentUserService.UserId, projectManagerProfile?.UserId),
            ProjectMemberRole.ProjectManager,
            now,
            currentUserService.UserName);

        await repository.AddAsync(project, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ProjectReadModel.ReadProjectDtoAsync(repository, paginationService, mapper, project.Id, cancellationToken);
    }
}

internal sealed class UpdateProjectCommandHandler(
    IRepository<Organization> organizationRepository,
    IRepository<Client> clientRepository,
    IRepository<UserProfile> userProfileRepository,
    IRepository<Team> teamRepository,
    IRepository<Project> repository,
    IRepository<ProjectMember> projectMemberRepository,
    IRepository<ProjectTeam> projectTeamRepository,
    IPaginationService paginationService,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IProjectAccessService projectAccessService)
    : IRequestHandler<UpdateProjectCommand, ProjectDto>
{
    public async Task<ProjectDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        await projectAccessService.EnsureCanAccessProjectAsync(request.Id, cancellationToken);

        var project = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ApplicationNotFoundException("Project was not found.");

        _ = await organizationRepository.GetByIdAsync(request.Request.OrganizationId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Organization was not found.");

        if (project.OrganizationId != request.Request.OrganizationId)
        {
            var existingMemberId = await paginationService.SingleOrDefaultAsync(
                projectMemberRepository.Query()
                    .Where(member => member.ProjectId == project.Id)
                    .Select(member => (Guid?)member.UserId),
                cancellationToken);

            if (existingMemberId.HasValue)
            {
                throw new ApplicationConflictException("Project organization cannot be changed while members are attached.");
            }
        }

        await ProjectRules.EnsureUniqueProjectCodeAsync(repository, paginationService, request.Request.OrganizationId, request.Request.Code, project.Id, cancellationToken);
        await ProjectRules.EnsureClientBelongsToOrganizationAsync(clientRepository, request.Request.ClientId, request.Request.OrganizationId, cancellationToken);
        var projectManagerProfile = await ProjectRules.EnsureUserProfileBelongsToOrganizationAsync(userProfileRepository, request.Request.ProjectManagerUserProfileId, request.Request.OrganizationId, "Project manager", cancellationToken);
        await ProjectRules.EnsureTeamsBelongToOrganizationAsync(teamRepository, paginationService, request.Request.TeamIds, request.Request.OrganizationId, cancellationToken);

        mapper.Map(request.Request, project);
        repository.Update(project);
        var now = dateTimeProvider.UtcNow;
        await ProjectRules.ReplaceProjectTeamsAsync(
            projectTeamRepository,
            paginationService,
            project.Id,
            request.Request.TeamIds,
            now,
            currentUserService.UserName,
            cancellationToken);
        await ProjectRules.UpsertProjectMembersAsync(
            projectMemberRepository,
            paginationService,
            project.Id,
            ProjectRules.BuildInitialProjectMemberIds(null, projectManagerProfile?.UserId),
            ProjectMemberRole.ProjectManager,
            now,
            currentUserService.UserName,
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ProjectReadModel.ReadProjectDtoAsync(repository, paginationService, mapper, project.Id, cancellationToken);
    }
}

internal sealed class DeleteProjectCommandHandler(
    IRepository<Project> repository,
    IUnitOfWork unitOfWork,
    IProjectAccessService projectAccessService)
    : IRequestHandler<DeleteProjectCommand>
{
    public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        await projectAccessService.EnsureCanAccessProjectAsync(request.Id, cancellationToken);

        var project = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ApplicationNotFoundException("Project was not found.");

        repository.Remove(project);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class BulkDeleteProjectsCommandHandler(
    IRepository<Project> repository,
    IPaginationService paginationService,
    IUnitOfWork unitOfWork,
    IProjectAccessService projectAccessService)
    : IRequestHandler<BulkDeleteProjectsCommand>
{
    public async Task Handle(BulkDeleteProjectsCommand request, CancellationToken cancellationToken)
    {
        await projectAccessService.EnsureCanAccessProjectsAsync(request.Request.Ids, cancellationToken);

        var projects = await paginationService.ToListAsync(
            repository.Query().Where(project => request.Request.Ids.Contains(project.Id)),
            cancellationToken);

        repository.RemoveRange(projects);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class ListProjectMembersQueryHandler(
    IRepository<ProjectMember> projectMemberRepository,
    IPaginationService paginationService,
    IMapper mapper,
    IProjectAccessService projectAccessService)
    : IRequestHandler<ListProjectMembersQuery, PagedResult<ProjectMemberDto>>
{
    public async Task<PagedResult<ProjectMemberDto>> Handle(ListProjectMembersQuery request, CancellationToken cancellationToken)
    {
        await projectAccessService.EnsureCanAccessProjectAsync(request.ProjectId, cancellationToken);

        var query = projectMemberRepository.Query().Where(member => member.ProjectId == request.ProjectId && member.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
        {
            var term = request.Request.Search.Trim();
            query = query.Where(member =>
                (member.UserProfile != null
                    && (member.UserProfile.FirstName.Contains(term)
                        || member.UserProfile.LastName.Contains(term)
                        || member.UserProfile.EmployeeCode.Contains(term)
                        || (member.UserProfile.Designation != null && member.UserProfile.Designation.Contains(term))))
                || member.UserId.ToString().Contains(term));
        }

        query = ApplySort(query, request.Request.SortBy, request.Request.SortDirection);

        return await paginationService.CreateAsync(
            query.ProjectTo<ProjectMemberDto>(mapper.ConfigurationProvider),
            request.Request.PageNumber,
            request.Request.PageSize,
            cancellationToken);
    }

    private static IQueryable<ProjectMember> ApplySort(IQueryable<ProjectMember> query, string? sortBy, string? direction)
    {
        var descending = direction?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;

        return (sortBy?.ToLowerInvariant(), descending) switch
        {
            ("employeecode", true) => query.OrderByDescending(member => member.UserProfile == null ? string.Empty : member.UserProfile.EmployeeCode),
            ("employeecode", false) => query.OrderBy(member => member.UserProfile == null ? string.Empty : member.UserProfile.EmployeeCode),
            ("roleinproject", true) => query.OrderByDescending(member => member.RoleInProject),
            ("roleinproject", false) => query.OrderBy(member => member.RoleInProject),
            ("joineddate", true) => query.OrderByDescending(member => member.JoinedDate),
            ("joineddate", false) => query.OrderBy(member => member.JoinedDate),
            ("userdisplayname", true) => query.OrderByDescending(member => member.UserProfile == null ? string.Empty : member.UserProfile.FirstName).ThenByDescending(member => member.UserProfile == null ? string.Empty : member.UserProfile.LastName),
            _ => query.OrderBy(member => member.UserProfile == null ? string.Empty : member.UserProfile.FirstName).ThenBy(member => member.UserProfile == null ? string.Empty : member.UserProfile.LastName)
        };
    }
}

internal sealed class AddProjectMemberCommandHandler(
    IRepository<Project> projectRepository,
    IRepository<UserProfile> userProfileRepository,
    IRepository<ProjectMember> projectMemberRepository,
    IRepository<WorkActivity> activityRepository,
    IPaginationService paginationService,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IProjectAccessService projectAccessService,
    INotificationDispatcher notificationQueue)
    : IRequestHandler<AddProjectMemberCommand, IReadOnlyCollection<ProjectMemberDto>>
{
    public async Task<IReadOnlyCollection<ProjectMemberDto>> Handle(AddProjectMemberCommand request, CancellationToken cancellationToken)
    {
        await projectAccessService.EnsureCanAccessProjectAsync(request.ProjectId, cancellationToken);

        var project = await projectRepository.GetByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Project was not found.");

        var userIds = request.Request.UserIds.Distinct().ToArray();

        await ProjectRules.EnsureUsersBelongToOrganizationAsync(
            userProfileRepository,
            paginationService,
            userIds,
            project.OrganizationId,
            cancellationToken);

        await ProjectRules.UpsertProjectMembersAsync(
            projectMemberRepository,
            paginationService,
            request.ProjectId,
            userIds,
            request.Request.RoleInProject,
            dateTimeProvider.UtcNow,
            currentUserService.UserName,
            cancellationToken);

        await WorkActivities.AddProjectAsync(
            activityRepository,
            request.ProjectId,
            WorkActivityType.ProjectMembershipChanged,
            "Project members added or updated.",
            null,
            $"{request.Request.RoleInProject}: {string.Join(",", userIds)}",
            dateTimeProvider,
            currentUserService,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await notificationQueue.EnqueueAsync(
            new NotificationIntent(
                userIds,
                request.ProjectId,
                null,
                NotificationType.ProjectInvitation,
                NotificationPriority.High,
                "Project invitation",
                $"{currentUserService.UserName ?? "A teammate"} added you to {project.Name}.",
                $"/app/projects/{project.Id}",
                "folder-kanban",
                currentUserService.UserId,
                currentUserService.UserName),
            cancellationToken);

        return await ProjectReadModel.ReadProjectMemberDtosAsync(projectMemberRepository, paginationService, mapper, request.ProjectId, userIds, cancellationToken);
    }
}

internal sealed class DeleteProjectMemberCommandHandler(
    IRepository<ProjectMember> repository,
    IRepository<WorkActivity> activityRepository,
    IPaginationService paginationService,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IProjectAccessService projectAccessService)
    : IRequestHandler<DeleteProjectMemberCommand>
{
    public async Task Handle(DeleteProjectMemberCommand request, CancellationToken cancellationToken)
    {
        await projectAccessService.EnsureCanAccessProjectAsync(request.ProjectId, cancellationToken);

        var member = await paginationService.SingleOrDefaultAsync(
            repository.Query()
                .Where(entity => entity.ProjectId == request.ProjectId && entity.UserId == request.UserId && entity.IsActive),
            cancellationToken)
            ?? throw new ApplicationNotFoundException("Project member was not found.");

        member.IsActive = false;
        repository.Update(member);
        await WorkActivities.AddProjectAsync(
            activityRepository,
            request.ProjectId,
            WorkActivityType.ProjectMembershipChanged,
            "Project member removed.",
            $"{member.RoleInProject}: {member.UserId}",
            null,
            dateTimeProvider,
            currentUserService,
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class BulkDeleteProjectMembersCommandHandler(
    IRepository<ProjectMember> repository,
    IRepository<WorkActivity> activityRepository,
    IPaginationService paginationService,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IProjectAccessService projectAccessService)
    : IRequestHandler<BulkDeleteProjectMembersCommand>
{
    public async Task Handle(BulkDeleteProjectMembersCommand request, CancellationToken cancellationToken)
    {
        await projectAccessService.EnsureCanAccessProjectAsync(request.ProjectId, cancellationToken);

        var members = await paginationService.ToListAsync(
            repository.Query()
                .Where(member => member.ProjectId == request.ProjectId && member.IsActive && request.Request.Ids.Contains(member.UserId)),
            cancellationToken);

        foreach (var member in members)
        {
            member.IsActive = false;
            repository.Update(member);
        }

        if (members.Count > 0)
        {
            await WorkActivities.AddProjectAsync(
                activityRepository,
                request.ProjectId,
                WorkActivityType.ProjectMembershipChanged,
                "Project members removed.",
                string.Join(",", members.Select(member => $"{member.RoleInProject}: {member.UserId}")),
                null,
                dateTimeProvider,
                currentUserService,
                cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class GetProjectSettingsQueryHandler(
    IRepository<Project> projectRepository,
    IRepository<ProjectSettings> settingsRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IProjectAccessService projectAccessService)
    : IRequestHandler<GetProjectSettingsQuery, ProjectSettingsDto>
{
    public async Task<ProjectSettingsDto> Handle(GetProjectSettingsQuery request, CancellationToken cancellationToken)
    {
        await projectAccessService.EnsureCanAccessProjectAsync(request.ProjectId, cancellationToken);

        var settings = await settingsRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (settings is not null)
        {
            return mapper.Map<ProjectSettingsDto>(settings);
        }

        _ = await projectRepository.GetByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Project was not found.");

        settings = new ProjectSettings { ProjectId = request.ProjectId };
        await settingsRepository.AddAsync(settings, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<ProjectSettingsDto>(settings);
    }
}

internal sealed class UpdateProjectSettingsCommandHandler(
    IRepository<Project> projectRepository,
    IRepository<ProjectSettings> settingsRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IProjectAccessService projectAccessService)
    : IRequestHandler<UpdateProjectSettingsCommand, ProjectSettingsDto>
{
    public async Task<ProjectSettingsDto> Handle(UpdateProjectSettingsCommand request, CancellationToken cancellationToken)
    {
        await projectAccessService.EnsureCanAccessProjectAsync(request.ProjectId, cancellationToken);

        _ = await projectRepository.GetByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Project was not found.");

        var settings = await settingsRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (settings is null)
        {
            settings = new ProjectSettings { ProjectId = request.ProjectId };
            await settingsRepository.AddAsync(settings, cancellationToken);
        }

        settings.EnableSprint = request.Request.EnableSprint;
        settings.EnableBacklog = request.Request.EnableBacklog;
        settings.EnableKanban = request.Request.EnableKanban;
        settings.EnableTimeTracking = request.Request.EnableTimeTracking;
        settings.EnableWiki = request.Request.EnableWiki;
        settings.EnableDocuments = request.Request.EnableDocuments;
        settings.EnableApprovals = request.Request.EnableApprovals;
        settings.EnableLeaveRequests = request.Request.EnableLeaveRequests;
        settings.EnableBugTracking = request.Request.EnableBugTracking;
        settings.EnableRiskRegister = request.Request.EnableRiskRegister;

        settingsRepository.Update(settings);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<ProjectSettingsDto>(settings);
    }
}

internal sealed class GetProjectDashboardQueryHandler(
    IRepository<Project> projectRepository,
    IRepository<ProjectMember> projectMemberRepository,
    IRepository<AuditLog> auditLogRepository,
    IPaginationService paginationService,
    IDateTimeProvider dateTimeProvider,
    IMapper mapper,
    IProjectAccessService projectAccessService)
    : IRequestHandler<GetProjectDashboardQuery, ProjectDashboardDto>
{
    public async Task<ProjectDashboardDto> Handle(GetProjectDashboardQuery request, CancellationToken cancellationToken)
    {
        await projectAccessService.EnsureCanAccessProjectAsync(request.ProjectId, cancellationToken);

        var project = await ProjectReadModel.ReadProjectDtoAsync(projectRepository, paginationService, mapper, request.ProjectId, cancellationToken);
        var projectIdText = request.ProjectId.ToString();

        var recentMembers = await paginationService.ToListAsync(
            projectMemberRepository.Query()
                .Where(member => member.ProjectId == request.ProjectId && member.IsActive)
                .OrderByDescending(member => member.JoinedDate)
                .Take(5)
                .Select(member => new RecentProjectMemberDto(
                    member.UserId,
                    member.UserProfile == null ? member.UserId.ToString() : member.UserProfile.FirstName + " " + member.UserProfile.LastName,
                    member.RoleInProject,
                    member.JoinedDate)),
            cancellationToken);

        var activity = await paginationService.ToListAsync(
            auditLogRepository.Query()
                .Where(log => log.EntityId == projectIdText
                    && (log.EntityName == nameof(Project)
                        || log.EntityName == nameof(ProjectMember)
                        || log.EntityName == nameof(ProjectSettings)
                        || log.EntityName == nameof(ProjectTeam)))
                .OrderByDescending(log => log.ChangedOn)
                .Take(8)
                .Select(log => new ProjectActivityDto(
                    log.EntityName,
                    log.Action,
                    log.ChangedBy ?? "system",
                    log.ChangedOn)),
            cancellationToken);

        var summary = new ProjectSummaryDto(
            project.Id,
            project.Name,
            project.Code,
            project.Status,
            project.Priority,
            project.StartDate,
            project.EndDate,
            project.EstimatedBudget,
            project.OrganizationName,
            project.ClientName,
            project.ProjectManagerName);

        return new ProjectDashboardDto(
            summary,
            project.MemberCount,
            project.TeamIds.Count,
            0,
            GetCompletionPercentage(project.Status),
            GetUpcomingDeadlines(project, dateTimeProvider.UtcNow),
            activity,
            recentMembers);
    }

    private static decimal GetCompletionPercentage(ProjectStatus status)
    {
        return status switch
        {
            ProjectStatus.Completed => 100,
            ProjectStatus.Archived => 100,
            ProjectStatus.Active => 50,
            ProjectStatus.OnHold => 50,
            _ => 0
        };
    }

    private static ProjectDeadlineDto[] GetUpcomingDeadlines(ProjectDto project, DateTimeOffset now)
    {
        if (!project.EndDate.HasValue)
        {
            return Array.Empty<ProjectDeadlineDto>();
        }

        var today = DateOnly.FromDateTime(now.UtcDateTime.Date);
        return project.EndDate.Value >= today
            ? [new ProjectDeadlineDto("Project end date", project.EndDate.Value, "Project")]
            : Array.Empty<ProjectDeadlineDto>();
    }
}

internal static class ProjectReadModel
{
    public static async Task<ProjectDto> ReadProjectDtoAsync(
        IRepository<Project> repository,
        IPaginationService paginationService,
        IMapper mapper,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var project = await paginationService.SingleOrDefaultAsync(
            repository.Query()
                .Where(entity => entity.Id == projectId)
                .ProjectTo<ProjectDto>(mapper.ConfigurationProvider),
            cancellationToken);

        return project ?? throw new ApplicationNotFoundException("Project was not found.");
    }

    public static async Task<ProjectMemberDto> ReadProjectMemberDtoAsync(
        IRepository<ProjectMember> repository,
        IPaginationService paginationService,
        IMapper mapper,
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var member = await paginationService.SingleOrDefaultAsync(
            repository.Query()
                .Where(entity => entity.ProjectId == projectId && entity.UserId == userId && entity.IsActive)
                .ProjectTo<ProjectMemberDto>(mapper.ConfigurationProvider),
            cancellationToken);

        return member ?? throw new ApplicationNotFoundException("Project member was not found.");
    }

    public static async Task<IReadOnlyCollection<ProjectMemberDto>> ReadProjectMemberDtosAsync(
        IRepository<ProjectMember> repository,
        IPaginationService paginationService,
        IMapper mapper,
        Guid projectId,
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        var requestedUserIds = userIds.Distinct().ToArray();

        return await paginationService.ToListAsync(
            repository.Query()
                .Where(entity => entity.ProjectId == projectId && entity.IsActive && requestedUserIds.Contains(entity.UserId))
                .OrderBy(entity => entity.UserProfile == null ? string.Empty : entity.UserProfile.FirstName)
                .ThenBy(entity => entity.UserProfile == null ? string.Empty : entity.UserProfile.LastName)
                .ProjectTo<ProjectMemberDto>(mapper.ConfigurationProvider),
            cancellationToken);
    }
}

internal static class ProjectRules
{
    public static async Task EnsureUniqueProjectCodeAsync(
        IRepository<Project> repository,
        IPaginationService paginationService,
        Guid organizationId,
        string code,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        var existingId = await paginationService.SingleOrDefaultAsync(
            repository.Query()
                .Where(project => project.OrganizationId == organizationId
                    && project.Code == code
                    && (!excludingId.HasValue || project.Id != excludingId.Value))
                .Select(project => (Guid?)project.Id),
            cancellationToken);

        if (existingId.HasValue)
        {
            throw new ApplicationConflictException("Project code already exists for the selected organization.");
        }
    }

    public static async Task EnsureClientBelongsToOrganizationAsync(
        IRepository<Client> repository,
        Guid? clientId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        if (!clientId.HasValue)
        {
            return;
        }

        var client = await repository.GetByIdAsync(clientId.Value, cancellationToken)
            ?? throw new ApplicationNotFoundException("Client was not found.");

        if (client.OrganizationId != organizationId)
        {
            throw new ApplicationConflictException("Client must belong to the selected organization.");
        }
    }

    public static async Task<UserProfile?> EnsureUserProfileBelongsToOrganizationAsync(
        IRepository<UserProfile> repository,
        Guid? userProfileId,
        Guid organizationId,
        string label,
        CancellationToken cancellationToken)
    {
        if (!userProfileId.HasValue)
        {
            return null;
        }

        var userProfile = await repository.GetByIdAsync(userProfileId.Value, cancellationToken)
            ?? throw new ApplicationNotFoundException($"{label} user profile was not found.");

        if (userProfile.OrganizationId != organizationId)
        {
            throw new ApplicationConflictException($"{label} must belong to the selected organization.");
        }

        if (!userProfile.IsActive)
        {
            throw new ApplicationConflictException($"{label} must be active.");
        }

        return userProfile;
    }

    public static async Task EnsureTeamsBelongToOrganizationAsync(
        IRepository<Team> repository,
        IPaginationService paginationService,
        IReadOnlyCollection<Guid> teamIds,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var requestedTeamIds = teamIds.Distinct().ToArray();
        if (requestedTeamIds.Length == 0)
        {
            return;
        }

        var matchedTeamIds = await paginationService.ToListAsync(
            repository.Query()
                .Where(team => requestedTeamIds.Contains(team.Id) && team.Department.OrganizationId == organizationId)
                .Select(team => team.Id),
            cancellationToken);

        if (matchedTeamIds.Count != requestedTeamIds.Length)
        {
            throw new ApplicationNotFoundException("One or more project teams were not found in the selected organization.");
        }
    }

    public static async Task EnsureUsersBelongToOrganizationAsync(
        IRepository<UserProfile> repository,
        IPaginationService paginationService,
        IReadOnlyCollection<Guid> userIds,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var requestedUserIds = userIds.Distinct().ToArray();
        if (requestedUserIds.Length == 0)
        {
            return;
        }

        var userProfiles = await paginationService.ToListAsync(
            repository.Query().Where(profile => requestedUserIds.Contains(profile.UserId)),
            cancellationToken);

        if (userProfiles.Count != requestedUserIds.Length)
        {
            throw new ApplicationNotFoundException("One or more project member user profiles were not found.");
        }

        if (userProfiles.Any(profile => profile.OrganizationId != organizationId))
        {
            throw new ApplicationConflictException("Project members must belong to the selected organization.");
        }

        if (userProfiles.Any(profile => !profile.IsActive))
        {
            throw new ApplicationConflictException("Project members must be active.");
        }
    }

    public static void AddProjectTeams(Project project, IReadOnlyCollection<Guid> teamIds, DateTimeOffset addedOn, string? addedBy)
    {
        foreach (var teamId in teamIds.Distinct())
        {
            project.ProjectTeams.Add(new ProjectTeam
            {
                ProjectId = project.Id,
                TeamId = teamId,
                AddedOn = addedOn,
                AddedBy = addedBy
            });
        }
    }

    public static IReadOnlyCollection<Guid> BuildInitialProjectMemberIds(Guid? currentUserId, Guid? projectManagerUserId)
    {
        return new[] { currentUserId, projectManagerUserId }
            .Where(userId => userId.HasValue)
            .Select(userId => userId!.Value)
            .Distinct()
            .ToArray();
    }

    public static void AddProjectMembers(
        Project project,
        IReadOnlyCollection<Guid> userIds,
        ProjectMemberRole roleInProject,
        DateTimeOffset joinedDate,
        string? addedBy)
    {
        foreach (var userId in userIds.Distinct())
        {
            project.Members.Add(new ProjectMember
            {
                ProjectId = project.Id,
                UserId = userId,
                RoleInProject = roleInProject,
                JoinedDate = joinedDate,
                IsActive = true,
                AddedBy = addedBy
            });
        }
    }

    public static async Task UpsertProjectMembersAsync(
        IRepository<ProjectMember> repository,
        IPaginationService paginationService,
        Guid projectId,
        IReadOnlyCollection<Guid> userIds,
        ProjectMemberRole roleInProject,
        DateTimeOffset joinedDate,
        string? addedBy,
        CancellationToken cancellationToken)
    {
        var requestedUserIds = userIds.Distinct().ToArray();
        if (requestedUserIds.Length == 0)
        {
            return;
        }

        var existingMembers = await paginationService.ToListAsync(
            repository.Query().Where(member => member.ProjectId == projectId && requestedUserIds.Contains(member.UserId)),
            cancellationToken);

        var existingByUserId = existingMembers.ToDictionary(member => member.UserId);

        foreach (var userId in requestedUserIds)
        {
            if (existingByUserId.TryGetValue(userId, out var member))
            {
                member.RoleInProject = roleInProject;
                member.IsActive = true;
                if (member.JoinedDate == default)
                {
                    member.JoinedDate = joinedDate;
                }

                repository.Update(member);
                continue;
            }

            await repository.AddAsync(
                new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = userId,
                    RoleInProject = roleInProject,
                    JoinedDate = joinedDate,
                    IsActive = true,
                    AddedBy = addedBy
                },
                cancellationToken);
        }
    }

    public static async Task ReplaceProjectTeamsAsync(
        IRepository<ProjectTeam> repository,
        IPaginationService paginationService,
        Guid projectId,
        IReadOnlyCollection<Guid> teamIds,
        DateTimeOffset addedOn,
        string? addedBy,
        CancellationToken cancellationToken)
    {
        var requestedTeamIds = teamIds.Distinct().ToHashSet();
        var currentProjectTeams = await paginationService.ToListAsync(
            repository.Query().Where(projectTeam => projectTeam.ProjectId == projectId),
            cancellationToken);

        var currentTeamIds = currentProjectTeams.Select(projectTeam => projectTeam.TeamId).ToHashSet();
        repository.RemoveRange(currentProjectTeams.Where(projectTeam => !requestedTeamIds.Contains(projectTeam.TeamId)));

        foreach (var teamId in requestedTeamIds.Where(teamId => !currentTeamIds.Contains(teamId)))
        {
            await repository.AddAsync(
                new ProjectTeam
                {
                    ProjectId = projectId,
                    TeamId = teamId,
                    AddedOn = addedOn,
                    AddedBy = addedBy
                },
                cancellationToken);
        }
    }
}
