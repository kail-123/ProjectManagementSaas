import type { PaletteMode } from '@mui/material';
import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface ThemeModeState {
  mode: PaletteMode;
  toggleMode: () => void;
  setMode: (mode: PaletteMode) => void;
}

export const useThemeModeStore = create<ThemeModeState>()(
  persist(
    (set) => ({
      mode: 'light',
      toggleMode: () => set((state) => ({ mode: state.mode === 'light' ? 'dark' : 'light' })),
      setMode: (mode) => set({ mode })
    }),
    {
      name: 'pmsa-theme-mode'
    }
  )
);
