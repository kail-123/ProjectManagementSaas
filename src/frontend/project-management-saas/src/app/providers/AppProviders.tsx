import { CssBaseline, ThemeProvider } from '@mui/material';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { useMemo } from 'react';
import type { PropsWithChildren } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { AuthSessionBootstrap } from '../../features/auth/ui/AuthSessionBootstrap';
import { NotificationRealtimeProvider } from '../../features/notifications/NotificationRealtimeProvider';
import { environment } from '../../shared/config/environment';
import { queryClient } from '../../shared/query/queryClient';
import { ToastProvider } from '../../shared/ui/ToastProvider';
import { createAppTheme } from '../theme/theme';
import { useThemeModeStore } from '../theme/themeModeStore';

export function AppProviders({ children }: PropsWithChildren) {
  const mode = useThemeModeStore((state) => state.mode);
  const theme = useMemo(() => createAppTheme(mode), [mode]);

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <QueryClientProvider client={queryClient}>
        <ToastProvider>
          <BrowserRouter>
            <AuthSessionBootstrap>
              <NotificationRealtimeProvider>{children}</NotificationRealtimeProvider>
            </AuthSessionBootstrap>
          </BrowserRouter>
        </ToastProvider>
        {environment.enableQueryDevtools ? <ReactQueryDevtools initialIsOpen={false} /> : null}
      </QueryClientProvider>
    </ThemeProvider>
  );
}
