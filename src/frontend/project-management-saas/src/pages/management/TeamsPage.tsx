import { Checkbox, FormControlLabel, ListItemText, MenuItem, Stack, Switch, TextField } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { usePermissions } from '../../features/auth/model/usePermissions';
import { managementApi } from '../../shared/management/managementApi';
import type { Team, TeamRequest } from '../../shared/management/managementTypes';
import type { DataTableColumn } from '../../shared/ui/DataTable';
import { permissions } from '../../shared/security/permissions';
import { CrudPage } from './CrudPage';
import { optional, statusChip } from './pageUtils';

const emptyTeam: TeamRequest = {
  departmentId: '',
  teamLeadUserProfileId: null,
  name: '',
  code: '',
  description: null,
  isActive: true,
  memberUserProfileIds: []
};

const columns: DataTableColumn<Team>[] = [
  { field: 'name', headerName: 'Name', minWidth: 180, render: (row) => row.name, exportValue: (row) => row.name },
  { field: 'code', headerName: 'Code', minWidth: 100, render: (row) => row.code, exportValue: (row) => row.code },
  { field: 'department', headerName: 'Department', minWidth: 180, render: (row) => row.departmentName, exportValue: (row) => row.departmentName },
  { field: 'teamLead', headerName: 'Team Lead', minWidth: 180, render: (row) => optional(row.teamLeadName), exportValue: (row) => row.teamLeadName },
  { field: 'members', headerName: 'Members', minWidth: 100, sortable: false, render: (row) => row.memberUserProfileIds.length, exportValue: (row) => row.memberUserProfileIds.length },
  { field: 'isActive', headerName: 'Status', minWidth: 100, render: (row) => statusChip(row.isActive), exportValue: (row) => row.isActive ? 'Active' : 'Inactive' }
];

export function TeamsPage() {
  const { hasPermission } = usePermissions();
  const canViewDepartments = hasPermission(permissions.departments.view);
  const canViewUsers = hasPermission(permissions.users.view);
  const departmentsQuery = useQuery({
    queryKey: ['departments', 'lookup'],
    queryFn: () => managementApi.departments.list({ pageNumber: 1, pageSize: 200, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canViewDepartments
  });
  const usersQuery = useQuery({
    queryKey: ['users', 'lookup'],
    queryFn: () => managementApi.users.list({ pageNumber: 1, pageSize: 200, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canViewUsers
  });
  const userProfiles = usersQuery.data?.items.filter((user) => user.profile).map((user) => user.profile!) ?? [];

  return (
    <CrudPage
      title="Teams"
      description="Delivery groups, team leads, and assigned members."
      queryKey="teams"
      exportFileName="teams"
      columns={columns}
      emptyRequest={emptyTeam}
      getRowLabel={(row) => row.name}
      toRequest={(row) => ({
        departmentId: row.departmentId,
        teamLeadUserProfileId: row.teamLeadUserProfileId,
        name: row.name,
        code: row.code,
        description: row.description,
        isActive: row.isActive,
        memberUserProfileIds: row.memberUserProfileIds
      })}
      list={(query) => managementApi.teams.list(query)}
      create={managementApi.teams.create}
      update={managementApi.teams.update}
      delete={managementApi.teams.delete}
      bulkDelete={managementApi.teams.bulkDelete}
      permissions={{
        create: permissions.teams.create,
        update: permissions.teams.update,
        delete: permissions.teams.delete,
        export: permissions.teams.export
      }}
      renderForm={(form, setForm) => (
        <Stack spacing={2}>
          <TextField select label="Department" required value={form.departmentId} onChange={(event) => setForm({ ...form, departmentId: event.target.value })}>
            {departmentsQuery.data?.items.map((department) => (
              <MenuItem key={department.id} value={department.id}>{department.name}</MenuItem>
            ))}
          </TextField>
          <TextField select label="Team Lead" value={form.teamLeadUserProfileId ?? ''} onChange={(event) => setForm({ ...form, teamLeadUserProfileId: event.target.value || null })}>
            <MenuItem value="">None</MenuItem>
            {userProfiles.map((profile) => (
              <MenuItem key={profile.id} value={profile.id}>{profile.firstName} {profile.lastName}</MenuItem>
            ))}
          </TextField>
          <TextField
            select
            label="Team Members"
            slotProps={{
              select: {
                multiple: true,
                renderValue: (selected: unknown) => userProfiles.filter((profile) => (selected as string[]).includes(profile.id)).map((profile) => `${profile.firstName} ${profile.lastName}`).join(', ')
              }
            }}
            value={form.memberUserProfileIds}
            onChange={(event) => setForm({ ...form, memberUserProfileIds: event.target.value as unknown as string[] })}
          >
            {userProfiles.map((profile) => (
              <MenuItem key={profile.id} value={profile.id}>
                <Checkbox checked={form.memberUserProfileIds.includes(profile.id)} />
                <ListItemText primary={`${profile.firstName} ${profile.lastName}`} secondary={profile.employeeCode} />
              </MenuItem>
            ))}
          </TextField>
          <TextField label="Name" required value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} />
          <TextField label="Code" required value={form.code} onChange={(event) => setForm({ ...form, code: event.target.value })} />
          <TextField label="Description" multiline minRows={3} value={form.description ?? ''} onChange={(event) => setForm({ ...form, description: event.target.value || null })} />
          <FormControlLabel control={<Switch checked={form.isActive} onChange={(event) => setForm({ ...form, isActive: event.target.checked })} />} label="Active" />
        </Stack>
      )}
    />
  );
}
