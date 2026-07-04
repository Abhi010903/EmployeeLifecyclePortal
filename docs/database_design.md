# Employee Lifecycle Portal
# Enterprise Database Design Document

Version: 1.0

---

# 1. Purpose

This document defines the complete enterprise database architecture for the Employee Lifecycle Portal.

It serves as the authoritative reference for:

- Database schema
- Entity relationships
- Naming conventions
- Primary and foreign keys
- Indexing strategy
- Migration strategy
- Performance optimization
- Future scalability

Although development will be incremental, this document represents the **final target architecture** of the system.

---

# 2. Database Philosophy

The database is designed using the following principles:

- Third Normal Form (3NF)
- Referential Integrity
- Soft Deletes where appropriate
- Auditability
- High Performance
- Scalability
- Security
- Cloud Readiness

The design supports future migration to distributed architectures without changing the business model.

---

# 3. Database Technology

Engine

- SQL Server

ORM

- Entity Framework Core

Migration Tool

- EF Core Migrations

Naming Convention

- PascalCase for tables
- Singular entity classes
- Plural table names where appropriate
- Primary Key: Id (GUID)
- Foreign Key: EntityNameId

---

# 4. Database Domains

The complete HRMS database is divided into functional domains.

| Domain | Planned Tables |
|---------|---------------:|
| Identity & Security | 10 |
| Organization | 10 |
| Employee Lifecycle | 18 |
| Attendance | 10 |
| Leave Management | 10 |
| Payroll | 15 |
| Recruitment | 12 |
| Performance | 10 |
| Training | 8 |
| Assets | 8 |
| Documents | 8 |
| Notifications | 8 |
| Audit & Logging | 10 |
| Reporting & Analytics | 6 |
| System Configuration | 8 |

**Estimated Total Tables: 151**

---

# 5. Identity & Security Domain

Tables:

- ApplicationUsers
- Roles
- Permissions
- RolePermissions
- UserRoles
- RefreshTokens
- LoginHistory
- PasswordHistory
- PasswordResetTokens
- EmailVerificationTokens

Purpose:

Manage authentication, authorization, security policies, session management, and account lifecycle.

---

# 6. Organization Domain

Tables:

- Organizations
- Branches
- Departments
- Teams
- CostCenters
- JobTitles
- Grades
- Designations
- Locations
- ReportingHierarchy

Purpose:

Represent the organizational structure independently from employee records.

---

# 7. Employee Lifecycle Domain

Tables:

- Employees
- EmployeeProfiles
- EmployeeContacts
- EmergencyContacts
- EmployeeAddresses
- EmployeeEducation
- EmployeeExperience
- EmployeeSkills
- EmployeeCertifications
- EmployeeDocuments
- EmployeeRoles
- EmployeeDepartments
- EmployeeManagers
- EmployeeTransfers
- EmploymentHistory
- Promotions
- Resignations
- ExitInterviews

Purpose:

Maintain the complete professional history of every employee.

---

# 8. Attendance Domain

Tables:

- Attendance
- AttendanceLogs
- AttendanceCorrections
- WorkSchedules
- ShiftAssignments
- Shifts
- Holidays
- OvertimeRequests
- OvertimeApprovals
- AttendanceSummaries

Purpose:

Track attendance, work schedules, shifts, overtime, and attendance analytics.

---

# 9. Leave Management Domain

Tables:

- LeaveTypes
- LeavePolicies
- LeaveBalances
- LeaveRequests
- LeaveApprovals
- LeaveHistory
- LeaveAttachments
- HolidayCalendar
- CompOffRequests
- LeaveAccruals

Purpose:

Handle leave planning, balances, approvals, and organizational policies.

---

# 10. Payroll Domain

Tables:

- Payrolls
- SalaryStructures
- SalaryComponents
- PayrollPeriods
- Payslips
- Bonuses
- Incentives
- Deductions
- TaxDeclarations
- TaxCalculations
- Reimbursements
- Loans
- LoanRepayments
- PayrollAudits
- PayrollSettings

Purpose:

Support salary processing, taxation, reimbursements, and payroll reporting.

---

# 11. Recruitment Domain

Tables:

- JobOpenings
- Applicants
- Resumes
- InterviewRounds
- InterviewSchedules
- InterviewFeedback
- OfferLetters
- CandidateDocuments
- RecruitmentSources
- HiringPipeline
- RecruitmentCampaigns
- BackgroundVerification

Purpose:

Manage hiring from job posting to employee onboarding.

---

# 12. Performance Domain

Tables:

- Goals
- KPIs
- PerformanceReviews
- ReviewCycles
- ReviewFeedback
- PerformanceRatings
- PromotionsRecommendations
- CareerPlans
- Objectives
- PerformanceHistory

Purpose:

Track employee performance, growth, and review cycles.

---

# 13. Training Domain

Tables:

- Courses
- TrainingPrograms
- EmployeeTrainings
- Assessments
- AssessmentResults
- Certifications
- LearningPaths
- TrainingFeedback

Purpose:

Support employee learning and development.

---

# 14. Asset Management Domain

Tables:

- Assets
- AssetCategories
- AssetAssignments
- AssetReturns
- AssetMaintenance
- AssetVendors
- AssetAudits
- AssetRequests

Purpose:

Track company-owned assets assigned to employees.

---

# 15. Document Management Domain

Tables:

- Documents
- DocumentTypes
- EmployeeDocuments
- OrganizationDocuments
- PolicyDocuments
- Templates
- DigitalSignatures
- DocumentVersions

Purpose:

Store and manage organizational and employee documentation.

---

# 16. Notification Domain

Tables:

- Notifications
- NotificationTemplates
- NotificationQueue
- EmailLogs
- SmsLogs
- PushNotifications
- UserPreferences
- AnnouncementBoard

Purpose:

Support enterprise communication across multiple channels.

---

# 17. Audit & Logging Domain

Tables:

- AuditLogs
- ActivityLogs
- ErrorLogs
- RequestLogs
- LoginAudits
- ChangeHistory
- DataSnapshots
- ApiUsage
- BackgroundJobLogs
- SystemEvents

Purpose:

Provide complete traceability and compliance.

---

# 18. Reporting Domain

Tables:

- SavedReports
- DashboardWidgets
- AnalyticsSnapshots
- ExportHistory
- ReportSchedules
- BIConfigurations

Purpose:

Support dashboards, exports, scheduled reports, and business intelligence.

---

# 19. System Configuration Domain

Tables:

- SystemSettings
- FeatureFlags
- EmailSettings
- SecuritySettings
- PasswordPolicies
- NumberSequences
- Localization
- ApplicationConfigurations

Purpose:

Centralize configurable application behavior.

---

# 20. Global Table Standards

Every business table should include:

- Id
- CreatedAtUtc
- CreatedBy
- LastModifiedAtUtc
- LastModifiedBy

Where applicable:

- IsDeleted
- DeletedAtUtc
- DeletedBy

This enables auditing and soft deletion.

---

# 21. Primary Key Strategy

- GUID primary keys
- Sequential GUIDs where appropriate
- Clustered primary keys
- Surrogate keys preferred over composite keys

---

# 22. Foreign Key Strategy

- Explicit foreign keys
- Cascade deletes only when safe
- Restrict deletes for business-critical relationships

---

# 23. Indexing Strategy

Indexes should exist for:

- Email
- EmployeeCode
- DepartmentId
- RoleId
- OrganizationId
- BranchId
- CreatedAtUtc
- LastModifiedAtUtc

Composite indexes should be created for frequently queried combinations.

---

# 24. Migration Strategy

- One migration per sprint when schema changes.
- Never edit applied migrations.
- Use descriptive migration names.
- Keep migration history in source control.

---

# 25. Future Database Evolution

The schema is designed to support:

- Multi-Tenant SaaS
- Read Replicas
- Partitioning
- Distributed Caching
- Event Sourcing
- Data Warehousing
- Elasticsearch Integration
- AI Analytics
- Machine Learning Features
- Microservices
- Cross-Region Deployment

No redesign of the core schema should be required as these capabilities are introduced.