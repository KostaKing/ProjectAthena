const API_BASE = '/api';

export interface DashboardStatsDto {
  totalCourses: number;
  totalEnrollments: number;
  activeEnrollments: number;
  completedEnrollments: number;
  completionRate: number;
  averageGrade?: number;
  totalStudents: number;
  totalTeachers: number;
  recentActivities: RecentActivityDto[];
}

export interface RecentActivityDto {
  id: string;
  type: string; // "enrollment", "completion", "course_created"
  description: string;
  timestamp: string;
  studentName?: string;
  courseName?: string;
}

class DashboardApiClient {
  private async getAuthHeaders(): Promise<HeadersInit> {
    const token = localStorage.getItem('projectathena_token');
    return {
      'Content-Type': 'application/json',
      ...(token && { 'Authorization': `Bearer ${token}` }),
    };
  }

  async getDashboardStats(): Promise<DashboardStatsDto> {
    const response = await fetch(`${API_BASE}/dashboard/stats`, {
      method: 'GET',
      headers: await this.getAuthHeaders(),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to fetch dashboard statistics');
    }

    return response.json();
  }
}

export const dashboardApi = new DashboardApiClient();