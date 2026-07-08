export interface PagedRequest {
  pageNumber: number;
  pageSize: number;
  search?: string;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface PagedResult<TItem> {
  items: TItem[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface Organization {
  id: string;
  name: string;
  code: string;
  logo: string | null;
  website: string | null;
  email: string;
  phone: string | null;
  address: string | null;
  city: string | null;
  state: string | null;
  country: string;
  timeZone: string;
  currency: string;
  isActive: boolean;
  createdBy: string | null;
  createdOn: string;
  updatedBy: string | null;
  updatedOn: string | null;
}

export type OrganizationRequest = Omit<Organization, 'id' | 'createdBy' | 'createdOn' | 'updatedBy' | 'updatedOn'>;

export interface Department {
  id: string;
  organizationId: string;
  organizationName: string;
  name: string;
  code: string;
  description: string | null;
  isActive: boolean;
  createdBy: string | null;
  createdOn: string;
  updatedBy: string | null;
  updatedOn: string | null;
}

export interface DepartmentRequest {
  organizationId: string;
  name: string;
  code: string;
  description: string | null;
  isActive: boolean;
}

export interface Team {
  id: string;
  departmentId: string;
  departmentName: string;
  teamLeadUserProfileId: string | null;
  teamLeadName: string | null;
  name: string;
  code: string;
  description: string | null;
  isActive: boolean;
  memberUserProfileIds: string[];
  createdBy: string | null;
  createdOn: string;
  updatedBy: string | null;
  updatedOn: string | null;
}

export interface TeamRequest {
  departmentId: string;
  teamLeadUserProfileId: string | null;
  name: string;
  code: string;
  description: string | null;
  isActive: boolean;
  memberUserProfileIds: string[];
}

export interface UserProfile {
  id: string;
  userId: string;
  organizationId: string | null;
  organizationName: string | null;
  departmentId: string | null;
  departmentName: string | null;
  teamId: string | null;
  teamName: string | null;
  firstName: string;
  lastName: string;
  employeeCode: string;
  designation: string | null;
  profilePhoto: string | null;
  phone: string | null;
  timeZone: string;
  skills: string | null;
  joiningDate: string | null;
  isActive: boolean;
}

export interface User {
  id: string;
  email: string;
  displayName: string | null;
  isActive: boolean;
  roles: string[];
  profile: UserProfile | null;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  employeeCode: string;
  designation: string | null;
  organizationId: string | null;
  departmentId: string | null;
  teamId: string | null;
  profilePhoto: string | null;
  phone: string | null;
  timeZone: string;
  skills: string | null;
  joiningDate: string | null;
  isActive: boolean;
  roles: string[];
}

export type UpdateUserRequest = Omit<CreateUserRequest, 'password'>;

export interface Permission {
  id: string;
  module: string;
  name: string;
  code: string;
  description: string | null;
  isActive: boolean;
  createdBy: string | null;
  createdOn: string;
  updatedBy: string | null;
  updatedOn: string | null;
}

export type PermissionRequest = Omit<Permission, 'id' | 'createdBy' | 'createdOn' | 'updatedBy' | 'updatedOn'>;

export interface Role {
  id: string;
  name: string;
  description: string | null;
  permissions: Permission[];
}

export interface RoleRequest {
  name: string;
  description: string | null;
}

export type ClientStatus = 'Prospect' | 'Active' | 'Inactive' | 'Archived';

export type ProjectStatus = 'Planning' | 'Active' | 'OnHold' | 'Completed' | 'Cancelled' | 'Archived';

export type ProjectPriority = 'Low' | 'Medium' | 'High' | 'Critical';

export type ProjectVisibility = 'Private' | 'Organization' | 'Public';

export type ProjectMemberRole =
  | 'Developer'
  | 'Qa'
  | 'ProjectManager'
  | 'BusinessAnalyst'
  | 'UiUx'
  | 'Client'
  | 'Viewer';

export interface Client {
  id: string;
  organizationId: string;
  organizationName: string;
  name: string;
  code: string;
  contactPerson: string | null;
  email: string;
  phone: string | null;
  mobile: string | null;
  gstNumber: string | null;
  pan: string | null;
  billingAddress: string | null;
  shippingAddress: string | null;
  country: string;
  state: string | null;
  city: string | null;
  website: string | null;
  notes: string | null;
  status: ClientStatus;
  createdBy: string | null;
  createdOn: string;
  updatedBy: string | null;
  updatedOn: string | null;
}

export type ClientRequest = Omit<Client, 'id' | 'organizationName' | 'createdBy' | 'createdOn' | 'updatedBy' | 'updatedOn'>;

export interface Project {
  id: string;
  organizationId: string;
  organizationName: string;
  clientId: string | null;
  clientName: string | null;
  projectManagerUserProfileId: string | null;
  projectManagerName: string | null;
  name: string;
  code: string;
  description: string | null;
  startDate: string | null;
  endDate: string | null;
  estimatedBudget: number | null;
  status: ProjectStatus;
  priority: ProjectPriority;
  visibility: ProjectVisibility;
  teamIds: string[];
  memberCount: number;
  createdBy: string | null;
  createdOn: string;
  updatedBy: string | null;
  updatedOn: string | null;
}

export interface ProjectRequest {
  organizationId: string;
  clientId: string | null;
  projectManagerUserProfileId: string | null;
  name: string;
  code: string;
  description: string | null;
  startDate: string | null;
  endDate: string | null;
  estimatedBudget: number | null;
  status: ProjectStatus;
  priority: ProjectPriority;
  visibility: ProjectVisibility;
  teamIds: string[];
}

export interface ProjectMember {
  id: string;
  projectId: string;
  userId: string;
  userProfileId: string | null;
  userDisplayName: string;
  employeeCode: string;
  designation: string | null;
  roleInProject: ProjectMemberRole;
  joinedDate: string;
  isActive: boolean;
  addedBy: string | null;
}

export interface ProjectMemberRequest {
  userIds: string[];
  roleInProject: ProjectMemberRole;
}

export interface ProjectSettings {
  projectId: string;
  enableSprint: boolean;
  enableBacklog: boolean;
  enableKanban: boolean;
  enableTimeTracking: boolean;
  enableWiki: boolean;
  enableDocuments: boolean;
  enableApprovals: boolean;
  enableLeaveRequests: boolean;
  enableBugTracking: boolean;
  enableRiskRegister: boolean;
  createdBy: string | null;
  createdOn: string;
  updatedBy: string | null;
  updatedOn: string | null;
}

export type ProjectSettingsRequest = Omit<ProjectSettings, 'projectId' | 'createdBy' | 'createdOn' | 'updatedBy' | 'updatedOn'>;

export interface ProjectDashboard {
  summary: ProjectSummary;
  memberCount: number;
  teamCount: number;
  taskCount: number;
  completionPercentage: number;
  upcomingDeadlines: ProjectDeadline[];
  activity: ProjectActivity[];
  recentMembers: RecentProjectMember[];
}

export interface ProjectSummary {
  id: string;
  name: string;
  code: string;
  status: ProjectStatus;
  priority: ProjectPriority;
  startDate: string | null;
  endDate: string | null;
  estimatedBudget: number | null;
  organizationName: string;
  clientName: string | null;
  projectManagerName: string | null;
}

export interface ProjectDeadline {
  title: string;
  dueDate: string;
  category: string;
}

export interface ProjectActivity {
  entityName: string;
  action: string;
  changedBy: string;
  changedOn: string;
}

export interface RecentProjectMember {
  userId: string;
  displayName: string;
  roleInProject: ProjectMemberRole;
  joinedDate: string;
}

export interface SearchResult {
  id: string;
  type: 'Client' | 'Project' | 'WorkItem';
  title: string;
  subtitle: string | null;
  url: string;
}

export type NotificationType =
  | 'TaskAssigned'
  | 'TaskReassigned'
  | 'TaskCompleted'
  | 'TaskReopened'
  | 'TaskOverdue'
  | 'Mention'
  | 'Reply'
  | 'Comment'
  | 'StatusChanged'
  | 'PriorityChanged'
  | 'DueDateChanged'
  | 'AttachmentAdded'
  | 'ProjectInvitation'
  | 'ProjectRemoved'
  | 'SprintStarted'
  | 'SprintCompleted'
  | 'ApprovalRequested'
  | 'ApprovalCompleted'
  | 'ProjectArchived'
  | 'SystemAnnouncement'
  | 'TaskUpdated'
  | 'TaskDeleted'
  | 'WatcherAdded';

export type NotificationPriority = 'Low' | 'Normal' | 'High' | 'Critical';

export interface WorkspaceNotification {
  id: string;
  userId: string;
  projectId: string | null;
  projectName: string | null;
  workItemId: string | null;
  workItemTitle: string | null;
  title: string;
  message: string;
  notificationType: NotificationType;
  priority: NotificationPriority;
  isRead: boolean;
  readAt: string | null;
  actionUrl: string | null;
  icon: string | null;
  createdBy: string | null;
  createdAt: string;
  expiresAt: string | null;
}

export interface NotificationPreferences {
  userId: string;
  assignmentNotifications: boolean;
  commentNotifications: boolean;
  mentionNotifications: boolean;
  replyNotifications: boolean;
  projectNotifications: boolean;
  emailNotifications: boolean;
  browserNotifications: boolean;
  soundNotifications: boolean;
  updatedAt: string;
}

export type NotificationPreferencesRequest = Omit<NotificationPreferences, 'userId' | 'updatedAt'>;

export interface WorkItemType {
  id: string;
  name: string;
  code: string;
  icon: string;
  color: string;
  workflowId: string;
  description: string | null;
  isActive: boolean;
}

export interface WorkWorkflowStatus {
  id: string;
  workflowId: string;
  name: string;
  code: string;
  color: string;
  sortOrder: number;
  isInitial: boolean;
  isTerminal: boolean;
  isActive: boolean;
}

export interface WorkWorkflowTransition {
  id: string;
  workflowId: string;
  fromStatusId: string;
  toStatusId: string;
  name: string;
  isActive: boolean;
}

export interface WorkWorkflow {
  id: string;
  name: string;
  code: string;
  description: string | null;
  isDefault: boolean;
  isActive: boolean;
  statuses: WorkWorkflowStatus[];
  transitions: WorkWorkflowTransition[];
}

export interface WorkPriority {
  id: string;
  name: string;
  code: string;
  color: string;
  sortOrder: number;
  isActive: boolean;
}

export interface WorkLabel {
  id: string;
  organizationId: string;
  name: string;
  color: string;
  description: string | null;
  isActive: boolean;
}

export interface WorkComponent {
  id: string;
  projectId: string;
  name: string;
  description: string | null;
  color: string;
  isActive: boolean;
}

export interface WorkWatcher {
  userProfileId: string;
  displayName: string;
  addedOn: string;
  addedBy: string | null;
}

export interface WorkMetadata {
  types: WorkItemType[];
  workflows: WorkWorkflow[];
  priorities: WorkPriority[];
  labels: WorkLabel[];
  components: WorkComponent[];
}

export interface WorkItem {
  id: string;
  projectId: string;
  projectName: string;
  parentWorkItemId: string | null;
  parentTitle: string | null;
  workItemTypeId: string;
  typeName: string;
  typeIcon: string;
  typeColor: string;
  workflowStatusId: string;
  statusName: string;
  statusColor: string;
  priorityId: string;
  priorityName: string;
  priorityColor: string;
  title: string;
  description: string | null;
  acceptanceCriteria: string | null;
  assigneeUserProfileId: string | null;
  assigneeName: string | null;
  reporterUserProfileId: string;
  reporterName: string;
  startDate: string | null;
  dueDate: string | null;
  estimatedHours: number | null;
  loggedHours: number;
  storyPoints: number | null;
  sprintId: string | null;
  labels: WorkLabel[];
  components: WorkComponent[];
  environment: string | null;
  severity: string | null;
  reproducible: boolean | null;
  commentCount: number;
  attachmentCount: number;
  watchers: WorkWatcher[];
  watcherCount: number;
  rowVersion: string;
  createdBy: string | null;
  createdOn: string;
  updatedBy: string | null;
  updatedOn: string | null;
}

export interface WorkItemRequest {
  projectId: string;
  parentWorkItemId: string | null;
  workItemTypeId: string;
  workflowStatusId: string | null;
  priorityId: string;
  title: string;
  description: string | null;
  acceptanceCriteria: string | null;
  assigneeUserProfileId: string | null;
  reporterUserProfileId: string;
  startDate: string | null;
  dueDate: string | null;
  estimatedHours: number | null;
  loggedHours: number;
  storyPoints: number | null;
  sprintId: string | null;
  labelIds: string[];
  componentIds: string[];
  watcherUserProfileIds: string[];
  environment: string | null;
  severity: string | null;
  reproducible: boolean | null;
  rowVersion: string | null;
}

export type WorkItemLinkType = 'Duplicate' | 'Blocks' | 'BlockedBy' | 'RelatesTo' | 'DependsOn';

export interface WorkComment {
  id: string;
  workItemId: string;
  parentCommentId: string | null;
  authorId: string | null;
  authorName: string;
  authorRole: string | null;
  message: string;
  bodyMarkdown: string;
  isEdited: boolean;
  editedAt: string | null;
  isDeleted: boolean;
  deletedAt: string | null;
  pinned: boolean;
  resolved: boolean;
  replyCount: number;
  mentionUserProfileIds: string[];
  mentionedUserIds: string[];
  mentions: WorkCommentMention[];
  attachments: WorkCommentAttachment[];
  reactions: WorkCommentReaction[];
  readReceipts: WorkCommentRead[];
  counters: WorkCommentCounters;
  readByCurrentUser: boolean;
  createdBy: string | null;
  createdOn: string;
  updatedBy: string | null;
  updatedOn: string | null;
}

export interface WorkCommentRequest {
  parentCommentId: string | null;
  message?: string;
  bodyMarkdown: string;
  mentionUserProfileIds: string[];
  mentionedUserIds: string[];
}

export interface WorkCommentMention {
  id: string;
  commentId: string;
  mentionedUserId: string;
  userProfileId: string;
  displayName: string;
  employeeCode: string;
  designation: string | null;
  department: string | null;
  profilePhoto: string | null;
  mentionedByUserId: string | null;
  createdAt: string;
}

export interface WorkMentionCandidate {
  userId: string;
  userProfileId: string;
  displayName: string;
  employeeCode: string;
  email: string;
  designation: string | null;
  department: string | null;
  profilePhoto: string | null;
}

export interface WorkCommentMentionsRequest {
  mentionedUserIds: string[];
}

export interface WorkCommentReaction {
  id: string;
  commentId: string;
  userId: string;
  displayName: string;
  emoji: string;
  createdAt: string;
}

export interface WorkCommentReactionRequest {
  emoji: string;
}

export interface WorkCommentRead {
  id: string;
  commentId: string;
  userId: string;
  displayName: string;
  readAt: string;
}

export interface WorkCommentCounters {
  replies: number;
  mentions: number;
  reactions: number;
  attachments: number;
  seenCount: number;
}

export interface WorkCommentHistory {
  id: string;
  commentId: string;
  editedBy: string | null;
  editedByName: string;
  editedAt: string;
  previousMessage: string;
  newMessage: string;
}

export interface WorkCommentAttachment {
  id: string;
  commentId: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  storagePath: string;
  previewType: string;
  uploadedBy: string | null;
  uploadedAt: string;
}

export interface WorkCommentAttachmentRequest {
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  storagePath: string;
}

export interface WorkAttachment {
  id: string;
  workItemId: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  storagePath: string;
  version: number;
  description: string | null;
  createdBy: string | null;
  createdOn: string;
}

export interface WorkAttachmentRequest {
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  storagePath: string;
  version: number;
  description: string | null;
}

export interface WorkActivity {
  id: string;
  projectId: string;
  workItemId: string | null;
  userId: string | null;
  actorName: string;
  actorRole: string | null;
  actorAvatar: string | null;
  activityType: string;
  category: string;
  description: string;
  fieldName: string | null;
  oldValue: string | null;
  newValue: string | null;
  createdAt: string;
}

export interface WorkItemLink {
  id: string;
  sourceWorkItemId: string;
  targetWorkItemId: string;
  targetTitle: string;
  linkType: WorkItemLinkType;
  description: string | null;
}

export interface WorkItemLinkRequest {
  targetWorkItemId: string;
  linkType: WorkItemLinkType;
  description: string | null;
}

export interface WorkSavedFilter {
  id: string;
  projectId: string | null;
  ownerUserId: string | null;
  name: string;
  queryJson: string;
  isShared: boolean;
  createdBy: string | null;
  createdOn: string;
}

export interface WorkSavedFilterRequest {
  projectId: string | null;
  name: string;
  queryJson: string;
  isShared: boolean;
}
