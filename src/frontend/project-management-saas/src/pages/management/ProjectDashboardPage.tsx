import { Box, Button, LinearProgress, List, ListItem, ListItemText, Paper, Skeleton, Stack, Typography } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { ArrowLeft, CalendarClock, CheckCircle2, History, Users } from 'lucide-react';
import type { ReactNode } from 'react';
import { Link as RouterLink, Navigate, useParams } from 'react-router-dom';
import { routePaths } from '../../app/router/routePaths';
import { managementApi } from '../../shared/management/managementApi';
import { MetricCard as SummaryMetricCard } from '../../shared/ui/MetricCard';
import { PageHeader } from '../../shared/ui/PageHeader';
import { enumLabel, formatCurrency, formatDate, formatDateTime, priorityChip, projectStatusChip } from './pageUtils';

export function ProjectDashboardPage() {
  const { projectId } = useParams();
  const dashboardQuery = useQuery({
    queryKey: ['projects', projectId, 'dashboard'],
    queryFn: () => managementApi.projects.dashboard(projectId!),
    enabled: Boolean(projectId)
  });

  if (!projectId) {
    return <Navigate to={routePaths.projects} replace />;
  }

  if (dashboardQuery.isLoading) {
    return <Skeleton variant="rounded" height={420} />;
  }

  if (!dashboardQuery.data) {
    return <Typography color="error">Project dashboard was not found.</Typography>;
  }

  const dashboard = dashboardQuery.data;
  const summary = dashboard.summary;

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Project Dashboard"
        description={summary.name}
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.app },
          { label: 'Projects', path: routePaths.projects },
          { label: summary.name, path: routePaths.projectDetails(projectId) },
          { label: 'Dashboard' }
        ]}
        actions={<Button component={RouterLink} to={routePaths.projectDetails(projectId)} startIcon={<ArrowLeft size={16} />} variant="outlined">Project Details</Button>}
      />

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'repeat(4, minmax(0, 1fr))' }, gap: 2 }}>
        <SummaryMetricCard title="Members" value={dashboard.memberCount} detail="Assigned members" icon={<Users size={20} />} />
        <SummaryMetricCard title="Teams" value={dashboard.teamCount} detail="Attached delivery teams" icon={<Users size={20} />} tone="info" />
        <SummaryMetricCard title="Tasks" value={dashboard.taskCount} detail="Current task count" icon={<CheckCircle2 size={20} />} tone="primary" />
        <SummaryMetricCard title="Completion" value={`${dashboard.completionPercentage}%`} detail="Project completion" icon={<CheckCircle2 size={20} />} tone="success" />
      </Box>

      <Paper sx={{ p: 3, border: '1px solid', borderColor: 'divider', borderRadius: 3 }}>
        <Stack spacing={2}>
          <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>
            {projectStatusChip(summary.status)}
            {priorityChip(summary.priority)}
          </Stack>
          <LinearProgress variant="determinate" value={Math.min(100, Math.max(0, dashboard.completionPercentage))} />
          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'repeat(3, minmax(0, 1fr))' }, gap: 2 }}>
            <Info label="Organization" value={summary.organizationName} />
            <Info label="Client" value={summary.clientName ?? '-'} />
            <Info label="Project Manager" value={summary.projectManagerName ?? '-'} />
            <Info label="Start Date" value={formatDate(summary.startDate)} />
            <Info label="End Date" value={formatDate(summary.endDate)} />
            <Info label="Budget" value={formatCurrency(summary.estimatedBudget)} />
          </Box>
        </Stack>
      </Paper>

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', lg: 'repeat(3, minmax(0, 1fr))' }, gap: 2 }}>
        <ListPanel title="Upcoming Deadlines" icon={<CalendarClock size={18} />}>
          {dashboard.upcomingDeadlines.length === 0 ? (
            <ListItem><ListItemText primary="-" /></ListItem>
          ) : dashboard.upcomingDeadlines.map((deadline) => (
            <ListItem key={`${deadline.category}-${deadline.dueDate}`}>
              <ListItemText primary={deadline.title} secondary={`${deadline.category} - ${formatDate(deadline.dueDate)}`} />
            </ListItem>
          ))}
        </ListPanel>

        <ListPanel title="Recent Members" icon={<Users size={18} />}>
          {dashboard.recentMembers.length === 0 ? (
            <ListItem><ListItemText primary="-" /></ListItem>
          ) : dashboard.recentMembers.map((member) => (
            <ListItem key={member.userId}>
              <ListItemText primary={member.displayName} secondary={`${enumLabel(member.roleInProject)} - ${formatDateTime(member.joinedDate)}`} />
            </ListItem>
          ))}
        </ListPanel>

        <ListPanel title="Activity" icon={<History size={18} />}>
          {dashboard.activity.length === 0 ? (
            <ListItem><ListItemText primary="-" /></ListItem>
          ) : dashboard.activity.map((activity) => (
            <ListItem key={`${activity.entityName}-${activity.action}-${activity.changedOn}`}>
              <ListItemText primary={`${activity.entityName} ${activity.action}`} secondary={`${activity.changedBy} - ${formatDateTime(activity.changedOn)}`} />
            </ListItem>
          ))}
        </ListPanel>
      </Box>
    </Stack>
  );
}

function ListPanel({ title, icon, children }: { title: string; icon: ReactNode; children: ReactNode }) {
  return (
    <Paper sx={{ p: 2, border: '1px solid', borderColor: 'divider', borderRadius: 3 }}>
      <Stack direction="row" spacing={1} sx={{ alignItems: 'center', mb: 1 }}>
        <Box sx={{ color: 'primary.main' }}>{icon}</Box>
        <Typography sx={{ fontWeight: 800 }}>{title}</Typography>
      </Stack>
      <List dense>{children}</List>
    </Paper>
  );
}

function Info({ label, value }: { label: string; value: string }) {
  return (
    <Box>
      <Typography variant="caption" color="text.secondary">{label}</Typography>
      <Typography sx={{ fontWeight: 700, wordBreak: 'break-word' }}>{value}</Typography>
    </Box>
  );
}
