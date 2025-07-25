import { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Label } from '../ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../ui/select';
import { Badge } from '../ui/badge';
// Note: Using basic HTML elements instead of missing UI components
// import { Separator } from '../ui/separator';
// import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '../ui/collapsible';
import { 
  enrollmentApi, 
  type EnrollmentReportRequestDto, 
  type EnrollmentReportResponseDto,
  EnrollmentStatus,
  ReportFormat,
  ReportGroupBy
} from '../../services/enrollmentApi';
import { courseApi, type CourseDto } from '../../services/courseApi';
import { studentApi, type StudentDto } from '../../services/studentApi';
import { toast } from 'sonner';
import { 
  ArrowLeft, 
  Download, 
  Users, 
  TrendingUp, 
  Calendar, 
  BookOpen, 
  GraduationCap,
  Filter,
  ChevronDown,
  FileText,
  BarChart3,
  PieChart,
  Loader2,
  Search
} from 'lucide-react';

interface AdvancedEnrollmentReportProps {
  onClose: () => void;
}

export function AdvancedEnrollmentReport({ onClose }: AdvancedEnrollmentReportProps) {
  const [report, setReport] = useState<EnrollmentReportResponseDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [courses, setCourses] = useState<CourseDto[]>([]);
  const [students, setStudents] = useState<StudentDto[]>([]);
  const [filtersExpanded, setFiltersExpanded] = useState(true);

  // Filter state
  const [filters, setFilters] = useState<EnrollmentReportRequestDto>({
    groupBy: ReportGroupBy.Course,
    format: ReportFormat.Json
  });

  useEffect(() => {
    loadInitialData();
  }, []);

  const loadInitialData = async () => {
    try {
      const [coursesData, studentsData] = await Promise.all([
        courseApi.getAllCourses(),
        studentApi.getAllStudents()
      ]);
      setCourses(coursesData);
      setStudents(studentsData.filter(s => s.isActive));
    } catch (error) {
      console.error('Error loading initial data:', error);
    }
  };

  const handleFilterChange = (key: keyof EnrollmentReportRequestDto, value: any) => {
    setFilters(prev => ({ ...prev, [key]: value }));
  };

  const generateReport = async () => {
    try {
      setLoading(true);
      const reportData = await enrollmentApi.generateAdvancedEnrollmentReport(filters);
      setReport(reportData);
      setFiltersExpanded(false);
    } catch (error) {
      toast.error('Failed to generate enrollment report');
      console.error('Error generating report:', error);
    } finally {
      setLoading(false);
    }
  };

  const clearFilters = () => {
    setFilters({
      groupBy: ReportGroupBy.Course,
      format: ReportFormat.Json
    });
  };

  const exportReport = (format: 'csv' | 'pdf') => {
    if (!report) return;

    if (format === 'csv') {
      const headers = [
        'Student Name', 'Student Email', 'Student Number', 'Course Code', 'Course Title', 
        'Instructor', 'Enrollment Date', 'Status', 'Grade', 'Completion Date'
      ];
      
      const csvContent = [
        headers,
        ...report.items.map(item => [
          item.studentName,
          item.studentEmail,
          item.studentNumber,
          item.courseCode,
          item.courseTitle,
          item.instructorName,
          new Date(item.enrollmentDate).toLocaleDateString(),
          EnrollmentStatus[item.status],
          item.grade?.toString() || '',
          item.completionDate ? new Date(item.completionDate).toLocaleDateString() : ''
        ])
      ].map(row => row.map(cell => `"${cell}"`).join(',')).join('\n');

      const blob = new Blob([csvContent], { type: 'text/csv' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `enrollment_report_${new Date().toISOString().split('T')[0]}.csv`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
      
      toast.success('Report exported as CSV');
    } else if (format === 'pdf') {
      toast.info('PDF export functionality coming soon');
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

  const getGroupByIcon = (groupBy: ReportGroupBy) => {
    switch (groupBy) {
      case ReportGroupBy.Course: return <BookOpen className="h-4 w-4" />;
      case ReportGroupBy.Student: return <Users className="h-4 w-4" />;
      case ReportGroupBy.Instructor: return <GraduationCap className="h-4 w-4" />;
      case ReportGroupBy.Status: return <BarChart3 className="h-4 w-4" />;
      case ReportGroupBy.Date: return <Calendar className="h-4 w-4" />;
      default: return <PieChart className="h-4 w-4" />;
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="outline" onClick={onClose}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Enrollments
          </Button>
          <div>
            <h2 className="text-2xl font-bold">Advanced Enrollment Reports</h2>
            <p className="text-muted-foreground">
              Generate comprehensive enrollment reports with custom filters
            </p>
          </div>
        </div>
        
        {report && (
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => exportReport('csv')}>
              <Download className="mr-2 h-4 w-4" />
              Export CSV
            </Button>
            <Button variant="outline" onClick={() => exportReport('pdf')}>
              <FileText className="mr-2 h-4 w-4" />
              Export PDF
            </Button>
          </div>
        )}
      </div>

      {/* Filters Section */}
      <Card>
        <div>
          <div onClick={() => setFiltersExpanded(!filtersExpanded)}>
            <CardHeader className="cursor-pointer hover:bg-muted/50 transition-colors">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Filter className="h-5 w-5" />
                  <CardTitle>Report Filters</CardTitle>
                </div>
                <ChevronDown className={`h-4 w-4 transition-transform ${filtersExpanded ? 'rotate-180' : ''}`} />
              </div>
              <CardDescription>
                Customize your report with various filters and grouping options
              </CardDescription>
            </CardHeader>
          </div>
          
          {filtersExpanded && (
            <CardContent className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {/* Course Filter */}
                <div className="space-y-2">
                  <Label htmlFor="course">Course</Label>
                  <Select value={filters.courseId || 'all'} onValueChange={(value) => handleFilterChange('courseId', value === 'all' ? undefined : value)}>
                    <SelectTrigger id="course">
                      <SelectValue placeholder="All courses" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">All courses</SelectItem>
                      {courses.map(course => (
                        <SelectItem key={course.id} value={course.id!}>
                          {course.courseCode} - {course.title}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {/* Student Filter */}
                <div className="space-y-2">
                  <Label htmlFor="student">Student</Label>
                  <Select value={filters.studentId || 'all'} onValueChange={(value) => handleFilterChange('studentId', value === 'all' ? undefined : value)}>
                    <SelectTrigger id="student">
                      <SelectValue placeholder="All students" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">All students</SelectItem>
                      {students.map(student => (
                        <SelectItem key={student.id} value={student.id!}>
                          {student.fullName} ({student.email})
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {/* Status Filter */}
                <div className="space-y-2">
                  <Label htmlFor="status">Status</Label>
                  <Select value={filters.status?.toString() || 'all'} onValueChange={(value) => handleFilterChange('status', value === 'all' ? undefined : parseInt(value))}>
                    <SelectTrigger id="status">
                      <SelectValue placeholder="All statuses" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">All statuses</SelectItem>
                      <SelectItem value="1">Active</SelectItem>
                      <SelectItem value="2">Completed</SelectItem>
                      <SelectItem value="3">Dropped</SelectItem>
                      <SelectItem value="4">Suspended</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                {/* Start Date */}
                <div className="space-y-2">
                  <Label htmlFor="startDate">Start Date</Label>
                  <Input
                    id="startDate"
                    type="date"
                    value={filters.startDate?.split('T')[0] || ''}
                    onChange={(e) => handleFilterChange('startDate', e.target.value ? `${e.target.value}T00:00:00.000Z` : undefined)}
                  />
                </div>

                {/* End Date */}
                <div className="space-y-2">
                  <Label htmlFor="endDate">End Date</Label>
                  <Input
                    id="endDate"
                    type="date"
                    value={filters.endDate?.split('T')[0] || ''}
                    onChange={(e) => handleFilterChange('endDate', e.target.value ? `${e.target.value}T23:59:59.999Z` : undefined)}
                  />
                </div>

                {/* Group By */}
                <div className="space-y-2">
                  <Label htmlFor="groupBy">Group By</Label>
                  <Select value={filters.groupBy?.toString()} onValueChange={(value) => handleFilterChange('groupBy', parseInt(value))}>
                    <SelectTrigger id="groupBy">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="1">Course</SelectItem>
                      <SelectItem value="2">Student</SelectItem>
                      <SelectItem value="3">Instructor</SelectItem>
                      <SelectItem value="4">Status</SelectItem>
                      <SelectItem value="5">Date</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {/* Grade Range */}
                <div className="space-y-2">
                  <Label>Grade Range</Label>
                  <div className="flex gap-2">
                    <Input
                      type="number"
                      placeholder="Min grade"
                      min="0"
                      max="100"
                      value={filters.minGrade || ''}
                      onChange={(e) => handleFilterChange('minGrade', e.target.value ? parseFloat(e.target.value) : undefined)}
                    />
                    <Input
                      type="number"
                      placeholder="Max grade"
                      min="0"
                      max="100"
                      value={filters.maxGrade || ''}
                      onChange={(e) => handleFilterChange('maxGrade', e.target.value ? parseFloat(e.target.value) : undefined)}
                    />
                  </div>
                </div>

                {/* Search */}
                <div className="space-y-2">
                  <Label htmlFor="search">Search</Label>
                  <div className="relative">
                    <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                    <Input
                      id="search"
                      placeholder="Search students, courses..."
                      value={filters.search || ''}
                      onChange={(e) => handleFilterChange('search', e.target.value || undefined)}
                      className="pl-10"
                    />
                  </div>
                </div>
              </div>

              <hr className="border-border" />

              <div className="flex justify-between">
                <Button variant="outline" onClick={clearFilters}>
                  Clear Filters
                </Button>
                <Button onClick={generateReport} disabled={loading}>
                  {loading ? (
                    <>
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      Generating...
                    </>
                  ) : (
                    <>
                      <BarChart3 className="mr-2 h-4 w-4" />
                      Generate Report
                    </>
                  )}
                </Button>
              </div>
            </CardContent>
          )}
        </div>
      </Card>

      {/* Report Results */}
      {report && (
        <>
          {/* Report Header */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                {getGroupByIcon(filters.groupBy!)}
                {report.title}
              </CardTitle>
              <CardDescription>
                Generated on {new Date(report.generatedAt).toLocaleString()}
              </CardDescription>
            </CardHeader>
          </Card>

          {/* Summary Statistics */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Total Enrollments</CardTitle>
                <Users className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{report.summary.totalEnrollments}</div>
                <p className="text-xs text-muted-foreground">
                  {report.summary.uniqueStudents} unique students in {report.summary.uniqueCourses} courses
                </p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Active Enrollments</CardTitle>
                <TrendingUp className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-green-600">{report.summary.activeEnrollments}</div>
                <p className="text-xs text-muted-foreground">
                  {report.summary.totalEnrollments ? ((report.summary.activeEnrollments / report.summary.totalEnrollments) * 100).toFixed(1) : 0}% of total
                </p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Completed</CardTitle>
                <GraduationCap className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-blue-600">{report.summary.completedEnrollments}</div>
                <p className="text-xs text-muted-foreground">
                  {report.summary.totalEnrollments ? ((report.summary.completedEnrollments / report.summary.totalEnrollments) * 100).toFixed(1) : 0}% completion rate
                </p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Average Grade</CardTitle>
                <BarChart3 className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">
                  {report.summary.averageGrade ? `${report.summary.averageGrade.toFixed(1)}%` : 'N/A'}
                </div>
                <p className="text-xs text-muted-foreground">
                  Range: {report.summary.lowestGrade?.toFixed(1) || 'N/A'} - {report.summary.highestGrade?.toFixed(1) || 'N/A'}%
                </p>
              </CardContent>
            </Card>
          </div>

          {/* Grouped Results */}
          {report.groups.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Results by {ReportGroupBy[filters.groupBy!]}</CardTitle>
                <CardDescription>
                  Data organized by the selected grouping criteria
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {report.groups.map((group, index) => (
                    <div key={index} className="border rounded-lg p-4">
                      <div className="flex items-center justify-between mb-3">
                        <h4 className="font-semibold">{group.groupLabel}</h4>
                        <div className="flex items-center gap-2">
                          <Badge variant="outline">{group.count} enrollments</Badge>
                          {group.averageGrade && (
                            <Badge variant="secondary">
                              Avg: {group.averageGrade.toFixed(1)}%
                            </Badge>
                          )}
                        </div>
                      </div>
                      
                      <div className="space-y-2">
                        {group.items.slice(0, 3).map((item, itemIndex) => (
                          <div key={itemIndex} className="flex items-center justify-between text-sm border-l-2 border-primary/20 pl-3">
                            <div>
                              <span className="font-medium">{item.studentName}</span>
                              <span className="text-muted-foreground ml-2">({item.courseCode})</span>
                            </div>
                            <div className="flex items-center gap-2">
                              {item.grade && <span className="text-xs">{item.grade}%</span>}
                              {getStatusBadge(item.status)}
                            </div>
                          </div>
                        ))}
                        {group.items.length > 3 && (
                          <div className="text-xs text-muted-foreground pl-3">
                            +{group.items.length - 3} more enrollments
                          </div>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}
        </>
      )}
    </div>
  );
}