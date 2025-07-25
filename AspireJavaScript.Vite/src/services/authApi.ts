import { components } from '../types/api';

type LoginRequestDto = components['schemas']['LoginRequestDto'];
type LoginResponseDto = components['schemas']['LoginResponseDto'];
type RegisterRequestDto = components['schemas']['RegisterRequestDto'];
type UserDto = components['schemas']['UserDto'];
type ChangePasswordRequestDto = components['schemas']['ChangePasswordRequestDto'];
type RefreshTokenRequestDto = components['schemas']['RefreshTokenRequestDto'];

const API_BASE = '/api';

class AuthApiClient {
  async login(request: LoginRequestDto): Promise<LoginResponseDto> {
    const response = await fetch(`${API_BASE}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Login failed');
    }

    return response.json();
  }

  async register(request: RegisterRequestDto): Promise<UserDto> {
    const response = await fetch(`${API_BASE}/auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Registration failed');
    }

    return response.json();
  }

  async refreshToken(request: RefreshTokenRequestDto): Promise<LoginResponseDto> {
    const response = await fetch(`${API_BASE}/auth/refresh`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Token refresh failed');
    }

    return response.json();
  }

  async logout(token: string): Promise<void> {
    const response = await fetch(`${API_BASE}/auth/logout`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      console.warn('Logout request failed, but continuing with local logout');
    }
  }

  async getCurrentUser(token: string): Promise<UserDto> {
    const response = await fetch(`${API_BASE}/auth/me`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to get current user');
    }

    return response.json();
  }

  async changePassword(token: string, request: ChangePasswordRequestDto): Promise<UserDto> {
    const response = await fetch(`${API_BASE}/auth/change-password`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Password change failed');
    }

    return response.json();
  }

  async getAllUsers(): Promise<UserDto[]> {
    const token = localStorage.getItem('projectathena_token');
    const response = await fetch(`${API_BASE}/admin/auth/users`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to get all users');
    }

    return response.json();
  }
}

export const authApi = new AuthApiClient();
export type { LoginRequestDto, LoginResponseDto, RegisterRequestDto, UserDto, ChangePasswordRequestDto, RefreshTokenRequestDto };