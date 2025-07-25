import React, { useState } from 'react';
import { useAuth } from '../../hooks/useAuth';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Label } from '../ui/label';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '../ui/card';
import { Eye, EyeOff, LogIn } from 'lucide-react';

interface LoginFormProps {
  onSwitchToRegister: () => void;
}

export function LoginForm({ onSwitchToRegister }: LoginFormProps) {
  const { login, isLoading } = useAuth();
  const [formData, setFormData] = useState({
    email: '',
    password: '',
  });
  const [showPassword, setShowPassword] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
    
    // Clear error when user starts typing
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: '' }));
    }
  };

  const validateForm = () => {
    const newErrors: Record<string, string> = {};

    if (!formData.email.trim()) {
      newErrors.email = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = 'Please enter a valid email address';
    }

    if (!formData.password) {
      newErrors.password = 'Password is required';
    } else if (formData.password.length < 6) {
      newErrors.password = 'Password must be at least 6 characters long';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    try {
      await login(formData.email, formData.password);
    } catch (error) {
      // Error handling is done in the auth context
    }
  };

  const handleDemoFill = (role: 'admin' | 'teacher' | 'student') => {
    const demoCredentials = {
      admin: { email: 'admin@projectathena.com', password: 'Admin123!' },
      teacher: { email: 'teacher@projectathena.com', password: 'Admin123!' },
      student: { email: 'student@projectathena.com', password: 'Admin123!' },
    };

    const credentials = demoCredentials[role];
    setFormData(credentials);
    
    // Clear any existing errors
    setErrors({});
  };

  return (
    <Card className="w-full max-w-md mx-auto shadow-lg border-0 bg-white/80 dark:bg-gray-900/80 backdrop-blur-sm">
      <CardHeader className="space-y-1 text-center">
        <CardTitle className="text-2xl font-bold tracking-tight">Welcome Back</CardTitle>
        <CardDescription>
          Sign in to your ProjectAthena account
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        <form 
          onSubmit={handleSubmit} 
          className="space-y-4"
          noValidate
          aria-label="Sign in form"
        >
          <div className="space-y-2">
            <Label htmlFor="email" className="text-sm font-medium">
              Email Address
            </Label>
            <Input
              id="email"
              name="email"
              type="email"
              placeholder="Enter your email address"
              value={formData.email}
              onChange={handleChange}
              className={errors.email ? 'border-red-500 focus:border-red-500' : ''}
              disabled={isLoading}
              aria-invalid={!!errors.email}
              aria-describedby={errors.email ? 'email-error' : undefined}
              autoComplete="email"
              required
            />
            {errors.email && (
              <p 
                id="email-error" 
                className="text-sm text-red-600 dark:text-red-400" 
                role="alert" 
                aria-live="polite"
              >
                {errors.email}
              </p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="password" className="text-sm font-medium">
              Password
            </Label>
            <div className="relative">
              <Input
                id="password"
                name="password"
                type={showPassword ? 'text' : 'password'}
                placeholder="Enter your password"
                value={formData.password}
                onChange={handleChange}
                className={errors.password ? 'border-red-500 focus:border-red-500 pr-10' : 'pr-10'}
                disabled={isLoading}
                aria-invalid={!!errors.password}
                aria-describedby={errors.password ? 'password-error' : undefined}
                autoComplete="current-password"
                required
              />
              <Button
                type="button"
                variant="ghost"
                size="icon"
                className="absolute right-0 top-0 h-full px-3 py-2 hover:bg-transparent"
                onClick={() => setShowPassword(!showPassword)}
                disabled={isLoading}
                aria-label={showPassword ? 'Hide password' : 'Show password'}
                tabIndex={-1}
              >
                {showPassword ? (
                  <EyeOff className="h-4 w-4 text-muted-foreground" />
                ) : (
                  <Eye className="h-4 w-4 text-muted-foreground" />
                )}
              </Button>
            </div>
            {errors.password && (
              <p 
                id="password-error" 
                className="text-sm text-red-600 dark:text-red-400" 
                role="alert" 
                aria-live="polite"
              >
                {errors.password}
              </p>
            )}
          </div>

          <Button 
            type="submit" 
            className="w-full h-11 text-sm font-medium" 
            disabled={isLoading}
            aria-describedby={isLoading ? 'signin-loading' : undefined}
          >
            {isLoading ? (
              <>
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2" />
                <span id="signin-loading">Signing In...</span>
              </>
            ) : (
              <>
                <LogIn className="h-4 w-4 mr-2" />
                Sign In
              </>
            )}
          </Button>
        </form>

        <div className="relative">
          <div className="absolute inset-0 flex items-center">
            <span className="w-full border-t" />
          </div>
          <div className="relative flex justify-center text-xs uppercase">
            <span className="bg-background px-2 text-muted-foreground">
              Quick Demo Login
            </span>
          </div>
        </div>

        <div className="space-y-3">
          <p className="text-xs text-center text-muted-foreground">
            Quick demo access - click to auto-fill credentials
          </p>
          <div className="grid grid-cols-3 gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => handleDemoFill('admin')}
              disabled={isLoading}
              className="text-xs py-2 hover:bg-blue-50 hover:border-blue-300 dark:hover:bg-blue-950 transition-colors"
              aria-label="Fill admin demo credentials"
              title="admin@projectathena.com"
            >
              Admin
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => handleDemoFill('teacher')}
              disabled={isLoading}
              className="text-xs py-2 hover:bg-green-50 hover:border-green-300 dark:hover:bg-green-950 transition-colors"
              aria-label="Fill teacher demo credentials"
              title="teacher@projectathena.com"
            >
              Teacher
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => handleDemoFill('student')}
              disabled={isLoading}
              className="text-xs py-2 hover:bg-purple-50 hover:border-purple-300 dark:hover:bg-purple-950 transition-colors"
              aria-label="Fill student demo credentials"
              title="student@projectathena.com"
            >
              Student
            </Button>
          </div>
          <p className="text-xs text-center text-muted-foreground">
            All demo accounts use password: <code className="bg-muted px-1.5 py-0.5 rounded text-xs font-mono">Admin123!</code>
          </p>
        </div>
      </CardContent>
      <CardFooter className="pt-6">
        <p className="text-center text-sm text-muted-foreground w-full">
          Don't have an account?{' '}
          <Button
            variant="link"
            className="p-0 h-auto font-semibold text-primary hover:underline"
            onClick={onSwitchToRegister}
            disabled={isLoading}
            aria-label="Switch to registration form"
          >
            Sign up here
          </Button>
        </p>
      </CardFooter>
    </Card>
  );
}