import { z } from 'zod'

export const studentEnrollmentSchema = z.object({
  courseId: z.string().uuid('Please select a valid course'),
  studentIds: z.array(z.string().uuid()).min(1, 'Please select at least one student'),
})

export type StudentEnrollmentFormData = z.infer<typeof studentEnrollmentSchema>

// Validation utilities
export const validateEnrollmentCapacity = (
  selectedCount: number, 
  currentEnrollments: number, 
  maxEnrollments: number
): string | null => {
  const availableSpots = maxEnrollments - currentEnrollments
  if (selectedCount > availableSpots) {
    return `Cannot enroll ${selectedCount} students. Only ${availableSpots} spots available.`
  }
  return null
}

export const validateStudentEligibility = (
  studentId: string,
  enrolledStudents: Set<string>
): string | null => {
  if (enrolledStudents.has(studentId)) {
    return 'Student is already enrolled in this course'
  }
  return null
}

export const sanitizeSearchInput = (search: string): string => {
  return search.trim().replace(/[<>\"'&]/g, '').substring(0, 100)
}