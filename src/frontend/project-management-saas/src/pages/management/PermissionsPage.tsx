import { FormControlLabel, Stack, Switch, TextField } from '@mui/material';
import { managementApi } from '../../shared/management/managementApi';
import type { Permission, PermissionRequest } from '../../shared/management/managementTypes';
import type { DataTableColumn } from '../../shared/ui/DataTable';
import { permissions } from '../../shared/security/permissions';
import { CrudPage } from './CrudPage';
import { optional, statusChip } from './pageUtils';

const emptyPermission: PermissionRequest = {
  module: '',
  name: '',
  code: '',
  description: null,
  isActive: true
};

const columns: DataTableColumn<Permission>[] = [
  { field: 'module', headerName: 'Module', minWidth: 160, render: (row) => row.module, exportValue: (row) => row.module },
  { field: 'name', headerName: 'Name', minWidth: 180, render: (row) => row.name, exportValue: (row) => row.name },
  { field: 'code', headerName: 'Code', minWidth: 180, render: (row) => row.code, exportValue: (row) => row.code },
  { field: 'description', headerName: 'Description', minWidth: 240, render: (row) => optional(row.description), exportValue: (row) => row.description },
  { field: 'isActive', headerName: 'Status', minWidth: 100, render: (row) => statusChip(row.isActive), exportValue: (row) => row.isActive ? 'Active' : 'Inactive' }
];

export function PermissionsPage() {
  return (
    <CrudPage
      title="Permissions"
      description="Module permissions grouped for dynamic role assignment."
      queryKey="permissions"
      exportFileName="permissions"
      columns={columns}
      emptyRequest={emptyPermission}
      getRowLabel={(row) => row.name}
      toRequest={(row) => ({
        module: row.module,
        name: row.name,
        code: row.code,
        description: row.description,
        isActive: row.isActive
      })}
      list={(query) => managementApi.permissions.list(query)}
      create={managementApi.permissions.create}
      update={managementApi.permissions.update}
      delete={managementApi.permissions.delete}
      bulkDelete={managementApi.permissions.bulkDelete}
      permissions={{
        create: permissions.permissions.create,
        update: permissions.permissions.update,
        delete: permissions.permissions.delete,
        export: permissions.permissions.export
      }}
      renderForm={(form, setForm) => (
        <Stack spacing={2}>
          <TextField label="Module" required value={form.module} onChange={(event) => setForm({ ...form, module: event.target.value })} />
          <TextField label="Name" required value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} />
          <TextField label="Code" required value={form.code} onChange={(event) => setForm({ ...form, code: event.target.value })} />
          <TextField label="Description" multiline minRows={3} value={form.description ?? ''} onChange={(event) => setForm({ ...form, description: event.target.value || null })} />
          <FormControlLabel control={<Switch checked={form.isActive} onChange={(event) => setForm({ ...form, isActive: event.target.checked })} />} label="Active" />
        </Stack>
      )}
    />
  );
}
