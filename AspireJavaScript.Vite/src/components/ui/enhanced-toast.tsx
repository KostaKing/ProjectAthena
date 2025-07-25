import { toast as sonnerToast } from "sonner"
import { CheckCircle, XCircle, AlertCircle, Info, Loader2 } from "lucide-react"

interface ToastOptions {
  title?: string
  description?: string
  variant?: "default" | "destructive" | "success" | "warning" | "info"
  action?: {
    label: string
    onClick: () => void
  }
  duration?: number
}

export const enhancedToast = {
  success: (message: string, options?: Omit<ToastOptions, 'variant'>) => {
    sonnerToast.success(message, {
      description: options?.description,
      duration: options?.duration || 4000,
      icon: <CheckCircle className="h-4 w-4" />,
      action: options?.action ? {
        label: options.action.label,
        onClick: options.action.onClick
      } : undefined,
    })
  },

  error: (message: string, options?: Omit<ToastOptions, 'variant'>) => {
    sonnerToast.error(message, {
      description: options?.description,
      duration: options?.duration || 6000,
      icon: <XCircle className="h-4 w-4" />,
      action: options?.action ? {
        label: options.action.label,
        onClick: options.action.onClick
      } : undefined,
    })
  },

  warning: (message: string, options?: Omit<ToastOptions, 'variant'>) => {
    sonnerToast.warning(message, {
      description: options?.description,
      duration: options?.duration || 5000,
      icon: <AlertCircle className="h-4 w-4" />,
      action: options?.action ? {
        label: options.action.label,
        onClick: options.action.onClick
      } : undefined,
    })
  },

  info: (message: string, options?: Omit<ToastOptions, 'variant'>) => {
    sonnerToast.info(message, {
      description: options?.description,
      duration: options?.duration || 4000,
      icon: <Info className="h-4 w-4" />,
      action: options?.action ? {
        label: options.action.label,
        onClick: options.action.onClick
      } : undefined,
    })
  },

  loading: (message: string, options?: Omit<ToastOptions, 'variant'>) => {
    return sonnerToast.loading(message, {
      description: options?.description,
      icon: <Loader2 className="h-4 w-4 animate-spin" />,
    })
  },

  promise: <T,>(
    promise: Promise<T>,
    options: {
      loading: string
      success: string | ((data: T) => string)
      error: string | ((error: any) => string)
    }
  ) => {
    return sonnerToast.promise(promise, {
      loading: options.loading,
      success: options.success,
      error: options.error,
    })
  }
}

export const useEnhancedToast = () => {
  return enhancedToast
}