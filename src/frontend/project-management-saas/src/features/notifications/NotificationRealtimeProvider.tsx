import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import type { HubConnection } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import type { PropsWithChildren } from 'react';
import { useEffect, useMemo } from 'react';
import { useAuthStore } from '../auth/model/authStore';
import { environment } from '../../shared/config/environment';
import type { WorkspaceNotification } from '../../shared/management/managementTypes';
import { permissions } from '../../shared/security/permissions';
import { useToast } from '../../shared/ui/toastContext';

const emptyPermissionCodes: string[] = [];

interface RealtimeNotificationPayload extends WorkspaceNotification {
  unreadCount: number;
}

interface UnreadCountPayload {
  unreadCount: number;
}

interface WorkItemChangedPayload {
  workItemId?: string;
}

interface CommentChangedPayload extends WorkItemChangedPayload {
  commentId?: string | null;
}

export function NotificationRealtimeProvider({ children }: PropsWithChildren) {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const accessToken = useAuthStore((state) => state.accessToken);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const permissionCodes = useAuthStore((state) => state.user?.permissions ?? emptyPermissionCodes);
  const canViewNotifications = useMemo(
    () => permissionCodes.includes(permissions.notifications.view),
    [permissionCodes]
  );

  useEffect(() => {
    if (!isAuthenticated || !accessToken || !canViewNotifications) {
      return;
    }

    const connection = new HubConnectionBuilder()
      .withUrl(getNotificationHubUrl(), {
        accessTokenFactory: () => accessToken,
        withCredentials: true
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    bindRealtimeHandlers(connection, queryClient, showToast);
    void connection.start().catch(() => undefined);

    return () => {
      unbindRealtimeHandlers(connection);
      if (connection.state !== HubConnectionState.Disconnected) {
        void connection.stop();
      }
    };
  }, [accessToken, canViewNotifications, isAuthenticated, queryClient, showToast]);

  return <>{children}</>;
}

function bindRealtimeHandlers(
  connection: HubConnection,
  queryClient: ReturnType<typeof useQueryClient>,
  showToast: (message: string, severity?: 'success' | 'info' | 'warning' | 'error') => void
) {
  connection.on('notification.created', (payload: RealtimeNotificationPayload) => {
    queryClient.setQueryData(['notifications', 'unread-count'], payload.unreadCount);
    void queryClient.invalidateQueries({ queryKey: ['notifications'] });
    showToast(payload.message || payload.title, payload.priority === 'Critical' ? 'warning' : 'info');
  });

  connection.on('notification.unreadCountChanged', (payload: UnreadCountPayload) => {
    queryClient.setQueryData(['notifications', 'unread-count'], payload.unreadCount);
  });

  connection.on('notification.updated', (payload: UnreadCountPayload) => {
    queryClient.setQueryData(['notifications', 'unread-count'], payload.unreadCount);
    void queryClient.invalidateQueries({ queryKey: ['notifications'] });
  });

  connection.on('workItem.updated', (payload: WorkItemChangedPayload) => {
    invalidateWorkItemQueries(queryClient, payload.workItemId);
  });

  connection.on('workItem.deleted', (payload: WorkItemChangedPayload) => {
    invalidateWorkItemQueries(queryClient, payload.workItemId);
  });

  connection.on('comment.created', (payload: CommentChangedPayload) => {
    invalidateCommentQueries(queryClient, payload.workItemId);
  });

  connection.on('reply.created', (payload: CommentChangedPayload) => {
    invalidateCommentQueries(queryClient, payload.workItemId);
  });

  connection.on('mention.created', (payload: CommentChangedPayload) => {
    invalidateCommentQueries(queryClient, payload.workItemId);
  });
}

function unbindRealtimeHandlers(connection: HubConnection) {
  [
    'notification.created',
    'notification.unreadCountChanged',
    'notification.updated',
    'workItem.updated',
    'workItem.deleted',
    'comment.created',
    'reply.created',
    'mention.created'
  ].forEach((eventName) => connection.off(eventName));
}

function invalidateWorkItemQueries(queryClient: ReturnType<typeof useQueryClient>, workItemId?: string) {
  void queryClient.invalidateQueries({ queryKey: ['work-items'] });

  if (!workItemId) {
    return;
  }

  void queryClient.invalidateQueries({ queryKey: ['work-item', workItemId] });
  void queryClient.invalidateQueries({ queryKey: ['work-item', workItemId, 'activity'] });
}

function invalidateCommentQueries(queryClient: ReturnType<typeof useQueryClient>, workItemId?: string) {
  invalidateWorkItemQueries(queryClient, workItemId);

  if (!workItemId) {
    return;
  }

  void queryClient.invalidateQueries({ queryKey: ['work-item', workItemId, 'comments'] });
}

function getNotificationHubUrl() {
  return `${environment.apiBaseUrl.replace(/\/api\/v\d+(?:\.\d+)?$/i, '')}/hubs/notifications`;
}
