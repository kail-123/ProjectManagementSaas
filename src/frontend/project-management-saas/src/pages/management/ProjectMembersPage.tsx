import { Button, Checkbox, Chip, ListItemText, MenuItem, Stack, TextField } from '@mui/material';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft } from 'lucide-react';
import { useState } from 'react';
import { Link as RouterLink, Navigate, useParams } from 'react-router-dom';
import { routePaths } from '../../app/router/routePaths';
import { getApiErrorMessage } from '../../shared/api/apiError';
import { usePermissions } from '../../features/auth/model/usePermissions';
import { managementApi } from '../../shared/management/managementApi';
import type { ProjectMember, ProjectMemberRequest, ProjectMemberRole } from '../../shared/management/managementTypes';
import { permissions } from '../../shared/security/permissions';
import { DataTable } from '../../shared/ui/DataTable';
import type { DataTableColumn, DataTableQuery } from '../../shared/ui/DataTable';
import { FormDialog } from '../../shared/ui/FormDialog';
import { PageHeader } from '../../shared/ui/PageHeader';
import { useToast } from '../../shared/ui/toastContext';
import { enumLabel, formatDateTime, optional } from './pageUtils';

const projectMemberRoles: ProjectMemberRole[] = ['Developer', 'Qa', 'ProjectManager', 'BusinessAnalyst', 'UiUx', 'Client', 'Viewer'];

const emptyMember: ProjectMemberRequest = {
  userIds: [],
  roleInProject: 'Developer'
};

const columns: DataTableColumn<ProjectMember>[] = [
  { field: 'userDisplayName', headerName: 'Member', minWidth: 180, render: (row) => row.userDisplayName, exportValue: (row) => row.userDisplayName },
  { field: 'employeeCode', headerName: 'Employee Code', minWidth: 130, render: (row) => row.employeeCode, exportValue: (row) => row.employeeCode },
  { field: 'designation', headerName: 'Designation', minWidth: 160, render: (row) => optional(row.designation), exportValue: (row) => row.designation },
  {
    field: 'roleInProject',
    headerName: 'Project Role',
    minWidth: 150,
    render: (row) => <Chip size="small" label={enumLabel(row.roleInProject)} />,
    exportValue: (row) => enumLabel(row.roleInProject)
  },
  { field: 'joinedDate', headerName: 'Joined On', minWidth: 170, render: (row) => formatDateTime(row.joinedDate), exportValue: (row) => row.joinedDate },
  { field: 'addedBy', headerName: 'Added By', minWidth: 160, sortable: false, render: (row) => optional(row.addedBy), exportValue: (row) => row.addedBy }
];

export function ProjectMembersPage() {
  const { projectId } = useParams();
  const [query, setQuery] = useState<DataTableQuery>({ pageNumber: 1, pageSize: 25, sortDirection: 'asc' });
  const [formOpen, setFormOpen] = useState(false);
  const [editingMember, setEditingMember] = useState<ProjectMember | null>(null);
  const [form, setForm] = useState<ProjectMemberRequest>(emptyMember);
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const { hasPermission } = usePermissions();
  const canManageMembers = hasPermission(permissions.projectMembers.manage);
  const canExportMembers = hasPermission(permissions.projectMembers.export);
  const canViewUsers = hasPermission(permissions.users.view);
  const canLoadUserLookup = canViewUsers || canManageMembers;

  const projectQuery = useQuery({
    queryKey: ['projects', projectId],
    queryFn: () => managementApi.projects.get(projectId!),
    enabled: Boolean(projectId)
  });
  const membersQuery = useQuery({
    queryKey: ['projects', projectId, 'members', query],
    queryFn: () => managementApi.projects.members.list(projectId!, query),
    enabled: Boolean(projectId)
  });
  const usersQuery = useQuery({
    queryKey: ['users', 'lookup'],
    queryFn: () => managementApi.users.list({ pageNumber: 1, pageSize: 1000, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canLoadUserLookup
  });

  const invalidate = async () => {
    if (!projectId) {
      return;
    }

    await queryClient.invalidateQueries({ queryKey: ['projects', projectId, 'members'] });
    await queryClient.invalidateQueries({ queryKey: ['projects', projectId] });
  };

  const saveMutation = useMutation({
    mutationFn: () => managementApi.projects.members.add(projectId!, form),
    onSuccess: async () => {
      showToast('Project member saved.');
      setFormOpen(false);
      await invalidate();
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to save project member.'), 'error')
  });

  const deleteMutation = useMutation({
    mutationFn: (member: ProjectMember) => managementApi.projects.members.delete(projectId!, member.userId),
    onSuccess: async () => {
      showToast('Project member removed.');
      await invalidate();
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to remove project member.'), 'error')
  });

  const bulkDeleteMutation = useMutation({
    mutationFn: (ids: string[]) => managementApi.projects.members.bulkDelete(projectId!, ids),
    onSuccess: async () => {
      showToast('Project members removed.');
      await invalidate();
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to remove selected project members.'), 'error')
  });

  const openCreate = () => {
    setEditingMember(null);
    setForm(emptyMember);
    setFormOpen(true);
  };

  const openEdit = (member: ProjectMember) => {
    setEditingMember(member);
    setForm({ userIds: [member.userId], roleInProject: member.roleInProject });
    setFormOpen(true);
  };

  const project = projectQuery.data;
  const users = usersQuery.data?.items
    .filter((user) => user.isActive && (!project?.organizationId || user.profile?.organizationId === project.organizationId))
    ?? [];

  if (!projectId) {
    return <Navigate to={routePaths.projects} replace />;
  }

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Project Members"
        description={projectQuery.isLoading ? undefined : project?.name}
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.app },
          { label: 'Projects', path: routePaths.projects },
          { label: project?.name ?? 'Project', path: routePaths.projectDetails(projectId) },
          { label: 'Members' }
        ]}
        actions={<Button component={RouterLink} to={routePaths.projectDetails(projectId)} startIcon={<ArrowLeft size={16} />} variant="outlined">Project Details</Button>}
      />

      <DataTable
        title="Project Members"
        rows={membersQuery.data?.items ?? []}
        totalCount={membersQuery.data?.totalCount ?? 0}
        columns={columns}
        query={query}
        loading={membersQuery.isLoading}
        exportFileName="project-members"
        getRowLabel={(row) => row.userDisplayName}
        onQueryChange={setQuery}
        onCreate={canManageMembers ? openCreate : undefined}
        onEdit={canManageMembers ? openEdit : undefined}
        onDelete={canManageMembers ? (member) => deleteMutation.mutate(member) : undefined}
        onBulkDelete={canManageMembers ? (ids) => bulkDeleteMutation.mutate(ids) : undefined}
        canExport={canExportMembers}
        error={membersQuery.isError}
        onRetry={() => void membersQuery.refetch()}
      />

      <FormDialog
        open={formOpen}
        title={editingMember ? 'Edit Project Member' : 'Add Project Member'}
        loading={saveMutation.isPending}
        onClose={() => setFormOpen(false)}
        onSubmit={() => saveMutation.mutate()}
      >
        <Stack spacing={2}>
          <TextField
            select
            label="User"
            required
            disabled={Boolean(editingMember)}
            value={form.userIds}
            onChange={(event) => setForm({ ...form, userIds: event.target.value as unknown as string[] })}
            slotProps={{
              select: {
                multiple: true,
                renderValue: (selected: unknown) => users
                  .filter((user) => (selected as string[]).includes(user.id))
                  .map((user) => user.displayName ?? user.email)
                  .join(', ')
              }
            }}
          >
            {users.map((user) => (
              <MenuItem key={user.id} value={user.id}>
                <Checkbox checked={form.userIds.includes(user.id)} />
                <ListItemText primary={user.displayName ?? user.email} secondary={user.profile?.employeeCode} />
              </MenuItem>
            ))}
          </TextField>
          <TextField
            select
            label="Role Inside Project"
            required
            value={form.roleInProject}
            onChange={(event) => setForm({ ...form, roleInProject: event.target.value as ProjectMemberRole })}
          >
            {projectMemberRoles.map((role) => (
              <MenuItem key={role} value={role}>{enumLabel(role)}</MenuItem>
            ))}
          </TextField>
        </Stack>
      </FormDialog>
    </Stack>
  );
}
