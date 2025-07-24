import { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Label } from '../ui/label';
import { Textarea } from '../ui/textarea';
import { courseApi, type CourseDto, type CreateCourseDto, type UpdateCourseDto } from '../../services/courseApi';
import { authApi } from '../../services/authApi';
import { toast } from 'sonner';
import { ArrowLeft, Save, X } from 'lucide-react';

interface CourseFormProps {
  course?: CourseDto | null;
  onClose: () => void;
}

export function CourseForm({ course, onClose }: CourseFormProps) {
  const [loading, setLoading] = useState(false);
  const [instructors, setInstructors] = useState<Array<{ id: string; name: string }>>([]);
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    courseCode: '',
    credits: 1,
    instructorId: '',
    startDate: '',
    endDate: '',
    maxEnrollments: 30,
  });

  const isEditing = !!course;

  useEffect(() => {
    if (course) {
      setFormData({
        title: course.title || '',
        description: course.description || '',
        courseCode: course.courseCode || '',
        credits: course.credits || 1,
        instructorId: course.instructorId || '',
        startDate: course.startDate ? course.startDate.split('T')[0] : '', // Convert to date format
        endDate: course.endDate ? course.endDate.split('T')[0] : '',
        maxEnrollments: course.maxEnrollments || 30,
      });
    }
    
    // Note: In production, you'd fetch instructors from an API
    // For now, we'll use mock data
    setInstructors([
      { id: '1', name: 'Dr. John Smith' },
      { id: '2', name: 'Prof. Jane Doe' },
      { id: '3', name: 'Dr. Mike Johnson' },
    ]);
  }, [course]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'credits' || name === 'maxEnrollments' ? parseInt(value) || 0 : value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.title || !formData.courseCode || !formData.startDate || !formData.endDate) {
      toast.error('Please fill in all required fields');
      return;
    }

    if (new Date(formData.startDate) >= new Date(formData.endDate)) {
      toast.error('End date must be after start date');
      return;
    }

    try {
      setLoading(true);
      
      if (isEditing && course) {
        const updateData: UpdateCourseDto = {
          title: formData.title,
          description: formData.description,
          credits: formData.credits,
          instructorId: formData.instructorId || undefined,
          startDate: formData.startDate,
          endDate: formData.endDate,
          maxEnrollments: formData.maxEnrollments,
        };
        
        await courseApi.updateCourse(course.id, updateData);
        toast.success('Course updated successfully');
      } else {
        const createData: CreateCourseDto = {
          title: formData.title,
          description: formData.description,
          courseCode: formData.courseCode,
          credits: formData.credits,
          instructorId: formData.instructorId || undefined,
          startDate: formData.startDate,
          endDate: formData.endDate,
          maxEnrollments: formData.maxEnrollments,
        };
        
        await courseApi.createCourse(createData);
        toast.success('Course created successfully');
      }
      
      onClose();
    } catch (error) {
      toast.error(isEditing ? 'Failed to update course' : 'Failed to create course');
      console.error('Error saving course:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="outline" onClick={onClose}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to Courses
        </Button>
        <div>
          <h2 className="text-2xl font-bold">
            {isEditing ? 'Edit Course' : 'Add New Course'}
          </h2>
          <p className="text-muted-foreground">
            {isEditing ? 'Update course information' : 'Create a new course for your platform'}
          </p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Course Information</CardTitle>
          <CardDescription>
            Enter the details for the course
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="space-y-2">
                <Label htmlFor="title">Course Title *</Label>
                <Input
                  id="title"
                  name="title"
                  value={formData.title}
                  onChange={handleChange}
                  placeholder="Introduction to Computer Science"
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="courseCode">Course Code *</Label>
                <Input
                  id="courseCode"
                  name="courseCode"
                  value={formData.courseCode}
                  onChange={handleChange}
                  placeholder="CS101"
                  disabled={isEditing}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="credits">Credits *</Label>
                <Input
                  id="credits"
                  name="credits"
                  type="number"
                  min="1"
                  max="10"
                  value={formData.credits}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="maxEnrollments">Max Enrollments *</Label>
                <Input
                  id="maxEnrollments"
                  name="maxEnrollments"
                  type="number"
                  min="1"
                  max="500"
                  value={formData.maxEnrollments}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="startDate">Start Date *</Label>
                <Input
                  id="startDate"
                  name="startDate"
                  type="date"
                  value={formData.startDate}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="endDate">End Date *</Label>
                <Input
                  id="endDate"
                  name="endDate"
                  type="date"
                  value={formData.endDate}
                  onChange={handleChange}
                  required
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="instructorId">Instructor</Label>
              <select
                id="instructorId"
                name="instructorId"
                value={formData.instructorId}
                onChange={handleChange}
                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <option value="">Select an instructor (optional)</option>
                {instructors.map((instructor) => (
                  <option key={instructor.id} value={instructor.id}>
                    {instructor.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="description">Description</Label>
              <Textarea
                id="description"
                name="description"
                value={formData.description}
                onChange={handleChange}
                placeholder="Course description and objectives..."
                rows={4}
              />
            </div>

            <div className="flex gap-4 pt-6">
              <Button type="submit" disabled={loading}>
                {loading ? (
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                ) : (
                  <Save className="mr-2 h-4 w-4" />
                )}
                {isEditing ? 'Update Course' : 'Create Course'}
              </Button>
              <Button type="button" variant="outline" onClick={onClose}>
                <X className="mr-2 h-4 w-4" />
                Cancel
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}