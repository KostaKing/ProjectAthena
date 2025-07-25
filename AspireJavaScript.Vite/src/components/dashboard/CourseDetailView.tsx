import { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Button } from '../ui/button';
import { Badge } from '../ui/badge';
import { Input } from '../ui/input';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../ui/tabs';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from '../ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../ui/select';
import { Label } from '../ui/label';
import { courseApi, type CourseDto } from '../../services/courseApi';
import { enrollmentApi, type EnrollmentDto, EnrollmentStatus, type CreateEnrollmentDto } from '../../services/enrollmentApi';
import { authApi, type UserDto } from '../../services/authApi';
import { toast } from 'sonner';
import { 
  ArrowLeft, 
  Users, 
  GraduationCap, 
  Calendar, 
  BookOpen, 
  UserPlus, 
  Search, 
  Mail,
  Edit,
  Trash2,
  Download
} from 'lucide-react';

// Enum mapping for display
const EnrollmentStatusValues = {
  Active: EnrollmentStatus.Value1,
  Completed: EnrollmentStatus.Value2,
  Dropped: EnrollmentStatus.Value3,
  Suspended: EnrollmentStatus.Value4,
} as const;

interface CourseDetailViewProps {
  courseId: string;
  onBack: () => void;
}

export function CourseDetailView({ courseId, onBack }: CourseDetailViewProps) {
  const [course, setCourse] = useState<CourseDto | null>(null);
  const [enrollments, setEnrollments] = useState<EnrollmentDto[]>([]);
  const [allUsers, setAllUsers] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<number | 'all'>('all');
  const [showAddStudent, setShowAddStudent] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState<string>('');
  const [activeTab, setActiveTab] = useState('overview');
  const [showEditCourse, setShowEditCourse] = useState(false);
  const [editForm, setEditForm] = useState({
    title: '',
    description: '',
    credits: 0,
    maxEnrollments: 0,
    startDate: '',
    endDate: ''
  });

  useEffect(() => {
    fetchCourseData();
    fetchAllUsers();
  }, [courseId]);

  useEffect(() => {
    if (course) {
      setEditForm({
        title: course.title || '',
        description: course.description || '',
        credits: course.credits || 0,
        maxEnrollments: course.maxEnrollments || 0,
        startDate: course.startDate ? course.startDate.split('T')[0] : '',
        endDate: course.endDate ? course.endDate.split('T')[0] : ''
      });
    }
  }, [course]);

  const fetchCourseData = async () => {
    try {
      setLoading(true);
      const [courseData, enrollmentData] = await Promise.all([
        courseApi.getCourseById(courseId),
        enrollmentApi.getEnrollmentsByCourse(courseId)
      ]);
      setCourse(courseData);
      setEnrollments(enrollmentData);
    } catch (error) {
      toast.error('Failed to fetch course data');
      console.error('Error:', error);
    } finally {
      setLoading(false);
    }
  };

  const fetchAllUsers = async () => {
    try {
      const users = await authApi.getAllUsers();
      // Filter to only student users (assuming role Value1 = Student)
      const students = users.filter((user: any) => user.role === 1); // UserRole.Value1
      setAllUsers(students);
    } catch (error) {
      console.error('Error fetching users:', error);
    }
  };

  const handleAddStudent = async () => {
    if (!selectedUserId) {
      toast.error('Please select a student');
      return;
    }

    try {
      const enrollmentData: CreateEnrollmentDto = {
        studentId: selectedUserId,
        courseId: courseId,
        enrollmentDate: new Date().toISOString()
      };

      await enrollmentApi.createEnrollment(enrollmentData);
      toast.success('Student enrolled successfully');
      setShowAddStudent(false);
      setSelectedUserId('');
      fetchCourseData(); // Refresh the data
    } catch (error) {
      toast.error('Failed to enroll student');
      console.error('Error:', error);
    }
  };

  const handleStatusChange = async (enrollmentId: string, newStatus: EnrollmentStatus) => {
    try {
      await enrollmentApi.updateEnrollmentStatus(enrollmentId, { 
        status: newStatus, 
        grade: null 
      });
      toast.success('Status updated successfully');
      fetchCourseData();
    } catch (error) {
      toast.error('Failed to update status');
      console.error('Error:', error);
    }
  };

  const handleRemoveStudent = async (enrollmentId: string) => {
    if (!confirm('Are you sure you want to remove this student from the course?')) return;

    try {
      await enrollmentApi.deleteEnrollment(enrollmentId);
      toast.success('Student removed successfully');
      fetchCourseData();
    } catch (error) {
      toast.error('Failed to remove student');
      console.error('Error:', error);
    }
  };

  const getStatusBadge = (status: EnrollmentStatus) => {
    const variants = {
      [EnrollmentStatus.Value1]: { variant: 'default' as const, label: 'Active' },
      [EnrollmentStatus.Value2]: { variant: 'secondary' as const, label: 'Completed' },
      [EnrollmentStatus.Value3]: { variant: 'destructive' as const, label: 'Dropped' },
      [EnrollmentStatus.Value4]: { variant: 'outline' as const, label: 'Suspended' },
    };
    
    const config = variants[status as keyof typeof variants];
    if (!config) return <Badge variant="outline">Unknown</Badge>;
    
    return <Badge variant={config.variant}>{config.label}</Badge>;
  };

  const filteredEnrollments = enrollments.filter(enrollment => {
    const matchesSearch = !searchTerm || 
      enrollment.studentName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      enrollment.studentEmail?.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesStatus = statusFilter === 'all' || enrollment.status === statusFilter;
    
    return matchesSearch && matchesStatus;
  });

  const formatDate = (dateString?: string) => {
    return dateString ? new Date(dateString).toLocaleDateString() : 'N/A';
  };

  const handleExportStudentList = () => {
    if (enrollments.length === 0) {
      toast.error('No students to export');
      return;
    }

    // Create CSV content
    const headers = ['Student Name', 'Email', 'Status', 'Enrollment Date', 'Grade', 'Completion Date'];
    const csvRows = [headers.join(',')];

    enrollments.forEach(enrollment => {
      const getStatusText = (status: EnrollmentStatus) => {
        const variants = {
          [EnrollmentStatus.Value1]: 'Active',
          [EnrollmentStatus.Value2]: 'Completed',
          [EnrollmentStatus.Value3]: 'Dropped',
          [EnrollmentStatus.Value4]: 'Suspended',
        };
        return variants[status as keyof typeof variants] || 'Unknown';
      };

      const row = [
        enrollment.studentName || 'Unknown Student',
        enrollment.studentEmail || 'No email',
        getStatusText(enrollment.status!),
        formatDate(enrollment.enrollmentDate),
        enrollment.grade ? `${enrollment.grade}%` : 'N/A',
        formatDate(enrollment.completionDate ?? undefined)
      ];
      csvRows.push(row.map(field => `"${field}"`).join(','));
    });

    // Create and download CSV file
    const csvContent = csvRows.join('\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', `${course?.title || 'course'}_students.csv`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    toast.success('Student list exported successfully');
  };

  const handleEditCourse = async () => {
    if (!editForm.title.trim()) {
      toast.error('Course title is required');
      return;
    }

    try {
      const updateData = {
        title: editForm.title,
        description: editForm.description,
        credits: editForm.credits,
        maxEnrollments: editForm.maxEnrollments || undefined,
        startDate: editForm.startDate ? new Date(editForm.startDate).toISOString() : undefined,
        endDate: editForm.endDate ? new Date(editForm.endDate).toISOString() : undefined
      };

      await courseApi.updateCourse(courseId, updateData);
      toast.success('Course updated successfully');
      setShowEditCourse(false);
      fetchCourseData(); // Refresh the data
    } catch (error) {
      toast.error('Failed to update course');
      console.error('Error:', error);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-8">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  if (!course) {
    return (
      <div className="text-center py-8">
        <p className="text-muted-foreground">Course not found</p>
        <Button onClick={onBack} className="mt-4">
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to Courses
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Button variant="ghost" size="sm" onClick={onBack}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h2 className="text-2xl font-bold">{course.title}</h2>
            <p className="text-muted-foreground">{course.courseCode}</p>
          </div>
        </div>
        <div className="flex space-x-2">
          <Dialog open={showAddStudent} onOpenChange={setShowAddStudent}>
            <DialogTrigger asChild>
              <Button>
                <UserPlus className="mr-2 h-4 w-4" />
                Add Student
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Add Student to Course</DialogTitle>
                <DialogDescription>
                  Select a student to enroll in {course.title}
                </DialogDescription>
              </DialogHeader>
              <div className="space-y-4">
                <Select value={selectedUserId} onValueChange={setSelectedUserId}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select a student" />
                  </SelectTrigger>
                  <SelectContent>
                    {allUsers.map((user) => (
                      <SelectItem key={user.id} value={user.id!}>
                        {user.fullName} ({user.email})
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <DialogFooter>
                <Button variant="outline" onClick={() => setShowAddStudent(false)}>
                  Cancel
                </Button>
                <Button onClick={handleAddStudent}>
                  Add Student
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
        </div>
      </div>

      {/* Course Overview Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Students</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{enrollments.length}</div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Active</CardTitle>
            <BookOpen className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {enrollments.filter(e => e.status === EnrollmentStatus.Value1).length}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Completed</CardTitle>
            <GraduationCap className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {enrollments.filter(e => e.status === EnrollmentStatus.Value2).length}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Average Grade</CardTitle>
            <Calendar className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {enrollments.filter(e => e.grade).length > 0 
                ? `${Math.round(enrollments.filter(e => e.grade).reduce((sum, e) => sum + (e.grade || 0), 0) / enrollments.filter(e => e.grade).length)}%`
                : 'N/A'
              }
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Main Content */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="students">Students ({enrollments.length})</TabsTrigger>
          <TabsTrigger value="settings">Settings</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Course Information</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Description</label>
                  <p className="text-sm">{course.description || 'No description available'}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Instructor</label>
                  <p className="text-sm">{course.instructorName || 'No instructor assigned'}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Duration</label>
                  <p className="text-sm">{formatDate(course.startDate)} - {formatDate(course.endDate)}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Credits</label>
                  <p className="text-sm">{course.credits || 'N/A'}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Max Enrollments</label>
                  <p className="text-sm">{course.maxEnrollments || 'Unlimited'}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Current Enrollments</label>
                  <p className="text-sm">{course.currentEnrollments || 0}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="students" className="space-y-4">
          {/* Search and Filter */}
          <Card>
            <CardHeader>
              <CardTitle>Search & Filter Students</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex gap-4">
                <div className="relative flex-1">
                  <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                  <Input
                    placeholder="Search by name or email..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="pl-10"
                  />
                </div>
                <Select value={statusFilter.toString()} onValueChange={(value: string) => setStatusFilter(value === 'all' ? 'all' : parseInt(value))}>
                  <SelectTrigger className="w-48">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Status</SelectItem>
                    <SelectItem value={EnrollmentStatusValues.Active.toString()}>Active</SelectItem>
                    <SelectItem value={EnrollmentStatusValues.Completed.toString()}>Completed</SelectItem>
                    <SelectItem value={EnrollmentStatusValues.Dropped.toString()}>Dropped</SelectItem>
                    <SelectItem value={EnrollmentStatusValues.Suspended.toString()}>Suspended</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </CardContent>
          </Card>

          {/* Student List */}
          <div className="space-y-4">
            {filteredEnrollments.length === 0 ? (
              <Card>
                <CardContent className="flex items-center justify-center py-8">
                  <div className="text-center">
                    <Users className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
                    <h3 className="text-lg font-semibold mb-2">No students found</h3>
                    <p className="text-muted-foreground mb-4">
                      {searchTerm || statusFilter !== 'all' 
                        ? 'No students match your search criteria.' 
                        : 'No students enrolled in this course yet.'}
                    </p>
                    <Button onClick={() => setShowAddStudent(true)}>
                      <UserPlus className="mr-2 h-4 w-4" />
                      Add Student
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ) : (
              filteredEnrollments.map((enrollment) => (
                <Card key={enrollment.id}>
                  <CardContent className="p-6">
                    <div className="flex justify-between items-start">
                      <div className="flex-1 space-y-2">
                        <div className="flex items-center gap-4">
                          <h3 className="font-semibold text-lg">{enrollment.studentName || 'Unknown Student'}</h3>
                          {enrollment.status && getStatusBadge(enrollment.status)}
                        </div>
                        
                        <div className="text-sm text-muted-foreground space-y-1">
                          <div className="flex items-center gap-2">
                            <Mail className="h-4 w-4" />
                            <span>{enrollment.studentEmail || 'No email'}</span>
                          </div>
                          <div className="flex items-center gap-2">
                            <Calendar className="h-4 w-4" />
                            <span>Enrolled: {formatDate(enrollment.enrollmentDate)}</span>
                          </div>
                          {enrollment.grade && (
                            <div className="flex items-center gap-2">
                              <GraduationCap className="h-4 w-4" />
                              <span>Grade: {enrollment.grade}%</span>
                            </div>
                          )}
                          {enrollment.completionDate && (
                            <div className="flex items-center gap-2">
                              <Calendar className="h-4 w-4" />
                              <span>Completed: {formatDate(enrollment.completionDate)}</span>
                            </div>
                          )}
                        </div>
                      </div>
                      
                      <div className="flex gap-2">
                        <Select 
                          value={enrollment.status?.toString() || EnrollmentStatusValues.Active.toString()} 
                          onValueChange={(value: string) => handleStatusChange(enrollment.id!, parseInt(value) as EnrollmentStatus)}
                        >
                          <SelectTrigger className="w-32">
                            <SelectValue />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value={EnrollmentStatusValues.Active.toString()}>Active</SelectItem>
                            <SelectItem value={EnrollmentStatusValues.Completed.toString()}>Completed</SelectItem>
                            <SelectItem value={EnrollmentStatusValues.Dropped.toString()}>Dropped</SelectItem>
                            <SelectItem value={EnrollmentStatusValues.Suspended.toString()}>Suspended</SelectItem>
                          </SelectContent>
                        </Select>
                        
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handleRemoveStudent(enrollment.id!)}
                          className="text-red-600 hover:text-red-700"
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))
            )}
          </div>
        </TabsContent>

        <TabsContent value="settings">
          <Card>
            <CardHeader>
              <CardTitle>Course Settings</CardTitle>
              <CardDescription>
                Manage course settings and configurations
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <Dialog open={showEditCourse} onOpenChange={setShowEditCourse}>
                  <DialogTrigger asChild>
                    <Button variant="outline">
                      <Edit className="mr-2 h-4 w-4" />
                      Edit Course Details
                    </Button>
                  </DialogTrigger>
                  <DialogContent className="max-w-lg">
                    <DialogHeader>
                      <DialogTitle>Edit Course Details</DialogTitle>
                      <DialogDescription>
                        Update course information and settings
                      </DialogDescription>
                    </DialogHeader>
                    <div className="space-y-4">
                      <div>
                        <Label htmlFor="title">Course Title</Label>
                        <Input
                          id="title"
                          value={editForm.title}
                          onChange={(e) => setEditForm(prev => ({ ...prev, title: e.target.value }))}
                          placeholder="Enter course title"
                        />
                      </div>
                      <div>
                        <Label htmlFor="description">Description</Label>
                        <Input
                          id="description"
                          value={editForm.description}
                          onChange={(e) => setEditForm(prev => ({ ...prev, description: e.target.value }))}
                          placeholder="Enter course description"
                        />
                      </div>
                      <div className="grid grid-cols-2 gap-4">
                        <div>
                          <Label htmlFor="credits">Credits</Label>
                          <Input
                            id="credits"
                            type="number"
                            value={editForm.credits}
                            onChange={(e) => setEditForm(prev => ({ ...prev, credits: parseInt(e.target.value) || 0 }))}
                            placeholder="Credits"
                          />
                        </div>
                        <div>
                          <Label htmlFor="maxEnrollments">Max Enrollments</Label>
                          <Input
                            id="maxEnrollments"
                            type="number"
                            value={editForm.maxEnrollments}
                            onChange={(e) => setEditForm(prev => ({ ...prev, maxEnrollments: parseInt(e.target.value) || 0 }))}
                            placeholder="Max enrollments"
                          />
                        </div>
                      </div>
                      <div className="grid grid-cols-2 gap-4">
                        <div>
                          <Label htmlFor="startDate">Start Date</Label>
                          <Input
                            id="startDate"
                            type="date"
                            value={editForm.startDate}
                            onChange={(e) => setEditForm(prev => ({ ...prev, startDate: e.target.value }))}
                          />
                        </div>
                        <div>
                          <Label htmlFor="endDate">End Date</Label>
                          <Input
                            id="endDate"
                            type="date"
                            value={editForm.endDate}
                            onChange={(e) => setEditForm(prev => ({ ...prev, endDate: e.target.value }))}
                          />
                        </div>
                      </div>
                    </div>
                    <DialogFooter>
                      <Button variant="outline" onClick={() => setShowEditCourse(false)}>
                        Cancel
                      </Button>
                      <Button onClick={handleEditCourse}>
                        Save Changes
                      </Button>
                    </DialogFooter>
                  </DialogContent>
                </Dialog>
                <Button variant="outline" onClick={handleExportStudentList}>
                  <Download className="mr-2 h-4 w-4" />
                  Export Student List
                </Button>
                <Button variant="destructive">
                  <Trash2 className="mr-2 h-4 w-4" />
                  Delete Course
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}