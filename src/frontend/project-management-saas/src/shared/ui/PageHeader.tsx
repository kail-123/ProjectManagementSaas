import { Box, Breadcrumbs, Button, Link, Stack, Typography } from '@mui/material';
import type { ReactNode } from 'react';
import type { ComponentProps } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import { motion } from 'framer-motion';
import { ChevronRight } from 'lucide-react';
import { routePaths } from '../../app/router/routePaths';

interface BreadcrumbItem {
  label: string;
  path?: string;
}

interface PageHeaderProps {
  title: string;
  description?: string;
  breadcrumbs?: BreadcrumbItem[];
  actions?: ReactNode;
  eyebrow?: string;
}

export function PageHeader({ title, description, breadcrumbs, actions, eyebrow }: PageHeaderProps) {
  const items = breadcrumbs ?? [{ label: 'Dashboard', path: routePaths.app }, { label: title }];

  return (
    <Box
      component={motion.div}
      initial={{ opacity: 0, y: 8 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.18, ease: 'easeOut' }}
      sx={{ mb: 3 }}
    >
      <Breadcrumbs separator={<ChevronRight size={14} />} sx={{ mb: 1 }}>
        {items.map((item) => item.path ? (
          <Link key={item.label} component={RouterLink} to={item.path} color="text.secondary" underline="hover" variant="caption">
            {item.label}
          </Link>
        ) : (
          <Typography key={item.label} variant="caption" color="text.primary" sx={{ fontWeight: 700 }}>
            {item.label}
          </Typography>
        ))}
      </Breadcrumbs>
      <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} sx={{ justifyContent: 'space-between', alignItems: { md: 'flex-end' } }}>
        <Box sx={{ minWidth: 0 }}>
          {eyebrow ? (
            <Typography variant="overline" color="primary.main">{eyebrow}</Typography>
          ) : null}
          <Typography variant="h3" component="h1" sx={{ fontWeight: 850 }}>
            {title}
          </Typography>
          {description ? (
            <Typography color="text.secondary" sx={{ mt: 0.75, maxWidth: 760 }}>
              {description}
            </Typography>
          ) : null}
        </Box>
        {actions ? (
          <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap', gap: 1 }}>
            {actions}
          </Stack>
        ) : null}
      </Stack>
    </Box>
  );
}

export function HeaderActionButton({ children, ...props }: ComponentProps<typeof Button>) {
  return (
    <Button variant="contained" {...props}>
      {children}
    </Button>
  );
}
