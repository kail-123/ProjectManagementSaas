import { create } from 'zustand';
import { createJSONStorage, persist } from 'zustand/middleware';
import type { AuthenticationResponse, CurrentUser } from './authTypes';

interface AuthState {
  accessToken: string | null;
  accessTokenExpiresAtUtc: string | null;
  user: CurrentUser | null;
  isAuthenticated: boolean;
  isInitialized: boolean;
  setSession: (session: AuthenticationResponse) => void;
  clearSession: () => void;
  markInitialized: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      accessTokenExpiresAtUtc: null,
      user: null,
      isAuthenticated: false,
      isInitialized: false,
      setSession: (session) =>
        set({
          accessToken: session.accessToken,
          accessTokenExpiresAtUtc: session.accessTokenExpiresAtUtc,
          user: session.user,
          isAuthenticated: true,
          isInitialized: true
        }),
      clearSession: () =>
        set({
          accessToken: null,
          accessTokenExpiresAtUtc: null,
          user: null,
          isAuthenticated: false,
          isInitialized: true
        }),
      markInitialized: () => set({ isInitialized: true })
    }),
    {
      name: 'project-management-saas-auth',
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({
        accessToken: state.accessToken,
        accessTokenExpiresAtUtc: state.accessTokenExpiresAtUtc,
        user: state.user,
        isAuthenticated: state.isAuthenticated
      }),
      onRehydrateStorage: () => (state) => {
        state?.markInitialized();
      }
    }
  )
);
