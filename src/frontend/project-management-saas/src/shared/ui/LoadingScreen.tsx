import { Box, CircularProgress, Stack, Typography } from '@mui/material';

interface LoadingScreenProps {
  label: string;
}

export function LoadingScreen({ label }: LoadingScreenProps) {
  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'grid',
        placeItems: 'center',
        bgcolor: 'background.default'
      }}
    >
      <Stack spacing={2} sx={{ alignItems: 'center' }}>
        <CircularProgress size={32} />
        <Typography variant="body2" color="text.secondary">
          {label}
        </Typography>
      </Stack>
    </Box>
  );
}
