import { Avatar, Box, Paper, Stack, Typography } from '@mui/material';
import { useAuthStore } from '../features/auth/model/authStore';
import { PageHeader } from '../shared/ui/PageHeader';

export function ProfilePage() {
  const user = useAuthStore((state) => state.user);

  return (
    <Stack spacing={3}>
      <PageHeader title="Profile" description={user?.email} />
      <Paper sx={{ p: 3, border: '1px solid', borderColor: 'divider', borderRadius: 3 }}>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2.5} sx={{ alignItems: { xs: 'flex-start', sm: 'center' } }}>
          <Avatar sx={{ width: 72, height: 72, fontSize: 26 }}>{getInitials(user?.displayName ?? user?.email ?? 'U')}</Avatar>
          <Box>
            <Typography variant="h5">{user?.displayName ?? 'Workspace user'}</Typography>
            <Typography color="text.secondary">{user?.email}</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              {user?.roles.join(', ') || 'Standard access'}
            </Typography>
          </Box>
        </Stack>
      </Paper>
    </Stack>
  );
}

function getInitials(value: string) {
  return value
    .split(/[.@\s_-]+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('');
}
