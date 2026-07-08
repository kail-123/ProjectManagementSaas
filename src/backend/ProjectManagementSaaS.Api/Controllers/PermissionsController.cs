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
[Route("api/v{version:apiVersion}/permissions")]
public sealed class PermissionsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = ApplicationPermissions.PermissionsView)]
    public async Task<ActionResult<PagedResult<PermissionDto>>> List(
        [FromQuery] PagedRequest request,
        [FromQuery] string? module,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListPermissionsQuery(request, module), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.PermissionsView)]
    public async Task<ActionResult<PermissionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetPermissionByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = ApplicationPermissions.PermissionsCreate)]
    public async Task<ActionResult<PermissionDto>> Create([FromBody] PermissionUpsertRequest request, CancellationToken cancellationToken)
    {
        var permission = await sender.Send(new CreatePermissionCommand(request), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = permission.Id, version = "1.0" }, permission);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.PermissionsUpdate)]
    public async Task<ActionResult<PermissionDto>> Update(Guid id, [FromBody] PermissionUpsertRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new UpdatePermissionCommand(id, request), cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.PermissionsDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeletePermissionCommand(id), cancellationToken);

        return NoContent();
    }

    [HttpPost("bulk-delete")]
    [Authorize(Policy = ApplicationPermissions.PermissionsDelete)]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new BulkDeletePermissionsCommand(request), cancellationToken);

        return NoContent();
    }
}
