import { Paper, Stack } from '@mui/material';
import { PageHeader } from '../shared/ui/PageHeader';
import { EmptyState } from '../shared/ui/StateBlocks';

export function SubscriptionsPage() {
  return (
    <Stack spacing={3}>
      <PageHeader title="Subscriptions" description="Plan, billing, and subscription records." />
      <Paper sx={{ p: 3, border: '1px solid', borderColor: 'divider', borderRadius: 3 }}>
        <EmptyState title="No subscriptions found" />
      </Paper>
    </Stack>
  );
}
