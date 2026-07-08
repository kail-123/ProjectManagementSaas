using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Common.Security;
using ProjectManagementSaaS.Application.Features.Work;
using ProjectManagementSaaS.Application.Features.Work.Contracts;
using ProjectManagementSaaS.Domain.Work;

namespace ProjectManagementSaaS.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/work")]
public sealed class WorkController(ISender sender) : ControllerBase
{
    [HttpGet("metadata")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsView)]
    public async Task<ActionResult<WorkMetadataDto>> Metadata(
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? organizationId,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetWorkMetadataQuery(projectId, organizationId), cancellationToken));
    }

    [HttpGet("items")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsView)]
    public async Task<ActionResult<PagedResult<WorkItemDto>>> ListItems(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? workItemTypeId,
        [FromQuery] Guid? workflowStatusId,
        [FromQuery] Guid? priorityId,
        [FromQuery] Guid? assigneeUserProfileId,
        [FromQuery] Guid? reporterUserProfileId,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(
            new ListWorkItemsQuery(request, projectId, workItemTypeId, workflowStatusId, priorityId, assigneeUserProfileId, reporterUserProfileId),
            cancellationToken));
    }

    [HttpGet("items/{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsView)]
    public async Task<ActionResult<WorkItemDto>> GetItem(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetWorkItemByIdQuery(id), cancellationToken));
    }

    [HttpPost("items")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsCreate)]
    public async Task<ActionResult<WorkItemDto>> CreateItem([FromBody] WorkItemUpsertRequest request, CancellationToken cancellationToken)
    {
        var workItem = await sender.Send(new CreateWorkItemCommand(request), cancellationToken);

        return CreatedAtAction(nameof(GetItem), new { id = workItem.Id, version = "1.0" }, workItem);
    }

    [HttpPut("items/{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsUpdate)]
    public async Task<ActionResult<WorkItemDto>> UpdateItem(Guid id, [FromBody] WorkItemUpsertRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new UpdateWorkItemCommand(id, request), cancellationToken));
    }

    [HttpPost("items/{id:guid}/transition")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsTransition)]
    public async Task<ActionResult<WorkItemDto>> TransitionItem(Guid id, [FromBody] WorkTransitionRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new TransitionWorkItemCommand(id, request), cancellationToken));
    }

    [HttpDelete("items/{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsDelete)]
    public async Task<IActionResult> DeleteItem(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteWorkItemCommand(id), cancellationToken);

        return NoContent();
    }

    [HttpPost("items/bulk-delete")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsDelete)]
    public async Task<IActionResult> BulkDeleteItems([FromBody] BulkDeleteRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new BulkDeleteWorkItemsCommand(request), cancellationToken);

        return NoContent();
    }

    [HttpGet("items/{id:guid}/comments")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsView)]
    public async Task<ActionResult<PagedResult<WorkCommentDto>>> ListComments(Guid id, [FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListWorkItemCommentsQuery(id, request), cancellationToken));
    }

    [HttpGet("items/{id:guid}/mention-candidates")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsView)]
    public async Task<ActionResult<PagedResult<WorkMentionCandidateDto>>> ListMentionCandidates(Guid id, [FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListWorkItemMentionCandidatesQuery(id, request), cancellationToken));
    }

    [HttpPost("items/{id:guid}/comments/mark-all-read")]
    [Authorize(Policy = ApplicationPermissions.WorkCommentsManage)]
    public async Task<IActionResult> MarkCommentsRead(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new MarkWorkItemCommentsReadCommand(id), cancellationToken);

        return NoContent();
    }

    [HttpPost("items/{id:guid}/comments")]
    [Authorize(Policy = ApplicationPermissions.WorkCommentsManage)]
    public async Task<ActionResult<WorkCommentDto>> AddComment(Guid id, [FromBody] WorkCommentUpsertRequest request, CancellationToken cancellationToken)
    {
        var comment = await sender.Send(new AddWorkItemCommentCommand(id, request), cancellationToken);

        return CreatedAtAction(nameof(ListComments), new { id, version = "1.0" }, comment);
    }

    [HttpPut("items/{id:guid}/comments/{commentId:guid}")]
    [Authorize(Policy = ApplicationPermissions.WorkCommentsManage)]
    public async Task<ActionResult<WorkCommentDto>> UpdateComment(Guid id, Guid commentId, [FromBody] WorkCommentUpsertRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new UpdateWorkItemCommentCommand(id, commentId, request), cancellationToken));
    }

    [HttpDelete("items/{id:guid}/comments/{commentId:guid}")]
    [Authorize(Policy = ApplicationPermissions.WorkCommentsManage)]
    public async Task<IActionResult> DeleteComment(Guid id, Guid commentId, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteWorkItemCommentCommand(id, commentId), cancellationToken);

        return NoContent();
    }

    [HttpPost("items/{id:guid}/comments/{commentId:guid}/restore")]
    [Authorize(Policy = ApplicationPermissions.WorkCommentsManage)]
    public async Task<ActionResult<WorkCommentDto>> RestoreComment(Guid id, Guid commentId, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new RestoreWorkItemCommentCommand(id, commentId), cancellationToken));
    }

    [HttpPost("items/{id:guid}/comments/{commentId:guid}/pin")]
    [Authorize(Policy = ApplicationPermissions.WorkCommentsManage)]
    public async Task<ActionResult<WorkCommentDto>> PinComment(Guid id, Guid commentId, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new PinWorkItemCommentCommand(id, commentId, true), cancellationToken));
    }

    [HttpPost("items/{id:guid}/comments/{commentId:guid}/unpin")]
    [Authorize(Policy = ApplicationPermissions.WorkCommentsManage)]
    public async Task<ActionResult<WorkCommentDto>> UnpinComment(Guid id, Guid commentId, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new PinWorkItemCommentCommand(id, commentId, false), cancellationToken));
    }

    [HttpPost("items/{id:guid}/comments/{commentId:guid}/resolve")]
    [Authorize(Policy = ApplicationPermissions.WorkCommentsManage)]
    public async Task<ActionResult<WorkCommentDto>> ResolveComment(Guid id, Guid commentId, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ResolveWorkItemCommentCommand(id, commentId, true), cancellationToken));
    }

    [HttpPost("items/{id:guid}/comments/{commentId:guid}/unresolve")]
    [Authorize(Policy = ApplicationPermissions.WorkCommentsManage)]
    public async Task<ActionResult<WorkCommentDto>> UnresolveComment(Guid id, Guid commentId, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ResolveWorkItemCommentCommand(id, commentId, false), cancellationToken));
    }

    [HttpGet("items/{id:guid}/comments/{commentId:guid}/history")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsView)]
    public async Task<ActionResult<PagedResult<WorkCommentHistoryDto>>> ListCommentHistory(Guid id, Guid commentId, [FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListWorkItemCommentHistoryQuery(id, commentId, request), cancellationToken));
    }

    [HttpGet("items/{id:guid}/comments/{commentId:guid}/attachments")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsView)]
    public async Task<ActionResult<PagedResult<WorkCommentAttachmentDto>>> ListCommentAttachments(Guid id, Guid commentId, [FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListWorkItemCommentAttachmentsQuery(id, commentId, request), cancellationToken));
    }

    [HttpPost("items/{id:guid}/comments/{commentId:guid}/attachments")]
    [Authorize(Policy = ApplicationPermissions.WorkCommentsManage)]
    public async Task<ActionResult<WorkCommentAttachmentDto>> AddCommentAttachment(Guid id, Guid commentId, [FromBody] WorkCommentAttachmentRequest request, CancellationToken cancellationToken)
    {
        var attachment = await sender.Send(new AddWorkItemCommentAttachmentCommand(id, commentId, request), cancellationToken);

        return CreatedAtAction(nameof(ListCommentAttachments), new { id, commentId, version = "1.0" }, attachment);
    }

    [HttpDelete("items/{id:guid}/comments/{commentId:guid}/attachments/{attachmentId:guid}")]
    [Authorize(Policy = ApplicationPermissions.WorkCommentsManage)]
    public async Task<IActionResult> DeleteCommentAttachment(Guid id, Guid commentId, Guid attachmentId, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteWorkItemCommentAttachmentCommand(id, commentId, attachmentId), cancellationToken);

        return NoContent();
    }

    [HttpGet("items/{id:guid}/comments/{commentId:guid}/mentions")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsView)]
    public async Task<ActionResult<PagedResult<WorkCommentMentionDto>>> ListCommentMentions(Guid id, Guid commentId, [FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListWorkItemCommentMentionsQuery(id, commentId, request), cancellationToken));
    }

    [HttpPost("items/{id:guid}/comments/{commentId:guid}/mentions")]
    [Authorize(Policy = ApplicationPermissions.WorkCommentsManage)]
    public async Task<ActionResult<IReadOnlyCollection<WorkCommentMentionDto>>> AddCommentMentions(Guid id, Guid commentId, [FromBody] WorkCommentMentionsRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new AddWorkItemCommentMentionsCommand(id, commentId, request), cancellationToken));
    }

    [HttpDelete("items/{id:guid}/comments/{commentId:guid}/mentions/{mentionId:guid}")]
    [Authorize(Policy = ApplicationPermissions.WorkCommentsManage)]
    public async Task<IActionResult> DeleteCommentMention(Guid id, Guid commentId, Guid mentionId, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteWorkItemCommentMentionCommand(id, commentId, mentionId), cancellationToken);

        return NoContent();
    }

    [HttpGet("items/{id:guid}/comments/{commentId:guid}/reactions")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsView)]
    public async Task<ActionResult<PagedResult<WorkCommentReactionDto>>> ListCommentReactions(Guid id, Guid commentId, [FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListWorkItemCommentReactionsQuery(id, commentId, request), cancellationToken));
    }

    [HttpPost("items/{id:guid}/comments/{commentId:guid}/reactions")]
    [Authorize(Policy = ApplicationPermissions.WorkCommentsManage)]
    public async Task<ActionResult<IReadOnlyCollection<WorkCommentReactionDto>>> ToggleCommentReaction(Guid id, Guid commentId, [FromBody] WorkCommentReactionRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ToggleWorkItemCommentReactionCommand(id, commentId, request), cancellationToken));
    }

    [HttpDelete("items/{id:guid}/comments/{commentId:guid}/reactions/{emoji}")]
    [Authorize(Policy = ApplicationPermissions.WorkCommentsManage)]
    public async Task<IActionResult> DeleteCommentReaction(Guid id, Guid commentId, string emoji, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteWorkItemCommentReactionCommand(id, commentId, Uri.UnescapeDataString(emoji)), cancellationToken);

        return NoContent();
    }

    [HttpGet("items/{id:guid}/comments/{commentId:guid}/reads")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsView)]
    public async Task<ActionResult<PagedResult<WorkCommentReadDto>>> ListCommentReads(Guid id, Guid commentId, [FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListWorkItemCommentReadsQuery(id, commentId, request), cancellationToken));
    }

    [HttpPost("items/{id:guid}/comments/{commentId:guid}/read")]
    [Authorize(Policy = ApplicationPermissions.WorkCommentsManage)]
    public async Task<ActionResult<WorkCommentReadDto>> MarkCommentRead(Guid id, Guid commentId, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new MarkWorkItemCommentReadCommand(id, commentId), cancellationToken));
    }

    [HttpGet("items/{id:guid}/attachments")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsView)]
    public async Task<ActionResult<PagedResult<WorkAttachmentDto>>> ListAttachments(Guid id, [FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListWorkItemAttachmentsQuery(id, request), cancellationToken));
    }

    [HttpPost("items/{id:guid}/attachments")]
    [Authorize(Policy = ApplicationPermissions.WorkAttachmentsManage)]
    public async Task<ActionResult<WorkAttachmentDto>> AddAttachment(Guid id, [FromBody] WorkAttachmentRequest request, CancellationToken cancellationToken)
    {
        var attachment = await sender.Send(new AddWorkItemAttachmentCommand(id, request), cancellationToken);

        return CreatedAtAction(nameof(ListAttachments), new { id, version = "1.0" }, attachment);
    }

    [HttpDelete("items/{id:guid}/attachments/{attachmentId:guid}")]
    [Authorize(Policy = ApplicationPermissions.WorkAttachmentsManage)]
    public async Task<IActionResult> DeleteAttachment(Guid id, Guid attachmentId, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteWorkItemAttachmentCommand(id, attachmentId), cancellationToken);

        return NoContent();
    }

    [HttpGet("items/{id:guid}/activity")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsView)]
    public async Task<ActionResult<PagedResult<WorkActivityDto>>> ListActivity(
        Guid id,
        [FromQuery] PagedRequest request,
        [FromQuery] WorkActivityCategory? category,
        [FromQuery] WorkActivityType? activityType,
        [FromQuery] DateTimeOffset? dateFrom,
        [FromQuery] DateTimeOffset? dateTo,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListWorkItemActivityQuery(id, request, category, activityType, dateFrom, dateTo), cancellationToken));
    }

    [HttpGet("items/{id:guid}/links")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsView)]
    public async Task<ActionResult<PagedResult<WorkItemLinkDto>>> ListLinks(Guid id, [FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListWorkItemLinksQuery(id, request), cancellationToken));
    }

    [HttpPost("items/{id:guid}/links")]
    [Authorize(Policy = ApplicationPermissions.WorkLinksManage)]
    public async Task<ActionResult<WorkItemLinkDto>> AddLink(Guid id, [FromBody] WorkItemLinkRequest request, CancellationToken cancellationToken)
    {
        var link = await sender.Send(new AddWorkItemLinkCommand(id, request), cancellationToken);

        return CreatedAtAction(nameof(ListLinks), new { id, version = "1.0" }, link);
    }

    [HttpDelete("items/{id:guid}/links/{linkId:guid}")]
    [Authorize(Policy = ApplicationPermissions.WorkLinksManage)]
    public async Task<IActionResult> DeleteLink(Guid id, Guid linkId, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteWorkItemLinkCommand(id, linkId), cancellationToken);

        return NoContent();
    }

    [HttpGet("saved-filters")]
    [Authorize(Policy = ApplicationPermissions.WorkItemsView)]
    public async Task<ActionResult<PagedResult<WorkSavedFilterDto>>> ListSavedFilters(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? projectId,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ListWorkSavedFiltersQuery(projectId, request), cancellationToken));
    }

    [HttpPost("saved-filters")]
    [Authorize(Policy = ApplicationPermissions.WorkFiltersManage)]
    public async Task<ActionResult<WorkSavedFilterDto>> CreateSavedFilter([FromBody] WorkSavedFilterRequest request, CancellationToken cancellationToken)
    {
        var filter = await sender.Send(new CreateWorkSavedFilterCommand(request), cancellationToken);

        return CreatedAtAction(nameof(ListSavedFilters), new { version = "1.0" }, filter);
    }

    [HttpDelete("saved-filters/{id:guid}")]
    [Authorize(Policy = ApplicationPermissions.WorkFiltersManage)]
    public async Task<IActionResult> DeleteSavedFilter(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteWorkSavedFilterCommand(id), cancellationToken);

        return NoContent();
    }
}
