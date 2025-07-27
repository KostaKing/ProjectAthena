# ProjectAthena - Educational Management System

ProjectAthena is a comprehensive educational management system built with modern .NET technologies and React. It provides a robust platform for managing courses, students, teachers, and enrollments in educational institutions.

## 🏗️ Architecture Overview

ProjectAthena follows a modern microservices architecture using .NET Aspire for orchestration:

### System Components

- **🎯 AspireJavaScript.AppHost**: .NET Aspire orchestrator managing all services
- **🌐 AspireJavaScript.MinimalApi**: Main API service using ASP.NET Core Minimal APIs
- **💾 ProjectAthena.Data**: Entity Framework Core data layer with PostgreSQL
- **📋 ProjectAthena.Dtos**: Data Transfer Objects for API contracts
- **🔄 ProjectAthena.DbWorkerService**: Database seeding and migration service
- **⚛️ AspireJavaScript.Vite**: React frontend with TypeScript and Vite
- **⚙️ AspireJavaScript.ServiceDefaults**: Common service configurations
- **🧪 ProjectAthena.Tests**: Comprehensive integration tests with AWS S3 mock testing

### Technology Stack

| Component | Technology |
|-----------|------------|
| **Backend Framework** | .NET 9.0 with ASP.NET Core Minimal APIs |
| **API Architecture** | Minimal APIs with auto-generated OpenAPI/Swagger |
| **Type Generation** | Automatic TypeScript type generation from C# DTOs |
| **Database** | PostgreSQL with Entity Framework Core |
| **Authentication** | ASP.NET Core Identity + JWT Bearer |
| **Validation** | FluentValidation with endpoint integration |
| **Frontend** | React 19 with TypeScript + auto-generated types |
| **Build Tool** | Vite with HMR and TypeScript support |
| **Styling** | Tailwind CSS with shadcn/ui component library |
| **UI Components** | shadcn/ui with responsive mobile-first design |
| **Orchestration** | .NET Aspire with service discovery |
| **Database Admin** | pgAdmin with Docker integration |
| **Testing** | xUnit with comprehensive integration tests |
| **Production Features** | Environment-specific configs, CancellationToken support |

## 🏛️ Design Decisions

### **Modern API Architecture**
- **Minimal APIs**: Lightweight, high-performance endpoints with automatic OpenAPI generation
- **Type Safety**: End-to-end type safety with auto-generated TypeScript types from C# DTOs
- **Service Discovery**: .NET Aspire automatic service resolution and configuration
- **Hot Reloading**: Vite HMR for instant frontend updates during development

### **Clean Architecture**
- **Separation of Concerns**: Clear boundaries between API, business logic, and data layers
- **Dependency Injection**: Built-in .NET DI container for loose coupling
- **Service Layer Pattern**: Business logic encapsulated in service classes
- **Repository Pattern**: Implicit through Entity Framework Core DbContext

### **Error Handling Strategy**
- **Endpoint Level**: Comprehensive error handling with specific exception types
- **Service Level**: Clean business logic without unnecessary try-catch blocks
- **Consistent Responses**: Standardized error responses with trace IDs for debugging
- **Logging**: Structured logging throughout the application

### **Database Design**
- **Soft Deletes**: Entities marked as inactive rather than physically deleted
- **Audit Trail**: CreatedAt/UpdatedAt timestamps on all entities
- **Referential Integrity**: Proper foreign key relationships
- **Unique Constraints**: Course codes, student numbers, employee numbers

## 🚀 Setup Instructions

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) - **Must be running with healthy engine**
- [Node.js 20+](https://nodejs.org)
- **Optional**: [Visual Studio 2022 17.12+](https://visualstudio.microsoft.com/vs/)

**Important**: Ensure Docker Desktop is running and the Docker engine is healthy before starting the application, as PostgreSQL runs in a Docker container.


**Security Configuration**:
- Set `Jwt:SecretKey` in production using:
  - Azure Key Vault (recommended)
  - Environment variables
  - User secrets (development only)
- Configure CORS origins for your production domains
- Use HTTPS in production environments

**Build Verification**:

**Note**: Before running these verification commands, ensure you've done a normal F5 run first to initialize the database and download Docker images.

```bash
# Ensure 0 warnings and errors
dotnet build --configuration Release

# Verify TypeScript compilation
cd AspireJavaScript.Vite
npx tsc --noEmit -p tsconfig.app.json

# Run all tests
dotnet test
```

### Local Development Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/KostaKing/ProjectAthena.git
   cd ProjectAthena
   ```

2. **Install .NET Aspire Workload** (if encountering Aspire SDK issues)
   ```bash
   dotnet workload install aspire
   ```
   
   **Note**: If you encounter errors like "Unable to load Aspire SDK" or "Aspire workload not found", run the above command to install the required Aspire workload.

3. **Run the Application**

   **Option A: Visual Studio**
   - Open `ProjectAthena.sln`
   - Set `ProjectAthena.AppHost` as startup project
   - Press F5 to run

   **Option B: .NET CLI**
   ```bash
   cd AspireJavaScript.AppHost
   dotnet run
   ```

   **⏳ First Run Note**: The initial startup may take several minutes as Docker images (PostgreSQL, pgAdmin) are downloaded. You can monitor the download progress in the .NET Aspire dashboard logs.

4. **Access the Application**
   - **.NET Aspire Dashboard**: Auto-launches in browser
   - **React Frontend**: Available through Aspire dashboard
   - **API Documentation**: Swagger UI available in development
   - **Database Admin**: pgAdmin available through Aspire dashboard

   **SSL Certificate Issues**: If you encounter SSL certificate errors when accessing the application, try:
   - Using a different browser (Chrome, Firefox, Edge)
   - Opening the application in incognito/private mode
   - Accepting the self-signed certificate warning in your browser

5. **Login to the Application**
   - Navigate to the React frontend through the Aspire dashboard
   - On the login page, **click the "Admin" button** to auto-fill login credentials
   - This will populate the email and password fields with the default admin account
   - Click "Sign In" to access the admin dashboard
   
   **Quick Login Options**:
   - **Admin**: Click "Admin" button for auto-fill (recommended for testing)
   - **Teacher**: Click "Teacher" button for instructor account
   - **Student**: Click "Student" button for student account

### Database Seeding

The `ProjectAthena.DbWorkerService` automatically:
- Creates and migrates the database
- Seeds initial data including:
  - Default admin user
  - Sample courses and instructors
  - Test students and enrollments

## 🎯 Key Features

### **Admin Dashboard**
- **User Management**: Comprehensive user directory with role-based access control
- **Course Management**: Full CRUD operations for courses with instructor assignments
- **Enrollment Oversight**: Monitor and manage student enrollments across all courses
- **System Analytics**: Basic metrics and reporting capabilities
- **Role-Based Navigation**: Different dashboard views for Admin, Teacher, and Student roles

### **Course Management System**
- **Course Creation**: Full CRUD operations with course codes, credits, and enrollment limits
- **Instructor Assignment**: Link courses to qualified teacher accounts
- **Enrollment Tracking**: Monitor current enrollments against maximum capacity
- **Course Scheduling**: Start and end date management for course sessions
- **Data Integrity**: Unique course codes and proper validation throughout

### **Advanced Enrollment Reporting System** 🏆
ProjectAthena's flagship feature - a comprehensive reporting engine that transforms enrollment data into actionable insights.

**📊 Multi-Dimensional Analytics**
- **Flexible Grouping**: Organize reports by Course, Student, Instructor, Status, or Date Range
- **Smart Filtering**: Filter by specific courses, students, enrollment statuses, date ranges, and grade ranges
- **Real-time Statistics**: Live calculation of total enrollments, completion rates, average grades, and performance metrics
- **Dynamic Summary Cards**: Visual dashboard showing active enrollments, completed courses, and grade distributions

**🔍 Advanced Filter Capabilities**
- **Course-Specific Reports**: Generate reports for individual courses or across all courses
- **Student Performance Tracking**: Filter by individual students or analyze entire cohorts
- **Date Range Analysis**: Track enrollment trends over custom time periods
- **Grade Range Filtering**: Focus on specific performance brackets (e.g., students scoring 80-100%)
- **Status-Based Insights**: Analyze active, completed, dropped, or suspended enrollments

**📈 Interactive Data Visualization**
- **Collapsible Filter Panel**: Intuitive UI with expandable/collapsible advanced filters
- **Real-time Validation**: Instant feedback on filter combinations with error prevention
- **Grouped Results Display**: Organized presentation showing enrollment counts and average grades per group
- **Status Badges**: Color-coded indicators for quick enrollment status identification
- **Performance Metrics**: Visual representation of completion rates and grade distributions

**💾 Export & Data Management**
- **JSON Data Export**: Structured enrollment data with comprehensive details
- **Report Data Structure**: Includes student details, course information, grades, dates, and status
- **AWS S3 Integration**: Mock S3 client for testing report storage and retrieval
- **Bulk Operations**: Support for handling multiple reports and data sets

**🎯 Business Intelligence Features**
- **Completion Rate Analysis**: Track course success rates across different time periods
- **Instructor Performance**: Compare enrollment outcomes across different instructors
- **Student Progress Monitoring**: Individual and cohort-based performance tracking
- **Enrollment Trend Analysis**: Historical data analysis for strategic planning
- **Grade Distribution Insights**: Identify patterns in student performance across courses

### **User Management**
- **Role-based Access Control**: Admin, Teacher, Student roles with appropriate permissions
- **Account Management**: Activate/deactivate users with confirmation dialogs
- **User Directory**: Searchable user list with sorting and filtering
- **Status Tracking**: Monitor active/inactive users with visual indicators
- **Batch Operations**: Manage multiple users efficiently

## 🔧 API Endpoints

### Authentication
- `POST /api/auth/login` - User authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/refresh` - Token refresh
- `POST /api/auth/logout` - User logout

### Courses
- `GET /api/courses` - List all courses
- `GET /api/courses/{id}` - Get course by ID
- `POST /api/courses` - Create course (Admin)
- `PUT /api/courses/{id}` - Update course (Admin)
- `DELETE /api/courses/{id}` - Delete course (Admin)

### Enrollments
- `GET /api/enrollments` - List enrollments with pagination
- `POST /api/enrollments` - Create enrollment (Admin)
- `GET /api/enrollments/student/{id}` - Get student enrollments
- `GET /api/enrollments/course/{id}` - Get course enrollments
- `POST /api/enrollments/reports/advanced` - Generate advanced reports

## 🧪 Testing

The project includes comprehensive integration tests covering:
- **Authentication workflows**: JWT token generation and validation for all user roles
- **Course management operations**: CRUD operations with proper authorization
- **Enrollment processes**: Advanced reporting system with filtering and grouping
- **Database operations**: Entity Framework integration with PostgreSQL
- **API endpoint functionality**: End-to-end API testing with real database integration
- **AWS S3 Mock Integration**: Report storage and retrieval testing with mock S3 client

**Test Commands**:
```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run specific test project
dotnet test ProjectAthena.Tests
```

**Test Results Summary**:
- ✅ Authentication Tests: JWT token generation and validation for Admin, Teacher, Student roles
- ✅ Enrollment Integration Tests: Advanced reporting with filtering, grouping, and data export
- ✅ AWS S3 Mock Tests: Report storage, retrieval, and bulk operations with mock S3 client
- ✅ All tests passing with PostgreSQL database integration via Aspire testing framework

## 🛠️ Development Notes

### **Code Quality Standards** 📋
- **Modern C# 12/13**: Pattern matching, collection expressions, and latest language features
- **Async Best Practices**: CancellationToken support throughout the application
- **Error Handling**: Consistent patterns across all endpoints with detailed exception handling
- **Logging**: Structured logging with appropriate levels and trace IDs
- **Validation**: FluentValidation for input validation with custom error messages
- **Documentation**: Comprehensive API documentation with Swagger/OpenAPI
- **Type Safety**: End-to-end type safety from C# DTOs to TypeScript frontend
- **Mobile Responsiveness**: shadcn/ui components with mobile-first responsive design

### **Performance Optimizations** ⚡
- **Minimal APIs**: Zero-allocation, high-performance endpoint routing
- **Modern C# Patterns**: Pattern matching, collection expressions, and async best practices
- **CancellationToken Support**: All async operations support cancellation for better resource management
- **AsNoTracking**: Used for read-only operations to improve EF Core performance
- **Selective Loading**: Include only necessary related data with optimized queries
- **Pagination**: Implemented for large data sets in enrollment listings
- **TypeScript Generation**: Build-time type generation eliminates runtime validation overhead
- **Mobile-First UI**: Responsive design with horizontal scrolling and optimized mobile navigation

### **Security Best Practices** 🛡️
- **Input Validation**: All inputs validated and sanitized using FluentValidation
- **SQL Injection**: Protected via Entity Framework Core parameterized queries
- **JWT Authentication**: Production-ready JWT Bearer token implementation with ASP.NET Core Identity
- **Role-Based Authorization**: Admin, Teacher, Student roles with proper endpoint protection
- **Password Security**: Enforced complexity requirements and account lockout protection
- **CORS Security**: Environment-specific CORS policies (permissive for development, restrictive for production)
- **Secret Management**: Secure JWT secret handling with environment-specific configuration
- **HTTPS Enforcement**: SSL/TLS configuration for production deployments

## 📁 Project Structure

```
ProjectAthena/
├── AspireJavaScript.AppHost/           # .NET Aspire orchestrator
├── AspireJavaScript.MinimalApi/        # Main API service
│   ├── ApiServices/                    # Business logic services
│   │   ├── Interfaces/                 # Service interfaces
│   │   └── Services/                   # Service implementations
│   ├── Endpoints/                      # Minimal API endpoints
│   │   ├── Students/                   # Student-specific endpoints
│   │   └── Teachers/                   # Teacher-specific endpoints
│   ├── Validators/                     # FluentValidation validators
│   └── Mappings/                       # DTO mapping extensions
├── AspireJavaScript.Vite/              # React frontend with TypeScript
│   ├── src/components/                 # React components
│   │   ├── auth/                       # Authentication components
│   │   ├── dashboard/                  # Dashboard and management components
│   │   ├── layout/                     # Layout components
│   │   └── ui/                         # shadcn/ui components
│   ├── src/services/                   # API service clients
│   └── src/types/                      # Auto-generated TypeScript types
├── AspireJavaScript.ServiceDefaults/   # Common service configurations
├── ProjectAthena.Data/                 # Data layer
│   ├── Models/                         # Entity models
│   │   ├── Students/                   # Student entity
│   │   └── Teachers/                   # Teacher entity
│   └── Persistence/                    # DbContext and configurations
├── ProjectAthena.Dtos/                 # Data Transfer Objects with mappings
├── ProjectAthena.DbWorkerService/      # Database seeding and migration service
├── ProjectAthena.ReportGenerationLambda/ # AWS Lambda for report generation
└── ProjectAthena.Tests/                # Comprehensive integration tests
    ├── Authentication tests            # JWT and user role testing
    ├── Enrollment integration tests    # Advanced reporting system tests
    └── AWS S3 mock tests              # Report storage testing with mock client
```


**ProjectAthena** - Empowering Education Through Technology

*Production-ready educational management system built with modern .NET 9.0, React, and shadcn/ui*
