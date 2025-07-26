# ProjectAthena - Educational Management System

ProjectAthena is a comprehensive educational management system built with modern .NET technologies and React. It provides a robust platform for managing courses, students, teachers, and enrollments in educational institutions.

## 🏗️ Architecture Overview

ProjectAthena follows a modern microservices architecture using .NET Aspire for orchestration:

### System Components

- **🎯 ProjectAthena.AppHost**: .NET Aspire orchestrator managing all services
- **🌐 ProjectAthena.MinimalApi**: Main API service using ASP.NET Core Minimal APIs
- **💾 ProjectAthena.Data**: Entity Framework Core data layer with PostgreSQL
- **📋 ProjectAthena.Dtos**: Data Transfer Objects for API contracts
- **🔄 ProjectAthena.DbWorkerService**: Database seeding and migration service
- **⚛️ AspireJavaScript.Vite**: React frontend with TypeScript and Vite
- **⚙️ ProjectAthena.ServiceDefaults**: Common service configurations
- **🧪 ProjectAthena.Tests**: Comprehensive integration tests

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
| **Orchestration** | .NET Aspire with service discovery |
| **Database Admin** | pgAdmin with Docker integration |
| **Testing** | xUnit with comprehensive integration tests |

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

### **Security Implementation**
- **Authentication**: JWT Bearer tokens with ASP.NET Core Identity
- **Authorization**: Role-based access control (Admin, Teacher, Student)
- **Password Policy**: Enforced complexity requirements
- **Account Lockout**: Protection against brute force attacks

### **Database Design**
- **Soft Deletes**: Entities marked as inactive rather than physically deleted
- **Audit Trail**: CreatedAt/UpdatedAt timestamps on all entities
- **Referential Integrity**: Proper foreign key relationships
- **Unique Constraints**: Course codes, student numbers, employee numbers

## 🚀 Setup Instructions

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for PostgreSQL)
- [Node.js 20+](https://nodejs.org)
- **Optional**: [Visual Studio 2022 17.12+](https://visualstudio.microsoft.com/vs/)

### Local Development Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/KostaKing/ProjectAthena
   cd ProjectAthena
   ```

2. **Run the Application**

   **Option A: Visual Studio**
   - Open `ProjectAthena.sln`
   - Set `ProjectAthena.AppHost` as startup project
   - Press F5 to run

   **Option B: .NET CLI**
   ```bash
   cd AspireJavaScript.AppHost
   dotnet run
   ```

3. **Access the Application**
   - **.NET Aspire Dashboard**: Auto-launches in browser
   - **React Frontend**: Available through Aspire dashboard
   - **API Documentation**: Swagger UI available in development
   - **Database Admin**: pgAdmin available through Aspire dashboard

### Database Seeding

The `ProjectAthena.DbWorkerService` automatically:
- Creates and migrates the database
- Seeds initial data including:
  - Default admin user
  - Sample courses and instructors
  - Test students and enrollments

## 🎯 Key Features

### **Advanced Admin Dashboard**
- **Real-time Analytics**: Live dashboard with completion rates, average grades, and enrollment metrics
- **Student-Teacher Ratio**: Automatic calculation and optimization recommendations
- **Activity Monitoring**: Recent system activities with detailed timestamps
- **Performance Indicators**: Color-coded metrics for quick assessment
- **Interactive Charts**: Visual representations of course and enrollment data

### **Comprehensive Course Management**
- **Course Creation**: Full CRUD operations with enrollment limits and schedules
- **Instructor Assignment**: Link courses to qualified instructors
- **Capacity Tracking**: Real-time enrollment monitoring with visual indicators
- **Status Management**: Track active, upcoming, and completed courses
- **Search & Filtering**: Advanced filtering by status, enrollment level, instructor

### **Advanced Reporting System**
- **Multi-dimensional Reports**: Group by course, student, instructor, status, or date
- **Advanced Filtering**: Date ranges, grade ranges, status filters, course-specific reports
- **Real-time Validation**: Client-side validation with immediate feedback
- **Export Capabilities**: CSV export with PDF export (coming soon)
- **Performance Analytics**: Average grades, completion rates, enrollment trends
- **Visual Summaries**: Progress bars, status badges, and enrollment statistics

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
- Authentication workflows
- Course management operations
- Enrollment processes
- Database operations
- API endpoint functionality

Run tests with:
```bash
dotnet test
```

## 🛠️ Development Notes

### **Code Quality Standards**
- **Error Handling**: Consistent patterns across all endpoints
- **Logging**: Structured logging with appropriate levels
- **Validation**: FluentValidation for input validation
- **Documentation**: Comprehensive API documentation with Swagger

### **Performance Optimizations**
- **Minimal APIs**: Zero-allocation, high-performance endpoint routing
- **AsNoTracking**: Used for read-only operations
- **Selective Loading**: Include only necessary related data  
- **Pagination**: Implemented for large data sets
- **Caching**: Memory caching for frequently accessed data
- **TypeScript Generation**: Build-time type generation eliminates runtime validation overhead

### **Security Best Practices**
- **Input Validation**: All inputs validated and sanitized
- **SQL Injection**: Protected via Entity Framework parameterized queries
- **Token Security**: Secure JWT configuration with proper expiration
- **CORS**: Configured for development (restrict for production)

## 📁 Project Structure

```
ProjectAthena/
├── AspireJavaScript.AppHost/           # .NET Aspire orchestrator
├── AspireJavaScript.MinimalApi/        # Main API service
│   ├── ApiServices/                    # Business logic services
│   ├── Endpoints/                      # Minimal API endpoints
│   ├── Validators/                     # FluentValidation validators
│   └── Mappings/                       # DTO mapping extensions
├── AspireJavaScript.Vite/              # React frontend
│   ├── src/components/                 # React components
│   ├── src/services/                   # API service clients
│   └── src/types/                      # TypeScript definitions
├── ProjectAthena.Data/                 # Data layer
│   ├── Models/                         # Entity models
│   ├── Persistence/                    # DbContext
│   └── Migrations/                     # EF migrations
├── ProjectAthena.Dtos/                 # Data Transfer Objects
├── ProjectAthena.DbWorkerService/      # Database seeding service
└── ProjectAthena.Tests/                # Integration tests
```

---

**ProjectAthena** - Empowering Education Through Technology