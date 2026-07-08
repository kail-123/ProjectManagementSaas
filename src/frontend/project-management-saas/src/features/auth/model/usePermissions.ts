import { useMemo } from 'react';
import { useAuthStore } from './authStore';
import type { PermissionCode } from '../../../shared/security/permissions';

const emptyPermissionCodes: PermissionCode[] = [];

export function usePermissions() {
  const permissionCodes = useAuthStore((state) => state.user?.permissions ?? emptyPermissionCodes);
  const permissionSet = useMemo(() => new Set(permissionCodes), [permissionCodes]);

  return {
    permissionCodes,
    hasPermission: (permission?: PermissionCode) => !permission || permissionSet.has(permission),
    hasAnyPermission: (permissions: readonly PermissionCode[]) =>
      permissions.length === 0 || permissions.some((permission) => permissionSet.has(permission))
  };
}
