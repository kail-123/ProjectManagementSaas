import {
  Alert,
  Box,
  Button,
  CircularProgress,
  InputAdornment,
  Paper,
  Stack,
  TextField,
  Typography,
  alpha,
  useTheme
} from '@mui/material';
import { Building2, LockKeyhole, LogIn, Mail, ShieldCheck } from 'lucide-react';
import { useMutation } from '@tanstack/react-query';
import type { FormEvent } from 'react';
import { useState } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import { routePaths } from '../../../app/router/routePaths';
import { authApi } from '../api/authApi';
import { useAuthStore } from '../model/authStore';

interface RedirectState {
  from?: {
    pathname?: string;
  };
}

export function SignInPage() {
  const theme = useTheme();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const setSession = useAuthStore((state) => state.setSession);
  const location = useLocation();
  const navigate = useNavigate();
  const redirectState = location.state as RedirectState | null;
  const returnTo = redirectState?.from?.pathname ?? routePaths.app;

  const signInMutation = useMutation({
    mutationFn: (request: { email: string; password: string }) => authApi.signIn(request),
    onSuccess: (session) => {
      setSession(session);
      void navigate(returnTo, { replace: true });
    }
  });

  if (isAuthenticated) {
    return <Navigate to={routePaths.app} replace />;
  }

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    signInMutation.mutate({ email, password });
  };

  return (
    <Box
      component="main"
      sx={{
        minHeight: '100vh',
        display: 'grid',
        gridTemplateColumns: { xs: '1fr', lg: 'minmax(420px, 0.92fr) minmax(420px, 1.08fr)' },
        bgcolor: 'background.default'
      }}
    >
      <Box
        sx={{
          display: { xs: 'none', lg: 'flex' },
          flexDirection: 'column',
          justifyContent: 'space-between',
          p: 6,
          borderRight: '1px solid',
          borderColor: 'divider',
          background:
            theme.palette.mode === 'dark'
              ? `linear-gradient(145deg, ${alpha(theme.palette.primary.dark, 0.32)}, ${alpha(theme.palette.background.paper, 0.86)})`
              : `linear-gradient(145deg, ${alpha(theme.palette.primary.light, 0.18)}, ${alpha(theme.palette.secondary.light, 0.12)})`
        }}
      >
        <Stack spacing={2}>
          <Box
            sx={{
              width: 56,
              height: 56,
              borderRadius: 3,
              display: 'grid',
              placeItems: 'center',
              bgcolor: 'background.paper',
              boxShadow: 3
            }}
          >
            <Building2 size={28} color={theme.palette.primary.main} />
          </Box>
          <Box>
            <Typography variant="h2" component="h1">PMSA Workspace</Typography>
            <Typography color="text.secondary" sx={{ mt: 1.25, maxWidth: 540 }}>
              Enterprise project operations for organizations, clients, delivery teams, and portfolios.
            </Typography>
          </Box>
        </Stack>

        <Stack spacing={1.5}>
          {['Secure access', 'Tenant-aware workspace', 'Portfolio command center'].map((label) => (
            <Stack key={label} direction="row" spacing={1.25} sx={{ alignItems: 'center' }}>
              <ShieldCheck size={18} color={theme.palette.success.main} />
              <Typography sx={{ fontWeight: 720 }}>{label}</Typography>
            </Stack>
          ))}
        </Stack>
      </Box>

      <Box sx={{ display: 'grid', placeItems: 'center', px: 2.5, py: 6 }}>
        <Paper
          component="section"
          sx={{
            width: '100%',
            maxWidth: 460,
            p: { xs: 3, sm: 4 },
            border: '1px solid',
            borderColor: 'divider',
            borderRadius: 4,
            boxShadow: 6
          }}
        >
          <Stack component="form" spacing={3} onSubmit={handleSubmit}>
            <Stack spacing={1}>
              <Box
                sx={{
                  width: 44,
                  height: 44,
                  borderRadius: 2.5,
                  display: { xs: 'grid', lg: 'none' },
                  placeItems: 'center',
                  bgcolor: alpha(theme.palette.primary.main, 0.1),
                  color: 'primary.main'
                }}
              >
                <Building2 size={22} />
              </Box>
              <Typography variant="h4" component="h1">Sign in</Typography>
              <Typography variant="body2" color="text.secondary">
                Continue to your organization workspace.
              </Typography>
            </Stack>

            {signInMutation.isError ? (
              <Alert severity="error">Unable to sign in with the provided credentials.</Alert>
            ) : null}

            <TextField
              label="Email"
              name="email"
              type="email"
              autoComplete="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
              slotProps={{
                input: {
                  startAdornment: (
                    <InputAdornment position="start">
                      <Mail size={17} />
                    </InputAdornment>
                  )
                }
              }}
            />

            <TextField
              label="Password"
              name="password"
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
              slotProps={{
                input: {
                  startAdornment: (
                    <InputAdornment position="start">
                      <LockKeyhole size={17} />
                    </InputAdornment>
                  )
                }
              }}
            />

            <Button
              type="submit"
              variant="contained"
              size="large"
              startIcon={signInMutation.isPending ? <CircularProgress color="inherit" size={16} /> : <LogIn size={17} />}
              disabled={signInMutation.isPending}
            >
              Sign In
            </Button>
          </Stack>
        </Paper>
      </Box>
    </Box>
  );
}
