using MediatR;
using ProjectManagementSaaS.Application.Abstractions.Identity;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Features.Security.Contracts;

namespace ProjectManagementSaaS.Application.Features.Security;

public sealed record ListRolesQuery(PagedRequest Request) : IRequest<PagedResult<RoleDto>>;

public sealed record GetRoleByIdQuery(Guid Id) : IRequest<RoleDto>;

public sealed record CreateRoleCommand(RoleUpsertRequest Request) : IRequest<RoleDto>;

public sealed record UpdateRoleCommand(Guid Id, RoleUpsertRequest Request) : IRequest<RoleDto>;

public sealed record DeleteRoleCommand(Guid Id) : IRequest;

public sealed record BulkDeleteRolesCommand(BulkDeleteRequest Request) : IRequest;

public sealed record AssignPermissionsToRoleCommand(Guid Id, AssignPermissionsRequest Request) : IRequest;

internal sealed class ListRolesQueryHandler(IRoleManagementService roleManagementService)
    : IRequestHandler<ListRolesQuery, PagedResult<RoleDto>>
{
    public Task<PagedResult<RoleDto>> Handle(ListRolesQuery request, CancellationToken cancellationToken)
    {
        return roleManagementService.ListAsync(request.Request, cancellationToken);
    }
}

internal sealed class GetRoleByIdQueryHandler(IRoleManagementService roleManagementService)
    : IRequestHandler<GetRoleByIdQuery, RoleDto>
{
    public Task<RoleDto> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        return roleManagementService.GetByIdAsync(request.Id, cancellationToken);
    }
}

internal sealed class CreateRoleCommandHandler(IRoleManagementService roleManagementService)
    : IRequestHandler<CreateRoleCommand, RoleDto>
{
    public Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        return roleManagementService.CreateAsync(request.Request, cancellationToken);
    }
}

internal sealed class UpdateRoleCommandHandler(IRoleManagementService roleManagementService)
    : IRequestHandler<UpdateRoleCommand, RoleDto>
{
    public Task<RoleDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        return roleManagementService.UpdateAsync(request.Id, request.Request, cancellationToken);
    }
}

internal sealed class DeleteRoleCommandHandler(IRoleManagementService roleManagementService)
    : IRequestHandler<DeleteRoleCommand>
{
    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        await roleManagementService.DeleteAsync(request.Id, cancellationToken);
    }
}

internal sealed class BulkDeleteRolesCommandHandler(IRoleManagementService roleManagementService)
    : IRequestHandler<BulkDeleteRolesCommand>
{
    public async Task Handle(BulkDeleteRolesCommand request, CancellationToken cancellationToken)
    {
        await roleManagementService.BulkDeleteAsync(request.Request.Ids, cancellationToken);
    }
}

internal sealed class AssignPermissionsToRoleCommandHandler(IRoleManagementService roleManagementService)
    : IRequestHandler<AssignPermissionsToRoleCommand>
{
    public async Task Handle(AssignPermissionsToRoleCommand request, CancellationToken cancellationToken)
    {
        await roleManagementService.AssignPermissionsAsync(request.Id, request.Request.PermissionIds, cancellationToken);
    }
}
