using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Common.Security;
using ProjectManagementSaaS.Application.Features.Security;
using ProjectManagementSaaS.Application.Features.Security.Contracts;

namespace ProjectManagementSaaS.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/roles")]
public sealed class RolesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = ApplicationPermissions.RolesView)]
    public async Task<ActionResult<PagedResult<RoleDto>>> List([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListRolesQuery(request), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.RolesView)]
    public async Task<ActionResult<RoleDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetRoleByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = ApplicationPermissions.RolesCreate)]
    public async Task<ActionResult<RoleDto>> Create([FromBody] RoleUpsertRequest request, CancellationToken cancellationToken)
    {
        var role = await sender.Send(new CreateRoleCommand(request), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = role.Id, version = "1.0" }, role);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.RolesUpdate)]
    public async Task<ActionResult<RoleDto>> Update(Guid id, [FromBody] RoleUpsertRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new UpdateRoleCommand(id, request), cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.RolesDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteRoleCommand(id), cancellationToken);

        return NoContent();
    }

    [HttpPost("bulk-delete")]
    [Authorize(Policy = ApplicationPermissions.RolesDelete)]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new BulkDeleteRolesCommand(request), cancellationToken);

        return NoContent();
    }

    [HttpPut("{id:guid}/permissions")]
    [Authorize(Policy = ApplicationPermissions.RolesAssignPermissions)]
    public async Task<IActionResult> AssignPermissions(Guid id, [FromBody] AssignPermissionsRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new AssignPermissionsToRoleCommand(id, request), cancellationToken);

        return NoContent();
    }
}
