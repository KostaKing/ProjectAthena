import React from 'react';
import { useAuth } from '../../hooks/useAuth';
import { UserRole } from '../../types/auth';
import { AuthPage } from '../auth/AuthPage';
import { LoadingSpinner } from './LoadingSpinner';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRole?: UserRole;
  allowedRoles?: UserRole[];
}

export function ProtectedRoute({ 
  children, 
  requiredRole, 
  allowedRoles 
}: ProtectedRouteProps) {
  const { isAuthenticated, isLoading, user } = useAuth();

  if (isLoading) {
    return <LoadingSpinner />;
  }

  if (!isAuthenticated) {
    return <AuthPage />;
  }

  // Check role-based access
  if (user && (requiredRole || allowedRoles)) {
    const hasRequiredRole = requiredRole ? user.role === requiredRole : true;
    const hasAllowedRole = allowedRoles ? allowedRoles.includes(user.role) : true;

    if (!hasRequiredRole || !hasAllowedRole) {
      return (
        <div className="min-h-screen bg-gradient-to-br from-red-50 to-red-100 flex items-center justify-center">
          <div className="text-center">
            <div className="bg-white p-8 rounded-lg shadow-md">
              <h2 className="text-2xl font-bold text-red-600 mb-2">Access Denied</h2>
              <p className="text-gray-600">
                You don't have permission to access this page.
              </p>
              <p className="text-sm text-gray-500 mt-2">
                Required role: {requiredRole ? getRoleName(requiredRole) : allowedRoles?.map(getRoleName).join(', ')}
              </p>
            </div>
          </div>
        </div>
      );
    }
  }

  return <>{children}</>;
}

function getRoleName(role: UserRole): string {
  switch (role) {
    case UserRole.Value3: // Admin
      return 'Admin';
    case UserRole.Value2: // Teacher
      return 'Teacher';
    case UserRole.Value1: // Student
      return 'Student';
    default:
      return 'Unknown';
  }
}