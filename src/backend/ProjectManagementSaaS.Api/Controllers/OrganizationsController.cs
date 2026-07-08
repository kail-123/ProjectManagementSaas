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
[Route("api/v{version:apiVersion}/organizations")]
public sealed class OrganizationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = ApplicationPermissions.OrganizationsView)]
    public async Task<ActionResult<PagedResult<OrganizationDto>>> List([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListOrganizationsQuery(request), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.OrganizationsView)]
    public async Task<ActionResult<OrganizationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetOrganizationByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = ApplicationPermissions.OrganizationsCreate)]
    public async Task<ActionResult<OrganizationDto>> Create([FromBody] OrganizationUpsertRequest request, CancellationToken cancellationToken)
    {
        var organization = await sender.Send(new CreateOrganizationCommand(request), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = organization.Id, version = "1.0" }, organization);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.OrganizationsUpdate)]
    public async Task<ActionResult<OrganizationDto>> Update(Guid id, [FromBody] OrganizationUpsertRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new UpdateOrganizationCommand(id, request), cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.OrganizationsDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteOrganizationCommand(id), cancellationToken);

        return NoContent();
    }

    [HttpPost("bulk-delete")]
    [Authorize(Policy = ApplicationPermissions.OrganizationsDelete)]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new BulkDeleteOrganizationsCommand(request), cancellationToken);

        return NoContent();
    }
}
