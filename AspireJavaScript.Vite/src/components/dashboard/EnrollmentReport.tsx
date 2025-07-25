import { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Button } from '../ui/button';
import { Badge } from '../ui/badge';
import { enrollmentApi, type EnrollmentReportDto, EnrollmentStatus } from '../../services/enrollmentApi';
import { toast } from 'sonner';
import { ArrowLeft, Download, Users, TrendingUp, Calendar, BookOpen, GraduationCap } from 'lucide-react';

interface EnrollmentReportProps {
  courseId: string;
  onClose: () => void;
}

export function EnrollmentReport({ courseId, onClose }: EnrollmentReportProps) {
  const [report, setReport] = useState<EnrollmentReportDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchReport();
  }, [courseId]);

  const fetchReport = async () => {
    try {
      setLoading(true);
      const reportData = await enrollmentApi.generateEnrollmentReport(courseId);
      setReport(reportData);
    } catch (error) {
      toast.error('Failed to generate enrollment report');
      console.error('Error generating report:', error);
    } finally {
      setLoading(false);
    }
  };

  const getStatusBadge = (status: EnrollmentStatus) => {
    const variants = {
      [EnrollmentStatus.Value1]: { variant: 'default' as const, label: 'Active' },
      [EnrollmentStatus.Value2]: { variant: 'secondary' as const, label: 'Completed' },
      [EnrollmentStatus.Value3]: { variant: 'destructive' as const, label: 'Dropped' },
      [EnrollmentStatus.Value4]: { variant: 'outline' as const, label: 'Suspended' },
    };
    
    const config = variants[status];
    return <Badge variant={config.variant}>{config.label}</Badge>;
  };

  const formatDate = (dateString?: string) => {
    return dateString ? new Date(dateString).toLocaleDateString() : 'N/A';
  };

  const exportReport = () => {
    if (!report) return;

    const csvContent = [
      ['Student Name', 'Email', 'Enrollment Date', 'Status', 'Grade', 'Completion Date'],
      ...(report.studentEnrollments || []).map(enrollment => [
        enrollment.studentName || '',
        enrollment.studentEmail || '',
        formatDate(enrollment.enrollmentDate),
        enrollment.status ? EnrollmentStatus[enrollment.status] : '',
        enrollment.grade?.toString() || '',
        enrollment.completionDate ? formatDate(enrollment.completionDate) : ''
      ])
    ].map(row => row.map(cell => `"${cell}"`).join(',')).join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${report.courseCode || 'course'}_enrollment_report.csv`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
    
    toast.success('Report exported successfully');
  };

  if (loading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="outline" onClick={onClose}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Enrollments
          </Button>
          <div>
            <h2 className="text-2xl font-bold">Course Enrollment Report</h2>
            <p className="text-muted-foreground">Loading report data...</p>
          </div>
        </div>
        
        <Card>
          <CardContent className="flex items-center justify-center py-12">
            <div className="text-center">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto mb-4"></div>
              <p className="text-muted-foreground">Generating enrollment report...</p>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (!report) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="outline" onClick={onClose}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Enrollments
          </Button>
          <div>
            <h2 className="text-2xl font-bold">Course Enrollment Report</h2>
            <p className="text-muted-foreground">Failed to load report</p>
          </div>
        </div>
        
        <Card>
          <CardContent className="flex items-center justify-center py-12">
            <div className="text-center">
              <p className="text-muted-foreground mb-4">Unable to generate report for this course</p>
              <Button onClick={fetchReport}>Try Again</Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="outline" onClick={onClose}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Enrollments
          </Button>
          <div>
            <h2 className="text-2xl font-bold">Course Enrollment Report</h2>
            <p className="text-muted-foreground">
              {report.courseTitle} ({report.courseCode})
            </p>
          </div>
        </div>
        
        <Button onClick={exportReport}>
          <Download className="mr-2 h-4 w-4" />
          Export CSV
        </Button>
      </div>

      {/* Course Overview */}
      <Card>
        <CardHeader>
          <CardTitle>Course Overview</CardTitle>
          <CardDescription>Basic course information and statistics</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            <div className="space-y-2">
              <div className="flex items-center gap-2">
                <BookOpen className="h-4 w-4 text-muted-foreground" />
                <span className="text-sm font-medium">Instructor</span>
              </div>
              <p className="text-2xl font-bold">{report.instructorName || 'No Instructor Assigned'}</p>
            </div>
            
            <div className="space-y-2">
              <div className="flex items-center gap-2">
                <Calendar className="h-4 w-4 text-muted-foreground" />
                <span className="text-sm font-medium">Duration</span>
              </div>
              <p className="text-lg font-semibold">
                {formatDate(report.startDate)} - {formatDate(report.endDate)}
              </p>
            </div>
            
            <div className="space-y-2">
              <div className="flex items-center gap-2">
                <Users className="h-4 w-4 text-muted-foreground" />
                <span className="text-sm font-medium">Total Enrollments</span>
              </div>
              <p className="text-2xl font-bold">{report.totalEnrollments || 0}</p>
            </div>
            
            <div className="space-y-2">
              <div className="flex items-center gap-2">
                <TrendingUp className="h-4 w-4 text-muted-foreground" />
                <span className="text-sm font-medium">Average Grade</span>
              </div>
              <p className="text-2xl font-bold">
                {(report.averageGrade || 0) > 0 ? `${report.averageGrade?.toFixed(1)}%` : 'N/A'}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Enrollment Statistics */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Active Enrollments</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">{report.activeEnrollments || 0}</div>
            <p className="text-xs text-muted-foreground">
              {report.totalEnrollments ? ((report.activeEnrollments || 0) / report.totalEnrollments * 100).toFixed(1) : 0}% of total
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Completed</CardTitle>
            <GraduationCap className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-blue-600">{report.completedEnrollments || 0}</div>
            <p className="text-xs text-muted-foreground">
              {report.totalEnrollments ? ((report.completedEnrollments || 0) / report.totalEnrollments * 100).toFixed(1) : 0}% completion rate
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Dropped</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-red-600">{report.droppedEnrollments || 0}</div>
            <p className="text-xs text-muted-foreground">
              {report.totalEnrollments ? ((report.droppedEnrollments || 0) / report.totalEnrollments * 100).toFixed(1) : 0}% drop rate
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Student Enrollments */}
      <Card>
        <CardHeader>
          <CardTitle>Student Enrollments</CardTitle>
          <CardDescription>
            Detailed list of all students enrolled in this course
          </CardDescription>
        </CardHeader>
        <CardContent>
          {(report.studentEnrollments?.length || 0) === 0 ? (
            <div className="text-center py-8">
              <Users className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
              <p className="text-muted-foreground">No students enrolled in this course</p>
            </div>
          ) : (
            <div className="space-y-4">
              {(report.studentEnrollments || []).map((enrollment, index) => (
                <div key={index} className="flex items-center justify-between p-4 border rounded-lg">
                  <div className="flex-1">
                    <h4 className="font-semibold">{enrollment.studentName || 'Unknown Student'}</h4>
                    <p className="text-sm text-muted-foreground">{enrollment.studentEmail || 'No email'}</p>
                    <p className="text-sm text-muted-foreground">
                      Enrolled: {formatDate(enrollment.enrollmentDate)}
                      {enrollment.completionDate && (
                        <> • Completed: {formatDate(enrollment.completionDate)}</>
                      )}
                    </p>
                  </div>
                  
                  <div className="flex items-center gap-4">
                    {enrollment.grade && (
                      <div className="text-right">
                        <p className="font-semibold">{enrollment.grade}%</p>
                        <p className="text-xs text-muted-foreground">Grade</p>
                      </div>
                    )}
                    {enrollment.status && getStatusBadge(enrollment.status)}
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}