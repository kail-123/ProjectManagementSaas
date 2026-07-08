import { Box, Button, Container, Stack, Typography } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';
import { routePaths } from '../app/router/routePaths';

export function NotFoundPage() {
  return (
    <Box component="main" sx={{ minHeight: '100vh', display: 'grid', placeItems: 'center', px: 2 }}>
      <Container maxWidth="sm" disableGutters>
        <Stack spacing={3} sx={{ alignItems: 'flex-start' }}>
          <Typography variant="h3" component="h1" sx={{ fontWeight: 800 }}>
            Page Not Found
          </Typography>
          <Button component={RouterLink} to={routePaths.app} variant="contained">
            Return
          </Button>
        </Stack>
      </Container>
    </Box>
  );
}
