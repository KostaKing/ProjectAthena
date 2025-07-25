import { useState, useEffect, useMemo } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Badge } from '../ui/badge';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../ui/dialog';
import { CourseForm } from './CourseForm';
import { CourseDetailView } from './CourseDetailView';
import { courseApi, type CourseDto } from '../../services/courseApi';
import { toast } from 'sonner';
import { Plus, Search, Edit, Trash2, BookOpen, Users, Calendar, Eye, Filter, AlertTriangle } from 'lucide-react';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../ui/select';

export function CourseManagement() {
  const [courses, setCourses] = useState<CourseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingCourse, setEditingCourse] = useState<CourseDto | null>(null);
  const [selectedCourseId, setSelectedCourseId] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [enrollmentFilter, setEnrollmentFilter] = useState<string>('all');
  const [deleteDialog, setDeleteDialog] = useState<{
    open: boolean;
    course: CourseDto | null;
  }>({ open: false, course: null });

  const fetchCourses = async () => {
    try {
      setLoading(true);
      const fetchedCourses = await courseApi.getAllCourses();
      setCourses(fetchedCourses);
    } catch (error) {
      toast.error('Failed to fetch courses');
      console.error('Error fetching courses:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCourses();
  }, []);

  const handleAddCourse = () => {
    setEditingCourse(null);
    setShowForm(true);
  };

  const handleEditCourse = (course: CourseDto) => {
    setEditingCourse(course);
    setShowForm(true);
  };

  const handleDeleteCourse = async (course: CourseDto) => {
    setDeleteDialog({ open: true, course });
  };

  const confirmDeleteCourse = async () => {
    if (!deleteDialog.course?.id) return;

    try {
      await courseApi.deleteCourse(deleteDialog.course.id);
      toast.success('Course deleted successfully');
      fetchCourses();
      setDeleteDialog({ open: false, course: null });
    } catch (error) {
      toast.error('Failed to delete course');
      console.error('Error deleting course:', error);
    }
  };

  const handleFormClose = () => {
    setShowForm(false);
    setEditingCourse(null);
    fetchCourses();
  };

  const filteredCourses = useMemo(() => {
    return courses.filter(course => {
      const matchesSearch = course.title?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        course.courseCode?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        course.instructorName?.toLowerCase().includes(searchTerm.toLowerCase());
      
      const matchesStatus = statusFilter === 'all' || 
        (statusFilter === 'active' && course.startDate && new Date(course.startDate) <= new Date() && (!course.endDate || new Date(course.endDate) >= new Date())) ||
        (statusFilter === 'upcoming' && course.startDate && new Date(course.startDate) > new Date()) ||
        (statusFilter === 'completed' && course.endDate && new Date(course.endDate) < new Date());
      
      const matchesEnrollment = enrollmentFilter === 'all' ||
        (enrollmentFilter === 'full' && (course.currentEnrollments || 0) >= (course.maxEnrollments || 1)) ||
        (enrollmentFilter === 'available' && (course.currentEnrollments || 0) < (course.maxEnrollments || 1)) ||
        (enrollmentFilter === 'empty' && (course.currentEnrollments || 0) === 0);
      
      return matchesSearch && matchesStatus && matchesEnrollment;
    });
  }, [courses, searchTerm, statusFilter, enrollmentFilter]);

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const getEnrollmentStatus = (current: number = 0, max: number = 1) => {
    const percentage = (current / max) * 100;
    if (percentage >= 90) return { color: 'bg-red-500', text: 'Full' };
    if (percentage >= 70) return { color: 'bg-yellow-500', text: 'High' };
    return { color: 'bg-green-500', text: 'Available' };
  };

  if (selectedCourseId) {
    return (
      <CourseDetailView
        courseId={selectedCourseId}
        onBack={() => setSelectedCourseId(null)}
      />
    );
  }

  if (showForm) {
    return (
      <CourseForm
        course={editingCourse}
        onClose={handleFormClose}
      />
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div className="space-y-1">
          <h2 className="text-2xl font-bold tracking-tight">Course Management</h2>
          <p className="text-muted-foreground">
            Add, edit, and manage courses in your system
          </p>
        </div>
        <Button onClick={handleAddCourse} className="shrink-0">
          <Plus className="mr-2 h-4 w-4" />
          Add Course
        </Button>
      </div>

      <Card className="transition-all duration-200 hover:shadow-md">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Filter className="h-5 w-5" />
            Search & Filter Courses
          </CardTitle>
          <CardDescription>
            Find and filter courses by various criteria
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search courses by title, code, or instructor..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
                aria-label="Search courses"
              />
            </div>
            <div className="flex flex-col sm:flex-row gap-2">
              <Select value={statusFilter} onValueChange={setStatusFilter}>
                <SelectTrigger className="w-full sm:w-40">
                  <SelectValue placeholder="Status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Status</SelectItem>
                  <SelectItem value="active">Active</SelectItem>
                  <SelectItem value="upcoming">Upcoming</SelectItem>
                  <SelectItem value="completed">Completed</SelectItem>
                </SelectContent>
              </Select>
              <Select value={enrollmentFilter} onValueChange={setEnrollmentFilter}>
                <SelectTrigger className="w-full sm:w-40">
                  <SelectValue placeholder="Enrollment" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Levels</SelectItem>
                  <SelectItem value="available">Available</SelectItem>
                  <SelectItem value="full">Full</SelectItem>
                  <SelectItem value="empty">Empty</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
          <div className="flex flex-wrap gap-2 text-sm text-muted-foreground">
            <div className="flex items-center gap-1">
              <div className="w-2 h-2 bg-green-500 rounded-full" />
              Available ({courses.filter(c => (c.currentEnrollments || 0) < (c.maxEnrollments || 1)).length})
            </div>
            <div className="flex items-center gap-1">
              <div className="w-2 h-2 bg-yellow-500 rounded-full" />
              High Enrollment ({courses.filter(c => (c.currentEnrollments || 0) / (c.maxEnrollments || 1) >= 0.7 && (c.currentEnrollments || 0) < (c.maxEnrollments || 1)).length})
            </div>
            <div className="flex items-center gap-1">
              <div className="w-2 h-2 bg-red-500 rounded-full" />
              Full ({courses.filter(c => (c.currentEnrollments || 0) >= (c.maxEnrollments || 1)).length})
            </div>
          </div>
        </CardContent>
      </Card>

      {loading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {Array.from({ length: 6 }).map((_, i) => (
            <Card key={i} className="animate-pulse">
              <CardHeader>
                <div className="flex justify-between items-start">
                  <div className="flex-1 space-y-2">
                    <div className="h-6 bg-muted rounded w-3/4" />
                    <div className="h-4 bg-muted rounded w-1/2" />
                  </div>
                  <div className="h-6 w-16 bg-muted rounded" />
                </div>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="h-4 bg-muted rounded w-full" />
                <div className="h-4 bg-muted rounded w-2/3" />
                <div className="space-y-2">
                  <div className="h-4 bg-muted rounded w-3/4" />
                  <div className="h-4 bg-muted rounded w-5/6" />
                  <div className="h-4 bg-muted rounded w-2/3" />
                </div>
                <div className="flex gap-2 pt-4">
                  <div className="h-8 w-16 bg-muted rounded" />
                  <div className="h-8 w-16 bg-muted rounded" />
                  <div className="h-8 w-20 bg-muted rounded" />
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : filteredCourses.length === 0 ? (
        <Card className="transition-all duration-200 hover:shadow-md">
          <CardContent className="flex items-center justify-center py-12">
            <div className="text-center space-y-4">
              <div className="p-4 bg-muted rounded-full w-fit mx-auto">
                <BookOpen className="h-8 w-8 text-muted-foreground" />
              </div>
              <div className="space-y-2">
                <h3 className="text-lg font-semibold">No courses found</h3>
                <p className="text-muted-foreground max-w-md">
                  {searchTerm 
                    ? 'No courses match your search criteria. Try adjusting your search terms.' 
                    : 'Get started by adding your first course to the system.'}
                </p>
              </div>
              {!searchTerm && (
                <Button onClick={handleAddCourse} className="mt-4">
                  <Plus className="mr-2 h-4 w-4" />
                  Add Your First Course
                </Button>
              )}
            </div>
          </CardContent>
        </Card>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredCourses.map((course) => {
            const enrollmentStatus = getEnrollmentStatus(course.currentEnrollments, course.maxEnrollments);
            
            return (
              <Card key={course.id} className="group transition-all duration-200 hover:shadow-lg hover:scale-[1.02] border-l-4 border-l-blue-500">
                <CardHeader className="pb-3">
                  <div className="flex justify-between items-start gap-4">
                    <div className="flex-1 min-w-0">
                      <CardTitle className="text-lg font-bold truncate group-hover:text-primary transition-colors">
                        {course.title}
                      </CardTitle>
                      <CardDescription className="flex items-center gap-2 mt-1">
                        <span className="font-mono text-xs bg-muted px-2 py-1 rounded">
                          {course.courseCode}
                        </span>
                        <span>•</span>
                        <span>{course.credits} Credits</span>
                      </CardDescription>
                    </div>
                    <Badge 
                      variant={enrollmentStatus.text === 'Full' ? 'destructive' : enrollmentStatus.text === 'High' ? 'secondary' : 'default'}
                      className="shrink-0"
                    >
                      {enrollmentStatus.text}
                    </Badge>
                  </div>
                </CardHeader>
                <CardContent className="space-y-4">
                  {course.description && (
                    <p className="text-sm text-muted-foreground line-clamp-2 leading-relaxed">
                      {course.description}
                    </p>
                  )}
                  
                  <div className="grid grid-cols-1 gap-2 text-sm">
                    <div className="flex items-center gap-2 p-2 bg-muted/50 rounded">
                      <Users className="h-4 w-4 text-blue-600" />
                      <span className="font-medium">
                        {course.currentEnrollments || 0}/{course.maxEnrollments || 0} enrolled
                      </span>
                      <div className="ml-auto flex-1 bg-muted-foreground/20 rounded-full h-2 max-w-[60px]">
                        <div 
                          className="bg-blue-600 h-2 rounded-full transition-all"
                          style={{ 
                            width: `${Math.min(((course.currentEnrollments || 0) / (course.maxEnrollments || 1)) * 100, 100)}%` 
                          }}
                        />
                      </div>
                    </div>
                    
                    <div className="flex items-center gap-2 p-2 bg-muted/50 rounded">
                      <Calendar className="h-4 w-4 text-green-600" />
                      <span className="text-xs">
                        {course.startDate ? formatDate(course.startDate) : 'TBD'} - {course.endDate ? formatDate(course.endDate) : 'TBD'}
                      </span>
                    </div>
                    
                    {course.instructorName && (
                      <div className="flex items-center gap-2 p-2 bg-muted/50 rounded">
                        <BookOpen className="h-4 w-4 text-purple-600" />
                        <span className="font-medium truncate">{course.instructorName}</span>
                      </div>
                    )}
                  </div>

                  <div className="flex flex-wrap gap-2 pt-2">
                    <Button
                      variant="default"
                      size="sm"
                      onClick={() => setSelectedCourseId(course.id!)}
                      className="flex-1 sm:flex-none"
                      aria-label={`View details for ${course.title}`}
                    >
                      <Eye className="h-4 w-4 mr-1" />
                      View
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleEditCourse(course)}
                      className="flex-1 sm:flex-none hover:bg-blue-50 hover:border-blue-300 dark:hover:bg-blue-950"
                      aria-label={`Edit ${course.title}`}
                    >
                      <Edit className="h-4 w-4 mr-1" />
                      Edit
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleDeleteCourse(course)}
                      className="flex-1 sm:flex-none text-red-600 hover:text-red-700 hover:bg-red-50 hover:border-red-300 dark:hover:bg-red-950 transition-colors"
                      aria-label={`Delete ${course.title}`}
                    >
                      <Trash2 className="h-4 w-4 mr-1" />
                      Delete
                    </Button>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialog.open} onOpenChange={(open) => setDeleteDialog(prev => ({ ...prev, open }))}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-destructive" />
              Delete Course
            </DialogTitle>
            <DialogDescription>
              Are you sure you want to delete the course <strong>{deleteDialog.course?.title}</strong>? 
              This action cannot be undone and will permanently remove:
              <ul className="list-disc ml-6 mt-2 space-y-1">
                <li>Course content and materials</li>
                <li>Student enrollments ({deleteDialog.course?.currentEnrollments || 0} students)</li>
                <li>Course progress and grades</li>
                <li>All related data</li>
              </ul>
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button 
              variant="outline" 
              onClick={() => setDeleteDialog({ open: false, course: null })}
            >
              Cancel
            </Button>
            <Button 
              variant="destructive"
              onClick={confirmDeleteCourse}
              className="gap-2"
            >
              <Trash2 className="h-4 w-4" />
              Delete Course
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}