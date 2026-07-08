import { Alert, Snackbar } from '@mui/material';
import type { AlertColor } from '@mui/material';
import type { PropsWithChildren } from 'react';
import { useCallback, useMemo, useState } from 'react';
import { ToastContext } from './toastContext';

interface ToastState {
  message: string;
  severity: AlertColor;
}

export function ToastProvider({ children }: PropsWithChildren) {
  const [toast, setToast] = useState<ToastState | null>(null);

  const showToast = useCallback((message: string, severity: AlertColor = 'success') => {
    setToast({ message, severity });
  }, []);

  const value = useMemo(() => ({ showToast }), [showToast]);

  return (
    <ToastContext.Provider value={value}>
      {children}
      <Snackbar
        open={toast !== null}
        autoHideDuration={5000}
        onClose={() => setToast(null)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        {toast ? (
          <Alert
            severity={toast.severity}
            variant="filled"
            onClose={() => setToast(null)}
            sx={{ borderRadius: 3, boxShadow: 6, alignItems: 'center' }}
          >
            {toast.message}
          </Alert>
        ) : undefined}
      </Snackbar>
    </ToastContext.Provider>
  );
}
