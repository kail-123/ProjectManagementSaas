import { Checkbox, IconButton, ListItemText, MenuItem, Stack, Switch, TextField, Tooltip, FormControlLabel } from '@mui/material';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { KeyRound, ToggleLeft, ToggleRight } from 'lucide-react';
import { useState } from 'react';
import { managementApi } from '../../shared/management/managementApi';
import type { CreateUserRequest, User } from '../../shared/management/managementTypes';
import { getApiErrorMessage } from '../../shared/api/apiError';
import { DataTable } from '../../shared/ui/DataTable';
import type { DataTableColumn, DataTableQuery } from '../../shared/ui/DataTable';
import { FormDialog } from '../../shared/ui/FormDialog';
import { PageHeader } from '../../shared/ui/PageHeader';
import { permissions } from '../../shared/security/permissions';
import { useToast } from '../../shared/ui/toastContext';
import { usePermissions } from '../../features/auth/model/usePermissions';
import { optional, statusChip } from './pageUtils';

interface UserForm extends CreateUserRequest {
  id?: string;
}

const emptyUser: UserForm = {
  email: '',
  password: '',
  firstName: '',
  lastName: '',
  employeeCode: '',
  designation: null,
  organizationId: null,
  departmentId: null,
  teamId: null,
  profilePhoto: null,
  phone: null,
  timeZone: 'Asia/Calcutta',
  skills: null,
  joiningDate: null,
  isActive: true,
  roles: []
};

const columns: DataTableColumn<User>[] = [
  { field: 'name', headerName: 'Name', minWidth: 180, render: (row) => row.displayName ?? '-', exportValue: (row) => row.displayName },
  { field: 'email', headerName: 'Email', minWidth: 220, render: (row) => row.email, exportValue: (row) => row.email },
  { field: 'employeeCode', headerName: 'Employee Code', minWidth: 140, render: (row) => optional(row.profile?.employeeCode), exportValue: (row) => row.profile?.employeeCode },
  { field: 'department', headerName: 'Department', minWidth: 160, render: (row) => optional(row.profile?.departmentName), exportValue: (row) => row.profile?.departmentName },
  { field: 'team', headerName: 'Team', minWidth: 160, render: (row) => optional(row.profile?.teamName), exportValue: (row) => row.profile?.teamName },
  { field: 'roles', headerName: 'Roles', minWidth: 180, sortable: false, render: (row) => row.roles.join(', ') || '-', exportValue: (row) => row.roles.join(', ') },
  { field: 'isActive', headerName: 'Status', minWidth: 100, render: (row) => statusChip(row.isActive), exportValue: (row) => row.isActive ? 'Active' : 'Inactive' }
];

export function UsersPage() {
  const [query, setQuery] = useState<DataTableQuery>({ pageNumber: 1, pageSize: 25, sortBy: 'name', sortDirection: 'asc' });
  const [formOpen, setFormOpen] = useState(false);
  const [form, setForm] = useState<UserForm>(emptyUser);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [resetUser, setResetUser] = useState<User | null>(null);
  const [newPassword, setNewPassword] = useState('');
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const { hasPermission } = usePermissions();
  const canCreate = hasPermission(permissions.users.create) && hasPermission(permissions.users.assignRoles);
  const canUpdate = hasPermission(permissions.users.update);
  const canDelete = hasPermission(permissions.users.delete);
  const canExport = hasPermission(permissions.users.export);
  const canManageStatus = hasPermission(permissions.users.manageStatus);
  const canResetPassword = hasPermission(permissions.users.resetPassword);
  const canAssignRoles = hasPermission(permissions.users.assignRoles);
  const canViewOrganizations = hasPermission(permissions.organizations.view);
  const canViewDepartments = hasPermission(permissions.departments.view);
  const canViewTeams = hasPermission(permissions.teams.view);
  const canViewRoles = hasPermission(permissions.roles.view);

  const usersQuery = useQuery({ queryKey: ['users', query], queryFn: () => managementApi.users.list(query) });
  const organizationsQuery = useQuery({
    queryKey: ['organizations', 'lookup'],
    queryFn: () => managementApi.organizations.list({ pageNumber: 1, pageSize: 200, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canViewOrganizations
  });
  const departmentsQuery = useQuery({
    queryKey: ['departments', 'lookup'],
    queryFn: () => managementApi.departments.list({ pageNumber: 1, pageSize: 200, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canViewDepartments
  });
  const teamsQuery = useQuery({
    queryKey: ['teams', 'lookup'],
    queryFn: () => managementApi.teams.list({ pageNumber: 1, pageSize: 200, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canViewTeams
  });
  const rolesQuery = useQuery({
    queryKey: ['roles', 'lookup'],
    queryFn: () => managementApi.roles.list({ pageNumber: 1, pageSize: 200, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canAssignRoles && canViewRoles
  });

  const invalidate = async () => queryClient.invalidateQueries({ queryKey: ['users'] });

  const saveMutation = useMutation({
    mutationFn: async () => {
      if (editingUser) {
        const request = {
          email: form.email,
          firstName: form.firstName,
          lastName: form.lastName,
          employeeCode: form.employeeCode,
          designation: form.designation,
          organizationId: form.organizationId,
          departmentId: form.departmentId,
          teamId: form.teamId,
          profilePhoto: form.profilePhoto,
          phone: form.phone,
          timeZone: form.timeZone,
          skills: form.skills,
          joiningDate: form.joiningDate,
          isActive: form.isActive,
          roles: form.roles
        };
        const user = await managementApi.users.update(editingUser.id, request);
        if (canAssignRoles) {
          await managementApi.users.assignRoles(editingUser.id, form.roles);
        }

        return user;
      }
      return managementApi.users.create(form);
    },
    onSuccess: async () => {
      showToast('User saved.');
      setFormOpen(false);
      await invalidate();
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to save user.'), 'error')
  });

  const activeMutation = useMutation({
    mutationFn: (user: User) => managementApi.users.setActiveStatus(user.id, !user.isActive),
    onSuccess: async () => {
      showToast('User status updated.');
      await invalidate();
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to update user status.'), 'error')
  });

  const deleteMutation = useMutation({
    mutationFn: (user: User) => managementApi.users.delete(user.id),
    onSuccess: async () => {
      showToast('User deleted.');
      await invalidate();
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to delete user.'), 'error')
  });

  const bulkDeleteMutation = useMutation({
    mutationFn: (ids: string[]) => managementApi.users.bulkDelete(ids),
    onSuccess: async () => {
      showToast('Users deleted.');
      await invalidate();
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to delete selected users.'), 'error')
  });

  const resetMutation = useMutation({
    mutationFn: () => managementApi.users.resetPassword(resetUser!.id, newPassword),
    onSuccess: () => {
      showToast('Password reset.');
      setResetUser(null);
      setNewPassword('');
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to reset password.'), 'error')
  });

  const openCreate = () => {
    setEditingUser(null);
    setForm(emptyUser);
    setFormOpen(true);
  };

  const openEdit = (user: User) => {
    setEditingUser(user);
    setForm({
      id: user.id,
      email: user.email,
      password: '',
      firstName: user.profile?.firstName ?? '',
      lastName: user.profile?.lastName ?? '',
      employeeCode: user.profile?.employeeCode ?? '',
      designation: user.profile?.designation ?? null,
      organizationId: user.profile?.organizationId ?? null,
      departmentId: user.profile?.departmentId ?? null,
      teamId: user.profile?.teamId ?? null,
      profilePhoto: user.profile?.profilePhoto ?? null,
      phone: user.profile?.phone ?? null,
      timeZone: user.profile?.timeZone ?? 'Asia/Calcutta',
      skills: user.profile?.skills ?? null,
      joiningDate: user.profile?.joiningDate ?? null,
      isActive: user.isActive,
      roles: user.roles
    });
    setFormOpen(true);
  };

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Users"
        description="Workspace identities, profile assignments, roles, and account status."
      />
      <DataTable
        title="Users"
        rows={usersQuery.data?.items ?? []}
        totalCount={usersQuery.data?.totalCount ?? 0}
        columns={columns}
        query={query}
        loading={usersQuery.isLoading}
        exportFileName="users"
        getRowLabel={(row) => row.email}
        onQueryChange={setQuery}
        onCreate={canCreate ? openCreate : undefined}
        onEdit={canUpdate ? openEdit : undefined}
        onDelete={canDelete ? (row) => deleteMutation.mutate(row) : undefined}
        onBulkDelete={canDelete ? (ids) => bulkDeleteMutation.mutate(ids) : undefined}
        canExport={canExport}
        error={usersQuery.isError}
        onRetry={() => void usersQuery.refetch()}
        renderRowActions={canManageStatus || canResetPassword ? (row) => (
          <>
            {canManageStatus ? (
              <Tooltip title={row.isActive ? 'Deactivate' : 'Activate'}>
                <IconButton size="small" onClick={() => activeMutation.mutate(row)}>
                  {row.isActive ? <ToggleLeft size={18} /> : <ToggleRight size={18} />}
                </IconButton>
              </Tooltip>
            ) : null}
            {canResetPassword ? (
              <Tooltip title="Reset Password">
                <IconButton size="small" onClick={() => setResetUser(row)}>
                  <KeyRound size={18} />
                </IconButton>
              </Tooltip>
            ) : null}
          </>
        ) : undefined}
      />
      <FormDialog open={formOpen} title={editingUser ? 'Edit User' : 'Create User'} loading={saveMutation.isPending} onClose={() => setFormOpen(false)} onSubmit={() => saveMutation.mutate()}>
        <Stack spacing={2}>
          <TextField label="Email" required type="email" value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} />
          {!editingUser ? <TextField label="Password" required type="password" value={form.password} onChange={(event) => setForm({ ...form, password: event.target.value })} /> : null}
          <TextField label="First Name" required value={form.firstName} onChange={(event) => setForm({ ...form, firstName: event.target.value })} />
          <TextField label="Last Name" required value={form.lastName} onChange={(event) => setForm({ ...form, lastName: event.target.value })} />
          <TextField label="Employee Code" required value={form.employeeCode} onChange={(event) => setForm({ ...form, employeeCode: event.target.value })} />
          <TextField label="Designation" value={form.designation ?? ''} onChange={(event) => setForm({ ...form, designation: event.target.value || null })} />
          <TextField select label="Organization" value={form.organizationId ?? ''} onChange={(event) => setForm({ ...form, organizationId: event.target.value || null })}>
            <MenuItem value="">None</MenuItem>
            {organizationsQuery.data?.items.map((organization) => <MenuItem key={organization.id} value={organization.id}>{organization.name}</MenuItem>)}
          </TextField>
          <TextField select label="Department" value={form.departmentId ?? ''} onChange={(event) => setForm({ ...form, departmentId: event.target.value || null })}>
            <MenuItem value="">None</MenuItem>
            {departmentsQuery.data?.items.map((department) => <MenuItem key={department.id} value={department.id}>{department.name}</MenuItem>)}
          </TextField>
          <TextField select label="Team" value={form.teamId ?? ''} onChange={(event) => setForm({ ...form, teamId: event.target.value || null })}>
            <MenuItem value="">None</MenuItem>
            {teamsQuery.data?.items.map((team) => <MenuItem key={team.id} value={team.id}>{team.name}</MenuItem>)}
          </TextField>
          {canAssignRoles ? (
            <TextField
              select
              label="Roles"
              slotProps={{
                select: {
                  multiple: true,
                  renderValue: (selected: unknown) => (selected as string[]).join(', ')
                }
              }}
              value={form.roles}
              onChange={(event) => setForm({ ...form, roles: event.target.value as unknown as string[] })}
            >
              {rolesQuery.data?.items.map((role) => (
                <MenuItem key={role.id} value={role.name}>
                  <Checkbox checked={form.roles.includes(role.name)} />
                  <ListItemText primary={role.name} secondary={role.description} />
                </MenuItem>
              ))}
            </TextField>
          ) : null}
          <TextField label="Profile Photo URL" value={form.profilePhoto ?? ''} onChange={(event) => setForm({ ...form, profilePhoto: event.target.value || null })} />
          <TextField label="Phone" value={form.phone ?? ''} onChange={(event) => setForm({ ...form, phone: event.target.value || null })} />
          <TextField label="Time Zone" required value={form.timeZone} onChange={(event) => setForm({ ...form, timeZone: event.target.value })} />
          <TextField label="Skills" multiline minRows={3} value={form.skills ?? ''} onChange={(event) => setForm({ ...form, skills: event.target.value || null })} />
          <TextField label="Joining Date" type="date" value={form.joiningDate ?? ''} onChange={(event) => setForm({ ...form, joiningDate: event.target.value || null })} slotProps={{ inputLabel: { shrink: true } }} />
          <FormControlLabel control={<Switch checked={form.isActive} onChange={(event) => setForm({ ...form, isActive: event.target.checked })} />} label="Active" />
        </Stack>
      </FormDialog>
      {resetUser ? (
        <FormDialog open={resetUser !== null} title="New Password" loading={resetMutation.isPending} onClose={() => setResetUser(null)} onSubmit={() => resetMutation.mutate()}>
          <TextField label="New Password" type="password" required value={newPassword} onChange={(event) => setNewPassword(event.target.value)} />
        </FormDialog>
      ) : null}
    </Stack>
  );
}
