import { useState, useEffect, useMemo } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Badge } from '../ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../ui/select';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../ui/tabs';
import { DataTable } from '../ui/data-table';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../ui/dialog';
import { authApi, type UserDto } from '../../services/authApi';
import { toast } from 'sonner';
import { ColumnDef } from '@tanstack/react-table';
import { 
  Users, 
  Search, 
  Edit, 
  Mail,
  UserCheck,
  UserX,
  GraduationCap,
  BookOpen,
  Shield,
  User,
  MoreHorizontal,
  ArrowUpDown,
  Trash2
} from 'lucide-react';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator, DropdownMenuTrigger } from '../ui/dropdown-menu';

// User role mapping
const UserRoleValues = {
  Student: 1,
  Teacher: 2,  
  Admin: 3,
} as const;

export function UserManagement() {
  const [users, setUsers] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [roleFilter, setRoleFilter] = useState<number | 'all'>('all');
  const [statusFilter, setStatusFilter] = useState<boolean | 'all'>('all');
  const [activeTab, setActiveTab] = useState('all');

  useEffect(() => {
    fetchUsers();
  }, []);

  const fetchUsers = async () => {
    try {
      setLoading(true);
      const fetchedUsers = await authApi.getAllUsers();
      setUsers(fetchedUsers);
    } catch (error) {
      toast.error('Failed to fetch users');
      console.error('Error fetching users:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleActivateUser = async (userId: string) => {
    try {
      const updatedUser = await authApi.activateUser(userId);
      toast.success('User activated successfully');
      
      // Update local state instead of refetching
      setUsers(prevUsers => 
        prevUsers.map(user => 
          user.id === userId ? { ...user, isActive: updatedUser.isActive } : user
        )
      );
    } catch (error) {
      toast.error('Failed to activate user');
      console.error('Error activating user:', error);
    }
  };

  const handleDeactivateUser = async (userId: string) => {
    try {
      const updatedUser = await authApi.deactivateUser(userId);
      toast.success('User deactivated successfully');
      
      // Update local state instead of refetching
      setUsers(prevUsers => 
        prevUsers.map(user => 
          user.id === userId ? { ...user, isActive: updatedUser.isActive } : user
        )
      );
    } catch (error) {
      toast.error('Failed to deactivate user');
      console.error('Error deactivating user:', error);
    }
  };

  const handleDeleteUser = async (userId: string) => {
    try {
      await authApi.deleteUser(userId);
      toast.success('User deleted successfully');
      
      // Remove user from local state instead of refetching
      setUsers(prevUsers => prevUsers.filter(user => user.id !== userId));
    } catch (error) {
      toast.error('Failed to delete user');
      console.error('Error deleting user:', error);
    }
  };

  const getRoleBadge = (role?: number) => {
    const variants = {
      [UserRoleValues.Student]: { variant: 'default' as const, label: 'Student', icon: GraduationCap },
      [UserRoleValues.Teacher]: { variant: 'secondary' as const, label: 'Teacher', icon: BookOpen },
      [UserRoleValues.Admin]: { variant: 'destructive' as const, label: 'Admin', icon: Shield },
    };
    
    const config = variants[role as keyof typeof variants];
    if (!config) return <Badge variant="outline">Unknown</Badge>;
    
    const IconComponent = config.icon;
    return (
      <Badge variant={config.variant} className="flex items-center gap-1">
        <IconComponent className="h-3 w-3" />
        {config.label}
      </Badge>
    );
  };

  const getStatusBadge = (isActive?: boolean) => {
    return isActive ? (
      <Badge variant="secondary" className="flex items-center gap-1">
        <UserCheck className="h-3 w-3" />
        Active
      </Badge>
    ) : (
      <Badge variant="outline" className="flex items-center gap-1">
        <UserX className="h-3 w-3" />
        Inactive
      </Badge>
    );
  };

  const filteredUsers = users.filter(user => {
    const matchesSearch = !searchTerm || 
      user.fullName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      user.email?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      user.firstName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      user.lastName?.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesRole = roleFilter === 'all' || user.role === roleFilter;
    const matchesStatus = statusFilter === 'all' || user.isActive === statusFilter;
    const matchesTab = activeTab === 'all' || 
      (activeTab === 'students' && user.role === UserRoleValues.Student) ||
      (activeTab === 'teachers' && user.role === UserRoleValues.Teacher) ||
      (activeTab === 'admins' && user.role === UserRoleValues.Admin);
    
    return matchesSearch && matchesRole && matchesStatus && matchesTab;
  });

  const formatDate = (dateString?: string) => {
    return dateString ? new Date(dateString).toLocaleDateString() : 'N/A';
  };

  const [confirmDialog, setConfirmDialog] = useState<{
    open: boolean;
    user: UserDto | null;
    action: 'activate' | 'deactivate' | 'delete';
  }>({ open: false, user: null, action: 'activate' });

  const userColumns: ColumnDef<UserDto>[] = useMemo(() => [
    {
      accessorKey: "fullName",
      id: "fullName",
      header: ({ column }) => (
        <Button
          variant="ghost"
          onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
          className="h-auto p-0 font-semibold"
        >
          User
          <ArrowUpDown className="ml-2 h-4 w-4" />
        </Button>
      ),
      cell: ({ row }) => {
        const user = row.original;
        return (
          <div className="flex items-center gap-3">
            <div className="h-10 w-10 rounded-full bg-muted flex items-center justify-center">
              <User className="h-5 w-5 text-muted-foreground" />
            </div>
            <div>
              <div className="font-medium">
                {user.fullName || `${user.firstName} ${user.lastName}`}
              </div>
              <div className="text-sm text-muted-foreground flex items-center gap-1">
                <Mail className="h-3 w-3" />
                {user.email}
              </div>
            </div>
          </div>
        );
      },
      accessorFn: (row) => row.fullName || `${row.firstName} ${row.lastName}`,
    },
    {
      accessorKey: "role",
      header: ({ column }) => (
        <Button
          variant="ghost"
          onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
          className="h-auto p-0 font-semibold"
        >
          Role
          <ArrowUpDown className="ml-2 h-4 w-4" />
        </Button>
      ),
      cell: ({ row }) => getRoleBadge(row.getValue("role")),
    },
    {
      accessorKey: "isActive",
      header: ({ column }) => (
        <Button
          variant="ghost"
          onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
          className="h-auto p-0 font-semibold"
        >
          Status
          <ArrowUpDown className="ml-2 h-4 w-4" />
        </Button>
      ),
      cell: ({ row }) => getStatusBadge(row.getValue("isActive")),
    },
    {
      accessorKey: "createdAt",
      header: ({ column }) => (
        <Button
          variant="ghost"
          onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
          className="h-auto p-0 font-semibold"
        >
          Created
          <ArrowUpDown className="ml-2 h-4 w-4" />
        </Button>
      ),
      cell: ({ row }) => {
        const date = row.getValue("createdAt") as string;
        return (
          <div className="text-sm">
            <div>{formatDate(date)}</div>
            {row.original.lastLoginAt && (
              <div className="text-xs text-muted-foreground">
                Last login: {formatDate(row.original.lastLoginAt)}
              </div>
            )}
          </div>
        );
      },
    },
    {
      id: "actions",
      header: "Actions",
      cell: ({ row }) => {
        const user = row.original;
        return (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="h-8 w-8 p-0">
                <span className="sr-only">Open menu</span>
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuLabel>Actions</DropdownMenuLabel>
              <DropdownMenuItem>
                <Edit className="mr-2 h-4 w-4" />
                Edit User
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem
                className={user.isActive ? "text-red-600" : "text-green-600"}
                onClick={() => setConfirmDialog({
                  open: true,
                  user,
                  action: user.isActive ? 'deactivate' : 'activate'
                })}
              >
                {user.isActive ? (
                  <>
                    <UserX className="mr-2 h-4 w-4" />
                    Deactivate
                  </>
                ) : (
                  <>
                    <UserCheck className="mr-2 h-4 w-4" />
                    Activate
                  </>
                )}
              </DropdownMenuItem>
              <DropdownMenuItem
                className="text-red-600"
                onClick={() => setConfirmDialog({
                  open: true,
                  user,
                  action: 'delete'
                })}
              >
                <Trash2 className="mr-2 h-4 w-4" />
                Delete User
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        );
      },
    },
  ], []);

  const userCounts = {
    total: users.length,
    students: users.filter(u => u.role === UserRoleValues.Student).length,
    teachers: users.filter(u => u.role === UserRoleValues.Teacher).length,
    admins: users.filter(u => u.role === UserRoleValues.Admin).length,
    active: users.filter(u => u.isActive).length,
    inactive: users.filter(u => !u.isActive).length,
  };

  if (loading) {
    return (
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <div className="space-y-1">
            <div className="h-8 w-64 bg-muted animate-pulse rounded" />
            <div className="h-4 w-96 bg-muted animate-pulse rounded" />
          </div>
          <div className="h-9 w-24 bg-muted animate-pulse rounded" />
        </div>
        <div className="grid grid-cols-2 md:grid-cols-6 gap-4">
          {Array.from({ length: 6 }).map((_, i) => (
            <Card key={i}>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <div className="h-4 w-20 bg-muted animate-pulse rounded" />
                <div className="h-4 w-4 bg-muted animate-pulse rounded" />
              </CardHeader>
              <CardContent>
                <div className="h-8 w-8 bg-muted animate-pulse rounded mb-2" />
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div className="space-y-1">
          <h2 className="text-2xl font-bold tracking-tight">User Management</h2>
          <p className="text-muted-foreground">
            Manage all users in your system
          </p>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-2 md:grid-cols-6 gap-4">
        <Card className="transition-all duration-200 hover:shadow-md border-l-4 border-l-blue-500">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Users</CardTitle>
            <div className="p-2 bg-blue-100 dark:bg-blue-900 rounded-full">
              <Users className="h-4 w-4 text-blue-600 dark:text-blue-400" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{userCounts.total}</div>
          </CardContent>
        </Card>

        <Card className="transition-all duration-200 hover:shadow-md border-l-4 border-l-green-500">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Students</CardTitle>
            <div className="p-2 bg-green-100 dark:bg-green-900 rounded-full">
              <GraduationCap className="h-4 w-4 text-green-600 dark:text-green-400" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{userCounts.students}</div>
          </CardContent>
        </Card>

        <Card className="transition-all duration-200 hover:shadow-md border-l-4 border-l-purple-500">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Teachers</CardTitle>
            <div className="p-2 bg-purple-100 dark:bg-purple-900 rounded-full">
              <BookOpen className="h-4 w-4 text-purple-600 dark:text-purple-400" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{userCounts.teachers}</div>
          </CardContent>
        </Card>

        <Card className="transition-all duration-200 hover:shadow-md border-l-4 border-l-red-500">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Admins</CardTitle>
            <div className="p-2 bg-red-100 dark:bg-red-900 rounded-full">
              <Shield className="h-4 w-4 text-red-600 dark:text-red-400" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{userCounts.admins}</div>
          </CardContent>
        </Card>

        <Card className="transition-all duration-200 hover:shadow-md border-l-4 border-l-emerald-500">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Active</CardTitle>
            <div className="p-2 bg-emerald-100 dark:bg-emerald-900 rounded-full">
              <UserCheck className="h-4 w-4 text-emerald-600 dark:text-emerald-400" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{userCounts.active}</div>
          </CardContent>
        </Card>

        <Card className="transition-all duration-200 hover:shadow-md border-l-4 border-l-gray-500">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Inactive</CardTitle>
            <div className="p-2 bg-gray-100 dark:bg-gray-800 rounded-full">
              <UserX className="h-4 w-4 text-gray-600 dark:text-gray-400" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{userCounts.inactive}</div>
          </CardContent>
        </Card>
      </div>

      {/* Main Content */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="grid w-full grid-cols-4 lg:w-fit">
          <TabsTrigger value="all" className="text-sm">
            All ({userCounts.total})
          </TabsTrigger>
          <TabsTrigger value="students" className="text-sm">
            <GraduationCap className="mr-1 h-3 w-3" />
            Students ({userCounts.students})
          </TabsTrigger>
          <TabsTrigger value="teachers" className="text-sm">
            <BookOpen className="mr-1 h-3 w-3" />
            Teachers ({userCounts.teachers})
          </TabsTrigger>
          <TabsTrigger value="admins" className="text-sm">
            <Shield className="mr-1 h-3 w-3" />
            Admins ({userCounts.admins})
          </TabsTrigger>
        </TabsList>

        <TabsContent value={activeTab} className="space-y-4">
          {/* Search and Filter */}
          <Card className="transition-all duration-200 hover:shadow-md">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Search className="h-5 w-5" />
                Search & Filter Users
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex flex-col sm:flex-row gap-4">
                <div className="relative flex-1">
                  <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                  <Input
                    placeholder="Search by name or email..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="pl-10"
                    aria-label="Search users"
                  />
                </div>
                <div className="flex flex-col sm:flex-row gap-2">
                  <Select value={roleFilter.toString()} onValueChange={(value: string) => setRoleFilter(value === 'all' ? 'all' : parseInt(value))}>
                    <SelectTrigger className="w-full sm:w-48">
                      <SelectValue placeholder="Filter by role" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">All Roles</SelectItem>
                      <SelectItem value={UserRoleValues.Student.toString()}>
                        <div className="flex items-center gap-2">
                          <GraduationCap className="h-4 w-4" />
                          Students
                        </div>
                      </SelectItem>
                      <SelectItem value={UserRoleValues.Teacher.toString()}>
                        <div className="flex items-center gap-2">
                          <BookOpen className="h-4 w-4" />
                          Teachers
                        </div>
                      </SelectItem>
                      <SelectItem value={UserRoleValues.Admin.toString()}>
                        <div className="flex items-center gap-2">
                          <Shield className="h-4 w-4" />
                          Admins
                        </div>
                      </SelectItem>
                    </SelectContent>
                  </Select>
                  <Select value={statusFilter.toString()} onValueChange={(value: string) => setStatusFilter(value === 'all' ? 'all' : value === 'true')}>
                    <SelectTrigger className="w-full sm:w-48">
                      <SelectValue placeholder="Filter by status" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">All Status</SelectItem>
                      <SelectItem value="true">
                        <div className="flex items-center gap-2">
                          <UserCheck className="h-4 w-4 text-green-600" />
                          Active
                        </div>
                      </SelectItem>
                      <SelectItem value="false">
                        <div className="flex items-center gap-2">
                          <UserX className="h-4 w-4 text-gray-600" />
                          Inactive
                        </div>
                      </SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* User Data Table */}
          <Card className="transition-all duration-200 hover:shadow-md">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Users className="h-5 w-5" />
                User Directory ({filteredUsers.length} users)
              </CardTitle>
            </CardHeader>
            <CardContent>
              <DataTable
                columns={userColumns}
                data={filteredUsers}
                searchKey="fullName"
                searchPlaceholder="Search users by name..."
              />
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Confirmation Dialog */}
      <Dialog open={confirmDialog.open} onOpenChange={(open) => setConfirmDialog(prev => ({ ...prev, open }))}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {confirmDialog.action === 'activate' 
                ? 'Activate User' 
                : confirmDialog.action === 'deactivate' 
                  ? 'Deactivate User' 
                  : 'Delete User'}
            </DialogTitle>
            <DialogDescription>
              Are you sure you want to {confirmDialog.action} {confirmDialog.user?.fullName || confirmDialog.user?.firstName}?
              {confirmDialog.action === 'deactivate' && ' This will revoke their access to the system.'}
              {confirmDialog.action === 'delete' && ' This action cannot be undone and will permanently remove the user from the system.'}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button 
              variant="outline" 
              onClick={() => setConfirmDialog({ open: false, user: null, action: 'activate' })}
            >
              Cancel
            </Button>
            <Button 
              variant={confirmDialog.action === 'deactivate' || confirmDialog.action === 'delete' ? 'destructive' : 'default'}
              onClick={async () => {
                if (confirmDialog.user) {
                  if (confirmDialog.action === 'activate') {
                    await handleActivateUser(confirmDialog.user.id!);
                  } else if (confirmDialog.action === 'deactivate') {
                    await handleDeactivateUser(confirmDialog.user.id!);
                  } else if (confirmDialog.action === 'delete') {
                    await handleDeleteUser(confirmDialog.user.id!);
                  }
                }
                setConfirmDialog({ open: false, user: null, action: 'activate' });
              }}
            >
              {confirmDialog.action === 'activate' 
                ? 'Activate' 
                : confirmDialog.action === 'deactivate' 
                  ? 'Deactivate' 
                  : 'Delete'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}