# Phase 5.1 Part 1 - Enterprise Threaded Comment Engine

## Storage Model

- `work.Comments` is the threaded comment table. Replies use `ParentCommentId`, allowing unlimited nesting through the same entity.
- `AuthorId` links new comments to `organization.UserProfiles.UserId`. It is nullable so legacy comments without a typed author can remain readable.
- `Message` stores the authored markdown/plain-text content. The migration renames the previous `BodyMarkdown` column to preserve existing comment text.
- `Pinned`, `Resolved`, `IsEdited`, `EditedAt`, `IsDeleted`, and `DeletedOn` drive collaboration state without hard-deleting thread context.
- `work.CommentHistory` stores every edit. The migration renames old history columns to `PreviousMessage`, `EditedAt`, and `EditedByName`.
- `work.CommentAttachments` stores multiple attachments per comment with file metadata, preview type, audit fields, and soft-delete support.

## API Surface

- `GET /api/v1/work/items/{id}/comments`
- `POST /api/v1/work/items/{id}/comments`
- `PUT /api/v1/work/items/{id}/comments/{commentId}`
- `DELETE /api/v1/work/items/{id}/comments/{commentId}`
- `POST /api/v1/work/items/{id}/comments/{commentId}/restore`
- `POST /api/v1/work/items/{id}/comments/{commentId}/pin`
- `POST /api/v1/work/items/{id}/comments/{commentId}/unpin`
- `POST /api/v1/work/items/{id}/comments/{commentId}/resolve`
- `POST /api/v1/work/items/{id}/comments/{commentId}/unresolve`
- `GET /api/v1/work/items/{id}/comments/{commentId}/history`
- `GET /api/v1/work/items/{id}/comments/{commentId}/attachments`
- `POST /api/v1/work/items/{id}/comments/{commentId}/attachments`
- `DELETE /api/v1/work/items/{id}/comments/{commentId}/attachments/{attachmentId}`

All endpoints first validate Project Membership through the work item/project access service. Direct URL/API access is rejected when the user is not a project member, unless the user is a System Administrator.

## Security Rules

- Any comment read/write starts with work-item access validation, which reuses Project Membership filtering.
- Authors can edit their own active comments and add/remove attachments on their own comments.
- Authors and comment moderators can soft-delete or restore comments.
- Project Manager, Organization Admin, Super Admin, and System Administrator roles can moderate comments and pin/unpin discussions.
- Any user with comment-manage permission and access to the work item can reply or resolve/reopen a discussion.
- Deleted comments stay in the thread as placeholders so replies remain visible.
- EF audit logging records add/update/delete/restore/pin/resolve/attachment actions through the existing audit pipeline.

## Frontend Behavior

- Work-item details now renders dynamic threaded comments from the permission-filtered API.
- The composer supports markdown controls, reply context, mentions, emoji insertion, paste/drop attachments, and multiple pending files.
- Reply previews scroll to and highlight the original comment.
- Resolved discussions auto-collapse and can be expanded.
- Unauthorized actions are hidden client-side and still enforced server-side.

# Phase 5.1 Part 2 - Mentions, Reactions, and Read Receipts

## Storage Model

- `work.CommentMentions` stores one row per mentioned project member. `MentionedUserId` points to `organization.UserProfiles.UserId`, while `MentionedByUserId` records who created the mention when that user has an active profile.
- `work.CommentReactions` stores one emoji reaction per comment/user/emoji combination. The unique index on `CommentId`, `UserId`, and `Emoji` makes reaction toggles idempotent.
- `work.CommentReads` stores one read receipt per comment/user. Marking a comment as read updates `ReadAt` instead of creating duplicates.
- Mention, reaction, and read tables inherit the same active-user and active-work-item query filters as comments.

## API Surface

- `GET /api/v1/work/items/{id}/mention-candidates`
- `POST /api/v1/work/items/{id}/comments/mark-all-read`
- `GET /api/v1/work/items/{id}/comments/{commentId}/mentions`
- `POST /api/v1/work/items/{id}/comments/{commentId}/mentions`
- `DELETE /api/v1/work/items/{id}/comments/{commentId}/mentions/{mentionId}`
- `GET /api/v1/work/items/{id}/comments/{commentId}/reactions`
- `POST /api/v1/work/items/{id}/comments/{commentId}/reactions`
- `DELETE /api/v1/work/items/{id}/comments/{commentId}/reactions/{emoji}`
- `GET /api/v1/work/items/{id}/comments/{commentId}/reads`
- `POST /api/v1/work/items/{id}/comments/{commentId}/read`

Comment list responses include mentioned users, reaction summaries, read receipt previews, counters, and `ReadByCurrentUser` so the UI does not need extra calls for the common thread view.

## Security Rules

- Every endpoint resolves the work item first and applies Project Membership access checks. Non-members cannot read candidates, mentions, reactions, read receipts, or mutate comment collaboration state.
- Mention candidates come only from active `ProjectMembers`; the system does not use `CreatedBy` as a project visibility shortcut.
- Mention writes reject users who are not active members of the work item's project.
- Reactions are limited to the supported emoji set enforced by the application layer.
- Read receipts can only be written for the current authenticated user.
- Users without comment-management permission do not see write actions in the React thread, and the backend still enforces authorization.

## Frontend Behavior

- The comment editor provides project-member `@` autocomplete backed by `GET /mention-candidates`.
- Selected mentions are stored as user IDs and sent through `MentionedUserIds`; legacy profile mention IDs remain accepted for backward compatibility.
- Threaded comments render mention chips, reaction counts, current-user reaction state, unread highlighting, read counts, and a compact profile drawer.
- Reacting, replying, editing, deleting, restoring, pinning, resolving, and marking comments read remain hidden unless the user has the relevant permission.

# Phase 5.1 Part 3 - Enterprise Activity Timeline

## Storage Model

- `work.ActivityLogs` is the immutable activity timeline table. It replaces the old `work.ActivityTimeline` name through a preserving migration.
- Each row stores `ProjectId`, optional `WorkItemId`, optional `UserId`, `ActivityType`, `Category`, `OldValue`, `NewValue`, `Description`, and `CreatedAt`.
- `ProjectId` is required so project-level events, such as membership changes, can be audited without tying them to a single work item.
- Legacy activity rows are backfilled from `work.WorkItems.ProjectId` during migration and categorized from their activity type.
- Activity rows are append-only in application code. No update or delete command is exposed.

## API Surface

- `GET /api/v1/work/items/{id}/activity`
- Query filters: `pageNumber`, `pageSize`, `search`, `category`, `activityType`, `dateFrom`, and `dateTo`.
- Responses include actor display data (`ActorName`, `ActorRole`, `ActorAvatar`) plus old/new values for field-level changes.

## Automatically Tracked Events

- Work item create/update/delete, title, description, status, priority, assignee, reporter, due date, estimated hours, story points, labels, watchers, workflow transitions, links, attachments, comments, replies, edits, deletes, pins, resolves, mentions, reactions, and project membership changes.

## Security Rules

- Timeline reads resolve the work item first and reuse Project Membership access checks.
- Users cannot view timeline rows for work items in projects where they are not members, unless they are System Administrators.
- Project membership changes are recorded as project-level activity entries with `WorkItemId = null`.

## Frontend Behavior

- Work-item details uses a fixed desktop workspace: details, attachments, links, and activity scroll in the left column while comments have their own independent scroll container.
- The comment editor stays visible at the bottom of the comments panel.
- Top-level comments render newest first; replies remain chronological inside their parent thread.
- Long or resolved threads auto-collapse, comments group by date, and consecutive messages by the same author compact their avatar/header.
- Activity renders as a grouped vertical timeline with category filters, search, load more, relative time, and CSV/PDF export.
