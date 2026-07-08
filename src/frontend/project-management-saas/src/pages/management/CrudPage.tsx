import { Stack } from '@mui/material';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { ReactNode } from 'react';
import { useState } from 'react';
import { PageHeader } from '../../shared/ui/PageHeader';
import { DataTable } from '../../shared/ui/DataTable';
import type { DataTableColumn, DataTableQuery } from '../../shared/ui/DataTable';
import { FormDialog } from '../../shared/ui/FormDialog';
import { useToast } from '../../shared/ui/toastContext';
import type { PagedResult } from '../../shared/management/managementTypes';
import { getApiErrorMessage } from '../../shared/api/apiError';
import { usePermissions } from '../../features/auth/model/usePermissions';
import type { CrudPermissionSet } from '../../shared/security/permissions';

interface CrudPageProps<TRow extends { id: string }, TRequest> {
  title: string;
  description?: string;
  queryKey: string;
  exportFileName: string;
  columns: DataTableColumn<TRow>[];
  emptyRequest: TRequest;
  getRowLabel: (row: TRow) => string;
  toRequest: (row: TRow) => TRequest;
  list: (query: DataTableQuery) => Promise<PagedResult<TRow>>;
  create: (request: TRequest) => Promise<TRow>;
  update: (id: string, request: TRequest) => Promise<TRow>;
  delete: (id: string) => Promise<void>;
  bulkDelete: (ids: readonly string[]) => Promise<void>;
  permissions: CrudPermissionSet;
  renderForm: (form: TRequest, setForm: (form: TRequest) => void) => ReactNode;
}

export function CrudPage<TRow extends { id: string }, TRequest>({
  title,
  description,
  queryKey,
  exportFileName,
  columns,
  emptyRequest,
  getRowLabel,
  toRequest,
  list,
  create,
  update,
  delete: deleteRecord,
  bulkDelete,
  permissions,
  renderForm
}: CrudPageProps<TRow, TRequest>) {
  const [query, setQuery] = useState<DataTableQuery>({ pageNumber: 1, pageSize: 25, sortDirection: 'asc' });
  const [editingRow, setEditingRow] = useState<TRow | null>(null);
  const [form, setForm] = useState<TRequest>(emptyRequest);
  const [formOpen, setFormOpen] = useState(false);
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const { hasPermission } = usePermissions();
  const canCreate = hasPermission(permissions.create);
  const canUpdate = hasPermission(permissions.update);
  const canDelete = hasPermission(permissions.delete);
  const canExport = hasPermission(permissions.export);

  const recordsQuery = useQuery({
    queryKey: [queryKey, query],
    queryFn: () => list(query)
  });

  const invalidate = async () => {
    await queryClient.invalidateQueries({ queryKey: [queryKey] });
  };

  const saveMutation = useMutation({
    mutationFn: () => (editingRow ? update(editingRow.id, form) : create(form)),
    onSuccess: async () => {
      showToast(`${title} saved.`);
      setFormOpen(false);
      await invalidate();
    },
    onError: (error) => showToast(getApiErrorMessage(error, `Unable to save ${title.toLowerCase()}.`), 'error')
  });

  const deleteMutation = useMutation({
    mutationFn: (row: TRow) => deleteRecord(row.id),
    onSuccess: async () => {
      showToast(`${title} deleted.`);
      await invalidate();
    },
    onError: (error) => showToast(getApiErrorMessage(error, `Unable to delete ${title.toLowerCase()}.`), 'error')
  });

  const bulkDeleteMutation = useMutation({
    mutationFn: (ids: string[]) => bulkDelete(ids),
    onSuccess: async () => {
      showToast(`${title} records deleted.`);
      await invalidate();
    },
    onError: (error) => showToast(getApiErrorMessage(error, `Unable to delete selected ${title.toLowerCase()} records.`), 'error')
  });

  const openCreate = () => {
    setEditingRow(null);
    setForm(emptyRequest);
    setFormOpen(true);
  };

  const openEdit = (row: TRow) => {
    setEditingRow(row);
    setForm(toRequest(row));
    setFormOpen(true);
  };

  return (
    <Stack spacing={3}>
      <PageHeader
        title={title}
        description={description ?? `${title} across the current workspace.`}
      />
      <DataTable
        title={title}
        rows={recordsQuery.data?.items ?? []}
        totalCount={recordsQuery.data?.totalCount ?? 0}
        columns={columns}
        query={query}
        loading={recordsQuery.isLoading}
        exportFileName={exportFileName}
        getRowLabel={getRowLabel}
        onQueryChange={setQuery}
        onCreate={canCreate ? openCreate : undefined}
        onEdit={canUpdate ? openEdit : undefined}
        onDelete={canDelete ? (row) => deleteMutation.mutate(row) : undefined}
        onBulkDelete={canDelete ? (ids) => bulkDeleteMutation.mutate(ids) : undefined}
        canExport={canExport}
        error={recordsQuery.isError}
        onRetry={() => void recordsQuery.refetch()}
      />
      <FormDialog
        open={formOpen}
        title={editingRow ? `Edit ${title}` : `Create ${title}`}
        loading={saveMutation.isPending}
        onClose={() => setFormOpen(false)}
        onSubmit={() => saveMutation.mutate()}
      >
        {renderForm(form, setForm)}
      </FormDialog>
    </Stack>
  );
}
