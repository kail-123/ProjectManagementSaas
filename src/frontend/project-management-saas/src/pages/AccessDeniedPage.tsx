import { Button, Paper, Stack, Typography } from '@mui/material';
import { ShieldAlert } from 'lucide-react';
import { Link as RouterLink } from 'react-router-dom';
import { routePaths } from '../app/router/routePaths';

export function AccessDeniedPage() {
  return (
    <Paper sx={{ p: 4, border: '1px solid', borderColor: 'divider', borderRadius: 3 }}>
      <Stack spacing={2} sx={{ alignItems: 'flex-start' }}>
        <ShieldAlert size={32} />
        <Typography variant="h4">Access Denied</Typography>
        <Typography color="text.secondary">Your account does not have permission to open this page.</Typography>
        <Button component={RouterLink} to={routePaths.app} variant="contained">Dashboard</Button>
      </Stack>
    </Paper>
  );
}
