import { components } from '../types/api';

const API_BASE = '/api';

export type StudentDto = components['schemas']['StudentDto'];

class StudentApiClient {
  private async getAuthHeaders(): Promise<HeadersInit> {
    const token = localStorage.getItem('projectathena_token');
    return {
      'Content-Type': 'application/json',
      ...(token && { 'Authorization': `Bearer ${token}` }),
    };
  }

  async getAllStudents(): Promise<StudentDto[]> {
    const response = await fetch(`${API_BASE}/students`, {
      method: 'GET',
      headers: await this.getAuthHeaders(),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to fetch students');
    }

    return response.json();
  }

  async getStudentById(id: string): Promise<StudentDto> {
    const response = await fetch(`${API_BASE}/students/${id}`, {
      method: 'GET',
      headers: await this.getAuthHeaders(),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to fetch student');
    }

    return response.json();
  }

  async getStudentByUserId(userId: string): Promise<StudentDto> {
    const response = await fetch(`${API_BASE}/students/user/${userId}`, {
      method: 'GET',
      headers: await this.getAuthHeaders(),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to fetch student');
    }

    return response.json();
  }
}

export const studentApi = new StudentApiClient();