import { Chip, MenuItem, Stack, TextField } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { usePermissions } from '../../features/auth/model/usePermissions';
import { managementApi } from '../../shared/management/managementApi';
import type { Client, ClientRequest, ClientStatus } from '../../shared/management/managementTypes';
import type { DataTableColumn } from '../../shared/ui/DataTable';
import { permissions } from '../../shared/security/permissions';
import { CrudPage } from './CrudPage';
import { optional } from './pageUtils';

const clientStatuses: ClientStatus[] = ['Prospect', 'Active', 'Inactive', 'Archived'];

const emptyClient: ClientRequest = {
  organizationId: '',
  name: '',
  code: '',
  contactPerson: null,
  email: '',
  phone: null,
  mobile: null,
  gstNumber: null,
  pan: null,
  billingAddress: null,
  shippingAddress: null,
  country: '',
  state: null,
  city: null,
  website: null,
  notes: null,
  status: 'Active'
};

const columns: DataTableColumn<Client>[] = [
  { field: 'name', headerName: 'Client', minWidth: 180, render: (row) => row.name, exportValue: (row) => row.name },
  { field: 'code', headerName: 'Code', minWidth: 100, render: (row) => row.code, exportValue: (row) => row.code },
  { field: 'organizationName', headerName: 'Organization', minWidth: 180, render: (row) => row.organizationName, exportValue: (row) => row.organizationName },
  { field: 'contactPerson', headerName: 'Contact', minWidth: 160, render: (row) => optional(row.contactPerson), exportValue: (row) => row.contactPerson },
  { field: 'email', headerName: 'Email', minWidth: 200, render: (row) => row.email, exportValue: (row) => row.email },
  { field: 'mobile', headerName: 'Mobile', minWidth: 130, render: (row) => optional(row.mobile), exportValue: (row) => row.mobile },
  { field: 'country', headerName: 'Country', minWidth: 130, render: (row) => row.country, exportValue: (row) => row.country },
  {
    field: 'status',
    headerName: 'Status',
    minWidth: 120,
    render: (row) => <Chip size="small" color={row.status === 'Active' ? 'success' : 'default'} label={row.status} />,
    exportValue: (row) => row.status
  }
];

export function ClientsPage() {
  const { hasPermission } = usePermissions();
  const canViewOrganizations = hasPermission(permissions.organizations.view);
  const organizationsQuery = useQuery({
    queryKey: ['organizations', 'lookup'],
    queryFn: () => managementApi.organizations.list({ pageNumber: 1, pageSize: 200, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canViewOrganizations
  });

  return (
    <CrudPage
      title="Clients"
      description="Client accounts, contacts, billing details, and account status."
      queryKey="clients"
      exportFileName="clients"
      columns={columns}
      emptyRequest={emptyClient}
      getRowLabel={(row) => row.name}
      toRequest={(row) => ({
        organizationId: row.organizationId,
        name: row.name,
        code: row.code,
        contactPerson: row.contactPerson,
        email: row.email,
        phone: row.phone,
        mobile: row.mobile,
        gstNumber: row.gstNumber,
        pan: row.pan,
        billingAddress: row.billingAddress,
        shippingAddress: row.shippingAddress,
        country: row.country,
        state: row.state,
        city: row.city,
        website: row.website,
        notes: row.notes,
        status: row.status
      })}
      list={(query) => managementApi.clients.list(query)}
      create={managementApi.clients.create}
      update={managementApi.clients.update}
      delete={managementApi.clients.delete}
      bulkDelete={managementApi.clients.bulkDelete}
      permissions={{
        create: permissions.clients.create,
        update: permissions.clients.update,
        delete: permissions.clients.delete,
        export: permissions.clients.export
      }}
      renderForm={(form, setForm) => (
        <Stack spacing={2}>
          <TextField select label="Organization" required value={form.organizationId} onChange={(event) => setForm({ ...form, organizationId: event.target.value })}>
            {organizationsQuery.data?.items.map((organization) => (
              <MenuItem key={organization.id} value={organization.id}>{organization.name}</MenuItem>
            ))}
          </TextField>
          <TextField label="Client Name" required value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} />
          <TextField label="Client Code" required value={form.code} onChange={(event) => setForm({ ...form, code: event.target.value })} />
          <TextField label="Contact Person" value={form.contactPerson ?? ''} onChange={(event) => setForm({ ...form, contactPerson: event.target.value || null })} />
          <TextField label="Email" required type="email" value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} />
          <TextField label="Phone" value={form.phone ?? ''} onChange={(event) => setForm({ ...form, phone: event.target.value || null })} />
          <TextField label="Mobile" value={form.mobile ?? ''} onChange={(event) => setForm({ ...form, mobile: event.target.value || null })} />
          <TextField label="GST Number" value={form.gstNumber ?? ''} onChange={(event) => setForm({ ...form, gstNumber: event.target.value || null })} />
          <TextField label="PAN" value={form.pan ?? ''} onChange={(event) => setForm({ ...form, pan: event.target.value || null })} />
          <TextField label="Billing Address" multiline minRows={2} value={form.billingAddress ?? ''} onChange={(event) => setForm({ ...form, billingAddress: event.target.value || null })} />
          <TextField label="Shipping Address" multiline minRows={2} value={form.shippingAddress ?? ''} onChange={(event) => setForm({ ...form, shippingAddress: event.target.value || null })} />
          <TextField label="Country" required value={form.country} onChange={(event) => setForm({ ...form, country: event.target.value })} />
          <TextField label="State" value={form.state ?? ''} onChange={(event) => setForm({ ...form, state: event.target.value || null })} />
          <TextField label="City" value={form.city ?? ''} onChange={(event) => setForm({ ...form, city: event.target.value || null })} />
          <TextField label="Website" value={form.website ?? ''} onChange={(event) => setForm({ ...form, website: event.target.value || null })} />
          <TextField label="Notes" multiline minRows={3} value={form.notes ?? ''} onChange={(event) => setForm({ ...form, notes: event.target.value || null })} />
          <TextField select label="Status" required value={form.status} onChange={(event) => setForm({ ...form, status: event.target.value as ClientStatus })}>
            {clientStatuses.map((status) => (
              <MenuItem key={status} value={status}>{status}</MenuItem>
            ))}
          </TextField>
        </Stack>
      )}
    />
  );
}
