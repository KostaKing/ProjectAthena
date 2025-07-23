import { components } from './api';

export type UserDto = components['schemas']['UserDto'];
export type LoginRequestDto = components['schemas']['LoginRequestDto'];
export type LoginResponseDto = components['schemas']['LoginResponseDto'];
export type RegisterRequestDto = components['schemas']['RegisterRequestDto'];
export type ChangePasswordRequestDto = components['schemas']['ChangePasswordRequestDto'];
export type RefreshTokenRequestDto = components['schemas']['RefreshTokenRequestDto'];

export interface AuthUser {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: UserRole;
  fullName: string;
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string;
}

export enum UserRole {
  Student = 1,
  Teacher = 2,
  Admin = 3
}

export interface AuthState {
  user: AuthUser | null;
  token: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}

export interface AuthContextType extends AuthState {
  login: (email: string, password: string) => Promise<void>;
  register: (data: RegisterRequestDto) => Promise<void>;
  logout: () => void;
  refreshAccessToken: () => Promise<boolean>;
  changePassword: (data: ChangePasswordRequestDto) => Promise<void>;
}