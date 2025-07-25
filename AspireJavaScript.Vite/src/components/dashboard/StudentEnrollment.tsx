import { useState, useEffect, useCallback } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Badge } from '../ui/badge';
import { Alert, AlertDescription } from '../ui/alert';
import { enrollmentApi, type CreateEnrollmentDto } from '../../services/enrollmentApi';
import { studentApi, type StudentDto } from '../../services/studentApi';
import { type CourseDto } from '../../services/courseApi';
import { toast } from 'sonner';
import { ArrowLeft, UserPlus, Search, Users, BookOpen, Loader2, AlertCircle, CheckCircle2 } from 'lucide-react';
import { 
  studentEnrollmentSchema, 
  type StudentEnrollmentFormData,
  validateEnrollmentCapacity,
  validateStudentEligibility,
  sanitizeSearchInput
} from '../../lib/validations/student-enrollment';

interface StudentEnrollmentProps {
  courses: CourseDto[];
  onClose: () => void;
}

export function StudentEnrollment({ courses, onClose }: StudentEnrollmentProps) {
  const [students, setStudents] = useState<StudentDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [enrollmentLoading, setEnrollmentLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCourse, setSelectedCourse] = useState<CourseDto | null>(null);
  const [selectedStudents, setSelectedStudents] = useState<Set<string>>(new Set());
  const [enrolledStudents, setEnrolledStudents] = useState<Set<string>>(new Set());
  const [validationErrors, setValidationErrors] = useState<string[]>([]);

  // Form setup with validation
  const form = useForm<StudentEnrollmentFormData>({
    resolver: zodResolver(studentEnrollmentSchema),
    defaultValues: {
      studentIds: []
    }
  });

  useEffect(() => {
    fetchStudents();
  }, []);

  useEffect(() => {
    if (selectedCourse?.id) {
      fetchEnrolledStudents(selectedCourse.id);
    } else {
      setEnrolledStudents(new Set());
    }
    setSelectedStudents(new Set()); // Clear selections when course changes
  }, [selectedCourse]);

  const fetchStudents = async () => {
    try {
      setLoading(true);
      const allStudents = await studentApi.getAllStudents();
      // Filter to get only active students
      const activeStudents = allStudents.filter(student => student.isActive);
      setStudents(activeStudents);
    } catch (error) {
      toast.error('Failed to fetch students');
      console.error('Error fetching students:', error);
    } finally {
      setLoading(false);
    }
  };

  const fetchEnrolledStudents = async (courseId: string) => {
    try {
      const enrollments = await enrollmentApi.getEnrollmentsByCourse(courseId);
      const enrolled = new Set(enrollments.map(e => e.studentId).filter((id): id is string => Boolean(id)));
      setEnrolledStudents(enrolled);
    } catch (error) {
      console.error('Error fetching enrolled students:', error);
    }
  };

  const handleStudentToggle = useCallback((studentId: string) => {
    // Validate student eligibility
    const eligibilityError = validateStudentEligibility(studentId, enrolledStudents);
    if (eligibilityError) {
      toast.error(eligibilityError);
      return;
    }
    
    const newSelected = new Set(selectedStudents);
    if (newSelected.has(studentId)) {
      newSelected.delete(studentId);
    } else {
      // Check enrollment capacity before adding
      if (selectedCourse) {
        const capacityError = validateEnrollmentCapacity(
          newSelected.size + 1,
          selectedCourse.currentEnrollments || 0,
          selectedCourse.maxEnrollments || 0
        );
        if (capacityError) {
          toast.error(capacityError);
          return;
        }
      }
      newSelected.add(studentId);
    }
    
    setSelectedStudents(newSelected);
    form.setValue('studentIds', Array.from(newSelected));
    setValidationErrors([]);
  }, [selectedStudents, enrolledStudents, selectedCourse, form]);

  const handleEnrollStudents = async (data: StudentEnrollmentFormData) => {
    try {
      setEnrollmentLoading(true);
      setValidationErrors([]);
      
      // Additional validation
      const errors: string[] = [];
      
      if (!selectedCourse) {
        errors.push('Please select a course');
      }
      
      if (data.studentIds.length === 0) {
        errors.push('Please select at least one student');
      }
      
      // Validate enrollment capacity
      if (selectedCourse) {
        const capacityError = validateEnrollmentCapacity(
          data.studentIds.length,
          selectedCourse.currentEnrollments || 0,
          selectedCourse.maxEnrollments || 0
        );
        if (capacityError) {
          errors.push(capacityError);
        }
      }
      
      // Validate student eligibility
      for (const studentId of data.studentIds) {
        const eligibilityError = validateStudentEligibility(studentId, enrolledStudents);
        if (eligibilityError) {
          const student = students.find(s => s.id === studentId);
          errors.push(`${student?.fullName || 'Student'}: Already enrolled`);
        }
      }
      
      if (errors.length > 0) {
        setValidationErrors(errors);
        return;
      }
      
      // Perform enrollments with error handling for individual failures
      const enrollmentResults = await Promise.allSettled(
        data.studentIds.map(async (studentId) => {
          const enrollmentData: CreateEnrollmentDto = {
            studentId,
            courseId: selectedCourse!.id!,
          };
          return enrollmentApi.createEnrollment(enrollmentData);
        })
      );
      
      // Analyze results
      const successful = enrollmentResults.filter(result => result.status === 'fulfilled').length;
      const failed = enrollmentResults.filter(result => result.status === 'rejected').length;
      
      if (successful > 0) {
        toast.success(`Successfully enrolled ${successful} student(s) in ${selectedCourse!.title}`);
      }
      
      if (failed > 0) {
        toast.error(`Failed to enroll ${failed} student(s). Please check for duplicates or capacity issues.`);
        console.error('Enrollment failures:', enrollmentResults.filter(r => r.status === 'rejected'));
      }
      
      // Refresh data and clear selections
      if (selectedCourse?.id) {
        await fetchEnrolledStudents(selectedCourse.id);
      }
      setSelectedStudents(new Set());
      form.reset({ studentIds: [] });
      
    } catch (error) {
      toast.error('Failed to process enrollments');
      console.error('Error enrolling students:', error);
    } finally {
      setEnrollmentLoading(false);
    }
  };

  const filteredStudents = students.filter(student => {
    if (!searchTerm.trim()) return true;
    
    const sanitizedTerm = sanitizeSearchInput(searchTerm).toLowerCase();
    return (
      student.fullName?.toLowerCase().includes(sanitizedTerm) ||
      student.email?.toLowerCase().includes(sanitizedTerm) ||
      student.studentNumber?.toLowerCase().includes(sanitizedTerm)
    );
  });

  const availableCourses = courses.filter(course => {
    if (!course.isActive) return false;
    const current = course.currentEnrollments || 0;
    const max = course.maxEnrollments || 0;
    return current < max && max > 0;
  });
  
  // Update course selection validation
  useEffect(() => {
    if (selectedCourse) {
      form.setValue('courseId', selectedCourse.id!);
      
      // Clear student selections if course capacity is exceeded
      const capacityError = validateEnrollmentCapacity(
        selectedStudents.size,
        selectedCourse.currentEnrollments || 0,
        selectedCourse.maxEnrollments || 0
      );
      
      if (capacityError) {
        setSelectedStudents(new Set());
        form.setValue('studentIds', []);
        setValidationErrors([capacityError]);
      }
    }
  }, [selectedCourse, selectedStudents.size, form]);

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
                placeholder="Search students by name, email, or student number..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
                maxLength={100}
              />
            </div>

            {loading ? (
              <div className="flex items-center justify-center py-8">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
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
                {filteredStudents.map((student) => {
                  const isEnrolled = enrolledStudents.has(student.id!);
                  const isSelected = selectedStudents.has(student.id!);
                  
                  return (
                    <div
                      key={student.id}
                      className={`p-3 border rounded-lg transition-colors ${
                        isEnrolled
                          ? 'border-muted bg-muted/30 cursor-not-allowed opacity-60'
                          : isSelected
                          ? 'border-primary bg-primary/5 cursor-pointer'
                          : 'border-border hover:bg-muted/50 cursor-pointer'
                      }`}
                      onClick={() => handleStudentToggle(student.id!)}
                    >
                      <div className="flex items-center justify-between">
                        <div>
                          <h4 className="font-medium">{student.fullName}</h4>
                          <p className="text-sm text-muted-foreground">{student.email}</p>
                        </div>
                        {isEnrolled ? (
                          <Badge variant="secondary">Already Enrolled</Badge>
                        ) : isSelected ? (
                          <Badge variant="default">Selected</Badge>
                        ) : null}
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Validation Errors */}
      {validationErrors.length > 0 && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            <ul className="list-disc list-inside space-y-1">
              {validationErrors.map((error, index) => (
                <li key={index}>{error}</li>
              ))}
            </ul>
          </AlertDescription>
        </Alert>
      )}

      {/* Action Panel */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <h3 className="font-semibold flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 text-green-600" />
                Enrollment Summary
              </h3>
              <p className="text-sm text-muted-foreground">
                {selectedCourse ? (
                  <>Course: <strong>{selectedCourse.title}</strong> ({selectedCourse.courseCode})</>
                ) : (
                  'No course selected'
                )}
              </p>
              <p className="text-sm text-muted-foreground">
                {selectedStudents.size} student(s) selected
                {selectedCourse && (
                  <span className="ml-2 text-xs">
                    • {(selectedCourse.maxEnrollments || 0) - (selectedCourse.currentEnrollments || 0)} spots available
                  </span>
                )}
              </p>
            </div>
            <div className="flex gap-2">
              <Button variant="outline" onClick={onClose}>
                Cancel
              </Button>
              <Button
                onClick={form.handleSubmit(handleEnrollStudents)}
                disabled={!selectedCourse || selectedStudents.size === 0 || enrollmentLoading || validationErrors.length > 0}
              >
                {enrollmentLoading ? (
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                ) : (
                  <UserPlus className="mr-2 h-4 w-4" />
                )}
                Enroll {selectedStudents.size} Student{selectedStudents.size !== 1 ? 's' : ''}
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}