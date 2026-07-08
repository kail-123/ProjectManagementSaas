using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using ProjectManagementSaaS.Application.Abstractions.Persistence;
using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Features.Security.Contracts;
using ProjectManagementSaaS.Domain.Security;

namespace ProjectManagementSaaS.Application.Features.Security;

public sealed record ListPermissionsQuery(PagedRequest Request, string? Module) : IRequest<PagedResult<PermissionDto>>;

public sealed record GetPermissionByIdQuery(Guid Id) : IRequest<PermissionDto>;

public sealed record CreatePermissionCommand(PermissionUpsertRequest Request) : IRequest<PermissionDto>;

public sealed record UpdatePermissionCommand(Guid Id, PermissionUpsertRequest Request) : IRequest<PermissionDto>;

public sealed record DeletePermissionCommand(Guid Id) : IRequest;

public sealed record BulkDeletePermissionsCommand(BulkDeleteRequest Request) : IRequest;

internal sealed class ListPermissionsQueryHandler(
    IRepository<Permission> repository,
    IPaginationService paginationService,
    IMapper mapper)
    : IRequestHandler<ListPermissionsQuery, PagedResult<PermissionDto>>
{
    public Task<PagedResult<PermissionDto>> Handle(ListPermissionsQuery request, CancellationToken cancellationToken)
    {
        var query = repository.Query();

        if (!string.IsNullOrWhiteSpace(request.Module))
        {
            query = query.Where(permission => permission.Module == request.Module);
        }

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
        {
            var term = request.Request.Search.Trim();
            query = query.Where(permission =>
                permission.Module.Contains(term)
                || permission.Name.Contains(term)
                || permission.Code.Contains(term));
        }

        var descending = request.Request.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
        query = (request.Request.SortBy?.ToLowerInvariant(), descending) switch
        {
            ("module", true) => query.OrderByDescending(permission => permission.Module),
            ("module", false) => query.OrderBy(permission => permission.Module),
            ("code", true) => query.OrderByDescending(permission => permission.Code),
            ("code", false) => query.OrderBy(permission => permission.Code),
            ("createdon", true) => query.OrderByDescending(permission => permission.CreatedOn),
            ("createdon", false) => query.OrderBy(permission => permission.CreatedOn),
            ("name", true) => query.OrderByDescending(permission => permission.Name),
            _ => query.OrderBy(permission => permission.Module).ThenBy(permission => permission.Name)
        };

        return paginationService.CreateAsync(
            query.ProjectTo<PermissionDto>(mapper.ConfigurationProvider),
            request.Request.PageNumber,
            request.Request.PageSize,
            cancellationToken);
    }
}

internal sealed class GetPermissionByIdQueryHandler(
    IRepository<Permission> repository,
    IPaginationService paginationService,
    IMapper mapper)
    : IRequestHandler<GetPermissionByIdQuery, PermissionDto>
{
    public async Task<PermissionDto> Handle(GetPermissionByIdQuery request, CancellationToken cancellationToken)
    {
        var permission = await paginationService.SingleOrDefaultAsync(
            repository.Query()
                .Where(entity => entity.Id == request.Id)
                .ProjectTo<PermissionDto>(mapper.ConfigurationProvider),
            cancellationToken);

        return permission ?? throw new ApplicationNotFoundException("Permission was not found.");
    }
}

internal sealed class CreatePermissionCommandHandler(
    IRepository<Permission> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<CreatePermissionCommand, PermissionDto>
{
    public async Task<PermissionDto> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = mapper.Map<Permission>(request.Request);
        permission.Id = Guid.NewGuid();

        await repository.AddAsync(permission, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<PermissionDto>(permission);
    }
}

internal sealed class UpdatePermissionCommandHandler(
    IRepository<Permission> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<UpdatePermissionCommand, PermissionDto>
{
    public async Task<PermissionDto> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ApplicationNotFoundException("Permission was not found.");

        mapper.Map(request.Request, permission);
        repository.Update(permission);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<PermissionDto>(permission);
    }
}

internal sealed class DeletePermissionCommandHandler(IRepository<Permission> repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePermissionCommand>
{
    public async Task Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ApplicationNotFoundException("Permission was not found.");

        repository.Remove(permission);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class BulkDeletePermissionsCommandHandler(
    IRepository<Permission> repository,
    IPaginationService paginationService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<BulkDeletePermissionsCommand>
{
    public async Task Handle(BulkDeletePermissionsCommand request, CancellationToken cancellationToken)
    {
        var permissions = await paginationService.ToListAsync(
            repository.Query().Where(permission => request.Request.Ids.Contains(permission.Id)),
            cancellationToken);

        repository.RemoveRange(permissions);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
