using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Common.Security;
using ProjectManagementSaaS.Application.Features.Users;
using ProjectManagementSaaS.Application.Features.Users.Contracts;

namespace ProjectManagementSaaS.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/users")]
public sealed class UsersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = ApplicationPermissions.UsersView)]
    public async Task<ActionResult<PagedResult<UserDto>>> List([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListUsersQuery(request), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.UsersView)]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetUserByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = ApplicationPermissions.UsersCreate)]
    [Authorize(Policy = ApplicationPermissions.UsersAssignRoles)]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await sender.Send(new CreateUserCommand(request), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = user.Id, version = "1.0" }, user);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.UsersUpdate)]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new UpdateUserCommand(id, request), cancellationToken));
    }

    [HttpPatch("{id:guid}/active-status")]
    [Authorize(Policy = ApplicationPermissions.UsersManageStatus)]
    public async Task<IActionResult> SetActiveStatus(Guid id, [FromBody] SetUserActiveStatusRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new SetUserActiveStatusCommand(id, request), cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.UsersDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteUserCommand(id), cancellationToken);

        return NoContent();
    }

    [HttpPost("bulk-delete")]
    [Authorize(Policy = ApplicationPermissions.UsersDelete)]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new BulkDeleteUsersCommand(request), cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/reset-password")]
    [Authorize(Policy = ApplicationPermissions.UsersResetPassword)]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new ResetUserPasswordCommand(id, request), cancellationToken);

        return NoContent();
    }

    [HttpPut("{id:guid}/roles")]
    [Authorize(Policy = ApplicationPermissions.UsersAssignRoles)]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] AssignRolesRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new AssignRolesToUserCommand(id, request), cancellationToken);

        return NoContent();
    }
}
