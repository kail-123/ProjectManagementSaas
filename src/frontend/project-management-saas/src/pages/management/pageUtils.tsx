import { Chip } from '@mui/material';
import type { ReactNode } from 'react';
import type { ProjectPriority, ProjectStatus } from '../../shared/management/managementTypes';

export function statusChip(isActive: boolean): ReactNode {
  return <Chip size="small" color={isActive ? 'success' : 'default'} label={isActive ? 'Active' : 'Inactive'} />;
}

export function optional(value: string | null | undefined) {
  return value?.trim() ? value : '-';
}

export function enumLabel(value: string | null | undefined) {
  if (!value) {
    return '-';
  }

  const labels: Record<string, string> = {
    OnHold: 'On Hold',
    Qa: 'QA',
    ProjectManager: 'Project Manager',
    BusinessAnalyst: 'Business Analyst',
    UiUx: 'UI UX'
  };

  return labels[value] ?? value;
}

export function projectStatusChip(status: ProjectStatus): ReactNode {
  const color = status === 'Active'
    ? 'success'
    : status === 'OnHold'
      ? 'warning'
      : status === 'Completed'
        ? 'primary'
        : status === 'Cancelled'
          ? 'error'
          : 'default';

  return <Chip size="small" color={color} label={enumLabel(status)} />;
}

export function priorityChip(priority: ProjectPriority): ReactNode {
  const color = priority === 'Critical'
    ? 'error'
    : priority === 'High'
      ? 'warning'
      : priority === 'Medium'
        ? 'info'
        : 'default';

  return <Chip size="small" color={color} label={priority} />;
}

export function formatDate(value: string | null | undefined) {
  return value ? new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' }).format(new Date(`${value}T00:00:00`)) : '-';
}

export function formatDateTime(value: string | null | undefined) {
  return value ? new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value)) : '-';
}

export function formatCurrency(value: number | null | undefined) {
  return typeof value === 'number'
    ? new Intl.NumberFormat(undefined, { maximumFractionDigits: 2 }).format(value)
    : '-';
}
