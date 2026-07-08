using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using ProjectManagementSaaS.Application.Abstractions.Persistence;
using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Features.Organizations.Contracts;
using ProjectManagementSaaS.Domain.Organizations;

namespace ProjectManagementSaaS.Application.Features.Organizations;

public sealed record ListDepartmentsQuery(PagedRequest Request, Guid? OrganizationId) : IRequest<PagedResult<DepartmentDto>>;

public sealed record GetDepartmentByIdQuery(Guid Id) : IRequest<DepartmentDto>;

public sealed record CreateDepartmentCommand(DepartmentUpsertRequest Request) : IRequest<DepartmentDto>;

public sealed record UpdateDepartmentCommand(Guid Id, DepartmentUpsertRequest Request) : IRequest<DepartmentDto>;

public sealed record DeleteDepartmentCommand(Guid Id) : IRequest;

public sealed record BulkDeleteDepartmentsCommand(BulkDeleteRequest Request) : IRequest;

internal sealed class ListDepartmentsQueryHandler(
    IRepository<Department> repository,
    IPaginationService paginationService,
    IMapper mapper)
    : IRequestHandler<ListDepartmentsQuery, PagedResult<DepartmentDto>>
{
    public Task<PagedResult<DepartmentDto>> Handle(ListDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var query = repository.Query();

        if (request.OrganizationId.HasValue)
        {
            query = query.Where(department => department.OrganizationId == request.OrganizationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
        {
            var term = request.Request.Search.Trim();
            query = query.Where(department =>
                department.Name.Contains(term)
                || department.Code.Contains(term)
                || department.Organization.Name.Contains(term));
        }

        var descending = request.Request.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
        query = (request.Request.SortBy?.ToLowerInvariant(), descending) switch
        {
            ("code", true) => query.OrderByDescending(department => department.Code),
            ("code", false) => query.OrderBy(department => department.Code),
            ("organization", true) => query.OrderByDescending(department => department.Organization.Name),
            ("organization", false) => query.OrderBy(department => department.Organization.Name),
            ("createdon", true) => query.OrderByDescending(department => department.CreatedOn),
            ("createdon", false) => query.OrderBy(department => department.CreatedOn),
            ("name", true) => query.OrderByDescending(department => department.Name),
            _ => query.OrderBy(department => department.Name)
        };

        return paginationService.CreateAsync(
            query.ProjectTo<DepartmentDto>(mapper.ConfigurationProvider),
            request.Request.PageNumber,
            request.Request.PageSize,
            cancellationToken);
    }
}

internal sealed class GetDepartmentByIdQueryHandler(
    IRepository<Department> repository,
    IPaginationService paginationService,
    IMapper mapper)
    : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto>
{
    public async Task<DepartmentDto> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var department = await paginationService.SingleOrDefaultAsync(
            repository.Query()
                .Where(entity => entity.Id == request.Id)
                .ProjectTo<DepartmentDto>(mapper.ConfigurationProvider),
            cancellationToken);

        return department ?? throw new ApplicationNotFoundException("Department was not found.");
    }
}

internal sealed class CreateDepartmentCommandHandler(
    IRepository<Organization> organizationRepository,
    IRepository<Department> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<CreateDepartmentCommand, DepartmentDto>
{
    public async Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(request.Request.OrganizationId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Organization was not found.");

        var department = mapper.Map<Department>(request.Request);
        department.Id = Guid.NewGuid();
        department.Organization = organization;

        await repository.AddAsync(department, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<DepartmentDto>(department);
    }
}

internal sealed class UpdateDepartmentCommandHandler(
    IRepository<Organization> organizationRepository,
    IRepository<Department> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<UpdateDepartmentCommand, DepartmentDto>
{
    public async Task<DepartmentDto> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(request.Request.OrganizationId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Organization was not found.");

        var department = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ApplicationNotFoundException("Department was not found.");

        mapper.Map(request.Request, department);
        department.Organization = organization;
        repository.Update(department);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<DepartmentDto>(department);
    }
}

internal sealed class DeleteDepartmentCommandHandler(IRepository<Department> repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteDepartmentCommand>
{
    public async Task Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ApplicationNotFoundException("Department was not found.");

        repository.Remove(department);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class BulkDeleteDepartmentsCommandHandler(
    IRepository<Department> repository,
    IPaginationService paginationService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<BulkDeleteDepartmentsCommand>
{
    public async Task Handle(BulkDeleteDepartmentsCommand request, CancellationToken cancellationToken)
    {
        var departments = await paginationService.ToListAsync(
            repository.Query().Where(department => request.Request.Ids.Contains(department.Id)),
            cancellationToken);

        repository.RemoveRange(departments);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
