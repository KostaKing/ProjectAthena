import { toast as sonnerToast } from "sonner"

export const useToast = () => {
  return {
    toast: ({
      title,
      description,
      variant = "default",
      ...props
    }: {
      title?: string
      description?: string
      variant?: "default" | "destructive"
    }) => {
      if (variant === "destructive") {
        sonnerToast.error(title || description, {
          description: title ? description : undefined,
        })
      } else {
        sonnerToast.success(title || description, {
          description: title ? description : undefined,
        })
      }
    },
  }
}