import { useAuth } from '../../hooks/useAuth';
import { UserRole } from '../../types/auth';
import { Button } from '../ui/button';
import { ThemeToggleButton } from '../ui/theme-toggle';
import { Badge } from '../ui/badge';
import { UserAvatar } from '../ui/user-avatar';
import { 
  DropdownMenu, 
  DropdownMenuContent, 
  DropdownMenuItem, 
  DropdownMenuLabel, 
  DropdownMenuSeparator, 
  DropdownMenuTrigger 
} from '../ui/dropdown-menu';
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from '../ui/sheet';
import { LogOut, Settings, ChevronDown, Shield, GraduationCap, BookOpen, Menu } from 'lucide-react';

export function Header() {
  const { user, logout } = useAuth();


  const getRoleName = (role: UserRole) => {
    switch (role) {
      case UserRole.Value3: // Admin
        return 'Admin';
      case UserRole.Value2: // Teacher
        return 'Teacher';
      case UserRole.Value1: // Student
        return 'Student';
      default:
        return 'Unknown';
    }
  };

  return (
    <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="container flex h-16 items-center">
        <div className="flex items-center space-x-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-md bg-primary text-primary-foreground">
            <GraduationCap className="h-5 w-5" />
          </div>
          <h1 className="text-xl font-bold tracking-tight">
            ProjectAthena
          </h1>
        </div>

        <div className="flex flex-1 items-center justify-end space-x-4">
          {user && (
            <>
              <div className="hidden md:flex items-center space-x-3">
                <div className="flex items-center space-x-2 text-sm">
                  <span className="font-medium">{user.fullName}</span>
                  <Badge 
                    variant={user.role === UserRole.Value3 ? 'destructive' : user.role === UserRole.Value2 ? 'secondary' : 'default'}
                    className="text-xs"
                  >
                    {user.role === UserRole.Value3 && <Shield className="mr-1 h-3 w-3" />}
                    {user.role === UserRole.Value2 && <BookOpen className="mr-1 h-3 w-3" />}
                    {user.role === UserRole.Value1 && <GraduationCap className="mr-1 h-3 w-3" />}
                    {getRoleName(user.role!)}
                  </Badge>
                </div>
              </div>

              <div className="flex items-center space-x-2">
                {/* Mobile menu button */}
                <div className="md:hidden">
                  <Sheet>
                    <SheetTrigger asChild>
                      <Button variant="ghost" size="sm">
                        <Menu className="h-5 w-5" />
                        <span className="sr-only">Open menu</span>
                      </Button>
                    </SheetTrigger>
                    <SheetContent side="right" className="w-80">
                      <SheetHeader>
                        <SheetTitle>ProjectAthena</SheetTitle>
                      </SheetHeader>
                      <div className="mt-6 space-y-4">
                        {/* User info */}
                        <div className="flex items-center space-x-3 p-4 bg-muted rounded-lg">
                          <UserAvatar user={{
                            firstName: user.firstName || undefined,
                            lastName: user.lastName || undefined,
                            fullName: user.fullName || undefined,
                            email: user.email || undefined
                          }} size="md" />
                          <div>
                            <p className="font-medium">{user.fullName}</p>
                            <p className="text-sm text-muted-foreground">{user.email}</p>
                            <Badge 
                              variant={user.role === UserRole.Value3 ? 'destructive' : user.role === UserRole.Value2 ? 'secondary' : 'default'}
                              className="text-xs mt-1"
                            >
                              {user.role === UserRole.Value3 && <Shield className="mr-1 h-3 w-3" />}
                              {user.role === UserRole.Value2 && <BookOpen className="mr-1 h-3 w-3" />}
                              {user.role === UserRole.Value1 && <GraduationCap className="mr-1 h-3 w-3" />}
                              {getRoleName(user.role!)}
                            </Badge>
                          </div>
                        </div>
                        
                        {/* Theme toggle */}
                        <div className="p-4 border rounded-lg">
                          <div className="flex items-center justify-between">
                            <span className="text-sm font-medium">Theme</span>
                            <ThemeToggleButton />
                          </div>
                        </div>
                        
                        {/* Logout button */}
                        <Button 
                          variant="ghost" 
                          className="w-full justify-start" 
                          onClick={logout}
                        >
                          <LogOut className="mr-2 h-4 w-4" />
                          Log Out
                        </Button>
                      </div>
                    </SheetContent>
                  </Sheet>
                </div>
                
                {/* Desktop theme toggle */}
                <div className="hidden md:block">
                  <ThemeToggleButton />
                </div>
                
                {/* Desktop user menu */}
                <div className="hidden md:block">
                  <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button 
                      variant="ghost" 
                      size="sm" 
                      className="relative h-9 w-9 rounded-full md:h-8 md:w-auto md:rounded-md md:px-3"
                      aria-label="User menu"
                    >
                      <div className="flex items-center space-x-2">
                        <UserAvatar user={{
                          firstName: user.firstName || undefined,
                          lastName: user.lastName || undefined,
                          fullName: user.fullName || undefined,
                          email: user.email || undefined
                        }} size="sm" />
                        <span className="hidden md:inline-block text-sm font-medium">
                          {user.firstName}
                        </span>
                        <ChevronDown className="hidden md:inline-block h-4 w-4" />
                      </div>
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent className="w-56" align="end" forceMount>
                    <DropdownMenuLabel className="font-normal">
                      <div className="flex flex-col space-y-1">
                        <p className="text-sm font-medium leading-none">{user.fullName}</p>
                        <p className="text-xs leading-none text-muted-foreground">
                          {user.email}
                        </p>
                        <div className="pt-1">
                          <Badge 
                            variant={user.role === UserRole.Value3 ? 'destructive' : user.role === UserRole.Value2 ? 'secondary' : 'default'}
                            className="text-xs w-fit"
                          >
                            {user.role === UserRole.Value3 && <Shield className="mr-1 h-3 w-3" />}
                            {user.role === UserRole.Value2 && <BookOpen className="mr-1 h-3 w-3" />}
                            {user.role === UserRole.Value1 && <GraduationCap className="mr-1 h-3 w-3" />}
                            {getRoleName(user.role!)}
                          </Badge>
                        </div>
                      </div>
                    </DropdownMenuLabel>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem>
                      <Settings className="mr-2 h-4 w-4" />
                      <span>Settings</span>
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem onClick={logout} className="text-red-600 dark:text-red-400">
                      <LogOut className="mr-2 h-4 w-4" />
                      <span>Sign out</span>
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
                </div>
              </div>
            </>
          )}
        </div>
      </div>
    </header>
  );
}