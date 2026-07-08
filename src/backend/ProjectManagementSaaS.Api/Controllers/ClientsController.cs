using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Common.Security;
using ProjectManagementSaaS.Application.Features.Projects;
using ProjectManagementSaaS.Application.Features.Projects.Contracts;
using ProjectManagementSaaS.Domain.Projects;

namespace ProjectManagementSaaS.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/clients")]
public sealed class ClientsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = ApplicationPermissions.ClientsView)]
    public async Task<ActionResult<PagedResult<ClientDto>>> List(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? organizationId,
        [FromQuery] ClientStatus? status,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListClientsQuery(request, organizationId, status), cancellationToken));
    }

    [HttpGet("search")]
    [Authorize(Policy = ApplicationPermissions.ClientsView)]
    public async Task<ActionResult<PagedResult<SearchResultDto>>> Search(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? organizationId,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new SearchClientsQuery(request, organizationId), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.ClientsView)]
    public async Task<ActionResult<ClientDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetClientByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = ApplicationPermissions.ClientsCreate)]
    public async Task<ActionResult<ClientDto>> Create([FromBody] ClientUpsertRequest request, CancellationToken cancellationToken)
    {
        var client = await sender.Send(new CreateClientCommand(request), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = client.Id, version = "1.0" }, client);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.ClientsUpdate)]
    public async Task<ActionResult<ClientDto>> Update(Guid id, [FromBody] ClientUpsertRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new UpdateClientCommand(id, request), cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.ClientsDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteClientCommand(id), cancellationToken);

        return NoContent();
    }

    [HttpPost("bulk-delete")]
    [Authorize(Policy = ApplicationPermissions.ClientsDelete)]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new BulkDeleteClientsCommand(request), cancellationToken);

        return NoContent();
    }
}
