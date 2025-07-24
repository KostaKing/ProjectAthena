import { components } from '../types/api';

type CourseDto = components['schemas']['CourseDto'];
type CreateCourseDto = components['schemas']['CreateCourseDto'];
type UpdateCourseDto = components['schemas']['UpdateCourseDto'];

const API_BASE = '/api';

class CourseApiClient {
  private async getAuthHeaders(): Promise<HeadersInit> {
    const token = localStorage.getItem('token');
    return {
      'Content-Type': 'application/json',
      ...(token && { 'Authorization': `Bearer ${token}` }),
    };
  }

  async getAllCourses(): Promise<CourseDto[]> {
    const response = await fetch(`${API_BASE}/courses`, {
      method: 'GET',
      headers: await this.getAuthHeaders(),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to fetch courses');
    }

    return response.json();
  }

  async getCourseById(id: string): Promise<CourseDto> {
    const response = await fetch(`${API_BASE}/courses/${id}`, {
      method: 'GET',
      headers: await this.getAuthHeaders(),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to fetch course');
    }

    return response.json();
  }

  async getCourseByCode(courseCode: string): Promise<CourseDto> {
    const response = await fetch(`${API_BASE}/courses/code/${courseCode}`, {
      method: 'GET',
      headers: await this.getAuthHeaders(),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to fetch course');
    }

    return response.json();
  }

  async createCourse(request: CreateCourseDto): Promise<CourseDto> {
    const response = await fetch(`${API_BASE}/courses`, {
      method: 'POST',
      headers: await this.getAuthHeaders(),
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to create course');
    }

    return response.json();
  }

  async updateCourse(id: string, request: UpdateCourseDto): Promise<CourseDto> {
    const response = await fetch(`${API_BASE}/courses/${id}`, {
      method: 'PUT',
      headers: await this.getAuthHeaders(),
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to update course');
    }

    return response.json();
  }

  async deleteCourse(id: string): Promise<void> {
    const response = await fetch(`${API_BASE}/courses/${id}`, {
      method: 'DELETE',
      headers: await this.getAuthHeaders(),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to delete course');
    }
  }
}

export const courseApi = new CourseApiClient();
export type { CourseDto, CreateCourseDto, UpdateCourseDto };