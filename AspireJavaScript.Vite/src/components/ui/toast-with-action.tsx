import { toast as sonnerToast } from "sonner"
import { Button } from "./button"
import { X, CheckCircle, AlertCircle, Info, AlertTriangle } from "lucide-react"

interface ToastAction {
  label: string
  onClick: () => void
}

interface ToastOptions {
  title?: string
  description?: string
  action?: ToastAction
  duration?: number
  variant?: 'default' | 'success' | 'warning' | 'error' | 'info'
}

const toastVariants = {
  default: {
    icon: Info,
    className: 'border-border'
  },
  success: {
    icon: CheckCircle,
    className: 'border-green-200 bg-green-50 text-green-900 dark:border-green-800 dark:bg-green-950 dark:text-green-50'
  },
  warning: {
    icon: AlertTriangle,
    className: 'border-yellow-200 bg-yellow-50 text-yellow-900 dark:border-yellow-800 dark:bg-yellow-950 dark:text-yellow-50'
  },
  error: {
    icon: AlertCircle,
    className: 'border-red-200 bg-red-50 text-red-900 dark:border-red-800 dark:bg-red-950 dark:text-red-50'
  },
  info: {
    icon: Info,
    className: 'border-blue-200 bg-blue-50 text-blue-900 dark:border-blue-800 dark:bg-blue-950 dark:text-blue-50'
  }
}

export const enhancedToast = {
  show: ({ title, description, action, duration = 5000, variant = 'default' }: ToastOptions) => {
    const config = toastVariants[variant]
    const Icon = config.icon

    return sonnerToast.custom(
      (t) => (
        <div className={`flex items-start gap-3 p-4 rounded-lg border shadow-lg ${config.className}`}>
          <Icon className="h-5 w-5 mt-0.5 shrink-0" />
          <div className="flex-1 space-y-1">
            {title && (
              <div className="font-semibold text-sm">{title}</div>
            )}
            {description && (
              <div className="text-sm opacity-90">{description}</div>
            )}
            {action && (
              <div className="pt-2">
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => {
                    action.onClick()
                    sonnerToast.dismiss(t)
                  }}
                  className="h-8 px-3 text-xs"
                >
                  {action.label}
                </Button>
              </div>
            )}
          </div>
          <Button
            variant="ghost"
            size="sm"
            className="h-6 w-6 p-0 hover:bg-black/5 dark:hover:bg-white/5"
            onClick={() => sonnerToast.dismiss(t)}
          >
            <X className="h-4 w-4" />
          </Button>
        </div>
      ),
      { duration }
    )
  },

  success: (options: Omit<ToastOptions, 'variant'>) => 
    enhancedToast.show({ ...options, variant: 'success' }),

  error: (options: Omit<ToastOptions, 'variant'>) => 
    enhancedToast.show({ ...options, variant: 'error' }),

  warning: (options: Omit<ToastOptions, 'variant'>) => 
    enhancedToast.show({ ...options, variant: 'warning' }),

  info: (options: Omit<ToastOptions, 'variant'>) => 
    enhancedToast.show({ ...options, variant: 'info' }),
}