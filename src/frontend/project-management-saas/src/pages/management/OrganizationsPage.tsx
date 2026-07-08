import { Stack, Switch, TextField, FormControlLabel } from '@mui/material';
import { managementApi } from '../../shared/management/managementApi';
import type { Organization, OrganizationRequest } from '../../shared/management/managementTypes';
import type { DataTableColumn } from '../../shared/ui/DataTable';
import { permissions } from '../../shared/security/permissions';
import { CrudPage } from './CrudPage';
import { statusChip } from './pageUtils';

const emptyOrganization: OrganizationRequest = {
  name: '',
  code: '',
  logo: null,
  website: null,
  email: '',
  phone: null,
  address: null,
  city: null,
  state: null,
  country: '',
  timeZone: 'Asia/Calcutta',
  currency: 'INR',
  isActive: true
};

const columns: DataTableColumn<Organization>[] = [
  { field: 'name', headerName: 'Name', minWidth: 180, render: (row) => row.name, exportValue: (row) => row.name },
  { field: 'code', headerName: 'Code', minWidth: 100, render: (row) => row.code, exportValue: (row) => row.code },
  { field: 'email', headerName: 'Email', minWidth: 180, render: (row) => row.email, exportValue: (row) => row.email },
  { field: 'country', headerName: 'Country', minWidth: 140, render: (row) => row.country, exportValue: (row) => row.country },
  { field: 'timeZone', headerName: 'Time Zone', minWidth: 160, render: (row) => row.timeZone, exportValue: (row) => row.timeZone },
  { field: 'currency', headerName: 'Currency', minWidth: 100, render: (row) => row.currency, exportValue: (row) => row.currency },
  { field: 'isActive', headerName: 'Status', minWidth: 100, render: (row) => statusChip(row.isActive), exportValue: (row) => row.isActive ? 'Active' : 'Inactive' }
];

export function OrganizationsPage() {
  return (
    <CrudPage
      title="Organizations"
      description="Tenant workspaces, regional defaults, and operating status."
      queryKey="organizations"
      exportFileName="organizations"
      columns={columns}
      emptyRequest={emptyOrganization}
      getRowLabel={(row) => row.name}
      toRequest={(row) => ({
        name: row.name,
        code: row.code,
        logo: row.logo,
        website: row.website,
        email: row.email,
        phone: row.phone,
        address: row.address,
        city: row.city,
        state: row.state,
        country: row.country,
        timeZone: row.timeZone,
        currency: row.currency,
        isActive: row.isActive
      })}
      list={managementApi.organizations.list}
      create={managementApi.organizations.create}
      update={managementApi.organizations.update}
      delete={managementApi.organizations.delete}
      bulkDelete={managementApi.organizations.bulkDelete}
      permissions={{
        create: permissions.organizations.create,
        update: permissions.organizations.update,
        delete: permissions.organizations.delete,
        export: permissions.organizations.export
      }}
      renderForm={(form, setForm) => (
        <Stack spacing={2}>
          <TextField label="Name" required value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} />
          <TextField label="Code" required value={form.code} onChange={(event) => setForm({ ...form, code: event.target.value })} />
          <TextField label="Logo URL" value={form.logo ?? ''} onChange={(event) => setForm({ ...form, logo: event.target.value || null })} />
          <TextField label="Website" value={form.website ?? ''} onChange={(event) => setForm({ ...form, website: event.target.value || null })} />
          <TextField label="Email" required type="email" value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} />
          <TextField label="Phone" value={form.phone ?? ''} onChange={(event) => setForm({ ...form, phone: event.target.value || null })} />
          <TextField label="Address" value={form.address ?? ''} onChange={(event) => setForm({ ...form, address: event.target.value || null })} />
          <TextField label="City" value={form.city ?? ''} onChange={(event) => setForm({ ...form, city: event.target.value || null })} />
          <TextField label="State" value={form.state ?? ''} onChange={(event) => setForm({ ...form, state: event.target.value || null })} />
          <TextField label="Country" required value={form.country} onChange={(event) => setForm({ ...form, country: event.target.value })} />
          <TextField label="Time Zone" required value={form.timeZone} onChange={(event) => setForm({ ...form, timeZone: event.target.value })} />
          <TextField label="Currency" required value={form.currency} onChange={(event) => setForm({ ...form, currency: event.target.value.toUpperCase() })} />
          <FormControlLabel control={<Switch checked={form.isActive} onChange={(event) => setForm({ ...form, isActive: event.target.checked })} />} label="Active" />
        </Stack>
      )}
    />
  );
}
