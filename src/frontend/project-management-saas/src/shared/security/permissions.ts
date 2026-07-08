export const permissions = {
  dashboard: { view: 'dashboard.view' },
  myTasks: { view: 'my-tasks.view' },
  calendar: { view: 'calendar.view' },
  notifications: { view: 'notifications.view' },
  profile: { view: 'profile.view' },
  search: { global: 'search.global' },
  organizations: {
    view: 'organizations.view',
    create: 'organizations.create',
    update: 'organizations.update',
    delete: 'organizations.delete',
    export: 'organizations.export'
  },
  departments: {
    view: 'departments.view',
    create: 'departments.create',
    update: 'departments.update',
    delete: 'departments.delete',
    export: 'departments.export'
  },
  teams: {
    view: 'teams.view',
    create: 'teams.create',
    update: 'teams.update',
    delete: 'teams.delete',
    export: 'teams.export'
  },
  users: {
    view: 'users.view',
    create: 'users.create',
    update: 'users.update',
    delete: 'users.delete',
    export: 'users.export',
    manageStatus: 'users.manage-status',
    resetPassword: 'users.reset-password',
    assignRoles: 'users.assign-roles'
  },
  roles: {
    view: 'roles.view',
    create: 'roles.create',
    update: 'roles.update',
    delete: 'roles.delete',
    export: 'roles.export',
    assignPermissions: 'roles.assign-permissions'
  },
  permissions: {
    view: 'permissions.view',
    create: 'permissions.create',
    update: 'permissions.update',
    delete: 'permissions.delete',
    export: 'permissions.export'
  },
  clients: {
    view: 'clients.view',
    create: 'clients.create',
    update: 'clients.update',
    delete: 'clients.delete',
    export: 'clients.export'
  },
  projects: {
    view: 'projects.view',
    create: 'projects.create',
    update: 'projects.update',
    delete: 'projects.delete',
    export: 'projects.export',
    dashboardView: 'projects.dashboard.view'
  },
  projectMembers: {
    view: 'project-members.view',
    manage: 'project-members.manage',
    export: 'project-members.export'
  },
  projectSettings: {
    view: 'project-settings.view',
    manage: 'project-settings.manage'
  },
  workItems: {
    view: 'work-items.view',
    create: 'work-items.create',
    update: 'work-items.update',
    delete: 'work-items.delete',
    export: 'work-items.export',
    transition: 'work-items.transition'
  },
  workComments: { manage: 'work-comments.manage' },
  workAttachments: { manage: 'work-attachments.manage' },
  workLinks: { manage: 'work-links.manage' },
  workFilters: { manage: 'work-filters.manage' },
  tenants: {
    view: 'tenants.view',
    create: 'tenants.create',
    update: 'tenants.update',
    delete: 'tenants.delete',
    export: 'tenants.export'
  },
  subscriptions: {
    view: 'subscriptions.view',
    create: 'subscriptions.create',
    update: 'subscriptions.update',
    delete: 'subscriptions.delete',
    export: 'subscriptions.export'
  }
} as const;

export type PermissionCode = string;

export interface CrudPermissionSet {
  create?: PermissionCode;
  update?: PermissionCode;
  delete?: PermissionCode;
  export?: PermissionCode;
}

export function hasPermission(permissionCodes: readonly string[], permission?: PermissionCode) {
  return !permission || permissionCodes.includes(permission);
}

export function hasAnyPermission(permissionCodes: readonly string[], requiredPermissions: readonly PermissionCode[]) {
  return requiredPermissions.length === 0 || requiredPermissions.some((permission) => hasPermission(permissionCodes, permission));
}
