import { z } from 'zod'
import { EnrollmentStatus, ReportFormat, ReportGroupBy } from '../../services/enrollmentApi'

export const enrollmentReportSchema = z.object({
  courseId: z.string().uuid().optional().or(z.literal('')),
  studentId: z.string().uuid().optional().or(z.literal('')),
  instructorId: z.string().uuid().optional().or(z.literal('')),
  status: z.nativeEnum(EnrollmentStatus).optional(),
  startDate: z.date().optional(),
  endDate: z.date().optional(),
  minGrade: z.number().min(0).max(100).optional(),
  maxGrade: z.number().min(0).max(100).optional(),
  format: z.nativeEnum(ReportFormat).default(ReportFormat.Json),
  groupBy: z.nativeEnum(ReportGroupBy).default(ReportGroupBy.Course),
}).refine((data) => {
  // Ensure start date is before end date
  if (data.startDate && data.endDate) {
    return data.startDate <= data.endDate
  }
  return true
}, {
  message: "Start date must be before or equal to end date",
  path: ["endDate"]
}).refine((data) => {
  // Ensure min grade is less than max grade
  if (data.minGrade !== undefined && data.maxGrade !== undefined) {
    return data.minGrade <= data.maxGrade
  }
  return true
}, {
  message: "Minimum grade must be less than or equal to maximum grade",
  path: ["maxGrade"]
})

export type EnrollmentReportFormData = z.infer<typeof enrollmentReportSchema>

// Utility functions for validation
export const validateGradeRange = (min?: number, max?: number): string | null => {
  if (min !== undefined && (min < 0 || min > 100)) {
    return "Minimum grade must be between 0 and 100"
  }
  if (max !== undefined && (max < 0 || max > 100)) {
    return "Maximum grade must be between 0 and 100"
  }
  if (min !== undefined && max !== undefined && min > max) {
    return "Minimum grade cannot be greater than maximum grade"
  }
  return null
}

export const validateDateRange = (startDate?: Date, endDate?: Date): string | null => {
  if (startDate && endDate && startDate > endDate) {
    return "Start date cannot be after end date"
  }
  if (startDate && startDate > new Date()) {
    return "Start date cannot be in the future"
  }
  return null
}

