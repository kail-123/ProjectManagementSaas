using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Common.Security;
using ProjectManagementSaaS.Application.Features.Organizations;
using ProjectManagementSaaS.Application.Features.Organizations.Contracts;

namespace ProjectManagementSaaS.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/departments")]
public sealed class DepartmentsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = ApplicationPermissions.DepartmentsView)]
    public async Task<ActionResult<PagedResult<DepartmentDto>>> List(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? organizationId,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListDepartmentsQuery(request, organizationId), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.DepartmentsView)]
    public async Task<ActionResult<DepartmentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetDepartmentByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = ApplicationPermissions.DepartmentsCreate)]
    public async Task<ActionResult<DepartmentDto>> Create([FromBody] DepartmentUpsertRequest request, CancellationToken cancellationToken)
    {
        var department = await sender.Send(new CreateDepartmentCommand(request), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = department.Id, version = "1.0" }, department);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.DepartmentsUpdate)]
    public async Task<ActionResult<DepartmentDto>> Update(Guid id, [FromBody] DepartmentUpsertRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new UpdateDepartmentCommand(id, request), cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.DepartmentsDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteDepartmentCommand(id), cancellationToken);

        return NoContent();
    }

    [HttpPost("bulk-delete")]
    [Authorize(Policy = ApplicationPermissions.DepartmentsDelete)]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new BulkDeleteDepartmentsCommand(request), cancellationToken);

        return NoContent();
    }
}
