import { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Badge } from '../ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../ui/tabs';
import { StudentEnrollment } from './StudentEnrollment';
import { EnrollmentReport } from './EnrollmentReport';
import { enrollmentApi, type EnrollmentDto, EnrollmentStatus, type PagedResult } from '../../services/enrollmentApi';

// Ensure we have the correct enum values matching the backend
const EnrollmentStatusValues = {
  Active: EnrollmentStatus.Value1,
  Completed: EnrollmentStatus.Value2,
  Dropped: EnrollmentStatus.Value3,
  Suspended: EnrollmentStatus.Value4,
} as const;
import { courseApi, type CourseDto } from '../../services/courseApi';
import { toast } from 'sonner';
import { Search, Users, Plus, FileText, Trash2, ChevronLeft, ChevronRight } from 'lucide-react';

export function EnrollmentManagement() {
  const [enrollmentData, setEnrollmentData] = useState<PagedResult<EnrollmentDto> | null>(null);
  const [courses, setCourses] = useState<CourseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<number | 'all'>('all');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(10);
  const [showStudentForm, setShowStudentForm] = useState(false);
  const [selectedCourseForReport, setSelectedCourseForReport] = useState<string | null>(null);

  const fetchData = async () => {
    try {
      setLoading(true);
      const search = searchTerm.trim() || undefined;
      const status = statusFilter === 'all' ? undefined : statusFilter;
      const [fetchedEnrollments, fetchedCourses] = await Promise.all([
        enrollmentApi.getAllEnrollments(search, status, currentPage, pageSize),
        courseApi.getAllCourses()
      ]);
      setEnrollmentData(fetchedEnrollments);
      setCourses(fetchedCourses);
    } catch (error) {
      toast.error('Failed to fetch data');
      console.error('Error fetching data:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [searchTerm, statusFilter, currentPage]);

  const handleSearch = (term: string) => {
    setSearchTerm(term);
    setCurrentPage(1); // Reset to first page when searching
  };

  const handleStatusFilter = (status: number | 'all') => {
    setStatusFilter(status);
    setCurrentPage(1); // Reset to first page when filtering
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handleUpdateStatus = async (enrollmentId: string, status: EnrollmentStatus, grade?: number) => {
    try {
      await enrollmentApi.updateEnrollmentStatus(enrollmentId, { 
        status: status, 
        grade: grade || null 
      });
      toast.success('Enrollment status updated');
      fetchData();
    } catch (error) {
      toast.error('Failed to update enrollment status');
      console.error('Error updating status:', error);
    }
  };

  const handleDeleteEnrollment = async (enrollmentId: string) => {
    if (!confirm('Are you sure you want to delete this enrollment?')) return;

    try {
      await enrollmentApi.deleteEnrollment(enrollmentId);
      toast.success('Enrollment deleted successfully');
      fetchData();
    } catch (error) {
      toast.error('Failed to delete enrollment');
      console.error('Error deleting enrollment:', error);
    }
  };

  const getStatusBadge = (status: EnrollmentStatus) => {
    const variants = {
      [EnrollmentStatusValues.Active]: { variant: 'default' as const, label: 'Active' },
      [EnrollmentStatusValues.Completed]: { variant: 'secondary' as const, label: 'Completed' },
      [EnrollmentStatusValues.Dropped]: { variant: 'destructive' as const, label: 'Dropped' },
      [EnrollmentStatusValues.Suspended]: { variant: 'outline' as const, label: 'Suspended' },
    };
    
    const config = variants[status as keyof typeof variants];
    
    // Fallback if status is not found in variants
    if (!config) {
      console.warn('Unknown enrollment status:', status);
      return <Badge variant="outline">Unknown ({status})</Badge>;
    }
    
    return <Badge variant={config.variant}>{config.label}</Badge>;
  };

  const enrollments = enrollmentData?.items || [];
  const totalCount = enrollmentData?.totalCount || 0;
  const totalPages = enrollmentData?.totalPages || 0;

  const formatDate = (dateString?: string) => {
    return dateString ? new Date(dateString).toLocaleDateString() : 'N/A';
  };

  if (showStudentForm) {
    return (
      <StudentEnrollment
        courses={courses}
        onClose={() => {
          setShowStudentForm(false);
          fetchData();
        }}
      />
    );
  }

  if (selectedCourseForReport) {
    return (
      <EnrollmentReport
        courseId={selectedCourseForReport}
        onClose={() => setSelectedCourseForReport(null)}
      />
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-2xl font-bold">Enrollment Management</h2>
          <p className="text-muted-foreground">
            View and manage student enrollments
          </p>
        </div>
        <div className="flex gap-2">
          <Button onClick={() => setShowStudentForm(true)}>
            <Plus className="mr-2 h-4 w-4" />
            Enroll Student
          </Button>
        </div>
      </div>

      <Tabs defaultValue="enrollments" className="space-y-6">
        <TabsList>
          <TabsTrigger value="enrollments">All Enrollments</TabsTrigger>
          <TabsTrigger value="reports">Course Reports</TabsTrigger>
        </TabsList>

        <TabsContent value="enrollments" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Search & Filter</CardTitle>
              <CardDescription>
                Find enrollments by student or course information
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex gap-4">
                <div className="relative flex-1">
                  <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                  <Input
                    placeholder="Search by student name, email, or course..."
                    value={searchTerm}
                    onChange={(e) => handleSearch(e.target.value)}
                    className="pl-10"
                  />
                </div>
                <select
                  value={statusFilter}
                  onChange={(e) => handleStatusFilter(e.target.value === 'all' ? 'all' : parseInt(e.target.value))}
                  className="flex h-10 rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                >
                  <option value="all">All Status</option>
                  <option value={EnrollmentStatusValues.Active}>Active</option>
                  <option value={EnrollmentStatusValues.Completed}>Completed</option>
                  <option value={EnrollmentStatusValues.Dropped}>Dropped</option>
                  <option value={EnrollmentStatusValues.Suspended}>Suspended</option>
                </select>
              </div>
            </CardContent>
          </Card>

          {loading ? (
            <Card>
              <CardContent className="flex items-center justify-center py-8">
                <div className="text-center">
                  <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto mb-4"></div>
                  <p className="text-muted-foreground">Loading enrollments...</p>
                </div>
              </CardContent>
            </Card>
          ) : enrollments.length === 0 ? (
            <Card>
              <CardContent className="flex items-center justify-center py-8">
                <div className="text-center">
                  <Users className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
                  <h3 className="text-lg font-semibold mb-2">No enrollments found</h3>
                  <p className="text-muted-foreground mb-4">
                    {searchTerm || statusFilter !== 'all' 
                      ? 'No enrollments match your search criteria.' 
                      : 'No student enrollments found.'}
                  </p>
                  <Button onClick={() => setShowStudentForm(true)}>
                    <Plus className="mr-2 h-4 w-4" />
                    Enroll Student
                  </Button>
                </div>
              </CardContent>
            </Card>
          ) : (
            <div className="space-y-4">
              {enrollments.map((enrollment) => (
                <Card key={enrollment.id}>
                  <CardContent className="p-6">
                    <div className="flex justify-between items-start">
                      <div className="flex-1 space-y-2">
                        <div className="flex items-center gap-4">
                          <h3 className="font-semibold text-lg">{enrollment.studentName || 'Unknown Student'}</h3>
                          {enrollment.status && getStatusBadge(enrollment.status)}
                        </div>
                        
                        <div className="text-sm text-muted-foreground space-y-1">
                          <p>{enrollment.studentEmail || 'No email'}</p>
                          <p><strong>Course:</strong> {enrollment.courseTitle || 'Unknown Course'} ({enrollment.courseCode || 'N/A'})</p>
                          <p><strong>Enrolled:</strong> {formatDate(enrollment.enrollmentDate)}</p>
                          {enrollment.grade && (
                            <p><strong>Grade:</strong> {enrollment.grade}%</p>
                          )}
                          {enrollment.completionDate && (
                            <p><strong>Completed:</strong> {formatDate(enrollment.completionDate)}</p>
                          )}
                        </div>
                      </div>
                      
                      <div className="flex gap-2">
                        <select
                          value={enrollment.status || EnrollmentStatusValues.Active}
                          onChange={(e) => handleUpdateStatus(enrollment.id!, parseInt(e.target.value) as EnrollmentStatus)}
                          className="text-sm border rounded px-2 py-1"
                        >
                          <option value={EnrollmentStatusValues.Active}>Active</option>
                          <option value={EnrollmentStatusValues.Completed}>Completed</option>
                          <option value={EnrollmentStatusValues.Dropped}>Dropped</option>
                          <option value={EnrollmentStatusValues.Suspended}>Suspended</option>
                        </select>
                        
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handleDeleteEnrollment(enrollment.id!)}
                          className="text-red-600 hover:text-red-700"
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}
              
              {/* Pagination Controls */}
              {totalPages > 1 && (
                <div className="flex items-center justify-between mt-6">
                  <div className="text-sm text-muted-foreground">
                    Showing {((currentPage - 1) * pageSize) + 1} to {Math.min(currentPage * pageSize, totalCount)} of {totalCount} enrollments
                  </div>
                  <div className="flex items-center space-x-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handlePageChange(currentPage - 1)}
                      disabled={currentPage === 1}
                    >
                      <ChevronLeft className="h-4 w-4" />
                      Previous
                    </Button>
                    
                    <div className="flex items-center space-x-1">
                      {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                        const page = i + 1;
                        return (
                          <Button
                            key={page}
                            variant={currentPage === page ? "default" : "outline"}
                            size="sm"
                            onClick={() => handlePageChange(page)}
                            className="w-8"
                          >
                            {page}
                          </Button>
                        );
                      })}
                      {totalPages > 5 && <span className="text-muted-foreground">...</span>}
                    </div>
                    
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handlePageChange(currentPage + 1)}
                      disabled={currentPage === totalPages}
                    >
                      Next
                      <ChevronRight className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              )}
            </div>
          )}
        </TabsContent>

        <TabsContent value="reports" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Course Reports</CardTitle>
              <CardDescription>
                Generate detailed enrollment reports for each course
              </CardDescription>
            </CardHeader>
            <CardContent>
              {courses.length === 0 ? (
                <p className="text-muted-foreground">No courses available for reporting.</p>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                  {courses.map((course) => (
                    <Card key={course.id} className="hover:shadow-md transition-shadow">
                      <CardHeader>
                        <CardTitle className="text-lg">{course.title}</CardTitle>
                        <CardDescription>{course.courseCode}</CardDescription>
                      </CardHeader>
                      <CardContent>
                        <div className="space-y-2 text-sm">
                          <p><strong>Enrollments:</strong> {course.currentEnrollments || 0}/{course.maxEnrollments || 0}</p>
                          <p><strong>Duration:</strong> {formatDate(course.startDate)} - {formatDate(course.endDate)}</p>
                        </div>
                        <Button
                          className="w-full mt-4"
                          onClick={() => setSelectedCourseForReport(course.id!)}
                        >
                          <FileText className="mr-2 h-4 w-4" />
                          View Report
                        </Button>
                      </CardContent>
                    </Card>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}