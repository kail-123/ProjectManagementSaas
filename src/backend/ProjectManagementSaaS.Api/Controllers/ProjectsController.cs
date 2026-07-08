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
[Route("api/v{version:apiVersion}/projects")]
public sealed class ProjectsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = ApplicationPermissions.ProjectsView)]
    public async Task<ActionResult<PagedResult<ProjectDto>>> List(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? organizationId,
        [FromQuery] Guid? clientId,
        [FromQuery] ProjectStatus? status,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListProjectsQuery(request, organizationId, clientId, status), cancellationToken));
    }

    [HttpGet("~/api/v{version:apiVersion}/my-projects")]
    [Authorize(Policy = ApplicationPermissions.DashboardView)]
    public async Task<ActionResult<PagedResult<ProjectDto>>> ListMyProjects(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? organizationId,
        [FromQuery] Guid? clientId,
        [FromQuery] ProjectStatus? status,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListMyProjectsQuery(request, organizationId, clientId, status), cancellationToken));
    }

    [HttpGet("search")]
    [Authorize(Policy = ApplicationPermissions.ProjectsView)]
    public async Task<ActionResult<PagedResult<SearchResultDto>>> Search(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? organizationId,
        [FromQuery] Guid? clientId,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new SearchProjectsQuery(request, organizationId, clientId), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.ProjectsView)]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetProjectByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = ApplicationPermissions.ProjectsCreate)]
    public async Task<ActionResult<ProjectDto>> Create([FromBody] ProjectUpsertRequest request, CancellationToken cancellationToken)
    {
        var project = await sender.Send(new CreateProjectCommand(request), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = project.Id, version = "1.0" }, project);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.ProjectsUpdate)]
    public async Task<ActionResult<ProjectDto>> Update(Guid id, [FromBody] ProjectUpsertRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new UpdateProjectCommand(id, request), cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.ProjectsDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteProjectCommand(id), cancellationToken);

        return NoContent();
    }

    [HttpPost("bulk-delete")]
    [Authorize(Policy = ApplicationPermissions.ProjectsDelete)]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new BulkDeleteProjectsCommand(request), cancellationToken);

        return NoContent();
    }

    [HttpGet("{id:guid}/members")]
    [HttpGet("~/api/v{version:apiVersion}/project/{id:guid}/members")]
    [Authorize(Policy = ApplicationPermissions.ProjectMembersView)]
    public async Task<ActionResult<PagedResult<ProjectMemberDto>>> ListMembers(
        Guid id,
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListProjectMembersQuery(id, request), cancellationToken));
    }

    [HttpPost("{id:guid}/members")]
    [HttpPost("~/api/v{version:apiVersion}/project/{id:guid}/members")]
    [Authorize(Policy = ApplicationPermissions.ProjectMembersManage)]
    public async Task<ActionResult<IReadOnlyCollection<ProjectMemberDto>>> AddMember(Guid id, [FromBody] ProjectMemberUpsertRequest request, CancellationToken cancellationToken)
    {
        var members = await sender.Send(new AddProjectMemberCommand(id, request), cancellationToken);

        return CreatedAtAction(nameof(ListMembers), new { id, version = "1.0" }, members);
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [HttpDelete("~/api/v{version:apiVersion}/project/{id:guid}/members/{userId:guid}")]
    [Authorize(Policy = ApplicationPermissions.ProjectMembersManage)]
    public async Task<IActionResult> DeleteMember(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteProjectMemberCommand(id, userId), cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/members/bulk-delete")]
    [Authorize(Policy = ApplicationPermissions.ProjectMembersManage)]
    public async Task<IActionResult> BulkDeleteMembers(Guid id, [FromBody] BulkDeleteRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new BulkDeleteProjectMembersCommand(id, request), cancellationToken);

        return NoContent();
    }

    [HttpGet("{id:guid}/settings")]
    [Authorize(Policy = ApplicationPermissions.ProjectSettingsView)]
    public async Task<ActionResult<ProjectSettingsDto>> GetSettings(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetProjectSettingsQuery(id), cancellationToken));
    }

    [HttpPut("{id:guid}/settings")]
    [Authorize(Policy = ApplicationPermissions.ProjectSettingsManage)]
    public async Task<ActionResult<ProjectSettingsDto>> UpdateSettings(
        Guid id,
        [FromBody] ProjectSettingsRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new UpdateProjectSettingsCommand(id, request), cancellationToken));
    }

    [HttpGet("{id:guid}/dashboard")]
    [Authorize(Policy = ApplicationPermissions.ProjectsDashboardView)]
    public async Task<ActionResult<ProjectDashboardDto>> GetDashboard(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetProjectDashboardQuery(id), cancellationToken));
    }
}
