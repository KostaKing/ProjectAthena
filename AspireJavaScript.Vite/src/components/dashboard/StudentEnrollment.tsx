import { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Badge } from '../ui/badge';
import { enrollmentApi, type CreateEnrollmentDto } from '../../services/enrollmentApi';
import { authApi, type UserDto } from '../../services/authApi';
import { type CourseDto } from '../../services/courseApi';
import { toast } from 'sonner';
import { ArrowLeft, UserPlus, Search, Users, BookOpen } from 'lucide-react';

interface StudentEnrollmentProps {
  courses: CourseDto[];
  onClose: () => void;
}

export function StudentEnrollment({ courses, onClose }: StudentEnrollmentProps) {
  const [students, setStudents] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCourse, setSelectedCourse] = useState<CourseDto | null>(null);
  const [selectedStudents, setSelectedStudents] = useState<Set<string>>(new Set());

  useEffect(() => {
    fetchStudents();
  }, []);

  const fetchStudents = async () => {
    try {
      setLoading(true);
      const allUsers = await authApi.getAllUsers();
      // Filter to get only active students (role 1 = Student)
      const studentUsers = allUsers.filter(user => user.role === 1 && user.isActive);
      setStudents(studentUsers);
    } catch (error) {
      toast.error('Failed to fetch students');
      console.error('Error fetching students:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleStudentToggle = (studentId: string) => {
    const newSelected = new Set(selectedStudents);
    if (newSelected.has(studentId)) {
      newSelected.delete(studentId);
    } else {
      newSelected.add(studentId);
    }
    setSelectedStudents(newSelected);
  };

  const handleEnrollStudents = async () => {
    if (!selectedCourse || selectedStudents.size === 0) {
      toast.error('Please select a course and at least one student');
      return;
    }

    try {
      setLoading(true);
      const enrollmentPromises = Array.from(selectedStudents).map(studentId => {
        const enrollmentData: CreateEnrollmentDto = {
          studentId,
          courseId: selectedCourse.id!,
        };
        return enrollmentApi.createEnrollment(enrollmentData);
      });

      await Promise.all(enrollmentPromises);
      toast.success(`Successfully enrolled ${selectedStudents.size} student(s) in ${selectedCourse.title}`);
      onClose();
    } catch (error) {
      toast.error('Failed to enroll students');
      console.error('Error enrolling students:', error);
    } finally {
      setLoading(false);
    }
  };

  const filteredStudents = students.filter(student =>
    student.fullName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    student.email?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const availableCourses = courses.filter(course => 
    (course.currentEnrollments || 0) < (course.maxEnrollments || 0)
  );

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="outline" onClick={onClose}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to Enrollments
        </Button>
        <div>
          <h2 className="text-2xl font-bold">Enroll Students</h2>
          <p className="text-muted-foreground">
            Assign students to available courses
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Course Selection */}
        <Card>
          <CardHeader>
            <CardTitle>Select Course</CardTitle>
            <CardDescription>
              Choose the course to enroll students in
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {availableCourses.length === 0 ? (
              <div className="text-center py-8">
                <BookOpen className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
                <p className="text-muted-foreground">No courses available for enrollment</p>
              </div>
            ) : (
              <div className="space-y-3">
                {availableCourses.map((course) => (
                  <div
                    key={course.id}
                    className={`p-4 border rounded-lg cursor-pointer transition-colors ${
                      selectedCourse?.id === course.id
                        ? 'border-primary bg-primary/5'
                        : 'border-border hover:bg-muted/50'
                    }`}
                    onClick={() => setSelectedCourse(course)}
                  >
                    <div className="flex justify-between items-start">
                      <div>
                        <h3 className="font-semibold">{course.title}</h3>
                        <p className="text-sm text-muted-foreground">{course.courseCode}</p>
                        <p className="text-sm text-muted-foreground mt-1">
                          {course.currentEnrollments || 0}/{course.maxEnrollments || 0} enrolled
                        </p>
                      </div>
                      <Badge 
                        variant={(course.currentEnrollments || 0) >= (course.maxEnrollments || 0) * 0.9 ? 'destructive' : 'default'}
                      >
                        {(course.maxEnrollments || 0) - (course.currentEnrollments || 0)} spots left
                      </Badge>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        {/* Student Selection */}
        <Card>
          <CardHeader>
            <CardTitle>Select Students</CardTitle>
            <CardDescription>
              Choose students to enroll in the selected course
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="relative">
              <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search students..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>

            {loading ? (
              <div className="flex items-center justify-center py-8">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
              </div>
            ) : filteredStudents.length === 0 ? (
              <div className="text-center py-8">
                <Users className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
                <p className="text-muted-foreground">
                  {searchTerm ? 'No students match your search' : 'No students available'}
                </p>
              </div>
            ) : (
              <div className="space-y-2 max-h-80 overflow-y-auto">
                {filteredStudents.map((student) => (
                  <div
                    key={student.id}
                    className={`p-3 border rounded-lg cursor-pointer transition-colors ${
                      selectedStudents.has(student.id!)
                        ? 'border-primary bg-primary/5'
                        : 'border-border hover:bg-muted/50'
                    }`}
                    onClick={() => handleStudentToggle(student.id!)}
                  >
                    <div className="flex items-center justify-between">
                      <div>
                        <h4 className="font-medium">{student.fullName}</h4>
                        <p className="text-sm text-muted-foreground">{student.email}</p>
                      </div>
                      {selectedStudents.has(student.id!) && (
                        <Badge variant="default">Selected</Badge>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Action Panel */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center justify-between">
            <div>
              <h3 className="font-semibold">Enrollment Summary</h3>
              <p className="text-sm text-muted-foreground">
                {selectedCourse ? (
                  <>Course: <strong>{selectedCourse.title}</strong></>
                ) : (
                  'No course selected'
                )}
                {' • '}
                {selectedStudents.size} student(s) selected
              </p>
            </div>
            <div className="flex gap-2">
              <Button variant="outline" onClick={onClose}>
                Cancel
              </Button>
              <Button
                onClick={handleEnrollStudents}
                disabled={!selectedCourse || selectedStudents.size === 0 || loading}
              >
                {loading ? (
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                ) : (
                  <UserPlus className="mr-2 h-4 w-4" />
                )}
                Enroll Students
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}