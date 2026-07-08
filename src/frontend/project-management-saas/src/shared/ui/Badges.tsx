import { Chip, alpha, useTheme } from '@mui/material';

interface BadgeProps {
  label: string;
  tone?: 'default' | 'primary' | 'success' | 'warning' | 'error' | 'info';
}

export function StatusBadge({ label, tone = 'default' }: BadgeProps) {
  const theme = useTheme();
  const color = tone === 'default' ? theme.palette.text.secondary : theme.palette[tone].main;

  return (
    <Chip
      size="small"
      label={label}
      sx={{
        bgcolor: alpha(color, theme.palette.mode === 'dark' ? 0.18 : 0.1),
        color,
        border: `1px solid ${alpha(color, 0.2)}`,
        fontWeight: 800
      }}
    />
  );
}
