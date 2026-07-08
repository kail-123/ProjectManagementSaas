using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using ProjectManagementSaaS.Application.Abstractions.Persistence;
using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Features.Organizations.Contracts;
using ProjectManagementSaaS.Domain.Organizations;

namespace ProjectManagementSaaS.Application.Features.Organizations;

public sealed record ListOrganizationsQuery(PagedRequest Request) : IRequest<PagedResult<OrganizationDto>>;

public sealed record GetOrganizationByIdQuery(Guid Id) : IRequest<OrganizationDto>;

public sealed record CreateOrganizationCommand(OrganizationUpsertRequest Request) : IRequest<OrganizationDto>;

public sealed record UpdateOrganizationCommand(Guid Id, OrganizationUpsertRequest Request) : IRequest<OrganizationDto>;

public sealed record DeleteOrganizationCommand(Guid Id) : IRequest;

public sealed record BulkDeleteOrganizationsCommand(BulkDeleteRequest Request) : IRequest;

internal sealed class ListOrganizationsQueryHandler(
    IRepository<Organization> repository,
    IPaginationService paginationService,
    IMapper mapper)
    : IRequestHandler<ListOrganizationsQuery, PagedResult<OrganizationDto>>
{
    public Task<PagedResult<OrganizationDto>> Handle(ListOrganizationsQuery request, CancellationToken cancellationToken)
    {
        var query = repository.Query();
        query = ApplySearch(query, request.Request.Search);
        query = ApplySort(query, request.Request.SortBy, request.Request.SortDirection);

        return paginationService.CreateAsync(
            query.ProjectTo<OrganizationDto>(mapper.ConfigurationProvider),
            request.Request.PageNumber,
            request.Request.PageSize,
            cancellationToken);
    }

    private static IQueryable<Organization> ApplySearch(IQueryable<Organization> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var term = search.Trim();
        return query.Where(organization =>
            organization.Name.Contains(term)
            || organization.Code.Contains(term)
            || organization.Email.Contains(term)
            || organization.Country.Contains(term));
    }

    private static IQueryable<Organization> ApplySort(IQueryable<Organization> query, string? sortBy, string? direction)
    {
        var descending = direction?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;

        return (sortBy?.ToLowerInvariant(), descending) switch
        {
            ("code", true) => query.OrderByDescending(organization => organization.Code),
            ("code", false) => query.OrderBy(organization => organization.Code),
            ("email", true) => query.OrderByDescending(organization => organization.Email),
            ("email", false) => query.OrderBy(organization => organization.Email),
            ("country", true) => query.OrderByDescending(organization => organization.Country),
            ("country", false) => query.OrderBy(organization => organization.Country),
            ("createdon", true) => query.OrderByDescending(organization => organization.CreatedOn),
            ("createdon", false) => query.OrderBy(organization => organization.CreatedOn),
            ("name", true) => query.OrderByDescending(organization => organization.Name),
            _ => query.OrderBy(organization => organization.Name)
        };
    }
}

internal sealed class GetOrganizationByIdQueryHandler(
    IRepository<Organization> repository,
    IPaginationService paginationService,
    IMapper mapper)
    : IRequestHandler<GetOrganizationByIdQuery, OrganizationDto>
{
    public async Task<OrganizationDto> Handle(GetOrganizationByIdQuery request, CancellationToken cancellationToken)
    {
        var organization = await paginationService.SingleOrDefaultAsync(
            repository.Query()
                .Where(entity => entity.Id == request.Id)
                .ProjectTo<OrganizationDto>(mapper.ConfigurationProvider),
            cancellationToken);

        return organization ?? throw new ApplicationNotFoundException("Organization was not found.");
    }
}

internal sealed class CreateOrganizationCommandHandler(
    IRepository<Organization> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<CreateOrganizationCommand, OrganizationDto>
{
    public async Task<OrganizationDto> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var organization = mapper.Map<Organization>(request.Request);
        organization.Id = Guid.NewGuid();

        await repository.AddAsync(organization, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<OrganizationDto>(organization);
    }
}

internal sealed class UpdateOrganizationCommandHandler(
    IRepository<Organization> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<UpdateOrganizationCommand, OrganizationDto>
{
    public async Task<OrganizationDto> Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var organization = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ApplicationNotFoundException("Organization was not found.");

        mapper.Map(request.Request, organization);
        repository.Update(organization);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<OrganizationDto>(organization);
    }
}

internal sealed class DeleteOrganizationCommandHandler(
    IRepository<Organization> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteOrganizationCommand>
{
    public async Task Handle(DeleteOrganizationCommand request, CancellationToken cancellationToken)
    {
        var organization = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ApplicationNotFoundException("Organization was not found.");

        repository.Remove(organization);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class BulkDeleteOrganizationsCommandHandler(
    IRepository<Organization> repository,
    IPaginationService paginationService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<BulkDeleteOrganizationsCommand>
{
    public async Task Handle(BulkDeleteOrganizationsCommand request, CancellationToken cancellationToken)
    {
        var organizations = await paginationService.ToListAsync(
            repository.Query().Where(organization => request.Request.Ids.Contains(organization.Id)),
            cancellationToken);

        repository.RemoveRange(organizations);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
