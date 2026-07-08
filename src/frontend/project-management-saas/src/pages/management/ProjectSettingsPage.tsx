import { Box, Button, FormControlLabel, Paper, Skeleton, Stack, Switch } from '@mui/material';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, Save } from 'lucide-react';
import { useState } from 'react';
import { Link as RouterLink, Navigate, useParams } from 'react-router-dom';
import { routePaths } from '../../app/router/routePaths';
import { getApiErrorMessage } from '../../shared/api/apiError';
import { usePermissions } from '../../features/auth/model/usePermissions';
import { managementApi } from '../../shared/management/managementApi';
import type { ProjectSettingsRequest } from '../../shared/management/managementTypes';
import { permissions } from '../../shared/security/permissions';
import { PageHeader } from '../../shared/ui/PageHeader';
import { useToast } from '../../shared/ui/toastContext';

const defaultSettings: ProjectSettingsRequest = {
  enableSprint: true,
  enableBacklog: true,
  enableKanban: true,
  enableTimeTracking: true,
  enableWiki: true,
  enableDocuments: true,
  enableApprovals: false,
  enableLeaveRequests: false,
  enableBugTracking: true,
  enableRiskRegister: false
};

const settingFields: Array<{ key: keyof ProjectSettingsRequest; label: string }> = [
  { key: 'enableSprint', label: 'Sprint' },
  { key: 'enableBacklog', label: 'Backlog' },
  { key: 'enableKanban', label: 'Kanban' },
  { key: 'enableTimeTracking', label: 'Time Tracking' },
  { key: 'enableWiki', label: 'Wiki' },
  { key: 'enableDocuments', label: 'Documents' },
  { key: 'enableApprovals', label: 'Approvals' },
  { key: 'enableLeaveRequests', label: 'Leave Requests' },
  { key: 'enableBugTracking', label: 'Bug Tracking' },
  { key: 'enableRiskRegister', label: 'Risk Register' }
];

export function ProjectSettingsPage() {
  const { projectId } = useParams();
  const [overrides, setOverrides] = useState<Partial<ProjectSettingsRequest>>({});
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const { hasPermission } = usePermissions();
  const canManageSettings = hasPermission(permissions.projectSettings.manage);

  const projectQuery = useQuery({
    queryKey: ['projects', projectId],
    queryFn: () => managementApi.projects.get(projectId!),
    enabled: Boolean(projectId)
  });
  const settingsQuery = useQuery({
    queryKey: ['projects', projectId, 'settings'],
    queryFn: () => managementApi.projects.settings.get(projectId!),
    enabled: Boolean(projectId)
  });

  const savedSettings: ProjectSettingsRequest = settingsQuery.data
    ? {
        enableSprint: settingsQuery.data.enableSprint,
        enableBacklog: settingsQuery.data.enableBacklog,
        enableKanban: settingsQuery.data.enableKanban,
        enableTimeTracking: settingsQuery.data.enableTimeTracking,
        enableWiki: settingsQuery.data.enableWiki,
        enableDocuments: settingsQuery.data.enableDocuments,
        enableApprovals: settingsQuery.data.enableApprovals,
        enableLeaveRequests: settingsQuery.data.enableLeaveRequests,
        enableBugTracking: settingsQuery.data.enableBugTracking,
        enableRiskRegister: settingsQuery.data.enableRiskRegister
      }
    : defaultSettings;
  const form: ProjectSettingsRequest = { ...savedSettings, ...overrides };

  const saveMutation = useMutation({
    mutationFn: () => managementApi.projects.settings.update(projectId!, form),
    onSuccess: async () => {
      showToast('Project settings saved.');
      setOverrides({});
      await queryClient.invalidateQueries({ queryKey: ['projects', projectId, 'settings'] });
    },
    onError: (error) => showToast(getApiErrorMessage(error, 'Unable to save project settings.'), 'error')
  });

  if (!projectId) {
    return <Navigate to={routePaths.projects} replace />;
  }

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Project Settings"
        description={projectQuery.data?.name}
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.app },
          { label: 'Projects', path: routePaths.projects },
          { label: projectQuery.data?.name ?? 'Project', path: routePaths.projectDetails(projectId) },
          { label: 'Settings' }
        ]}
        actions={(
          <>
          <Button component={RouterLink} to={routePaths.projectDetails(projectId)} startIcon={<ArrowLeft size={16} />} variant="outlined">Project Details</Button>
          {canManageSettings ? (
            <Button startIcon={<Save size={16} />} variant="contained" disabled={saveMutation.isPending || settingsQuery.isLoading} onClick={() => saveMutation.mutate()}>
              Save
            </Button>
          ) : null}
          </>
        )}
      />

      <Paper sx={{ p: 3, border: '1px solid', borderColor: 'divider', borderRadius: 3 }}>
        {settingsQuery.isLoading ? (
          <Stack spacing={2}>
            {Array.from({ length: 6 }).map((_, index) => <Skeleton key={index} height={40} />)}
          </Stack>
        ) : (
          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'repeat(2, minmax(0, 1fr))' }, gap: 2 }}>
            {settingFields.map((field) => (
              <FormControlLabel
                key={field.key}
                control={
                  <Switch
                    disabled={!canManageSettings}
                    checked={form[field.key]}
                    onChange={(event) => setOverrides((current) => ({ ...current, [field.key]: event.target.checked }))}
                  />
                }
                label={field.label}
              />
            ))}
          </Box>
        )}
      </Paper>
    </Stack>
  );
}
