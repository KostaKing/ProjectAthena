import { useAuth } from '../../hooks/useAuth';
import { UserRole } from '../../types/auth';
import { Button } from '../ui/button';
import { LogOut, User, Settings } from 'lucide-react';

export function Header() {
  const { user, logout } = useAuth();

  const getRoleColor = (role: UserRole) => {
    switch (role) {
      case UserRole.Value3: // Admin
        return 'bg-red-100 text-red-800';
      case UserRole.Value2: // Teacher
        return 'bg-blue-100 text-blue-800';
      case UserRole.Value1: // Student
        return 'bg-green-100 text-green-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getRoleName = (role: UserRole) => {
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
  };

  return (
    <header className="bg-white shadow-sm border-b">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          <div className="flex items-center">
            <h1 className="text-xl font-semibold text-gray-900">
              ProjectAthena
            </h1>
          </div>

          <div className="flex items-center space-x-4">
            {user && (
              <>
                <div className="flex items-center space-x-3 text-sm">
                  <div className="flex items-center space-x-2">
                    <User className="h-4 w-4 text-gray-500" />
                    <span className="text-gray-700">{user.fullName}</span>
                  </div>
                  <span
                    className={`px-2 py-1 rounded-full text-xs font-medium ${getRoleColor(user.role)}`}
                  >
                    {getRoleName(user.role)}
                  </span>
                </div>

                <div className="flex items-center space-x-2">
                  <Button variant="ghost" size="sm">
                    <Settings className="h-4 w-4" />
                  </Button>
                  <Button variant="ghost" size="sm" onClick={logout}>
                    <LogOut className="h-4 w-4" />
                  </Button>
                </div>
              </>
            )}
          </div>
        </div>
      </div>
    </header>
  );
}