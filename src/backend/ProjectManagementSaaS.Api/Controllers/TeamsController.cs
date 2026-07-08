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
[Route("api/v{version:apiVersion}/teams")]
public sealed class TeamsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = ApplicationPermissions.TeamsView)]
    public async Task<ActionResult<PagedResult<TeamDto>>> List(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? departmentId,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListTeamsQuery(request, departmentId), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.TeamsView)]
    public async Task<ActionResult<TeamDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetTeamByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = ApplicationPermissions.TeamsCreate)]
    public async Task<ActionResult<TeamDto>> Create([FromBody] TeamUpsertRequest request, CancellationToken cancellationToken)
    {
        var team = await sender.Send(new CreateTeamCommand(request), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = team.Id, version = "1.0" }, team);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.TeamsUpdate)]
    public async Task<ActionResult<TeamDto>> Update(Guid id, [FromBody] TeamUpsertRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new UpdateTeamCommand(id, request), cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.TeamsDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteTeamCommand(id), cancellationToken);

        return NoContent();
    }

    [HttpPost("bulk-delete")]
    [Authorize(Policy = ApplicationPermissions.TeamsDelete)]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new BulkDeleteTeamsCommand(request), cancellationToken);

        return NoContent();
    }
}
