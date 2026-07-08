import { Box, Button, Paper, Stack, Typography } from '@mui/material';
import type { ReactNode } from 'react';
import { Inbox, RefreshCcw, TriangleAlert } from 'lucide-react';

interface StateBlockProps {
  title: string;
  description?: string;
  action?: ReactNode;
}

export function EmptyState({ title, description, action }: StateBlockProps) {
  return (
    <StateShell icon={<Inbox size={28} />} title={title} description={description} action={action} />
  );
}

export function ErrorState({ title, description, onRetry }: StateBlockProps & { onRetry?: () => void }) {
  return (
    <StateShell
      icon={<TriangleAlert size={28} />}
      title={title}
      description={description}
      action={onRetry ? <Button variant="outlined" startIcon={<RefreshCcw size={16} />} onClick={onRetry}>Retry</Button> : undefined}
    />
  );
}

function StateShell({ icon, title, description, action }: StateBlockProps & { icon: ReactNode }) {
  return (
    <Paper sx={{ p: 4, border: '1px dashed', borderColor: 'divider', borderRadius: 3, textAlign: 'center' }}>
      <Stack spacing={1.5} sx={{ alignItems: 'center' }}>
        <Box sx={{ width: 56, height: 56, borderRadius: 3, display: 'grid', placeItems: 'center', bgcolor: 'action.hover', color: 'text.secondary' }}>
          {icon}
        </Box>
        <Typography variant="h6">{title}</Typography>
        {description ? <Typography color="text.secondary" sx={{ maxWidth: 520 }}>{description}</Typography> : null}
        {action ? <Box sx={{ pt: 1 }}>{action}</Box> : null}
      </Stack>
    </Paper>
  );
}
