import { useState } from 'react';
import { Header } from '../layout/Header';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Button } from '../ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../ui/tabs';
import { CourseManagement } from './CourseManagement';
import { EnrollmentManagement } from './EnrollmentManagement';
import { BookOpen, Users, GraduationCap, TrendingUp, Plus } from 'lucide-react';

export function AdminDashboard() {
  const [activeTab, setActiveTab] = useState('overview');

  return (
    <div className="min-h-screen bg-gray-50">
      <Header />
      
      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <div className="mb-8">
            <h1 className="text-3xl font-bold text-gray-900">
              Learning Management Dashboard
            </h1>
            <p className="mt-2 text-gray-600">
              Manage courses, enrollments, and generate reports for your educational platform.
            </p>
          </div>

          <Tabs value={activeTab} onValueChange={setActiveTab} className="space-y-6">
            <TabsList className="grid w-full grid-cols-3">
              <TabsTrigger value="overview">Overview</TabsTrigger>
              <TabsTrigger value="courses">Course Management</TabsTrigger>
              <TabsTrigger value="enrollments">Enrollment Management</TabsTrigger>
            </TabsList>

            <TabsContent value="overview" className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
                <Card>
                  <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle className="text-sm font-medium">Total Courses</CardTitle>
                    <BookOpen className="h-4 w-4 text-muted-foreground" />
                  </CardHeader>
                  <CardContent>
                    <div className="text-2xl font-bold">0</div>
                    <p className="text-xs text-muted-foreground">
                      Active courses in system
                    </p>
                  </CardContent>
                </Card>

                <Card>
                  <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle className="text-sm font-medium">Total Enrollments</CardTitle>
                    <Users className="h-4 w-4 text-muted-foreground" />
                  </CardHeader>
                  <CardContent>
                    <div className="text-2xl font-bold">0</div>
                    <p className="text-xs text-muted-foreground">
                      Active student enrollments
                    </p>
                  </CardContent>
                </Card>

                <Card>
                  <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle className="text-sm font-medium">Completion Rate</CardTitle>
                    <GraduationCap className="h-4 w-4 text-muted-foreground" />
                  </CardHeader>
                  <CardContent>
                    <div className="text-2xl font-bold">0%</div>
                    <p className="text-xs text-muted-foreground">
                      Course completion rate
                    </p>
                  </CardContent>
                </Card>

                <Card>
                  <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle className="text-sm font-medium">Average Grade</CardTitle>
                    <TrendingUp className="h-4 w-4 text-muted-foreground" />
                  </CardHeader>
                  <CardContent>
                    <div className="text-2xl font-bold">-</div>
                    <p className="text-xs text-muted-foreground">
                      Overall student performance
                    </p>
                  </CardContent>
                </Card>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <Card>
                  <CardHeader>
                    <CardTitle>Quick Actions</CardTitle>
                    <CardDescription>
                      Common administrative tasks
                    </CardDescription>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <Button 
                      onClick={() => setActiveTab('courses')} 
                      className="w-full justify-start"
                      variant="outline"
                    >
                      <Plus className="mr-2 h-4 w-4" />
                      Add New Course
                    </Button>
                    <Button 
                      onClick={() => setActiveTab('enrollments')} 
                      className="w-full justify-start"
                      variant="outline"
                    >
                      <Users className="mr-2 h-4 w-4" />
                      Manage Enrollments
                    </Button>
                  </CardContent>
                </Card>

                <Card>
                  <CardHeader>
                    <CardTitle>Recent Activity</CardTitle>
                    <CardDescription>
                      Latest system activities
                    </CardDescription>
                  </CardHeader>
                  <CardContent>
                    <p className="text-sm text-muted-foreground">
                      No recent activity to display.
                    </p>
                  </CardContent>
                </Card>
              </div>
            </TabsContent>

            <TabsContent value="courses">
              <CourseManagement />
            </TabsContent>

            <TabsContent value="enrollments">
              <EnrollmentManagement />
            </TabsContent>
          </Tabs>
        </div>
      </main>
    </div>
  );
}