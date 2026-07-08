using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using ProjectManagementSaaS.Application.Abstractions.Persistence;
using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Features.Projects.Contracts;
using ProjectManagementSaaS.Domain.Organizations;
using ProjectManagementSaaS.Domain.Projects;

namespace ProjectManagementSaaS.Application.Features.Projects;

public sealed record ListClientsQuery(PagedRequest Request, Guid? OrganizationId, ClientStatus? Status) : IRequest<PagedResult<ClientDto>>;

public sealed record GetClientByIdQuery(Guid Id) : IRequest<ClientDto>;

public sealed record CreateClientCommand(ClientUpsertRequest Request) : IRequest<ClientDto>;

public sealed record UpdateClientCommand(Guid Id, ClientUpsertRequest Request) : IRequest<ClientDto>;

public sealed record DeleteClientCommand(Guid Id) : IRequest;

public sealed record BulkDeleteClientsCommand(BulkDeleteRequest Request) : IRequest;

internal sealed class ListClientsQueryHandler(
    IRepository<Client> repository,
    IPaginationService paginationService,
    IMapper mapper)
    : IRequestHandler<ListClientsQuery, PagedResult<ClientDto>>
{
    public Task<PagedResult<ClientDto>> Handle(ListClientsQuery request, CancellationToken cancellationToken)
    {
        var query = repository.Query();

        if (request.OrganizationId.HasValue)
        {
            query = query.Where(client => client.OrganizationId == request.OrganizationId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(client => client.Status == request.Status.Value);
        }

        query = ApplySearch(query, request.Request.Search);
        query = ApplySort(query, request.Request.SortBy, request.Request.SortDirection);

        return paginationService.CreateAsync(
            query.ProjectTo<ClientDto>(mapper.ConfigurationProvider),
            request.Request.PageNumber,
            request.Request.PageSize,
            cancellationToken);
    }

    private static IQueryable<Client> ApplySearch(IQueryable<Client> query, string? search)
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
            || (client.Mobile != null && client.Mobile.Contains(term))
            || client.Organization.Name.Contains(term));
    }

    private static IQueryable<Client> ApplySort(IQueryable<Client> query, string? sortBy, string? direction)
    {
        var descending = direction?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;

        return (sortBy?.ToLowerInvariant(), descending) switch
        {
            ("code", true) => query.OrderByDescending(client => client.Code),
            ("code", false) => query.OrderBy(client => client.Code),
            ("organizationname", true) => query.OrderByDescending(client => client.Organization.Name),
            ("organizationname", false) => query.OrderBy(client => client.Organization.Name),
            ("email", true) => query.OrderByDescending(client => client.Email),
            ("email", false) => query.OrderBy(client => client.Email),
            ("status", true) => query.OrderByDescending(client => client.Status),
            ("status", false) => query.OrderBy(client => client.Status),
            ("createdon", true) => query.OrderByDescending(client => client.CreatedOn),
            ("createdon", false) => query.OrderBy(client => client.CreatedOn),
            ("name", true) => query.OrderByDescending(client => client.Name),
            _ => query.OrderBy(client => client.Name)
        };
    }
}

internal sealed class GetClientByIdQueryHandler(
    IRepository<Client> repository,
    IPaginationService paginationService,
    IMapper mapper)
    : IRequestHandler<GetClientByIdQuery, ClientDto>
{
    public async Task<ClientDto> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var client = await paginationService.SingleOrDefaultAsync(
            repository.Query()
                .Where(entity => entity.Id == request.Id)
                .ProjectTo<ClientDto>(mapper.ConfigurationProvider),
            cancellationToken);

        return client ?? throw new ApplicationNotFoundException("Client was not found.");
    }
}

internal sealed class CreateClientCommandHandler(
    IRepository<Organization> organizationRepository,
    IRepository<Client> repository,
    IPaginationService paginationService,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<CreateClientCommand, ClientDto>
{
    public async Task<ClientDto> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(request.Request.OrganizationId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Organization was not found.");

        await EnsureUniqueCodeAsync(repository, paginationService, request.Request.OrganizationId, request.Request.Code, null, cancellationToken);

        var client = mapper.Map<Client>(request.Request);
        client.Id = Guid.NewGuid();
        client.Organization = organization;

        await repository.AddAsync(client, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<ClientDto>(client);
    }

    private static async Task EnsureUniqueCodeAsync(
        IRepository<Client> repository,
        IPaginationService paginationService,
        Guid organizationId,
        string code,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        var existingId = await paginationService.SingleOrDefaultAsync(
            repository.Query()
                .Where(client => client.OrganizationId == organizationId
                    && client.Code == code
                    && (!excludingId.HasValue || client.Id != excludingId.Value))
                .Select(client => (Guid?)client.Id),
            cancellationToken);

        if (existingId.HasValue)
        {
            throw new ApplicationConflictException("Client code already exists for the selected organization.");
        }
    }
}

internal sealed class UpdateClientCommandHandler(
    IRepository<Organization> organizationRepository,
    IRepository<Client> repository,
    IRepository<Project> projectRepository,
    IPaginationService paginationService,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<UpdateClientCommand, ClientDto>
{
    public async Task<ClientDto> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        var client = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ApplicationNotFoundException("Client was not found.");

        var organization = await organizationRepository.GetByIdAsync(request.Request.OrganizationId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Organization was not found.");

        if (client.OrganizationId != request.Request.OrganizationId)
        {
            var attachedProjectId = await paginationService.SingleOrDefaultAsync(
                projectRepository.Query()
                    .Where(project => project.ClientId == client.Id)
                    .Select(project => (Guid?)project.Id),
                cancellationToken);

            if (attachedProjectId.HasValue)
            {
                throw new ApplicationConflictException("Client organization cannot be changed while projects are attached.");
            }
        }

        await EnsureUniqueCodeAsync(repository, paginationService, request.Request.OrganizationId, request.Request.Code, client.Id, cancellationToken);

        mapper.Map(request.Request, client);
        client.Organization = organization;
        repository.Update(client);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<ClientDto>(client);
    }

    private static async Task EnsureUniqueCodeAsync(
        IRepository<Client> repository,
        IPaginationService paginationService,
        Guid organizationId,
        string code,
        Guid excludingId,
        CancellationToken cancellationToken)
    {
        var existingId = await paginationService.SingleOrDefaultAsync(
            repository.Query()
                .Where(client => client.OrganizationId == organizationId
                    && client.Code == code
                    && client.Id != excludingId)
                .Select(client => (Guid?)client.Id),
            cancellationToken);

        if (existingId.HasValue)
        {
            throw new ApplicationConflictException("Client code already exists for the selected organization.");
        }
    }
}

internal sealed class DeleteClientCommandHandler(
    IRepository<Client> repository,
    IRepository<Project> projectRepository,
    IPaginationService paginationService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteClientCommand>
{
    public async Task Handle(DeleteClientCommand request, CancellationToken cancellationToken)
    {
        var client = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ApplicationNotFoundException("Client was not found.");

        var attachedProjectId = await paginationService.SingleOrDefaultAsync(
            projectRepository.Query()
                .Where(project => project.ClientId == client.Id)
                .Select(project => (Guid?)project.Id),
            cancellationToken);

        if (attachedProjectId.HasValue)
        {
            throw new ApplicationConflictException("Client cannot be deleted while projects are attached.");
        }

        repository.Remove(client);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class BulkDeleteClientsCommandHandler(
    IRepository<Client> repository,
    IRepository<Project> projectRepository,
    IPaginationService paginationService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<BulkDeleteClientsCommand>
{
    public async Task Handle(BulkDeleteClientsCommand request, CancellationToken cancellationToken)
    {
        var attachedClientId = await paginationService.SingleOrDefaultAsync(
            projectRepository.Query()
                .Where(project => project.ClientId.HasValue && request.Request.Ids.Contains(project.ClientId.Value))
                .Select(project => project.ClientId),
            cancellationToken);

        if (attachedClientId.HasValue)
        {
            throw new ApplicationConflictException("One or more clients cannot be deleted while projects are attached.");
        }

        var clients = await paginationService.ToListAsync(
            repository.Query().Where(client => request.Request.Ids.Contains(client.Id)),
            cancellationToken);

        repository.RemoveRange(clients);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
