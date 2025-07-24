import { useAuth } from '../../hooks/useAuth';
import { UserRole } from '../../types/auth';
import { AdminDashboard } from './AdminDashboard';
import { Header } from '../layout/Header';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Users, BookOpen, Settings } from 'lucide-react';

export function Dashboard() {
  const { user } = useAuth();

  // Route admin users to the dedicated AdminDashboard
  if (user?.role === UserRole.Value3) {
    return <AdminDashboard />;
  }

  const getRoleSpecificContent = () => {
    if (!user) return null;

    switch (user.role) {
      case UserRole.Value3: // Admin
        return (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Total Users</CardTitle>
                <Users className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">3</div>
                <p className="text-xs text-muted-foreground">
                  +0% from last month
                </p>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">System Status</CardTitle>
                <Settings className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-green-600">Online</div>
                <p className="text-xs text-muted-foreground">
                  All systems operational
                </p>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Active Sessions</CardTitle>
                <BookOpen className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">1</div>
                <p className="text-xs text-muted-foreground">
                  Current active users
                </p>
              </CardContent>
            </Card>
          </div>
        );
      case UserRole.Value2: // Teacher
        return (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
            <Card>
              <CardHeader>
                <CardTitle>My Classes</CardTitle>
                <CardDescription>
                  Manage your classes and students
                </CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-sm text-muted-foreground">
                  No classes assigned yet.
                </p>
              </CardContent>
            </Card>
            <Card>
              <CardHeader>
                <CardTitle>Recent Activity</CardTitle>
                <CardDescription>
                  Your recent teaching activities
                </CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-sm text-muted-foreground">
                  No recent activity.
                </p>
              </CardContent>
            </Card>
          </div>
        );
      case UserRole.Value1: // Student
        return (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
            <Card>
              <CardHeader>
                <CardTitle>My Courses</CardTitle>
                <CardDescription>
                  Your enrolled courses
                </CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-sm text-muted-foreground">
                  No courses enrolled yet.
                </p>
              </CardContent>
            </Card>
            <Card>
              <CardHeader>
                <CardTitle>Assignments</CardTitle>
                <CardDescription>
                  Upcoming assignments and deadlines
                </CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-sm text-muted-foreground">
                  No assignments due.
                </p>
              </CardContent>
            </Card>
          </div>
        );
      default:
        return null;
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <Header />
      
      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <div className="mb-8">
            <h1 className="text-3xl font-bold text-gray-900">
              Welcome back, {user?.firstName}!
            </h1>
            <p className="mt-2 text-gray-600">
              Here's what's happening in your ProjectAthena dashboard.
            </p>
          </div>

          {getRoleSpecificContent()}
        </div>
      </main>
    </div>
  );
}