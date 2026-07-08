import type { AlertColor } from '@mui/material';
import { createContext, useContext } from 'react';

export interface ToastContextValue {
  showToast: (message: string, severity?: AlertColor) => void;
}

export const ToastContext = createContext<ToastContextValue | null>(null);

export function useToast() {
  const context = useContext(ToastContext);

  if (!context) {
    throw new Error('useToast must be used within ToastProvider.');
  }

  return context;
}
