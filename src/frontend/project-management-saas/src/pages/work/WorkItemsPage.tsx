import {
  Box,
  Button,
  Chip,
  Drawer,
  IconButton,
  MenuItem,
  Stack,
  TextField,
  Typography
} from '@mui/material';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Eye, X } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import { routePaths } from '../../app/router/routePaths';
import { getApiErrorMessage } from '../../shared/api/apiError';
import { usePermissions } from '../../features/auth/model/usePermissions';
import { managementApi } from '../../shared/management/managementApi';
import type { Project, UserProfile, WorkItem, WorkItemRequest } from '../../shared/management/managementTypes';
import { permissions } from '../../shared/security/permissions';
import { DataTable } from '../../shared/ui/DataTable';
import type { DataTableColumn, DataTableQuery } from '../../shared/ui/DataTable';
import { FormDialog } from '../../shared/ui/FormDialog';
import { PageHeader } from '../../shared/ui/PageHeader';
import { useToast } from '../../shared/ui/toastContext';
import { formatDate, optional } from '../management/pageUtils';
import { WorkItemForm } from './workItemForm';
import { buildDefaultRequest, emptyWorkItem, toRequest } from './workItemFormModel';

export function WorkItemsPage() {
  const [query, setQuery] = useState<DataTableQuery>({ pageNumber: 1, pageSize: 25, sortBy: 'createdOn', sortDirection: 'desc' });
  const [formOpen, setFormOpen] = useState(false);
  const [editingWorkItem, setEditingWorkItem] = useState<WorkItem | null>(null);
  const [selectedWorkItem, setSelectedWorkItem] = useState<WorkItem | null>(null);
  const [form, setForm] = useState<WorkItemRequest>(emptyWorkItem);
  const [filters, setFilters] = useState({
    projectId: '',
    workItemTypeId: '',
    workflowStatusId: '',
    priorityId: '',
    assigneeUserProfileId: ''
  });
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const { hasPermission } = usePermissions();
  const canCreate = hasPermission(permissions.workItems.create);
  const canUpdate = hasPermission(permissions.workItems.update);
  const canDelete = hasPermission(permissions.workItems.delete);
  const canExport = hasPermission(permissions.workItems.export);
  const canViewProjects = hasPermission(permissions.projects.view);
  const canViewUsers = hasPermission(permissions.users.view);

  const metadataQuery = useQuery({ queryKey: ['work', 'metadata'], queryFn: () => managementApi.work.metadata() });
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
  const workItemsQuery = useQuery({
    queryKey: ['work-items', query, filters],
    queryFn: () => managementApi.work.items.list(query, {
      projectId: filters.projectId || null,
      workItemTypeId: filters.workItemTypeId || null,
      workflowStatusId: filters.workflowStatusId || null,
      priorityId: filters.priorityId || null,
      assigneeUserProfileId: filters.assigneeUserProfileId || null
    })
  });

  const projects = projectsQuery.data?.items ?? [];
  const userProfiles = useMemo(() => usersQuery.data?.items.filter((user) => user.profile).map((user) => user.profile!) ?? [], [usersQuery.data?.items]);
  const metadata = metadataQuery.data;

  const columns: DataTableColumn<WorkItem>[] = [
    {
      field: 'title',
      headerName: 'Work Item',
      minWidth: 260,
      render: (row) => (
        <Button color="inherit" component={RouterLink} to={routePaths.workItemDetails(row.id)} sx={{ justifyContent: 'flex-start', px: 0 }}>
          {row.title}
        </Button>
      ),
      exportValue: (row) => row.title
    },
    { field: 'projectName', headerName: 'Project', minWidth: 180, render: (row) => row.projectName, exportValue: (row) => row.projectName },
    { field: 'type', headerName: 'Type', minWidth: 120, render: (row) => colorChip(row.typeName, row.typeColor), exportValue: (row) => row.typeName },
    { field: 'status', headerName: 'Status', minWidth: 140, render: (row) => colorChip(row.statusName, row.statusColor), exportValue: (row) => row.statusName },
    { field: 'priority', headerName: 'Priority', minWidth: 130, render: (row) => colorChip(row.priorityName, row.priorityColor), exportValue: (row) => row.priorityName },
    { field: 'assignee', headerName: 'Assignee', minWidth: 160, render: (row) => optional(row.assigneeName), exportValue: (row) => row.assigneeName },
    { field: 'reporter', headerName: 'Reporter', minWidth: 160, render: (row) => row.reporterName, exportValue: (row) => row.reporterName },
    { field: 'dueDate', headerName: 'Due', minWidth: 120, render: (row) => formatDate(row.dueDate), exportValue: (row) => row.dueDate },
    { field: 'storyPoints', headerName: 'Points', minWidth: 90, sortable: false, render: (row) => row.storyPoints ?? '-', exportValue: (row) => row.storyPoints },
    { field: 'comments', headerName: 'Comments', minWidth: 110, sortable: false, render: (row) => row.commentCount, exportValue: (row) => row.commentCount }
  ];

  const saveMutation = useMutation({
    mutationFn: () => editingWorkItem ? managementApi.work.items.update(editingWorkItem.id, form) : managementApi.work.items.create(form),
    onSuccess: async (saved) => {
      showToast('Work item saved.');
      setFormOpen(false);
      setSelectedWorkItem(saved);
      await queryClient.invalidateQueries({ queryKey: ['work-items'] });
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to save work item.'), 'error')
  });

  const deleteMutation = useMutation({
    mutationFn: (workItem: WorkItem) => managementApi.work.items.delete(workItem.id),
    onSuccess: async () => {
      showToast('Work item deleted.');
      setSelectedWorkItem(null);
      await queryClient.invalidateQueries({ queryKey: ['work-items'] });
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to delete work item.'), 'error')
  });

  const bulkDeleteMutation = useMutation({
    mutationFn: (ids: string[]) => managementApi.work.items.bulkDelete(ids),
    onSuccess: async () => {
      showToast('Work items deleted.');
      await queryClient.invalidateQueries({ queryKey: ['work-items'] });
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to delete selected work items.'), 'error')
  });

  const openCreate = () => {
    setEditingWorkItem(null);
    setForm(buildDefaultRequest(projects, userProfiles, metadata));
    setFormOpen(true);
  };

  const openEdit = (workItem: WorkItem) => {
    setEditingWorkItem(workItem);
    setForm(toRequest(workItem));
    setFormOpen(true);
  };

  const updateFilters = (nextFilters: typeof filters) => {
    setFilters(nextFilters);
    setQuery((current) => ({ ...current, pageNumber: 1 }));
  };

  useEffect(() => {
    const handler = (event: KeyboardEvent) => {
      if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'n') {
        event.preventDefault();
        if (canCreate) {
          openCreate();
        }
      }

      if (event.key === 'Escape') {
        setSelectedWorkItem(null);
      }
    };

    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  });

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Work Items"
        description="Tasks, bugs, stories, epics, support requests, and change work across active projects."
      />
      <WorkItemFilters
        filters={filters}
        setFilters={updateFilters}
        projects={projects}
        userProfiles={userProfiles}
        metadata={metadata}
      />
      <DataTable
        title="Work Items"
        rows={workItemsQuery.data?.items ?? []}
        totalCount={workItemsQuery.data?.totalCount ?? 0}
        columns={columns}
        query={query}
        loading={workItemsQuery.isLoading}
        exportFileName="work-items"
        getRowLabel={(row) => row.title}
        onQueryChange={setQuery}
        onCreate={canCreate ? openCreate : undefined}
        onEdit={canUpdate ? openEdit : undefined}
        onDelete={canDelete ? (row) => deleteMutation.mutate(row) : undefined}
        onBulkDelete={canDelete ? (ids) => bulkDeleteMutation.mutate(ids) : undefined}
        canExport={canExport}
        error={workItemsQuery.isError}
        onRetry={() => void workItemsQuery.refetch()}
        renderRowActions={(row) => (
          <IconButton size="small" onClick={() => setSelectedWorkItem(row)} aria-label="View work item">
            <Eye size={17} />
          </IconButton>
        )}
      />

      <FormDialog
        open={formOpen}
        title={editingWorkItem ? 'Edit Work Item' : 'Create Work Item'}
        loading={saveMutation.isPending}
        onClose={() => setFormOpen(false)}
        onSubmit={() => saveMutation.mutate()}
      >
        <WorkItemForm
          form={form}
          setForm={setForm}
          projects={projects}
          userProfiles={userProfiles}
          metadata={metadata}
        />
      </FormDialog>

      <Drawer anchor="right" open={selectedWorkItem !== null} onClose={() => setSelectedWorkItem(null)} slotProps={{ paper: { sx: { width: { xs: '100%', sm: 520 }, p: 3 } } }}>
        {selectedWorkItem ? (
          <Stack spacing={2.5}>
            <Stack direction="row" sx={{ alignItems: 'flex-start', justifyContent: 'space-between', gap: 2 }}>
              <Box>
                <Typography variant="overline" color="primary.main">{selectedWorkItem.projectName}</Typography>
                <Typography variant="h5">{selectedWorkItem.title}</Typography>
              </Box>
              <IconButton onClick={() => setSelectedWorkItem(null)}><X size={18} /></IconButton>
            </Stack>
            <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>
              {colorChip(selectedWorkItem.typeName, selectedWorkItem.typeColor)}
              {colorChip(selectedWorkItem.statusName, selectedWorkItem.statusColor)}
              {colorChip(selectedWorkItem.priorityName, selectedWorkItem.priorityColor)}
            </Stack>
            <Info label="Assignee" value={optional(selectedWorkItem.assigneeName)} />
            <Info label="Reporter" value={selectedWorkItem.reporterName} />
            <Info label="Due Date" value={formatDate(selectedWorkItem.dueDate)} />
            <Info label="Watchers" value={selectedWorkItem.watchers.map((watcher) => watcher.displayName).join(', ') || '-'} />
            <Info label="Description" value={optional(selectedWorkItem.description)} multiline />
            <Stack direction="row" spacing={1}>
              <Button component={RouterLink} to={routePaths.workItemDetails(selectedWorkItem.id)} variant="contained">Open Details</Button>
              {canUpdate ? <Button variant="outlined" onClick={() => openEdit(selectedWorkItem)}>Edit</Button> : null}
            </Stack>
          </Stack>
        ) : null}
      </Drawer>
    </Stack>
  );
}

function WorkItemFilters({
  filters,
  setFilters,
  projects,
  userProfiles,
  metadata
}: {
  filters: {
    projectId: string;
    workItemTypeId: string;
    workflowStatusId: string;
    priorityId: string;
    assigneeUserProfileId: string;
  };
  setFilters: (filters: {
    projectId: string;
    workItemTypeId: string;
    workflowStatusId: string;
    priorityId: string;
    assigneeUserProfileId: string;
  }) => void;
  projects: Project[];
  userProfiles: UserProfile[];
  metadata?: Awaited<ReturnType<typeof managementApi.work.metadata>>;
}) {
  const selectedType = metadata?.types.find((type) => type.id === filters.workItemTypeId);
  const statuses = selectedType
    ? metadata?.workflows.find((workflow) => workflow.id === selectedType.workflowId)?.statuses ?? []
    : metadata?.workflows.flatMap((workflow) => workflow.statuses) ?? [];

  return (
    <Box
      sx={{
        display: 'grid',
        gridTemplateColumns: { xs: '1fr', md: 'repeat(5, minmax(0, 1fr)) auto' },
        gap: 1.5,
        alignItems: 'center'
      }}
    >
      <TextField select size="small" label="Project" value={filters.projectId} onChange={(event) => setFilters({ ...filters, projectId: event.target.value })}>
        <MenuItem value="">All Projects</MenuItem>
        {projects.map((project) => <MenuItem key={project.id} value={project.id}>{project.name}</MenuItem>)}
      </TextField>
      <TextField select size="small" label="Type" value={filters.workItemTypeId} onChange={(event) => setFilters({ ...filters, workItemTypeId: event.target.value, workflowStatusId: '' })}>
        <MenuItem value="">All Types</MenuItem>
        {metadata?.types.map((type) => <MenuItem key={type.id} value={type.id}>{type.name}</MenuItem>)}
      </TextField>
      <TextField select size="small" label="Status" value={filters.workflowStatusId} onChange={(event) => setFilters({ ...filters, workflowStatusId: event.target.value })}>
        <MenuItem value="">All Statuses</MenuItem>
        {statuses.map((status) => <MenuItem key={status.id} value={status.id}>{status.name}</MenuItem>)}
      </TextField>
      <TextField select size="small" label="Priority" value={filters.priorityId} onChange={(event) => setFilters({ ...filters, priorityId: event.target.value })}>
        <MenuItem value="">All Priorities</MenuItem>
        {metadata?.priorities.map((priority) => <MenuItem key={priority.id} value={priority.id}>{priority.name}</MenuItem>)}
      </TextField>
      <TextField select size="small" label="Assignee" value={filters.assigneeUserProfileId} onChange={(event) => setFilters({ ...filters, assigneeUserProfileId: event.target.value })}>
        <MenuItem value="">Any Assignee</MenuItem>
        {userProfiles.map((profile) => <MenuItem key={profile.id} value={profile.id}>{profile.firstName} {profile.lastName}</MenuItem>)}
      </TextField>
      <Button
        variant="outlined"
        onClick={() => setFilters({ projectId: '', workItemTypeId: '', workflowStatusId: '', priorityId: '', assigneeUserProfileId: '' })}
      >
        Clear
      </Button>
    </Box>
  );
}

function colorChip(label: string, color: string) {
  return <Chip size="small" label={label} sx={{ bgcolor: color, color: '#fff', fontWeight: 760 }} />;
}

function Info({ label, value, multiline = false }: { label: string; value: string; multiline?: boolean }) {
  return (
    <Box>
      <Typography variant="caption" color="text.secondary">{label}</Typography>
      <Typography sx={{ fontWeight: 700, whiteSpace: multiline ? 'pre-wrap' : 'normal' }}>{value}</Typography>
    </Box>
  );
}
