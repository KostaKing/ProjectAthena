import { useState, useEffect } from 'react';
import { useAuth } from '../../hooks/useAuth';
import { UserRole } from '../../types/auth';
import { AdminDashboard } from './AdminDashboard';
import { Header } from '../layout/Header';
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card';
import { BookOpen, GraduationCap } from 'lucide-react';
import { courseApi, type CourseDto } from '../../services/courseApi';
import { enrollmentApi, type EnrollmentDto } from '../../services/enrollmentApi';
import { toast } from 'sonner';

export function Dashboard() {
  const { user } = useAuth();
  const [courses, setCourses] = useState<CourseDto[]>([]);
  const [enrollments, setEnrollments] = useState<EnrollmentDto[]>([]);
  const [loading, setLoading] = useState(true);

  // Route admin users to the dedicated AdminDashboard
  if (user?.role === UserRole.Value3) {
    return <AdminDashboard />;
  }

  useEffect(() => {
    if (user) {
      fetchDashboardData();
    }
  }, [user]);

  const fetchDashboardData = async () => {
    try {
      setLoading(true);
      if (user?.role === UserRole.Value2) { // Teacher
        const allCourses = await courseApi.getAllCourses();
        // Filter courses where user is the instructor
        const teacherCourses = allCourses.filter((course: CourseDto) => course.instructorId === user.id);
        setCourses(teacherCourses);
      } else if (user?.role === UserRole.Value1) { // Student
        const userEnrollmentsResult = await enrollmentApi.getAllEnrollments();
        // Filter enrollments for current user
        const studentEnrollments = userEnrollmentsResult.items.filter((enrollment: EnrollmentDto) => enrollment.studentId === user.id);
        setEnrollments(studentEnrollments);
        
        // Get course details for enrolled courses
        if (studentEnrollments.length > 0) {
          const allCourses = await courseApi.getAllCourses();
          const enrolledCourses = allCourses.filter((course: CourseDto) => 
            studentEnrollments.some((enrollment: EnrollmentDto) => enrollment.courseId === course.id)
          );
          setCourses(enrolledCourses);
        }
      }
    } catch (error) {
      toast.error('Failed to load dashboard data');
      console.error('Error fetching dashboard data:', error);
    } finally {
      setLoading(false);
    }
  };

  const getRoleSpecificContent = () => {
    if (!user) return null;

    switch (user.role) {
      case UserRole.Value2: // Teacher
        return (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">My Classes</CardTitle>
                <BookOpen className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{loading ? '...' : courses.length}</div>
                <p className="text-xs text-muted-foreground">
                  {courses.length === 1 ? 'Active course' : 'Active courses'}
                </p>
                {!loading && courses.length > 0 && (
                  <div className="mt-4 space-y-2">
                    <p className="text-sm font-medium">Recent courses:</p>
                    {courses.slice(0, 3).map(course => (
                      <div key={course.id} className="text-sm text-muted-foreground">
                        {course.title} ({course.courseCode})
                      </div>
                    ))}
                  </div>
                )}
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Total Students</CardTitle>
                <GraduationCap className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{loading ? '...' : courses.reduce((total, course) => total + (course.currentEnrollments || 0), 0)}</div>
                <p className="text-xs text-muted-foreground">
                  Across all your courses
                </p>
              </CardContent>
            </Card>
          </div>
        );
      case UserRole.Value1: // Student
        return (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">My Courses</CardTitle>
                <BookOpen className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{loading ? '...' : courses.length}</div>
                <p className="text-xs text-muted-foreground">
                  {courses.length === 1 ? 'Enrolled course' : 'Enrolled courses'}
                </p>
                {!loading && courses.length > 0 && (
                  <div className="mt-4 space-y-2">
                    <p className="text-sm font-medium">Current courses:</p>
                    {courses.slice(0, 3).map(course => (
                      <div key={course.id} className="text-sm text-muted-foreground">
                        {course.title} ({course.courseCode})
                      </div>
                    ))}
                  </div>
                )}
                {!loading && courses.length === 0 && (
                  <p className="text-sm text-muted-foreground mt-2">
                    No courses enrolled yet.
                  </p>
                )}
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Enrollments</CardTitle>
                <GraduationCap className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{loading ? '...' : enrollments.length}</div>
                <p className="text-xs text-muted-foreground">
                  Total enrollments
                </p>
                {!loading && enrollments.length > 0 && (
                  <div className="mt-4 space-y-1">
                    <p className="text-sm font-medium">Recent enrollments:</p>
                    {enrollments.slice(-3).map(enrollment => (
                      <div key={enrollment.id} className="text-sm text-muted-foreground">
                        Status: {enrollment.status}
                      </div>
                    ))}
                  </div>
                )}
                {!loading && enrollments.length === 0 && (
                  <p className="text-sm text-muted-foreground mt-2">
                    No enrollments found.
                  </p>
                )}
              </CardContent>
            </Card>
          </div>
        );
      default:
        return null;
    }
  };

  return (
    <div className="min-h-screen bg-background">
      <Header />
      
      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <div className="mb-8">
            <h1 className="text-3xl font-bold text-foreground">
              Welcome back, {user?.firstName}!
            </h1>
            <p className="mt-2 text-muted-foreground">
              Here's what's happening in your ProjectAthena dashboard.
            </p>
          </div>

          {getRoleSpecificContent()}
        </div>
      </main>
    </div>
  );
}