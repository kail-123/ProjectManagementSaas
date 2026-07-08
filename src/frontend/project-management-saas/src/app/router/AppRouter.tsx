import { lazy, Suspense } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import type { ReactNode } from 'react';
import { LoadingScreen } from '../../shared/ui/LoadingScreen';
import { permissions } from '../../shared/security/permissions';
import type { PermissionCode } from '../../shared/security/permissions';
import { PermissionRoute } from './PermissionRoute';
import { ProtectedRoute } from './ProtectedRoute';
import { routePaths } from './routePaths';

const SignInPage = lazy(() =>
  import('../../features/auth/ui/SignInPage').then((module) => ({ default: module.SignInPage }))
);
const AppHomePage = lazy(() => import('../../pages/AppHomePage').then((module) => ({ default: module.AppHomePage })));
const MyTasksPage = lazy(() => import('../../pages/MyTasksPage').then((module) => ({ default: module.MyTasksPage })));
const CalendarPage = lazy(() => import('../../pages/CalendarPage').then((module) => ({ default: module.CalendarPage })));
const NotificationsPage = lazy(() => import('../../pages/NotificationsPage').then((module) => ({ default: module.NotificationsPage })));
const ProfilePage = lazy(() => import('../../pages/ProfilePage').then((module) => ({ default: module.ProfilePage })));
const TenantsPage = lazy(() => import('../../pages/TenantsPage').then((module) => ({ default: module.TenantsPage })));
const SubscriptionsPage = lazy(() => import('../../pages/SubscriptionsPage').then((module) => ({ default: module.SubscriptionsPage })));
const OrganizationsPage = lazy(() => import('../../pages/management/OrganizationsPage').then((module) => ({ default: module.OrganizationsPage })));
const DepartmentsPage = lazy(() => import('../../pages/management/DepartmentsPage').then((module) => ({ default: module.DepartmentsPage })));
const TeamsPage = lazy(() => import('../../pages/management/TeamsPage').then((module) => ({ default: module.TeamsPage })));
const UsersPage = lazy(() => import('../../pages/management/UsersPage').then((module) => ({ default: module.UsersPage })));
const RolesPage = lazy(() => import('../../pages/management/RolesPage').then((module) => ({ default: module.RolesPage })));
const PermissionsPage = lazy(() => import('../../pages/management/PermissionsPage').then((module) => ({ default: module.PermissionsPage })));
const ClientsPage = lazy(() => import('../../pages/management/ClientsPage').then((module) => ({ default: module.ClientsPage })));
const ProjectsPage = lazy(() => import('../../pages/management/ProjectsPage').then((module) => ({ default: module.ProjectsPage })));
const ProjectDetailsPage = lazy(() => import('../../pages/management/ProjectDetailsPage').then((module) => ({ default: module.ProjectDetailsPage })));
const ProjectMembersPage = lazy(() => import('../../pages/management/ProjectMembersPage').then((module) => ({ default: module.ProjectMembersPage })));
const ProjectSettingsPage = lazy(() => import('../../pages/management/ProjectSettingsPage').then((module) => ({ default: module.ProjectSettingsPage })));
const ProjectDashboardPage = lazy(() => import('../../pages/management/ProjectDashboardPage').then((module) => ({ default: module.ProjectDashboardPage })));
const WorkItemsPage = lazy(() => import('../../pages/work/WorkItemsPage').then((module) => ({ default: module.WorkItemsPage })));
const WorkItemDetailsPage = lazy(() => import('../../pages/work/WorkItemDetailsPage').then((module) => ({ default: module.WorkItemDetailsPage })));
const AccessDeniedPage = lazy(() => import('../../pages/AccessDeniedPage').then((module) => ({ default: module.AccessDeniedPage })));
const NotFoundPage = lazy(() => import('../../pages/NotFoundPage').then((module) => ({ default: module.NotFoundPage })));
const AppLayout = lazy(() => import('../../shared/layout/AppLayout').then((module) => ({ default: module.AppLayout })));

interface AppRouteDefinition {
  index?: boolean;
  path?: string;
  permission?: PermissionCode;
  element: ReactNode;
}

const appRoutes: AppRouteDefinition[] = [
  { index: true, permission: permissions.dashboard.view, element: <AppHomePage /> },
  { path: 'my-tasks', permission: permissions.myTasks.view, element: <MyTasksPage /> },
  { path: 'calendar', permission: permissions.calendar.view, element: <CalendarPage /> },
  { path: 'notifications', permission: permissions.notifications.view, element: <NotificationsPage /> },
  { path: 'profile', permission: permissions.profile.view, element: <ProfilePage /> },
  { path: 'tenants', permission: permissions.tenants.view, element: <TenantsPage /> },
  { path: 'subscriptions', permission: permissions.subscriptions.view, element: <SubscriptionsPage /> },
  { path: 'organizations', permission: permissions.organizations.view, element: <OrganizationsPage /> },
  { path: 'departments', permission: permissions.departments.view, element: <DepartmentsPage /> },
  { path: 'teams', permission: permissions.teams.view, element: <TeamsPage /> },
  { path: 'users', permission: permissions.users.view, element: <UsersPage /> },
  { path: 'roles', permission: permissions.roles.view, element: <RolesPage /> },
  { path: 'permissions', permission: permissions.permissions.view, element: <PermissionsPage /> },
  { path: 'clients', permission: permissions.clients.view, element: <ClientsPage /> },
  { path: 'projects', permission: permissions.projects.view, element: <ProjectsPage /> },
  { path: 'projects/:projectId', permission: permissions.projects.view, element: <ProjectDetailsPage /> },
  { path: 'projects/:projectId/members', permission: permissions.projectMembers.view, element: <ProjectMembersPage /> },
  { path: 'projects/:projectId/settings', permission: permissions.projectSettings.view, element: <ProjectSettingsPage /> },
  { path: 'projects/:projectId/dashboard', permission: permissions.projects.dashboardView, element: <ProjectDashboardPage /> },
  { path: 'work-items', permission: permissions.workItems.view, element: <WorkItemsPage /> },
  { path: 'work-items/:workItemId', permission: permissions.workItems.view, element: <WorkItemDetailsPage /> }
];

export function AppRouter() {
  return (
    <Suspense fallback={<LoadingScreen label="Loading" />}>
      <Routes>
        <Route path={routePaths.root} element={<Navigate to={routePaths.app} replace />} />
        <Route path={routePaths.signIn} element={<SignInPage />} />
        <Route element={<ProtectedRoute />}>
          <Route path={routePaths.app} element={<AppLayout />}>
            <Route path="access-denied" element={<AccessDeniedPage />} />
            {appRoutes.map((route) => (
              <Route
                key={route.index ? 'index' : route.path}
                index={route.index}
                path={route.path}
                element={<PermissionRoute permission={route.permission}>{route.element}</PermissionRoute>}
              />
            ))}
          </Route>
        </Route>
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </Suspense>
  );
}
