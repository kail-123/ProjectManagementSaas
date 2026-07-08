# Phase 5.1 Part 4 - Enterprise Real-Time Notification Platform

## Database

- `notifications.Notifications` stores user-scoped in-app notifications with optional `ProjectId` and `WorkItemId` context, read state, action URL, priority, type, and expiration.
- `notifications.NotificationPreferences` stores per-user toggles for assignment, comment, mention, reply, project, email, browser, and sound categories. Email/browser/sound are stored for future phases but not delivered yet.
- `notifications.NotificationDeliveries` audits in-app delivery attempts for each created notification.
- `notifications.NotificationOutbox` reserves the future swap point for RabbitMQ, Azure Service Bus, or Kafka-backed publishing.

## API

- `GET /api/v1/notifications` supports paging, search, read filter, project filter, type filter, and priority filter.
- `GET /api/v1/notifications/unread-count` returns the signed-in user's unread count.
- `POST /api/v1/notifications/{id}/read` marks one notification read.
- `POST /api/v1/notifications/{id}/unread` marks one notification unread.
- `POST /api/v1/notifications/mark-all-read` marks all visible notifications read.
- `DELETE /api/v1/notifications/{id}` deletes one owned notification.
- `DELETE /api/v1/notifications/read` clears read notifications.
- `GET /api/v1/notifications/preferences` returns current-user preferences.
- `PUT /api/v1/notifications/preferences` updates current-user preferences.

## SignalR

- Hub: `/hubs/notifications`
- Auth: JWT bearer token via the standard Authorization header or SignalR `access_token` query parameter.
- Groups: `user:{userId}`. Clients never choose groups; the hub assigns the authenticated user group on connect.
- Events: `notification.created`, `notification.updated`, `notification.unreadCountChanged`, `workItem.updated`, `workItem.deleted`, `comment.created`, `reply.created`, `mention.created`.

## Security

- Every notification API and hub connection requires `notifications.view`.
- Notification list/actions are hard-scoped to the current user's `UserId`.
- The notification processor filters recipients by active user profile, project membership, permission, and user preferences before persisting or pushing events.
- Users do not receive project notifications unless they are active members of that project.

## Event Flow

1. Work/project command commits its primary database changes.
2. Command enqueues a `NotificationIntent`.
3. The in-process background worker resolves active permitted recipients.
4. Notifications and delivery audit rows are persisted.
5. SignalR publishes the notification and related live-update event to the user's group.
6. React updates unread badge, notification list, work item queries, comment queries, and activity queries without refresh.
