import { Box, Paper, Stack, Typography } from '@mui/material';
import { PageHeader } from '../shared/ui/PageHeader';

const days = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

export function CalendarPage() {
  return (
    <Stack spacing={3}>
      <PageHeader title="Calendar" description="Schedule, due dates, and personal planning." />
      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'repeat(7, minmax(0, 1fr))' }, gap: 1.5 }}>
        {days.map((day) => (
          <Paper key={day} sx={{ minHeight: 220, p: 2, border: '1px solid', borderColor: 'divider', borderRadius: 3 }}>
            <Typography variant="subtitle2" sx={{ fontWeight: 850 }}>{day}</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>No events</Typography>
          </Paper>
        ))}
      </Box>
    </Stack>
  );
}
