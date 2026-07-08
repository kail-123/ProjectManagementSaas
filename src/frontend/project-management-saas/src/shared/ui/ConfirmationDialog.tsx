import { Box, Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Stack } from '@mui/material';
import { TriangleAlert } from 'lucide-react';

interface ConfirmationDialogProps {
  open: boolean;
  title: string;
  description: string;
  confirmLabel?: string;
  confirmColor?: 'primary' | 'error';
  onCancel: () => void;
  onConfirm: () => void;
  loading?: boolean;
}

export function ConfirmationDialog({
  open,
  title,
  description,
  confirmLabel = 'Confirm',
  confirmColor = 'primary',
  onCancel,
  onConfirm,
  loading = false
}: ConfirmationDialogProps) {
  return (
    <Dialog open={open} onClose={loading ? undefined : onCancel} fullWidth maxWidth="xs">
      <DialogTitle>
        <Stack direction="row" spacing={1.5} sx={{ alignItems: 'center' }}>
          <Box sx={{ width: 38, height: 38, borderRadius: 2.5, display: 'grid', placeItems: 'center', bgcolor: 'warning.light', color: 'warning.dark' }}>
            <TriangleAlert size={20} />
          </Box>
          {title}
        </Stack>
      </DialogTitle>
      <DialogContent>
        <DialogContentText>{description}</DialogContentText>
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2.5 }}>
        <Button onClick={onCancel} disabled={loading} variant="outlined">
          Cancel
        </Button>
        <Button
          onClick={onConfirm}
          color={confirmColor}
          variant="contained"
          disabled={loading}
          startIcon={loading ? <CircularProgress color="inherit" size={16} /> : undefined}
        >
          {confirmLabel}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
