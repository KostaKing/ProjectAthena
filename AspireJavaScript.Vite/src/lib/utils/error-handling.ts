import { toast } from 'sonner'

// Error types for better categorization
export enum ErrorType {
  VALIDATION = 'validation',
  AUTHENTICATION = 'authentication',
  AUTHORIZATION = 'authorization',
  NETWORK = 'network',
  SERVER = 'server',
  CLIENT = 'client',
  UNKNOWN = 'unknown'
}

export interface AppError extends Error {
  type: ErrorType
  code?: string
  statusCode?: number
  details?: Record<string, any>
}

// Error factory functions
export const createError = (
  message: string,
  type: ErrorType = ErrorType.UNKNOWN,
  statusCode?: number,
  details?: Record<string, any>
): AppError => {
  const error = new Error(message) as AppError
  error.type = type
  error.statusCode = statusCode
  error.details = details
  return error
}

export const createValidationError = (
  message: string,
  field?: string,
  value?: any
): AppError => {
  return createError(message, ErrorType.VALIDATION, 400, { field, value })
}

export const createAuthError = (message: string, statusCode: number = 401): AppError => {
  return createError(message, ErrorType.AUTHENTICATION, statusCode)
}

export const createNetworkError = (message: string = 'Network error occurred'): AppError => {
  return createError(message, ErrorType.NETWORK, 0)
}

// Error parsing from API responses
export const parseApiError = async (response: Response): Promise<AppError> => {
  let message = 'An unexpected error occurred'
  let details: Record<string, any> = {}
  
  try {
    const contentType = response.headers.get('content-type')
    
    if (contentType?.includes('application/json')) {
      const errorData = await response.json()
      
      // Handle different API error formats
      if (errorData.message) {
        message = errorData.message
      } else if (errorData.error) {
        message = errorData.error
      } else if (errorData.title) {
        message = errorData.title
      }
      
      // Extract validation errors
      if (errorData.errors) {
        details.validationErrors = errorData.errors
      }
      
      details.raw = errorData
    } else {
      message = await response.text() || message
    }
  } catch (parseError) {
    console.warn('Failed to parse error response:', parseError)
  }
  
  // Determine error type based on status code
  let type = ErrorType.SERVER
  if (response.status >= 400 && response.status < 500) {
    if (response.status === 401) {
      type = ErrorType.AUTHENTICATION
    } else if (response.status === 403) {
      type = ErrorType.AUTHORIZATION
    } else if (response.status === 422 || response.status === 400) {
      type = ErrorType.VALIDATION
    } else {
      type = ErrorType.CLIENT
    }
  }
  
  return createError(message, type, response.status, details)
}

// Error handling utilities
export const handleApiError = async (response: Response): Promise<never> => {
  const error = await parseApiError(response)
  throw error
}

export const isAppError = (error: any): error is AppError => {
  return error instanceof Error && 'type' in error
}

export const getErrorMessage = (error: unknown): string => {
  if (isAppError(error)) {
    return error.message
  }
  
  if (error instanceof Error) {
    return error.message
  }
  
  if (typeof error === 'string') {
    return error
  }
  
  return 'An unexpected error occurred'
}

// Toast error handlers with categorization
export const showErrorToast = (error: unknown, fallbackMessage?: string) => {
  const message = getErrorMessage(error)
  
  if (isAppError(error)) {
    switch (error.type) {
      case ErrorType.VALIDATION:
        toast.error(`Validation Error: ${message}`)
        break
      case ErrorType.AUTHENTICATION:
        toast.error(`Authentication Error: ${message}`)
        break
      case ErrorType.AUTHORIZATION:
        toast.error(`Access Denied: ${message}`)
        break
      case ErrorType.NETWORK:
        toast.error(`Network Error: ${message}`)
        break
      default:
        toast.error(message)
    }
  } else {
    toast.error(fallbackMessage || message)
  }
}

// Retry mechanism for network errors
export const withRetry = async <T>(
  operation: () => Promise<T>,
  maxRetries: number = 3,
  delayMs: number = 1000
): Promise<T> => {
  let lastError: Error
  
  for (let attempt = 1; attempt <= maxRetries; attempt++) {
    try {
      return await operation()
    } catch (error) {
      lastError = error instanceof Error ? error : new Error(String(error))
      
      if (attempt === maxRetries) {
        break
      }
      
      // Only retry on network errors or 5xx server errors
      if (isAppError(error)) {
        if (error.type !== ErrorType.NETWORK && 
            (error.statusCode === undefined || error.statusCode < 500)) {
          break
        }
      }
      
      // Exponential backoff
      await new Promise(resolve => setTimeout(resolve, delayMs * Math.pow(2, attempt - 1)))
    }
  }
  
  throw lastError!
}

// Global error boundary helper
export const logError = (error: unknown, context?: string) => {
  const errorInfo = {
    message: getErrorMessage(error),
    context,
    timestamp: new Date().toISOString(),
    url: window.location.href,
    userAgent: navigator.userAgent,
    ...(isAppError(error) && {
      type: error.type,
      statusCode: error.statusCode,
      details: error.details
    })
  }
  
  console.error('Application Error:', errorInfo)
  
  // In production, you would send this to your error tracking service
  // Example: sendToErrorTrackingService(errorInfo)
}

// Form error helpers
export const getFieldError = (error: AppError, fieldName: string): string | undefined => {
  if (error.type === ErrorType.VALIDATION && error.details?.validationErrors) {
    const fieldErrors = error.details.validationErrors[fieldName]
    if (Array.isArray(fieldErrors) && fieldErrors.length > 0) {
      return fieldErrors[0]
    }
  }
  return undefined
}

export const hasFieldError = (error: AppError, fieldName: string): boolean => {
  return getFieldError(error, fieldName) !== undefined
}

// API client error handler
export const createApiErrorHandler = (_defaultMessage: string) => {
  return async (response: Response) => {
    if (!response.ok) {
      const error = await parseApiError(response)
      logError(error, `API ${response.url}`)
      throw error
    }
    return response
  }
}