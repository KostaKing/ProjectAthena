import { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Badge } from '../ui/badge';
import { CourseForm } from './CourseForm';
import { courseApi, type CourseDto } from '../../services/courseApi';
import { toast } from 'sonner';
import { Plus, Search, Edit, Trash2, BookOpen, Users, Calendar } from 'lucide-react';

export function CourseManagement() {
  const [courses, setCourses] = useState<CourseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingCourse, setEditingCourse] = useState<CourseDto | null>(null);

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

  const handleDeleteCourse = async (courseId: string) => {
    if (!confirm('Are you sure you want to delete this course?')) return;

    try {
      await courseApi.deleteCourse(courseId);
      toast.success('Course deleted successfully');
      fetchCourses();
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

  const filteredCourses = courses.filter(course =>
    course.title?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    course.courseCode?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    course.instructorName?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const getEnrollmentStatus = (current: number = 0, max: number = 1) => {
    const percentage = (current / max) * 100;
    if (percentage >= 90) return { color: 'bg-red-500', text: 'Full' };
    if (percentage >= 70) return { color: 'bg-yellow-500', text: 'High' };
    return { color: 'bg-green-500', text: 'Available' };
  };

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
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-2xl font-bold">Course Management</h2>
          <p className="text-muted-foreground">
            Add, edit, and manage courses in your system
          </p>
        </div>
        <Button onClick={handleAddCourse}>
          <Plus className="mr-2 h-4 w-4" />
          Add Course
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Search Courses</CardTitle>
          <CardDescription>
            Find courses by title, code, or instructor
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="relative">
            <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search courses..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>
        </CardContent>
      </Card>

      {loading ? (
        <Card>
          <CardContent className="flex items-center justify-center py-8">
            <div className="text-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto mb-4"></div>
              <p className="text-muted-foreground">Loading courses...</p>
            </div>
          </CardContent>
        </Card>
      ) : filteredCourses.length === 0 ? (
        <Card>
          <CardContent className="flex items-center justify-center py-8">
            <div className="text-center">
              <BookOpen className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
              <h3 className="text-lg font-semibold mb-2">No courses found</h3>
              <p className="text-muted-foreground mb-4">
                {searchTerm ? 'No courses match your search criteria.' : 'Get started by adding your first course.'}
              </p>
              {!searchTerm && (
                <Button onClick={handleAddCourse}>
                  <Plus className="mr-2 h-4 w-4" />
                  Add Course
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
              <Card key={course.id} className="hover:shadow-md transition-shadow">
                <CardHeader>
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <CardTitle className="text-lg">{course.title}</CardTitle>
                      <CardDescription className="mt-1">
                        {course.courseCode} • {course.credits} Credits
                      </CardDescription>
                    </div>
                    <Badge variant="outline" className={`${enrollmentStatus.color} text-white`}>
                      {enrollmentStatus.text}
                    </Badge>
                  </div>
                </CardHeader>
                <CardContent className="space-y-4">
                  {course.description && (
                    <p className="text-sm text-muted-foreground line-clamp-2">
                      {course.description}
                    </p>
                  )}
                  
                  <div className="space-y-2 text-sm">
                    <div className="flex items-center gap-2">
                      <Users className="h-4 w-4 text-muted-foreground" />
                      <span>{course.currentEnrollments || 0}/{course.maxEnrollments || 0} enrolled</span>
                    </div>
                    
                    <div className="flex items-center gap-2">
                      <Calendar className="h-4 w-4 text-muted-foreground" />
                      <span>{course.startDate ? formatDate(course.startDate) : 'TBD'} - {course.endDate ? formatDate(course.endDate) : 'TBD'}</span>
                    </div>
                    
                    {course.instructorName && (
                      <div className="flex items-center gap-2">
                        <BookOpen className="h-4 w-4 text-muted-foreground" />
                        <span>{course.instructorName}</span>
                      </div>
                    )}
                  </div>

                  <div className="flex gap-2 pt-4">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleEditCourse(course)}
                    >
                      <Edit className="h-4 w-4 mr-1" />
                      Edit
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleDeleteCourse(course.id!)}
                      className="text-red-600 hover:text-red-700"
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
    </div>
  );
}