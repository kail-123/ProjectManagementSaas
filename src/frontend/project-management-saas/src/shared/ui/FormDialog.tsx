import { Box, Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle, IconButton, Stack, Typography } from '@mui/material';
import { X } from 'lucide-react';
import type { PropsWithChildren } from 'react';

interface FormDialogProps extends PropsWithChildren {
  open: boolean;
  title: string;
  submitLabel?: string;
  loading?: boolean;
  onClose: () => void;
  onSubmit: () => void;
}

export function FormDialog({
  open,
  title,
  submitLabel = 'Save',
  loading = false,
  onClose,
  onSubmit,
  children
}: FormDialogProps) {
  return (
    <Dialog open={open} onClose={loading ? undefined : onClose} fullWidth maxWidth="md" scroll="paper">
      <DialogTitle sx={{ px: 3, py: 2.25 }}>
        <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between', gap: 2 }}>
          <Box>
            <Typography variant="h5">{title}</Typography>
          </Box>
          <IconButton onClick={onClose} disabled={loading}>
            <X size={18} />
          </IconButton>
        </Stack>
      </DialogTitle>
      <DialogContent dividers sx={{ p: 3 }}>{children}</DialogContent>
      <DialogActions sx={{ px: 3, py: 2, bgcolor: 'background.default' }}>
        <Button onClick={onClose} disabled={loading} variant="outlined">
          Cancel
        </Button>
        <Button
          variant="contained"
          onClick={onSubmit}
          disabled={loading}
          startIcon={loading ? <CircularProgress color="inherit" size={16} /> : undefined}
        >
          {submitLabel}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
