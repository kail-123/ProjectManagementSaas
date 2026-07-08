import { FormControlLabel, MenuItem, Stack, Switch, TextField } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { usePermissions } from '../../features/auth/model/usePermissions';
import { managementApi } from '../../shared/management/managementApi';
import type { Department, DepartmentRequest } from '../../shared/management/managementTypes';
import type { DataTableColumn } from '../../shared/ui/DataTable';
import { permissions } from '../../shared/security/permissions';
import { CrudPage } from './CrudPage';
import { optional, statusChip } from './pageUtils';

const emptyDepartment: DepartmentRequest = {
  organizationId: '',
  name: '',
  code: '',
  description: null,
  isActive: true
};

const columns: DataTableColumn<Department>[] = [
  { field: 'name', headerName: 'Name', minWidth: 180, render: (row) => row.name, exportValue: (row) => row.name },
  { field: 'code', headerName: 'Code', minWidth: 100, render: (row) => row.code, exportValue: (row) => row.code },
  { field: 'organization', headerName: 'Organization', minWidth: 180, render: (row) => row.organizationName, exportValue: (row) => row.organizationName },
  { field: 'description', headerName: 'Description', minWidth: 220, render: (row) => optional(row.description), exportValue: (row) => row.description },
  { field: 'isActive', headerName: 'Status', minWidth: 100, render: (row) => statusChip(row.isActive), exportValue: (row) => row.isActive ? 'Active' : 'Inactive' }
];

export function DepartmentsPage() {
  const { hasPermission } = usePermissions();
  const canViewOrganizations = hasPermission(permissions.organizations.view);
  const organizationsQuery = useQuery({
    queryKey: ['organizations', 'lookup'],
    queryFn: () => managementApi.organizations.list({ pageNumber: 1, pageSize: 200, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canViewOrganizations
  });

  return (
    <CrudPage
      title="Departments"
      description="Department structure connected to each organization."
      queryKey="departments"
      exportFileName="departments"
      columns={columns}
      emptyRequest={emptyDepartment}
      getRowLabel={(row) => row.name}
      toRequest={(row) => ({
        organizationId: row.organizationId,
        name: row.name,
        code: row.code,
        description: row.description,
        isActive: row.isActive
      })}
      list={(query) => managementApi.departments.list(query)}
      create={managementApi.departments.create}
      update={managementApi.departments.update}
      delete={managementApi.departments.delete}
      bulkDelete={managementApi.departments.bulkDelete}
      permissions={{
        create: permissions.departments.create,
        update: permissions.departments.update,
        delete: permissions.departments.delete,
        export: permissions.departments.export
      }}
      renderForm={(form, setForm) => (
        <Stack spacing={2}>
          <TextField select label="Organization" required value={form.organizationId} onChange={(event) => setForm({ ...form, organizationId: event.target.value })}>
            {organizationsQuery.data?.items.map((organization) => (
              <MenuItem key={organization.id} value={organization.id}>{organization.name}</MenuItem>
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
