import { Box, Checkbox, FormControlLabel, ListItemText, MenuItem, Stack, Switch, TextField } from '@mui/material';
import type { Project, UserProfile, WorkItemRequest, WorkMetadata } from '../../shared/management/managementTypes';

export function WorkItemForm({
  form,
  setForm,
  projects,
  userProfiles,
  metadata
}: {
  form: WorkItemRequest;
  setForm: (form: WorkItemRequest) => void;
  projects: Project[];
  userProfiles: UserProfile[];
  metadata?: WorkMetadata;
}) {
  const selectedProject = projects.find((project) => project.id === form.projectId);
  const projectProfiles = selectedProject
    ? userProfiles.filter((profile) => profile.organizationId === selectedProject.organizationId)
    : userProfiles;
  const selectedType = metadata?.types.find((type) => type.id === form.workItemTypeId);
  const statuses = metadata?.workflows.find((workflow) => workflow.id === selectedType?.workflowId)?.statuses ?? [];
  const labels = selectedProject ? metadata?.labels.filter((label) => label.organizationId === selectedProject.organizationId) ?? [] : [];
  const components = metadata?.components.filter((component) => component.projectId === form.projectId) ?? [];

  return (
    <Stack spacing={2}>
      <TextField select label="Project" required value={form.projectId} onChange={(event) => setForm({ ...form, projectId: event.target.value, componentIds: [], labelIds: [] })}>
        {projects.map((project) => <MenuItem key={project.id} value={project.id}>{project.name}</MenuItem>)}
      </TextField>
      <TextField
        select
        label="Type"
        required
        value={form.workItemTypeId}
        onChange={(event) => {
          const typeId = event.target.value;
          const type = metadata?.types.find((item) => item.id === typeId);
          const initialStatus = metadata?.workflows.find((workflow) => workflow.id === type?.workflowId)?.statuses.find((status) => status.isInitial);
          setForm({ ...form, workItemTypeId: typeId, workflowStatusId: initialStatus?.id ?? null });
        }}
      >
        {metadata?.types.map((type) => <MenuItem key={type.id} value={type.id}>{type.name}</MenuItem>)}
      </TextField>
      <TextField select label="Status" value={form.workflowStatusId ?? ''} onChange={(event) => setForm({ ...form, workflowStatusId: event.target.value || null })}>
        {statuses.map((status) => <MenuItem key={status.id} value={status.id}>{status.name}</MenuItem>)}
      </TextField>
      <TextField select label="Priority" required value={form.priorityId} onChange={(event) => setForm({ ...form, priorityId: event.target.value })}>
        {metadata?.priorities.map((priority) => <MenuItem key={priority.id} value={priority.id}>{priority.name}</MenuItem>)}
      </TextField>
      <TextField label="Title" required value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} />
      <TextField label="Description" multiline minRows={4} value={form.description ?? ''} onChange={(event) => setForm({ ...form, description: event.target.value || null })} />
      <TextField label="Acceptance Criteria" multiline minRows={3} value={form.acceptanceCriteria ?? ''} onChange={(event) => setForm({ ...form, acceptanceCriteria: event.target.value || null })} />
      <TextField select label="Assignee" value={form.assigneeUserProfileId ?? ''} onChange={(event) => setForm({ ...form, assigneeUserProfileId: event.target.value || null })}>
        <MenuItem value="">Unassigned</MenuItem>
        {projectProfiles.map((profile) => <MenuItem key={profile.id} value={profile.id}>{profile.firstName} {profile.lastName}</MenuItem>)}
      </TextField>
      <TextField select label="Reporter" required value={form.reporterUserProfileId} onChange={(event) => setForm({ ...form, reporterUserProfileId: event.target.value })}>
        {projectProfiles.map((profile) => <MenuItem key={profile.id} value={profile.id}>{profile.firstName} {profile.lastName}</MenuItem>)}
      </TextField>
      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, gap: 2 }}>
        <TextField label="Start Date" type="date" value={form.startDate ?? ''} onChange={(event) => setForm({ ...form, startDate: event.target.value || null })} slotProps={{ inputLabel: { shrink: true } }} />
        <TextField label="Due Date" type="date" value={form.dueDate ?? ''} onChange={(event) => setForm({ ...form, dueDate: event.target.value || null })} slotProps={{ inputLabel: { shrink: true } }} />
        <TextField label="Estimated Hours" type="number" value={form.estimatedHours ?? ''} onChange={(event) => setForm({ ...form, estimatedHours: event.target.value ? Number(event.target.value) : null })} />
        <TextField label="Logged Hours" type="number" value={form.loggedHours} onChange={(event) => setForm({ ...form, loggedHours: Number(event.target.value) })} />
        <TextField label="Story Points" type="number" value={form.storyPoints ?? ''} onChange={(event) => setForm({ ...form, storyPoints: event.target.value ? Number(event.target.value) : null })} />
        <TextField label="Severity" value={form.severity ?? ''} onChange={(event) => setForm({ ...form, severity: event.target.value || null })} />
      </Box>
      <TextField
        select
        label="Labels"
        value={form.labelIds}
        onChange={(event) => setForm({ ...form, labelIds: event.target.value as unknown as string[] })}
        slotProps={{ select: { multiple: true, renderValue: (selected) => labels.filter((label) => (selected as string[]).includes(label.id)).map((label) => label.name).join(', ') } }}
      >
        {labels.map((label) => (
          <MenuItem key={label.id} value={label.id}>
            <Checkbox checked={form.labelIds.includes(label.id)} />
            <ListItemText primary={label.name} />
          </MenuItem>
        ))}
      </TextField>
      <TextField
        select
        label="Components"
        value={form.componentIds}
        onChange={(event) => setForm({ ...form, componentIds: event.target.value as unknown as string[] })}
        slotProps={{ select: { multiple: true, renderValue: (selected) => components.filter((component) => (selected as string[]).includes(component.id)).map((component) => component.name).join(', ') } }}
      >
        {components.map((component) => (
          <MenuItem key={component.id} value={component.id}>
            <Checkbox checked={form.componentIds.includes(component.id)} />
            <ListItemText primary={component.name} />
          </MenuItem>
        ))}
      </TextField>
      <TextField
        select
        label="Watchers"
        value={form.watcherUserProfileIds}
        onChange={(event) => setForm({ ...form, watcherUserProfileIds: event.target.value as unknown as string[] })}
        slotProps={{ select: { multiple: true, renderValue: (selected) => projectProfiles.filter((profile) => (selected as string[]).includes(profile.id)).map((profile) => `${profile.firstName} ${profile.lastName}`).join(', ') } }}
      >
        {projectProfiles.map((profile) => (
          <MenuItem key={profile.id} value={profile.id}>
            <Checkbox checked={form.watcherUserProfileIds.includes(profile.id)} />
            <ListItemText primary={`${profile.firstName} ${profile.lastName}`} />
          </MenuItem>
        ))}
      </TextField>
      <TextField label="Environment" value={form.environment ?? ''} onChange={(event) => setForm({ ...form, environment: event.target.value || null })} />
      <FormControlLabel control={<Switch checked={form.reproducible === true} onChange={(event) => setForm({ ...form, reproducible: event.target.checked })} />} label="Reproducible" />
    </Stack>
  );
}
