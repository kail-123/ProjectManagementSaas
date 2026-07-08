import { Checkbox, Link, ListItemText, MenuItem, Stack, TextField } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { Link as RouterLink } from 'react-router-dom';
import { routePaths } from '../../app/router/routePaths';
import { useAuthStore } from '../../features/auth/model/authStore';
import { usePermissions } from '../../features/auth/model/usePermissions';
import { managementApi } from '../../shared/management/managementApi';
import { permissions } from '../../shared/security/permissions';
import type {
  Project,
  ProjectPriority,
  ProjectRequest,
  ProjectStatus,
  ProjectVisibility
} from '../../shared/management/managementTypes';
import type { DataTableColumn } from '../../shared/ui/DataTable';
import { CrudPage } from './CrudPage';
import { enumLabel, formatCurrency, formatDate, optional, priorityChip, projectStatusChip } from './pageUtils';

const projectStatuses: ProjectStatus[] = ['Planning', 'Active', 'OnHold', 'Completed', 'Cancelled', 'Archived'];
const projectPriorities: ProjectPriority[] = ['Low', 'Medium', 'High', 'Critical'];
const projectVisibilities: ProjectVisibility[] = ['Private', 'Organization', 'Public'];

const emptyProject: ProjectRequest = {
  organizationId: '',
  clientId: null,
  projectManagerUserProfileId: null,
  name: '',
  code: '',
  description: null,
  startDate: null,
  endDate: null,
  estimatedBudget: null,
  status: 'Planning',
  priority: 'Medium',
  visibility: 'Organization',
  teamIds: []
};

const columns: DataTableColumn<Project>[] = [
  {
    field: 'name',
    headerName: 'Project',
    minWidth: 200,
    render: (row) => <Link component={RouterLink} to={routePaths.projectDetails(row.id)}>{row.name}</Link>,
    exportValue: (row) => row.name
  },
  { field: 'code', headerName: 'Code', minWidth: 100, render: (row) => row.code, exportValue: (row) => row.code },
  { field: 'organizationName', headerName: 'Organization', minWidth: 180, render: (row) => row.organizationName, exportValue: (row) => row.organizationName },
  { field: 'clientName', headerName: 'Client', minWidth: 160, render: (row) => optional(row.clientName), exportValue: (row) => row.clientName },
  { field: 'projectManagerName', headerName: 'Manager', minWidth: 170, sortable: false, render: (row) => optional(row.projectManagerName), exportValue: (row) => row.projectManagerName },
  { field: 'status', headerName: 'Status', minWidth: 120, render: (row) => projectStatusChip(row.status), exportValue: (row) => enumLabel(row.status) },
  { field: 'priority', headerName: 'Priority', minWidth: 120, render: (row) => priorityChip(row.priority), exportValue: (row) => row.priority },
  { field: 'visibility', headerName: 'Visibility', minWidth: 130, sortable: false, render: (row) => enumLabel(row.visibility), exportValue: (row) => enumLabel(row.visibility) },
  { field: 'startDate', headerName: 'Start', minWidth: 120, render: (row) => formatDate(row.startDate), exportValue: (row) => row.startDate },
  { field: 'endDate', headerName: 'End', minWidth: 120, render: (row) => formatDate(row.endDate), exportValue: (row) => row.endDate },
  { field: 'estimatedBudget', headerName: 'Budget', minWidth: 120, sortable: false, render: (row) => formatCurrency(row.estimatedBudget), exportValue: (row) => row.estimatedBudget },
  { field: 'memberCount', headerName: 'Members', minWidth: 100, sortable: false, render: (row) => row.memberCount, exportValue: (row) => row.memberCount }
];

export function ProjectsPage() {
  const { hasPermission } = usePermissions();
  const isSystemAdministrator = useAuthStore((state) => state.user?.roles.includes('SystemAdministrator') ?? false);
  const canViewOrganizations = hasPermission(permissions.organizations.view);
  const canViewClients = hasPermission(permissions.clients.view);
  const canViewDepartments = hasPermission(permissions.departments.view);
  const canViewTeams = hasPermission(permissions.teams.view);
  const canViewUsers = hasPermission(permissions.users.view);
  const organizationsQuery = useQuery({
    queryKey: ['organizations', 'lookup'],
    queryFn: () => managementApi.organizations.list({ pageNumber: 1, pageSize: 1000, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canViewOrganizations
  });
  const clientsQuery = useQuery({
    queryKey: ['clients', 'lookup'],
    queryFn: () => managementApi.clients.list({ pageNumber: 1, pageSize: 1000, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canViewClients
  });
  const departmentsQuery = useQuery({
    queryKey: ['departments', 'lookup'],
    queryFn: () => managementApi.departments.list({ pageNumber: 1, pageSize: 1000, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canViewDepartments
  });
  const teamsQuery = useQuery({
    queryKey: ['teams', 'lookup'],
    queryFn: () => managementApi.teams.list({ pageNumber: 1, pageSize: 1000, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canViewTeams
  });
  const usersQuery = useQuery({
    queryKey: ['users', 'lookup'],
    queryFn: () => managementApi.users.list({ pageNumber: 1, pageSize: 1000, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canViewUsers
  });

  const departmentOrganization = new Map(departmentsQuery.data?.items.map((department) => [department.id, department.organizationId]) ?? []);

  return (
    <CrudPage
      title="Projects"
      description="Project portfolio, ownership, timelines, budgets, and visibility."
      queryKey={isSystemAdministrator ? 'projects' : 'my-projects'}
      exportFileName="projects"
      columns={columns}
      emptyRequest={emptyProject}
      getRowLabel={(row) => row.name}
      toRequest={(row) => ({
        organizationId: row.organizationId,
        clientId: row.clientId,
        projectManagerUserProfileId: row.projectManagerUserProfileId,
        name: row.name,
        code: row.code,
        description: row.description,
        startDate: row.startDate,
        endDate: row.endDate,
        estimatedBudget: row.estimatedBudget,
        status: row.status,
        priority: row.priority,
        visibility: row.visibility,
        teamIds: row.teamIds
      })}
      list={(query) => isSystemAdministrator ? managementApi.projects.list(query) : managementApi.projects.myProjects(query)}
      create={managementApi.projects.create}
      update={managementApi.projects.update}
      delete={managementApi.projects.delete}
      bulkDelete={managementApi.projects.bulkDelete}
      permissions={{
        create: permissions.projects.create,
        update: permissions.projects.update,
        delete: permissions.projects.delete,
        export: permissions.projects.export
      }}
      renderForm={(form, setForm) => {
        const filteredClients = clientsQuery.data?.items.filter((client) => client.organizationId === form.organizationId) ?? [];
        const filteredTeams = teamsQuery.data?.items.filter((team) => departmentOrganization.get(team.departmentId) === form.organizationId) ?? [];
        const userProfiles = usersQuery.data?.items
          .filter((user) => user.profile?.organizationId === form.organizationId)
          .map((user) => user.profile!)
          ?? [];

        return (
          <Stack spacing={2}>
            <TextField
              select
              label="Organization"
              required
              value={form.organizationId}
              onChange={(event) => setForm({
                ...form,
                organizationId: event.target.value,
                clientId: null,
                projectManagerUserProfileId: null,
                teamIds: []
              })}
            >
              {organizationsQuery.data?.items.map((organization) => (
                <MenuItem key={organization.id} value={organization.id}>{organization.name}</MenuItem>
              ))}
            </TextField>
            <TextField select label="Client" value={form.clientId ?? ''} onChange={(event) => setForm({ ...form, clientId: event.target.value || null })}>
              <MenuItem value="">None</MenuItem>
              {filteredClients.map((client) => (
                <MenuItem key={client.id} value={client.id}>{client.name}</MenuItem>
              ))}
            </TextField>
            <TextField select label="Project Manager" value={form.projectManagerUserProfileId ?? ''} onChange={(event) => setForm({ ...form, projectManagerUserProfileId: event.target.value || null })}>
              <MenuItem value="">None</MenuItem>
              {userProfiles.map((profile) => (
                <MenuItem key={profile.id} value={profile.id}>{profile.firstName} {profile.lastName}</MenuItem>
              ))}
            </TextField>
            <TextField
              select
              label="Project Teams"
              slotProps={{
                select: {
                  multiple: true,
                  renderValue: (selected: unknown) => filteredTeams.filter((team) => (selected as string[]).includes(team.id)).map((team) => team.name).join(', ')
                }
              }}
              value={form.teamIds}
              onChange={(event) => setForm({ ...form, teamIds: event.target.value as unknown as string[] })}
            >
              {filteredTeams.map((team) => (
                <MenuItem key={team.id} value={team.id}>
                  <Checkbox checked={form.teamIds.includes(team.id)} />
                  <ListItemText primary={team.name} secondary={team.departmentName} />
                </MenuItem>
              ))}
            </TextField>
            <TextField label="Project Name" required value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} />
            <TextField label="Project Code" required value={form.code} onChange={(event) => setForm({ ...form, code: event.target.value })} />
            <TextField label="Description" multiline minRows={3} value={form.description ?? ''} onChange={(event) => setForm({ ...form, description: event.target.value || null })} />
            <TextField
              label="Start Date"
              type="date"
              value={form.startDate ?? ''}
              onChange={(event) => setForm({ ...form, startDate: event.target.value || null })}
              slotProps={{ inputLabel: { shrink: true } }}
            />
            <TextField
              label="End Date"
              type="date"
              value={form.endDate ?? ''}
              onChange={(event) => setForm({ ...form, endDate: event.target.value || null })}
              slotProps={{ inputLabel: { shrink: true } }}
            />
            <TextField
              label="Estimated Budget"
              type="number"
              value={form.estimatedBudget ?? ''}
              onChange={(event) => setForm({ ...form, estimatedBudget: event.target.value ? Number(event.target.value) : null })}
            />
            <TextField select label="Status" required value={form.status} onChange={(event) => setForm({ ...form, status: event.target.value as ProjectStatus })}>
              {projectStatuses.map((status) => (
                <MenuItem key={status} value={status}>{enumLabel(status)}</MenuItem>
              ))}
            </TextField>
            <TextField select label="Priority" required value={form.priority} onChange={(event) => setForm({ ...form, priority: event.target.value as ProjectPriority })}>
              {projectPriorities.map((priority) => (
                <MenuItem key={priority} value={priority}>{priority}</MenuItem>
              ))}
            </TextField>
            <TextField select label="Visibility" required value={form.visibility} onChange={(event) => setForm({ ...form, visibility: event.target.value as ProjectVisibility })}>
              {projectVisibilities.map((visibility) => (
                <MenuItem key={visibility} value={visibility}>{enumLabel(visibility)}</MenuItem>
              ))}
            </TextField>
          </Stack>
        );
      }}
    />
  );
}
