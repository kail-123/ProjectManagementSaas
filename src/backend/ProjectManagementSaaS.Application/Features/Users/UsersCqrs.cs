using MediatR;
using ProjectManagementSaaS.Application.Abstractions.Identity;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Features.Users.Contracts;

namespace ProjectManagementSaaS.Application.Features.Users;

public sealed record ListUsersQuery(PagedRequest Request) : IRequest<PagedResult<UserDto>>;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;

public sealed record CreateUserCommand(CreateUserRequest Request) : IRequest<UserDto>;

public sealed record UpdateUserCommand(Guid Id, UpdateUserRequest Request) : IRequest<UserDto>;

public sealed record SetUserActiveStatusCommand(Guid Id, SetUserActiveStatusRequest Request) : IRequest;

public sealed record DeleteUserCommand(Guid Id) : IRequest;

public sealed record BulkDeleteUsersCommand(BulkDeleteRequest Request) : IRequest;

public sealed record ResetUserPasswordCommand(Guid Id, ResetPasswordRequest Request) : IRequest;

public sealed record AssignRolesToUserCommand(Guid Id, AssignRolesRequest Request) : IRequest;

internal sealed class ListUsersQueryHandler(IUserManagementService userManagementService)
    : IRequestHandler<ListUsersQuery, PagedResult<UserDto>>
{
    public Task<PagedResult<UserDto>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        return userManagementService.ListAsync(request.Request, cancellationToken);
    }
}

internal sealed class GetUserByIdQueryHandler(IUserManagementService userManagementService)
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return userManagementService.GetByIdAsync(request.Id, cancellationToken);
    }
}

internal sealed class CreateUserCommandHandler(IUserManagementService userManagementService)
    : IRequestHandler<CreateUserCommand, UserDto>
{
    public Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        return userManagementService.CreateAsync(request.Request, cancellationToken);
    }
}

internal sealed class UpdateUserCommandHandler(IUserManagementService userManagementService)
    : IRequestHandler<UpdateUserCommand, UserDto>
{
    public Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        return userManagementService.UpdateAsync(request.Id, request.Request, cancellationToken);
    }
}

internal sealed class SetUserActiveStatusCommandHandler(IUserManagementService userManagementService)
    : IRequestHandler<SetUserActiveStatusCommand>
{
    public async Task Handle(SetUserActiveStatusCommand request, CancellationToken cancellationToken)
    {
        await userManagementService.SetActiveStatusAsync(request.Id, request.Request.IsActive, cancellationToken);
    }
}

internal sealed class DeleteUserCommandHandler(IUserManagementService userManagementService)
    : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        await userManagementService.DeleteAsync(request.Id, cancellationToken);
    }
}

internal sealed class BulkDeleteUsersCommandHandler(IUserManagementService userManagementService)
    : IRequestHandler<BulkDeleteUsersCommand>
{
    public async Task Handle(BulkDeleteUsersCommand request, CancellationToken cancellationToken)
    {
        await userManagementService.BulkDeleteAsync(request.Request.Ids, cancellationToken);
    }
}

internal sealed class ResetUserPasswordCommandHandler(IUserManagementService userManagementService)
    : IRequestHandler<ResetUserPasswordCommand>
{
    public async Task Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        await userManagementService.ResetPasswordAsync(request.Id, request.Request, cancellationToken);
    }
}

internal sealed class AssignRolesToUserCommandHandler(IUserManagementService userManagementService)
    : IRequestHandler<AssignRolesToUserCommand>
{
    public async Task Handle(AssignRolesToUserCommand request, CancellationToken cancellationToken)
    {
        await userManagementService.AssignRolesAsync(request.Id, request.Request.Roles, cancellationToken);
    }
}
