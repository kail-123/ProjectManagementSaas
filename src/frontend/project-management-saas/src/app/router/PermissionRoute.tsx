import type { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { usePermissions } from '../../features/auth/model/usePermissions';
import type { PermissionCode } from '../../shared/security/permissions';
import { routePaths } from './routePaths';

export function PermissionRoute({
  permission,
  children
}: {
  permission?: PermissionCode;
  children: ReactNode;
}) {
  const { hasPermission } = usePermissions();

  if (!hasPermission(permission)) {
    return <Navigate to={routePaths.unauthorized} replace />;
  }

  return children;
}
