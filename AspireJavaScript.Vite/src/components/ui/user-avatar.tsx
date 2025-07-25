import { Avatar, AvatarFallback, AvatarImage } from "./avatar"
import { cn } from "../../lib/utils"

interface UserAvatarProps {
  user: {
    firstName?: string
    lastName?: string
    fullName?: string
    email?: string
  }
  src?: string
  alt?: string
  className?: string
  size?: 'sm' | 'md' | 'lg' | 'xl'
  showStatus?: boolean
  isOnline?: boolean
}

const sizeClasses = {
  sm: 'h-6 w-6 text-xs',
  md: 'h-8 w-8 text-sm',
  lg: 'h-10 w-10 text-sm',
  xl: 'h-12 w-12 text-base'
}

const statusClasses = {
  sm: 'h-2 w-2',
  md: 'h-2.5 w-2.5',
  lg: 'h-3 w-3',
  xl: 'h-3.5 w-3.5'
}

export function UserAvatar({ 
  user, 
  src, 
  alt, 
  className, 
  size = 'md',
  showStatus = false,
  isOnline = false
}: UserAvatarProps) {
  // Generate initials from user name
  const getInitials = () => {
    if (user.firstName && user.lastName) {
      return `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase()
    }
    
    if (user.fullName) {
      const names = user.fullName.split(' ')
      if (names.length >= 2) {
        return `${names[0].charAt(0)}${names[names.length - 1].charAt(0)}`.toUpperCase()
      }
      return names[0].charAt(0).toUpperCase()
    }
    
    if (user.email) {
      return user.email.charAt(0).toUpperCase()
    }
    
    return '?'
  }

  // Generate consistent color based on user's name or email
  const getAvatarColor = () => {
    const str = user.fullName || user.email || 'default'
    let hash = 0
    for (let i = 0; i < str.length; i++) {
      hash = str.charCodeAt(i) + ((hash << 5) - hash)
    }
    
    const colors = [
      'bg-red-500 text-white',
      'bg-blue-500 text-white',
      'bg-green-500 text-white',
      'bg-yellow-500 text-white',
      'bg-purple-500 text-white',
      'bg-pink-500 text-white',
      'bg-indigo-500 text-white',
      'bg-teal-500 text-white',
    ]
    
    return colors[Math.abs(hash) % colors.length]
  }

  const displayName = user.fullName || `${user.firstName || ''} ${user.lastName || ''}`.trim()

  return (
    <div className="relative inline-block">
      <Avatar className={cn(sizeClasses[size], className)}>
        <AvatarImage 
          src={src} 
          alt={alt || displayName || user.email} 
        />
        <AvatarFallback className={getAvatarColor()}>
          {getInitials()}
        </AvatarFallback>
      </Avatar>
      
      {showStatus && (
        <div 
          className={cn(
            "absolute bottom-0 right-0 rounded-full border-2 border-background",
            statusClasses[size],
            isOnline ? "bg-green-500" : "bg-gray-400"
          )}
        />
      )}
    </div>
  )
}