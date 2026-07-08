import {
  Avatar,
  Badge,
  Box,
  Button,
  Chip,
  Divider,
  Drawer,
  FormControlLabel,
  IconButton,
  InputAdornment,
  LinearProgress,
  MenuItem,
  Paper,
  Select,
  Stack,
  Switch,
  TextField,
  Tooltip,
  Typography,
  alpha,
  useTheme
} from '@mui/material';
import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AlertCircle,
  AtSign,
  Bell,
  CalendarClock,
  Check,
  CheckCheck,
  Eye,
  FolderKanban,
  MessageSquare,
  Paperclip,
  Search,
  Trash2,
  UserCheck,
  X
} from 'lucide-react';
import type { LucideIcon } from 'lucide-react';
import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { managementApi } from '../../shared/management/managementApi';
import type {
  NotificationPreferences,
  NotificationPreferencesRequest,
  NotificationType,
  WorkspaceNotification
} from '../../shared/management/managementTypes';
import { EmptyState } from '../../shared/ui/StateBlocks';
import { useToast } from '../../shared/ui/toastContext';

type NotificationFilter = 'All' | 'Unread' | 'Assignments' | 'Comments' | 'Mentions' | 'Projects' | 'System';

const pageSize = 24;
const filters: NotificationFilter[] = ['All', 'Unread', 'Assignments', 'Comments', 'Mentions', 'Projects', 'System'];

const assignmentTypes: NotificationType[] = [
  'TaskAssigned',
  'TaskReassigned',
  'TaskCompleted',
  'TaskReopened',
  'TaskOverdue',
  'TaskUpdated',
  'TaskDeleted',
  'StatusChanged',
  'PriorityChanged',
  'DueDateChanged',
  'WatcherAdded'
];

const commentTypes: NotificationType[] = ['Comment', 'Reply', 'AttachmentAdded'];
const projectTypes: NotificationType[] = ['ProjectInvitation', 'ProjectRemoved', 'ProjectArchived'];

export function NotificationDrawer({ open, onClose }: { open: boolean; onClose: () => void }) {
  return (
    <Drawer
      anchor="right"
      open={open}
      onClose={onClose}
      slotProps={{
        paper: {
          sx: {
            width: { xs: '100%', sm: 430 },
            maxWidth: '100vw'
          }
        }
      }}
    >
      <NotificationCenterPanel compact onNavigate={onClose} />
    </Drawer>
  );
}

export function NotificationCenterPanel({
  compact,
  onNavigate
}: {
  compact?: boolean;
  onNavigate?: () => void;
}) {
  const [search, setSearch] = useState('');
  const [filter, setFilter] = useState<NotificationFilter>('All');
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const navigate = useNavigate();
  const theme = useTheme();

  const query = useInfiniteQuery({
    queryKey: ['notifications', filter, search],
    queryFn: ({ pageParam }) =>
      managementApi.notifications.list(
        {
          pageNumber: pageParam,
          pageSize,
          search,
          sortBy: 'createdAt',
          sortDirection: 'desc'
        },
        toApiFilter(filter)
      ),
    initialPageParam: 1,
    getNextPageParam: (page) => (page.pageNumber < page.totalPages ? page.pageNumber + 1 : undefined)
  });

  const notifications = useMemo(
    () => query.data?.pages.flatMap((page) => page.items) ?? [],
    [query.data]
  );
  const visibleNotifications = useMemo(
    () => applyClientFilter(notifications, filter),
    [filter, notifications]
  );
  const invalidate = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['notifications'] }),
      queryClient.invalidateQueries({ queryKey: ['notifications', 'unread-count'] })
    ]);
  };

  const markReadMutation = useMutation({
    mutationFn: (id: string) => managementApi.notifications.markRead(id),
    onSuccess: invalidate,
    onError: () => showToast('Unable to update notification.', 'error')
  });

  const markUnreadMutation = useMutation({
    mutationFn: (id: string) => managementApi.notifications.markUnread(id),
    onSuccess: invalidate,
    onError: () => showToast('Unable to update notification.', 'error')
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => managementApi.notifications.delete(id),
    onSuccess: invalidate,
    onError: () => showToast('Unable to delete notification.', 'error')
  });

  const markAllReadMutation = useMutation({
    mutationFn: () => managementApi.notifications.markAllRead(),
    onSuccess: invalidate,
    onError: () => showToast('Unable to mark notifications read.', 'error')
  });

  const clearReadMutation = useMutation({
    mutationFn: () => managementApi.notifications.clearRead(),
    onSuccess: invalidate,
    onError: () => showToast('Unable to clear notifications.', 'error')
  });

  const openNotification = async (notification: WorkspaceNotification) => {
    if (!notification.isRead) {
      await markReadMutation.mutateAsync(notification.id);
    }

    if (notification.actionUrl?.startsWith('/app/')) {
      navigate(notification.actionUrl);
      onNavigate?.();
    }
  };

  return (
    <Stack sx={{ height: compact ? '100%' : 'auto', minHeight: compact ? 0 : 520 }}>
      <Stack
        spacing={1.5}
        sx={{
          p: compact ? 2.5 : 0,
          pb: compact ? 1.5 : 2,
          borderBottom: compact ? '1px solid' : 'none',
          borderColor: 'divider'
        }}
      >
        <Stack direction="row" sx={{ alignItems: 'center', gap: 1 }}>
            <Badge color="error" variant="dot" invisible={!notifications.some((item) => !item.isRead)}>
            <Bell size={20} />
          </Badge>
          <Box sx={{ flex: 1, minWidth: 0 }}>
            <Typography variant={compact ? 'h6' : 'h5'} sx={{ fontWeight: 850 }}>Notifications</Typography>
            <Typography variant="caption" color="text.secondary">Live workspace updates</Typography>
          </Box>
          {compact ? (
            <Tooltip title="Close">
              <IconButton onClick={onNavigate}>
                <X size={18} />
              </IconButton>
            </Tooltip>
          ) : null}
        </Stack>

        <TextField
          size="small"
          placeholder="Search notifications"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <Search size={16} />
                </InputAdornment>
              )
            }
          }}
        />

        <Stack direction="row" spacing={1}>
          <Select size="small" value={filter} onChange={(event) => setFilter(event.target.value as NotificationFilter)} sx={{ minWidth: 150 }}>
            {filters.map((item) => <MenuItem key={item} value={item}>{item}</MenuItem>)}
          </Select>
          <Button
            size="small"
            variant="outlined"
            startIcon={<CheckCheck size={16} />}
            onClick={() => markAllReadMutation.mutate()}
            disabled={markAllReadMutation.isPending}
          >
            Mark all read
          </Button>
          {!compact ? (
            <Button
              size="small"
              color="inherit"
              onClick={() => clearReadMutation.mutate()}
              disabled={clearReadMutation.isPending}
            >
              Clear read
            </Button>
          ) : null}
        </Stack>
      </Stack>

      {query.isFetching && !query.isFetchingNextPage ? <LinearProgress /> : null}

      <Box
        onScroll={(event) => {
          const target = event.currentTarget;
          if (query.hasNextPage && !query.isFetchingNextPage && target.scrollTop + target.clientHeight >= target.scrollHeight - 280) {
            void query.fetchNextPage();
          }
        }}
        sx={{
          flex: compact ? 1 : 'initial',
          minHeight: 0,
          overflowY: compact ? 'auto' : 'visible',
          px: compact ? 1.25 : 0,
          py: compact ? 1 : 0
        }}
      >
        {!query.isLoading && visibleNotifications.length === 0 ? (
          <Box sx={{ py: 8 }}>
            <EmptyState title="No notifications" description="You are all caught up." />
          </Box>
        ) : null}

        <Stack spacing={2}>
          {groupNotifications(visibleNotifications).map((group) => (
            <Stack key={group.label} spacing={0.75}>
              <DateHeader label={group.label} />
              {group.items.map((notification) => (
                <NotificationRow
                  key={notification.id}
                  notification={notification}
                  onOpen={() => void openNotification(notification)}
                  onRead={() => markReadMutation.mutate(notification.id)}
                  onUnread={() => markUnreadMutation.mutate(notification.id)}
                  onDelete={() => deleteMutation.mutate(notification.id)}
                />
              ))}
            </Stack>
          ))}
        </Stack>

        {query.hasNextPage ? (
          <Button
            fullWidth
            sx={{ mt: 2, mb: compact ? 2 : 0 }}
            onClick={() => void query.fetchNextPage()}
            disabled={query.isFetchingNextPage}
          >
            {query.isFetchingNextPage ? 'Loading...' : 'Load more'}
          </Button>
        ) : null}

        <Box sx={{ height: compact ? theme.spacing(2) : 0 }} />
      </Box>
    </Stack>
  );
}

export function NotificationPreferencesPanel() {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const query = useQuery({
    queryKey: ['notifications', 'preferences'],
    queryFn: () => managementApi.notifications.preferences()
  });

  const mutation = useMutation({
    mutationFn: (request: NotificationPreferencesRequest) => managementApi.notifications.updatePreferences(request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['notifications', 'preferences'] });
      showToast('Notification preferences updated.');
    },
    onError: () => showToast('Unable to update preferences.', 'error')
  });

  const preferences = query.data;

  if (!preferences) {
    return <Paper sx={{ p: 2, border: '1px solid', borderColor: 'divider' }}><LinearProgress /></Paper>;
  }

  const updatePreference = (key: keyof NotificationPreferencesRequest, value: boolean) => {
    mutation.mutate(toPreferenceRequest({ ...preferences, [key]: value }));
  };

  return (
    <Paper sx={{ p: 2.5, border: '1px solid', borderColor: 'divider', borderRadius: 2 }}>
      <Stack spacing={1.25}>
        <Typography variant="h6" sx={{ fontWeight: 850 }}>Preferences</Typography>
        <Divider />
        {preferenceRows.map((row) => (
          <FormControlLabel
            key={row.key}
            control={
              <Switch
                checked={Boolean(preferences[row.key])}
                onChange={(event) => updatePreference(row.key, event.target.checked)}
                disabled={mutation.isPending}
              />
            }
            label={
              <Stack>
                <Typography sx={{ fontWeight: 700 }}>{row.label}</Typography>
                <Typography variant="caption" color="text.secondary">{row.description}</Typography>
              </Stack>
            }
          />
        ))}
      </Stack>
    </Paper>
  );
}

function NotificationRow({
  notification,
  onOpen,
  onRead,
  onUnread,
  onDelete
}: {
  notification: WorkspaceNotification;
  onOpen: () => void;
  onRead: () => void;
  onUnread: () => void;
  onDelete: () => void;
}) {
  const theme = useTheme();
  const Icon = getNotificationIcon(notification.notificationType);

  return (
    <Box
      onClick={onOpen}
      sx={{
        position: 'relative',
        cursor: 'pointer',
        borderRadius: 2,
        px: 1.25,
        py: 1.2,
        border: '1px solid',
        borderColor: notification.isRead ? 'divider' : alpha(theme.palette.primary.main, 0.4),
        bgcolor: notification.isRead ? 'transparent' : alpha(theme.palette.primary.main, theme.palette.mode === 'dark' ? 0.12 : 0.06),
        '&:hover .notification-actions': { opacity: 1 }
      }}
    >
      <Stack direction="row" spacing={1.25}>
        <Avatar sx={{ width: 36, height: 36, bgcolor: getNotificationColor(notification.notificationType), color: 'white' }}>
          <Icon size={18} />
        </Avatar>
        <Box sx={{ minWidth: 0, flex: 1 }}>
          <Stack direction="row" spacing={1} sx={{ alignItems: 'center', minWidth: 0 }}>
            <Typography variant="body2" sx={{ fontWeight: notification.isRead ? 700 : 850, flex: 1 }} noWrap>
              {notification.title}
            </Typography>
            <Typography variant="caption" color="text.secondary" sx={{ flexShrink: 0 }}>
              {formatRelativeTime(notification.createdAt)}
            </Typography>
          </Stack>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 0.25 }}>
            {notification.message}
          </Typography>
          <Stack direction="row" spacing={0.75} sx={{ mt: 1, alignItems: 'center', flexWrap: 'wrap' }}>
            <Chip size="small" label={formatNotificationType(notification.notificationType)} />
            {notification.priority !== 'Normal' ? <Chip size="small" color={notification.priority === 'Critical' ? 'error' : 'warning'} label={notification.priority} /> : null}
            {notification.projectName ? <Typography variant="caption" color="text.secondary">{notification.projectName}</Typography> : null}
          </Stack>
        </Box>
      </Stack>

      <Stack
        className="notification-actions"
        direction="row"
        spacing={0.25}
        sx={{
          position: 'absolute',
          top: 6,
          right: 6,
          opacity: 0,
          transition: 'opacity 140ms ease',
          bgcolor: 'background.paper',
          borderRadius: 1.5,
          boxShadow: 1
        }}
        onClick={(event) => event.stopPropagation()}
      >
        <Tooltip title={notification.isRead ? 'Mark unread' : 'Mark read'}>
          <IconButton size="small" onClick={notification.isRead ? onUnread : onRead}>
            {notification.isRead ? <Eye size={15} /> : <Check size={15} />}
          </IconButton>
        </Tooltip>
        <Tooltip title="Delete">
          <IconButton size="small" color="error" onClick={onDelete}>
            <Trash2 size={15} />
          </IconButton>
        </Tooltip>
      </Stack>
    </Box>
  );
}

function DateHeader({ label }: { label: string }) {
  return (
    <Stack direction="row" spacing={1} sx={{ alignItems: 'center', py: 0.5 }}>
      <Typography variant="caption" sx={{ fontWeight: 850, color: 'text.secondary', textTransform: 'uppercase' }}>{label}</Typography>
      <Divider sx={{ flex: 1 }} />
    </Stack>
  );
}

function toApiFilter(filter: NotificationFilter) {
  if (filter === 'Unread') {
    return { isRead: false };
  }

  return {};
}

function applyClientFilter(notifications: WorkspaceNotification[], filter: NotificationFilter) {
  if (filter === 'Assignments') {
    return notifications.filter((notification) => assignmentTypes.includes(notification.notificationType));
  }

  if (filter === 'Comments') {
    return notifications.filter((notification) => commentTypes.includes(notification.notificationType));
  }

  if (filter === 'Mentions') {
    return notifications.filter((notification) => notification.notificationType === 'Mention');
  }

  if (filter === 'Projects') {
    return notifications.filter((notification) => projectTypes.includes(notification.notificationType));
  }

  if (filter === 'System') {
    return notifications.filter((notification) => notification.notificationType === 'SystemAnnouncement');
  }

  return notifications;
}

function groupNotifications(notifications: WorkspaceNotification[]) {
  const groups = new Map<string, WorkspaceNotification[]>();
  notifications.forEach((notification) => {
    const label = getDateLabel(notification.createdAt);
    groups.set(label, [...(groups.get(label) ?? []), notification]);
  });

  return Array.from(groups.entries()).map(([label, items]) => ({ label, items }));
}

function getDateLabel(value: string) {
  const date = new Date(value);
  const today = new Date();
  const startOfToday = new Date(today.getFullYear(), today.getMonth(), today.getDate()).getTime();
  const startOfDate = new Date(date.getFullYear(), date.getMonth(), date.getDate()).getTime();
  const diffDays = Math.round((startOfToday - startOfDate) / 86_400_000);

  if (diffDays === 0) return 'Today';
  if (diffDays === 1) return 'Yesterday';
  if (diffDays <= 7) return 'Last Week';
  return 'Older';
}

function formatRelativeTime(value: string) {
  const deltaSeconds = Math.max(1, Math.floor((Date.now() - new Date(value).getTime()) / 1000));
  if (deltaSeconds < 60) return 'now';
  const deltaMinutes = Math.floor(deltaSeconds / 60);
  if (deltaMinutes < 60) return `${deltaMinutes}m`;
  const deltaHours = Math.floor(deltaMinutes / 60);
  if (deltaHours < 24) return `${deltaHours}h`;
  return `${Math.floor(deltaHours / 24)}d`;
}

function getNotificationIcon(type: NotificationType): LucideIcon {
  if (assignmentTypes.includes(type)) return UserCheck;
  if (commentTypes.includes(type)) return MessageSquare;
  if (type === 'Mention') return AtSign;
  if (type === 'DueDateChanged') return CalendarClock;
  if (type === 'AttachmentAdded') return Paperclip;
  if (projectTypes.includes(type)) return FolderKanban;
  if (type === 'SystemAnnouncement') return Bell;
  return AlertCircle;
}

function getNotificationColor(type: NotificationType) {
  if (type === 'Mention') return '#7c3aed';
  if (type === 'AttachmentAdded') return '#0891b2';
  if (projectTypes.includes(type)) return '#2563eb';
  if (assignmentTypes.includes(type)) return '#16a34a';
  if (commentTypes.includes(type)) return '#db2777';
  return '#64748b';
}

function formatNotificationType(type: NotificationType) {
  return type.replace(/([a-z])([A-Z])/g, '$1 $2');
}

function toPreferenceRequest(preferences: NotificationPreferences): NotificationPreferencesRequest {
  return {
    assignmentNotifications: preferences.assignmentNotifications,
    commentNotifications: preferences.commentNotifications,
    mentionNotifications: preferences.mentionNotifications,
    replyNotifications: preferences.replyNotifications,
    projectNotifications: preferences.projectNotifications,
    emailNotifications: preferences.emailNotifications,
    browserNotifications: preferences.browserNotifications,
    soundNotifications: preferences.soundNotifications
  };
}

const preferenceRows: Array<{
  key: keyof NotificationPreferencesRequest;
  label: string;
  description: string;
}> = [
  { key: 'assignmentNotifications', label: 'Assignments', description: 'Task assignment, reassignment and task updates.' },
  { key: 'commentNotifications', label: 'Comments', description: 'Comments and attachment activity on work items.' },
  { key: 'mentionNotifications', label: 'Mentions', description: 'Direct mentions in threaded discussions.' },
  { key: 'replyNotifications', label: 'Replies', description: 'Replies inside comment threads.' },
  { key: 'projectNotifications', label: 'Projects', description: 'Project invitations and project lifecycle updates.' },
  { key: 'emailNotifications', label: 'Email', description: 'Reserved for the email phase.' },
  { key: 'browserNotifications', label: 'Browser', description: 'Reserved for the desktop/browser notification phase.' },
  { key: 'soundNotifications', label: 'Sound', description: 'Reserved for realtime sound cues.' }
];
