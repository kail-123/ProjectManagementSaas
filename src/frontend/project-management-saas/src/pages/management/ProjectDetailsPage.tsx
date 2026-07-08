import { Box, Button, Checkbox, Divider, IconButton, ListItemText, MenuItem, Paper, Skeleton, Stack, TextField, Tooltip, Typography } from '@mui/material';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, Gauge, Plus, Settings, Trash2, Users } from 'lucide-react';
import { useState } from 'react';
import { Link as RouterLink, Navigate, useParams } from 'react-router-dom';
import { routePaths } from '../../app/router/routePaths';
import { getApiErrorMessage } from '../../shared/api/apiError';
import { usePermissions } from '../../features/auth/model/usePermissions';
import { managementApi } from '../../shared/management/managementApi';
import type { ProjectMemberRole } from '../../shared/management/managementTypes';
import { permissions } from '../../shared/security/permissions';
import { PageHeader } from '../../shared/ui/PageHeader';
import { useToast } from '../../shared/ui/toastContext';
import { enumLabel, formatCurrency, formatDate, optional, priorityChip, projectStatusChip } from './pageUtils';

const projectMemberRoles: ProjectMemberRole[] = ['Developer', 'Qa', 'ProjectManager', 'BusinessAnalyst', 'UiUx', 'Client', 'Viewer'];

export function ProjectDetailsPage() {
  const { projectId } = useParams();
  const { hasPermission } = usePermissions();
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const [selectedUserIds, setSelectedUserIds] = useState<string[]>([]);
  const [selectedRole, setSelectedRole] = useState<ProjectMemberRole>('Developer');
  const canViewDashboard = hasPermission(permissions.projects.dashboardView);
  const canViewMembers = hasPermission(permissions.projectMembers.view);
  const canManageMembers = hasPermission(permissions.projectMembers.manage);
  const canViewUsers = hasPermission(permissions.users.view);
  const canViewSettings = hasPermission(permissions.projectSettings.view);

  const projectQuery = useQuery({
    queryKey: ['projects', projectId],
    queryFn: () => managementApi.projects.get(projectId!),
    enabled: Boolean(projectId)
  });
  const membersQuery = useQuery({
    queryKey: ['projects', projectId, 'members', 'details'],
    queryFn: () => managementApi.projects.members.list(projectId!, { pageNumber: 1, pageSize: 100, sortBy: 'userDisplayName', sortDirection: 'asc' }),
    enabled: Boolean(projectId) && canViewMembers
  });
  const usersQuery = useQuery({
    queryKey: ['users', 'project-member-lookup', projectQuery.data?.organizationId],
    queryFn: () => managementApi.users.list({ pageNumber: 1, pageSize: 200, sortBy: 'name', sortDirection: 'asc' }),
    enabled: Boolean(projectQuery.data?.organizationId) && canManageMembers && canViewUsers
  });

  const invalidateMembers = async () => {
    await queryClient.invalidateQueries({ queryKey: ['projects', projectId, 'members'] });
    await queryClient.invalidateQueries({ queryKey: ['projects', projectId] });
  };

  const addMembersMutation = useMutation({
    mutationFn: () => managementApi.projects.members.add(projectId!, { userIds: selectedUserIds, roleInProject: selectedRole }),
    onSuccess: async () => {
      showToast('Project members added.');
      setSelectedUserIds([]);
      await invalidateMembers();
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to add project members.'), 'error')
  });
  const changeRoleMutation = useMutation({
    mutationFn: ({ userId, roleInProject }: { userId: string; roleInProject: ProjectMemberRole }) =>
      managementApi.projects.members.add(projectId!, { userIds: [userId], roleInProject }),
    onSuccess: async () => {
      showToast('Project role updated.');
      await invalidateMembers();
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to update project role.'), 'error')
  });
  const removeMemberMutation = useMutation({
    mutationFn: (userId: string) => managementApi.projects.members.delete(projectId!, userId),
    onSuccess: async () => {
      showToast('Project member removed.');
      await invalidateMembers();
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to remove project member.'), 'error')
  });

  if (!projectId) {
    return <Navigate to={routePaths.projects} replace />;
  }

  if (projectQuery.isLoading) {
    return <Skeleton variant="rounded" height={360} />;
  }

  if (!projectQuery.data) {
    return <Typography color="error">Project was not found.</Typography>;
  }

  const project = projectQuery.data;
  const memberUserIds = new Set(membersQuery.data?.items.map((member) => member.userId) ?? []);
  const availableUsers = usersQuery.data?.items
    .filter((user) => user.profile?.organizationId === project.organizationId && !memberUserIds.has(user.id))
    ?? [];

  return (
    <Stack spacing={3}>
      <PageHeader
        title={project.name}
        description={project.code}
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.app },
          { label: 'Projects', path: routePaths.projects },
          { label: project.name }
        ]}
        actions={(
          <>
            <Button component={RouterLink} to={routePaths.projects} startIcon={<ArrowLeft size={16} />} variant="outlined">Back</Button>
            {canViewDashboard ? <Button component={RouterLink} to={routePaths.projectDashboard(project.id)} startIcon={<Gauge size={16} />} variant="outlined">Dashboard</Button> : null}
            {canViewMembers ? <Button component={RouterLink} to={routePaths.projectMembers(project.id)} startIcon={<Users size={16} />} variant="outlined">Members</Button> : null}
            {canViewSettings ? <Button component={RouterLink} to={routePaths.projectSettings(project.id)} startIcon={<Settings size={16} />} variant="contained">Settings</Button> : null}
          </>
        )}
      />

      <Paper sx={{ p: 3, border: '1px solid', borderColor: 'divider', borderRadius: 3 }}>
        <Stack spacing={2}>
          <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>
            {projectStatusChip(project.status)}
            {priorityChip(project.priority)}
          </Stack>
          <Typography color="text.secondary">{optional(project.description)}</Typography>
          <Divider />
          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'repeat(3, minmax(0, 1fr))' }, gap: 2 }}>
            <Info label="Organization" value={project.organizationName} />
            <Info label="Client" value={optional(project.clientName)} />
            <Info label="Project Manager" value={optional(project.projectManagerName)} />
            <Info label="Start Date" value={formatDate(project.startDate)} />
            <Info label="End Date" value={formatDate(project.endDate)} />
            <Info label="Estimated Budget" value={formatCurrency(project.estimatedBudget)} />
            <Info label="Visibility" value={enumLabel(project.visibility)} />
            <Info label="Teams" value={project.teamIds.length.toString()} />
            <Info label="Members" value={project.memberCount.toString()} />
            <Info label="Created By" value={optional(project.createdBy)} />
            <Info label="Created On" value={formatDate(project.createdOn?.slice(0, 10))} />
            <Info label="Updated By" value={optional(project.updatedBy)} />
          </Box>
        </Stack>
      </Paper>

      {canViewMembers ? (
        <Paper sx={{ p: 3, border: '1px solid', borderColor: 'divider', borderRadius: 3 }}>
          <Stack spacing={2}>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} sx={{ alignItems: { md: 'center' } }}>
              <Box sx={{ flexGrow: 1 }}>
                <Typography variant="h6">Project Members</Typography>
                <Typography variant="body2" color="text.secondary">{membersQuery.data?.totalCount ?? 0} active members</Typography>
              </Box>
              {canManageMembers ? (
                <>
                  <TextField
                    select
                    size="small"
                    label="Users"
                    value={selectedUserIds}
                    onChange={(event) => setSelectedUserIds(event.target.value as unknown as string[])}
                    sx={{ minWidth: { xs: '100%', md: 300 } }}
                    slotProps={{
                      select: {
                        multiple: true,
                        renderValue: (selected: unknown) => availableUsers
                          .filter((user) => (selected as string[]).includes(user.id))
                          .map((user) => user.displayName ?? user.email)
                          .join(', ')
                      }
                    }}
                  >
                    {availableUsers.map((user) => (
                      <MenuItem key={user.id} value={user.id}>
                        <Checkbox checked={selectedUserIds.includes(user.id)} />
                        <ListItemText primary={user.displayName ?? user.email} secondary={user.profile?.employeeCode} />
                      </MenuItem>
                    ))}
                  </TextField>
                  <TextField
                    select
                    size="small"
                    label="Role"
                    value={selectedRole}
                    onChange={(event) => setSelectedRole(event.target.value as ProjectMemberRole)}
                    sx={{ minWidth: 180 }}
                  >
                    {projectMemberRoles.map((role) => (
                      <MenuItem key={role} value={role}>{enumLabel(role)}</MenuItem>
                    ))}
                  </TextField>
                  <Button
                    variant="contained"
                    startIcon={<Plus size={16} />}
                    disabled={selectedUserIds.length === 0 || addMembersMutation.isPending}
                    onClick={() => addMembersMutation.mutate()}
                  >
                    Add
                  </Button>
                </>
              ) : null}
            </Stack>
            <Divider />
            <Stack spacing={1}>
              {(membersQuery.data?.items ?? []).map((member) => (
                <Box
                  key={member.userId}
                  sx={{
                    display: 'grid',
                    gridTemplateColumns: { xs: '1fr', md: 'minmax(0, 1fr) 220px 40px' },
                    gap: 1.5,
                    alignItems: 'center',
                    py: 1
                  }}
                >
                  <Box>
                    <Typography sx={{ fontWeight: 760 }}>{member.userDisplayName}</Typography>
                    <Typography variant="body2" color="text.secondary">{member.employeeCode || optional(member.designation)}</Typography>
                  </Box>
                  <TextField
                    select
                    size="small"
                    label="Project Role"
                    value={member.roleInProject}
                    disabled={!canManageMembers || changeRoleMutation.isPending}
                    onChange={(event) => changeRoleMutation.mutate({ userId: member.userId, roleInProject: event.target.value as ProjectMemberRole })}
                  >
                    {projectMemberRoles.map((role) => (
                      <MenuItem key={role} value={role}>{enumLabel(role)}</MenuItem>
                    ))}
                  </TextField>
                  {canManageMembers ? (
                    <Tooltip title="Remove member">
                      <IconButton color="error" onClick={() => removeMemberMutation.mutate(member.userId)} disabled={removeMemberMutation.isPending}>
                        <Trash2 size={18} />
                      </IconButton>
                    </Tooltip>
                  ) : null}
                </Box>
              ))}
              {!membersQuery.isLoading && (membersQuery.data?.items.length ?? 0) === 0 ? (
                <Typography color="text.secondary">No members assigned.</Typography>
              ) : null}
            </Stack>
          </Stack>
        </Paper>
      ) : null}
    </Stack>
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
