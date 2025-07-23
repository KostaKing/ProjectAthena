import React, { createContext, useContext, useReducer, useEffect, useCallback } from 'react';
import { AuthState, AuthContextType, AuthUser, UserRole, RegisterRequestDto, ChangePasswordRequestDto } from '../types/auth';
import { authApi } from '../services/authApi';
import { useToast } from '../components/ui/use-toast';

const AuthContext = createContext<AuthContextType | null>(null);

type AuthAction =
  | { type: 'LOGIN_START' }
  | { type: 'LOGIN_SUCCESS'; payload: { user: AuthUser; token: string; refreshToken: string } }
  | { type: 'LOGIN_FAILURE' }
  | { type: 'LOGOUT' }
  | { type: 'REFRESH_TOKEN_SUCCESS'; payload: { token: string; refreshToken: string } }
  | { type: 'SET_LOADING'; payload: boolean };

const initialState: AuthState = {
  user: null,
  token: null,
  refreshToken: null,
  isAuthenticated: false,
  isLoading: true,
};

function authReducer(state: AuthState, action: AuthAction): AuthState {
  switch (action.type) {
    case 'LOGIN_START':
      return { ...state, isLoading: true };
    case 'LOGIN_SUCCESS':
      return {
        ...state,
        user: action.payload.user,
        token: action.payload.token,
        refreshToken: action.payload.refreshToken,
        isAuthenticated: true,
        isLoading: false,
      };
    case 'LOGIN_FAILURE':
      return { ...state, isLoading: false };
    case 'LOGOUT':
      return {
        ...initialState,
        isLoading: false,
      };
    case 'REFRESH_TOKEN_SUCCESS':
      return {
        ...state,
        token: action.payload.token,
        refreshToken: action.payload.refreshToken,
      };
    case 'SET_LOADING':
      return { ...state, isLoading: action.payload };
    default:
      return state;
  }
}

const TOKEN_KEY = 'projectathena_token';
const REFRESH_TOKEN_KEY = 'projectathena_refresh_token';
const USER_KEY = 'projectathena_user';

interface AuthProviderProps {
  children: React.ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [state, dispatch] = useReducer(authReducer, initialState);
  const { toast } = useToast();

  // Load stored auth data on mount
  useEffect(() => {
    const loadStoredAuth = () => {
      try {
        const token = localStorage.getItem(TOKEN_KEY);
        const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
        const userJson = localStorage.getItem(USER_KEY);

        if (token && refreshToken && userJson) {
          const user = JSON.parse(userJson) as AuthUser;
          dispatch({
            type: 'LOGIN_SUCCESS',
            payload: { user, token, refreshToken },
          });
        } else {
          dispatch({ type: 'SET_LOADING', payload: false });
        }
      } catch (error) {
        console.error('Error loading stored auth:', error);
        dispatch({ type: 'SET_LOADING', payload: false });
      }
    };

    loadStoredAuth();
  }, []);

  const storeAuthData = (user: AuthUser, token: string, refreshToken: string) => {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
  };

  const clearAuthData = () => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  };

  const login = async (email: string, password: string) => {
    try {
      dispatch({ type: 'LOGIN_START' });

      const response = await authApi.login({ email, password, rememberMe: true });
      
      const user: AuthUser = {
        id: response.user.id,
        firstName: response.user.firstName,
        lastName: response.user.lastName,
        email: response.user.email,
        role: response.user.role as UserRole,
        fullName: response.user.fullName,
        isActive: response.user.isActive,
        createdAt: response.user.createdAt.toString(),
        lastLoginAt: response.user.lastLoginAt?.toString(),
      };

      storeAuthData(user, response.token, response.refreshToken);
      
      dispatch({
        type: 'LOGIN_SUCCESS',
        payload: {
          user,
          token: response.token,
          refreshToken: response.refreshToken,
        },
      });

      toast({
        title: "Login successful",
        description: `Welcome back, ${user.firstName}!`,
      });
    } catch (error) {
      dispatch({ type: 'LOGIN_FAILURE' });
      toast({
        variant: "destructive",
        title: "Login failed",
        description: error instanceof Error ? error.message : "An error occurred during login",
      });
      throw error;
    }
  };

  const register = async (data: RegisterRequestDto) => {
    try {
      dispatch({ type: 'LOGIN_START' });

      const user = await authApi.register(data);
      
      // After registration, automatically log the user in
      await login(data.email, data.password);

      toast({
        title: "Registration successful",
        description: "Your account has been created successfully!",
      });
    } catch (error) {
      dispatch({ type: 'LOGIN_FAILURE' });
      toast({
        variant: "destructive",
        title: "Registration failed",
        description: error instanceof Error ? error.message : "An error occurred during registration",
      });
      throw error;
    }
  };

  const logout = useCallback(async () => {
    try {
      if (state.token) {
        await authApi.logout(state.token);
      }
    } catch (error) {
      console.warn('Logout API call failed:', error);
    } finally {
      clearAuthData();
      dispatch({ type: 'LOGOUT' });
      toast({
        title: "Logged out",
        description: "You have been successfully logged out.",
      });
    }
  }, [state.token, toast]);

  const refreshAccessToken = useCallback(async (): Promise<boolean> => {
    try {
      if (!state.refreshToken) {
        return false;
      }

      const response = await authApi.refreshToken({ refreshToken: state.refreshToken });
      
      const user: AuthUser = {
        id: response.user.id,
        firstName: response.user.firstName,
        lastName: response.user.lastName,
        email: response.user.email,
        role: response.user.role as UserRole,
        fullName: response.user.fullName,
        isActive: response.user.isActive,
        createdAt: response.user.createdAt.toString(),
        lastLoginAt: response.user.lastLoginAt?.toString(),
      };

      storeAuthData(user, response.token, response.refreshToken);
      
      dispatch({
        type: 'REFRESH_TOKEN_SUCCESS',
        payload: {
          token: response.token,
          refreshToken: response.refreshToken,
        },
      });

      return true;
    } catch (error) {
      console.error('Token refresh failed:', error);
      logout();
      return false;
    }
  }, [state.refreshToken, logout]);

  const changePassword = async (data: ChangePasswordRequestDto) => {
    try {
      if (!state.token) {
        throw new Error('Not authenticated');
      }

      await authApi.changePassword(state.token, data);
      
      toast({
        title: "Password changed",
        description: "Your password has been updated successfully.",
      });
    } catch (error) {
      toast({
        variant: "destructive",
        title: "Password change failed",
        description: error instanceof Error ? error.message : "An error occurred while changing password",
      });
      throw error;
    }
  };

  const contextValue: AuthContextType = {
    ...state,
    login,
    register,
    logout,
    refreshAccessToken,
    changePassword,
  };

  return (
    <AuthContext.Provider value={contextValue}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}