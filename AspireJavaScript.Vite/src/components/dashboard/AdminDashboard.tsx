import { useState, useEffect } from 'react';
import { Header } from '../layout/Header';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Button } from '../ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../ui/tabs';
import { Badge } from '../ui/badge';
import { Progress } from '../ui/progress';
import { CourseManagement } from './CourseManagement';
import { EnrollmentManagement } from './EnrollmentManagement';
import { UserManagement } from './UserManagement';
import { BookOpen, Users, GraduationCap, Plus, Activity, RefreshCw, UserCheck, UserCog, AlertCircle, CheckCircle, Clock, BarChart3 } from 'lucide-react';
import { dashboardApi, type DashboardStatsDto } from '../../services/dashboardApi';
import { Skeleton } from '../ui/skeleton';
import { Alert, AlertDescription } from '../ui/alert';
import { toast } from 'sonner';

export function AdminDashboard() {
  const [activeTab, setActiveTab] = useState('overview');
  const [stats, setStats] = useState<DashboardStatsDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchDashboardStats();
  }, []);

  const fetchDashboardStats = async () => {
    try {
      setLoading(true);
      const data = await dashboardApi.getDashboardStats();
      setStats(data);
    } catch (error) {
      toast.error('Failed to load dashboard statistics');
      console.error('Error fetching dashboard stats:', error);
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getActivityIcon = (type: string) => {
    switch (type) {
      case 'enrollment':
        return <Users className="h-4 w-4 text-blue-500" />;
      case 'completion':
        return <GraduationCap className="h-4 w-4 text-green-500" />;
      case 'course_created':
        return <BookOpen className="h-4 w-4 text-purple-500" />;
      default:
        return <Activity className="h-4 w-4 text-gray-500" />;
    }
  };

  return (
    <div className="min-h-screen bg-background">
      <Header />
      
      <main className="container mx-auto py-6">
        <div className="space-y-6">
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div className="space-y-1">
              <h1 className="text-3xl font-bold tracking-tight">
                Learning Management Dashboard
              </h1>
              <p className="text-muted-foreground">
                Manage courses, enrollments, and generate reports for your educational platform.
              </p>
            </div>
            <Button 
              onClick={fetchDashboardStats} 
              disabled={loading}
              variant="outline"
              size="sm"
              className="shrink-0"
              aria-label="Refresh dashboard data"
            >
              <RefreshCw className={`mr-2 h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
              Refresh
            </Button>
          </div>

          <Tabs value={activeTab} onValueChange={setActiveTab} className="space-y-6">
            <TabsList className="grid w-full grid-cols-4 lg:w-fit lg:grid-cols-4">
              <TabsTrigger value="overview" className="text-sm">
                <Activity className="mr-2 h-4 w-4" />
                Overview
              </TabsTrigger>
              <TabsTrigger value="courses" className="text-sm">
                <BookOpen className="mr-2 h-4 w-4" />
                Courses
              </TabsTrigger>
              <TabsTrigger value="enrollments" className="text-sm">
                <Users className="mr-2 h-4 w-4" />
                Enrollments
              </TabsTrigger>
              <TabsTrigger value="users" className="text-sm">
                <UserCheck className="mr-2 h-4 w-4" />
                Users
              </TabsTrigger>
            </TabsList>

            <TabsContent value="overview" className="space-y-6">
              {loading ? (
                <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
                  {Array.from({ length: 4 }).map((_, i) => (
                    <Card key={i}>
                      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <Skeleton className="h-4 w-24" />
                        <Skeleton className="h-4 w-4 rounded" />
                      </CardHeader>
                      <CardContent>
                        <Skeleton className="h-8 w-16 mb-2" />
                        <Skeleton className="h-3 w-32" />
                      </CardContent>
                    </Card>
                  ))}
                </div>
              ) : !stats ? (
                <Alert>
                  <AlertDescription>
                    Failed to load dashboard statistics. Please try refreshing the page.
                  </AlertDescription>
                </Alert>
              ) : (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 lg:gap-6">
                  <Card className="group transition-all duration-300 hover:shadow-lg hover:scale-[1.02] border-l-4 border-l-blue-500 relative overflow-hidden">
                    <div className="absolute inset-0 bg-gradient-to-br from-blue-50/50 to-transparent dark:from-blue-950/20 pointer-events-none" />
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2 relative">
                      <CardTitle className="text-sm font-medium">Total Courses</CardTitle>
                      <div className="p-2 bg-blue-100 dark:bg-blue-900 rounded-full group-hover:scale-110 transition-transform">
                        <BookOpen className="h-4 w-4 text-blue-600 dark:text-blue-400" />
                      </div>
                    </CardHeader>
                    <CardContent className="relative">
                      <div className="text-3xl font-bold tracking-tight">{stats?.totalCourses || 0}</div>
                      <div className="flex items-center justify-between mt-2">
                        <p className="text-xs text-muted-foreground">
                          Active courses in system
                        </p>
                        <Badge variant="secondary" className="text-xs">
                          <Activity className="h-3 w-3 mr-1" />
                          Live
                        </Badge>
                      </div>
                    </CardContent>
                  </Card>

                  <Card className="group transition-all duration-300 hover:shadow-lg hover:scale-[1.02] border-l-4 border-l-green-500 relative overflow-hidden">
                    <div className="absolute inset-0 bg-gradient-to-br from-green-50/50 to-transparent dark:from-green-950/20 pointer-events-none" />
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2 relative">
                      <CardTitle className="text-sm font-medium">Total Enrollments</CardTitle>
                      <div className="p-2 bg-green-100 dark:bg-green-900 rounded-full group-hover:scale-110 transition-transform">
                        <Users className="h-4 w-4 text-green-600 dark:text-green-400" />
                      </div>
                    </CardHeader>
                    <CardContent className="relative">
                      <div className="text-3xl font-bold tracking-tight">{stats?.totalEnrollments || 0}</div>
                      <div className="flex items-center justify-between mt-2">
                        <p className="text-xs text-muted-foreground">
                          {stats?.activeEnrollments || 0} active, {stats?.completedEnrollments || 0} completed
                        </p>
                        {stats?.activeEnrollments && stats?.totalEnrollments && (
                          <Badge variant="outline" className="text-xs">
                            {Math.round((stats.activeEnrollments / stats.totalEnrollments) * 100)}% active
                          </Badge>
                        )}
                      </div>
                    </CardContent>
                  </Card>

                  <Card className="group transition-all duration-300 hover:shadow-lg hover:scale-[1.02] border-l-4 border-l-purple-500 relative overflow-hidden">
                    <div className="absolute inset-0 bg-gradient-to-br from-purple-50/50 to-transparent dark:from-purple-950/20 pointer-events-none" />
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2 relative">
                      <CardTitle className="text-sm font-medium">Completion Rate</CardTitle>
                      <div className="p-2 bg-purple-100 dark:bg-purple-900 rounded-full group-hover:scale-110 transition-transform">
                        <GraduationCap className="h-4 w-4 text-purple-600 dark:text-purple-400" />
                      </div>
                    </CardHeader>
                    <CardContent className="relative space-y-2">
                      <div className="text-3xl font-bold tracking-tight">{stats?.completionRate || 0}%</div>
                      <Progress value={stats?.completionRate || 0} className="h-2" />
                      <div className="flex items-center justify-between">
                        <p className="text-xs text-muted-foreground">
                          Course completion rate
                        </p>
                        <Badge 
                          variant={stats?.completionRate && stats.completionRate > 75 ? "default" : stats?.completionRate && stats.completionRate > 50 ? "secondary" : "destructive"}
                          className="text-xs"
                        >
                          {stats?.completionRate && stats.completionRate > 75 ? (
                            <><CheckCircle className="h-3 w-3 mr-1" />Excellent</>
                          ) : stats?.completionRate && stats.completionRate > 50 ? (
                            <><Clock className="h-3 w-3 mr-1" />Good</>
                          ) : (
                            <><AlertCircle className="h-3 w-3 mr-1" />Needs Attention</>
                          )}
                        </Badge>
                      </div>
                    </CardContent>
                  </Card>

                  <Card className="group transition-all duration-300 hover:shadow-lg hover:scale-[1.02] border-l-4 border-l-orange-500 relative overflow-hidden">
                    <div className="absolute inset-0 bg-gradient-to-br from-orange-50/50 to-transparent dark:from-orange-950/20 pointer-events-none" />
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2 relative">
                      <CardTitle className="text-sm font-medium">Average Grade</CardTitle>
                      <div className="p-2 bg-orange-100 dark:bg-orange-900 rounded-full group-hover:scale-110 transition-transform">
                        <BarChart3 className="h-4 w-4 text-orange-600 dark:text-orange-400" />
                      </div>
                    </CardHeader>
                    <CardContent className="relative space-y-2">
                      <div className="text-3xl font-bold tracking-tight">
                        {stats?.averageGrade ? `${stats.averageGrade}%` : '-'}
                      </div>
                      {stats?.averageGrade && (
                        <Progress value={stats.averageGrade} className="h-2" />
                      )}
                      <div className="flex items-center justify-between">
                        <p className="text-xs text-muted-foreground">
                          Overall student performance
                        </p>
                        {stats?.averageGrade && (
                          <Badge 
                            variant={stats.averageGrade >= 90 ? "default" : stats.averageGrade >= 80 ? "secondary" : stats.averageGrade >= 70 ? "outline" : "destructive"}
                            className="text-xs"
                          >
                            {stats.averageGrade >= 90 ? 'A' : stats.averageGrade >= 80 ? 'B' : stats.averageGrade >= 70 ? 'C' : 'D'}
                          </Badge>
                        )}
                      </div>
                    </CardContent>
                  </Card>
                </div>
              )}

              {!loading && (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 lg:gap-6">
                  <Card className="group transition-all duration-300 hover:shadow-lg hover:scale-[1.02] relative overflow-hidden">
                    <div className="absolute inset-0 bg-gradient-to-br from-blue-50/30 to-transparent dark:from-blue-950/10 pointer-events-none" />
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2 relative">
                      <CardTitle className="text-sm font-medium">Total Students</CardTitle>
                      <div className="p-2 bg-blue-100 dark:bg-blue-900 rounded-full group-hover:scale-110 transition-transform">
                        <UserCheck className="h-4 w-4 text-blue-600 dark:text-blue-400" />
                      </div>
                    </CardHeader>
                    <CardContent className="relative">
                      <div className="text-3xl font-bold tracking-tight">{stats?.totalStudents || 0}</div>
                      <div className="flex items-center justify-between mt-2">
                        <p className="text-xs text-muted-foreground">
                          Registered students
                        </p>
                        <Badge variant="secondary" className="text-xs">
                          Active
                        </Badge>
                      </div>
                    </CardContent>
                  </Card>

                  <Card className="group transition-all duration-300 hover:shadow-lg hover:scale-[1.02] relative overflow-hidden">
                    <div className="absolute inset-0 bg-gradient-to-br from-green-50/30 to-transparent dark:from-green-950/10 pointer-events-none" />
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2 relative">
                      <CardTitle className="text-sm font-medium">Total Teachers</CardTitle>
                      <div className="p-2 bg-green-100 dark:bg-green-900 rounded-full group-hover:scale-110 transition-transform">
                        <UserCog className="h-4 w-4 text-green-600 dark:text-green-400" />
                      </div>
                    </CardHeader>
                    <CardContent className="relative">
                      <div className="text-3xl font-bold tracking-tight">{stats?.totalTeachers || 0}</div>
                      <div className="flex items-center justify-between mt-2">
                        <p className="text-xs text-muted-foreground">
                          Active instructors
                        </p>
                        <Badge variant="outline" className="text-xs">
                          Teaching
                        </Badge>
                      </div>
                    </CardContent>
                  </Card>

                  <Card className="group transition-all duration-300 hover:shadow-lg hover:scale-[1.02] relative overflow-hidden">
                    <div className="absolute inset-0 bg-gradient-to-br from-purple-50/30 to-transparent dark:from-purple-950/10 pointer-events-none" />
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2 relative">
                      <CardTitle className="text-sm font-medium">Student-Teacher Ratio</CardTitle>
                      <div className="p-2 bg-purple-100 dark:bg-purple-900 rounded-full group-hover:scale-110 transition-transform">
                        <Users className="h-4 w-4 text-purple-600 dark:text-purple-400" />
                      </div>
                    </CardHeader>
                    <CardContent className="relative">
                      <div className="text-3xl font-bold tracking-tight">
                        {stats?.totalTeachers && stats?.totalStudents 
                          ? Math.round((stats.totalStudents / stats.totalTeachers) * 10) / 10
                          : '-'
                        }:1
                      </div>
                      <div className="flex items-center justify-between mt-2">
                        <p className="text-xs text-muted-foreground">
                          Students per teacher
                        </p>
                        <Badge 
                          variant={stats?.totalTeachers && stats?.totalStudents && (stats.totalStudents / stats.totalTeachers) <= 20 ? "default" : "secondary"}
                          className="text-xs"
                        >
                          {stats?.totalTeachers && stats?.totalStudents && (stats.totalStudents / stats.totalTeachers) <= 20 ? 'Optimal' : 'High'}
                        </Badge>
                      </div>
                    </CardContent>
                  </Card>
                </div>
              )}

              <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 lg:gap-6">
                <Card className="transition-all duration-200 hover:shadow-md">
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <div className="p-1 bg-primary/10 rounded">
                        <Plus className="h-4 w-4 text-primary" />
                      </div>
                      Quick Actions
                    </CardTitle>
                    <CardDescription>
                      Common administrative tasks
                    </CardDescription>
                  </CardHeader>
                  <CardContent className="space-y-3">
                    <Button 
                      onClick={() => setActiveTab('courses')} 
                      className="w-full justify-start h-auto py-3"
                      variant="outline"
                    >
                      <Plus className="mr-3 h-4 w-4" />
                      <div className="text-left">
                        <div className="font-medium">Add New Course</div>
                        <div className="text-xs text-muted-foreground">Create a new course offering</div>
                      </div>
                    </Button>
                    <Button 
                      onClick={() => setActiveTab('enrollments')} 
                      className="w-full justify-start h-auto py-3"
                      variant="outline"
                    >
                      <Users className="mr-3 h-4 w-4" />
                      <div className="text-left">
                        <div className="font-medium">Manage Enrollments</div>
                        <div className="text-xs text-muted-foreground">View and manage student enrollments</div>
                      </div>
                    </Button>
                  </CardContent>
                </Card>

                <Card className="transition-all duration-200 hover:shadow-md">
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <div className="p-1 bg-primary/10 rounded">
                        <Activity className="h-4 w-4 text-primary" />
                      </div>
                      Recent Activity
                    </CardTitle>
                    <CardDescription>
                      Latest system activities
                    </CardDescription>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    {loading ? (
                      <div className="flex items-center justify-center py-8">
                        <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary"></div>
                      </div>
                    ) : stats?.recentActivities && stats.recentActivities.length > 0 ? (
                      <div className="space-y-4">
                        {stats.recentActivities.slice(0, 5).map((activity) => (
                          <div key={activity.id} className="flex items-start space-x-3 p-3 rounded-lg bg-muted/50 hover:bg-muted/70 transition-colors">
                            <div className="mt-0.5">
                              {getActivityIcon(activity.type)}
                            </div>
                            <div className="flex-1 min-w-0 space-y-1">
                              <p className="text-sm font-medium leading-tight">{activity.description}</p>
                              <p className="text-xs text-muted-foreground">{formatDate(activity.timestamp)}</p>
                            </div>
                          </div>
                        ))}
                      </div>
                    ) : (
                      <div className="text-center py-8">
                        <Activity className="h-8 w-8 text-muted-foreground mx-auto mb-2" />
                        <p className="text-sm text-muted-foreground">
                          No recent activity to display.
                        </p>
                      </div>
                    )}
                  </CardContent>
                </Card>
              </div>
            </TabsContent>

            <TabsContent value="courses">
              <CourseManagement />
            </TabsContent>

            <TabsContent value="enrollments">
              <EnrollmentManagement />
            </TabsContent>

            <TabsContent value="users">
              <UserManagement />
            </TabsContent>
          </Tabs>
        </div>
      </main>
    </div>
  );
}