using System.IO;
using System.Globalization;
using MediatR;
using ProjectManagementSaaS.Application.Abstractions.Common;
using ProjectManagementSaaS.Application.Abstractions.Identity;
using ProjectManagementSaaS.Application.Abstractions.Notifications;
using ProjectManagementSaaS.Application.Abstractions.Persistence;
using ProjectManagementSaaS.Application.Abstractions.Projects;
using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Common.Security;
using ProjectManagementSaaS.Application.Features.Work.Contracts;
using ProjectManagementSaaS.Domain.Organizations;
using ProjectManagementSaaS.Domain.Notifications;
using ProjectManagementSaaS.Domain.Projects;
using ProjectManagementSaaS.Domain.Work;

namespace ProjectManagementSaaS.Application.Features.Work;

public sealed record GetWorkMetadataQuery(Guid? ProjectId, Guid? OrganizationId) : IRequest<WorkMetadataDto>;

public sealed record ListWorkItemsQuery(
    PagedRequest Request,
    Guid? ProjectId,
    Guid? WorkItemTypeId,
    Guid? WorkflowStatusId,
    Guid? PriorityId,
    Guid? AssigneeUserProfileId,
    Guid? ReporterUserProfileId) : IRequest<PagedResult<WorkItemDto>>;

public sealed record GetWorkItemByIdQuery(Guid Id) : IRequest<WorkItemDto>;

public sealed record CreateWorkItemCommand(WorkItemUpsertRequest Request) : IRequest<WorkItemDto>;

public sealed record UpdateWorkItemCommand(Guid Id, WorkItemUpsertRequest Request) : IRequest<WorkItemDto>;

public sealed record DeleteWorkItemCommand(Guid Id) : IRequest;

public sealed record BulkDeleteWorkItemsCommand(BulkDeleteRequest Request) : IRequest;

public sealed record TransitionWorkItemCommand(Guid Id, WorkTransitionRequest Request) : IRequest<WorkItemDto>;

public sealed record ListWorkItemCommentsQuery(Guid WorkItemId, PagedRequest Request) : IRequest<PagedResult<WorkCommentDto>>;

public sealed record AddWorkItemCommentCommand(Guid WorkItemId, WorkCommentUpsertRequest Request) : IRequest<WorkCommentDto>;

public sealed record UpdateWorkItemCommentCommand(Guid WorkItemId, Guid CommentId, WorkCommentUpsertRequest Request) : IRequest<WorkCommentDto>;

public sealed record DeleteWorkItemCommentCommand(Guid WorkItemId, Guid CommentId) : IRequest;

public sealed record RestoreWorkItemCommentCommand(Guid WorkItemId, Guid CommentId) : IRequest<WorkCommentDto>;

public sealed record PinWorkItemCommentCommand(Guid WorkItemId, Guid CommentId, bool Pinned) : IRequest<WorkCommentDto>;

public sealed record ResolveWorkItemCommentCommand(Guid WorkItemId, Guid CommentId, bool Resolved) : IRequest<WorkCommentDto>;

public sealed record ListWorkItemCommentHistoryQuery(Guid WorkItemId, Guid CommentId, PagedRequest Request) : IRequest<PagedResult<WorkCommentHistoryDto>>;

public sealed record ListWorkItemCommentAttachmentsQuery(Guid WorkItemId, Guid CommentId, PagedRequest Request) : IRequest<PagedResult<WorkCommentAttachmentDto>>;

public sealed record AddWorkItemCommentAttachmentCommand(Guid WorkItemId, Guid CommentId, WorkCommentAttachmentRequest Request) : IRequest<WorkCommentAttachmentDto>;

public sealed record DeleteWorkItemCommentAttachmentCommand(Guid WorkItemId, Guid CommentId, Guid AttachmentId) : IRequest;

public sealed record ListWorkItemMentionCandidatesQuery(Guid WorkItemId, PagedRequest Request) : IRequest<PagedResult<WorkMentionCandidateDto>>;

public sealed record ListWorkItemCommentMentionsQuery(Guid WorkItemId, Guid CommentId, PagedRequest Request) : IRequest<PagedResult<WorkCommentMentionDto>>;

public sealed record AddWorkItemCommentMentionsCommand(Guid WorkItemId, Guid CommentId, WorkCommentMentionsRequest Request) : IRequest<IReadOnlyCollection<WorkCommentMentionDto>>;

public sealed record DeleteWorkItemCommentMentionCommand(Guid WorkItemId, Guid CommentId, Guid MentionId) : IRequest;

public sealed record ListWorkItemCommentReactionsQuery(Guid WorkItemId, Guid CommentId, PagedRequest Request) : IRequest<PagedResult<WorkCommentReactionDto>>;

public sealed record ToggleWorkItemCommentReactionCommand(Guid WorkItemId, Guid CommentId, WorkCommentReactionRequest Request) : IRequest<IReadOnlyCollection<WorkCommentReactionDto>>;

public sealed record DeleteWorkItemCommentReactionCommand(Guid WorkItemId, Guid CommentId, string Emoji) : IRequest;

public sealed record ListWorkItemCommentReadsQuery(Guid WorkItemId, Guid CommentId, PagedRequest Request) : IRequest<PagedResult<WorkCommentReadDto>>;

public sealed record MarkWorkItemCommentReadCommand(Guid WorkItemId, Guid CommentId) : IRequest<WorkCommentReadDto>;

public sealed record MarkWorkItemCommentsReadCommand(Guid WorkItemId) : IRequest;

public sealed record ListWorkItemAttachmentsQuery(Guid WorkItemId, PagedRequest Request) : IRequest<PagedResult<WorkAttachmentDto>>;

public sealed record AddWorkItemAttachmentCommand(Guid WorkItemId, WorkAttachmentRequest Request) : IRequest<WorkAttachmentDto>;

public sealed record DeleteWorkItemAttachmentCommand(Guid WorkItemId, Guid AttachmentId) : IRequest;

public sealed record ListWorkItemActivityQuery(
    Guid WorkItemId,
    PagedRequest Request,
    WorkActivityCategory? Category,
    WorkActivityType? ActivityType,
    DateTimeOffset? DateFrom,
    DateTimeOffset? DateTo) : IRequest<PagedResult<WorkActivityDto>>;

public sealed record ListWorkItemLinksQuery(Guid WorkItemId, PagedRequest Request) : IRequest<PagedResult<WorkItemLinkDto>>;

public sealed record AddWorkItemLinkCommand(Guid WorkItemId, WorkItemLinkRequest Request) : IRequest<WorkItemLinkDto>;

public sealed record DeleteWorkItemLinkCommand(Guid WorkItemId, Guid LinkId) : IRequest;

public sealed record ListWorkSavedFiltersQuery(Guid? ProjectId, PagedRequest Request) : IRequest<PagedResult<WorkSavedFilterDto>>;

public sealed record CreateWorkSavedFilterCommand(WorkSavedFilterRequest Request) : IRequest<WorkSavedFilterDto>;

public sealed record DeleteWorkSavedFilterCommand(Guid Id) : IRequest;

internal sealed class GetWorkMetadataQueryHandler(
    IRepository<WorkItemType> typeRepository,
    IRepository<WorkWorkflow> workflowRepository,
    IRepository<WorkPriority> priorityRepository,
    IRepository<WorkLabel> labelRepository,
    IRepository<WorkComponent> componentRepository,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<GetWorkMetadataQuery, WorkMetadataDto>
{
    public async Task<WorkMetadataDto> Handle(GetWorkMetadataQuery request, CancellationToken cancellationToken)
    {
        if (request.ProjectId.HasValue)
        {
            await projectAccessService.EnsureCanAccessProjectAsync(request.ProjectId.Value, cancellationToken);
        }

        var types = await paginationService.ToListAsync(
            typeRepository.Query()
                .Where(type => type.IsActive)
                .OrderBy(type => type.Name)
                .Select(type => new WorkItemTypeDto(type.Id, type.Name, type.Code, type.Icon, type.Color, type.WorkflowId, type.Description, type.IsActive)),
            cancellationToken);

        var workflows = await paginationService.ToListAsync(
            workflowRepository.Query()
                .Where(workflow => workflow.IsActive)
                .OrderByDescending(workflow => workflow.IsDefault)
                .ThenBy(workflow => workflow.Name)
                .Select(workflow => new WorkWorkflowDto(
                    workflow.Id,
                    workflow.Name,
                    workflow.Code,
                    workflow.Description,
                    workflow.IsDefault,
                    workflow.IsActive,
                    workflow.Statuses
                        .Where(status => status.IsActive)
                        .OrderBy(status => status.SortOrder)
                        .Select(status => new WorkWorkflowStatusDto(status.Id, status.WorkflowId, status.Name, status.Code, status.Color, status.SortOrder, status.IsInitial, status.IsTerminal, status.IsActive))
                        .ToArray(),
                    workflow.Transitions
                        .Where(transition => transition.IsActive)
                        .Select(transition => new WorkWorkflowTransitionDto(transition.Id, transition.WorkflowId, transition.FromStatusId, transition.ToStatusId, transition.Name, transition.IsActive))
                        .ToArray())),
            cancellationToken);

        var priorities = await paginationService.ToListAsync(
            priorityRepository.Query()
                .Where(priority => priority.IsActive)
                .OrderBy(priority => priority.SortOrder)
                .Select(priority => new WorkPriorityDto(priority.Id, priority.Name, priority.Code, priority.Color, priority.SortOrder, priority.IsActive)),
            cancellationToken);

        var labelsQuery = labelRepository.Query().Where(label => label.IsActive);
        if (request.OrganizationId.HasValue)
        {
            labelsQuery = labelsQuery.Where(label => label.OrganizationId == request.OrganizationId.Value);
        }

        var labels = await paginationService.ToListAsync(
            labelsQuery
                .OrderBy(label => label.Name)
                .Select(label => new WorkLabelDto(label.Id, label.OrganizationId, label.Name, label.Color, label.Description, label.IsActive)),
            cancellationToken);

        var componentsQuery = componentRepository.Query().Where(component => component.IsActive);
        if (request.ProjectId.HasValue)
        {
            componentsQuery = componentsQuery.Where(component => component.ProjectId == request.ProjectId.Value);
        }
        else if (!projectAccessService.CanAccessAllProjects)
        {
            var accessibleProjectIds = await projectAccessService.GetAccessibleProjectIdsAsync(cancellationToken);
            componentsQuery = componentsQuery.Where(component => accessibleProjectIds.Contains(component.ProjectId));
        }

        var components = await paginationService.ToListAsync(
            componentsQuery
                .OrderBy(component => component.Name)
                .Select(component => new WorkComponentDto(component.Id, component.ProjectId, component.Name, component.Description, component.Color, component.IsActive)),
            cancellationToken);

        return new WorkMetadataDto(types, workflows, priorities, labels, components);
    }
}

internal sealed class ListWorkItemsQueryHandler(
    IRepository<WorkItem> repository,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<ListWorkItemsQuery, PagedResult<WorkItemDto>>
{
    public async Task<PagedResult<WorkItemDto>> Handle(ListWorkItemsQuery request, CancellationToken cancellationToken)
    {
        var query = repository.Query();

        if (request.ProjectId.HasValue)
        {
            await projectAccessService.EnsureCanAccessProjectAsync(request.ProjectId.Value, cancellationToken);
            query = query.Where(workItem => workItem.ProjectId == request.ProjectId.Value);
        }
        else if (!projectAccessService.CanAccessAllProjects)
        {
            var userId = projectAccessService.CurrentUserId;
            query = query.Where(workItem => workItem.Project.Members.Any(member => member.UserId == userId && member.IsActive));
        }

        if (request.WorkItemTypeId.HasValue)
        {
            query = query.Where(workItem => workItem.WorkItemTypeId == request.WorkItemTypeId.Value);
        }

        if (request.WorkflowStatusId.HasValue)
        {
            query = query.Where(workItem => workItem.WorkflowStatusId == request.WorkflowStatusId.Value);
        }

        if (request.PriorityId.HasValue)
        {
            query = query.Where(workItem => workItem.PriorityId == request.PriorityId.Value);
        }

        if (request.AssigneeUserProfileId.HasValue)
        {
            query = query.Where(workItem => workItem.AssigneeUserProfileId == request.AssigneeUserProfileId.Value);
        }

        if (request.ReporterUserProfileId.HasValue)
        {
            query = query.Where(workItem => workItem.ReporterUserProfileId == request.ReporterUserProfileId.Value);
        }

        query = WorkItemQueries.ApplySearch(query, request.Request.Search);
        query = WorkItemQueries.ApplySort(query, request.Request.SortBy, request.Request.SortDirection);

        var page = await paginationService.CreateAsync(
            query.Select(WorkItemQueries.ToReadRow()),
            request.Request.PageNumber,
            request.Request.PageSize,
            cancellationToken);

        return new PagedResult<WorkItemDto>(
            page.Items.Select(WorkItemQueries.ToDto).ToArray(),
            page.PageNumber,
            page.PageSize,
            page.TotalCount);
    }
}

internal sealed class GetWorkItemByIdQueryHandler(
    IRepository<WorkItem> repository,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<GetWorkItemByIdQuery, WorkItemDto>
{
    public async Task<WorkItemDto> Handle(GetWorkItemByIdQuery request, CancellationToken cancellationToken)
    {
        return await WorkItemQueries.ReadDtoAsync(repository, paginationService, projectAccessService, request.Id, cancellationToken);
    }
}

internal sealed class CreateWorkItemCommandHandler(
    IRepository<Project> projectRepository,
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkItemType> typeRepository,
    IRepository<WorkWorkflowStatus> statusRepository,
    IRepository<WorkPriority> priorityRepository,
    IRepository<UserProfile> userProfileRepository,
    IRepository<WorkLabel> labelRepository,
    IRepository<WorkComponent> componentRepository,
    IRepository<WorkActivity> activityRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService,
    INotificationDispatcher notificationQueue)
    : IRequestHandler<CreateWorkItemCommand, WorkItemDto>
{
    public async Task<WorkItemDto> Handle(CreateWorkItemCommand request, CancellationToken cancellationToken)
    {
        var project = await WorkRules.GetProjectAsync(projectRepository, request.Request.ProjectId, cancellationToken);
        await projectAccessService.EnsureCanAccessProjectAsync(project.Id, cancellationToken);
        var type = await WorkRules.GetTypeAsync(typeRepository, request.Request.WorkItemTypeId, cancellationToken);
        var status = request.Request.WorkflowStatusId.HasValue
            ? await WorkRules.GetStatusAsync(statusRepository, request.Request.WorkflowStatusId.Value, type.WorkflowId, cancellationToken)
            : await WorkRules.GetInitialStatusAsync(statusRepository, type.WorkflowId, paginationService, cancellationToken);

        _ = await WorkRules.GetPriorityAsync(priorityRepository, request.Request.PriorityId, cancellationToken);
        await WorkRules.EnsureParentBelongsToProjectAsync(workItemRepository, paginationService, request.Request.ParentWorkItemId, project.Id, cancellationToken);
        await WorkRules.EnsureUserBelongsToOrganizationAsync(userProfileRepository, request.Request.ReporterUserProfileId, project.OrganizationId, "Reporter", cancellationToken);
        await WorkRules.EnsureUserBelongsToOrganizationAsync(userProfileRepository, request.Request.AssigneeUserProfileId, project.OrganizationId, "Assignee", cancellationToken);
        await WorkRules.EnsureUsersBelongToOrganizationAsync(userProfileRepository, paginationService, request.Request.WatcherUserProfileIds, project.OrganizationId, "Watchers", cancellationToken);
        await WorkRules.EnsureLabelsBelongToOrganizationAsync(labelRepository, paginationService, request.Request.LabelIds, project.OrganizationId, cancellationToken);
        await WorkRules.EnsureComponentsBelongToProjectAsync(componentRepository, paginationService, request.Request.ComponentIds, project.Id, cancellationToken);

        var workItem = new WorkItem
        {
            Id = Guid.NewGuid(),
            ProjectId = request.Request.ProjectId,
            ParentWorkItemId = request.Request.ParentWorkItemId,
            WorkItemTypeId = request.Request.WorkItemTypeId,
            WorkflowStatusId = status.Id,
            PriorityId = request.Request.PriorityId,
            Title = request.Request.Title.Trim(),
            Description = Normalize(request.Request.Description),
            AcceptanceCriteria = Normalize(request.Request.AcceptanceCriteria),
            AssigneeUserProfileId = request.Request.AssigneeUserProfileId,
            ReporterUserProfileId = request.Request.ReporterUserProfileId,
            StartDate = request.Request.StartDate,
            DueDate = request.Request.DueDate,
            EstimatedHours = request.Request.EstimatedHours,
            LoggedHours = request.Request.LoggedHours,
            StoryPoints = request.Request.StoryPoints,
            SprintId = request.Request.SprintId,
            Environment = Normalize(request.Request.Environment),
            Severity = Normalize(request.Request.Severity),
            Reproducible = request.Request.Reproducible
        };

        foreach (var labelId in request.Request.LabelIds.Distinct())
        {
            workItem.Labels.Add(new WorkItemLabel { WorkItemId = workItem.Id, LabelId = labelId });
        }

        foreach (var componentId in request.Request.ComponentIds.Distinct())
        {
            workItem.Components.Add(new WorkItemComponent { WorkItemId = workItem.Id, ComponentId = componentId });
        }

        foreach (var watcherId in request.Request.WatcherUserProfileIds.Distinct())
        {
            workItem.Watchers.Add(new WorkItemWatcher
            {
                WorkItemId = workItem.Id,
                UserProfileId = watcherId,
                AddedOn = dateTimeProvider.UtcNow,
                AddedBy = currentUserService.UserName
            });
        }

        await workItemRepository.AddAsync(workItem, cancellationToken);
        await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.Created, "Work item created.", null, null, null, dateTimeProvider, currentUserService, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await WorkNotifications.EnqueueForUserProfilesAsync(
            notificationQueue,
            userProfileRepository,
            paginationService,
            [workItem.AssigneeUserProfileId],
            workItem.ProjectId,
            workItem.Id,
            NotificationType.TaskAssigned,
            NotificationPriority.High,
            "Task assigned",
            $"{currentUserService.UserName ?? "A teammate"} assigned you {workItem.Title}.",
            WorkNotifications.WorkItemUrl(workItem.Id),
            "check-circle",
            currentUserService,
            cancellationToken);

        return await WorkItemQueries.ReadDtoAsync(workItemRepository, paginationService, projectAccessService, workItem.Id, cancellationToken);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

internal sealed class UpdateWorkItemCommandHandler(
    IRepository<Project> projectRepository,
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkItemLabel> workItemLabelRepository,
    IRepository<WorkItemComponent> workItemComponentRepository,
    IRepository<WorkItemWatcher> workItemWatcherRepository,
    IRepository<WorkItemType> typeRepository,
    IRepository<WorkWorkflowStatus> statusRepository,
    IRepository<WorkPriority> priorityRepository,
    IRepository<UserProfile> userProfileRepository,
    IRepository<WorkLabel> labelRepository,
    IRepository<WorkComponent> componentRepository,
    IRepository<WorkActivity> activityRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService,
    INotificationDispatcher notificationQueue)
    : IRequestHandler<UpdateWorkItemCommand, WorkItemDto>
{
    public async Task<WorkItemDto> Handle(UpdateWorkItemCommand request, CancellationToken cancellationToken)
    {
        var workItem = await workItemRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ApplicationNotFoundException("Work item was not found.");
        await projectAccessService.EnsureCanAccessProjectAsync(workItem.ProjectId, cancellationToken);

        WorkRules.EnsureRowVersion(workItem.RowVersion, request.Request.RowVersion);

        var project = await WorkRules.GetProjectAsync(projectRepository, request.Request.ProjectId, cancellationToken);
        await projectAccessService.EnsureCanAccessProjectAsync(project.Id, cancellationToken);
        var type = await WorkRules.GetTypeAsync(typeRepository, request.Request.WorkItemTypeId, cancellationToken);
        var statusId = request.Request.WorkflowStatusId ?? workItem.WorkflowStatusId;
        _ = await WorkRules.GetStatusAsync(statusRepository, statusId, type.WorkflowId, cancellationToken);
        _ = await WorkRules.GetPriorityAsync(priorityRepository, request.Request.PriorityId, cancellationToken);
        await WorkRules.EnsureParentBelongsToProjectAsync(workItemRepository, paginationService, request.Request.ParentWorkItemId, project.Id, cancellationToken);
        await WorkRules.EnsureUserBelongsToOrganizationAsync(userProfileRepository, request.Request.ReporterUserProfileId, project.OrganizationId, "Reporter", cancellationToken);
        await WorkRules.EnsureUserBelongsToOrganizationAsync(userProfileRepository, request.Request.AssigneeUserProfileId, project.OrganizationId, "Assignee", cancellationToken);
        await WorkRules.EnsureUsersBelongToOrganizationAsync(userProfileRepository, paginationService, request.Request.WatcherUserProfileIds, project.OrganizationId, "Watchers", cancellationToken);
        await WorkRules.EnsureLabelsBelongToOrganizationAsync(labelRepository, paginationService, request.Request.LabelIds, project.OrganizationId, cancellationToken);
        await WorkRules.EnsureComponentsBelongToProjectAsync(componentRepository, paginationService, request.Request.ComponentIds, project.Id, cancellationToken);

        var oldStatusId = workItem.WorkflowStatusId;
        var oldPriorityId = workItem.PriorityId;
        var oldAssigneeId = workItem.AssigneeUserProfileId;
        var oldReporterId = workItem.ReporterUserProfileId;
        var oldTitle = workItem.Title;
        var oldDescription = workItem.Description;
        var oldDueDate = workItem.DueDate;
        var oldEstimatedHours = workItem.EstimatedHours;
        var oldStoryPoints = workItem.StoryPoints;

        workItem.ProjectId = request.Request.ProjectId;
        workItem.ParentWorkItemId = request.Request.ParentWorkItemId;
        workItem.WorkItemTypeId = request.Request.WorkItemTypeId;
        workItem.WorkflowStatusId = statusId;
        workItem.PriorityId = request.Request.PriorityId;
        workItem.Title = request.Request.Title.Trim();
        workItem.Description = Normalize(request.Request.Description);
        workItem.AcceptanceCriteria = Normalize(request.Request.AcceptanceCriteria);
        workItem.AssigneeUserProfileId = request.Request.AssigneeUserProfileId;
        workItem.ReporterUserProfileId = request.Request.ReporterUserProfileId;
        workItem.StartDate = request.Request.StartDate;
        workItem.DueDate = request.Request.DueDate;
        workItem.EstimatedHours = request.Request.EstimatedHours;
        workItem.LoggedHours = request.Request.LoggedHours;
        workItem.StoryPoints = request.Request.StoryPoints;
        workItem.SprintId = request.Request.SprintId;
        workItem.Environment = Normalize(request.Request.Environment);
        workItem.Severity = Normalize(request.Request.Severity);
        workItem.Reproducible = request.Request.Reproducible;

        workItemRepository.Update(workItem);
        var labelChanges = await ReplaceLabelsAsync(workItemLabelRepository, paginationService, workItem.Id, request.Request.LabelIds, cancellationToken);
        await ReplaceComponentsAsync(workItemComponentRepository, paginationService, workItem.Id, request.Request.ComponentIds, cancellationToken);
        var watcherChanges = await ReplaceWatchersAsync(workItemWatcherRepository, paginationService, workItem.Id, request.Request.WatcherUserProfileIds, dateTimeProvider.UtcNow, currentUserService.UserName, cancellationToken);

        await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.Updated, "Work item updated.", null, null, null, dateTimeProvider, currentUserService, cancellationToken);
        if (!string.Equals(oldTitle, workItem.Title, StringComparison.Ordinal))
        {
            await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.TitleChanged, "Title changed.", nameof(WorkItem.Title), oldTitle, workItem.Title, dateTimeProvider, currentUserService, cancellationToken);
        }

        if (oldStatusId != workItem.WorkflowStatusId)
        {
            await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.StatusChanged, "Status changed.", nameof(WorkItem.WorkflowStatusId), oldStatusId.ToString(), workItem.WorkflowStatusId.ToString(), dateTimeProvider, currentUserService, cancellationToken);
        }

        if (oldPriorityId != workItem.PriorityId)
        {
            await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.PriorityChanged, "Priority changed.", nameof(WorkItem.PriorityId), oldPriorityId.ToString(), workItem.PriorityId.ToString(), dateTimeProvider, currentUserService, cancellationToken);
        }

        if (oldAssigneeId != workItem.AssigneeUserProfileId)
        {
            await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.AssignmentChanged, "Assignment changed.", nameof(WorkItem.AssigneeUserProfileId), oldAssigneeId?.ToString(), workItem.AssigneeUserProfileId?.ToString(), dateTimeProvider, currentUserService, cancellationToken);
        }

        if (oldReporterId != workItem.ReporterUserProfileId)
        {
            await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.ReporterChanged, "Reporter changed.", nameof(WorkItem.ReporterUserProfileId), oldReporterId.ToString(), workItem.ReporterUserProfileId.ToString(), dateTimeProvider, currentUserService, cancellationToken);
        }

        if (!string.Equals(oldDescription, workItem.Description, StringComparison.Ordinal))
        {
            await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.DescriptionChanged, "Description changed.", nameof(WorkItem.Description), oldDescription, workItem.Description, dateTimeProvider, currentUserService, cancellationToken);
        }

        if (oldDueDate != workItem.DueDate)
        {
            await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.DueDateChanged, "Due date changed.", nameof(WorkItem.DueDate), oldDueDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), workItem.DueDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), dateTimeProvider, currentUserService, cancellationToken);
        }

        if (oldEstimatedHours != workItem.EstimatedHours)
        {
            await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.EstimatedHoursChanged, "Estimated hours changed.", nameof(WorkItem.EstimatedHours), oldEstimatedHours?.ToString(CultureInfo.InvariantCulture), workItem.EstimatedHours?.ToString(CultureInfo.InvariantCulture), dateTimeProvider, currentUserService, cancellationToken);
        }

        if (oldStoryPoints != workItem.StoryPoints)
        {
            await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.StoryPointsChanged, "Story points changed.", nameof(WorkItem.StoryPoints), oldStoryPoints?.ToString(CultureInfo.InvariantCulture), workItem.StoryPoints?.ToString(CultureInfo.InvariantCulture), dateTimeProvider, currentUserService, cancellationToken);
        }

        if (labelChanges.Added.Count > 0)
        {
            await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.LabelAdded, "Labels added.", nameof(WorkItemLabel.LabelId), null, string.Join(",", labelChanges.Added), dateTimeProvider, currentUserService, cancellationToken);
        }

        if (labelChanges.Removed.Count > 0)
        {
            await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.LabelRemoved, "Labels removed.", nameof(WorkItemLabel.LabelId), string.Join(",", labelChanges.Removed), null, dateTimeProvider, currentUserService, cancellationToken);
        }

        if (watcherChanges.Added.Count > 0)
        {
            await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.WatcherAdded, "Watchers added.", nameof(WorkItemWatcher.UserProfileId), null, string.Join(",", watcherChanges.Added), dateTimeProvider, currentUserService, cancellationToken);
        }

        if (watcherChanges.Removed.Count > 0)
        {
            await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.WatcherRemoved, "Watchers removed.", nameof(WorkItemWatcher.UserProfileId), string.Join(",", watcherChanges.Removed), null, dateTimeProvider, currentUserService, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await WorkNotifications.EnqueueForWorkItemParticipantsAsync(
            notificationQueue,
            workItemRepository,
            userProfileRepository,
            paginationService,
            workItem.Id,
            workItem.ProjectId,
            Array.Empty<Guid>(),
            NotificationType.TaskUpdated,
            NotificationPriority.Normal,
            "Task updated",
            $"{currentUserService.UserName ?? "A teammate"} updated {workItem.Title}.",
            WorkNotifications.WorkItemUrl(workItem.Id),
            "edit",
            currentUserService,
            cancellationToken);

        if (oldAssigneeId != workItem.AssigneeUserProfileId)
        {
            await WorkNotifications.EnqueueForUserProfilesAsync(
                notificationQueue,
                userProfileRepository,
                paginationService,
                [workItem.AssigneeUserProfileId],
                workItem.ProjectId,
                workItem.Id,
                NotificationType.TaskReassigned,
                NotificationPriority.High,
                "Task reassigned",
                $"{currentUserService.UserName ?? "A teammate"} reassigned {workItem.Title} to you.",
                WorkNotifications.WorkItemUrl(workItem.Id),
                "user-check",
                currentUserService,
                cancellationToken);
        }

        if (oldStatusId != workItem.WorkflowStatusId)
        {
            await WorkNotifications.EnqueueForWorkItemParticipantsAsync(
                notificationQueue,
                workItemRepository,
                userProfileRepository,
                paginationService,
                workItem.Id,
                workItem.ProjectId,
                Array.Empty<Guid>(),
                NotificationType.StatusChanged,
                NotificationPriority.Normal,
                "Status changed",
                $"{currentUserService.UserName ?? "A teammate"} changed status on {workItem.Title}.",
                WorkNotifications.WorkItemUrl(workItem.Id),
                "git-branch",
                currentUserService,
                cancellationToken);
        }

        if (oldPriorityId != workItem.PriorityId)
        {
            await WorkNotifications.EnqueueForWorkItemParticipantsAsync(
                notificationQueue,
                workItemRepository,
                userProfileRepository,
                paginationService,
                workItem.Id,
                workItem.ProjectId,
                Array.Empty<Guid>(),
                NotificationType.PriorityChanged,
                NotificationPriority.Normal,
                "Priority changed",
                $"{currentUserService.UserName ?? "A teammate"} changed priority on {workItem.Title}.",
                WorkNotifications.WorkItemUrl(workItem.Id),
                "alert-circle",
                currentUserService,
                cancellationToken);
        }

        if (oldDueDate != workItem.DueDate)
        {
            await WorkNotifications.EnqueueForWorkItemParticipantsAsync(
                notificationQueue,
                workItemRepository,
                userProfileRepository,
                paginationService,
                workItem.Id,
                workItem.ProjectId,
                Array.Empty<Guid>(),
                NotificationType.DueDateChanged,
                NotificationPriority.Normal,
                "Due date changed",
                $"{currentUserService.UserName ?? "A teammate"} changed due date on {workItem.Title}.",
                WorkNotifications.WorkItemUrl(workItem.Id),
                "calendar",
                currentUserService,
                cancellationToken);
        }

        if (watcherChanges.Added.Count > 0)
        {
            await WorkNotifications.EnqueueForUserProfilesAsync(
                notificationQueue,
                userProfileRepository,
                paginationService,
                watcherChanges.Added.Select(watcherId => (Guid?)watcherId).ToArray(),
                workItem.ProjectId,
                workItem.Id,
                NotificationType.WatcherAdded,
                NotificationPriority.Normal,
                "Added as watcher",
                $"{currentUserService.UserName ?? "A teammate"} added you as a watcher on {workItem.Title}.",
                WorkNotifications.WorkItemUrl(workItem.Id),
                "eye",
                currentUserService,
                cancellationToken);
        }

        return await WorkItemQueries.ReadDtoAsync(workItemRepository, paginationService, projectAccessService, workItem.Id, cancellationToken);
    }

    private static async Task<CollectionChange<Guid>> ReplaceLabelsAsync(
        IRepository<WorkItemLabel> repository,
        IPaginationService paginationService,
        Guid workItemId,
        IReadOnlyCollection<Guid> labelIds,
        CancellationToken cancellationToken)
    {
        var existing = await paginationService.ToListAsync(repository.Query().Where(label => label.WorkItemId == workItemId), cancellationToken);
        var requested = labelIds.Distinct().ToHashSet();
        var removed = existing.Where(label => !requested.Contains(label.LabelId)).Select(label => label.LabelId).ToArray();
        repository.RemoveRange(existing.Where(label => removed.Contains(label.LabelId)));

        var current = existing.Select(label => label.LabelId).ToHashSet();
        var added = requested.Where(labelId => !current.Contains(labelId)).ToArray();
        foreach (var labelId in added)
        {
            await repository.AddAsync(new WorkItemLabel { WorkItemId = workItemId, LabelId = labelId }, cancellationToken);
        }

        return new CollectionChange<Guid>(added, removed);
    }

    private static async Task ReplaceComponentsAsync(
        IRepository<WorkItemComponent> repository,
        IPaginationService paginationService,
        Guid workItemId,
        IReadOnlyCollection<Guid> componentIds,
        CancellationToken cancellationToken)
    {
        var existing = await paginationService.ToListAsync(repository.Query().Where(component => component.WorkItemId == workItemId), cancellationToken);
        var requested = componentIds.Distinct().ToHashSet();
        repository.RemoveRange(existing.Where(component => !requested.Contains(component.ComponentId)));

        var current = existing.Select(component => component.ComponentId).ToHashSet();
        foreach (var componentId in requested.Where(componentId => !current.Contains(componentId)))
        {
            await repository.AddAsync(new WorkItemComponent { WorkItemId = workItemId, ComponentId = componentId }, cancellationToken);
        }
    }

    private static async Task<CollectionChange<Guid>> ReplaceWatchersAsync(
        IRepository<WorkItemWatcher> repository,
        IPaginationService paginationService,
        Guid workItemId,
        IReadOnlyCollection<Guid> watcherIds,
        DateTimeOffset addedOn,
        string? addedBy,
        CancellationToken cancellationToken)
    {
        var existing = await paginationService.ToListAsync(repository.Query().Where(watcher => watcher.WorkItemId == workItemId), cancellationToken);
        var requested = watcherIds.Distinct().ToHashSet();
        var removed = existing.Where(watcher => !requested.Contains(watcher.UserProfileId)).Select(watcher => watcher.UserProfileId).ToArray();
        repository.RemoveRange(existing.Where(watcher => removed.Contains(watcher.UserProfileId)));

        var current = existing.Select(watcher => watcher.UserProfileId).ToHashSet();
        var added = requested.Where(watcherId => !current.Contains(watcherId)).ToArray();
        foreach (var watcherId in added)
        {
            await repository.AddAsync(
                new WorkItemWatcher
                {
                    WorkItemId = workItemId,
                    UserProfileId = watcherId,
                    AddedOn = addedOn,
                    AddedBy = addedBy
                },
                cancellationToken);
        }

        return new CollectionChange<Guid>(added, removed);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record CollectionChange<TValue>(IReadOnlyCollection<TValue> Added, IReadOnlyCollection<TValue> Removed);
}

internal sealed class DeleteWorkItemCommandHandler(
    IRepository<WorkItem> repository,
    IRepository<UserProfile> userProfileRepository,
    IRepository<WorkActivity> activityRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService,
    INotificationDispatcher notificationQueue)
    : IRequestHandler<DeleteWorkItemCommand>
{
    public async Task Handle(DeleteWorkItemCommand request, CancellationToken cancellationToken)
    {
        var workItem = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ApplicationNotFoundException("Work item was not found.");
        await projectAccessService.EnsureCanAccessProjectAsync(workItem.ProjectId, cancellationToken);
        var recipientUserIds = await WorkNotifications.ResolveWorkItemParticipantUserIdsAsync(
            repository,
            userProfileRepository,
            paginationService,
            workItem.Id,
            Array.Empty<Guid>(),
            cancellationToken);

        await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.Updated, "Work item deleted.", null, null, null, dateTimeProvider, currentUserService, cancellationToken);
        repository.Remove(workItem);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await WorkNotifications.EnqueueAsync(
            notificationQueue,
            recipientUserIds,
            workItem.ProjectId,
            workItem.Id,
            NotificationType.TaskDeleted,
            NotificationPriority.High,
            "Task deleted",
            $"{currentUserService.UserName ?? "A teammate"} deleted {workItem.Title}.",
            null,
            "trash",
            currentUserService,
            cancellationToken);
    }
}

internal sealed class BulkDeleteWorkItemsCommandHandler(
    IRepository<WorkItem> repository,
    IPaginationService paginationService,
    IUnitOfWork unitOfWork,
    IProjectAccessService projectAccessService)
    : IRequestHandler<BulkDeleteWorkItemsCommand>
{
    public async Task Handle(BulkDeleteWorkItemsCommand request, CancellationToken cancellationToken)
    {
        if (!projectAccessService.CanAccessAllProjects)
        {
            var userId = projectAccessService.CurrentUserId;
            var inaccessibleId = await paginationService.SingleOrDefaultAsync(
                repository.Query()
                    .Where(workItem => request.Request.Ids.Contains(workItem.Id)
                        && !workItem.Project.Members.Any(member => member.UserId == userId && member.IsActive))
                    .Select(workItem => (Guid?)workItem.Id),
                cancellationToken);

            if (inaccessibleId.HasValue)
            {
                throw new ApplicationForbiddenException("You do not have access to one or more selected work items.");
            }
        }

        var workItems = await paginationService.ToListAsync(
            repository.Query().Where(workItem => request.Request.Ids.Contains(workItem.Id)),
            cancellationToken);

        repository.RemoveRange(workItems);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class TransitionWorkItemCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkWorkflowTransition> transitionRepository,
    IRepository<UserProfile> userProfileRepository,
    IRepository<WorkActivity> activityRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService,
    INotificationDispatcher notificationQueue)
    : IRequestHandler<TransitionWorkItemCommand, WorkItemDto>
{
    public async Task<WorkItemDto> Handle(TransitionWorkItemCommand request, CancellationToken cancellationToken)
    {
        var workItem = await workItemRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ApplicationNotFoundException("Work item was not found.");
        await projectAccessService.EnsureCanAccessProjectAsync(workItem.ProjectId, cancellationToken);

        WorkRules.EnsureRowVersion(workItem.RowVersion, request.Request.RowVersion);

        var transition = await paginationService.SingleOrDefaultAsync(
            transitionRepository.Query()
                .Where(entity => entity.FromStatusId == workItem.WorkflowStatusId
                    && entity.ToStatusId == request.Request.ToStatusId
                    && entity.IsActive),
            cancellationToken)
            ?? throw new ApplicationConflictException("Workflow transition is not allowed.");

        var previousStatusId = workItem.WorkflowStatusId;
        workItem.WorkflowStatusId = transition.ToStatusId;
        workItemRepository.Update(workItem);

        await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.WorkflowTransitioned, transition.Name, nameof(WorkItem.WorkflowStatusId), previousStatusId.ToString(), transition.ToStatusId.ToString(), dateTimeProvider, currentUserService, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await WorkNotifications.EnqueueForWorkItemParticipantsAsync(
            notificationQueue,
            workItemRepository,
            userProfileRepository,
            paginationService,
            workItem.Id,
            workItem.ProjectId,
            Array.Empty<Guid>(),
            NotificationType.StatusChanged,
            NotificationPriority.Normal,
            "Status changed",
            $"{currentUserService.UserName ?? "A teammate"} moved {workItem.Title} with {transition.Name}.",
            WorkNotifications.WorkItemUrl(workItem.Id),
            "git-branch",
            currentUserService,
            cancellationToken);

        return await WorkItemQueries.ReadDtoAsync(workItemRepository, paginationService, projectAccessService, workItem.Id, cancellationToken);
    }
}

internal sealed class ListWorkItemCommentsQueryHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService,
    ICurrentUserService currentUserService)
    : IRequestHandler<ListWorkItemCommentsQuery, PagedResult<WorkCommentDto>>
{
    public async Task<PagedResult<WorkCommentDto>> Handle(ListWorkItemCommentsQuery request, CancellationToken cancellationToken)
    {
        _ = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);

        var query = commentRepository.Query().Where(comment => comment.WorkItemId == request.WorkItemId);
        if (!string.IsNullOrWhiteSpace(request.Request.Search))
        {
            var term = request.Request.Search.Trim();
            query = query.Where(comment => comment.Message.Contains(term));
        }

        query = request.Request.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true
            ? query.OrderByDescending(comment => comment.Pinned).ThenByDescending(comment => comment.CreatedOn)
            : query.OrderByDescending(comment => comment.Pinned).ThenBy(comment => comment.CreatedOn);

        return await paginationService.CreateAsync(query.Select(WorkItemQueries.ToCommentDto(currentUserService.UserId)), request.Request.PageNumber, request.Request.PageSize, cancellationToken);
    }
}

internal sealed class AddWorkItemCommentCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<UserProfile> userProfileRepository,
    IRepository<ProjectMember> projectMemberRepository,
    IRepository<WorkActivity> activityRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService,
    INotificationDispatcher notificationQueue)
    : IRequestHandler<AddWorkItemCommentCommand, WorkCommentDto>
{
    public async Task<WorkCommentDto> Handle(AddWorkItemCommentCommand request, CancellationToken cancellationToken)
    {
        var workItem = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        var currentUserId = WorkRules.GetCurrentUserId(currentUserService);
        var authorId = await WorkRules.ResolveActiveProfileUserIdAsync(userProfileRepository, paginationService, currentUserId, cancellationToken);
        var mentionedUserIds = await WorkRules.ResolveMentionedUserIdsAsync(
            userProfileRepository,
            projectMemberRepository,
            paginationService,
            workItem.ProjectId,
            request.Request.MentionedUserIds,
            request.Request.MentionUserProfileIds,
            cancellationToken);

        if (mentionedUserIds.Count > 0)
        {
            await WorkRules.EnsureCurrentUserIsProjectMemberAsync(projectMemberRepository, paginationService, workItem.ProjectId, currentUserId, cancellationToken, projectAccessService.CanAccessAllProjects);
        }

        Guid? parentAuthorUserId = null;
        if (request.Request.ParentCommentId.HasValue)
        {
            var parent = await paginationService.SingleOrDefaultAsync(
                commentRepository.Query()
                    .Where(comment => comment.Id == request.Request.ParentCommentId.Value && comment.WorkItemId == request.WorkItemId && !comment.IsDeleted)
                    .Select(comment => new ParentCommentLookup(comment.Id, comment.AuthorId)),
                cancellationToken);
            if (parent is null)
            {
                throw new ApplicationNotFoundException("Parent comment was not found.");
            }

            parentAuthorUserId = parent.AuthorId;
        }

        var message = request.Request.RequiredMessage.Trim();
        var comment = new WorkComment
        {
            Id = Guid.NewGuid(),
            WorkItemId = request.WorkItemId,
            ParentCommentId = request.Request.ParentCommentId,
            AuthorId = authorId,
            Message = message
        };

        foreach (var mentionId in mentionedUserIds)
        {
            comment.Mentions.Add(new WorkCommentMention
            {
                Id = Guid.NewGuid(),
                CommentId = comment.Id,
                MentionedUserId = mentionId,
                MentionedByUserId = authorId,
                CreatedAt = dateTimeProvider.UtcNow
            });
        }

        await commentRepository.AddAsync(comment, cancellationToken);
        var activityType = request.Request.ParentCommentId.HasValue ? WorkActivityType.ReplyAdded : WorkActivityType.CommentAdded;
        await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, activityType, request.Request.ParentCommentId.HasValue ? "Reply added." : "Comment added.", null, null, null, dateTimeProvider, currentUserService, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (mentionedUserIds.Count > 0)
        {
            await WorkNotifications.EnqueueAsync(
                notificationQueue,
                mentionedUserIds,
                workItem.ProjectId,
                workItem.Id,
                NotificationType.Mention,
                NotificationPriority.High,
                "You were mentioned",
                $"{currentUserService.UserName ?? "A teammate"} mentioned you on {workItem.Title}.",
                WorkNotifications.CommentUrl(workItem.Id, comment.Id),
                "at-sign",
                currentUserService,
                cancellationToken);
        }

        await WorkNotifications.EnqueueForWorkItemParticipantsAsync(
            notificationQueue,
            workItemRepository,
            userProfileRepository,
            paginationService,
            workItem.Id,
            workItem.ProjectId,
            parentAuthorUserId.HasValue ? [parentAuthorUserId.Value] : Array.Empty<Guid>(),
            request.Request.ParentCommentId.HasValue ? NotificationType.Reply : NotificationType.Comment,
            NotificationPriority.Normal,
            request.Request.ParentCommentId.HasValue ? "New reply" : "New comment",
            $"{currentUserService.UserName ?? "A teammate"} {(request.Request.ParentCommentId.HasValue ? "replied on" : "commented on")} {workItem.Title}.",
            WorkNotifications.CommentUrl(workItem.Id, comment.Id),
            "message-circle",
            currentUserService,
            cancellationToken);

        return await WorkRules.ReadCommentDtoAsync(commentRepository, paginationService, currentUserId, comment.Id, cancellationToken);
    }

    private sealed record ParentCommentLookup(Guid Id, Guid? AuthorId);
}

internal sealed class UpdateWorkItemCommentCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<WorkCommentMention> mentionRepository,
    IRepository<WorkCommentHistory> historyRepository,
    IRepository<UserProfile> userProfileRepository,
    IRepository<ProjectMember> projectMemberRepository,
    IRepository<WorkActivity> activityRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService,
    INotificationDispatcher notificationQueue)
    : IRequestHandler<UpdateWorkItemCommentCommand, WorkCommentDto>
{
    public async Task<WorkCommentDto> Handle(UpdateWorkItemCommentCommand request, CancellationToken cancellationToken)
    {
        var workItem = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);

        var currentUserId = WorkRules.GetCurrentUserId(currentUserService);
        var authorId = await WorkRules.ResolveActiveProfileUserIdAsync(userProfileRepository, paginationService, currentUserId, cancellationToken);
        var mentionedUserIds = await WorkRules.ResolveMentionedUserIdsAsync(
            userProfileRepository,
            projectMemberRepository,
            paginationService,
            workItem.ProjectId,
            request.Request.MentionedUserIds,
            request.Request.MentionUserProfileIds,
            cancellationToken);

        if (mentionedUserIds.Count > 0)
        {
            await WorkRules.EnsureCurrentUserIsProjectMemberAsync(projectMemberRepository, paginationService, workItem.ProjectId, currentUserId, cancellationToken, projectAccessService.CanAccessAllProjects);
        }

        var comment = await WorkRules.GetCommentAsync(commentRepository, paginationService, request.WorkItemId, request.CommentId, cancellationToken);
        WorkRules.EnsureCanEditComment(comment, currentUserId);

        var message = request.Request.RequiredMessage.Trim();
        var previousMessage = comment.Message;
        await historyRepository.AddAsync(
            new WorkCommentHistory
            {
                Id = Guid.NewGuid(),
                CommentId = comment.Id,
                EditedBy = currentUserId,
                EditedByName = currentUserService.UserName ?? currentUserId.ToString(),
                EditedAt = dateTimeProvider.UtcNow,
                PreviousMessage = previousMessage,
                NewMessage = message
            },
            cancellationToken);

        comment.Message = message;
        comment.IsEdited = true;
        comment.EditedAt = dateTimeProvider.UtcNow;
        commentRepository.Update(comment);

        var existingMentions = await paginationService.ToListAsync(mentionRepository.Query().Where(mention => mention.CommentId == comment.Id), cancellationToken);
        var requestedMentions = mentionedUserIds.ToHashSet();
        mentionRepository.RemoveRange(existingMentions.Where(mention => !requestedMentions.Contains(mention.MentionedUserId)));

        var currentMentions = existingMentions.Select(mention => mention.MentionedUserId).ToHashSet();
        var newMentionedUserIds = requestedMentions.Where(mentionId => !currentMentions.Contains(mentionId)).ToArray();
        foreach (var mentionId in newMentionedUserIds)
        {
            await mentionRepository.AddAsync(
                new WorkCommentMention
                {
                    Id = Guid.NewGuid(),
                    CommentId = comment.Id,
                    MentionedUserId = mentionId,
                    MentionedByUserId = authorId,
                    CreatedAt = dateTimeProvider.UtcNow
                },
                cancellationToken);
        }

        await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.CommentEdited, "Comment edited.", nameof(WorkComment.Message), previousMessage, message, dateTimeProvider, currentUserService, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (newMentionedUserIds.Length > 0)
        {
            await WorkNotifications.EnqueueAsync(
                notificationQueue,
                newMentionedUserIds,
                workItem.ProjectId,
                workItem.Id,
                NotificationType.Mention,
                NotificationPriority.High,
                "You were mentioned",
                $"{currentUserService.UserName ?? "A teammate"} mentioned you on {workItem.Title}.",
                WorkNotifications.CommentUrl(workItem.Id, comment.Id),
                "at-sign",
                currentUserService,
                cancellationToken);
        }

        return await WorkRules.ReadCommentDtoAsync(commentRepository, paginationService, currentUserId, comment.Id, cancellationToken);
    }
}

internal sealed class DeleteWorkItemCommentCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<WorkActivity> activityRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<DeleteWorkItemCommentCommand>
{
    public async Task Handle(DeleteWorkItemCommentCommand request, CancellationToken cancellationToken)
    {
        var workItem = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        var currentUserId = WorkRules.GetCurrentUserId(currentUserService);
        var comment = await WorkRules.GetCommentAsync(commentRepository, paginationService, request.WorkItemId, request.CommentId, cancellationToken);
        WorkRules.EnsureCanModerateOrOwnComment(comment, currentUserId, currentUserService);

        commentRepository.Remove(comment);
        await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.CommentDeleted, "Comment deleted.", null, null, null, dateTimeProvider, currentUserService, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class RestoreWorkItemCommentCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<WorkActivity> activityRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<RestoreWorkItemCommentCommand, WorkCommentDto>
{
    public async Task<WorkCommentDto> Handle(RestoreWorkItemCommentCommand request, CancellationToken cancellationToken)
    {
        var workItem = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        var currentUserId = WorkRules.GetCurrentUserId(currentUserService);
        var comment = await WorkRules.GetCommentAsync(commentRepository, paginationService, request.WorkItemId, request.CommentId, cancellationToken);
        WorkRules.EnsureCanModerateOrOwnComment(comment, currentUserId, currentUserService, allowDeleted: true);

        if (!comment.IsDeleted)
        {
            throw new ApplicationConflictException("Comment is not deleted.");
        }

        comment.IsDeleted = false;
        comment.DeletedOn = null;
        comment.DeletedBy = null;
        commentRepository.Update(comment);

        await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.CommentUpdated, "Comment restored.", null, null, null, dateTimeProvider, currentUserService, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await WorkRules.ReadCommentDtoAsync(commentRepository, paginationService, currentUserId, comment.Id, cancellationToken);
    }
}

internal sealed class PinWorkItemCommentCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<WorkActivity> activityRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<PinWorkItemCommentCommand, WorkCommentDto>
{
    public async Task<WorkCommentDto> Handle(PinWorkItemCommentCommand request, CancellationToken cancellationToken)
    {
        var workItem = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        if (!WorkRules.CanModerateComments(currentUserService))
        {
            throw new ApplicationForbiddenException("Only project managers and administrators can pin comments.");
        }

        var comment = await WorkRules.GetCommentAsync(commentRepository, paginationService, request.WorkItemId, request.CommentId, cancellationToken);
        WorkRules.EnsureCommentIsActive(comment);

        comment.Pinned = request.Pinned;
        commentRepository.Update(comment);

        await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, request.Pinned ? WorkActivityType.Pinned : WorkActivityType.Unpinned, request.Pinned ? "Comment pinned." : "Comment unpinned.", nameof(WorkComment.Pinned), (!request.Pinned).ToString(), request.Pinned.ToString(), dateTimeProvider, currentUserService, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await WorkRules.ReadCommentDtoAsync(commentRepository, paginationService, currentUserService.UserId, comment.Id, cancellationToken);
    }
}

internal sealed class ResolveWorkItemCommentCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<WorkActivity> activityRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<ResolveWorkItemCommentCommand, WorkCommentDto>
{
    public async Task<WorkCommentDto> Handle(ResolveWorkItemCommentCommand request, CancellationToken cancellationToken)
    {
        var workItem = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        var comment = await WorkRules.GetCommentAsync(commentRepository, paginationService, request.WorkItemId, request.CommentId, cancellationToken);
        WorkRules.EnsureCommentIsActive(comment);

        comment.Resolved = request.Resolved;
        commentRepository.Update(comment);

        await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, request.Resolved ? WorkActivityType.Resolved : WorkActivityType.Reopened, request.Resolved ? "Discussion resolved." : "Discussion reopened.", nameof(WorkComment.Resolved), (!request.Resolved).ToString(), request.Resolved.ToString(), dateTimeProvider, currentUserService, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await WorkRules.ReadCommentDtoAsync(commentRepository, paginationService, currentUserService.UserId, comment.Id, cancellationToken);
    }
}

internal sealed class ListWorkItemCommentHistoryQueryHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<WorkCommentHistory> historyRepository,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<ListWorkItemCommentHistoryQuery, PagedResult<WorkCommentHistoryDto>>
{
    public async Task<PagedResult<WorkCommentHistoryDto>> Handle(ListWorkItemCommentHistoryQuery request, CancellationToken cancellationToken)
    {
        _ = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        _ = await WorkRules.GetCommentAsync(commentRepository, paginationService, request.WorkItemId, request.CommentId, cancellationToken);

        var query = historyRepository.Query()
            .Where(history => history.CommentId == request.CommentId)
            .OrderByDescending(history => history.EditedAt)
            .Select(history => new WorkCommentHistoryDto(
                history.Id,
                history.CommentId,
                history.EditedBy,
                history.EditedByProfile == null ? history.EditedByName ?? "Unknown user" : history.EditedByProfile.FirstName + " " + history.EditedByProfile.LastName,
                history.EditedAt,
                history.PreviousMessage,
                history.NewMessage));

        return await paginationService.CreateAsync(query, request.Request.PageNumber, request.Request.PageSize, cancellationToken);
    }
}

internal sealed class ListWorkItemCommentAttachmentsQueryHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<WorkCommentAttachment> attachmentRepository,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<ListWorkItemCommentAttachmentsQuery, PagedResult<WorkCommentAttachmentDto>>
{
    public async Task<PagedResult<WorkCommentAttachmentDto>> Handle(ListWorkItemCommentAttachmentsQuery request, CancellationToken cancellationToken)
    {
        _ = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        _ = await WorkRules.GetCommentAsync(commentRepository, paginationService, request.WorkItemId, request.CommentId, cancellationToken);

        var query = attachmentRepository.Query()
            .Where(attachment => attachment.CommentId == request.CommentId)
            .OrderByDescending(attachment => attachment.CreatedOn)
            .Select(WorkItemQueries.ToCommentAttachmentDto());

        return await paginationService.CreateAsync(query, request.Request.PageNumber, request.Request.PageSize, cancellationToken);
    }
}

internal sealed class AddWorkItemCommentAttachmentCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<WorkCommentAttachment> attachmentRepository,
    IRepository<UserProfile> userProfileRepository,
    IRepository<WorkActivity> activityRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService,
    INotificationDispatcher notificationQueue)
    : IRequestHandler<AddWorkItemCommentAttachmentCommand, WorkCommentAttachmentDto>
{
    public async Task<WorkCommentAttachmentDto> Handle(AddWorkItemCommentAttachmentCommand request, CancellationToken cancellationToken)
    {
        var workItem = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        var currentUserId = WorkRules.GetCurrentUserId(currentUserService);
        var comment = await WorkRules.GetCommentAsync(commentRepository, paginationService, request.WorkItemId, request.CommentId, cancellationToken);
        WorkRules.EnsureCanEditComment(comment, currentUserId);

        var attachment = new WorkCommentAttachment
        {
            Id = Guid.NewGuid(),
            CommentId = comment.Id,
            FileName = request.Request.FileName.Trim(),
            ContentType = request.Request.ContentType.Trim(),
            FileSizeBytes = request.Request.FileSizeBytes,
            StoragePath = request.Request.StoragePath.Trim(),
            PreviewType = WorkRules.GetPreviewType(request.Request.ContentType, request.Request.FileName),
            UploadedBy = currentUserService.UserName ?? currentUserId.ToString(),
            UploadedAt = dateTimeProvider.UtcNow
        };

        await attachmentRepository.AddAsync(attachment, cancellationToken);
        await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.AttachmentAdded, "Comment attachment added.", nameof(WorkCommentAttachment.FileName), null, attachment.FileName, dateTimeProvider, currentUserService, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await WorkNotifications.EnqueueForWorkItemParticipantsAsync(
            notificationQueue,
            workItemRepository,
            userProfileRepository,
            paginationService,
            workItem.Id,
            workItem.ProjectId,
            comment.AuthorId.HasValue ? [comment.AuthorId.Value] : Array.Empty<Guid>(),
            NotificationType.AttachmentAdded,
            NotificationPriority.Normal,
            "Attachment uploaded",
            $"{currentUserService.UserName ?? "A teammate"} uploaded {attachment.FileName} in a comment on {workItem.Title}.",
            WorkNotifications.CommentUrl(workItem.Id, comment.Id),
            "paperclip",
            currentUserService,
            cancellationToken);

        return await paginationService.SingleOrDefaultAsync(
            attachmentRepository.Query().Where(entity => entity.Id == attachment.Id).Select(WorkItemQueries.ToCommentAttachmentDto()),
            cancellationToken)
            ?? throw new ApplicationNotFoundException("Comment attachment was not found.");
    }
}

internal sealed class DeleteWorkItemCommentAttachmentCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<WorkCommentAttachment> attachmentRepository,
    IRepository<WorkActivity> activityRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<DeleteWorkItemCommentAttachmentCommand>
{
    public async Task Handle(DeleteWorkItemCommentAttachmentCommand request, CancellationToken cancellationToken)
    {
        var workItem = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        var currentUserId = WorkRules.GetCurrentUserId(currentUserService);
        var comment = await WorkRules.GetCommentAsync(commentRepository, paginationService, request.WorkItemId, request.CommentId, cancellationToken);
        WorkRules.EnsureCanModerateOrOwnComment(comment, currentUserId, currentUserService);

        var attachment = await paginationService.SingleOrDefaultAsync(
            attachmentRepository.Query().Where(entity => entity.Id == request.AttachmentId && entity.CommentId == comment.Id),
            cancellationToken)
            ?? throw new ApplicationNotFoundException("Comment attachment was not found.");

        attachmentRepository.Remove(attachment);
        await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.AttachmentDeleted, "Comment attachment deleted.", nameof(WorkCommentAttachment.FileName), attachment.FileName, null, dateTimeProvider, currentUserService, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class ListWorkItemMentionCandidatesQueryHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<ProjectMember> projectMemberRepository,
    IUserManagementService userManagementService,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService,
    ICurrentUserService currentUserService)
    : IRequestHandler<ListWorkItemMentionCandidatesQuery, PagedResult<WorkMentionCandidateDto>>
{
    public async Task<PagedResult<WorkMentionCandidateDto>> Handle(ListWorkItemMentionCandidatesQuery request, CancellationToken cancellationToken)
    {
        var workItem = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        var currentUserId = WorkRules.GetCurrentUserId(currentUserService);
        await WorkRules.EnsureCurrentUserIsProjectMemberAsync(projectMemberRepository, paginationService, workItem.ProjectId, currentUserId, cancellationToken, projectAccessService.CanAccessAllProjects);

        return await userManagementService.ListProjectMentionCandidatesAsync(workItem.ProjectId, request.Request, cancellationToken);
    }
}

internal sealed class ListWorkItemCommentMentionsQueryHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<WorkCommentMention> mentionRepository,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<ListWorkItemCommentMentionsQuery, PagedResult<WorkCommentMentionDto>>
{
    public async Task<PagedResult<WorkCommentMentionDto>> Handle(ListWorkItemCommentMentionsQuery request, CancellationToken cancellationToken)
    {
        _ = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        _ = await WorkRules.GetCommentAsync(commentRepository, paginationService, request.WorkItemId, request.CommentId, cancellationToken);

        var query = mentionRepository.Query()
            .Where(mention => mention.CommentId == request.CommentId)
            .OrderBy(mention => mention.CreatedAt)
            .Select(WorkItemQueries.ToMentionDto());

        return await paginationService.CreateAsync(query, request.Request.PageNumber, request.Request.PageSize, cancellationToken);
    }
}

internal sealed class AddWorkItemCommentMentionsCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<WorkCommentMention> mentionRepository,
    IRepository<UserProfile> userProfileRepository,
    IRepository<ProjectMember> projectMemberRepository,
    IRepository<WorkActivity> activityRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService,
    INotificationDispatcher notificationQueue)
    : IRequestHandler<AddWorkItemCommentMentionsCommand, IReadOnlyCollection<WorkCommentMentionDto>>
{
    public async Task<IReadOnlyCollection<WorkCommentMentionDto>> Handle(AddWorkItemCommentMentionsCommand request, CancellationToken cancellationToken)
    {
        var workItem = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        var currentUserId = WorkRules.GetCurrentUserId(currentUserService);
        var mentionedByUserId = await WorkRules.EnsureCurrentUserIsProjectMemberAsync(projectMemberRepository, paginationService, workItem.ProjectId, currentUserId, cancellationToken, projectAccessService.CanAccessAllProjects);
        var comment = await WorkRules.GetCommentAsync(commentRepository, paginationService, request.WorkItemId, request.CommentId, cancellationToken);
        WorkRules.EnsureCommentIsActive(comment);

        var mentionedUserIds = await WorkRules.ResolveMentionedUserIdsAsync(
            userProfileRepository,
            projectMemberRepository,
            paginationService,
            workItem.ProjectId,
            request.Request.MentionedUserIds,
            Array.Empty<Guid>(),
            cancellationToken);

        var existingMentionedUserIds = (await paginationService.ToListAsync(
            mentionRepository.Query()
                .Where(mention => mention.CommentId == comment.Id && mentionedUserIds.Contains(mention.MentionedUserId))
                .Select(mention => mention.MentionedUserId),
            cancellationToken))
            .ToHashSet();

        var newMentionedUserIds = mentionedUserIds.Where(mentionedUserId => !existingMentionedUserIds.Contains(mentionedUserId)).ToArray();
        foreach (var mentionedUserId in newMentionedUserIds)
        {
            await mentionRepository.AddAsync(
                new WorkCommentMention
                {
                    Id = Guid.NewGuid(),
                    CommentId = comment.Id,
                    MentionedUserId = mentionedUserId,
                    MentionedByUserId = mentionedByUserId,
                    CreatedAt = dateTimeProvider.UtcNow
                },
                cancellationToken);
        }

        await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.MentionAdded, "Comment mention added.", nameof(WorkCommentMention.MentionedUserId), null, string.Join(",", mentionedUserIds), dateTimeProvider, currentUserService, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (newMentionedUserIds.Length > 0)
        {
            await WorkNotifications.EnqueueAsync(
                notificationQueue,
                newMentionedUserIds,
                workItem.ProjectId,
                workItem.Id,
                NotificationType.Mention,
                NotificationPriority.High,
                "You were mentioned",
                $"{currentUserService.UserName ?? "A teammate"} mentioned you on {workItem.Title}.",
                WorkNotifications.CommentUrl(workItem.Id, comment.Id),
                "at-sign",
                currentUserService,
                cancellationToken);
        }

        return await paginationService.ToListAsync(
            mentionRepository.Query()
                .Where(mention => mention.CommentId == comment.Id && mentionedUserIds.Contains(mention.MentionedUserId))
                .OrderBy(mention => mention.CreatedAt)
                .Select(WorkItemQueries.ToMentionDto()),
            cancellationToken);
    }
}

internal sealed class DeleteWorkItemCommentMentionCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<WorkCommentMention> mentionRepository,
    IRepository<WorkActivity> activityRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<DeleteWorkItemCommentMentionCommand>
{
    public async Task Handle(DeleteWorkItemCommentMentionCommand request, CancellationToken cancellationToken)
    {
        var workItem = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        var currentUserId = WorkRules.GetCurrentUserId(currentUserService);
        var comment = await WorkRules.GetCommentAsync(commentRepository, paginationService, request.WorkItemId, request.CommentId, cancellationToken);
        WorkRules.EnsureCanEditComment(comment, currentUserId);

        var mention = await paginationService.SingleOrDefaultAsync(
            mentionRepository.Query().Where(entity => entity.Id == request.MentionId && entity.CommentId == comment.Id),
            cancellationToken)
            ?? throw new ApplicationNotFoundException("Comment mention was not found.");

        mentionRepository.Remove(mention);
        await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.MentionRemoved, "Comment mention removed.", nameof(WorkCommentMention.MentionedUserId), mention.MentionedUserId.ToString(), null, dateTimeProvider, currentUserService, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class ListWorkItemCommentReactionsQueryHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<WorkCommentReaction> reactionRepository,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<ListWorkItemCommentReactionsQuery, PagedResult<WorkCommentReactionDto>>
{
    public async Task<PagedResult<WorkCommentReactionDto>> Handle(ListWorkItemCommentReactionsQuery request, CancellationToken cancellationToken)
    {
        _ = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        _ = await WorkRules.GetCommentAsync(commentRepository, paginationService, request.WorkItemId, request.CommentId, cancellationToken);

        var query = reactionRepository.Query()
            .Where(reaction => reaction.CommentId == request.CommentId)
            .OrderBy(reaction => reaction.Emoji)
            .ThenBy(reaction => reaction.CreatedAt)
            .Select(WorkItemQueries.ToReactionDto());

        return await paginationService.CreateAsync(query, request.Request.PageNumber, request.Request.PageSize, cancellationToken);
    }
}

internal sealed class ToggleWorkItemCommentReactionCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<WorkCommentReaction> reactionRepository,
    IRepository<WorkActivity> activityRepository,
    IRepository<ProjectMember> projectMemberRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<ToggleWorkItemCommentReactionCommand, IReadOnlyCollection<WorkCommentReactionDto>>
{
    public async Task<IReadOnlyCollection<WorkCommentReactionDto>> Handle(ToggleWorkItemCommentReactionCommand request, CancellationToken cancellationToken)
    {
        var workItem = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        var currentUserId = WorkRules.GetCurrentUserId(currentUserService);
        var reactionUserId = await WorkRules.EnsureCurrentUserIsProjectMemberAsync(projectMemberRepository, paginationService, workItem.ProjectId, currentUserId, cancellationToken, projectAccessService.CanAccessAllProjects);
        var emoji = WorkRules.EnsureSupportedReaction(request.Request.Emoji);
        var comment = await WorkRules.GetCommentAsync(commentRepository, paginationService, request.WorkItemId, request.CommentId, cancellationToken);
        WorkRules.EnsureCommentIsActive(comment);

        var existing = await paginationService.SingleOrDefaultAsync(
            reactionRepository.Query().Where(reaction => reaction.CommentId == comment.Id && reaction.UserId == reactionUserId && reaction.Emoji == emoji),
            cancellationToken);

        if (existing is null)
        {
            await reactionRepository.AddAsync(
                new WorkCommentReaction
                {
                    Id = Guid.NewGuid(),
                    CommentId = comment.Id,
                    UserId = reactionUserId,
                    Emoji = emoji,
                    CreatedAt = dateTimeProvider.UtcNow
                },
                cancellationToken);

            await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.ReactionAdded, "Reaction added.", nameof(WorkCommentReaction.Emoji), null, emoji, dateTimeProvider, currentUserService, cancellationToken);
        }
        else
        {
            reactionRepository.Remove(existing);
            await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.ReactionRemoved, "Reaction removed.", nameof(WorkCommentReaction.Emoji), emoji, null, dateTimeProvider, currentUserService, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await paginationService.ToListAsync(
            reactionRepository.Query()
                .Where(reaction => reaction.CommentId == comment.Id)
                .OrderBy(reaction => reaction.Emoji)
                .ThenBy(reaction => reaction.CreatedAt)
                .Select(WorkItemQueries.ToReactionDto()),
            cancellationToken);
    }
}

internal sealed class DeleteWorkItemCommentReactionCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<WorkCommentReaction> reactionRepository,
    IRepository<WorkActivity> activityRepository,
    IRepository<ProjectMember> projectMemberRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<DeleteWorkItemCommentReactionCommand>
{
    public async Task Handle(DeleteWorkItemCommentReactionCommand request, CancellationToken cancellationToken)
    {
        var workItem = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        var currentUserId = WorkRules.GetCurrentUserId(currentUserService);
        var reactionUserId = await WorkRules.EnsureCurrentUserIsProjectMemberAsync(projectMemberRepository, paginationService, workItem.ProjectId, currentUserId, cancellationToken, projectAccessService.CanAccessAllProjects);
        var emoji = WorkRules.EnsureSupportedReaction(request.Emoji);
        var comment = await WorkRules.GetCommentAsync(commentRepository, paginationService, request.WorkItemId, request.CommentId, cancellationToken);

        var existing = await paginationService.SingleOrDefaultAsync(
            reactionRepository.Query().Where(reaction => reaction.CommentId == comment.Id && reaction.UserId == reactionUserId && reaction.Emoji == emoji),
            cancellationToken)
            ?? throw new ApplicationNotFoundException("Comment reaction was not found.");

        reactionRepository.Remove(existing);
        await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.ReactionRemoved, "Reaction removed.", nameof(WorkCommentReaction.Emoji), emoji, null, dateTimeProvider, currentUserService, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class ListWorkItemCommentReadsQueryHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<WorkCommentRead> readRepository,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<ListWorkItemCommentReadsQuery, PagedResult<WorkCommentReadDto>>
{
    public async Task<PagedResult<WorkCommentReadDto>> Handle(ListWorkItemCommentReadsQuery request, CancellationToken cancellationToken)
    {
        _ = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        _ = await WorkRules.GetCommentAsync(commentRepository, paginationService, request.WorkItemId, request.CommentId, cancellationToken);

        var query = readRepository.Query()
            .Where(read => read.CommentId == request.CommentId)
            .OrderByDescending(read => read.ReadAt)
            .Select(WorkItemQueries.ToReadReceiptDto());

        return await paginationService.CreateAsync(query, request.Request.PageNumber, request.Request.PageSize, cancellationToken);
    }
}

internal sealed class MarkWorkItemCommentReadCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<WorkCommentRead> readRepository,
    IRepository<ProjectMember> projectMemberRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<MarkWorkItemCommentReadCommand, WorkCommentReadDto>
{
    public async Task<WorkCommentReadDto> Handle(MarkWorkItemCommentReadCommand request, CancellationToken cancellationToken)
    {
        var workItem = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        var currentUserId = WorkRules.GetCurrentUserId(currentUserService);
        var readUserId = await WorkRules.EnsureCurrentUserIsProjectMemberAsync(projectMemberRepository, paginationService, workItem.ProjectId, currentUserId, cancellationToken, projectAccessService.CanAccessAllProjects);
        var comment = await WorkRules.GetCommentAsync(commentRepository, paginationService, request.WorkItemId, request.CommentId, cancellationToken);

        var read = await paginationService.SingleOrDefaultAsync(
            readRepository.Query().Where(entity => entity.CommentId == comment.Id && entity.UserId == readUserId),
            cancellationToken);

        if (read is null)
        {
            read = new WorkCommentRead
            {
                Id = Guid.NewGuid(),
                CommentId = comment.Id,
                UserId = readUserId,
                ReadAt = dateTimeProvider.UtcNow
            };
            await readRepository.AddAsync(read, cancellationToken);
        }
        else
        {
            read.ReadAt = dateTimeProvider.UtcNow;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await paginationService.SingleOrDefaultAsync(
            readRepository.Query()
                .Where(entity => entity.Id == read.Id)
                .Select(WorkItemQueries.ToReadReceiptDto()),
            cancellationToken)
            ?? throw new ApplicationNotFoundException("Read receipt was not found.");
    }
}

internal sealed class MarkWorkItemCommentsReadCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkComment> commentRepository,
    IRepository<WorkCommentRead> readRepository,
    IRepository<ProjectMember> projectMemberRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<MarkWorkItemCommentsReadCommand>
{
    public async Task Handle(MarkWorkItemCommentsReadCommand request, CancellationToken cancellationToken)
    {
        var workItem = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        var currentUserId = WorkRules.GetCurrentUserId(currentUserService);
        var readUserId = await WorkRules.EnsureCurrentUserIsProjectMemberAsync(projectMemberRepository, paginationService, workItem.ProjectId, currentUserId, cancellationToken, projectAccessService.CanAccessAllProjects);
        var now = dateTimeProvider.UtcNow;

        var commentIds = await paginationService.ToListAsync(
            commentRepository.Query()
                .Where(comment => comment.WorkItemId == request.WorkItemId && !comment.IsDeleted)
                .Select(comment => comment.Id),
            cancellationToken);

        if (commentIds.Count == 0)
        {
            return;
        }

        var existingReads = await paginationService.ToListAsync(
            readRepository.Query().Where(read => commentIds.Contains(read.CommentId) && read.UserId == readUserId),
            cancellationToken);

        var existingByCommentId = existingReads.ToDictionary(read => read.CommentId);
        foreach (var commentId in commentIds)
        {
            if (existingByCommentId.TryGetValue(commentId, out var existingRead))
            {
                existingRead.ReadAt = now;
            }
            else
            {
                await readRepository.AddAsync(
                    new WorkCommentRead
                    {
                        Id = Guid.NewGuid(),
                        CommentId = commentId,
                        UserId = readUserId,
                        ReadAt = now
                    },
                    cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class ListWorkItemAttachmentsQueryHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkAttachment> attachmentRepository,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<ListWorkItemAttachmentsQuery, PagedResult<WorkAttachmentDto>>
{
    public async Task<PagedResult<WorkAttachmentDto>> Handle(ListWorkItemAttachmentsQuery request, CancellationToken cancellationToken)
    {
        _ = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        var query = attachmentRepository.Query().Where(attachment => attachment.WorkItemId == request.WorkItemId);
        query = request.Request.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true
            ? query.OrderByDescending(attachment => attachment.CreatedOn)
            : query.OrderBy(attachment => attachment.CreatedOn);

        return await paginationService.CreateAsync(query.Select(WorkItemQueries.ToAttachmentDto()), request.Request.PageNumber, request.Request.PageSize, cancellationToken);
    }
}

internal sealed class AddWorkItemAttachmentCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkAttachment> attachmentRepository,
    IRepository<UserProfile> userProfileRepository,
    IRepository<WorkActivity> activityRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService,
    INotificationDispatcher notificationQueue)
    : IRequestHandler<AddWorkItemAttachmentCommand, WorkAttachmentDto>
{
    public async Task<WorkAttachmentDto> Handle(AddWorkItemAttachmentCommand request, CancellationToken cancellationToken)
    {
        var workItem = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        var attachment = new WorkAttachment
        {
            Id = Guid.NewGuid(),
            WorkItemId = request.WorkItemId,
            FileName = request.Request.FileName.Trim(),
            ContentType = request.Request.ContentType.Trim(),
            FileSizeBytes = request.Request.FileSizeBytes,
            StoragePath = request.Request.StoragePath.Trim(),
            Version = request.Request.Version,
            Description = string.IsNullOrWhiteSpace(request.Request.Description) ? null : request.Request.Description.Trim()
        };

        await attachmentRepository.AddAsync(attachment, cancellationToken);
        await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.AttachmentAdded, "Attachment added.", nameof(WorkAttachment.FileName), null, attachment.FileName, dateTimeProvider, currentUserService, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await WorkNotifications.EnqueueForWorkItemParticipantsAsync(
            notificationQueue,
            workItemRepository,
            userProfileRepository,
            paginationService,
            workItem.Id,
            workItem.ProjectId,
            Array.Empty<Guid>(),
            NotificationType.AttachmentAdded,
            NotificationPriority.Normal,
            "Attachment uploaded",
            $"{currentUserService.UserName ?? "A teammate"} uploaded {attachment.FileName} to {workItem.Title}.",
            WorkNotifications.WorkItemUrl(workItem.Id),
            "paperclip",
            currentUserService,
            cancellationToken);

        return await paginationService.SingleOrDefaultAsync(attachmentRepository.Query().Where(entity => entity.Id == attachment.Id).Select(WorkItemQueries.ToAttachmentDto()), cancellationToken)
            ?? throw new ApplicationNotFoundException("Attachment was not found.");
    }
}

internal sealed class DeleteWorkItemAttachmentCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkAttachment> attachmentRepository,
    IRepository<WorkActivity> activityRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<DeleteWorkItemAttachmentCommand>
{
    public async Task Handle(DeleteWorkItemAttachmentCommand request, CancellationToken cancellationToken)
    {
        var workItem = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        var attachment = await attachmentRepository.GetByIdAsync(request.AttachmentId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Attachment was not found.");
        if (attachment.WorkItemId != request.WorkItemId)
        {
            throw new ApplicationNotFoundException("Attachment was not found.");
        }

        attachmentRepository.Remove(attachment);
        await WorkActivities.AddAsync(activityRepository, workItem.ProjectId, workItem.Id, WorkActivityType.AttachmentDeleted, "Attachment deleted.", nameof(WorkAttachment.FileName), attachment.FileName, null, dateTimeProvider, currentUserService, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class ListWorkItemActivityQueryHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkActivity> activityRepository,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<ListWorkItemActivityQuery, PagedResult<WorkActivityDto>>
{
    public async Task<PagedResult<WorkActivityDto>> Handle(ListWorkItemActivityQuery request, CancellationToken cancellationToken)
    {
        _ = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);

        var query = activityRepository.Query()
            .Where(activity => activity.WorkItemId == request.WorkItemId);

        if (request.Category.HasValue)
        {
            query = query.Where(activity => activity.Category == request.Category.Value);
        }

        if (request.ActivityType.HasValue)
        {
            query = query.Where(activity => activity.ActivityType == request.ActivityType.Value);
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(activity => activity.CreatedAt >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(activity => activity.CreatedAt <= request.DateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
        {
            var search = request.Request.Search.Trim();
            query = query.Where(activity =>
                activity.Description.Contains(search)
                || (activity.OldValue != null && activity.OldValue.Contains(search))
                || (activity.NewValue != null && activity.NewValue.Contains(search))
                || (activity.FieldName != null && activity.FieldName.Contains(search))
                || (activity.CreatedBy != null && activity.CreatedBy.Contains(search))
                || (activity.User != null && (activity.User.FirstName + " " + activity.User.LastName).Contains(search)));
        }

        return await paginationService.CreateAsync(
            query
                .OrderByDescending(activity => activity.CreatedAt)
                .Select(activity => new WorkActivityDto(
                    activity.Id,
                    activity.ProjectId,
                    activity.WorkItemId,
                    activity.UserId,
                    activity.User == null ? activity.CreatedBy ?? "System" : activity.User.FirstName + " " + activity.User.LastName,
                    activity.User == null ? null : activity.User.Designation,
                    activity.User == null ? null : activity.User.ProfilePhoto,
                    activity.ActivityType,
                    activity.Category,
                    activity.Description,
                    activity.FieldName,
                    activity.OldValue,
                    activity.NewValue,
                    activity.CreatedAt)),
            request.Request.PageNumber,
            request.Request.PageSize,
            cancellationToken);
    }
}

internal sealed class ListWorkItemLinksQueryHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkItemLink> linkRepository,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<ListWorkItemLinksQuery, PagedResult<WorkItemLinkDto>>
{
    public async Task<PagedResult<WorkItemLinkDto>> Handle(ListWorkItemLinksQuery request, CancellationToken cancellationToken)
    {
        _ = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);

        return await paginationService.CreateAsync(
            linkRepository.Query()
                .Where(link => link.SourceWorkItemId == request.WorkItemId || link.TargetWorkItemId == request.WorkItemId)
                .OrderBy(link => link.LinkType)
                .Select(link => new WorkItemLinkDto(
                    link.Id,
                    link.SourceWorkItemId,
                    link.TargetWorkItemId,
                    link.TargetWorkItem.Title,
                    link.LinkType,
                    link.Description)),
            request.Request.PageNumber,
            request.Request.PageSize,
            cancellationToken);
    }
}

internal sealed class AddWorkItemLinkCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkItemLink> linkRepository,
    IRepository<WorkActivity> activityRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<AddWorkItemLinkCommand, WorkItemLinkDto>
{
    public async Task<WorkItemLinkDto> Handle(AddWorkItemLinkCommand request, CancellationToken cancellationToken)
    {
        var source = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        var target = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.Request.TargetWorkItemId, cancellationToken);

        if (source.Id == target.Id)
        {
            throw new ApplicationConflictException("A work item cannot be linked to itself.");
        }

        var existingId = await paginationService.SingleOrDefaultAsync(
            linkRepository.Query()
                .Where(link => link.SourceWorkItemId == source.Id
                    && link.TargetWorkItemId == target.Id
                    && link.LinkType == request.Request.LinkType)
                .Select(link => (Guid?)link.Id),
            cancellationToken);

        if (existingId.HasValue)
        {
            throw new ApplicationConflictException("Work item link already exists.");
        }

        var link = new WorkItemLink
        {
            Id = Guid.NewGuid(),
            SourceWorkItemId = source.Id,
            TargetWorkItemId = target.Id,
            LinkType = request.Request.LinkType,
            Description = string.IsNullOrWhiteSpace(request.Request.Description) ? null : request.Request.Description.Trim()
        };

        await linkRepository.AddAsync(link, cancellationToken);
        await WorkActivities.AddAsync(activityRepository, source.ProjectId, source.Id, WorkActivityType.Linked, "Work item linked.", nameof(WorkItemLink.LinkType), null, request.Request.LinkType.ToString(), dateTimeProvider, currentUserService, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new WorkItemLinkDto(link.Id, link.SourceWorkItemId, link.TargetWorkItemId, target.Title, link.LinkType, link.Description);
    }
}

internal sealed class DeleteWorkItemLinkCommandHandler(
    IRepository<WorkItem> workItemRepository,
    IRepository<WorkItemLink> linkRepository,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<DeleteWorkItemLinkCommand>
{
    public async Task Handle(DeleteWorkItemLinkCommand request, CancellationToken cancellationToken)
    {
        _ = await WorkRules.GetWorkItemAsync(workItemRepository, paginationService, projectAccessService, request.WorkItemId, cancellationToken);
        var link = await linkRepository.GetByIdAsync(request.LinkId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Work item link was not found.");
        if (link.SourceWorkItemId != request.WorkItemId && link.TargetWorkItemId != request.WorkItemId)
        {
            throw new ApplicationNotFoundException("Work item link was not found.");
        }

        linkRepository.Remove(link);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class ListWorkSavedFiltersQueryHandler(
    IRepository<WorkSavedFilter> repository,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<ListWorkSavedFiltersQuery, PagedResult<WorkSavedFilterDto>>
{
    public async Task<PagedResult<WorkSavedFilterDto>> Handle(ListWorkSavedFiltersQuery request, CancellationToken cancellationToken)
    {
        var query = repository.Query();
        if (request.ProjectId.HasValue)
        {
            await projectAccessService.EnsureCanAccessProjectAsync(request.ProjectId.Value, cancellationToken);
            query = query.Where(filter => filter.ProjectId == request.ProjectId.Value || filter.ProjectId == null);
        }
        else if (!projectAccessService.CanAccessAllProjects)
        {
            var accessibleProjectIds = await projectAccessService.GetAccessibleProjectIdsAsync(cancellationToken);
            query = query.Where(filter => filter.ProjectId == null || (filter.ProjectId.HasValue && accessibleProjectIds.Contains(filter.ProjectId.Value)));
        }

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
        {
            var term = request.Request.Search.Trim();
            query = query.Where(filter => filter.Name.Contains(term));
        }

        return await paginationService.CreateAsync(
            query.OrderBy(filter => filter.Name)
                .Select(filter => new WorkSavedFilterDto(filter.Id, filter.ProjectId, filter.OwnerUserId, filter.Name, filter.QueryJson, filter.IsShared, filter.CreatedBy, filter.CreatedOn)),
            request.Request.PageNumber,
            request.Request.PageSize,
            cancellationToken);
    }
}

internal sealed class CreateWorkSavedFilterCommandHandler(
    IRepository<Project> projectRepository,
    IRepository<WorkSavedFilter> repository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IProjectAccessService projectAccessService)
    : IRequestHandler<CreateWorkSavedFilterCommand, WorkSavedFilterDto>
{
    public async Task<WorkSavedFilterDto> Handle(CreateWorkSavedFilterCommand request, CancellationToken cancellationToken)
    {
        if (request.Request.ProjectId.HasValue)
        {
            _ = await projectRepository.GetByIdAsync(request.Request.ProjectId.Value, cancellationToken)
                ?? throw new ApplicationNotFoundException("Project was not found.");
            await projectAccessService.EnsureCanAccessProjectAsync(request.Request.ProjectId.Value, cancellationToken);
        }

        var filter = new WorkSavedFilter
        {
            Id = Guid.NewGuid(),
            ProjectId = request.Request.ProjectId,
            OwnerUserId = currentUserService.UserId,
            Name = request.Request.Name.Trim(),
            QueryJson = request.Request.QueryJson.Trim(),
            IsShared = request.Request.IsShared
        };

        await repository.AddAsync(filter, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new WorkSavedFilterDto(filter.Id, filter.ProjectId, filter.OwnerUserId, filter.Name, filter.QueryJson, filter.IsShared, filter.CreatedBy, filter.CreatedOn);
    }
}

internal sealed class DeleteWorkSavedFilterCommandHandler(
    IRepository<WorkSavedFilter> repository,
    IUnitOfWork unitOfWork,
    IProjectAccessService projectAccessService)
    : IRequestHandler<DeleteWorkSavedFilterCommand>
{
    public async Task Handle(DeleteWorkSavedFilterCommand request, CancellationToken cancellationToken)
    {
        var filter = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ApplicationNotFoundException("Saved filter was not found.");
        if (filter.ProjectId.HasValue)
        {
            await projectAccessService.EnsureCanAccessProjectAsync(filter.ProjectId.Value, cancellationToken);
        }

        repository.Remove(filter);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal static class WorkItemQueries
{
    public static IQueryable<WorkItem> ApplySearch(IQueryable<WorkItem> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var term = search.Trim();
        return query.Where(workItem =>
            workItem.Title.Contains(term)
            || (workItem.Description != null && workItem.Description.Contains(term))
            || (workItem.AcceptanceCriteria != null && workItem.AcceptanceCriteria.Contains(term))
            || workItem.Project.Name.Contains(term)
            || workItem.Type.Name.Contains(term)
            || workItem.Status.Name.Contains(term)
            || workItem.Priority.Name.Contains(term)
            || workItem.Reporter.FirstName.Contains(term)
            || workItem.Reporter.LastName.Contains(term)
            || (workItem.Assignee != null && (workItem.Assignee.FirstName.Contains(term) || workItem.Assignee.LastName.Contains(term))));
    }

    public static IQueryable<WorkItem> ApplySort(IQueryable<WorkItem> query, string? sortBy, string? direction)
    {
        var descending = direction?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;

        return (sortBy?.ToLowerInvariant(), descending) switch
        {
            ("type", true) => query.OrderByDescending(workItem => workItem.Type.Name),
            ("type", false) => query.OrderBy(workItem => workItem.Type.Name),
            ("status", true) => query.OrderByDescending(workItem => workItem.Status.SortOrder),
            ("status", false) => query.OrderBy(workItem => workItem.Status.SortOrder),
            ("priority", true) => query.OrderByDescending(workItem => workItem.Priority.SortOrder),
            ("priority", false) => query.OrderBy(workItem => workItem.Priority.SortOrder),
            ("duedate", true) => query.OrderByDescending(workItem => workItem.DueDate),
            ("duedate", false) => query.OrderBy(workItem => workItem.DueDate),
            ("createdon", true) => query.OrderByDescending(workItem => workItem.CreatedOn),
            ("createdon", false) => query.OrderBy(workItem => workItem.CreatedOn),
            ("title", true) => query.OrderByDescending(workItem => workItem.Title),
            _ => query.OrderBy(workItem => workItem.Title)
        };
    }

    public static System.Linq.Expressions.Expression<Func<WorkItem, WorkItemReadRow>> ToReadRow()
    {
        return workItem => new WorkItemReadRow(
            workItem.Id,
            workItem.ProjectId,
            workItem.Project.Name,
            workItem.ParentWorkItemId,
            workItem.ParentWorkItem == null ? null : workItem.ParentWorkItem.Title,
            workItem.WorkItemTypeId,
            workItem.Type.Name,
            workItem.Type.Icon,
            workItem.Type.Color,
            workItem.WorkflowStatusId,
            workItem.Status.Name,
            workItem.Status.Color,
            workItem.PriorityId,
            workItem.Priority.Name,
            workItem.Priority.Color,
            workItem.Title,
            workItem.Description,
            workItem.AcceptanceCriteria,
            workItem.AssigneeUserProfileId,
            workItem.Assignee == null ? null : workItem.Assignee.FirstName + " " + workItem.Assignee.LastName,
            workItem.ReporterUserProfileId,
            workItem.Reporter.FirstName + " " + workItem.Reporter.LastName,
            workItem.StartDate,
            workItem.DueDate,
            workItem.EstimatedHours,
            workItem.LoggedHours,
            workItem.StoryPoints,
            workItem.SprintId,
            workItem.Labels.Select(label => new WorkLabelDto(label.Label.Id, label.Label.OrganizationId, label.Label.Name, label.Label.Color, label.Label.Description, label.Label.IsActive)).ToArray(),
            workItem.Components.Select(component => new WorkComponentDto(component.Component.Id, component.Component.ProjectId, component.Component.Name, component.Component.Description, component.Component.Color, component.Component.IsActive)).ToArray(),
            workItem.Environment,
            workItem.Severity,
            workItem.Reproducible,
            workItem.Comments.Count,
            workItem.Attachments.Count,
            workItem.Watchers
                .OrderBy(watcher => watcher.UserProfile.FirstName)
                .ThenBy(watcher => watcher.UserProfile.LastName)
                .Select(watcher => new WorkWatcherDto(
                    watcher.UserProfileId,
                    watcher.UserProfile.FirstName + " " + watcher.UserProfile.LastName,
                    watcher.AddedOn,
                    watcher.AddedBy))
                .ToArray(),
            workItem.Watchers.Count,
            workItem.RowVersion,
            workItem.CreatedBy,
            workItem.CreatedOn,
            workItem.UpdatedBy,
            workItem.UpdatedOn);
    }

    public static WorkItemDto ToDto(WorkItemReadRow row)
    {
        return new WorkItemDto(
            row.Id,
            row.ProjectId,
            row.ProjectName,
            row.ParentWorkItemId,
            row.ParentTitle,
            row.WorkItemTypeId,
            row.TypeName,
            row.TypeIcon,
            row.TypeColor,
            row.WorkflowStatusId,
            row.StatusName,
            row.StatusColor,
            row.PriorityId,
            row.PriorityName,
            row.PriorityColor,
            row.Title,
            row.Description,
            row.AcceptanceCriteria,
            row.AssigneeUserProfileId,
            row.AssigneeName,
            row.ReporterUserProfileId,
            row.ReporterName,
            row.StartDate,
            row.DueDate,
            row.EstimatedHours,
            row.LoggedHours,
            row.StoryPoints,
            row.SprintId,
            row.Labels,
            row.Components,
            row.Environment,
            row.Severity,
            row.Reproducible,
            row.CommentCount,
            row.AttachmentCount,
            row.Watchers,
            row.WatcherCount,
            Convert.ToBase64String(row.RowVersion),
            row.CreatedBy,
            row.CreatedOn,
            row.UpdatedBy,
            row.UpdatedOn);
    }

    public static async Task<WorkItemDto> ReadDtoAsync(
        IRepository<WorkItem> repository,
        IPaginationService paginationService,
        IProjectAccessService projectAccessService,
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = repository.Query().Where(workItem => workItem.Id == id);
        if (!projectAccessService.CanAccessAllProjects)
        {
            var userId = projectAccessService.CurrentUserId;
            query = query.Where(workItem => workItem.Project.Members.Any(member => member.UserId == userId && member.IsActive));
        }

        var row = await paginationService.SingleOrDefaultAsync(
            query
                .Select(ToReadRow()),
            cancellationToken);

        return row is null ? throw new ApplicationNotFoundException("Work item was not found.") : ToDto(row);
    }

    public static System.Linq.Expressions.Expression<Func<WorkComment, WorkCommentDto>> ToCommentDto(Guid? currentUserId = null)
    {
        var userId = currentUserId ?? Guid.Empty;
        return comment => new WorkCommentDto(
            comment.Id,
            comment.WorkItemId,
            comment.ParentCommentId,
            comment.AuthorId,
            comment.Author == null ? comment.CreatedBy ?? "Unknown user" : comment.Author.FirstName + " " + comment.Author.LastName,
            comment.Author == null ? null : comment.Author.Designation,
            comment.IsDeleted ? string.Empty : comment.Message,
            comment.IsDeleted ? string.Empty : comment.Message,
            comment.IsEdited,
            comment.EditedAt,
            comment.IsDeleted,
            comment.DeletedOn,
            comment.Pinned,
            comment.Resolved,
            comment.Replies.Count(reply => !reply.IsDeleted),
            comment.Mentions.Select(mention => mention.MentionedUser.Id).ToArray(),
            comment.Mentions.Select(mention => mention.MentionedUserId).ToArray(),
            comment.Mentions
                .OrderBy(mention => mention.CreatedAt)
                .Select(mention => new WorkCommentMentionDto(
                    mention.Id,
                    mention.CommentId,
                    mention.MentionedUserId,
                    mention.MentionedUser.Id,
                    mention.MentionedUser.FirstName + " " + mention.MentionedUser.LastName,
                    mention.MentionedUser.EmployeeCode,
                    mention.MentionedUser.Designation,
                    mention.MentionedUser.Department == null ? null : mention.MentionedUser.Department.Name,
                    mention.MentionedUser.ProfilePhoto,
                    mention.MentionedByUserId,
                    mention.CreatedAt))
                .ToArray(),
            comment.Attachments
                .Where(attachment => !attachment.IsDeleted)
                .OrderBy(attachment => attachment.FileName)
                .Select(attachment => new WorkCommentAttachmentDto(
                    attachment.Id,
                    attachment.CommentId,
                    attachment.FileName,
                    attachment.ContentType,
                    attachment.FileSizeBytes,
                    attachment.StoragePath,
                    attachment.PreviewType,
                    attachment.UploadedBy,
                    attachment.UploadedAt))
                .ToArray(),
            comment.Reactions
                .OrderBy(reaction => reaction.Emoji)
                .ThenBy(reaction => reaction.CreatedAt)
                .Select(reaction => new WorkCommentReactionDto(
                    reaction.Id,
                    reaction.CommentId,
                    reaction.UserId,
                    reaction.User.FirstName + " " + reaction.User.LastName,
                    reaction.Emoji,
                    reaction.CreatedAt))
                .ToArray(),
            comment.Reads
                .OrderByDescending(read => read.ReadAt)
                .Take(12)
                .Select(read => new WorkCommentReadDto(
                    read.Id,
                    read.CommentId,
                    read.UserId,
                    read.User.FirstName + " " + read.User.LastName,
                    read.ReadAt))
                .ToArray(),
            new WorkCommentCountersDto(
                comment.Replies.Count(reply => !reply.IsDeleted),
                comment.Mentions.Count,
                comment.Reactions.Count,
                comment.Attachments.Count(attachment => !attachment.IsDeleted),
                comment.Reads.Count),
            comment.AuthorId == userId || comment.Reads.Any(read => read.UserId == userId),
            comment.CreatedBy,
            comment.CreatedOn,
            comment.UpdatedBy,
            comment.UpdatedOn);
    }

    public static System.Linq.Expressions.Expression<Func<WorkCommentMention, WorkCommentMentionDto>> ToMentionDto()
    {
        return mention => new WorkCommentMentionDto(
            mention.Id,
            mention.CommentId,
            mention.MentionedUserId,
            mention.MentionedUser.Id,
            mention.MentionedUser.FirstName + " " + mention.MentionedUser.LastName,
            mention.MentionedUser.EmployeeCode,
            mention.MentionedUser.Designation,
            mention.MentionedUser.Department == null ? null : mention.MentionedUser.Department.Name,
            mention.MentionedUser.ProfilePhoto,
            mention.MentionedByUserId,
            mention.CreatedAt);
    }

    public static System.Linq.Expressions.Expression<Func<WorkCommentReaction, WorkCommentReactionDto>> ToReactionDto()
    {
        return reaction => new WorkCommentReactionDto(
            reaction.Id,
            reaction.CommentId,
            reaction.UserId,
            reaction.User.FirstName + " " + reaction.User.LastName,
            reaction.Emoji,
            reaction.CreatedAt);
    }

    public static System.Linq.Expressions.Expression<Func<WorkCommentRead, WorkCommentReadDto>> ToReadReceiptDto()
    {
        return read => new WorkCommentReadDto(
            read.Id,
            read.CommentId,
            read.UserId,
            read.User.FirstName + " " + read.User.LastName,
            read.ReadAt);
    }

    public static System.Linq.Expressions.Expression<Func<WorkCommentAttachment, WorkCommentAttachmentDto>> ToCommentAttachmentDto()
    {
        return attachment => new WorkCommentAttachmentDto(
            attachment.Id,
            attachment.CommentId,
            attachment.FileName,
            attachment.ContentType,
            attachment.FileSizeBytes,
            attachment.StoragePath,
            attachment.PreviewType,
            attachment.UploadedBy,
            attachment.UploadedAt);
    }

    public static System.Linq.Expressions.Expression<Func<WorkAttachment, WorkAttachmentDto>> ToAttachmentDto()
    {
        return attachment => new WorkAttachmentDto(
            attachment.Id,
            attachment.WorkItemId,
            attachment.FileName,
            attachment.ContentType,
            attachment.FileSizeBytes,
            attachment.StoragePath,
            attachment.Version,
            attachment.Description,
            attachment.CreatedBy,
            attachment.CreatedOn);
    }
}

internal sealed record WorkItemReadRow(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    Guid? ParentWorkItemId,
    string? ParentTitle,
    Guid WorkItemTypeId,
    string TypeName,
    string TypeIcon,
    string TypeColor,
    Guid WorkflowStatusId,
    string StatusName,
    string StatusColor,
    Guid PriorityId,
    string PriorityName,
    string PriorityColor,
    string Title,
    string? Description,
    string? AcceptanceCriteria,
    Guid? AssigneeUserProfileId,
    string? AssigneeName,
    Guid ReporterUserProfileId,
    string ReporterName,
    DateOnly? StartDate,
    DateOnly? DueDate,
    decimal? EstimatedHours,
    decimal LoggedHours,
    int? StoryPoints,
    Guid? SprintId,
    IReadOnlyCollection<WorkLabelDto> Labels,
    IReadOnlyCollection<WorkComponentDto> Components,
    string? Environment,
    string? Severity,
    bool? Reproducible,
    int CommentCount,
    int AttachmentCount,
    IReadOnlyCollection<WorkWatcherDto> Watchers,
    int WatcherCount,
    byte[] RowVersion,
    string? CreatedBy,
    DateTimeOffset CreatedOn,
    string? UpdatedBy,
    DateTimeOffset? UpdatedOn);

internal sealed record WorkItemContext(Guid Id, Guid ProjectId, Guid OrganizationId, string Title);

internal static class WorkRules
{
    private static readonly HashSet<string> SupportedReactions = new(StringComparer.Ordinal)
    {
        "\ud83d\udc4d",
        "\u2764\ufe0f",
        "\ud83d\ude02",
        "\ud83d\udd25",
        "\ud83c\udf89",
        "\ud83d\ude80",
        "\ud83d\udc40",
        "\ud83d\ude04",
        "\ud83d\ude22",
        "\u2757"
    };

    public static async Task<Project> GetProjectAsync(IRepository<Project> repository, Guid projectId, CancellationToken cancellationToken)
    {
        return await repository.GetByIdAsync(projectId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Project was not found.");
    }

    public static async Task<WorkItemContext> GetWorkItemAsync(
        IRepository<WorkItem> repository,
        IPaginationService paginationService,
        IProjectAccessService projectAccessService,
        Guid workItemId,
        CancellationToken cancellationToken)
    {
        var workItem = await paginationService.SingleOrDefaultAsync(
            repository.Query()
                .Where(entity => entity.Id == workItemId)
                .Select(entity => new WorkItemContext(entity.Id, entity.ProjectId, entity.Project.OrganizationId, entity.Title)),
            cancellationToken);

        if (workItem is null)
        {
            throw new ApplicationNotFoundException("Work item was not found.");
        }

        await projectAccessService.EnsureCanAccessProjectAsync(workItem.ProjectId, cancellationToken);

        return workItem;
    }

    public static async Task<WorkComment> GetCommentAsync(
        IRepository<WorkComment> repository,
        IPaginationService paginationService,
        Guid workItemId,
        Guid commentId,
        CancellationToken cancellationToken)
    {
        return await paginationService.SingleOrDefaultAsync(
            repository.Query().Where(comment => comment.Id == commentId && comment.WorkItemId == workItemId),
            cancellationToken)
            ?? throw new ApplicationNotFoundException("Comment was not found.");
    }

    public static async Task<WorkCommentDto> ReadCommentDtoAsync(
        IRepository<WorkComment> repository,
        IPaginationService paginationService,
        Guid? currentUserId,
        Guid commentId,
        CancellationToken cancellationToken)
    {
        return await paginationService.SingleOrDefaultAsync(
            repository.Query().Where(comment => comment.Id == commentId).Select(WorkItemQueries.ToCommentDto(currentUserId)),
            cancellationToken)
            ?? throw new ApplicationNotFoundException("Comment was not found.");
    }

    public static Guid GetCurrentUserId(ICurrentUserService currentUserService)
    {
        return currentUserService.UserId
            ?? throw new ApplicationUnauthorizedException("Authenticated user id claim is missing or invalid.");
    }

    public static async Task<Guid?> ResolveActiveProfileUserIdAsync(
        IRepository<UserProfile> repository,
        IPaginationService paginationService,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await paginationService.SingleOrDefaultAsync(
            repository.Query()
                .Where(profile => profile.UserId == userId && profile.IsActive)
                .Select(profile => (Guid?)profile.UserId),
            cancellationToken);
    }

    public static async Task<Guid> EnsureCurrentUserIsProjectMemberAsync(
        IRepository<ProjectMember> repository,
        IPaginationService paginationService,
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken,
        bool bypassMembershipCheck = false)
    {
        if (bypassMembershipCheck)
        {
            return userId;
        }

        var matchedUserId = await paginationService.SingleOrDefaultAsync(
            repository.Query()
                .Where(member => member.ProjectId == projectId
                    && member.UserId == userId
                    && member.IsActive
                    && member.UserProfile != null
                    && member.UserProfile.IsActive)
                .Select(member => (Guid?)member.UserId),
            cancellationToken);

        return matchedUserId ?? throw new ApplicationForbiddenException("Only active project members can collaborate on comments.");
    }

    public static async Task<IReadOnlyCollection<Guid>> ResolveMentionedUserIdsAsync(
        IRepository<UserProfile> userProfileRepository,
        IRepository<ProjectMember> projectMemberRepository,
        IPaginationService paginationService,
        Guid projectId,
        IReadOnlyCollection<Guid> mentionedUserIds,
        IReadOnlyCollection<Guid> legacyMentionUserProfileIds,
        CancellationToken cancellationToken)
    {
        var requestedUserIds = mentionedUserIds.Distinct().ToHashSet();
        var legacyProfileIds = legacyMentionUserProfileIds.Distinct().ToArray();

        if (legacyProfileIds.Length > 0)
        {
            var legacyUserIds = await paginationService.ToListAsync(
                userProfileRepository.Query()
                    .Where(profile => legacyProfileIds.Contains(profile.Id) && profile.IsActive)
                    .Select(profile => profile.UserId),
                cancellationToken);

            if (legacyUserIds.Count != legacyProfileIds.Length)
            {
                throw new ApplicationConflictException("Mentions must reference active users.");
            }

            foreach (var legacyUserId in legacyUserIds)
            {
                requestedUserIds.Add(legacyUserId);
            }
        }

        if (requestedUserIds.Count == 0)
        {
            return Array.Empty<Guid>();
        }

        var validUserIds = await paginationService.ToListAsync(
            projectMemberRepository.Query()
                .Where(member => member.ProjectId == projectId
                    && requestedUserIds.Contains(member.UserId)
                    && member.IsActive
                    && member.UserProfile != null
                    && member.UserProfile.IsActive)
                .Select(member => member.UserId),
            cancellationToken);

        if (validUserIds.Count != requestedUserIds.Count)
        {
            throw new ApplicationConflictException("Mentions must be active members of the current project.");
        }

        return requestedUserIds.ToArray();
    }

    public static string EnsureSupportedReaction(string emoji)
    {
        var normalized = emoji.Trim();
        if (!SupportedReactions.Contains(normalized))
        {
            throw new ApplicationConflictException("Reaction emoji is not supported.");
        }

        return normalized;
    }

    public static bool CanModerateComments(ICurrentUserService currentUserService)
    {
        return currentUserService.IsInRole(ApplicationRoles.ProjectManager)
            || currentUserService.IsInRole(ApplicationRoles.OrganizationAdmin)
            || currentUserService.IsInRole(ApplicationRoles.SuperAdmin)
            || currentUserService.IsInRole(ApplicationRoles.SystemAdministrator);
    }

    public static void EnsureCanEditComment(WorkComment comment, Guid currentUserId)
    {
        EnsureCommentIsActive(comment);
        if (comment.AuthorId != currentUserId)
        {
            throw new ApplicationForbiddenException("You can only edit your own comments.");
        }
    }

    public static void EnsureCanModerateOrOwnComment(WorkComment comment, Guid currentUserId, ICurrentUserService currentUserService, bool allowDeleted = false)
    {
        if (!allowDeleted)
        {
            EnsureCommentIsActive(comment);
        }

        if (comment.AuthorId == currentUserId || CanModerateComments(currentUserService))
        {
            return;
        }

        throw new ApplicationForbiddenException("You can only delete or restore your own comments.");
    }

    public static void EnsureCommentIsActive(WorkComment comment)
    {
        if (comment.IsDeleted)
        {
            throw new ApplicationConflictException("Comment is deleted.");
        }
    }

    public static string GetPreviewType(string contentType, string fileName)
    {
        var normalizedContentType = contentType.Trim().ToLowerInvariant();
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (normalizedContentType.StartsWith("image/", StringComparison.Ordinal))
        {
            return "image";
        }

        if (normalizedContentType == "application/pdf" || extension == ".pdf")
        {
            return "pdf";
        }

        if (normalizedContentType.StartsWith("video/", StringComparison.Ordinal))
        {
            return "video";
        }

        if (extension is ".doc" or ".docx")
        {
            return "word";
        }

        if (extension is ".xls" or ".xlsx" or ".csv")
        {
            return "excel";
        }

        if (extension is ".zip" or ".rar" or ".7z")
        {
            return "archive";
        }

        return "file";
    }

    public static async Task<WorkItemType> GetTypeAsync(IRepository<WorkItemType> repository, Guid typeId, CancellationToken cancellationToken)
    {
        var type = await repository.GetByIdAsync(typeId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Work item type was not found.");
        if (!type.IsActive)
        {
            throw new ApplicationConflictException("Work item type is inactive.");
        }

        return type;
    }

    public static async Task<WorkWorkflowStatus> GetStatusAsync(IRepository<WorkWorkflowStatus> repository, Guid statusId, Guid workflowId, CancellationToken cancellationToken)
    {
        var status = await repository.GetByIdAsync(statusId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Workflow status was not found.");
        if (status.WorkflowId != workflowId)
        {
            throw new ApplicationConflictException("Workflow status does not belong to the selected work item type workflow.");
        }

        if (!status.IsActive)
        {
            throw new ApplicationConflictException("Workflow status is inactive.");
        }

        return status;
    }

    public static async Task<WorkWorkflowStatus> GetInitialStatusAsync(IRepository<WorkWorkflowStatus> repository, Guid workflowId, IPaginationService paginationService, CancellationToken cancellationToken)
    {
        return await paginationService.SingleOrDefaultAsync(
            repository.Query().Where(status => status.WorkflowId == workflowId && status.IsInitial && status.IsActive),
            cancellationToken)
            ?? throw new ApplicationConflictException("Workflow does not have an active initial status.");
    }

    public static async Task<WorkPriority> GetPriorityAsync(IRepository<WorkPriority> repository, Guid priorityId, CancellationToken cancellationToken)
    {
        var priority = await repository.GetByIdAsync(priorityId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Priority was not found.");
        if (!priority.IsActive)
        {
            throw new ApplicationConflictException("Priority is inactive.");
        }

        return priority;
    }

    public static async Task EnsureParentBelongsToProjectAsync(
        IRepository<WorkItem> repository,
        IPaginationService paginationService,
        Guid? parentWorkItemId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        if (!parentWorkItemId.HasValue)
        {
            return;
        }

        var parentProjectId = await paginationService.SingleOrDefaultAsync(
            repository.Query()
                .Where(workItem => workItem.Id == parentWorkItemId.Value)
                .Select(workItem => (Guid?)workItem.ProjectId),
            cancellationToken);

        if (!parentProjectId.HasValue)
        {
            throw new ApplicationNotFoundException("Parent work item was not found.");
        }

        if (parentProjectId.Value != projectId)
        {
            throw new ApplicationConflictException("Parent work item must belong to the selected project.");
        }
    }

    public static async Task EnsureUserBelongsToOrganizationAsync(IRepository<UserProfile> repository, Guid? userProfileId, Guid organizationId, string label, CancellationToken cancellationToken)
    {
        if (!userProfileId.HasValue)
        {
            return;
        }

        var profile = await repository.GetByIdAsync(userProfileId.Value, cancellationToken)
            ?? throw new ApplicationNotFoundException($"{label} user profile was not found.");
        if (profile.OrganizationId != organizationId)
        {
            throw new ApplicationConflictException($"{label} must belong to the project organization.");
        }

        if (!profile.IsActive)
        {
            throw new ApplicationConflictException($"{label} must be active.");
        }
    }

    public static async Task EnsureUsersBelongToOrganizationAsync(
        IRepository<UserProfile> repository,
        IPaginationService paginationService,
        IReadOnlyCollection<Guid> userProfileIds,
        Guid organizationId,
        string label,
        CancellationToken cancellationToken)
    {
        var requestedIds = userProfileIds.Distinct().ToArray();
        if (requestedIds.Length == 0)
        {
            return;
        }

        var matchedCount = await paginationService.ToListAsync(
            repository.Query()
                .Where(profile => requestedIds.Contains(profile.Id) && profile.OrganizationId == organizationId && profile.IsActive)
                .Select(profile => profile.Id),
            cancellationToken);

        if (matchedCount.Count != requestedIds.Length)
        {
            throw new ApplicationConflictException($"{label} must belong to the project organization and be active.");
        }
    }

    public static async Task EnsureLabelsBelongToOrganizationAsync(
        IRepository<WorkLabel> repository,
        IPaginationService paginationService,
        IReadOnlyCollection<Guid> labelIds,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var requestedIds = labelIds.Distinct().ToArray();
        if (requestedIds.Length == 0)
        {
            return;
        }

        var matchedIds = await paginationService.ToListAsync(
            repository.Query()
                .Where(label => requestedIds.Contains(label.Id) && label.OrganizationId == organizationId && label.IsActive)
                .Select(label => label.Id),
            cancellationToken);

        if (matchedIds.Count != requestedIds.Length)
        {
            throw new ApplicationConflictException("Labels must belong to the project organization.");
        }
    }

    public static async Task EnsureComponentsBelongToProjectAsync(
        IRepository<WorkComponent> repository,
        IPaginationService paginationService,
        IReadOnlyCollection<Guid> componentIds,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var requestedIds = componentIds.Distinct().ToArray();
        if (requestedIds.Length == 0)
        {
            return;
        }

        var matchedIds = await paginationService.ToListAsync(
            repository.Query()
                .Where(component => requestedIds.Contains(component.Id) && component.ProjectId == projectId && component.IsActive)
                .Select(component => component.Id),
            cancellationToken);

        if (matchedIds.Count != requestedIds.Length)
        {
            throw new ApplicationConflictException("Components must belong to the selected project.");
        }
    }

    public static void EnsureRowVersion(byte[] currentRowVersion, string? requestRowVersion)
    {
        if (string.IsNullOrWhiteSpace(requestRowVersion))
        {
            throw new ApplicationConflictException("Row version is required for this operation.");
        }

        var current = Convert.ToBase64String(currentRowVersion);
        if (!string.Equals(current, requestRowVersion, StringComparison.Ordinal))
        {
            throw new ApplicationConflictException("Work item has changed. Refresh and try again.");
        }
    }
}

internal static class WorkActivities
{
    public static async Task AddAsync(
        IRepository<WorkActivity> activityRepository,
        Guid projectId,
        Guid workItemId,
        WorkActivityType activityType,
        string description,
        string? fieldName,
        string? oldValue,
        string? newValue,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        await activityRepository.AddAsync(
            new WorkActivity
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                WorkItemId = workItemId,
                UserId = currentUserService.UserId,
                ActivityType = activityType,
                Category = GetCategory(activityType),
                Description = description,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                CreatedBy = currentUserService.UserName,
                CreatedAt = dateTimeProvider.UtcNow
            },
            cancellationToken);
    }

    public static async Task AddProjectAsync(
        IRepository<WorkActivity> activityRepository,
        Guid projectId,
        WorkActivityType activityType,
        string description,
        string? oldValue,
        string? newValue,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        await activityRepository.AddAsync(
            new WorkActivity
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                UserId = currentUserService.UserId,
                ActivityType = activityType,
                Category = GetCategory(activityType),
                Description = description,
                OldValue = oldValue,
                NewValue = newValue,
                CreatedBy = currentUserService.UserName,
                CreatedAt = dateTimeProvider.UtcNow
            },
            cancellationToken);
    }

    public static WorkActivityCategory GetCategory(WorkActivityType activityType)
    {
        return activityType switch
        {
            WorkActivityType.CommentAdded or WorkActivityType.ReplyAdded or WorkActivityType.CommentEdited or WorkActivityType.CommentUpdated or WorkActivityType.CommentDeleted => WorkActivityCategory.Comments,
            WorkActivityType.AssignmentChanged or WorkActivityType.ReporterChanged or WorkActivityType.WatcherAdded or WorkActivityType.WatcherRemoved => WorkActivityCategory.Assignments,
            WorkActivityType.StatusChanged or WorkActivityType.WorkflowTransitioned or WorkActivityType.Resolved or WorkActivityType.Reopened => WorkActivityCategory.StatusChanges,
            WorkActivityType.PriorityChanged => WorkActivityCategory.Priority,
            WorkActivityType.AttachmentAdded or WorkActivityType.AttachmentUpdated or WorkActivityType.AttachmentDeleted => WorkActivityCategory.Attachments,
            WorkActivityType.MentionAdded or WorkActivityType.MentionRemoved => WorkActivityCategory.Mentions,
            WorkActivityType.ReactionAdded or WorkActivityType.ReactionRemoved => WorkActivityCategory.Reactions,
            WorkActivityType.ProjectMembershipChanged => WorkActivityCategory.ProjectMembership,
            _ => WorkActivityCategory.System
        };
    }
}

internal static class WorkNotifications
{
    public static async Task EnqueueForUserProfilesAsync(
        INotificationDispatcher notificationQueue,
        IRepository<UserProfile> userProfileRepository,
        IPaginationService paginationService,
        IReadOnlyCollection<Guid?> userProfileIds,
        Guid projectId,
        Guid? workItemId,
        NotificationType notificationType,
        NotificationPriority priority,
        string title,
        string message,
        string? actionUrl,
        string? icon,
        ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        var userIds = await ResolveUserIdsForProfilesAsync(userProfileRepository, paginationService, userProfileIds, cancellationToken);
        await EnqueueAsync(notificationQueue, userIds, projectId, workItemId, notificationType, priority, title, message, actionUrl, icon, currentUserService, cancellationToken);
    }

    public static async Task EnqueueForWorkItemParticipantsAsync(
        INotificationDispatcher notificationQueue,
        IRepository<WorkItem> workItemRepository,
        IRepository<UserProfile> userProfileRepository,
        IPaginationService paginationService,
        Guid workItemId,
        Guid projectId,
        IReadOnlyCollection<Guid> additionalUserIds,
        NotificationType notificationType,
        NotificationPriority priority,
        string title,
        string message,
        string? actionUrl,
        string? icon,
        ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        var userIds = await ResolveWorkItemParticipantUserIdsAsync(
            workItemRepository,
            userProfileRepository,
            paginationService,
            workItemId,
            additionalUserIds,
            cancellationToken);

        await EnqueueAsync(notificationQueue, userIds, projectId, workItemId, notificationType, priority, title, message, actionUrl, icon, currentUserService, cancellationToken);
    }

    public static async Task<IReadOnlyCollection<Guid>> ResolveWorkItemParticipantUserIdsAsync(
        IRepository<WorkItem> workItemRepository,
        IRepository<UserProfile> userProfileRepository,
        IPaginationService paginationService,
        Guid workItemId,
        IReadOnlyCollection<Guid> additionalUserIds,
        CancellationToken cancellationToken)
    {
        var participantProfileIds = await paginationService.SingleOrDefaultAsync(
            workItemRepository.Query()
                .Where(workItem => workItem.Id == workItemId)
                .Select(workItem => new WorkItemParticipantProfiles(workItem.AssigneeUserProfileId, workItem.ReporterUserProfileId)),
            cancellationToken);

        var userIds = new HashSet<Guid>(additionalUserIds);
        if (participantProfileIds is not null)
        {
            var directUserIds = await ResolveUserIdsForProfilesAsync(
                userProfileRepository,
                paginationService,
                [participantProfileIds.AssigneeUserProfileId, participantProfileIds.ReporterUserProfileId],
                cancellationToken);

            foreach (var userId in directUserIds)
            {
                userIds.Add(userId);
            }
        }

        var watcherUserIds = await paginationService.ToListAsync(
            workItemRepository.Query()
                .Where(workItem => workItem.Id == workItemId)
                .SelectMany(workItem => workItem.Watchers.Select(watcher => watcher.UserProfile.UserId)),
            cancellationToken);

        foreach (var watcherUserId in watcherUserIds)
        {
            userIds.Add(watcherUserId);
        }

        return userIds.ToArray();
    }

    public static async Task<IReadOnlyCollection<Guid>> ResolveUserIdsForProfilesAsync(
        IRepository<UserProfile> userProfileRepository,
        IPaginationService paginationService,
        IReadOnlyCollection<Guid?> userProfileIds,
        CancellationToken cancellationToken)
    {
        var profileIds = userProfileIds
            .Where(profileId => profileId.HasValue)
            .Select(profileId => profileId!.Value)
            .Distinct()
            .ToArray();

        if (profileIds.Length == 0)
        {
            return Array.Empty<Guid>();
        }

        return await paginationService.ToListAsync(
            userProfileRepository.Query()
                .Where(profile => profileIds.Contains(profile.Id) && profile.IsActive)
                .Select(profile => profile.UserId),
            cancellationToken);
    }

    public static async Task EnqueueAsync(
        INotificationDispatcher notificationQueue,
        IReadOnlyCollection<Guid> userIds,
        Guid? projectId,
        Guid? workItemId,
        NotificationType notificationType,
        NotificationPriority priority,
        string title,
        string message,
        string? actionUrl,
        string? icon,
        ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return;
        }

        await notificationQueue.EnqueueAsync(
            new NotificationIntent(
                userIds.Distinct().ToArray(),
                projectId,
                workItemId,
                notificationType,
                priority,
                title,
                message,
                actionUrl,
                icon,
                currentUserService.UserId,
                currentUserService.UserName),
            cancellationToken);
    }

    public static string WorkItemUrl(Guid workItemId) => $"/app/work-items/{workItemId}";

    public static string CommentUrl(Guid workItemId, Guid commentId) => $"/app/work-items/{workItemId}#comment-{commentId}";

    private sealed record WorkItemParticipantProfiles(Guid? AssigneeUserProfileId, Guid ReporterUserProfileId);
}

