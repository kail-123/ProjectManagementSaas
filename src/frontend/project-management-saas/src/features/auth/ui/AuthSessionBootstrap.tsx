import type { PropsWithChildren } from 'react';
import { useEffect, useRef } from 'react';
import { LoadingScreen } from '../../../shared/ui/LoadingScreen';
import { authApi } from '../api/authApi';
import { useAuthStore } from '../model/authStore';

export function AuthSessionBootstrap({ children }: PropsWithChildren) {
  const accessToken = useAuthStore((state) => state.accessToken);
  const clearSession = useAuthStore((state) => state.clearSession);
  const isInitialized = useAuthStore((state) => state.isInitialized);
  const setCurrentUser = useAuthStore((state) => state.setCurrentUser);
  const setSession = useAuthStore((state) => state.setSession);
  const hasBootstrapped = useRef(false);

  useEffect(() => {
    if (hasBootstrapped.current) {
      return;
    }

    hasBootstrapped.current = true;

    if (accessToken) {
      void authApi
        .getCurrentUser()
        .then(setCurrentUser)
        .catch(() => authApi.refreshSession().then(setSession).catch(clearSession));
      return;
    }

    void authApi
      .refreshSession()
      .then(setSession)
      .catch(clearSession);
  }, [accessToken, clearSession, setCurrentUser, setSession]);

  if (!isInitialized) {
    return <LoadingScreen label="Securing session" />;
  }

  return children;
}
