import { Box, Paper, Stack, Typography, alpha, useTheme } from '@mui/material';
import type { ReactNode } from 'react';
import { motion } from 'framer-motion';

interface MetricCardProps {
  title: string;
  value: string | number;
  detail?: string;
  icon?: ReactNode;
  tone?: 'primary' | 'success' | 'warning' | 'error' | 'info';
}

export function MetricCard({ title, value, detail, icon, tone = 'primary' }: MetricCardProps) {
  const theme = useTheme();
  const color = theme.palette[tone].main;

  return (
    <Paper
      component={motion.div}
      whileHover={{ y: -2 }}
      transition={{ duration: 0.16 }}
      sx={{
        p: 2.5,
        border: '1px solid',
        borderColor: 'divider',
        borderRadius: 3,
        minHeight: 132,
        background: `linear-gradient(180deg, ${alpha(color, theme.palette.mode === 'dark' ? 0.12 : 0.08)}, transparent 80%), ${theme.palette.background.paper}`
      }}
    >
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start', gap: 2 }}>
        <Box>
          <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 800, textTransform: 'uppercase' }}>
            {title}
          </Typography>
          <Typography variant="h3" sx={{ mt: 1, fontWeight: 850 }}>
            {value}
          </Typography>
        </Box>
        {icon ? (
          <Box sx={{ width: 40, height: 40, borderRadius: 2.5, display: 'grid', placeItems: 'center', color, bgcolor: alpha(color, 0.12) }}>
            {icon}
          </Box>
        ) : null}
      </Stack>
      {detail ? (
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1.5 }}>
          {detail}
        </Typography>
      ) : null}
    </Paper>
  );
}
