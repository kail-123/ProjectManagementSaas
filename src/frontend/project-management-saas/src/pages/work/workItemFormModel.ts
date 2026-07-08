import type { Project, UserProfile, WorkItem, WorkItemRequest, WorkMetadata } from '../../shared/management/managementTypes';

export const emptyWorkItem: WorkItemRequest = {
  projectId: '',
  parentWorkItemId: null,
  workItemTypeId: '',
  workflowStatusId: null,
  priorityId: '',
  title: '',
  description: null,
  acceptanceCriteria: null,
  assigneeUserProfileId: null,
  reporterUserProfileId: '',
  startDate: null,
  dueDate: null,
  estimatedHours: null,
  loggedHours: 0,
  storyPoints: null,
  sprintId: null,
  labelIds: [],
  componentIds: [],
  watcherUserProfileIds: [],
  environment: null,
  severity: null,
  reproducible: null,
  rowVersion: null
};

export function buildDefaultRequest(projects: Project[], userProfiles: UserProfile[], metadata?: WorkMetadata): WorkItemRequest {
  const project = projects[0];
  const type = metadata?.types[0];
  const workflow = metadata?.workflows.find((item) => item.id === type?.workflowId);
  const initialStatus = workflow?.statuses.find((status) => status.isInitial) ?? workflow?.statuses[0];
  const priority = metadata?.priorities.find((item) => item.code === 'MEDIUM') ?? metadata?.priorities[0];
  const reporter = userProfiles.find((profile) => !project || profile.organizationId === project.organizationId) ?? userProfiles[0];

  return {
    ...emptyWorkItem,
    projectId: project?.id ?? '',
    workItemTypeId: type?.id ?? '',
    workflowStatusId: initialStatus?.id ?? null,
    priorityId: priority?.id ?? '',
    reporterUserProfileId: reporter?.id ?? ''
  };
}

export function toRequest(workItem: WorkItem): WorkItemRequest {
  return {
    projectId: workItem.projectId,
    parentWorkItemId: workItem.parentWorkItemId,
    workItemTypeId: workItem.workItemTypeId,
    workflowStatusId: workItem.workflowStatusId,
    priorityId: workItem.priorityId,
    title: workItem.title,
    description: workItem.description,
    acceptanceCriteria: workItem.acceptanceCriteria,
    assigneeUserProfileId: workItem.assigneeUserProfileId,
    reporterUserProfileId: workItem.reporterUserProfileId,
    startDate: workItem.startDate,
    dueDate: workItem.dueDate,
    estimatedHours: workItem.estimatedHours,
    loggedHours: workItem.loggedHours,
    storyPoints: workItem.storyPoints,
    sprintId: workItem.sprintId,
    labelIds: workItem.labels.map((label) => label.id),
    componentIds: workItem.components.map((component) => component.id),
    watcherUserProfileIds: workItem.watchers.map((watcher) => watcher.userProfileId),
    environment: workItem.environment,
    severity: workItem.severity,
    reproducible: workItem.reproducible,
    rowVersion: workItem.rowVersion
  };
}
