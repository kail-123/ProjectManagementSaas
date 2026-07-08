using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Common.Security;
using ProjectManagementSaaS.Application.Features.Projects;
using ProjectManagementSaaS.Application.Features.Projects.Contracts;

namespace ProjectManagementSaaS.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/search")]
public sealed class SearchController(ISender sender) : ControllerBase
{
    [HttpGet("global")]
    [Authorize(Policy = ApplicationPermissions.SearchGlobal)]
    public async Task<ActionResult<PagedResult<SearchResultDto>>> Global([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GlobalSearchQuery(request), cancellationToken));
    }
}
