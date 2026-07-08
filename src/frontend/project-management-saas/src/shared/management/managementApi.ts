import { httpClient } from '../api/httpClient';
import type {
  Client,
  ClientRequest,
  ClientStatus,
  CreateUserRequest,
  Department,
  DepartmentRequest,
  NotificationPreferences,
  NotificationPreferencesRequest,
  NotificationPriority,
  NotificationType,
  Organization,
  OrganizationRequest,
  PagedRequest,
  PagedResult,
  Permission,
  PermissionRequest,
  Project,
  ProjectDashboard,
  ProjectMember,
  ProjectMemberRequest,
  ProjectSettings,
  ProjectSettingsRequest,
  ProjectRequest,
  ProjectStatus,
  Role,
  RoleRequest,
  SearchResult,
  Team,
  TeamRequest,
  UpdateUserRequest,
  User,
  WorkActivity,
  WorkAttachment,
  WorkAttachmentRequest,
  WorkComment,
  WorkCommentAttachment,
  WorkCommentAttachmentRequest,
  WorkCommentHistory,
  WorkCommentMention,
  WorkCommentMentionsRequest,
  WorkCommentReaction,
  WorkCommentReactionRequest,
  WorkCommentRead,
  WorkCommentRequest,
  WorkItem,
  WorkItemLink,
  WorkItemLinkRequest,
  WorkItemRequest,
  WorkMetadata,
  WorkMentionCandidate,
  WorkSavedFilter,
  WorkSavedFilterRequest,
  WorkspaceNotification
} from './managementTypes';

function toQueryParams(request: PagedRequest, extra?: Record<string, string | number | boolean | null | undefined>) {
  const params: Record<string, string | number | boolean> = {
    pageNumber: request.pageNumber,
    pageSize: request.pageSize
  };

  if (request.search?.trim()) {
    params.search = request.search.trim();
  }

  if (request.sortBy) {
    params.sortBy = request.sortBy;
  }

  if (request.sortDirection) {
    params.sortDirection = request.sortDirection;
  }

  Object.entries(extra ?? {}).forEach(([key, value]) => {
    if (value !== null && value !== undefined && value !== '') {
      params[key] = value;
    }
  });

  return params;
}

async function get<T>(resource: string) {
  const response = await httpClient.get<T>(resource);

  return response.data;
}

async function list<T>(resource: string, request: PagedRequest, extra?: Record<string, string | number | boolean | null | undefined>) {
  const response = await httpClient.get<PagedResult<T>>(resource, {
    params: toQueryParams(request, extra)
  });

  return response.data;
}

async function create<T, TRequest>(resource: string, request: TRequest) {
  const response = await httpClient.post<T>(resource, request);

  return response.data;
}

async function update<T, TRequest>(resource: string, id: string, request: TRequest) {
  const response = await httpClient.put<T>(`${resource}/${id}`, request);

  return response.data;
}

async function remove(resource: string, id: string) {
  await httpClient.delete(`${resource}/${id}`);
}

async function bulkDelete(resource: string, ids: readonly string[]) {
  await httpClient.post(`${resource}/bulk-delete`, { ids });
}

export const managementApi = {
  organizations: {
    list: (request: PagedRequest) => list<Organization>('/organizations', request),
    create: (request: OrganizationRequest) => create<Organization, OrganizationRequest>('/organizations', request),
    update: (id: string, request: OrganizationRequest) => update<Organization, OrganizationRequest>('/organizations', id, request),
    delete: (id: string) => remove('/organizations', id),
    bulkDelete: (ids: readonly string[]) => bulkDelete('/organizations', ids)
  },
  departments: {
    list: (request: PagedRequest, organizationId?: string | null) => list<Department>('/departments', request, { organizationId }),
    create: (request: DepartmentRequest) => create<Department, DepartmentRequest>('/departments', request),
    update: (id: string, request: DepartmentRequest) => update<Department, DepartmentRequest>('/departments', id, request),
    delete: (id: string) => remove('/departments', id),
    bulkDelete: (ids: readonly string[]) => bulkDelete('/departments', ids)
  },
  teams: {
    list: (request: PagedRequest, departmentId?: string | null) => list<Team>('/teams', request, { departmentId }),
    create: (request: TeamRequest) => create<Team, TeamRequest>('/teams', request),
    update: (id: string, request: TeamRequest) => update<Team, TeamRequest>('/teams', id, request),
    delete: (id: string) => remove('/teams', id),
    bulkDelete: (ids: readonly string[]) => bulkDelete('/teams', ids)
  },
  users: {
    list: (request: PagedRequest) => list<User>('/users', request),
    create: (request: CreateUserRequest) => create<User, CreateUserRequest>('/users', request),
    update: (id: string, request: UpdateUserRequest) => update<User, UpdateUserRequest>('/users', id, request),
    delete: (id: string) => remove('/users', id),
    bulkDelete: (ids: readonly string[]) => bulkDelete('/users', ids),
    setActiveStatus: async (id: string, isActive: boolean) => {
      await httpClient.patch(`/users/${id}/active-status`, { isActive });
    },
    resetPassword: async (id: string, newPassword: string) => {
      await httpClient.post(`/users/${id}/reset-password`, { newPassword });
    },
    assignRoles: async (id: string, roles: readonly string[]) => {
      await httpClient.put(`/users/${id}/roles`, { roles });
    }
  },
  roles: {
    list: (request: PagedRequest) => list<Role>('/roles', request),
    create: (request: RoleRequest) => create<Role, RoleRequest>('/roles', request),
    update: (id: string, request: RoleRequest) => update<Role, RoleRequest>('/roles', id, request),
    delete: (id: string) => remove('/roles', id),
    bulkDelete: (ids: readonly string[]) => bulkDelete('/roles', ids),
    assignPermissions: async (id: string, permissionIds: readonly string[]) => {
      await httpClient.put(`/roles/${id}/permissions`, { permissionIds });
    }
  },
  permissions: {
    list: (request: PagedRequest, module?: string | null) => list<Permission>('/permissions', request, { module }),
    create: (request: PermissionRequest) => create<Permission, PermissionRequest>('/permissions', request),
    update: (id: string, request: PermissionRequest) => update<Permission, PermissionRequest>('/permissions', id, request),
    delete: (id: string) => remove('/permissions', id),
    bulkDelete: (ids: readonly string[]) => bulkDelete('/permissions', ids)
  },
  clients: {
    list: (request: PagedRequest, organizationId?: string | null, status?: ClientStatus | null) =>
      list<Client>('/clients', request, { organizationId, status }),
    search: (request: PagedRequest, organizationId?: string | null) => list<SearchResult>('/clients/search', request, { organizationId }),
    create: (request: ClientRequest) => create<Client, ClientRequest>('/clients', request),
    update: (id: string, request: ClientRequest) => update<Client, ClientRequest>('/clients', id, request),
    delete: (id: string) => remove('/clients', id),
    bulkDelete: (ids: readonly string[]) => bulkDelete('/clients', ids)
  },
  projects: {
    list: (request: PagedRequest, organizationId?: string | null, clientId?: string | null, status?: ProjectStatus | null) =>
      list<Project>('/projects', request, { organizationId, clientId, status }),
    myProjects: (request: PagedRequest, organizationId?: string | null, clientId?: string | null, status?: ProjectStatus | null) =>
      list<Project>('/my-projects', request, { organizationId, clientId, status }),
    search: (request: PagedRequest, organizationId?: string | null, clientId?: string | null) =>
      list<SearchResult>('/projects/search', request, { organizationId, clientId }),
    get: (id: string) => get<Project>(`/projects/${id}`),
    create: (request: ProjectRequest) => create<Project, ProjectRequest>('/projects', request),
    update: (id: string, request: ProjectRequest) => update<Project, ProjectRequest>('/projects', id, request),
    delete: (id: string) => remove('/projects', id),
    bulkDelete: (ids: readonly string[]) => bulkDelete('/projects', ids),
    members: {
      list: (projectId: string, request: PagedRequest) => list<ProjectMember>(`/project/${projectId}/members`, request),
      add: (projectId: string, request: ProjectMemberRequest) =>
        create<ProjectMember[], ProjectMemberRequest>(`/project/${projectId}/members`, request),
      delete: (projectId: string, userId: string) => remove(`/project/${projectId}/members`, userId),
      bulkDelete: (projectId: string, ids: readonly string[]) => bulkDelete(`/projects/${projectId}/members`, ids)
    },
    settings: {
      get: (projectId: string) => get<ProjectSettings>(`/projects/${projectId}/settings`),
      update: (projectId: string, request: ProjectSettingsRequest) =>
        update<ProjectSettings, ProjectSettingsRequest>('/projects', `${projectId}/settings`, request)
    },
    dashboard: (projectId: string) => get<ProjectDashboard>(`/projects/${projectId}/dashboard`)
  },
  work: {
    metadata: (projectId?: string | null, organizationId?: string | null) => get<WorkMetadata>(`/work/metadata${toOptionalQueryString({ projectId, organizationId })}`),
    items: {
      list: (
        request: PagedRequest,
        filters?: {
          projectId?: string | null;
          workItemTypeId?: string | null;
          workflowStatusId?: string | null;
          priorityId?: string | null;
          assigneeUserProfileId?: string | null;
          reporterUserProfileId?: string | null;
        }
      ) => list<WorkItem>('/work/items', request, filters),
      get: (id: string) => get<WorkItem>(`/work/items/${id}`),
      create: (request: WorkItemRequest) => create<WorkItem, WorkItemRequest>('/work/items', request),
      update: (id: string, request: WorkItemRequest) => update<WorkItem, WorkItemRequest>('/work/items', id, request),
      delete: (id: string) => remove('/work/items', id),
      bulkDelete: (ids: readonly string[]) => bulkDelete('/work/items', ids),
      transition: async (id: string, toStatusId: string, rowVersion: string) => {
        const response = await httpClient.post<WorkItem>(`/work/items/${id}/transition`, { toStatusId, rowVersion });
        return response.data;
      }
    },
    comments: {
      list: (workItemId: string, request: PagedRequest) => list<WorkComment>(`/work/items/${workItemId}/comments`, request),
      mentionCandidates: (workItemId: string, request: PagedRequest) => list<WorkMentionCandidate>(`/work/items/${workItemId}/mention-candidates`, request),
      add: (workItemId: string, request: WorkCommentRequest) => create<WorkComment, WorkCommentRequest>(`/work/items/${workItemId}/comments`, request),
      update: (workItemId: string, commentId: string, request: WorkCommentRequest) =>
        update<WorkComment, WorkCommentRequest>(`/work/items/${workItemId}/comments`, commentId, request),
      delete: (workItemId: string, commentId: string) => remove(`/work/items/${workItemId}/comments`, commentId),
      restore: async (workItemId: string, commentId: string) => {
        const response = await httpClient.post<WorkComment>(`/work/items/${workItemId}/comments/${commentId}/restore`);
        return response.data;
      },
      pin: async (workItemId: string, commentId: string) => {
        const response = await httpClient.post<WorkComment>(`/work/items/${workItemId}/comments/${commentId}/pin`);
        return response.data;
      },
      unpin: async (workItemId: string, commentId: string) => {
        const response = await httpClient.post<WorkComment>(`/work/items/${workItemId}/comments/${commentId}/unpin`);
        return response.data;
      },
      resolve: async (workItemId: string, commentId: string) => {
        const response = await httpClient.post<WorkComment>(`/work/items/${workItemId}/comments/${commentId}/resolve`);
        return response.data;
      },
      unresolve: async (workItemId: string, commentId: string) => {
        const response = await httpClient.post<WorkComment>(`/work/items/${workItemId}/comments/${commentId}/unresolve`);
        return response.data;
      },
      history: (workItemId: string, commentId: string, request: PagedRequest) => list<WorkCommentHistory>(`/work/items/${workItemId}/comments/${commentId}/history`, request),
      markAllRead: async (workItemId: string) => {
        await httpClient.post(`/work/items/${workItemId}/comments/mark-all-read`);
      },
      mentions: {
        list: (workItemId: string, commentId: string, request: PagedRequest) => list<WorkCommentMention>(`/work/items/${workItemId}/comments/${commentId}/mentions`, request),
        add: (workItemId: string, commentId: string, request: WorkCommentMentionsRequest) =>
          create<WorkCommentMention[], WorkCommentMentionsRequest>(`/work/items/${workItemId}/comments/${commentId}/mentions`, request),
        delete: (workItemId: string, commentId: string, mentionId: string) => remove(`/work/items/${workItemId}/comments/${commentId}/mentions`, mentionId)
      },
      reactions: {
        list: (workItemId: string, commentId: string, request: PagedRequest) => list<WorkCommentReaction>(`/work/items/${workItemId}/comments/${commentId}/reactions`, request),
        toggle: async (workItemId: string, commentId: string, request: WorkCommentReactionRequest) => {
          const response = await httpClient.post<WorkCommentReaction[]>(`/work/items/${workItemId}/comments/${commentId}/reactions`, request);
          return response.data;
        },
        delete: (workItemId: string, commentId: string, emoji: string) => remove(`/work/items/${workItemId}/comments/${commentId}/reactions`, encodeURIComponent(emoji))
      },
      reads: {
        list: (workItemId: string, commentId: string, request: PagedRequest) => list<WorkCommentRead>(`/work/items/${workItemId}/comments/${commentId}/reads`, request),
        mark: async (workItemId: string, commentId: string) => {
          const response = await httpClient.post<WorkCommentRead>(`/work/items/${workItemId}/comments/${commentId}/read`);
          return response.data;
        }
      },
      attachments: {
        list: (workItemId: string, commentId: string, request: PagedRequest) => list<WorkCommentAttachment>(`/work/items/${workItemId}/comments/${commentId}/attachments`, request),
        add: (workItemId: string, commentId: string, request: WorkCommentAttachmentRequest) =>
          create<WorkCommentAttachment, WorkCommentAttachmentRequest>(`/work/items/${workItemId}/comments/${commentId}/attachments`, request),
        delete: (workItemId: string, commentId: string, attachmentId: string) => remove(`/work/items/${workItemId}/comments/${commentId}/attachments`, attachmentId)
      }
    },
    attachments: {
      list: (workItemId: string, request: PagedRequest) => list<WorkAttachment>(`/work/items/${workItemId}/attachments`, request),
      add: (workItemId: string, request: WorkAttachmentRequest) => create<WorkAttachment, WorkAttachmentRequest>(`/work/items/${workItemId}/attachments`, request),
      delete: (workItemId: string, attachmentId: string) => remove(`/work/items/${workItemId}/attachments`, attachmentId)
    },
    activity: {
      list: (
        workItemId: string,
        request: PagedRequest,
        filters?: {
          category?: string | null;
          activityType?: string | null;
          dateFrom?: string | null;
          dateTo?: string | null;
        }
      ) => list<WorkActivity>(`/work/items/${workItemId}/activity`, request, filters)
    },
    links: {
      list: (workItemId: string, request: PagedRequest) => list<WorkItemLink>(`/work/items/${workItemId}/links`, request),
      add: (workItemId: string, request: WorkItemLinkRequest) => create<WorkItemLink, WorkItemLinkRequest>(`/work/items/${workItemId}/links`, request),
      delete: (workItemId: string, linkId: string) => remove(`/work/items/${workItemId}/links`, linkId)
    },
    savedFilters: {
      list: (request: PagedRequest, projectId?: string | null) => list<WorkSavedFilter>('/work/saved-filters', request, { projectId }),
      create: (request: WorkSavedFilterRequest) => create<WorkSavedFilter, WorkSavedFilterRequest>('/work/saved-filters', request),
      delete: (id: string) => remove('/work/saved-filters', id)
    }
  },
  search: {
    global: (request: PagedRequest) => list<SearchResult>('/search/global', request)
  },
  notifications: {
    list: (
      request: PagedRequest,
      filters?: {
        type?: NotificationType | null;
        priority?: NotificationPriority | null;
        isRead?: boolean | null;
        projectId?: string | null;
      }
    ) => list<WorkspaceNotification>('/notifications', request, filters),
    unreadCount: () => get<number>('/notifications/unread-count'),
    markRead: async (id: string) => {
      await httpClient.post(`/notifications/${id}/read`);
    },
    markUnread: async (id: string) => {
      await httpClient.post(`/notifications/${id}/unread`);
    },
    markAllRead: async () => {
      await httpClient.post('/notifications/mark-all-read');
    },
    delete: (id: string) => remove('/notifications', id),
    clearRead: async () => {
      await httpClient.delete('/notifications/read');
    },
    preferences: () => get<NotificationPreferences>('/notifications/preferences'),
    updatePreferences: (request: NotificationPreferencesRequest) =>
      update<NotificationPreferences, NotificationPreferencesRequest>('/notifications', 'preferences', request)
  }
};

function toOptionalQueryString(values: Record<string, string | null | undefined>) {
  const params = new URLSearchParams();
  Object.entries(values).forEach(([key, value]) => {
    if (value) {
      params.set(key, value);
    }
  });

  const query = params.toString();
  return query ? `?${query}` : '';
}
