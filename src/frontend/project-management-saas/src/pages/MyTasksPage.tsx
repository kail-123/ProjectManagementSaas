import { Box, Chip, Paper, Stack } from '@mui/material';
import { CheckCircle2, Clock3, ListChecks } from 'lucide-react';
import { MetricCard } from '../shared/ui/MetricCard';
import { PageHeader } from '../shared/ui/PageHeader';
import { EmptyState } from '../shared/ui/StateBlocks';

export function MyTasksPage() {
  return (
    <Stack spacing={3}>
      <PageHeader title="My Tasks" description="Personal work assigned to you." />

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'repeat(3, minmax(0, 1fr))' }, gap: 2 }}>
        <MetricCard title="Open" value={0} detail="Assigned tasks" icon={<ListChecks size={20} />} />
        <MetricCard title="Due Soon" value={0} detail="Next seven days" icon={<Clock3 size={20} />} tone="info" />
        <MetricCard title="Completed" value={0} detail="This week" icon={<CheckCircle2 size={20} />} tone="success" />
      </Box>

      <Paper sx={{ p: 3, border: '1px solid', borderColor: 'divider', borderRadius: 3 }}>
        <Stack spacing={2}>
          <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>
            <Chip size="small" label="Assigned to me" />
            <Chip size="small" label="Open" />
          </Stack>
          <EmptyState title="No tasks assigned" description="Tasks assigned to your profile will appear here." />
        </Stack>
      </Paper>
    </Stack>
  );
}
