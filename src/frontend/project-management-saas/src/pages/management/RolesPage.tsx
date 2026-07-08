import { Checkbox, ListItemText, MenuItem, Stack, TextField } from '@mui/material';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { getApiErrorMessage } from '../../shared/api/apiError';
import { managementApi } from '../../shared/management/managementApi';
import type { Role, RoleRequest } from '../../shared/management/managementTypes';
import { DataTable } from '../../shared/ui/DataTable';
import type { DataTableColumn, DataTableQuery } from '../../shared/ui/DataTable';
import { FormDialog } from '../../shared/ui/FormDialog';
import { PageHeader } from '../../shared/ui/PageHeader';
import { permissions } from '../../shared/security/permissions';
import { useToast } from '../../shared/ui/toastContext';
import { usePermissions } from '../../features/auth/model/usePermissions';
import { optional } from './pageUtils';

interface RoleForm extends RoleRequest {
  permissionIds: string[];
}

const emptyRole: RoleForm = {
  name: '',
  description: null,
  permissionIds: []
};

const columns: DataTableColumn<Role>[] = [
  { field: 'name', headerName: 'Name', minWidth: 180, render: (row) => row.name, exportValue: (row) => row.name },
  { field: 'description', headerName: 'Description', minWidth: 240, render: (row) => optional(row.description), exportValue: (row) => row.description },
  { field: 'permissions', headerName: 'Permissions', minWidth: 140, sortable: false, render: (row) => row.permissions.length, exportValue: (row) => row.permissions.length }
];

export function RolesPage() {
  const [query, setQuery] = useState<DataTableQuery>({ pageNumber: 1, pageSize: 25, sortBy: 'name', sortDirection: 'asc' });
  const [formOpen, setFormOpen] = useState(false);
  const [editingRole, setEditingRole] = useState<Role | null>(null);
  const [form, setForm] = useState<RoleForm>(emptyRole);
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const { hasPermission } = usePermissions();
  const canCreate = hasPermission(permissions.roles.create);
  const canUpdate = hasPermission(permissions.roles.update);
  const canDelete = hasPermission(permissions.roles.delete);
  const canExport = hasPermission(permissions.roles.export);
  const canAssignPermissions = hasPermission(permissions.roles.assignPermissions);
  const canViewPermissions = hasPermission(permissions.permissions.view);

  const rolesQuery = useQuery({ queryKey: ['roles', query], queryFn: () => managementApi.roles.list(query) });
  const permissionsQuery = useQuery({
    queryKey: ['permissions', 'lookup'],
    queryFn: () => managementApi.permissions.list({ pageNumber: 1, pageSize: 200, sortBy: 'module', sortDirection: 'asc' }),
    enabled: canAssignPermissions && canViewPermissions
  });

  const invalidate = async () => queryClient.invalidateQueries({ queryKey: ['roles'] });

  const saveMutation = useMutation({
    mutationFn: async () => {
      const role = editingRole
        ? await managementApi.roles.update(editingRole.id, { name: form.name, description: form.description })
        : await managementApi.roles.create({ name: form.name, description: form.description });
      if (canAssignPermissions) {
        await managementApi.roles.assignPermissions(role.id, form.permissionIds);
      }
    },
    onSuccess: async () => {
      showToast('Role saved.');
      setFormOpen(false);
      await invalidate();
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to save role.'), 'error')
  });

  const deleteMutation = useMutation({
    mutationFn: (role: Role) => managementApi.roles.delete(role.id),
    onSuccess: async () => {
      showToast('Role deleted.');
      await invalidate();
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to delete role.'), 'error')
  });

  const bulkDeleteMutation = useMutation({
    mutationFn: (ids: string[]) => managementApi.roles.bulkDelete(ids),
    onSuccess: async () => {
      showToast('Roles deleted.');
      await invalidate();
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to delete selected roles.'), 'error')
  });

  const openCreate = () => {
    setEditingRole(null);
    setForm(emptyRole);
    setFormOpen(true);
  };

  const openEdit = (role: Role) => {
    setEditingRole(role);
    setForm({ name: role.name, description: role.description, permissionIds: role.permissions.map((permission) => permission.id) });
    setFormOpen(true);
  };

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Roles"
        description="Role definitions and permission assignments for workspace access."
      />
      <DataTable
        title="Roles"
        rows={rolesQuery.data?.items ?? []}
        totalCount={rolesQuery.data?.totalCount ?? 0}
        columns={columns}
        query={query}
        loading={rolesQuery.isLoading}
        exportFileName="roles"
        getRowLabel={(row) => row.name}
        onQueryChange={setQuery}
        onCreate={canCreate ? openCreate : undefined}
        onEdit={canUpdate ? openEdit : undefined}
        onDelete={canDelete ? (row) => deleteMutation.mutate(row) : undefined}
        onBulkDelete={canDelete ? (ids) => bulkDeleteMutation.mutate(ids) : undefined}
        canExport={canExport}
        error={rolesQuery.isError}
        onRetry={() => void rolesQuery.refetch()}
      />
      <FormDialog
        open={formOpen}
        title={editingRole ? 'Edit Role' : 'Create Role'}
        loading={saveMutation.isPending}
        onClose={() => setFormOpen(false)}
        onSubmit={() => saveMutation.mutate()}
      >
        <Stack spacing={2}>
          <TextField label="Name" required value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} />
          <TextField label="Description" multiline minRows={3} value={form.description ?? ''} onChange={(event) => setForm({ ...form, description: event.target.value || null })} />
          {canAssignPermissions ? (
            <TextField
              select
              label="Permissions"
              slotProps={{
                select: {
                  multiple: true,
                  renderValue: (selected: unknown) => permissionsQuery.data?.items.filter((permission) => (selected as string[]).includes(permission.id)).map((permission) => permission.code).join(', ')
                }
              }}
              value={form.permissionIds}
              onChange={(event) => setForm({ ...form, permissionIds: event.target.value as unknown as string[] })}
            >
              {permissionsQuery.data?.items.map((permission) => (
                <MenuItem key={permission.id} value={permission.id}>
                  <Checkbox checked={form.permissionIds.includes(permission.id)} />
                  <ListItemText primary={permission.name} secondary={`${permission.module} / ${permission.code}`} />
                </MenuItem>
              ))}
            </TextField>
          ) : null}
        </Stack>
      </FormDialog>
    </Stack>
  );
}
