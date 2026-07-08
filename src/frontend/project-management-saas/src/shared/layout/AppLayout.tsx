import {
  Avatar,
  Badge,
  Box,
  Button,
  Chip,
  Collapse,
  Dialog,
  DialogContent,
  DialogTitle,
  Divider,
  Drawer,
  IconButton,
  InputAdornment,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Menu,
  MenuItem,
  Stack,
  TextField,
  Toolbar,
  Tooltip,
  Typography,
  alpha,
  useMediaQuery,
  useTheme
} from '@mui/material';
import { useMutation, useQuery } from '@tanstack/react-query';
import {
  Bell,
  Building2,
  CalendarDays,
  ChevronDown,
  ChevronRight,
  CircleHelp,
  Command,
  CreditCard,
  FolderKanban,
  LayoutDashboard,
  LifeBuoy,
  ListChecks,
  LogOut,
  KeyRound,
  Moon,
  PanelLeftClose,
  PanelLeftOpen,
  Search,
  Settings,
  ShieldCheck,
  Sparkles,
  Sun,
  UserCircle,
  UsersRound
} from 'lucide-react';
import type { LucideIcon } from 'lucide-react';
import { useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { Link as RouterLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { routePaths } from '../../app/router/routePaths';
import { designTokens } from '../../app/theme/theme';
import { useThemeModeStore } from '../../app/theme/themeModeStore';
import { authApi } from '../../features/auth/api/authApi';
import { useAuthStore } from '../../features/auth/model/authStore';
import { usePermissions } from '../../features/auth/model/usePermissions';
import { NotificationDrawer } from '../../features/notifications/NotificationCenter';
import { managementApi } from '../management/managementApi';
import { permissions } from '../security/permissions';
import type { PermissionCode } from '../security/permissions';

interface NavItem {
  label: string;
  icon: LucideIcon;
  permission?: PermissionCode;
  path?: string;
  disabled?: boolean;
  children?: NavItem[];
}

const navCatalog: NavItem[] = [
  { label: 'Dashboard', icon: LayoutDashboard, path: routePaths.app, permission: permissions.dashboard.view },
  { label: 'My Tasks', icon: ListChecks, path: routePaths.myTasks, permission: permissions.myTasks.view },
  { label: 'Calendar', icon: CalendarDays, path: routePaths.calendar, permission: permissions.calendar.view },
  { label: 'Notifications', icon: Bell, path: routePaths.notifications, permission: permissions.notifications.view },
  { label: 'Profile', icon: UserCircle, path: routePaths.profile, permission: permissions.profile.view },
  { label: 'Projects', icon: FolderKanban, path: routePaths.projects, permission: permissions.projects.view },
  { label: 'Work Items', icon: ListChecks, path: routePaths.workItems, permission: permissions.workItems.view },
  { label: 'Clients', icon: Building2, path: routePaths.clients, permission: permissions.clients.view },
  { label: 'Tenants', icon: Building2, path: routePaths.tenants, permission: permissions.tenants.view },
  { label: 'Subscriptions', icon: CreditCard, path: routePaths.subscriptions, permission: permissions.subscriptions.view },
  {
    label: 'Settings',
    icon: Settings,
    children: [
      { label: 'Organizations', icon: Building2, path: routePaths.organizations, permission: permissions.organizations.view },
      { label: 'Departments', icon: Building2, path: routePaths.departments, permission: permissions.departments.view },
      { label: 'Teams', icon: UsersRound, path: routePaths.teams, permission: permissions.teams.view },
      { label: 'Users', icon: UsersRound, path: routePaths.users, permission: permissions.users.view },
      { label: 'Roles', icon: ShieldCheck, path: routePaths.roles, permission: permissions.roles.view },
      { label: 'Permissions', icon: KeyRound, path: routePaths.permissions, permission: permissions.permissions.view }
    ]
  }
];

export function AppLayout() {
  const [collapsed, setCollapsed] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);
  const sidebarWidth = collapsed ? designTokens.sidebarCollapsedWidth : designTokens.sidebarWidth;

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
      <Sidebar
        collapsed={collapsed}
        onCollapse={() => setCollapsed((current) => !current)}
        mobileOpen={mobileOpen}
        onCloseMobile={() => setMobileOpen(false)}
      />
      <Box
        sx={{
          minHeight: '100vh',
          ml: { lg: `${sidebarWidth}px` },
          transition: 'margin-left 180ms ease'
        }}
      >
        <Topbar onOpenSidebar={() => setMobileOpen(true)} />
        <Box
          component="main"
          sx={{
            px: { xs: 2, sm: 3, xl: 4 },
            py: { xs: 2, md: 3 },
            maxWidth: 1680,
            mx: 'auto'
          }}
        >
          <Outlet />
        </Box>
      </Box>
    </Box>
  );
}

function Sidebar({
  collapsed,
  onCollapse,
  mobileOpen,
  onCloseMobile
}: {
  collapsed: boolean;
  onCollapse: () => void;
  mobileOpen: boolean;
  onCloseMobile: () => void;
}) {
  const theme = useTheme();
  const isDesktop = useMediaQuery(theme.breakpoints.up('lg'));
  const content = <SidebarContent collapsed={collapsed} onCollapse={onCollapse} onNavigate={onCloseMobile} />;

  if (!isDesktop) {
    return (
      <Drawer
        open={mobileOpen}
        onClose={onCloseMobile}
        variant="temporary"
        ModalProps={{ keepMounted: true }}
        slotProps={{ paper: { sx: sidebarPaperSx(false) } }}
      >
        {content}
      </Drawer>
    );
  }

  return (
    <Drawer
      open
      variant="permanent"
      slotProps={{ paper: { sx: sidebarPaperSx(collapsed) } }}
    >
      {content}
    </Drawer>
  );
}

function SidebarContent({
  collapsed,
  onCollapse,
  onNavigate
}: {
  collapsed: boolean;
  onCollapse: () => void;
  onNavigate: () => void;
}) {
  const [settingsOpen, setSettingsOpen] = useState(true);
  const location = useLocation();
  const { hasPermission } = usePermissions();
  const visibleNav = useMemo(() => filterNavItems(navCatalog, hasPermission), [hasPermission]);

  return (
    <Stack sx={{ minHeight: '100%', p: 1.5 }}>
      <Stack direction="row" sx={{ alignItems: 'center', minHeight: 52, px: 1, mb: 1 }}>
        <Box
          sx={{
            width: 36,
            height: 36,
            borderRadius: 2.5,
            display: 'grid',
            placeItems: 'center',
            color: 'white',
            background: 'linear-gradient(135deg, #4f46e5, #0891b2)'
          }}
        >
          <Sparkles size={18} />
        </Box>
        {!collapsed ? (
          <Box sx={{ ml: 1.5, minWidth: 0, flex: 1 }}>
            <Typography sx={{ fontWeight: 850, lineHeight: 1.1 }}>PMSA</Typography>
            <Typography variant="caption" color="text.secondary">Enterprise workspace</Typography>
          </Box>
        ) : null}
        <Tooltip title={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}>
          <IconButton size="small" onClick={onCollapse} sx={{ ml: 'auto', display: { xs: 'none', lg: 'inline-flex' } }}>
            {collapsed ? <PanelLeftOpen size={18} /> : <PanelLeftClose size={18} />}
          </IconButton>
        </Tooltip>
      </Stack>

      <List disablePadding sx={{ flex: 1 }}>
        {visibleNav.map((item) => {
          if (item.children) {
            const hasActiveChild = item.children.some((child) => child.path && isActivePath(location.pathname, child.path));
            return (
              <Box key={item.label}>
                <NavButton
                  item={item}
                  collapsed={collapsed}
                  active={hasActiveChild}
                  onClick={() => setSettingsOpen((current) => !current)}
                  trailing={!collapsed ? (settingsOpen ? <ChevronDown size={16} /> : <ChevronRight size={16} />) : undefined}
                />
                <Collapse in={!collapsed && settingsOpen} timeout={160} unmountOnExit>
                  <Stack sx={{ ml: 1.5, pl: 1.25, borderLeft: '1px solid', borderColor: 'divider' }}>
                    {item.children.map((child) => (
                      <NavButton
                        key={child.label}
                        item={child}
                        dense
                        collapsed={false}
                        active={Boolean(child.path && isActivePath(location.pathname, child.path))}
                        onNavigate={onNavigate}
                      />
                    ))}
                  </Stack>
                </Collapse>
              </Box>
            );
          }

          return (
            <NavButton
              key={item.label}
              item={item}
              collapsed={collapsed}
              active={Boolean(item.path && isActivePath(location.pathname, item.path))}
              onNavigate={onNavigate}
            />
          );
        })}
      </List>

      <Box sx={{ p: collapsed ? 0 : 1 }}>
        {!collapsed ? (
          <Stack
            spacing={1}
            sx={{
              p: 1.5,
              borderRadius: 3,
              border: '1px solid',
              borderColor: 'divider',
              bgcolor: 'background.paper'
            }}
          >
            <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
              <LifeBuoy size={16} />
              <Typography variant="subtitle2">Support</Typography>
            </Stack>
          </Stack>
        ) : (
          <Tooltip title="Support">
            <IconButton>
              <LifeBuoy size={18} />
            </IconButton>
          </Tooltip>
        )}
      </Box>
    </Stack>
  );
}

function NavButton({
  item,
  active,
  collapsed,
  dense,
  trailing,
  onClick,
  onNavigate
}: {
  item: NavItem;
  active: boolean;
  collapsed: boolean;
  dense?: boolean;
  trailing?: ReactNode;
  onClick?: () => void;
  onNavigate?: () => void;
}) {
  const Icon = item.icon;
  const button = (
    <ListItemButton
      component={item.path && !item.disabled ? RouterLink : 'button'}
      to={item.path && !item.disabled ? item.path : undefined}
      disabled={item.disabled}
      onClick={() => {
        onClick?.();
        if (item.path && !item.disabled) {
          onNavigate?.();
        }
      }}
      sx={(theme) => ({
        minHeight: dense ? 36 : 42,
        borderRadius: 2.25,
        px: collapsed ? 1 : dense ? 1.25 : 1.5,
        mb: 0.4,
        justifyContent: collapsed ? 'center' : 'flex-start',
        color: active ? 'primary.main' : 'text.secondary',
        bgcolor: active ? alpha(theme.palette.primary.main, theme.palette.mode === 'dark' ? 0.18 : 0.1) : 'transparent',
        '&:hover': {
          bgcolor: active ? alpha(theme.palette.primary.main, theme.palette.mode === 'dark' ? 0.22 : 0.12) : alpha(theme.palette.text.primary, 0.05)
        },
        '&.Mui-disabled': {
          opacity: 0.42
        }
      })}
    >
      <ListItemIcon sx={{ minWidth: collapsed ? 0 : 34, color: 'inherit' }}>
        <Icon size={dense ? 16 : 18} />
      </ListItemIcon>
      {!collapsed ? (
        <>
          <ListItemText
            primary={<Typography variant="body2" sx={{ fontWeight: active ? 780 : 650 }}>{item.label}</Typography>}
          />
          {item.disabled ? <Chip size="small" label="Soon" sx={{ height: 20 }} /> : trailing}
        </>
      ) : null}
    </ListItemButton>
  );

  return collapsed ? <Tooltip title={item.label} placement="right">{button}</Tooltip> : button;
}

function Topbar({ onOpenSidebar }: { onOpenSidebar: () => void }) {
  const theme = useTheme();
  const user = useAuthStore((state) => state.user);
  const clearSession = useAuthStore((state) => state.clearSession);
  const { hasPermission } = usePermissions();
  const navigate = useNavigate();
  const mode = useThemeModeStore((state) => state.mode);
  const toggleMode = useThemeModeStore((state) => state.toggleMode);
  const [userMenuAnchor, setUserMenuAnchor] = useState<HTMLElement | null>(null);
  const [commandOpen, setCommandOpen] = useState(false);
  const [notificationsOpen, setNotificationsOpen] = useState(false);
  const canSearch = hasPermission(permissions.search.global);
  const canViewNotifications = hasPermission(permissions.notifications.view);
  const canViewProfile = hasPermission(permissions.profile.view);

  const unreadCountQuery = useQuery({
    queryKey: ['notifications', 'unread-count'],
    queryFn: () => managementApi.notifications.unreadCount(),
    enabled: canViewNotifications,
    refetchInterval: 60000
  });

  const signOutMutation = useMutation({
    mutationFn: () => authApi.revokeSession(),
    onSettled: () => {
      clearSession();
      void navigate(routePaths.signIn, { replace: true });
    }
  });

  return (
    <Box
      component="header"
      sx={{
        position: 'sticky',
        top: 0,
        zIndex: theme.zIndex.appBar,
        height: designTokens.topbarHeight,
        display: 'flex',
        alignItems: 'center',
        borderBottom: '1px solid',
        borderColor: 'divider',
        backdropFilter: 'blur(18px)',
        bgcolor: alpha(theme.palette.background.default, theme.palette.mode === 'dark' ? 0.76 : 0.82)
      }}
    >
      <Toolbar sx={{ width: '100%', gap: 1.5, px: { xs: 2, md: 3 } }}>
        <IconButton onClick={onOpenSidebar} sx={{ display: { lg: 'none' } }}>
          <PanelLeftOpen size={20} />
        </IconButton>

        {canSearch ? <GlobalSearchButton onOpen={() => setCommandOpen(true)} /> : <Box sx={{ flexGrow: 1 }} />}

        <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center', ml: 'auto' }}>
          {canSearch ? (
            <Tooltip title="Command palette">
              <IconButton onClick={() => setCommandOpen(true)}>
                <Command size={19} />
              </IconButton>
            </Tooltip>
          ) : null}
          {canViewNotifications ? (
            <Tooltip title="Notifications">
              <IconButton onClick={() => setNotificationsOpen(true)}>
                <Badge color="error" badgeContent={unreadCountQuery.data ?? 0} max={99}>
                  <Bell size={19} />
                </Badge>
              </IconButton>
            </Tooltip>
          ) : null}
          <Tooltip title="Help">
            <IconButton>
              <CircleHelp size={19} />
            </IconButton>
          </Tooltip>
          <Tooltip title={mode === 'light' ? 'Switch to dark theme' : 'Switch to light theme'}>
            <IconButton onClick={toggleMode}>
              {mode === 'light' ? <Moon size={19} /> : <Sun size={19} />}
            </IconButton>
          </Tooltip>
          <Divider flexItem orientation="vertical" sx={{ mx: 0.5 }} />
          <Button
            color="inherit"
            onClick={(event) => setUserMenuAnchor(event.currentTarget)}
            sx={{ px: 0.75, minWidth: 0 }}
          >
            <Avatar sx={{ width: 34, height: 34, fontSize: 14 }}>
              {getInitials(user?.displayName ?? user?.email ?? 'U')}
            </Avatar>
            <Box sx={{ ml: 1, textAlign: 'left', display: { xs: 'none', md: 'block' } }}>
              <Typography variant="subtitle2" sx={{ lineHeight: 1.1 }}>{user?.displayName ?? 'Workspace user'}</Typography>
              <Typography variant="caption" color="text.secondary">{user?.email}</Typography>
            </Box>
            <ChevronDown size={16} style={{ marginLeft: 8 }} />
          </Button>
        </Stack>
      </Toolbar>

      <Menu anchorEl={userMenuAnchor} open={Boolean(userMenuAnchor)} onClose={() => setUserMenuAnchor(null)}>
        <MenuItem disabled>
          <Stack>
            <Typography variant="subtitle2">{user?.displayName ?? user?.email}</Typography>
            <Typography variant="caption" color="text.secondary">{user?.roles.join(', ') || 'Standard access'}</Typography>
          </Stack>
        </MenuItem>
        <Divider />
        {canViewProfile ? (
          <MenuItem
            onClick={() => {
              setUserMenuAnchor(null);
              void navigate(routePaths.profile);
            }}
          >
            <ListItemIcon><UserCircle size={18} /></ListItemIcon>
            Profile
          </MenuItem>
        ) : null}
        <MenuItem onClick={() => signOutMutation.mutate()} disabled={signOutMutation.isPending}>
          <ListItemIcon><LogOut size={18} /></ListItemIcon>
          Sign out
        </MenuItem>
      </Menu>

      {canSearch ? <CommandPalette open={commandOpen} onClose={() => setCommandOpen(false)} /> : null}
      {canViewNotifications ? <NotificationDrawer open={notificationsOpen} onClose={() => setNotificationsOpen(false)} /> : null}
    </Box>
  );
}

function GlobalSearchButton({ onOpen }: { onOpen: () => void }) {
  return (
    <Button
      variant="outlined"
      color="inherit"
      onClick={onOpen}
      sx={{
        height: 42,
        width: { xs: '100%', sm: 360, md: 460 },
        justifyContent: 'flex-start',
        color: 'text.secondary',
        bgcolor: 'background.paper'
      }}
      startIcon={<Search size={18} />}
    >
      Search projects, clients, work items, people...
    </Button>
  );
}

function CommandPalette({ open, onClose }: { open: boolean; onClose: () => void }) {
  const [search, setSearch] = useState('');
  const navigate = useNavigate();
  const { hasPermission } = usePermissions();
  const query = useQuery({
    queryKey: ['global-search', search],
    queryFn: () => managementApi.search.global({ pageNumber: 1, pageSize: 8, search, sortBy: 'title', sortDirection: 'asc' }),
    enabled: open && hasPermission(permissions.search.global)
  });

  const quickLinks = useMemo(
    () => [
      { label: 'Projects', path: routePaths.projects, permission: permissions.projects.view },
      { label: 'Work items', path: routePaths.workItems, permission: permissions.workItems.view },
      { label: 'Users', path: routePaths.users, permission: permissions.users.view },
      { label: 'Clients', path: routePaths.clients, permission: permissions.clients.view },
      { label: 'Organizations', path: routePaths.organizations, permission: permissions.organizations.view }
    ].filter((item) => hasPermission(item.permission)),
    [hasPermission]
  );

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle sx={{ pb: 1 }}>
        <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
          <Command size={19} />
          <Typography variant="h6">Command Palette</Typography>
        </Stack>
      </DialogTitle>
      <DialogContent sx={{ pt: 1 }}>
        <TextField
          autoFocus
          placeholder="Search workspace"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <Search size={18} />
                </InputAdornment>
              )
            }
          }}
        />
        <Stack spacing={1} sx={{ mt: 2 }}>
          {(query.data?.items.length ? query.data.items : quickLinks).map((item) => (
            <ListItemButton
              key={'id' in item ? item.id : item.label}
              onClick={() => {
                void navigate('url' in item ? item.url : item.path);
                onClose();
              }}
              sx={{ borderRadius: 2, border: '1px solid', borderColor: 'divider' }}
            >
              <ListItemIcon>
                {'type' in item && item.type === 'Project' ? <FolderKanban size={18} /> : <Search size={18} />}
              </ListItemIcon>
              <ListItemText
                primary={'title' in item ? item.title : item.label}
                secondary={'subtitle' in item ? item.subtitle : undefined}
              />
            </ListItemButton>
          ))}
        </Stack>
      </DialogContent>
    </Dialog>
  );
}

function sidebarPaperSx(collapsed: boolean) {
  return {
    width: collapsed ? designTokens.sidebarCollapsedWidth : designTokens.sidebarWidth,
    bgcolor: 'background.paper',
    borderRight: '1px solid',
    borderColor: 'divider',
    transition: 'width 180ms ease',
    overflowX: 'hidden'
  };
}

function isActivePath(pathname: string, path: string) {
  return path === routePaths.app ? pathname === path : pathname.startsWith(path);
}

function filterNavItems(items: NavItem[], hasPermission: (permission?: PermissionCode) => boolean): NavItem[] {
  return items
    .map((item) => {
      if (!item.children) {
        return hasPermission(item.permission) ? item : null;
      }

      const children = filterNavItems(item.children, hasPermission);
      return children.length > 0 ? { ...item, children } : null;
    })
    .filter((item): item is NavItem => Boolean(item));
}

function getInitials(value: string) {
  return value
    .split(/[.@\s_-]+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('');
}
