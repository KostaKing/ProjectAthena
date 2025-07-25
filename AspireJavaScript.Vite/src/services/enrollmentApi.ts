import { components, EnrollmentStatus } from '../types/api';

type EnrollmentDto = components['schemas']['EnrollmentDto'];
type CreateEnrollmentDto = components['schemas']['CreateEnrollmentDto'];
type UpdateEnrollmentStatusDto = components['schemas']['UpdateEnrollmentStatusDto'];
type EnrollmentSummaryDto = components['schemas']['EnrollmentSummaryDto'];
type EnrollmentReportDto = components['schemas']['EnrollmentReportDto'];

// Advanced reporting types
export interface EnrollmentReportRequestDto {
  courseId?: string;
  studentId?: string;
  instructorId?: string;
  status?: EnrollmentStatus;
  startDate?: string;
  endDate?: string;
  minGrade?: number;
  maxGrade?: number;
  search?: string;
  format?: ReportFormat;
  groupBy?: ReportGroupBy;
}

export interface EnrollmentReportResponseDto {
  title: string;
  generatedAt: string;
  parameters: EnrollmentReportRequestDto;
  summary: EnrollmentReportSummaryDto;
  groups: EnrollmentReportGroupDto[];
  items: EnrollmentReportItemDto[];
}

export interface EnrollmentReportSummaryDto {
  totalEnrollments: number;
  activeEnrollments: number;
  completedEnrollments: number;
  droppedEnrollments: number;
  suspendedEnrollments: number;
  averageGrade?: number;
  highestGrade?: number;
  lowestGrade?: number;
  uniqueCourses: number;
  uniqueStudents: number;
  uniqueInstructors: number;
}

export interface EnrollmentReportGroupDto {
  groupKey: string;
  groupLabel: string;
  count: number;
  averageGrade?: number;
  items: EnrollmentReportItemDto[];
}

export interface EnrollmentReportItemDto {
  enrollmentId: string;
  studentId: string;
  studentName: string;
  studentEmail: string;
  studentNumber: string;
  courseId: string;
  courseCode: string;
  courseTitle: string;
  instructorId: string;
  instructorName: string;
  enrollmentDate: string;
  status: EnrollmentStatus;
  grade?: number;
  completionDate?: string;
  createdAt: string;
  updatedAt?: string;
}

export enum ReportFormat {
  Json = 1,
  Csv = 2,
  Pdf = 3
}

export enum ReportGroupBy {
  Course = 1,
  Student = 2,
  Instructor = 3,
  Status = 4,
  Date = 5
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

const API_BASE = '/api';

class EnrollmentApiClient {
  private async getAuthHeaders(): Promise<HeadersInit> {
    const token = localStorage.getItem('projectathena_token');
    return {
      'Content-Type': 'application/json',
      ...(token && { 'Authorization': `Bearer ${token}` }),
    };
  }

  async getAllEnrollments(
    search?: string, 
    status?: number, 
    page: number = 1, 
    pageSize: number = 10
  ): Promise<PagedResult<EnrollmentDto>> {
    const params = new URLSearchParams();
    if (search) params.append('search', search);
    if (status !== undefined) params.append('status', status.toString());
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());

    const response = await fetch(`${API_BASE}/enrollments?${params}`, {
      method: 'GET',
      headers: await this.getAuthHeaders(),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to fetch enrollments');
    }

    return response.json();
  }

  async getEnrollmentById(id: string): Promise<EnrollmentDto> {
    const response = await fetch(`${API_BASE}/enrollments/${id}`, {
      method: 'GET',
      headers: await this.getAuthHeaders(),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to fetch enrollment');
    }

    return response.json();
  }

  async getEnrollmentsByStudent(studentId: string): Promise<EnrollmentDto[]> {
    const response = await fetch(`${API_BASE}/enrollments/student/${studentId}`, {
      method: 'GET',
      headers: await this.getAuthHeaders(),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to fetch student enrollments');
    }

    return response.json();
  }

  async getEnrollmentsByCourse(courseId: string): Promise<EnrollmentDto[]> {
    const response = await fetch(`${API_BASE}/enrollments/course/${courseId}`, {
      method: 'GET',
      headers: await this.getAuthHeaders(),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to fetch course enrollments');
    }

    return response.json();
  }

  async createEnrollment(request: CreateEnrollmentDto): Promise<EnrollmentDto> {
    const response = await fetch(`${API_BASE}/enrollments`, {
      method: 'POST',
      headers: await this.getAuthHeaders(),
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to create enrollment');
    }

    return response.json();
  }

  async updateEnrollmentStatus(id: string, request: UpdateEnrollmentStatusDto): Promise<void> {
    const response = await fetch(`${API_BASE}/enrollments/${id}/status`, {
      method: 'PATCH',
      headers: await this.getAuthHeaders(),
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to update enrollment status');
    }
  }

  async deleteEnrollment(id: string): Promise<void> {
    const response = await fetch(`${API_BASE}/enrollments/${id}`, {
      method: 'DELETE',
      headers: await this.getAuthHeaders(),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to delete enrollment');
    }
  }

  async generateEnrollmentReport(courseId: string): Promise<EnrollmentReportDto> {
    const response = await fetch(`${API_BASE}/enrollments/report/${courseId}`, {
      method: 'GET',
      headers: await this.getAuthHeaders(),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to generate enrollment report');
    }

    return response.json();
  }

  async generateAdvancedEnrollmentReport(request: EnrollmentReportRequestDto): Promise<EnrollmentReportResponseDto> {
    const response = await fetch(`${API_BASE}/enrollments/reports/advanced`, {
      method: 'POST',
      headers: await this.getAuthHeaders(),
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to generate advanced enrollment report');
    }

    return response.json();
  }
}

export const enrollmentApi = new EnrollmentApiClient();
export { EnrollmentStatus };
export type { EnrollmentDto, CreateEnrollmentDto, UpdateEnrollmentStatusDto, EnrollmentSummaryDto, EnrollmentReportDto };