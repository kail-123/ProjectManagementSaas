using MediatR;
using ProjectManagementSaaS.Application.Abstractions.Persistence;
using ProjectManagementSaaS.Application.Abstractions.Projects;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Features.Projects.Contracts;
using ProjectManagementSaaS.Domain.Projects;
using ProjectManagementSaaS.Domain.Work;

namespace ProjectManagementSaaS.Application.Features.Projects;

public sealed record GlobalSearchQuery(PagedRequest Request) : IRequest<PagedResult<SearchResultDto>>;

public sealed record SearchClientsQuery(PagedRequest Request, Guid? OrganizationId) : IRequest<PagedResult<SearchResultDto>>;

public sealed record SearchProjectsQuery(PagedRequest Request, Guid? OrganizationId, Guid? ClientId) : IRequest<PagedResult<SearchResultDto>>;

internal sealed class GlobalSearchQueryHandler(
    IRepository<Client> clientRepository,
    IRepository<Project> projectRepository,
    IRepository<WorkItem> workItemRepository,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<GlobalSearchQuery, PagedResult<SearchResultDto>>
{
    public Task<PagedResult<SearchResultDto>> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
    {
        var clients = SearchQueryBuilder.ApplyClientSearch(clientRepository.Query(), request.Request.Search)
            .Select(client => new
            {
                client.Id,
                Type = SearchResultTypes.Client,
                Title = client.Name,
                Subtitle = client.Code + " - " + client.Organization.Name,
                Url = SearchRoutes.Clients
            });

        var projects = SearchQueryBuilder.ApplyProjectSearch(projectAccessService.ApplyProjectFilter(projectRepository.Query()), request.Request.Search)
            .Select(project => new
            {
                project.Id,
                Type = SearchResultTypes.Project,
                Title = project.Name,
                Subtitle = project.Code + " - " + project.Organization.Name,
                Url = SearchRoutes.Projects
            });

        var workItemQuery = workItemRepository.Query();
        if (!projectAccessService.CanAccessAllProjects)
        {
            var userId = projectAccessService.CurrentUserId;
            workItemQuery = workItemQuery.Where(workItem => workItem.Project.Members.Any(member => member.UserId == userId && member.IsActive));
        }

        var workItems = SearchQueryBuilder.ApplyWorkItemSearch(workItemQuery, request.Request.Search)
            .Select(workItem => new
            {
                workItem.Id,
                Type = SearchResultTypes.WorkItem,
                Title = workItem.Title,
                Subtitle = workItem.Type.Name + " - " + workItem.Project.Name,
                Url = SearchRoutes.WorkItems
            });

        var descending = request.Request.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
        var query = (request.Request.SortBy?.ToLowerInvariant(), descending) switch
        {
            ("type", true) => clients.Concat(projects).Concat(workItems).OrderByDescending(result => result.Type).ThenByDescending(result => result.Title),
            ("type", false) => clients.Concat(projects).Concat(workItems).OrderBy(result => result.Type).ThenBy(result => result.Title),
            ("title", true) => clients.Concat(projects).Concat(workItems).OrderByDescending(result => result.Title),
            _ => clients.Concat(projects).Concat(workItems).OrderBy(result => result.Title)
        };

        return paginationService.CreateAsync(
            query.Select(result => new SearchResultDto(result.Id, result.Type, result.Title, result.Subtitle, result.Url)),
            request.Request.PageNumber,
            request.Request.PageSize,
            cancellationToken);
    }
}

internal sealed class SearchClientsQueryHandler(
    IRepository<Client> repository,
    IPaginationService paginationService)
    : IRequestHandler<SearchClientsQuery, PagedResult<SearchResultDto>>
{
    public Task<PagedResult<SearchResultDto>> Handle(SearchClientsQuery request, CancellationToken cancellationToken)
    {
        var query = repository.Query();
        if (request.OrganizationId.HasValue)
        {
            query = query.Where(client => client.OrganizationId == request.OrganizationId.Value);
        }

        var results = SearchQueryBuilder.ApplyClientSearch(query, request.Request.Search)
            .Select(client => new SearchResultDto(
                client.Id,
                SearchResultTypes.Client,
                client.Name,
                client.Code + " - " + client.Organization.Name,
                SearchRoutes.Clients));

        results = SearchQueryBuilder.ApplySearchSort(results, request.Request.SortBy, request.Request.SortDirection);

        return paginationService.CreateAsync(results, request.Request.PageNumber, request.Request.PageSize, cancellationToken);
    }
}

internal sealed class SearchProjectsQueryHandler(
    IRepository<Project> repository,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService)
    : IRequestHandler<SearchProjectsQuery, PagedResult<SearchResultDto>>
{
    public Task<PagedResult<SearchResultDto>> Handle(SearchProjectsQuery request, CancellationToken cancellationToken)
    {
        var query = projectAccessService.ApplyProjectFilter(repository.Query());
        if (request.OrganizationId.HasValue)
        {
            query = query.Where(project => project.OrganizationId == request.OrganizationId.Value);
        }

        if (request.ClientId.HasValue)
        {
            query = query.Where(project => project.ClientId == request.ClientId.Value);
        }

        var results = SearchQueryBuilder.ApplyProjectSearch(query, request.Request.Search)
            .Select(project => new SearchResultDto(
                project.Id,
                SearchResultTypes.Project,
                project.Name,
                project.Code + " - " + project.Organization.Name,
                SearchRoutes.Projects));

        results = SearchQueryBuilder.ApplySearchSort(results, request.Request.SortBy, request.Request.SortDirection);

        return paginationService.CreateAsync(results, request.Request.PageNumber, request.Request.PageSize, cancellationToken);
    }
}

internal static class SearchQueryBuilder
{
    public static IQueryable<Client> ApplyClientSearch(IQueryable<Client> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var term = search.Trim();
        return query.Where(client =>
            client.Name.Contains(term)
            || client.Code.Contains(term)
            || client.Email.Contains(term)
            || (client.ContactPerson != null && client.ContactPerson.Contains(term))
            || client.Organization.Name.Contains(term));
    }

    public static IQueryable<Project> ApplyProjectSearch(IQueryable<Project> query, string? search)
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
            || (project.Client != null && project.Client.Name.Contains(term)));
    }

    public static IQueryable<SearchResultDto> ApplySearchSort(IQueryable<SearchResultDto> query, string? sortBy, string? direction)
    {
        var descending = direction?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;

        return (sortBy?.ToLowerInvariant(), descending) switch
        {
            ("type", true) => query.OrderByDescending(result => result.Type).ThenByDescending(result => result.Title),
            ("type", false) => query.OrderBy(result => result.Type).ThenBy(result => result.Title),
            ("title", true) => query.OrderByDescending(result => result.Title),
            _ => query.OrderBy(result => result.Title)
        };
    }

    public static IQueryable<WorkItem> ApplyWorkItemSearch(IQueryable<WorkItem> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var term = search.Trim();
        return query.Where(workItem =>
            workItem.Title.Contains(term)
            || (workItem.Description != null && workItem.Description.Contains(term))
            || workItem.Project.Name.Contains(term)
            || workItem.Type.Name.Contains(term)
            || workItem.Status.Name.Contains(term));
    }
}

internal static class SearchResultTypes
{
    public const string Client = "Client";
    public const string Project = "Project";
    public const string WorkItem = "WorkItem";
}

internal static class SearchRoutes
{
    public const string Clients = "/app/clients";
    public const string Projects = "/app/projects";
    public const string WorkItems = "/app/work-items";
}
