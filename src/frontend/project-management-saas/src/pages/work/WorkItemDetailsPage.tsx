import {
  Avatar,
  Box,
  Button,
  Checkbox,
  Chip,
  Divider,
  Drawer,
  IconButton,
  InputAdornment,
  Link,
  Menu,
  MenuItem,
  Paper,
  Skeleton,
  Stack,
  TextField,
  Tooltip,
  Typography
} from '@mui/material';
import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  ArrowLeft,
  Bold,
  CheckCircle2,
  ChevronDown,
  ChevronRight,
  Code2,
  Copy,
  Download,
  FileUp,
  History,
  Italic,
  Link as LinkIcon,
  Link2,
  List as ListIcon,
  ListChecks,
  ListOrdered,
  MessageSquare,
  Paperclip,
  Pencil,
  Pin,
  PinOff,
  Quote,
  RefreshCcw,
  Reply,
  RotateCcw,
  Search,
  Send,
  Smile,
  Trash2,
  Underline
} from 'lucide-react';
import { useEffect, useMemo, useRef, useState } from 'react';
import type { ComponentProps, MutableRefObject, ReactNode } from 'react';
import { Link as RouterLink, Navigate, useParams } from 'react-router-dom';
import { routePaths } from '../../app/router/routePaths';
import { getApiErrorMessage } from '../../shared/api/apiError';
import { useAuthStore } from '../../features/auth/model/authStore';
import { usePermissions } from '../../features/auth/model/usePermissions';
import { managementApi } from '../../shared/management/managementApi';
import type {
  WorkAttachment,
  WorkAttachmentRequest,
  WorkComment,
  WorkCommentAttachment,
  WorkCommentMention,
  WorkActivity,
  WorkMentionCandidate,
  WorkItemLink,
  WorkItemLinkRequest,
  WorkItemLinkType,
  WorkItemRequest
} from '../../shared/management/managementTypes';
import { permissions } from '../../shared/security/permissions';
import { ConfirmationDialog } from '../../shared/ui/ConfirmationDialog';
import { EmptyState, ErrorState } from '../../shared/ui/StateBlocks';
import { FormDialog } from '../../shared/ui/FormDialog';
import { PageHeader } from '../../shared/ui/PageHeader';
import { useToast } from '../../shared/ui/toastContext';
import { formatDate, formatDateTime, optional } from '../management/pageUtils';
import { WorkItemForm } from './workItemForm';
import { toRequest } from './workItemFormModel';

const pageRequest = { pageNumber: 1, pageSize: 50, sortBy: 'createdOn', sortDirection: 'desc' as const };
const commentPageRequest = { pageNumber: 1, pageSize: 40, sortBy: 'createdOn', sortDirection: 'desc' as const };
const activityPageRequest = { pageNumber: 1, pageSize: 40, sortBy: 'createdAt', sortDirection: 'desc' as const };
const linkTypes: WorkItemLinkType[] = ['Duplicate', 'Blocks', 'BlockedBy', 'RelatesTo', 'DependsOn'];
const supportedReactions = ['\u{1F44D}', '\u2764\uFE0F', '\u{1F602}', '\u{1F525}', '\u{1F389}', '\u{1F680}', '\u{1F440}', '\u{1F604}', '\u{1F622}', '\u2757'];
const emojiPickerOptions = [...supportedReactions, '\u2705', '\u{1F3AF}', '\u{1F4A1}', '\u{1F64F}'];
const longThreadReplyThreshold = 5;
const compactCommentWindowMs = 5 * 60 * 1000;
const activityCategories = [
  { value: '', label: 'All' },
  { value: 'Comments', label: 'Comments' },
  { value: 'Assignments', label: 'Assignments' },
  { value: 'StatusChanges', label: 'Status' },
  { value: 'Priority', label: 'Priority' },
  { value: 'Attachments', label: 'Attachments' },
  { value: 'Mentions', label: 'Mentions' },
  { value: 'Reactions', label: 'Reactions' },
  { value: 'System', label: 'System' }
] as const;

const emptyAttachment: WorkAttachmentRequest = {
  fileName: '',
  contentType: '',
  fileSizeBytes: 0,
  storagePath: '',
  version: 1,
  description: null
};

const emptyLink: WorkItemLinkRequest = {
  targetWorkItemId: '',
  linkType: 'RelatesTo',
  description: null
};

type PendingDelete =
  | { kind: 'comment'; id: string; label: string }
  | { kind: 'commentAttachment'; id: string; commentId: string; label: string }
  | { kind: 'attachment'; id: string; label: string }
  | { kind: 'link'; id: string; label: string }
  | null;

interface PendingCommentFile {
  id: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  storagePath: string;
}

export function WorkItemDetailsPage() {
  const { workItemId } = useParams();
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const { hasPermission } = usePermissions();
  const currentUser = useAuthStore((state) => state.user);
  const canUpdateWorkItem = hasPermission(permissions.workItems.update);
  const canTransitionWorkItem = hasPermission(permissions.workItems.transition);
  const canManageComments = hasPermission(permissions.workComments.manage);
  const canManageAttachments = hasPermission(permissions.workAttachments.manage);
  const canManageLinks = hasPermission(permissions.workLinks.manage);
  const canViewProjects = hasPermission(permissions.projects.view);
  const canViewUsers = hasPermission(permissions.users.view);
  const commentInputRef = useRef<HTMLTextAreaElement | null>(null);
  const [editOpen, setEditOpen] = useState(false);
  const [form, setForm] = useState(() => toRequestPlaceholder());
  const [transitionStatusId, setTransitionStatusId] = useState('');
  const [commentBody, setCommentBody] = useState('');
  const [mentionedUserIds, setMentionedUserIds] = useState<string[]>([]);
  const [editingComment, setEditingComment] = useState<WorkComment | null>(null);
  const [replyTo, setReplyTo] = useState<WorkComment | null>(null);
  const [pendingCommentFiles, setPendingCommentFiles] = useState<PendingCommentFile[]>([]);
  const [profileDrawerUser, setProfileDrawerUser] = useState<WorkMentionCandidate | WorkCommentMention | null>(null);
  const [attachmentOpen, setAttachmentOpen] = useState(false);
  const [attachmentForm, setAttachmentForm] = useState<WorkAttachmentRequest>(emptyAttachment);
  const [linkOpen, setLinkOpen] = useState(false);
  const [linkForm, setLinkForm] = useState<WorkItemLinkRequest>(emptyLink);
  const [pendingDelete, setPendingDelete] = useState<PendingDelete>(null);
  const [activityCategory, setActivityCategory] = useState('');
  const [activitySearch, setActivitySearch] = useState('');
  const debouncedActivitySearch = useDebouncedValue(activitySearch, 250);
  const canModerateComments = Boolean(currentUser?.roles.some((role) => ['Project Manager', 'Organization Admin', 'Super Admin', 'SystemAdministrator'].includes(role)));

  const workItemQuery = useQuery({
    queryKey: ['work-item', workItemId],
    queryFn: () => managementApi.work.items.get(workItemId!),
    enabled: Boolean(workItemId)
  });
  const workItem = workItemQuery.data;

  const metadataQuery = useQuery({
    queryKey: ['work', 'metadata', workItem?.projectId],
    queryFn: () => managementApi.work.metadata(workItem?.projectId),
    enabled: Boolean(workItem?.projectId)
  });
  const projectsQuery = useQuery({
    queryKey: ['projects', 'lookup'],
    queryFn: () => managementApi.projects.list({ pageNumber: 1, pageSize: 200, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canViewProjects
  });
  const usersQuery = useQuery({
    queryKey: ['users', 'lookup'],
    queryFn: () => managementApi.users.list({ pageNumber: 1, pageSize: 200, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canViewUsers
  });
  const commentsQuery = useInfiniteQuery({
    queryKey: ['work-item', workItemId, 'comments'],
    queryFn: ({ pageParam }) => managementApi.work.comments.list(workItemId!, { ...commentPageRequest, pageNumber: pageParam }),
    initialPageParam: 1,
    getNextPageParam: (lastPage) => lastPage.pageNumber < lastPage.totalPages ? lastPage.pageNumber + 1 : undefined,
    enabled: Boolean(workItemId)
  });
  const attachmentsQuery = useQuery({
    queryKey: ['work-item', workItemId, 'attachments'],
    queryFn: () => managementApi.work.attachments.list(workItemId!, pageRequest),
    enabled: Boolean(workItemId)
  });
  const activityQuery = useInfiniteQuery({
    queryKey: ['work-item', workItemId, 'activity', activityCategory, debouncedActivitySearch],
    queryFn: ({ pageParam }) => managementApi.work.activity.list(
      workItemId!,
      { ...activityPageRequest, pageNumber: pageParam, search: debouncedActivitySearch },
      { category: activityCategory || null }
    ),
    initialPageParam: 1,
    getNextPageParam: (lastPage) => lastPage.pageNumber < lastPage.totalPages ? lastPage.pageNumber + 1 : undefined,
    enabled: Boolean(workItemId)
  });
  const linksQuery = useQuery({
    queryKey: ['work-item', workItemId, 'links'],
    queryFn: () => managementApi.work.links.list(workItemId!, pageRequest),
    enabled: Boolean(workItemId)
  });
  const relatedLookupQuery = useQuery({
    queryKey: ['work-items', 'lookup', workItem?.projectId],
    queryFn: () => managementApi.work.items.list(
      { pageNumber: 1, pageSize: 100, sortBy: 'title', sortDirection: 'asc' },
      { projectId: workItem?.projectId }
    ),
    enabled: Boolean(workItem?.projectId) && canManageLinks
  });

  const userProfiles = useMemo(() => usersQuery.data?.items.filter((user) => user.profile).map((user) => user.profile!) ?? [], [usersQuery.data?.items]);
  const comments = useMemo(() => commentsQuery.data?.pages.flatMap((page) => page.items) ?? [], [commentsQuery.data]);
  const activities = useMemo(() => activityQuery.data?.pages.flatMap((page) => page.items) ?? [], [activityQuery.data]);
  const currentType = metadataQuery.data?.types.find((type) => type.id === workItem?.workItemTypeId);
  const currentWorkflow = metadataQuery.data?.workflows.find((workflow) => workflow.id === currentType?.workflowId);
  const availableStatuses = currentWorkflow?.transitions
    .filter((transition) => transition.fromStatusId === workItem?.workflowStatusId)
    .map((transition) => currentWorkflow.statuses.find((status) => status.id === transition.toStatusId))
    .filter((status): status is NonNullable<typeof status> => Boolean(status)) ?? [];

  useEffect(() => {
    const handler = (event: KeyboardEvent) => {
      if (event.key.toLowerCase() === 'm' && !event.ctrlKey && !event.metaKey && !event.altKey) {
        const target = event.target as HTMLElement | null;
        if (target?.tagName !== 'INPUT' && target?.tagName !== 'TEXTAREA') {
          event.preventDefault();
          commentInputRef.current?.focus();
        }
      }
    };

    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, []);

  const updateMutation = useMutation({
    mutationFn: () => managementApi.work.items.update(workItem!.id, form),
    onSuccess: async (saved) => {
      showToast('Work item updated.');
      setEditOpen(false);
      queryClient.setQueryData(['work-item', saved.id], saved);
      await queryClient.invalidateQueries({ queryKey: ['work-items'] });
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to update work item.'), 'error')
  });

  const transitionMutation = useMutation({
    mutationFn: () => managementApi.work.items.transition(workItem!.id, transitionStatusId, workItem!.rowVersion),
    onSuccess: async (saved) => {
      showToast('Status updated.');
      setTransitionStatusId('');
      queryClient.setQueryData(['work-item', saved.id], saved);
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['work-items'] }),
        queryClient.invalidateQueries({ queryKey: ['work-item', saved.id, 'activity'] })
      ]);
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to transition work item.'), 'error')
  });

  const addCommentMutation = useMutation({
    mutationFn: async () => {
      if (editingComment) {
        const saved = await managementApi.work.comments.update(workItemId!, editingComment.id, {
          parentCommentId: editingComment.parentCommentId,
          message: commentBody,
          bodyMarkdown: commentBody,
          mentionUserProfileIds: [],
          mentionedUserIds
        });

        await Promise.all(pendingCommentFiles.map((file) => managementApi.work.comments.attachments.add(workItemId!, saved.id, toCommentAttachmentRequest(file))));
        return saved;
      }

      const saved = await managementApi.work.comments.add(workItemId!, {
        parentCommentId: replyTo?.id ?? null,
        message: commentBody,
        bodyMarkdown: commentBody,
        mentionUserProfileIds: [],
        mentionedUserIds
      });

      await Promise.all(pendingCommentFiles.map((file) => managementApi.work.comments.attachments.add(workItemId!, saved.id, toCommentAttachmentRequest(file))));
      return saved;
    },
    onSuccess: async () => {
      showToast(editingComment ? 'Comment updated.' : 'Comment added.');
      setCommentBody('');
      setMentionedUserIds([]);
      setEditingComment(null);
      setReplyTo(null);
      setPendingCommentFiles([]);
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['work-item', workItemId] }),
        queryClient.invalidateQueries({ queryKey: ['work-item', workItemId, 'comments'] }),
        queryClient.invalidateQueries({ queryKey: ['work-item', workItemId, 'activity'] })
      ]);
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to save comment.'), 'error')
  });

  const addAttachmentMutation = useMutation({
    mutationFn: () => managementApi.work.attachments.add(workItemId!, attachmentForm),
    onSuccess: async () => {
      showToast('Attachment added.');
      setAttachmentForm(emptyAttachment);
      setAttachmentOpen(false);
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['work-item', workItemId] }),
        queryClient.invalidateQueries({ queryKey: ['work-item', workItemId, 'attachments'] }),
        queryClient.invalidateQueries({ queryKey: ['work-item', workItemId, 'activity'] })
      ]);
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to add attachment.'), 'error')
  });

  const addLinkMutation = useMutation({
    mutationFn: () => managementApi.work.links.add(workItemId!, linkForm),
    onSuccess: async () => {
      showToast('Work item linked.');
      setLinkForm(emptyLink);
      setLinkOpen(false);
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['work-item', workItemId, 'links'] }),
        queryClient.invalidateQueries({ queryKey: ['work-item', workItemId, 'activity'] })
      ]);
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to link work item.'), 'error')
  });

  const commentActionMutation = useMutation({
    mutationFn: ({ action, comment }: { action: 'restore' | 'pin' | 'unpin' | 'resolve' | 'unresolve'; comment: WorkComment }) => {
      if (action === 'restore') {
        return managementApi.work.comments.restore(workItemId!, comment.id);
      }

      if (action === 'pin') {
        return managementApi.work.comments.pin(workItemId!, comment.id);
      }

      if (action === 'unpin') {
        return managementApi.work.comments.unpin(workItemId!, comment.id);
      }

      if (action === 'resolve') {
        return managementApi.work.comments.resolve(workItemId!, comment.id);
      }

      return managementApi.work.comments.unresolve(workItemId!, comment.id);
    },
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['work-item', workItemId, 'comments'] }),
        queryClient.invalidateQueries({ queryKey: ['work-item', workItemId, 'activity'] })
      ]);
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to update comment.'), 'error')
  });

  const reactionMutation = useMutation({
    mutationFn: ({ comment, emoji }: { comment: WorkComment; emoji: string }) => managementApi.work.comments.reactions.toggle(workItemId!, comment.id, { emoji }),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['work-item', workItemId, 'comments'] }),
        queryClient.invalidateQueries({ queryKey: ['work-item', workItemId, 'activity'] })
      ]);
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to update reaction.'), 'error')
  });

  const markAllReadMutation = useMutation({
    mutationFn: () => managementApi.work.comments.markAllRead(workItemId!),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['work-item', workItemId, 'comments'] });
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to mark comments as read.'), 'error')
  });

  const deleteMutation = useMutation({
    mutationFn: (target: Exclude<PendingDelete, null>) => {
      if (target.kind === 'comment') {
        return managementApi.work.comments.delete(workItemId!, target.id);
      }

      if (target.kind === 'commentAttachment') {
        return managementApi.work.comments.attachments.delete(workItemId!, target.commentId, target.id);
      }

      if (target.kind === 'attachment') {
        return managementApi.work.attachments.delete(workItemId!, target.id);
      }

      return managementApi.work.links.delete(workItemId!, target.id);
    },
    onSuccess: async () => {
      showToast('Record deleted.');
      setPendingDelete(null);
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['work-item', workItemId] }),
        queryClient.invalidateQueries({ queryKey: ['work-item', workItemId, 'comments'] }),
        queryClient.invalidateQueries({ queryKey: ['work-item', workItemId, 'attachments'] }),
        queryClient.invalidateQueries({ queryKey: ['work-item', workItemId, 'links'] }),
        queryClient.invalidateQueries({ queryKey: ['work-item', workItemId, 'activity'] })
      ]);
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to delete record.'), 'error')
  });

  useEffect(() => {
    const hasUnread = comments.some((comment) => !comment.isDeleted && !comment.readByCurrentUser);
    if (workItemId && canManageComments && hasUnread && !markAllReadMutation.isPending) {
      const timeout = window.setTimeout(() => markAllReadMutation.mutate(), 900);
      return () => window.clearTimeout(timeout);
    }

    return undefined;
  }, [canManageComments, comments, markAllReadMutation.isPending, workItemId]);

  if (!workItemId) {
    return <Navigate to={routePaths.workItems} replace />;
  }

  if (workItemQuery.isLoading) {
    return <Skeleton variant="rounded" height={420} />;
  }

  if (workItemQuery.isError || !workItem) {
    return <ErrorState title="Unable to load work item" description="The requested work item was not returned by the server." onRetry={() => void workItemQuery.refetch()} />;
  }

  return (
    <Stack spacing={2} sx={{ height: { xs: 'auto', xl: 'calc(100vh - 96px)' }, minHeight: 0 }}>
      <PageHeader
        title={workItem.title}
        eyebrow={workItem.typeName}
        description={workItem.projectName}
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.app },
          { label: 'Work Items', path: routePaths.workItems },
          { label: workItem.title }
        ]}
        actions={(
          <>
            <Button component={RouterLink} to={routePaths.workItems} startIcon={<ArrowLeft size={16} />} variant="outlined">Back</Button>
            {canUpdateWorkItem ? (
              <Button
                startIcon={<Pencil size={16} />}
                variant="contained"
                onClick={() => {
                  setForm(toRequest(workItem));
                  setEditOpen(true);
                }}
              >
                Edit
              </Button>
            ) : null}
          </>
        )}
      />

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', xl: 'minmax(0, 1.05fr) minmax(430px, 0.95fr)' }, gap: 2.5, alignItems: 'stretch', flex: 1, minHeight: 0, overflow: { xs: 'visible', xl: 'hidden' } }}>
        <Stack spacing={2.5} sx={{ minHeight: 0, overflow: { xs: 'visible', xl: 'auto' }, pr: { xl: 1 }, pb: 1 }}>
          <Panel title="Summary">
            <Stack spacing={2}>
              <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>
                <ColorChip label={workItem.typeName} color={workItem.typeColor} />
                <ColorChip label={workItem.statusName} color={workItem.statusColor} />
                <ColorChip label={workItem.priorityName} color={workItem.priorityColor} />
              </Stack>
              <Typography color="text.secondary" sx={{ whiteSpace: 'pre-wrap' }}>{optional(workItem.description)}</Typography>
              <Divider />
              <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, minmax(0, 1fr))', lg: 'repeat(3, minmax(0, 1fr))' }, gap: 2 }}>
                <Info label="Assignee" value={optional(workItem.assigneeName)} />
                <Info label="Reporter" value={workItem.reporterName} />
                <Info label="Parent" value={optional(workItem.parentTitle)} />
                <Info label="Start Date" value={formatDate(workItem.startDate)} />
                <Info label="Due Date" value={formatDate(workItem.dueDate)} />
                <Info label="Estimated Hours" value={workItem.estimatedHours?.toString() ?? '-'} />
                <Info label="Logged Hours" value={workItem.loggedHours.toString()} />
                <Info label="Story Points" value={workItem.storyPoints?.toString() ?? '-'} />
                <Info label="Severity" value={optional(workItem.severity)} />
                <Info label="Environment" value={optional(workItem.environment)} />
                <Info label="Reproducible" value={workItem.reproducible === null ? '-' : workItem.reproducible ? 'Yes' : 'No'} />
                <Info label="Created On" value={formatDateTime(workItem.createdOn)} />
              </Box>
              <Divider />
              <SectionChips title="Labels" items={workItem.labels.map((label) => ({ label: label.name, color: label.color }))} />
              <SectionChips title="Components" items={workItem.components.map((component) => ({ label: component.name, color: component.color }))} />
              <SectionChips title="Watchers" items={workItem.watchers.map((watcher) => ({ label: watcher.displayName, color: '#64748b' }))} />
              <Box>
                <Typography variant="subtitle2" sx={{ mb: 1 }}>Acceptance Criteria</Typography>
                <Typography color="text.secondary" sx={{ whiteSpace: 'pre-wrap' }}>{optional(workItem.acceptanceCriteria)}</Typography>
              </Box>
            </Stack>
          </Panel>

          <Panel
            title="Workflow"
            action={canTransitionWorkItem ? <Button startIcon={<RefreshCcw size={16} />} variant="outlined" disabled={!transitionStatusId || transitionMutation.isPending} onClick={() => transitionMutation.mutate()}>Transition</Button> : undefined}
          >
            <TextField
              select
              label="Next Status"
              disabled={!canTransitionWorkItem}
              value={transitionStatusId}
              onChange={(event) => setTransitionStatusId(event.target.value)}
              helperText={availableStatuses.length === 0 ? 'No active transition is available from the current status.' : undefined}
            >
              <MenuItem value="">Select status</MenuItem>
              {availableStatuses.map((status) => <MenuItem key={status.id} value={status.id}>{status.name}</MenuItem>)}
            </TextField>
          </Panel>

          <Panel
            title="Attachments"
            action={canManageAttachments ? <Button startIcon={<FileUp size={16} />} variant="outlined" onClick={() => setAttachmentOpen(true)}>Add</Button> : undefined}
          >
            <AttachmentList
              attachments={attachmentsQuery.data?.items ?? []}
              loading={attachmentsQuery.isLoading}
              canManage={canManageAttachments}
              onDelete={(attachment) => setPendingDelete({ kind: 'attachment', id: attachment.id, label: attachment.fileName })}
            />
          </Panel>

          <Panel
            title="Linked Work"
            action={canManageLinks ? <Button startIcon={<Link2 size={16} />} variant="outlined" onClick={() => setLinkOpen(true)}>Link</Button> : undefined}
          >
            <LinkList
              links={linksQuery.data?.items ?? []}
              loading={linksQuery.isLoading}
              canManage={canManageLinks}
              onDelete={(link) => setPendingDelete({ kind: 'link', id: link.id, label: link.targetTitle })}
            />
          </Panel>

          <ActivityTimelinePanel
            activities={activities}
            loading={activityQuery.isLoading}
            fetchingMore={activityQuery.isFetchingNextPage}
            hasMore={Boolean(activityQuery.hasNextPage)}
            category={activityCategory}
            search={activitySearch}
            onCategoryChange={setActivityCategory}
            onSearchChange={setActivitySearch}
            onLoadMore={() => void activityQuery.fetchNextPage()}
          />
        </Stack>

        <Stack spacing={0} sx={{ minHeight: 0 }}>
          <Panel
            title="Comments"
            sx={{ height: { xs: 'auto', xl: '100%' }, minHeight: { xs: 560, xl: 0 }, display: 'flex', flexDirection: 'column' }}
            action={(
              <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
                <Chip icon={<MessageSquare size={14} />} label={workItem.commentCount} size="small" />
                {canManageComments ? (
                  <Button size="small" variant="outlined" disabled={markAllReadMutation.isPending} onClick={() => markAllReadMutation.mutate()}>
                    Mark All Read
                  </Button>
                ) : null}
              </Stack>
            )}
          >
            <Box sx={{ display: 'grid', gridTemplateRows: canManageComments ? 'minmax(0, 1fr) auto' : 'minmax(0, 1fr)', minHeight: { xs: 520, xl: 0 }, height: { xs: '72vh', xl: '100%' } }}>
              <Box
                onScroll={(event) => {
                  const target = event.currentTarget;
                  if (commentsQuery.hasNextPage && !commentsQuery.isFetchingNextPage && target.scrollTop + target.clientHeight >= target.scrollHeight - 320) {
                    void commentsQuery.fetchNextPage();
                  }
                }}
                sx={{ minHeight: 0, overflowY: 'auto', pr: 0.75, pb: 2 }}
              >
              <ThreadedComments
                comments={comments}
                loading={commentsQuery.isLoading}
                fetchingMore={commentsQuery.isFetchingNextPage}
                hasMore={Boolean(commentsQuery.hasNextPage)}
                canManage={canManageComments}
                canModerate={canModerateComments}
                currentUserId={currentUser?.id ?? null}
                workItemId={workItemId}
                onReply={(comment) => {
                  setEditingComment(null);
                  setReplyTo(comment);
                  setCommentBody('');
                  setPendingCommentFiles([]);
                  setMentionedUserIds([]);
                  commentInputRef.current?.focus();
                }}
                onEdit={(comment) => {
                  setEditingComment(comment);
                  setReplyTo(null);
                  setPendingCommentFiles([]);
                  setCommentBody(comment.message || comment.bodyMarkdown);
                  setMentionedUserIds([...(comment.mentionedUserIds ?? [])]);
                  commentInputRef.current?.focus();
                }}
                onDelete={(comment) => setPendingDelete({ kind: 'comment', id: comment.id, label: 'comment' })}
                onRestore={(comment) => commentActionMutation.mutate({ action: 'restore', comment })}
                onPinToggle={(comment) => commentActionMutation.mutate({ action: comment.pinned ? 'unpin' : 'pin', comment })}
                onResolveToggle={(comment) => commentActionMutation.mutate({ action: comment.resolved ? 'unresolve' : 'resolve', comment })}
                onReact={(comment, emoji) => reactionMutation.mutate({ comment, emoji })}
                onOpenProfile={setProfileDrawerUser}
                onDeleteAttachment={(comment, attachment) => setPendingDelete({ kind: 'commentAttachment', id: attachment.id, commentId: comment.id, label: attachment.fileName })}
                onLoadMore={() => void commentsQuery.fetchNextPage()}
              />
              </Box>
              {canManageComments ? (
                <Box sx={{ borderTop: '1px solid', borderColor: 'divider', bgcolor: 'background.paper', pt: 2 }}>
                  <Stack spacing={1.5}>
                    <RichCommentEditor
                      value={commentBody}
                      onChange={setCommentBody}
                      inputRef={commentInputRef}
                      workItemId={workItemId}
                      editingComment={editingComment}
                      replyTo={replyTo}
                      pendingFiles={pendingCommentFiles}
                      mentionedUserIds={mentionedUserIds}
                      onMentionedUserIdsChange={setMentionedUserIds}
                      onOpenProfile={setProfileDrawerUser}
                      onClearContext={() => {
                        setEditingComment(null);
                        setReplyTo(null);
                        setPendingCommentFiles([]);
                        setCommentBody('');
                        setMentionedUserIds([]);
                      }}
                      onFilesSelected={(files) => setPendingCommentFiles((current) => [...current, ...files.map(toPendingCommentFile)])}
                      onRemoveFile={(fileId) => setPendingCommentFiles((current) => current.filter((file) => file.id !== fileId))}
                    />
                    <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end', flexWrap: 'wrap' }}>
                      {editingComment || replyTo || pendingCommentFiles.length || mentionedUserIds.length ? (
                        <Button variant="outlined" onClick={() => { setEditingComment(null); setReplyTo(null); setPendingCommentFiles([]); setCommentBody(''); setMentionedUserIds([]); }}>Cancel</Button>
                      ) : null}
                      <Button startIcon={<Send size={16} />} variant="contained" disabled={!commentBody.trim() || addCommentMutation.isPending} onClick={() => addCommentMutation.mutate()}>
                        {editingComment ? 'Update' : replyTo ? 'Reply' : 'Comment'}
                      </Button>
                    </Stack>
                  </Stack>
                </Box>
              ) : null}
            </Box>
          </Panel>

        </Stack>
      </Box>

      <FormDialog open={editOpen} title="Edit Work Item" loading={updateMutation.isPending} onClose={() => setEditOpen(false)} onSubmit={() => updateMutation.mutate()}>
        <WorkItemForm
          form={form}
          setForm={setForm}
          projects={projectsQuery.data?.items ?? []}
          userProfiles={userProfiles}
          metadata={metadataQuery.data}
        />
      </FormDialog>

      <FormDialog open={attachmentOpen} title="Add Attachment" loading={addAttachmentMutation.isPending} onClose={() => setAttachmentOpen(false)} onSubmit={() => addAttachmentMutation.mutate()}>
        <Stack spacing={2}>
          <Button component="label" startIcon={<FileUp size={16} />} variant="outlined">
            Select File
            <input
              hidden
              type="file"
              onChange={(event) => {
                const file = event.target.files?.[0];
                if (!file) {
                  return;
                }

                setAttachmentForm({
                  ...attachmentForm,
                  fileName: file.name,
                  contentType: file.type,
                  fileSizeBytes: file.size
                });
              }}
            />
          </Button>
          <TextField label="File Name" required value={attachmentForm.fileName} onChange={(event) => setAttachmentForm({ ...attachmentForm, fileName: event.target.value })} />
          <TextField label="Content Type" required value={attachmentForm.contentType} onChange={(event) => setAttachmentForm({ ...attachmentForm, contentType: event.target.value })} />
          <TextField label="File Size Bytes" type="number" value={attachmentForm.fileSizeBytes} onChange={(event) => setAttachmentForm({ ...attachmentForm, fileSizeBytes: Number(event.target.value) })} />
          <TextField label="Storage Path" required value={attachmentForm.storagePath} onChange={(event) => setAttachmentForm({ ...attachmentForm, storagePath: event.target.value })} />
          <TextField label="Version" type="number" value={attachmentForm.version} onChange={(event) => setAttachmentForm({ ...attachmentForm, version: Number(event.target.value) })} />
          <TextField label="Description" multiline minRows={2} value={attachmentForm.description ?? ''} onChange={(event) => setAttachmentForm({ ...attachmentForm, description: event.target.value || null })} />
        </Stack>
      </FormDialog>

      <FormDialog open={linkOpen} title="Link Work Item" loading={addLinkMutation.isPending} onClose={() => setLinkOpen(false)} onSubmit={() => addLinkMutation.mutate()}>
        <Stack spacing={2}>
          <TextField select label="Target Work Item" required value={linkForm.targetWorkItemId} onChange={(event) => setLinkForm({ ...linkForm, targetWorkItemId: event.target.value })}>
            {relatedLookupQuery.data?.items.filter((item) => item.id !== workItem.id).map((item) => <MenuItem key={item.id} value={item.id}>{item.title}</MenuItem>)}
          </TextField>
          <TextField select label="Relationship" required value={linkForm.linkType} onChange={(event) => setLinkForm({ ...linkForm, linkType: event.target.value as WorkItemLinkType })}>
            {linkTypes.map((type) => <MenuItem key={type} value={type}>{formatLinkType(type)}</MenuItem>)}
          </TextField>
          <TextField label="Description" multiline minRows={2} value={linkForm.description ?? ''} onChange={(event) => setLinkForm({ ...linkForm, description: event.target.value || null })} />
        </Stack>
      </FormDialog>

      <UserProfileDrawer user={profileDrawerUser} onClose={() => setProfileDrawerUser(null)} />

      <ConfirmationDialog
        open={pendingDelete !== null}
        title="Delete record"
        description={pendingDelete ? `Delete ${pendingDelete.label}?` : ''}
        confirmLabel="Delete"
        confirmColor="error"
        onCancel={() => setPendingDelete(null)}
        onConfirm={() => {
          if (pendingDelete) {
            deleteMutation.mutate(pendingDelete);
          }
        }}
      />
    </Stack>
  );
}

function Panel({ title, action, children, sx }: { title: string; action?: ReactNode; children: ReactNode; sx?: ComponentProps<typeof Paper>['sx'] }) {
  return (
    <Paper sx={[{ p: 2.5, border: '1px solid', borderColor: 'divider', borderRadius: 3 }, ...(Array.isArray(sx) ? sx : sx ? [sx] : [])]}>
      <Stack spacing={2} sx={{ height: '100%', minHeight: 0 }}>
        <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between', gap: 2 }}>
          <Typography variant="h6" sx={{ fontWeight: 850 }}>{title}</Typography>
          {action}
        </Stack>
        {children}
      </Stack>
    </Paper>
  );
}

function Info({ label, value }: { label: string; value: string }) {
  return (
    <Box>
      <Typography variant="caption" color="text.secondary">{label}</Typography>
      <Typography sx={{ fontWeight: 700, wordBreak: 'break-word' }}>{value}</Typography>
    </Box>
  );
}

function ColorChip({ label, color }: { label: string; color: string }) {
  return <Chip size="small" label={label} sx={{ bgcolor: color, color: '#fff', fontWeight: 760 }} />;
}

function SectionChips({ title, items }: { title: string; items: Array<{ label: string; color: string }> }) {
  return (
    <Box>
      <Typography variant="subtitle2" sx={{ mb: 1 }}>{title}</Typography>
      <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>
        {items.length ? items.map((item) => <ColorChip key={`${title}-${item.label}`} label={item.label} color={item.color} />) : <Typography color="text.secondary">-</Typography>}
      </Stack>
    </Box>
  );
}

function RichCommentEditor({
  value,
  onChange,
  inputRef,
  workItemId,
  editingComment,
  replyTo,
  pendingFiles,
  mentionedUserIds,
  onMentionedUserIdsChange,
  onOpenProfile,
  onClearContext,
  onFilesSelected,
  onRemoveFile
}: {
  value: string;
  onChange: (value: string) => void;
  inputRef: MutableRefObject<HTMLTextAreaElement | null>;
  workItemId?: string;
  editingComment: WorkComment | null;
  replyTo: WorkComment | null;
  pendingFiles: PendingCommentFile[];
  mentionedUserIds: string[];
  onMentionedUserIdsChange: (userIds: string[]) => void;
  onOpenProfile: (user: WorkMentionCandidate | WorkCommentMention) => void;
  onClearContext: () => void;
  onFilesSelected: (files: File[]) => void;
  onRemoveFile: (fileId: string) => void;
}) {
  const [emojiOpen, setEmojiOpen] = useState(false);
  const [mentionRange, setMentionRange] = useState<{ start: number; end: number } | null>(null);
  const [mentionSearch, setMentionSearch] = useState('');
  const [highlightedMentionIndex, setHighlightedMentionIndex] = useState(0);
  const [selectedMentionDetails, setSelectedMentionDetails] = useState<WorkMentionCandidate[]>([]);
  const debouncedMentionSearch = useDebouncedValue(mentionSearch, 200);

  const mentionCandidatesQuery = useQuery({
    queryKey: ['work-item', workItemId, 'mention-candidates', debouncedMentionSearch],
    queryFn: () => managementApi.work.comments.mentionCandidates(workItemId!, { pageNumber: 1, pageSize: 20, search: debouncedMentionSearch, sortBy: 'name', sortDirection: 'asc' }),
    enabled: Boolean(workItemId && mentionRange)
  });

  const mentionCandidates = (mentionCandidatesQuery.data?.items ?? []).filter((candidate) => !mentionedUserIds.includes(candidate.userId));

  useEffect(() => {
    if (editingComment) {
      setSelectedMentionDetails(editingComment.mentions.map(toCandidateFromMention));
    } else if (mentionedUserIds.length === 0) {
      setSelectedMentionDetails([]);
    }
  }, [editingComment, mentionedUserIds.length]);

  const closeMentionSearch = () => {
    setMentionRange(null);
    setMentionSearch('');
    setHighlightedMentionIndex(0);
  };

  const syncMentionToken = (nextValue: string, caret: number | null) => {
    if (caret === null) {
      closeMentionSearch();
      return;
    }

    const beforeCaret = nextValue.slice(0, caret);
    const match = /(^|\s)@([\w.-]*)$/.exec(beforeCaret);
    if (!match) {
      closeMentionSearch();
      return;
    }

    const token = match[2] ?? '';
    setMentionRange({ start: caret - token.length - 1, end: caret });
    setMentionSearch(token);
    setHighlightedMentionIndex(0);
  };

  const selectMention = (candidate: WorkMentionCandidate) => {
    if (!mentionRange) {
      return;
    }

    const nextValue = `${value.slice(0, mentionRange.start)}@${candidate.displayName} ${value.slice(mentionRange.end)}`;
    const nextCaret = mentionRange.start + candidate.displayName.length + 2;
    onChange(nextValue);

    if (!mentionedUserIds.includes(candidate.userId)) {
      onMentionedUserIdsChange([...mentionedUserIds, candidate.userId]);
      setSelectedMentionDetails((current) => current.some((item) => item.userId === candidate.userId) ? current : [...current, candidate]);
    }

    closeMentionSearch();
    window.requestAnimationFrame(() => {
      inputRef.current?.focus();
      inputRef.current?.setSelectionRange(nextCaret, nextCaret);
    });
  };

  const openMentionSearch = () => {
    const textarea = inputRef.current;
    const start = textarea?.selectionStart ?? value.length;
    const end = textarea?.selectionEnd ?? value.length;
    const prefix = start > 0 && !/\s/.test(value[start - 1]) ? ' @' : '@';
    const nextValue = `${value.slice(0, start)}${prefix}${value.slice(end)}`;
    const caret = start + prefix.length;
    onChange(nextValue);
    setMentionRange({ start: caret - 1, end: caret });
    setMentionSearch('');
    setHighlightedMentionIndex(0);

    window.requestAnimationFrame(() => {
      inputRef.current?.focus();
      inputRef.current?.setSelectionRange(caret, caret);
    });
  };

  const removeMention = (userId: string) => {
    onMentionedUserIdsChange(mentionedUserIds.filter((id) => id !== userId));
    setSelectedMentionDetails((current) => current.filter((item) => item.userId !== userId));
  };

  const insertSnippet = (before: string, after = before, fallback = 'text') => {
    const textarea = inputRef.current;
    if (!textarea) {
      onChange(`${value}${before}${fallback}${after}`);
      return;
    }

    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const selected = value.slice(start, end) || fallback;
    const next = `${value.slice(0, start)}${before}${selected}${after}${value.slice(end)}`;
    onChange(next);
    window.requestAnimationFrame(() => {
      textarea.focus();
      textarea.setSelectionRange(start + before.length, start + before.length + selected.length);
    });
  };

  const prefixLines = (prefix: string) => {
    const textarea = inputRef.current;
    const start = textarea?.selectionStart ?? value.length;
    const end = textarea?.selectionEnd ?? value.length;
    const selected = value.slice(start, end) || 'text';
    const nextBlock = selected.split('\n').map((line) => `${prefix}${line}`).join('\n');
    onChange(`${value.slice(0, start)}${nextBlock}${value.slice(end)}`);
  };

  const collectFiles = (fileList: FileList | null | undefined) => {
    const files = Array.from(fileList ?? []);
    if (files.length) {
      onFilesSelected(files);
    }
  };

  return (
    <Stack spacing={1.25}>
      {editingComment || replyTo ? (
        <Stack
          direction="row"
          spacing={1}
          sx={{ alignItems: 'center', border: '1px solid', borderColor: 'divider', borderRadius: 2, px: 1.5, py: 1, bgcolor: 'action.hover' }}
        >
          <Reply size={16} />
          <Box
            sx={{ minWidth: 0, flex: 1, cursor: replyTo ? 'pointer' : 'default' }}
            onClick={() => {
              if (replyTo) {
                document.getElementById(`comment-${replyTo.id}`)?.scrollIntoView({ behavior: 'smooth', block: 'center' });
              }
            }}
          >
            <Typography variant="caption" color="text.secondary">{editingComment ? 'Editing' : `Replying to ${replyTo?.authorName ?? 'comment'}`}</Typography>
            <Typography variant="body2" noWrap>{editingComment ? editingComment.message : replyTo?.message}</Typography>
          </Box>
          <IconButton size="small" onClick={onClearContext} aria-label="Clear comment context"><RotateCcw size={15} /></IconButton>
        </Stack>
      ) : null}

      <Stack direction="row" spacing={0.5} sx={{ flexWrap: 'wrap', alignItems: 'center' }}>
        <IconButton size="small" aria-label="Bold" onClick={() => insertSnippet('**')}><Bold size={16} /></IconButton>
        <IconButton size="small" aria-label="Italic" onClick={() => insertSnippet('*')}><Italic size={16} /></IconButton>
        <IconButton size="small" aria-label="Underline" onClick={() => insertSnippet('<u>', '</u>')}><Underline size={16} /></IconButton>
        <IconButton size="small" aria-label="Bulleted list" onClick={() => prefixLines('- ')}><ListIcon size={16} /></IconButton>
        <IconButton size="small" aria-label="Numbered list" onClick={() => prefixLines('1. ')}><ListOrdered size={16} /></IconButton>
        <IconButton size="small" aria-label="Checklist" onClick={() => prefixLines('- [ ] ')}><ListChecks size={16} /></IconButton>
        <IconButton size="small" aria-label="Quote" onClick={() => prefixLines('> ')}><Quote size={16} /></IconButton>
        <IconButton size="small" aria-label="Code block" onClick={() => insertSnippet('```\n', '\n```', 'code')}><Code2 size={16} /></IconButton>
        <IconButton size="small" aria-label="Inline code" onClick={() => insertSnippet('`')}><Code2 size={16} /></IconButton>
        <IconButton size="small" aria-label="Link" onClick={() => insertSnippet('[', '](https://)', 'link')}><LinkIcon size={16} /></IconButton>
        <IconButton size="small" aria-label="Mention" onClick={openMentionSearch}>@</IconButton>
        <IconButton size="small" aria-label="Emoji" onClick={() => setEmojiOpen((open) => !open)}><Smile size={16} /></IconButton>
        <Button component="label" size="small" startIcon={<Paperclip size={15} />} variant="outlined">
          Attach
          <input hidden multiple type="file" onChange={(event) => collectFiles(event.target.files)} />
        </Button>
      </Stack>

      {emojiOpen ? (
        <Stack direction="row" spacing={0.5} sx={{ flexWrap: 'wrap' }}>
          {emojiPickerOptions.map((emoji) => (
            <IconButton key={emoji} size="small" aria-label={`Insert ${emoji}`} onClick={() => insertSnippet(emoji, '', '')}>{emoji}</IconButton>
          ))}
        </Stack>
      ) : null}

      {selectedMentionDetails.length ? (
        <Stack direction="row" spacing={0.75} sx={{ flexWrap: 'wrap', alignItems: 'center' }}>
          {selectedMentionDetails.map((mention) => (
            <Chip
              key={mention.userId}
              avatar={<Avatar src={mention.profilePhoto ?? undefined}>{getCommentInitials(mention.displayName)}</Avatar>}
              label={`@${mention.displayName}`}
              size="small"
              onClick={() => onOpenProfile(mention)}
              onDelete={() => removeMention(mention.userId)}
            />
          ))}
        </Stack>
      ) : null}

      <TextField
        multiline
        minRows={4}
        label={editingComment ? 'Edit Comment' : replyTo ? 'Reply' : 'Add Comment'}
        value={value}
        onChange={(event) => {
          onChange(event.target.value);
          syncMentionToken(event.target.value, event.target.selectionStart);
        }}
        onKeyDown={(event) => {
          if (!mentionRange) {
            return;
          }

          if (event.key === 'Escape') {
            event.preventDefault();
            closeMentionSearch();
          } else if (event.key === 'ArrowDown') {
            event.preventDefault();
            setHighlightedMentionIndex((index) => Math.min(index + 1, Math.max(mentionCandidates.length - 1, 0)));
          } else if (event.key === 'ArrowUp') {
            event.preventDefault();
            setHighlightedMentionIndex((index) => Math.max(index - 1, 0));
          } else if ((event.key === 'Enter' || event.key === 'Tab') && mentionCandidates[highlightedMentionIndex]) {
            event.preventDefault();
            selectMention(mentionCandidates[highlightedMentionIndex]);
          }
        }}
        inputRef={inputRef}
        onPaste={(event) => collectFiles(event.clipboardData.files)}
        onDragOver={(event) => event.preventDefault()}
        onDrop={(event) => {
          event.preventDefault();
          collectFiles(event.dataTransfer.files);
        }}
      />

      {mentionRange ? (
        <Stack spacing={0.5} sx={{ border: '1px solid', borderColor: 'divider', borderRadius: 2, p: 1, bgcolor: 'background.paper', boxShadow: 1 }}>
          {mentionCandidatesQuery.isFetching ? <Typography variant="caption" color="text.secondary">Searching project members...</Typography> : null}
          {!mentionCandidatesQuery.isFetching && mentionCandidates.length === 0 ? <Typography variant="caption" color="text.secondary">No matching project members.</Typography> : null}
          {mentionCandidates.map((candidate, index) => (
            <Button
              key={candidate.userId}
              color="inherit"
              onClick={() => selectMention(candidate)}
              sx={{
                justifyContent: 'flex-start',
                textAlign: 'left',
                bgcolor: index === highlightedMentionIndex ? 'action.hover' : 'transparent'
              }}
            >
              <Stack direction="row" spacing={1} sx={{ alignItems: 'center', minWidth: 0 }}>
                <Avatar src={candidate.profilePhoto ?? undefined} sx={{ width: 28, height: 28, fontSize: 12 }}>{getCommentInitials(candidate.displayName)}</Avatar>
                <Box sx={{ minWidth: 0 }}>
                  <Typography variant="body2" sx={{ fontWeight: 760 }} noWrap>{candidate.displayName}</Typography>
                  <Typography variant="caption" color="text.secondary" noWrap>
                    {[candidate.employeeCode, candidate.designation, candidate.department].filter(Boolean).join(' - ')}
                  </Typography>
                </Box>
              </Stack>
            </Button>
          ))}
        </Stack>
      ) : null}

      {pendingFiles.length ? (
        <Stack spacing={0.75}>
          {pendingFiles.map((file) => (
            <Stack key={file.id} direction="row" spacing={1} sx={{ alignItems: 'center', border: '1px solid', borderColor: 'divider', borderRadius: 2, px: 1.25, py: 0.75 }}>
              <Paperclip size={15} />
              <Typography variant="body2" sx={{ flex: 1, minWidth: 0 }} noWrap>{file.fileName}</Typography>
              <Typography variant="caption" color="text.secondary">{formatFileSize(file.fileSizeBytes)}</Typography>
              <IconButton size="small" onClick={() => onRemoveFile(file.id)} aria-label="Remove file"><Trash2 size={14} /></IconButton>
            </Stack>
          ))}
        </Stack>
      ) : null}
    </Stack>
  );
}

function UserProfileDrawer({
  user,
  onClose
}: {
  user: WorkMentionCandidate | WorkCommentMention | null;
  onClose: () => void;
}) {
  if (!user) {
    return null;
  }

  const email = 'email' in user ? user.email : null;
  const createdAt = 'createdAt' in user ? user.createdAt : null;

  return (
    <Drawer anchor="right" open={Boolean(user)} onClose={onClose} slotProps={{ paper: { sx: { width: { xs: '100%', sm: 380 }, p: 3 } } }}>
      <Stack spacing={2.5}>
        <Stack direction="row" spacing={1.5} sx={{ alignItems: 'center' }}>
          <Avatar src={user.profilePhoto ?? undefined} sx={{ width: 56, height: 56, fontSize: 20, fontWeight: 800 }}>
            {getCommentInitials(user.displayName)}
          </Avatar>
          <Box sx={{ minWidth: 0 }}>
            <Typography variant="h6" sx={{ fontWeight: 820 }} noWrap>{user.displayName}</Typography>
            <Typography variant="body2" color="text.secondary" noWrap>{optional(user.designation)}</Typography>
          </Box>
        </Stack>
        <Divider />
        <Stack spacing={1.5}>
          <Info label="Employee Code" value={optional(user.employeeCode)} />
          <Info label="Department" value={optional(user.department)} />
          {email ? <Info label="Email" value={email} /> : null}
          {createdAt ? <Info label="Mentioned On" value={formatDateTime(createdAt)} /> : null}
        </Stack>
      </Stack>
    </Drawer>
  );
}

function toCandidateFromMention(mention: WorkCommentMention): WorkMentionCandidate {
  return {
    userId: mention.mentionedUserId,
    userProfileId: mention.userProfileId,
    displayName: mention.displayName,
    employeeCode: mention.employeeCode,
    email: '',
    designation: mention.designation,
    department: mention.department,
    profilePhoto: mention.profilePhoto
  };
}

function groupCommentReactions(reactions: WorkComment['reactions'], currentUserId: string | null) {
  const groups = new Map<string, { count: number; reactedByCurrentUser: boolean; names: string[] }>();
  const normalizedCurrentUserId = currentUserId?.toLowerCase();

  reactions.forEach((reaction) => {
    const group = groups.get(reaction.emoji) ?? { count: 0, reactedByCurrentUser: false, names: [] };
    group.count += 1;
    group.names.push(reaction.displayName);
    group.reactedByCurrentUser = group.reactedByCurrentUser || Boolean(normalizedCurrentUserId && reaction.userId.toLowerCase() === normalizedCurrentUserId);
    groups.set(reaction.emoji, group);
  });

  return groups;
}

function formatReadReceiptSummary(names: string[]) {
  const uniqueNames = Array.from(new Set(names.filter(Boolean)));
  if (uniqueNames.length === 0) {
    return '';
  }

  if (uniqueNames.length <= 2) {
    return `Seen by ${uniqueNames.join(', ')}`;
  }

  return `Seen by ${uniqueNames.slice(0, 2).join(', ')} +${uniqueNames.length - 2} more`;
}

function useDebouncedValue<TValue>(value: TValue, delayMs: number) {
  const [debouncedValue, setDebouncedValue] = useState(value);

  useEffect(() => {
    const timeout = window.setTimeout(() => setDebouncedValue(value), delayMs);
    return () => window.clearTimeout(timeout);
  }, [delayMs, value]);

  return debouncedValue;
}

function ActivityTimelinePanel({
  activities,
  loading,
  fetchingMore,
  hasMore,
  category,
  search,
  onCategoryChange,
  onSearchChange,
  onLoadMore
}: {
  activities: WorkActivity[];
  loading: boolean;
  fetchingMore: boolean;
  hasMore: boolean;
  category: string;
  search: string;
  onCategoryChange: (category: string) => void;
  onSearchChange: (search: string) => void;
  onLoadMore: () => void;
}) {
  const [exportAnchor, setExportAnchor] = useState<HTMLElement | null>(null);
  const groupedActivities = useMemo(() => groupActivitiesByPeriod(activities), [activities]);

  const exportCsv = () => {
    const header = ['Time', 'Actor', 'Role', 'Category', 'Action', 'Field', 'Old Value', 'New Value'].map(csvEscape).join(',');
    const body = activities.map((activity) => [
      activity.createdAt,
      activity.actorName,
      activity.actorRole ?? '',
      activity.category,
      activity.description,
      activity.fieldName ?? '',
      activity.oldValue ?? '',
      activity.newValue ?? ''
    ].map((value) => csvEscape(String(value))).join(',')).join('\n');
    downloadBlob('work-item-activity.csv', 'text/csv;charset=utf-8', `${header}\n${body}`);
  };

  const exportPdf = async () => {
    const [{ default: jsPDF }, { default: autoTable }] = await Promise.all([
      import('jspdf'),
      import('jspdf-autotable')
    ]);
    const doc = new jsPDF({ orientation: 'landscape' });
    doc.text('Work Item Activity', 14, 14);
    autoTable(doc, {
      head: [['Time', 'Actor', 'Category', 'Action', 'Change']],
      body: activities.map((activity) => [
        formatDateTime(activity.createdAt),
        activity.actorName,
        activity.category,
        activity.description,
        activity.fieldName ? `${activity.fieldName}: ${optional(activity.oldValue)} -> ${optional(activity.newValue)}` : ''
      ]),
      startY: 20
    });
    doc.save('work-item-activity.pdf');
  };

  return (
    <Panel
      title="Activity"
      action={(
        <>
          <Button startIcon={<Download size={16} />} variant="outlined" size="small" disabled={!activities.length} onClick={(event) => setExportAnchor(event.currentTarget)}>
            Export
          </Button>
          <Menu anchorEl={exportAnchor} open={Boolean(exportAnchor)} onClose={() => setExportAnchor(null)}>
            <MenuItem onClick={() => { exportCsv(); setExportAnchor(null); }}>Export CSV</MenuItem>
            <MenuItem onClick={() => { void exportPdf(); setExportAnchor(null); }}>Export PDF</MenuItem>
          </Menu>
        </>
      )}
    >
      <Stack spacing={1.75}>
        <TextField
          size="small"
          placeholder="Search activity"
          value={search}
          onChange={(event) => onSearchChange(event.target.value)}
          slotProps={{ input: { startAdornment: <InputAdornment position="start"><Search size={16} /></InputAdornment> } }}
        />
        <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>
          {activityCategories.map((item) => (
            <Chip
              key={item.value || 'all'}
              label={item.label}
              size="small"
              color={category === item.value ? 'primary' : 'default'}
              variant={category === item.value ? 'filled' : 'outlined'}
              onClick={() => onCategoryChange(item.value)}
            />
          ))}
        </Stack>
        <Divider />
        {loading ? Array.from({ length: 4 }).map((_, index) => <Skeleton key={index} height={70} />) : null}
        {!loading && !activities.length ? <EmptyState title="No activity yet" /> : null}
        {groupedActivities.map((group) => (
          <Stack key={group.label} spacing={1.25}>
            <DateDivider label={group.label} />
            {group.activities.map((activity) => <ActivityTimelineItem key={activity.id} activity={activity} />)}
          </Stack>
        ))}
        {hasMore ? (
          <Button variant="outlined" disabled={fetchingMore} onClick={onLoadMore}>
            {fetchingMore ? 'Loading...' : 'Load more activity'}
          </Button>
        ) : null}
      </Stack>
    </Panel>
  );
}

function ActivityTimelineItem({ activity }: { activity: WorkActivity }) {
  return (
    <Stack direction="row" spacing={1.5} sx={{ alignItems: 'flex-start' }}>
      <Avatar src={activity.actorAvatar ?? undefined} sx={{ width: 34, height: 34, fontSize: 13, fontWeight: 800 }}>{getCommentInitials(activity.actorName)}</Avatar>
      <Box sx={{ flex: 1, minWidth: 0, borderLeft: '2px solid', borderColor: 'divider', pl: 1.5, pb: 1 }}>
        <Stack direction="row" spacing={1} sx={{ alignItems: 'center', flexWrap: 'wrap' }}>
          <Typography sx={{ fontWeight: 800 }}>{activity.actorName}</Typography>
          {activity.actorRole ? <Typography variant="caption" color="text.secondary">{activity.actorRole}</Typography> : null}
          <Chip size="small" variant="outlined" label={formatActivityCategory(activity.category)} />
          <Typography variant="caption" color="text.secondary">{formatRelativeTime(activity.createdAt)}</Typography>
        </Stack>
        <Typography sx={{ fontWeight: 700 }}>{activity.description}</Typography>
        <Typography variant="caption" color="text.secondary">{activity.activityType} - {formatDateTime(activity.createdAt)}</Typography>
        {activity.fieldName ? (
          <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
            {formatActivityField(activity.fieldName)}: {optional(activity.oldValue)} -&gt; {optional(activity.newValue)}
          </Typography>
        ) : null}
      </Box>
    </Stack>
  );
}

function DateDivider({ label }: { label: string }) {
  return (
    <Stack direction="row" spacing={1.25} sx={{ alignItems: 'center' }}>
      <Divider sx={{ flex: 1 }} />
      <Chip size="small" label={label} variant="outlined" />
      <Divider sx={{ flex: 1 }} />
    </Stack>
  );
}

function groupCommentsByDate(comments: WorkComment[]) {
  const groups = new Map<string, WorkComment[]>();
  comments.forEach((comment) => {
    const label = formatCommentDateGroup(comment.createdOn);
    groups.set(label, [...(groups.get(label) ?? []), comment]);
  });

  return Array.from(groups.entries()).map(([label, groupedComments]) => ({ label, comments: groupedComments }));
}

function groupActivitiesByPeriod(activities: WorkActivity[]) {
  const groups = new Map<string, WorkActivity[]>();
  activities.forEach((activity) => {
    const label = formatActivityPeriod(activity.createdAt);
    groups.set(label, [...(groups.get(label) ?? []), activity]);
  });

  return Array.from(groups.entries()).map(([label, groupedActivities]) => ({ label, activities: groupedActivities }));
}

function formatCommentDateGroup(value: string) {
  const date = new Date(value);
  const now = new Date();
  if (isSameDate(date, now)) {
    return 'Today';
  }

  const yesterday = new Date(now);
  yesterday.setDate(now.getDate() - 1);
  if (isSameDate(date, yesterday)) {
    return 'Yesterday';
  }

  const daysAgo = Math.floor((startOfDay(now).getTime() - startOfDay(date).getTime()) / 86400000);
  if (daysAgo > 1 && daysAgo < 7) {
    return date.toLocaleDateString(undefined, { weekday: 'long' });
  }

  if (daysAgo >= 7 && daysAgo < 14) {
    return 'Last Week';
  }

  return date.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
}

function formatActivityPeriod(value: string) {
  const date = new Date(value);
  const now = new Date();
  if (isSameDate(date, now)) {
    return 'Today';
  }

  const yesterday = new Date(now);
  yesterday.setDate(now.getDate() - 1);
  if (isSameDate(date, yesterday)) {
    return 'Yesterday';
  }

  const daysAgo = Math.floor((startOfDay(now).getTime() - startOfDay(date).getTime()) / 86400000);
  if (daysAgo < 7) {
    return 'This Week';
  }

  if (daysAgo < 14) {
    return 'Last Week';
  }

  return 'Older';
}

function startOfDay(date: Date) {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate());
}

function isSameDate(left: Date, right: Date) {
  return left.getFullYear() === right.getFullYear() && left.getMonth() === right.getMonth() && left.getDate() === right.getDate();
}

function shouldCompactComment(previous: WorkComment | null, comment: WorkComment) {
  if (!previous || previous.isDeleted || comment.isDeleted) {
    return false;
  }

  const sameAuthor = previous.authorId && comment.authorId
    ? previous.authorId.toLowerCase() === comment.authorId.toLowerCase()
    : previous.authorName === comment.authorName;

  return sameAuthor && Math.abs(new Date(previous.createdOn).getTime() - new Date(comment.createdOn).getTime()) <= compactCommentWindowMs;
}

function formatRelativeTime(value: string) {
  const diffMs = Date.now() - new Date(value).getTime();
  const minute = 60 * 1000;
  const hour = 60 * minute;
  const day = 24 * hour;

  if (diffMs < minute) {
    return 'Just now';
  }

  if (diffMs < hour) {
    const minutes = Math.max(1, Math.floor(diffMs / minute));
    return `${minutes}m ago`;
  }

  if (diffMs < day) {
    const hours = Math.floor(diffMs / hour);
    return `${hours}h ago`;
  }

  const days = Math.floor(diffMs / day);
  return `${days}d ago`;
}

function formatActivityCategory(value: string) {
  return value.replace(/([a-z])([A-Z])/g, '$1 $2');
}

function formatActivityField(value: string) {
  return value.replace(/^WorkItem\./, '').replace(/^WorkComment\./, '').replace(/([a-z])([A-Z])/g, '$1 $2');
}

function csvEscape(value: string) {
  return `"${value.replaceAll('"', '""')}"`;
}

function downloadBlob(fileName: string, type: string, content: string) {
  const blob = new Blob([content], { type });
  const link = document.createElement('a');
  link.href = URL.createObjectURL(blob);
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(link.href);
}

function ThreadedComments({
  comments,
  loading,
  fetchingMore,
  hasMore,
  canManage,
  canModerate,
  currentUserId,
  workItemId,
  onReply,
  onEdit,
  onDelete,
  onRestore,
  onPinToggle,
  onResolveToggle,
  onReact,
  onOpenProfile,
  onDeleteAttachment,
  onLoadMore
}: {
  comments: WorkComment[];
  loading: boolean;
  fetchingMore: boolean;
  hasMore: boolean;
  canManage: boolean;
  canModerate: boolean;
  currentUserId: string | null;
  workItemId?: string;
  onReply: (comment: WorkComment) => void;
  onEdit: (comment: WorkComment) => void;
  onDelete: (comment: WorkComment) => void;
  onRestore: (comment: WorkComment) => void;
  onPinToggle: (comment: WorkComment) => void;
  onResolveToggle: (comment: WorkComment) => void;
  onReact: (comment: WorkComment, emoji: string) => void;
  onOpenProfile: (user: WorkMentionCandidate | WorkCommentMention) => void;
  onDeleteAttachment: (comment: WorkComment, attachment: WorkCommentAttachment) => void;
  onLoadMore: () => void;
}) {
  const [collapsedIds, setCollapsedIds] = useState<Set<string>>(() => new Set());
  const [highlightedCommentId, setHighlightedCommentId] = useState<string | null>(null);
  const [visibleRootCount, setVisibleRootCount] = useState(30);
  const commentRefs = useRef(new Map<string, HTMLDivElement>());

  const commentsById = useMemo(() => new Map(comments.map((comment) => [comment.id, comment])), [comments]);
  const childrenByParent = useMemo(() => {
    const map = new Map<string | null, WorkComment[]>();
    comments.forEach((comment) => {
      const parentId = comment.parentCommentId ?? null;
      map.set(parentId, [...(map.get(parentId) ?? []), comment]);
    });

    map.forEach((children, parentId) => {
      children.sort((left, right) => {
        const pinnedOrder = Number(right.pinned) - Number(left.pinned);
        if (pinnedOrder !== 0) {
          return pinnedOrder;
        }

        return parentId === null
          ? new Date(right.createdOn).getTime() - new Date(left.createdOn).getTime()
          : new Date(left.createdOn).getTime() - new Date(right.createdOn).getTime();
      });
    });

    return map;
  }, [comments]);
  const rootComments = childrenByParent.get(null) ?? [];
  const visibleRootComments = rootComments.slice(0, visibleRootCount);
  const groupedRootComments = useMemo(() => groupCommentsByDate(visibleRootComments), [visibleRootComments]);

  const scrollToComment = (commentId: string) => {
    const node = commentRefs.current.get(commentId);
    if (!node) {
      return;
    }

    node.scrollIntoView({ behavior: 'smooth', block: 'center' });
    setHighlightedCommentId(commentId);
    window.setTimeout(() => setHighlightedCommentId(null), 1800);
  };

  useEffect(() => {
    setCollapsedIds((current) => {
      const next = new Set(current);
      comments
        .filter((comment) => (comment.resolved && comment.replyCount > 0) || comment.replyCount > longThreadReplyThreshold)
        .forEach((comment) => next.add(comment.id));
      return next.size === current.size ? current : next;
    });
  }, [comments]);

  useEffect(() => {
    setVisibleRootCount(30);
  }, [comments.length]);

  useEffect(() => {
    const commentId = window.location.hash.startsWith('#comment-') ? window.location.hash.replace('#comment-', '') : null;
    if (commentId) {
      window.setTimeout(() => scrollToComment(commentId), 100);
    }
  }, [comments]);

  if (loading) {
    return <Stack spacing={1}>{Array.from({ length: 3 }).map((_, index) => <Skeleton key={index} height={72} />)}</Stack>;
  }

  if (!comments.length) {
    return <EmptyState title="No comments yet" />;
  }

  return (
    <Stack spacing={2}>
      {groupedRootComments.map((group) => (
        <Stack key={group.label} spacing={1.25}>
          <DateDivider label={group.label} />
          {group.comments.map((comment, index) => {
            const previous = group.comments[index - 1] ?? null;
            return (
              <CommentNode
                key={comment.id}
                comment={comment}
                commentsById={commentsById}
                childrenByParent={childrenByParent}
                collapsedIds={collapsedIds}
                highlightedCommentId={highlightedCommentId}
                currentUserId={currentUserId}
                workItemId={workItemId}
                depth={0}
                compact={shouldCompactComment(previous, comment)}
                canManage={canManage}
                canModerate={canModerate}
                setCommentRef={(commentId, node) => {
                  if (node) {
                    commentRefs.current.set(commentId, node);
                  } else {
                    commentRefs.current.delete(commentId);
                  }
                }}
                onToggleCollapse={(commentId) => setCollapsedIds((current) => {
                  const next = new Set(current);
                  if (next.has(commentId)) {
                    next.delete(commentId);
                  } else {
                    next.add(commentId);
                  }
                  return next;
                })}
                onJumpToComment={scrollToComment}
                onReply={onReply}
                onEdit={onEdit}
                onDelete={onDelete}
                onRestore={onRestore}
                onPinToggle={onPinToggle}
                onResolveToggle={onResolveToggle}
                onReact={onReact}
                onOpenProfile={onOpenProfile}
                onDeleteAttachment={onDeleteAttachment}
              />
            );
          })}
        </Stack>
      ))}
      {visibleRootCount < rootComments.length ? (
        <Button variant="text" onClick={() => setVisibleRootCount((count) => count + 30)}>
          Show more comments
        </Button>
      ) : null}
      {hasMore ? (
        <Button variant="outlined" disabled={fetchingMore} onClick={onLoadMore}>
          {fetchingMore ? 'Loading...' : 'Load older comments'}
        </Button>
      ) : null}
    </Stack>
  );
}

function CommentNode({
  comment,
  commentsById,
  childrenByParent,
  collapsedIds,
  highlightedCommentId,
  currentUserId,
  workItemId,
  depth,
  compact,
  canManage,
  canModerate,
  setCommentRef,
  onToggleCollapse,
  onJumpToComment,
  onReply,
  onEdit,
  onDelete,
  onRestore,
  onPinToggle,
  onResolveToggle,
  onReact,
  onOpenProfile,
  onDeleteAttachment
}: {
  comment: WorkComment;
  commentsById: Map<string, WorkComment>;
  childrenByParent: Map<string | null, WorkComment[]>;
  collapsedIds: Set<string>;
  highlightedCommentId: string | null;
  currentUserId: string | null;
  workItemId?: string;
  depth: number;
  compact: boolean;
  canManage: boolean;
  canModerate: boolean;
  setCommentRef: (commentId: string, node: HTMLDivElement | null) => void;
  onToggleCollapse: (commentId: string) => void;
  onJumpToComment: (commentId: string) => void;
  onReply: (comment: WorkComment) => void;
  onEdit: (comment: WorkComment) => void;
  onDelete: (comment: WorkComment) => void;
  onRestore: (comment: WorkComment) => void;
  onPinToggle: (comment: WorkComment) => void;
  onResolveToggle: (comment: WorkComment) => void;
  onReact: (comment: WorkComment, emoji: string) => void;
  onOpenProfile: (user: WorkMentionCandidate | WorkCommentMention) => void;
  onDeleteAttachment: (comment: WorkComment, attachment: WorkCommentAttachment) => void;
}) {
  const [historyOpen, setHistoryOpen] = useState(false);
  const children = childrenByParent.get(comment.id) ?? [];
  const isCollapsed = collapsedIds.has(comment.id);
  const isOwnComment = Boolean(currentUserId && comment.authorId && comment.authorId.toLowerCase() === currentUserId.toLowerCase());
  const parentComment = comment.parentCommentId ? commentsById.get(comment.parentCommentId) : null;
  const canEdit = canManage && isOwnComment && !comment.isDeleted;
  const canDelete = canManage && !comment.isDeleted && (isOwnComment || canModerate);
  const canRestore = canManage && comment.isDeleted && (isOwnComment || canModerate);
  const canPin = canManage && canModerate && !comment.isDeleted;
  const canResolve = canManage && !comment.isDeleted;
  const canReply = canManage && !comment.isDeleted;
  const showActions = canReply || canEdit || canDelete || canRestore || canPin || canResolve;
  const isUnread = canManage && !comment.isDeleted && !comment.readByCurrentUser && !isOwnComment;
  const reactionGroups = groupCommentReactions(comment.reactions ?? [], currentUserId);
  const historyQuery = useQuery({
    queryKey: ['work-item', workItemId, 'comments', comment.id, 'history'],
    queryFn: () => managementApi.work.comments.history(workItemId!, comment.id, pageRequest),
    enabled: Boolean(workItemId && historyOpen)
  });

  const copyLink = async () => {
    const url = `${window.location.origin}${window.location.pathname}#comment-${comment.id}`;
    await navigator.clipboard?.writeText(url);
    window.history.replaceState(null, '', `#comment-${comment.id}`);
    onJumpToComment(comment.id);
  };

  return (
    <Box ref={(node: HTMLDivElement | null) => setCommentRef(comment.id, node)} id={`comment-${comment.id}`} sx={{ pl: Math.min(depth, 4) * 2, borderLeft: depth ? '1px solid' : 'none', borderColor: 'divider' }}>
      <Stack
        spacing={1.25}
        sx={{
          p: compact ? 1.25 : 1.5,
          border: '1px solid',
          borderColor: highlightedCommentId === comment.id ? 'primary.main' : 'divider',
          borderRadius: 1.5,
          bgcolor: highlightedCommentId === comment.id ? 'action.selected' : isUnread ? 'rgba(25, 118, 210, 0.06)' : isOwnComment ? 'action.hover' : 'background.paper',
          opacity: comment.isDeleted ? 0.72 : 1,
          '& .comment-actions': {
            opacity: { xs: 1, md: 0 },
            transition: 'opacity 140ms ease'
          },
          '&:hover .comment-actions, &:focus-within .comment-actions': {
            opacity: 1
          }
        }}
      >
        <Stack direction="row" spacing={1.25} sx={{ alignItems: 'flex-start' }}>
          <Avatar sx={{ width: 34, height: 34, fontSize: 13, fontWeight: 800, visibility: compact ? 'hidden' : 'visible' }}>{getCommentInitials(comment.authorName)}</Avatar>
          <Box sx={{ minWidth: 0, flex: 1 }}>
            {compact ? (
              <Typography variant="caption" color="text.secondary">
                {formatRelativeTime(comment.createdOn)}{comment.isEdited ? ' - edited' : ''}
              </Typography>
            ) : (
              <>
                <Stack direction="row" spacing={1} sx={{ alignItems: 'center', flexWrap: 'wrap' }}>
                  <Typography sx={{ fontWeight: 800 }}>{comment.authorName}</Typography>
                  {comment.authorRole ? <Typography variant="caption" color="text.secondary">{comment.authorRole}</Typography> : null}
                  {comment.pinned ? <Chip icon={<Pin size={13} />} label="Pinned" size="small" /> : null}
                  {comment.resolved ? <Chip icon={<CheckCircle2 size={13} />} label="Resolved" size="small" color="success" variant="outlined" /> : null}
                  {isUnread ? <Chip label="Unread" size="small" color="primary" variant="outlined" /> : null}
                </Stack>
                <Typography variant="caption" color="text.secondary">
                  {formatDateTime(comment.createdOn)}{comment.isEdited ? ` - edited ${comment.editedAt ? formatDateTime(comment.editedAt) : ''}` : ''}
                </Typography>
              </>
            )}
          </Box>
          <Stack direction="row" spacing={0.25} className="comment-actions">
            <Tooltip title="Copy link"><IconButton size="small" onClick={() => void copyLink()} aria-label="Copy comment link"><Copy size={15} /></IconButton></Tooltip>
            {canManage && !comment.isDeleted ? <Tooltip title="React"><IconButton size="small" onClick={() => onReact(comment, supportedReactions[0])} aria-label="React"><Smile size={15} /></IconButton></Tooltip> : null}
          </Stack>
        </Stack>

        {parentComment ? (
          <Box
            onClick={() => onJumpToComment(parentComment.id)}
            sx={{ borderLeft: '3px solid', borderColor: 'primary.main', bgcolor: 'action.hover', borderRadius: 1, px: 1.25, py: 0.75, cursor: 'pointer' }}
          >
            <Typography variant="caption" color="text.secondary">{parentComment.authorName}</Typography>
            <Typography variant="body2" noWrap>{parentComment.isDeleted ? 'Deleted comment' : parentComment.message}</Typography>
          </Box>
        ) : null}

        {comment.isDeleted ? (
          <Typography color="text.secondary" sx={{ fontStyle: 'italic' }}>Comment deleted.</Typography>
        ) : (
          <MarkdownMessage text={comment.message || comment.bodyMarkdown} />
        )}

        {!comment.isDeleted && comment.attachments.length ? (
          <CommentAttachmentList
            attachments={comment.attachments}
            canManage={isOwnComment || canModerate}
            onDelete={(attachment) => onDeleteAttachment(comment, attachment)}
          />
        ) : null}

        {!comment.isDeleted && comment.mentions.length ? (
          <Stack direction="row" spacing={0.75} sx={{ flexWrap: 'wrap', alignItems: 'center' }}>
            {comment.mentions.map((mention) => (
              <Chip
                key={mention.id}
                size="small"
                avatar={<Avatar src={mention.profilePhoto ?? undefined}>{getCommentInitials(mention.displayName)}</Avatar>}
                label={`@${mention.displayName}`}
                onClick={() => onOpenProfile(mention)}
              />
            ))}
          </Stack>
        ) : null}

        {!comment.isDeleted ? (
          <Stack direction="row" spacing={0.75} sx={{ alignItems: 'center', flexWrap: 'wrap' }}>
            {supportedReactions.map((emoji) => {
              const group = reactionGroups.get(emoji);
              if (!canManage && !group?.count) {
                return null;
              }

              if (!canManage) {
                return <Chip key={emoji} size="small" variant="outlined" label={`${emoji} ${group?.count ?? 0}`} />;
              }

              return (
                <Button
                  key={emoji}
                  size="small"
                  variant={group?.reactedByCurrentUser ? 'contained' : group?.count ? 'outlined' : 'text'}
                  onClick={() => onReact(comment, emoji)}
                  sx={{ minWidth: 42, px: 1, py: 0.25 }}
                >
                  {emoji}{group?.count ? ` ${group.count}` : ''}
                </Button>
              );
            })}
            {comment.counters?.seenCount ? (
              <Chip size="small" variant="outlined" label={`Seen ${comment.counters.seenCount}`} />
            ) : null}
            {comment.readReceipts?.length ? (
              <Typography variant="caption" color="text.secondary">
                {formatReadReceiptSummary(comment.readReceipts.map((read) => read.displayName))}
              </Typography>
            ) : null}
          </Stack>
        ) : null}

        {showActions ? (
          <Stack direction="row" spacing={0.5} className="comment-actions" sx={{ alignItems: 'center', flexWrap: 'wrap' }}>
            {canReply ? <Tooltip title="Reply"><IconButton size="small" onClick={() => onReply(comment)} aria-label="Reply"><Reply size={15} /></IconButton></Tooltip> : null}
            {canEdit ? <Tooltip title="Edit"><IconButton size="small" onClick={() => onEdit(comment)} aria-label="Edit comment"><Pencil size={15} /></IconButton></Tooltip> : null}
            {canDelete ? <Tooltip title="Delete"><IconButton size="small" color="error" onClick={() => onDelete(comment)} aria-label="Delete comment"><Trash2 size={15} /></IconButton></Tooltip> : null}
            {canRestore ? <Tooltip title="Restore"><IconButton size="small" onClick={() => onRestore(comment)} aria-label="Restore comment"><RotateCcw size={15} /></IconButton></Tooltip> : null}
            {canPin ? <Tooltip title={comment.pinned ? 'Unpin' : 'Pin'}><IconButton size="small" onClick={() => onPinToggle(comment)} aria-label={comment.pinned ? 'Unpin comment' : 'Pin comment'}>{comment.pinned ? <PinOff size={15} /> : <Pin size={15} />}</IconButton></Tooltip> : null}
            {canResolve ? <Tooltip title={comment.resolved ? 'Reopen' : 'Resolve'}><IconButton size="small" color={comment.resolved ? 'success' : 'default'} onClick={() => onResolveToggle(comment)} aria-label={comment.resolved ? 'Reopen discussion' : 'Resolve discussion'}><CheckCircle2 size={15} /></IconButton></Tooltip> : null}
            {comment.isEdited ? <Tooltip title="History"><IconButton size="small" onClick={() => setHistoryOpen((open) => !open)} aria-label="View comment history"><History size={15} /></IconButton></Tooltip> : null}
          </Stack>
        ) : null}

        {historyOpen ? (
          <Stack spacing={1} sx={{ borderTop: '1px solid', borderColor: 'divider', pt: 1 }}>
            {historyQuery.isLoading ? <Skeleton height={56} /> : null}
            {historyQuery.data?.items.length === 0 ? <Typography variant="caption" color="text.secondary">No edit history.</Typography> : null}
            {historyQuery.data?.items.map((entry) => (
              <Box key={entry.id} sx={{ bgcolor: 'action.hover', borderRadius: 1, p: 1 }}>
                <Typography variant="caption" color="text.secondary">{entry.editedByName} - {formatDateTime(entry.editedAt)}</Typography>
                <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>{entry.previousMessage}</Typography>
              </Box>
            ))}
          </Stack>
        ) : null}
      </Stack>

      {children.length ? (
        <Button
          size="small"
          startIcon={isCollapsed ? <ChevronRight size={15} /> : <ChevronDown size={15} />}
          onClick={() => onToggleCollapse(comment.id)}
          sx={{ mt: 0.75, ml: 0.5 }}
        >
          {children.length} {children.length === 1 ? 'Reply' : 'Replies'} - {isCollapsed ? 'Expand' : 'Collapse'}
        </Button>
      ) : null}

      {!isCollapsed ? children.map((child, index) => (
        <CommentNode
          key={child.id}
          comment={child}
          commentsById={commentsById}
          childrenByParent={childrenByParent}
          collapsedIds={collapsedIds}
          highlightedCommentId={highlightedCommentId}
          currentUserId={currentUserId}
          workItemId={workItemId}
          depth={depth + 1}
          canManage={canManage}
          canModerate={canModerate}
          setCommentRef={setCommentRef}
          onToggleCollapse={onToggleCollapse}
          onJumpToComment={onJumpToComment}
          compact={shouldCompactComment(children[index - 1] ?? null, child)}
          onReply={onReply}
          onEdit={onEdit}
          onDelete={onDelete}
          onRestore={onRestore}
          onPinToggle={onPinToggle}
          onResolveToggle={onResolveToggle}
          onReact={onReact}
          onOpenProfile={onOpenProfile}
          onDeleteAttachment={onDeleteAttachment}
        />
      )) : null}
    </Box>
  );
}

function CommentAttachmentList({
  attachments,
  canManage,
  onDelete
}: {
  attachments: WorkCommentAttachment[];
  canManage: boolean;
  onDelete: (attachment: WorkCommentAttachment) => void;
}) {
  return (
    <Stack spacing={0.75}>
      {attachments.map((attachment) => (
        <Stack key={attachment.id} spacing={0.75} sx={{ border: '1px solid', borderColor: 'divider', borderRadius: 2, p: 1 }}>
          <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
            <Paperclip size={15} />
            <Link href={attachment.storagePath} target="_blank" rel="noreferrer" underline="hover" sx={{ flex: 1, minWidth: 0, fontWeight: 750 }} noWrap>
              {attachment.fileName}
            </Link>
            <Typography variant="caption" color="text.secondary">{formatFileSize(attachment.fileSizeBytes)}</Typography>
            <IconButton component="a" href={attachment.storagePath} target="_blank" rel="noreferrer" size="small" aria-label="Download comment attachment"><Download size={15} /></IconButton>
            {canManage ? <IconButton size="small" color="error" onClick={() => onDelete(attachment)} aria-label="Delete comment attachment"><Trash2 size={15} /></IconButton> : null}
          </Stack>
          <CommentAttachmentPreview attachment={attachment} />
        </Stack>
      ))}
    </Stack>
  );
}

function CommentAttachmentPreview({ attachment }: { attachment: WorkCommentAttachment }) {
  if (attachment.previewType === 'image') {
    return <Box component="img" src={attachment.storagePath} alt={attachment.fileName} sx={{ maxHeight: 220, maxWidth: '100%', borderRadius: 1, objectFit: 'contain', bgcolor: 'background.default' }} />;
  }

  if (attachment.previewType === 'video') {
    return <Box component="video" src={attachment.storagePath} controls sx={{ maxHeight: 260, width: '100%', borderRadius: 1, bgcolor: 'background.default' }} />;
  }

  if (attachment.previewType === 'pdf') {
    return <Typography variant="caption" color="text.secondary">PDF preview available from the attachment link.</Typography>;
  }

  return null;
}

function MarkdownMessage({ text }: { text: string }) {
  const blocks = text.split('```');

  return (
    <Stack spacing={0.75}>
      {blocks.map((block, index) => {
        if (index % 2 === 1) {
          return (
            <Box key={`${index}-${block.slice(0, 8)}`} component="pre" sx={{ m: 0, p: 1.25, borderRadius: 1, overflowX: 'auto', bgcolor: 'grey.900', color: 'grey.100', fontSize: 13 }}>
              <code>{block.trim()}</code>
            </Box>
          );
        }

        return block.split('\n').map((line, lineIndex) => {
          if (!line.trim()) {
            return <Box key={`${index}-${lineIndex}`} sx={{ height: 8 }} />;
          }

          if (line.startsWith('> ')) {
            return (
              <Typography key={`${index}-${lineIndex}`} variant="body2" sx={{ borderLeft: '3px solid', borderColor: 'divider', pl: 1, color: 'text.secondary' }}>
                {renderInlineMarkdown(line.slice(2))}
              </Typography>
            );
          }

          if (line.startsWith('- [ ] ') || line.startsWith('- [x] ')) {
            return (
              <Stack key={`${index}-${lineIndex}`} direction="row" spacing={0.75} sx={{ alignItems: 'center' }}>
                <Checkbox size="small" checked={line.startsWith('- [x] ')} disabled />
                <Typography variant="body2">{renderInlineMarkdown(line.slice(6))}</Typography>
              </Stack>
            );
          }

          if (line.startsWith('- ')) {
            return <Typography key={`${index}-${lineIndex}`} variant="body2">• {renderInlineMarkdown(line.slice(2))}</Typography>;
          }

          if (/^\d+\.\s/.test(line)) {
            return <Typography key={`${index}-${lineIndex}`} variant="body2">{renderInlineMarkdown(line)}</Typography>;
          }

          return <Typography key={`${index}-${lineIndex}`} variant="body2" sx={{ whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}>{renderInlineMarkdown(line)}</Typography>;
        });
      })}
    </Stack>
  );
}

function renderInlineMarkdown(text: string): ReactNode[] {
  const nodes: ReactNode[] = [];
  const pattern = /(\[([^\]]+)\]\(([^)]+)\)|`([^`]+)`|\*\*([^*]+)\*\*|\*([^*]+)\*|<u>(.*?)<\/u>)/g;
  let lastIndex = 0;
  let match: RegExpExecArray | null;

  while ((match = pattern.exec(text)) !== null) {
    if (match.index > lastIndex) {
      nodes.push(text.slice(lastIndex, match.index));
    }

    if (match[2] && match[3]) {
      nodes.push(<Link key={`${match.index}-link`} href={match[3]} target="_blank" rel="noreferrer">{match[2]}</Link>);
    } else if (match[4]) {
      nodes.push(<Box key={`${match.index}-code`} component="code" sx={{ px: 0.5, borderRadius: 0.5, bgcolor: 'action.hover' }}>{match[4]}</Box>);
    } else if (match[5]) {
      nodes.push(<Box key={`${match.index}-bold`} component="strong">{match[5]}</Box>);
    } else if (match[6]) {
      nodes.push(<Box key={`${match.index}-italic`} component="em">{match[6]}</Box>);
    } else if (match[7]) {
      nodes.push(<Box key={`${match.index}-underline`} component="span" sx={{ textDecoration: 'underline' }}>{match[7]}</Box>);
    }

    lastIndex = pattern.lastIndex;
  }

  if (lastIndex < text.length) {
    nodes.push(text.slice(lastIndex));
  }

  return nodes;
}

function getCommentInitials(name: string) {
  return name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('') || '?';
}

function toPendingCommentFile(file: File): PendingCommentFile {
  return {
    id: typeof crypto !== 'undefined' && 'randomUUID' in crypto ? crypto.randomUUID() : `${Date.now()}-${file.name}`,
    fileName: file.name,
    contentType: file.type || 'application/octet-stream',
    fileSizeBytes: file.size,
    storagePath: URL.createObjectURL(file)
  };
}

function toCommentAttachmentRequest(file: PendingCommentFile) {
  return {
    fileName: file.fileName,
    contentType: file.contentType,
    fileSizeBytes: file.fileSizeBytes,
    storagePath: file.storagePath
  };
}

function formatFileSize(bytes: number) {
  if (bytes < 1024) {
    return `${bytes} B`;
  }

  if (bytes < 1024 * 1024) {
    return `${(bytes / 1024).toFixed(1)} KB`;
  }

  return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}

function AttachmentList({
  attachments,
  loading,
  canManage,
  onDelete
}: {
  attachments: WorkAttachment[];
  loading: boolean;
  canManage: boolean;
  onDelete: (attachment: WorkAttachment) => void;
}) {
  if (loading) {
    return <Stack spacing={1}>{Array.from({ length: 3 }).map((_, index) => <Skeleton key={index} height={64} />)}</Stack>;
  }

  if (!attachments.length) {
    return <EmptyState title="No attachments" />;
  }

  return (
    <Stack spacing={1.25}>
      {attachments.map((attachment) => (
        <Stack key={attachment.id} direction="row" spacing={1.5} sx={{ alignItems: 'center', justifyContent: 'space-between', border: '1px solid', borderColor: 'divider', borderRadius: 2, p: 1.5 }}>
          <Box sx={{ minWidth: 0 }}>
            <Link href={attachment.storagePath} target="_blank" rel="noreferrer" underline="hover" sx={{ fontWeight: 750 }}>{attachment.fileName}</Link>
            <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
              {attachment.contentType} - {attachment.fileSizeBytes.toLocaleString()} bytes - v{attachment.version}
            </Typography>
          </Box>
          <Stack direction="row" spacing={0.5}>
            <IconButton component="a" href={attachment.storagePath} target="_blank" rel="noreferrer" size="small" aria-label="Download attachment"><Download size={16} /></IconButton>
            {canManage ? <IconButton size="small" color="error" onClick={() => onDelete(attachment)} aria-label="Delete attachment"><Trash2 size={16} /></IconButton> : null}
          </Stack>
        </Stack>
      ))}
    </Stack>
  );
}

function LinkList({
  links,
  loading,
  canManage,
  onDelete
}: {
  links: WorkItemLink[];
  loading: boolean;
  canManage: boolean;
  onDelete: (link: WorkItemLink) => void;
}) {
  if (loading) {
    return <Stack spacing={1}>{Array.from({ length: 2 }).map((_, index) => <Skeleton key={index} height={56} />)}</Stack>;
  }

  if (!links.length) {
    return <EmptyState title="No linked work" />;
  }

  return (
    <Stack spacing={1.25}>
      {links.map((link) => (
        <Stack key={link.id} direction="row" spacing={1.5} sx={{ alignItems: 'center', justifyContent: 'space-between', border: '1px solid', borderColor: 'divider', borderRadius: 2, p: 1.5 }}>
          <Box sx={{ minWidth: 0 }}>
            <Typography sx={{ fontWeight: 750 }}>{link.targetTitle}</Typography>
            <Typography variant="caption" color="text.secondary">{formatLinkType(link.linkType)} - {optional(link.description)}</Typography>
          </Box>
          {canManage ? <IconButton size="small" color="error" onClick={() => onDelete(link)} aria-label="Delete link"><Trash2 size={16} /></IconButton> : null}
        </Stack>
      ))}
    </Stack>
  );
}

function formatLinkType(value: WorkItemLinkType) {
  return value
    .replace('BlockedBy', 'Blocked By')
    .replace('RelatesTo', 'Relates To')
    .replace('DependsOn', 'Depends On');
}

function toRequestPlaceholder(): WorkItemRequest {
  return {
    projectId: '',
    parentWorkItemId: null,
    workItemTypeId: '',
    workflowStatusId: null,
    priorityId: '',
    title: '',
    description: null,
    acceptanceCriteria: null,
    assigneeUserProfileId: null,
    reporterUserProfileId: '',
    startDate: null,
    dueDate: null,
    estimatedHours: null,
    loggedHours: 0,
    storyPoints: null,
    sprintId: null,
    labelIds: [],
    componentIds: [],
    watcherUserProfileIds: [],
    environment: null,
    severity: null,
    reproducible: null,
    rowVersion: null
  };
}
