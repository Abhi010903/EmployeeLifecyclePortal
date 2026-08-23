# Employee Lifecycle Portal

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18.3-61DAFB?logo=react&logoColor=black)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.2-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Vite](https://img.shields.io/badge/Vite-5.0-646CFF?logo=vite&logoColor=white)](https://vitejs.dev/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind-3.4-38B2AC?logo=tailwind-css&logoColor=white)](https://tailwindcss.com/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC292B?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server/)

> **A modern, role-aware Employee Lifecycle Management System with database-backed HR workflows, multi-stage approvals, and employee self-service capabilities.**

---

## 1. Project Overview

### Problem Statement
Managing employee lifecycles across recruitment, onboarding, daily attendance, leave workflows, payroll calculations, task allocations, and asset assignments often suffers from disconnected tools, manual spreadsheets, and unauthorized cross-tenant data exposure.

### Purpose & Target Audience
The **Employee Lifecycle Portal** provides a centralized, database-backed platform designed for:
- **HR & System Administrators**: Manage company-wide employee records, organizational departments, system roles, payroll structures, company assets, and institutional settings.
- **Managers & Team Leads**: Supervise direct reports, monitor daily shifts, review team tasks, and conduct multi-stage leave request reviews.
- **Employees**: Access a self-service workspace to record daily check-ins/outs (IST), review personal leave quotas, submit time-off applications, view itemized monthly payslips, track personal tasks, and inspect assigned assets.

### Role-Based Access Control (RBAC) & Data Isolation
Security is enforced at both the UI layer and the backend API controller/handler level. Employee self-service endpoints automatically derive employee identity from validated JWT security claims, preventing Insecure Direct Object Reference (IDOR) vulnerabilities.

---

## 2. Key Features

### Implemented Modules

| Module | Features & Capabilities | Status |
|--------|--------------------------|:------:|
| **Authentication & Authorization** | JWT bearer authentication, BCrypt password hashing, role claims (`Admin`, `HR`, `Manager`, `Team Lead`, `Employee`), automatic user-to-employee claim mapping. | ✅ Implemented |
| **Employee Management** | Employee directory with search, filtering, employee code generation, manager hierarchy assignment, department assignment, and document attachments. | ✅ Implemented |
| **Department Management** | Department directory, cost center tracking, budget overview, department heads, and employee counts. | ✅ Implemented |
| **Role & Permission Management** | System role definitions, role assignments, and permission boundaries. | ✅ Implemented |
| **Attendance Tracking** | Real-time Check-In and Check-Out, daily presence status (Present, Late, Absent, Half Day), working hours calculation, and IST (Asia/Kolkata) timestamp formatting. | ✅ Implemented |
| **Leave Management** | Master leave types (Annual, Sick, Casual, Maternity/Paternity), balance tracking (used vs remaining), multi-stage approval workflow (`Pending` $\rightarrow$ `ManagerApproved` $\rightarrow$ `Approved` / `Rejected`). | ✅ Implemented |
| **Payroll & Payslips** | Base salary structure assignment, automated earnings/deductions breakdown (HRA, DA, PF, Professional Tax), monthly payslip generation, and detailed printable payslip view. | ✅ Implemented |
| **Reimbursements** | Expense claim submissions, category classification, claim reason tracking, and status monitoring. | ✅ Implemented |
| **Performance Management** | KPI definitions, quarterly performance review scoring, goals tracking, and employee review notes. | ✅ Implemented |
| **Recruitment & Staffing** | Job opening postings, candidate pipeline tracking, interview scheduling, offer letters, and department staffing requests. | ✅ Implemented |
| **Work Tasks** | Task creation, priority levels (Low, Medium, High, Urgent), progress tracking (0-100%), deadline management, and assignment to employees. | ✅ Implemented |
| **Asset Management** | Hardware/software asset catalog, serial numbers, warranty tracking, employee asset assignments, and maintenance logs. | ✅ Implemented |
| **Reports & Analytics** | Department-wise headcount distribution, salary distribution charts, attendance summaries, and export tools. | ✅ Implemented |
| **Dashboard** | Dynamic role-specific dashboards: executive statistics for Admins/Managers; personal attendance, leave cards, active tasks, and recent payslips for Employees. | ✅ Implemented |
| **System Settings** | Company legal profile, Indian public holiday calendar, organizational work shifts, and system configurations. | ✅ Implemented |

---

## 3. Role & Authority Model

The application strictly differentiates permissions between administrative roles and employee self-service.

```
                    ┌──────────────────────────────────────────────┐
                    │               JWT Bearer Token               │
                    └──────────────────────┬───────────────────────┘
                                           │
                 ┌─────────────────────────┼─────────────────────────┐
                 ▼                         ▼                         ▼
         ┌───────────────┐         ┌───────────────┐         ┌───────────────┐
         │     Admin     │         │    Manager    │         │   Employee    │
         └───────┬───────┘         └───────┬───────┘         └───────┬───────┘
                 │                         │                         │
     Full system governance;    Supervises team reports;  Self-service portal only;
     Payroll & Master Data      Multi-stage approvals     Strict user claim isolation
```

### 1. Admin
- **Scope**: Organization-wide administrative authority.
- **Capabilities**:
  - Full CRUD operations on Employees, Departments, Roles, and System Settings.
  - Final approval on multi-stage leave requests.
  - Payroll structure initialization and organization-wide payslip generation.
  - Access to company analytics, audit logs, and recruitment pipelines.

### 2. Manager / Team Lead
- **Scope**: Supervisory authority over assigned teams and departments.
- **Capabilities**:
  - View employee directory and department statistics.
  - Review team attendance logs and daily presence.
  - Perform **Manager Stage Approval** (`ManagerApproved`) on subordinate leave requests.
  - Assign tasks and conduct employee performance reviews.

### 3. Employee (Self-Service)
- **Scope**: Strictly isolated to the authenticated user's own profile and records.
- **Capabilities**:
  - View personal employee profile and employment timeline.
  - Record daily check-in / check-out sessions.
  - View personal leave balances and submit leave requests.
  - View personal itemized payslips and salary breakdown.
  - Track personal assigned work tasks and submit expense reimbursements.
- **Backend Enforcement**:
  - Employee endpoints query data filtered by `EmployeeId` derived from authenticated claims.
  - Non-administrative users cannot query or mutate records belonging to other employees.

---

## 4. Architecture

The backend is built following **Clean Architecture** and **CQRS (Command Query Responsibility Segregation)** with **MediatR**.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       Frontend: React 18 + TypeScript                       │
│           (Vite SPA, Tailwind CSS, Zustand Store, Axios Interceptors)        │
└──────────────────────────────────────┬──────────────────────────────────────┘
                                       │ HTTP REST / JSON (Port 5086)
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                    API Layer: ASP.NET Core 10 Web API                       │
│    (Controllers, JWT Middleware, Exception Handler, Correlation ID, CORS)    │
└──────────────────────────────────────┬──────────────────────────────────────┘
                                       │ MediatR Commands / Queries
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                    Application Layer: Core Business Logic                   │
│        (CQRS Handlers, DTOs, FluentValidation, Interfaces, Exceptions)       │
└───────────────────┬─────────────────────────────────────┬───────────────────┘
                    │                                     │
                    ▼                                     ▼
┌──────────────────────────────────────┐ ┌────────────────────────────────────┐
│      Domain Layer: Core Entities     │ │  Infrastructure: Data Persistence  │
│  (BaseEntity, AuditableEntity, Enums)│ │ (EF Core 10, DbContext, Seeder)    │
└──────────────────────────────────────┘ └─────────────────┬──────────────────┘
                                                           │
                                                           ▼
                                         ┌────────────────────────────────────┐
                                         │ SQL Server / MSSQLLocalDB Database │
                                         └────────────────────────────────────┘
```

### Layer Responsibilities

1. **Presentation (Frontend)**: React 18 single-page application built with Vite, TypeScript, and Tailwind CSS. Manages local authentication state, JWT storage, routing, and user interface components.
2. **API Layer (`EmployeeLifecyclePortal.Api`)**: Exposes RESTful endpoints, enforces JWT authentication, handles global exception serialization, registers Serilog structured logging, and coordinates dependency injection.
3. **Application Layer (`EmployeeLifecyclePortal.Application`)**: Encapsulates business logic into MediatR Command and Query handlers, DTO definitions, and validation rules.
4. **Domain Layer (`EmployeeLifecyclePortal.Domain`)**: Defines domain entities (`Employee`, `Department`, `Attendance`, `LeaveRequest`, `Payslip`, `ApplicationUser`, etc.) and business rules.
5. **Infrastructure Layer (`EmployeeLifecyclePortal.Infrastructure`)**: Manages Entity Framework Core `ApplicationDbContext`, SQL Server mapping, database seeding, and JWT token generation.
6. **Shared Layer (`EmployeeLifecyclePortal.Shared`)**: Common helpers, constants, and shared utility structures.

---

## 5. Technology Stack

### Backend
- **Runtime / Framework**: .NET 10.0 (`net10.0`), C# 12
- **Web Framework**: ASP.NET Core 10 Web API
- **ORM**: Entity Framework Core 10.0.8 (`Microsoft.EntityFrameworkCore.SqlServer`)
- **CQRS / Mediator**: MediatR 14.1.0
- **Validation**: FluentValidation 12.1.1
- **Security & Authentication**: `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.9, `BCrypt.Net-Next` 4.2.0
- **Logging**: Serilog 10.0.0 (`Serilog.AspNetCore`, `Serilog.Sinks.Console`, `Serilog.Sinks.File`)
- **API Documentation**: Swashbuckle / OpenAPI 10.2.1
- **Health Checks**: `AspNetCore.HealthChecks.SqlServer` 9.0.0

### Frontend
- **Library**: React 18.3.1
- **Language**: TypeScript 5.2.2
- **Build Tool**: Vite 5.0.8
- **Styling**: Tailwind CSS 3.4.1, PostCSS, Autoprefixer
- **Routing**: React Router DOM 6.20.0
- **State Management**: Zustand 4.4.1
- **HTTP Client**: Axios 1.6.2 (with request/response interceptors)
- **Forms & Validation**: React Hook Form 7.48.0, Zod 3.22.4
- **Charts & Visualizations**: Recharts 2.10.3
- **Icons**: Lucide React 0.294.0
- **Notifications**: React Hot Toast 2.4.1

### Database
- **Database Engine**: Microsoft SQL Server 2022 / SQL Server LocalDB (`MSSQLLocalDB`)

---

## 6. Project Structure

```
EmployeeLifecyclePortal/
├── EmployeeLifecyclePortal.slnx              # Visual Studio solution file
├── Dockerfile                                # Production multi-stage Docker build for API
├── docker-compose.yml                        # Docker Compose definition (SQL Server + API + Frontend)
│
├── EmployeeLifecyclePortal.Api/              # ASP.NET Core 10 Web API project
│   ├── Controllers/                          # REST API controllers (Auth, Attendance, Payroll, etc.)
│   ├── Middleware/                           # Global exception handling, correlation ID, request logging
│   ├── Services/                             # CurrentUserService, identity claims resolver
│   ├── appsettings.json                      # Base configuration
│   ├── appsettings.Development.json          # Development configuration (JWT secret, Serilog)
│   └── Program.cs                            # Application startup, DI container, and middleware pipeline
│
├── EmployeeLifecyclePortal.Application/      # CQRS Application layer
│   ├── Authorization/                        # Permission definitions & policy constants
│   ├── Commands/                             # Write operations (Auth, Attendance, Leave, Employees)
│   ├── Queries/                              # Read operations (Dashboard, Attendance, Payroll, Leave)
│   ├── DTOs/                                 # Data Transfer Objects
│   ├── Exceptions/                           # Custom domain exceptions (NotFoundException, ForbiddenException)
│   └── Interfaces/                           # Application contracts (IApplicationDbContext, ICurrentUserService)
│
├── EmployeeLifecyclePortal.Domain/           # Enterprise Domain layer
│   ├── Common/                               # BaseEntity, AuditableEntity
│   ├── Entities/                             # Employee, Attendance, Leave, Payroll, Asset, Recruitment entities
│   └── Enums/                                # Domain enumerations
│
├── EmployeeLifecyclePortal.Infrastructure/   # Data access & external services
│   ├── Persistence/                          # ApplicationDbContext, entity type configurations
│   │   └── DatabaseSeeder.cs                 # Schema initialization and seed data generator
│   └── Security/                             # JwtTokenService, password hashing
│
├── EmployeeLifecyclePortal.Shared/           # Cross-cutting utility library
│
├── EmployeeLifecyclePortal.Tests/            # Unit test suite (xUnit, FluentAssertions, Moq)
│   └── Domain/                               # Core domain entity tests
│
├── frontend/                                 # React + TypeScript single-page application
│   ├── src/
│   │   ├── api/                              # Axios API client modules (auth, attendance, payroll, employees)
│   │   ├── components/                       # UI components (Layout, Header, Sidebar, Modal, Badge, Card)
│   │   ├── pages/                            # Feature pages (Dashboard, Leave, Attendance, Payroll, etc.)
│   │   ├── store/                            # Zustand global authentication store
│   │   ├── types/                            # TypeScript interfaces and DTO typings
│   │   └── utils/                            # Date formatting (IST), route guards, export helpers
│   ├── index.html                            # Frontend entry HTML
│   ├── package.json                          # Node dependencies and scripts
│   ├── tailwind.config.ts                    # Tailwind CSS configuration
│   └── vite.config.ts                        # Vite configuration and proxy rules
│
└── README.md                                 # Project documentation
```

---

## 7. Prerequisites

Before setting up the project on a fresh machine, ensure the following tools are installed:

1. **Git**: Version 2.30 or higher ([Download](https://git-scm.com/))
2. **.NET 10.0 SDK**: Required for compiling and running the backend ([Download](https://dotnet.microsoft.com/download/dotnet/10.0))
   - Verify installation: `dotnet --version` (should output `10.0.xxx`)
3. **Node.js**: Version 18.x or 20.x LTS ([Download](https://nodejs.org/))
   - Verify installation: `node -v` and `npm -v`
4. **SQL Server**:
   - **Windows**: Microsoft SQL Server LocalDB (comes standard with Visual Studio / SSDT) or SQL Server 2019/2022 Express.
   - **macOS / Linux**: SQL Server running in Docker or an accessible remote SQL Server instance.

---

## 8. Clone the Project

Open PowerShell or your preferred terminal and clone the repository:

```powershell
git clone https://github.com/Abhi010903/EmployeeLifecyclePortal.git
cd EmployeeLifecyclePortal
```

---

## 9. Configuration

### Backend Configuration (`appsettings.json` / `appsettings.Development.json`)

The backend configuration is managed via `EmployeeLifecyclePortal.Api/appsettings.json` and environment variables.

#### Connection String
By default, the application connects to local SQL Server LocalDB:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=EmployeeLifecyclePortalDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```
*If you are using SQL Server Express, update the connection string to:*
`Server=.\\SQLEXPRESS;Database=EmployeeLifecyclePortalDb;Trusted_Connection=True;TrustServerCertificate=True`

#### JWT Authentication Secret
The application requires a secure JWT signing key (minimum 32 characters). For local development, this is pre-configured in `EmployeeLifecyclePortal.Api/appsettings.Development.json` or can be set via environment variable:

```powershell
# Optional: Override via environment variable
$env:JWT_SECRET="YourSuperSecretDevelopmentKeyAtLeast32BytesLong!"
```

> [!NOTE]
> Never commit production secrets to version control. In production environments, provide `ConnectionStrings__DefaultConnection` and `JWT_SECRET` via secure environment variables or a key vault.

### Frontend Configuration (`frontend/.env`)

By default, the Vite development server proxies requests to `http://localhost:5086`. To explicitly set the backend URL:

```powershell
cd frontend
# Create a local .env file if overriding the default API URL
Set-Content -Path .env -Value "VITE_API_URL=http://localhost:5086/api"
```

---

## 10. Database Setup & Initialization

The project features an **automatic schema synchronization and seeding mechanism**:

1. **Automatic Initialization**: On backend startup, `DatabaseSeeder.cs` automatically:
   - Verifies whether the `EmployeeLifecyclePortalDb` database exists.
   - Creates the database and all 25+ relational entity tables if missing.
   - Automatically synchronizes schema columns across entity model updates.
   - Seeds initial administrative roles, departments, leave types, shifts, Indian holiday calendars, and demo accounts.
2. **No Manual SQL Script Required**: You do **not** need to manually execute SQL creation scripts before launching the application for the first time.

### Starting SQL Server LocalDB (Windows)
Ensure LocalDB is running:
```powershell
sqllocaldb start MSSQLLocalDB
```

---

## 11. Run the Backend

Navigate to the project root and launch the ASP.NET Core API:

```powershell
# From the repository root
dotnet restore
dotnet build
dotnet run --project EmployeeLifecyclePortal.Api
```

### Verification
Once started, the backend outputs:
```
[INF] Starting EmployeeLifecyclePortal API...
[INF] Database initialized and seed data successfully verified.
[INF] Now listening on: http://localhost:5086
```
- **API URL**: `http://localhost:5086`
- **Swagger Documentation**: `http://localhost:5086/swagger`
- **Health Check**: `http://localhost:5086/health`

---

## 12. Run the Frontend

Open a second terminal window, navigate to the `frontend` folder, and start the development server:

```powershell
cd frontend
npm install
npm run dev
```

### Verification
The Vite dev server will start instantly:
```
  VITE v5.0.8  ready in 420 ms

  ➜  Local:   http://localhost:3000/
  ➜  Network: use --host to expose
```
- **Frontend Application URL**: `http://localhost:3000`

---

## 13. Demo / Local Development Credentials

> [!IMPORTANT]
> **Non-Production Only**: The following credentials are automatically initialized by `DatabaseSeeder.cs` strictly for local testing and demonstration.

| Role | Username | Email | Password | Primary Capabilities |
|------|----------|-------|----------|----------------------|
| **Admin** | System Admin | `admin@example.com` | `Admin@123456` | Full organization management, payroll setup, final leave approvals, system settings. |
| **Manager** | Vivek Singh | `manager@example.com` | `Manager@123456` | Team supervision, attendance review, multi-stage leave approvals. |
| **Employee** | Rahul Sharma | `employee@example.com` | `Employee@123456` | Self-service check-in/out, leave applications, personal payslips, assigned tasks. |

---

## 14. First Login Walkthrough

1. Open your browser and navigate to **`http://localhost:3000`**.
2. You will be greeted by the **Employee Lifecycle Portal** login page.
3. Sign in as **Admin** (`admin@example.com` / `Admin@123456`):
   - Review executive KPIs (Total Employees, Active Departments, Present Today, Pending Approvals).
   - Navigate to **Employees** to view the staff directory.
   - Navigate to **Payroll** to inspect salary structures and monthly payslip generation.
4. Log out and sign in as **Employee** (`employee@example.com` / `Employee@123456`):
   - The dashboard switches to **Employee Self-Service** mode.
   - Go to **Attendance** and click **Check In** to record an IST timestamp session.
   - Go to **Leave** to review your annual/sick leave balances and submit a new leave request.
   - Go to **Payroll** to review your personal itemized payslip breakdown.

---

## 15. Manual Verification Checklist

Use this checklist to verify a complete local deployment:

### 1. Administrative Workflow (Admin)
- [ ] Log in with `admin@example.com`.
- [ ] View the **Dashboard** metrics and analytics widgets.
- [ ] Navigate to **Employees**; verify staff list and employee profile modal.
- [ ] Navigate to **Departments**; verify budget and employee count summaries.
- [ ] Navigate to **Roles**; verify system permissions.
- [ ] Navigate to **Payroll**; view salary components and generated payslips.
- [ ] Navigate to **Settings**; verify Company Profile and Holiday Calendar.

### 2. Supervisory Workflow (Manager)
- [ ] Log in with `manager@example.com`.
- [ ] Navigate to **Leave**; verify pending leave requests from employees.
- [ ] Click **Approve** on a pending leave request; verify status changes to `ManagerApproved`.

### 3. Employee Self-Service Workflow (Employee)
- [ ] Log in with `employee@example.com`.
- [ ] Verify that administrative settings and role management are hidden.
- [ ] Navigate to **Attendance**; test the **Check In** / **Check Out** workflow.
- [ ] Navigate to **Leave**; verify personal leave cards (Annual, Sick, Casual, Maternity/Paternity).
- [ ] Click **Request Leave**, select a leave type, pick dates, enter reason, and submit.
- [ ] Verify the newly created request appears in your Leave History with status `Pending`.
- [ ] Navigate to **Payroll**; click on a payslip to open the detailed breakdown modal.

### 4. Security & Data Isolation
- [ ] Verify that an Employee cannot view or edit another employee's salary or payslips.
- [ ] Verify that Employee leave balance cards only display the authenticated user's data.
- [ ] Attempting to access unauthorized API endpoints returns HTTP `401 Unauthorized` or `403 Forbidden`.

---

## 16. Build Verification

To verify that the entire codebase compiles cleanly:

### Backend Build
```powershell
dotnet build
```
*Expected output*: `Build succeeded. 0 Warning(s), 0 Error(s)`

### Frontend Production Build
```powershell
cd frontend
npm run build
```
*Expected output*: `tsc && vite build` completes with `✓ built in X.XXs` and zero TypeScript errors.

---

## 17. Testing

The solution includes an automated testing project (`EmployeeLifecyclePortal.Tests`) utilizing **xUnit**, **Moq**, and **FluentAssertions**:

```powershell
# Run the test suite
dotnet test
```

The test suite covers:
- Core `Employee` domain entity creation and invariant checks.
- Manager assignment and self-referencing validation rules.
- Status transition logic (Active, Terminated, OnLeave).

---

## 18. Docker Support (Optional)

A multi-container setup is available via `docker-compose.yml` for containerized environments:

### Prerequisites
- Docker Desktop with Compose V2.

### Launching the Stack
```powershell
# Set environment variables for database password and JWT secret
$env:MSSQL_SA_PASSWORD="YourStrong(!)Password2026"
$env:JWT_SECRET="YourSuperSecretContainerKey32BytesLong!"

# Start all services
docker compose up -d
```

### Containers Created
- **`mssql`**: SQL Server 2022 Linux Container (Port `1433`).
- **`api`**: ASP.NET Core 10 Web API (Port `5000`).
- **`frontend`**: React Vite Development Container (Port `3000`).

---

## 19. Security Notes

- **JWT Token Validation**: Stateless JWT bearer tokens with standard claims (`NameIdentifier`, `Email`, `Role`, `EmployeeId`). Tokens expire after 8 hours.
- **Password Protection**: Passwords are saved as one-way cryptographic hashes using BCrypt with unique work factors and salts.
- **Parameterized Queries**: All database operations use Entity Framework Core LINQ queries, preventing SQL injection.
- **Data Protection & Correlation**: Cross-cutting `CorrelationIdMiddleware` attaches unique GUID correlation tokens to every request for audit tracing.
- **CORS Policy**: Configured to restrict origin access to authorized frontend endpoints during execution.

---

## 20. Development Workflow

Follow this standard process when contributing to the portal:

1. **Pull Latest Changes**: `git pull origin main`
2. **Build Backend**: `dotnet build`
3. **Run API**: `dotnet run --project EmployeeLifecyclePortal.Api`
4. **Run Frontend**: `cd frontend && npm run dev`
5. **Implement Changes**: Add CQRS commands/queries in Application layer and UI components in Frontend.
6. **Verify Compilation**: Execute `dotnet build` and `npm run build` to ensure zero compilation or type errors.
7. **Run Tests**: Execute `dotnet test`.
8. **Format & Clean**: Ensure all file headers, imports, and indentation conform to repository standards.

---

## 21. Troubleshooting

### 1. Backend Fails to Start: "JWT signing key is not configured"
- **Cause**: The application requires a JWT key with at least 256 bits (32 characters).
- **Fix**: Ensure `EmployeeLifecyclePortal.Api/appsettings.Development.json` has `Jwt:Secret` configured, or set the environment variable `$env:JWT_SECRET="YourSecretKeyOfAtLeast32CharactersHere!"`.

### 2. Database Connection Error: "Cannot connect to (localdb)\MSSQLLocalDB"
- **Cause**: SQL Server LocalDB instance is not started.
- **Fix**: Run `sqllocaldb start MSSQLLocalDB` in PowerShell. If using SQL Server Express, update `DefaultConnection` in `appsettings.json` to point to `Server=.\\SQLEXPRESS;Database=EmployeeLifecyclePortalDb;Trusted_Connection=True;TrustServerCertificate=True`.

### 3. Port Conflict on Port 5086 or 3000
- **Cause**: Another process is running on the default ports.
- **Fix**:
  - Check active processes: `netstat -ano | findstr :5086`
  - Terminate the conflicting process: `taskkill /PID <PID> /F`

### 4. Frontend Shows "Failed to load leave data" or 401 Unauthorized
- **Cause**: Expired JWT session token stored in browser localStorage.
- **Fix**: Log out, clear browser cookies/localStorage, and log in again with fresh credentials.

### 5. Frontend Build / Dependency Errors
- **Cause**: Outdated `node_modules` cache.
- **Fix**:
  ```powershell
  cd frontend
  Remove-Item -Recurse -Force node_modules, package-lock.json
  npm install
  npm run build
  ```

---

## 22. Project Status

The Employee Lifecycle Portal is a **feature-complete enterprise demonstration system**. All core modules (Authentication, Employees, Departments, Roles, Attendance, Leave Management, Payroll, Performance, Recruitment, Tasks, Assets, and Settings) are fully connected to a live SQL Server database with CQRS handlers and responsive React interfaces.

---

## 23. Future Enhancements

The following optional enhancements are planned for future roadmap releases:
- **Biometric Hardware Integration**: Real-time webhook ingestion from fingerprint/facial recognition timeclock terminals.
- **External Identity Provider Integration**: Single Sign-On (SSO) support via OpenID Connect / SAML 2.0 (Azure AD, Okta, Google Workspace).
- **Automated Tax Filing & Direct Deposit**: Banking NACHA / payment gateway API integrations for automated salary disbursement.
- **AI-Powered Workforce Analytics**: Predictive employee turnover modeling and automated leave schedule optimization.

---

## 📄 License

Proprietary — Employee Lifecycle Portal  
© 2026 All Rights Reserved
