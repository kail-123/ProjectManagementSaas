import {
  Box,
  Button,
  LinearProgress,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  Paper,
  Skeleton,
  Stack,
  Typography,
  useTheme
} from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import {
  ResponsiveContainer,
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Line,
  LineChart,
  Pie,
  PieChart,
  Tooltip as ChartTooltip,
  XAxis,
  YAxis
} from 'recharts';
import { ArrowRight, CalendarClock, CheckCircle2, Clock3, FolderKanban, ListChecks, Rocket, TrendingUp } from 'lucide-react';
import type { ReactNode } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import { routePaths } from '../app/router/routePaths';
import { useAuthStore } from '../features/auth/model/authStore';
import { usePermissions } from '../features/auth/model/usePermissions';
import { managementApi } from '../shared/management/managementApi';
import type { Project, ProjectPriority, ProjectStatus } from '../shared/management/managementTypes';
import { permissions } from '../shared/security/permissions';
import { MetricCard } from '../shared/ui/MetricCard';
import { PageHeader } from '../shared/ui/PageHeader';
import { EmptyState } from '../shared/ui/StateBlocks';
import { formatDate, priorityChip, projectStatusChip } from './management/pageUtils';

export function AppHomePage() {
  const theme = useTheme();
  const { hasPermission } = usePermissions();
  const isSystemAdministrator = useAuthStore((state) => state.user?.roles.includes('SystemAdministrator') ?? false);
  const canViewDashboard = hasPermission(permissions.dashboard.view);
  const canViewProjects = hasPermission(permissions.projects.view);
  const canViewUsers = hasPermission(permissions.users.view);
  const canViewTeams = hasPermission(permissions.teams.view);
  const canViewClients = hasPermission(permissions.clients.view);
  const canViewMyTasks = hasPermission(permissions.myTasks.view);
  const canViewCalendar = hasPermission(permissions.calendar.view);
  const canViewNotifications = hasPermission(permissions.notifications.view);
  const projectsQuery = useQuery({
    queryKey: ['dashboard', 'projects'],
    queryFn: () => isSystemAdministrator
      ? managementApi.projects.list({ pageNumber: 1, pageSize: 200, sortBy: 'createdOn', sortDirection: 'desc' })
      : managementApi.projects.myProjects({ pageNumber: 1, pageSize: 200, sortBy: 'createdOn', sortDirection: 'desc' }),
    enabled: canViewDashboard
  });
  const usersQuery = useQuery({
    queryKey: ['dashboard', 'users'],
    queryFn: () => managementApi.users.list({ pageNumber: 1, pageSize: 200, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canViewUsers
  });
  const teamsQuery = useQuery({
    queryKey: ['dashboard', 'teams'],
    queryFn: () => managementApi.teams.list({ pageNumber: 1, pageSize: 200, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canViewTeams
  });
  const clientsQuery = useQuery({
    queryKey: ['dashboard', 'clients'],
    queryFn: () => managementApi.clients.list({ pageNumber: 1, pageSize: 200, sortBy: 'name', sortDirection: 'asc' }),
    enabled: canViewClients
  });

  const projects = projectsQuery.data?.items ?? [];
  const completedProjects = projects.filter((project) => project.status === 'Completed').length;
  const activeProjects = projects.filter((project) => project.status === 'Active').length;
  const overdueProjects = projects.filter(isProjectOverdue).length;
  const sprintProgress = projects.length > 0 ? Math.round((completedProjects / projects.length) * 100) : 0;
  const velocity = projects.reduce((total, project) => total + project.teamIds.length + project.memberCount, 0);
  const isLoading = projectsQuery.isLoading || usersQuery.isLoading || teamsQuery.isLoading || clientsQuery.isLoading;

  const statusData = buildStatusData(projects);
  const priorityData = buildPriorityData(projects);
  const timelineData = buildTimelineData(projects);
  const workloadData = buildWorkloadData(projects);
  const upcomingProjects = projects
    .filter((project) => project.endDate)
    .sort((left, right) => String(left.endDate).localeCompare(String(right.endDate)))
    .slice(0, 5);

  if (!canViewDashboard) {
    return (
      <Stack spacing={3}>
        <PageHeader
          eyebrow="Workspace"
          title="Dashboard"
          description="Your current workspace overview."
          actions={(
            <>
              {canViewMyTasks ? <Button component={RouterLink} to={routePaths.myTasks} variant="contained" endIcon={<ArrowRight size={16} />}>My Tasks</Button> : null}
              {canViewCalendar ? <Button component={RouterLink} to={routePaths.calendar} variant="outlined">Calendar</Button> : null}
              {canViewNotifications ? <Button component={RouterLink} to={routePaths.notifications} variant="outlined">Notifications</Button> : null}
            </>
          )}
        />

        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: 'repeat(3, 1fr)' }, gap: 2 }}>
          {canViewMyTasks ? <MetricCard title="My Tasks" value={0} detail="Assigned tasks" icon={<ListChecks size={20} />} /> : null}
          {canViewCalendar ? <MetricCard title="Calendar" value={0} detail="Events today" icon={<CalendarClock size={20} />} tone="info" /> : null}
          {canViewNotifications ? <MetricCard title="Notifications" value={0} detail="Unread alerts" icon={<Clock3 size={20} />} tone="primary" /> : null}
        </Box>

        <Paper sx={{ p: 3, border: '1px solid', borderColor: 'divider', borderRadius: 3 }}>
          <EmptyState title="No dashboard activity" />
        </Paper>
      </Stack>
    );
  }

  return (
    <Stack spacing={3}>
      <PageHeader
        eyebrow="Workspace"
        title="Command Center"
        description="Portfolio health, delivery momentum, workload, and upcoming commitments."
        actions={(
          <>
            {canViewProjects ? <Button component={RouterLink} to={routePaths.projects} variant="contained" endIcon={<ArrowRight size={16} />}>Open Projects</Button> : null}
            {canViewClients ? <Button component={RouterLink} to={routePaths.clients} variant="outlined">Clients</Button> : null}
          </>
        )}
      />

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, 1fr)', xl: 'repeat(4, 1fr)' }, gap: 2 }}>
        {isLoading ? <MetricSkeleton count={4} /> : (
          <>
            <MetricCard title="Total Projects" value={projectsQuery.data?.totalCount ?? 0} detail={`${activeProjects} currently active`} icon={<FolderKanban size={20} />} />
            <MetricCard title="Open Tasks" value={0} detail="No task activity recorded" icon={<ListChecks size={20} />} tone="info" />
            <MetricCard title="Completed" value={completedProjects} detail={`${sprintProgress}% portfolio completion`} icon={<CheckCircle2 size={20} />} tone="success" />
            <MetricCard title="Overdue" value={overdueProjects} detail="Projects past end date" icon={<Clock3 size={20} />} tone={overdueProjects > 0 ? 'error' : 'primary'} />
          </>
        )}
      </Box>

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', xl: '1.4fr 1fr' }, gap: 2 }}>
        <ChartCard title="Project Momentum" subtitle="Projects created across the current workspace">
          <ResponsiveContainer width="100%" height={280}>
            <AreaChart data={timelineData}>
              <defs>
                <linearGradient id="momentum" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor={theme.palette.primary.main} stopOpacity={0.35} />
                  <stop offset="95%" stopColor={theme.palette.primary.main} stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid stroke={theme.palette.divider} vertical={false} />
              <XAxis dataKey="month" stroke={theme.palette.text.secondary} />
              <YAxis stroke={theme.palette.text.secondary} allowDecimals={false} />
              <ChartTooltip />
              <Area type="monotone" dataKey="projects" stroke={theme.palette.primary.main} fill="url(#momentum)" strokeWidth={3} />
            </AreaChart>
          </ResponsiveContainer>
        </ChartCard>

        <Paper sx={{ p: 2.5, border: '1px solid', borderColor: 'divider', borderRadius: 3 }}>
          <Stack spacing={2}>
            <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center' }}>
              <Box>
                <Typography variant="h6">Sprint Progress</Typography>
                <Typography variant="body2" color="text.secondary">Based on completed projects in the current portfolio.</Typography>
              </Box>
              <Rocket size={22} color={theme.palette.primary.main} />
            </Stack>
            <Typography variant="h2">{sprintProgress}%</Typography>
            <LinearProgress variant="determinate" value={sprintProgress} sx={{ height: 10, borderRadius: 999 }} />
            <Stack direction="row" spacing={2}>
              <MiniMetric label="Velocity" value={velocity} icon={<TrendingUp size={16} />} />
              <MiniMetric label="Teams" value={teamsQuery.data?.totalCount ?? 0} icon={<Rocket size={16} />} />
              <MiniMetric label="Clients" value={clientsQuery.data?.totalCount ?? 0} icon={<FolderKanban size={16} />} />
            </Stack>
          </Stack>
        </Paper>
      </Box>

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', lg: '1fr 1fr', xl: '1fr 1fr 1fr' }, gap: 2 }}>
        <ChartCard title="Status Mix" subtitle="Portfolio distribution by status">
          <ResponsiveContainer width="100%" height={240}>
            <PieChart>
              <Pie data={statusData} dataKey="value" nameKey="name" outerRadius={82} innerRadius={48} paddingAngle={4}>
                {statusData.map((entry, index) => <Cell key={entry.name} fill={chartColors[index % chartColors.length]} />)}
              </Pie>
              <ChartTooltip />
            </PieChart>
          </ResponsiveContainer>
        </ChartCard>

        <ChartCard title="Priority Load" subtitle="Projects grouped by priority">
          <ResponsiveContainer width="100%" height={240}>
            <BarChart data={priorityData}>
              <CartesianGrid stroke={theme.palette.divider} vertical={false} />
              <XAxis dataKey="name" stroke={theme.palette.text.secondary} />
              <YAxis stroke={theme.palette.text.secondary} allowDecimals={false} />
              <ChartTooltip />
              <Bar dataKey="value" fill={theme.palette.secondary.main} radius={[8, 8, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>

        <ChartCard title="Team Workload" subtitle="Member and team concentration by project">
          <ResponsiveContainer width="100%" height={240}>
            <LineChart data={workloadData}>
              <CartesianGrid stroke={theme.palette.divider} vertical={false} />
              <XAxis dataKey="name" stroke={theme.palette.text.secondary} />
              <YAxis stroke={theme.palette.text.secondary} allowDecimals={false} />
              <ChartTooltip />
              <Line type="monotone" dataKey="workload" stroke={theme.palette.primary.main} strokeWidth={3} dot={{ r: 3 }} />
            </LineChart>
          </ResponsiveContainer>
        </ChartCard>
      </Box>

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', xl: '1.2fr 1fr 1fr' }, gap: 2 }}>
        <Panel title="Recent Projects">
          {projects.length === 0 ? (
            <EmptyState title="No projects yet" description="Create projects to populate portfolio health and delivery metrics." />
          ) : (
            <List disablePadding>
              {projects.slice(0, 5).map((project) => (
                <ListItem key={project.id} disableGutters secondaryAction={priorityChip(project.priority)}>
                  <ListItemAvatar>
                    <Box sx={{ width: 38, height: 38, borderRadius: 2.25, display: 'grid', placeItems: 'center', bgcolor: 'action.hover' }}>
                      <FolderKanban size={18} />
                    </Box>
                  </ListItemAvatar>
                  <ListItemText
                    primary={<Typography sx={{ fontWeight: 760 }}>{project.name}</Typography>}
                    secondary={`${project.organizationName} - ${project.clientName ?? 'Internal'}`}
                  />
                  {projectStatusChip(project.status)}
                </ListItem>
              ))}
            </List>
          )}
        </Panel>

        <Panel title="Recent Tasks">
          <EmptyState title="No task activity" description="No task records are available for this workspace." />
        </Panel>

        <Panel title="Upcoming Deadlines">
          {upcomingProjects.length === 0 ? (
            <EmptyState title="No deadlines" description="Projects with end dates will appear here." />
          ) : (
            <List disablePadding>
              {upcomingProjects.map((project) => (
                <ListItem key={project.id} disableGutters>
                  <ListItemAvatar>
                    <Box sx={{ width: 38, height: 38, borderRadius: 2.25, display: 'grid', placeItems: 'center', bgcolor: 'action.hover' }}>
                      <CalendarClock size={18} />
                    </Box>
                  </ListItemAvatar>
                  <ListItemText primary={<Typography sx={{ fontWeight: 760 }}>{project.name}</Typography>} secondary={formatDate(project.endDate)} />
                </ListItem>
              ))}
            </List>
          )}
        </Panel>
      </Box>
    </Stack>
  );
}

function ChartCard({ title, subtitle, children }: { title: string; subtitle: string; children: ReactNode }) {
  return (
    <Paper sx={{ p: 2.5, border: '1px solid', borderColor: 'divider', borderRadius: 3 }}>
      <Typography variant="h6">{title}</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>{subtitle}</Typography>
      {children}
    </Paper>
  );
}

function Panel({ title, children }: { title: string; children: ReactNode }) {
  return (
    <Paper sx={{ p: 2.5, border: '1px solid', borderColor: 'divider', borderRadius: 3, minHeight: 300 }}>
      <Typography variant="h6" sx={{ mb: 1.5 }}>{title}</Typography>
      {children}
    </Paper>
  );
}

function MiniMetric({ label, value, icon }: { label: string; value: number; icon: ReactNode }) {
  return (
    <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
      {icon}
      <Box>
        <Typography variant="caption" color="text.secondary">{label}</Typography>
        <Typography variant="subtitle2">{value}</Typography>
      </Box>
    </Stack>
  );
}

function MetricSkeleton({ count }: { count: number }) {
  return Array.from({ length: count }).map((_, index) => (
    <Skeleton key={index} variant="rounded" height={132} />
  ));
}

function isProjectOverdue(project: Project) {
  if (!project.endDate || ['Completed', 'Cancelled', 'Archived'].includes(project.status)) {
    return false;
  }

  return new Date(`${project.endDate}T00:00:00`) < new Date(new Date().toDateString());
}

function buildStatusData(projects: Project[]) {
  const statuses: ProjectStatus[] = ['Planning', 'Active', 'OnHold', 'Completed', 'Cancelled', 'Archived'];
  return statuses
    .map((status) => ({ name: status, value: projects.filter((project) => project.status === status).length }))
    .filter((entry) => entry.value > 0);
}

function buildPriorityData(projects: Project[]) {
  const priorities: ProjectPriority[] = ['Low', 'Medium', 'High', 'Critical'];
  return priorities
    .map((priority) => ({ name: priority, value: projects.filter((project) => project.priority === priority).length }));
}

function buildTimelineData(projects: Project[]) {
  const formatter = new Intl.DateTimeFormat(undefined, { month: 'short' });
  const months = Array.from({ length: 6 }).map((_, index) => {
    const date = new Date();
    date.setMonth(date.getMonth() - (5 - index));
    return {
      month: formatter.format(date),
      monthIndex: date.getMonth(),
      year: date.getFullYear(),
      projects: 0
    };
  });

  projects.forEach((project) => {
    const date = new Date(project.createdOn);
    const bucket = months.find((month) => month.monthIndex === date.getMonth() && month.year === date.getFullYear());
    if (bucket) {
      bucket.projects += 1;
    }
  });

  return months.map(({ month, projects }) => ({ month, projects }));
}

function buildWorkloadData(projects: Project[]) {
  return projects.slice(0, 8).map((project) => ({
    name: project.code,
    workload: project.memberCount + project.teamIds.length
  }));
}

const chartColors = ['#4f46e5', '#0891b2', '#16a34a', '#d97706', '#dc2626', '#64748b'];
