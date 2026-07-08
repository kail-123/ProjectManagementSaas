import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuthStore } from '../../features/auth/model/authStore';
import { routePaths } from './routePaths';

export function ProtectedRoute() {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to={routePaths.signIn} replace state={{ from: location }} />;
  }

  return <Outlet />;
}
