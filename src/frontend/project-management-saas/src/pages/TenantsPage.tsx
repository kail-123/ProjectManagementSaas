import { Paper, Stack } from '@mui/material';
import { PageHeader } from '../shared/ui/PageHeader';
import { EmptyState } from '../shared/ui/StateBlocks';

export function TenantsPage() {
  return (
    <Stack spacing={3}>
      <PageHeader title="Tenants" description="Tenant records and operating status." />
      <Paper sx={{ p: 3, border: '1px solid', borderColor: 'divider', borderRadius: 3 }}>
        <EmptyState title="No tenants found" />
      </Paper>
    </Stack>
  );
}
