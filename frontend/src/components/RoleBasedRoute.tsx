import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

interface RoleBasedRouteProps {
  children: React.ReactNode;
  /** Allowed roles for this route. Empty = any authenticated user. */
  allowedRoles?: string[];
}

/**
 * Protects routes by role. If user is not in allowedRoles, redirects to dashboard.
 * Use with ProtectedRoute so only authenticated users hit this.
 */
const RoleBasedRoute: React.FC<RoleBasedRouteProps> = ({ children, allowedRoles = [] }) => {
  const { user } = useAuth();

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  if (allowedRoles.length > 0 && !allowedRoles.includes(user.role)) {
    return <Navigate to="/dashboard" replace />;
  }

  return <>{children}</>;
};

export default RoleBasedRoute;
