import { components, UserRole as ApiUserRole } from './api';

// Re-export API types for convenience
export type UserDto = components['schemas']['UserDto'];
export type LoginRequestDto = components['schemas']['LoginRequestDto'];
export type LoginResponseDto = components['schemas']['LoginResponseDto'];
export type RegisterRequestDto = components['schemas']['RegisterRequestDto'];
export type ChangePasswordRequestDto = components['schemas']['ChangePasswordRequestDto'];
export type RefreshTokenRequestDto = components['schemas']['RefreshTokenRequestDto'];
export { ApiUserRole as UserRole };

// Use the API-generated UserDto as AuthUser
export type AuthUser = UserDto;

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