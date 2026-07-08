import { useMemo } from 'react';
import { useAuthStore } from './authStore';
import type { PermissionCode } from '../../../shared/security/permissions';

const emptyPermissionCodes: PermissionCode[] = [];

export function usePermissions() {
  const user = useAuthStore((state) => state.user);
  const permissionCodes = user?.permissions ?? emptyPermissionCodes;
  const isSystemAdministrator = user?.roles.includes('SystemAdministrator') ?? false;
  const permissionSet = useMemo(() => new Set(permissionCodes), [permissionCodes]);

  return {
    permissionCodes,
    hasPermission: (permission?: PermissionCode) => isSystemAdministrator || !permission || permissionSet.has(permission),
    hasAnyPermission: (permissions: readonly PermissionCode[]) =>
      isSystemAdministrator || permissions.length === 0 || permissions.some((permission) => permissionSet.has(permission))
  };
}
