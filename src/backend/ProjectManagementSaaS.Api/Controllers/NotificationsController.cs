using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Common.Security;
using ProjectManagementSaaS.Application.Features.Notifications;
using ProjectManagementSaaS.Application.Features.Notifications.Contracts;
using ProjectManagementSaaS.Domain.Notifications;

namespace ProjectManagementSaaS.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Policy = ApplicationPermissions.NotificationsView)]
[Route("api/v{version:apiVersion}/notifications")]
public sealed class NotificationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<NotificationDto>>> List(
        [FromQuery] PagedRequest request,
        [FromQuery] NotificationType? type,
        [FromQuery] NotificationPriority? priority,
        [FromQuery] bool? isRead,
        [FromQuery] Guid? projectId,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListNotificationsQuery(request, type, priority, isRead, projectId), cancellationToken));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> UnreadCount(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetUnreadNotificationCountQuery(), cancellationToken));
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new MarkNotificationReadCommand(id), cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/unread")]
    public async Task<IActionResult> MarkUnread(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new MarkNotificationUnreadCommand(id), cancellationToken);

        return NoContent();
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        await sender.Send(new MarkAllNotificationsReadCommand(), cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteNotificationCommand(id), cancellationToken);

        return NoContent();
    }

    [HttpDelete("read")]
    public async Task<IActionResult> ClearRead(CancellationToken cancellationToken)
    {
        await sender.Send(new ClearReadNotificationsCommand(), cancellationToken);

        return NoContent();
    }

    [HttpGet("preferences")]
    public async Task<ActionResult<NotificationPreferencesDto>> Preferences(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetNotificationPreferencesQuery(), cancellationToken));
    }

    [HttpPut("preferences")]
    public async Task<ActionResult<NotificationPreferencesDto>> UpdatePreferences(
        [FromBody] NotificationPreferencesRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new UpdateNotificationPreferencesCommand(request), cancellationToken));
    }
}
