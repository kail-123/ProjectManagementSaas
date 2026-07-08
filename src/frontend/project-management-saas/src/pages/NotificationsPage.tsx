import { Box, Stack } from '@mui/material';
import { NotificationCenterPanel, NotificationPreferencesPanel } from '../features/notifications/NotificationCenter';
import { PageHeader } from '../shared/ui/PageHeader';

export function NotificationsPage() {
  return (
    <Stack spacing={3}>
      <PageHeader title="Notifications" description="Alerts and workspace updates." />
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: { xs: '1fr', lg: 'minmax(0, 1fr) 360px' },
          gap: 3,
          alignItems: 'start'
        }}
      >
        <NotificationCenterPanel />
        <NotificationPreferencesPanel />
      </Box>
    </Stack>
  );
}
