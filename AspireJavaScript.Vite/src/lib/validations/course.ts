import { z } from 'zod'

export const courseSchema = z.object({
  title: z.string()
    .min(1, 'Course title is required')
    .max(100, 'Course title cannot exceed 100 characters')
    .regex(/^[a-zA-Z0-9\s\-&().,]+$/, 'Course title contains invalid characters'),
  
  description: z.string()
    .max(500, 'Description cannot exceed 500 characters')
    .optional(),
  
  courseCode: z.string()
    .min(1, 'Course code is required')
    .max(20, 'Course code cannot exceed 20 characters')
    .regex(/^[A-Z0-9\-]+$/, 'Course code must contain only uppercase letters, numbers, and hyphens'),
  
  credits: z.number()
    .int('Credits must be a whole number')
    .min(1, 'Credits must be at least 1')
    .max(12, 'Credits cannot exceed 12'),
  
  instructorId: z.string().uuid().optional().or(z.literal('')),
  
  startDate: z.date({
    message: 'Start date is required'
  }),
  
  endDate: z.date({
    message: 'End date is required'
  }),
  
  maxEnrollments: z.number()
    .int('Maximum enrollments must be a whole number')
    .min(1, 'Maximum enrollments must be at least 1')
    .max(1000, 'Maximum enrollments cannot exceed 1000'),
    
}).refine((data) => {
  // Ensure start date is before end date
  return data.startDate < data.endDate
}, {
  message: "End date must be after start date",
  path: ["endDate"]
}).refine((data) => {
  // Ensure start date is not in the past (for new courses)
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  return data.startDate >= today
}, {
  message: "Start date cannot be in the past",
  path: ["startDate"]
}).refine((data) => {
  // Ensure course duration is reasonable (at least 1 week, max 1 year)
  const diffTime = Math.abs(data.endDate.getTime() - data.startDate.getTime())
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
  return diffDays >= 7 && diffDays <= 365
}, {
  message: "Course duration must be between 1 week and 1 year",
  path: ["endDate"]
})

export type CourseFormData = z.infer<typeof courseSchema>

// Update schema for editing existing courses (allows past start dates)
export const updateCourseSchema = courseSchema.omit({ startDate: true }).extend({
  startDate: z.date({
    message: 'Start date is required'
  })
})

export type UpdateCourseFormData = z.infer<typeof updateCourseSchema>

// Validation utilities
export const validateCourseCode = async (courseCode: string, _excludeId?: string): Promise<string | null> => {
  // This would typically make an API call to check uniqueness
  // For now, just basic format validation
  if (!/^[A-Z0-9\-]+$/.test(courseCode)) {
    return 'Course code must contain only uppercase letters, numbers, and hyphens'
  }
  return null
}

export const validateEnrollmentCapacity = (
  currentEnrollments: number, 
  newMaxEnrollments: number
): string | null => {
  if (newMaxEnrollments < currentEnrollments) {
    return `Cannot reduce capacity below current enrollments (${currentEnrollments})`
  }
  return null
}

export const getCourseStatus = (startDate: Date, endDate: Date): 'upcoming' | 'active' | 'completed' => {
  const now = new Date()
  
  if (now < startDate) {
    return 'upcoming'
  } else if (now > endDate) {
    return 'completed'
  } else {
    return 'active'
  }
}

export const formatCourseDuration = (startDate: Date, endDate: Date): string => {
  const diffTime = Math.abs(endDate.getTime() - startDate.getTime())
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
  
  if (diffDays < 7) {
    return `${diffDays} day${diffDays !== 1 ? 's' : ''}`
  } else if (diffDays < 30) {
    const weeks = Math.ceil(diffDays / 7)
    return `${weeks} week${weeks !== 1 ? 's' : ''}`
  } else if (diffDays < 365) {
    const months = Math.ceil(diffDays / 30)
    return `${months} month${months !== 1 ? 's' : ''}`
  } else {
    const years = Math.ceil(diffDays / 365)
    return `${years} year${years !== 1 ? 's' : ''}`
  }
}