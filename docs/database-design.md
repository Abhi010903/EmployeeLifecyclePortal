# Database Design

## Version 1 Core Tables

---

## Departments

### Columns

* Id (GUID)
* Name
* Description
* CreatedAtUtc
* CreatedBy
* LastModifiedAtUtc
* LastModifiedBy

### Relationships

* One Department → Many Employees

---

## Roles

### Columns

* Id (GUID)
* Name
* Description
* CreatedAtUtc
* CreatedBy
* LastModifiedAtUtc
* LastModifiedBy

### Relationships

* One Role → Many EmployeeRoles

---

## Employees

### Columns

* Id (GUID)
* EmployeeCode
* FirstName
* LastName
* Email
* Status
* DepartmentId
* CreatedAtUtc
* CreatedBy
* LastModifiedAtUtc
* LastModifiedBy

### Relationships

* One Employee → One Department
* One Employee → Many EmployeeRoles

### Business Rules

* Employee must belong to exactly one department.
* Employee can have a maximum of two active roles.

---

## EmployeeRoles

### Columns

* Id (GUID)
* EmployeeId
* RoleId
* CreatedAtUtc
* CreatedBy
* LastModifiedAtUtc
* LastModifiedBy

### Relationships

* Many EmployeeRoles → One Employee
* Many EmployeeRoles → One Role

### Business Rules

* Prevent duplicate role assignments.
* Maximum two role assignments per employee.

---

# Entity Relationships

Departments
│
└── Employees

Employees
│
└── EmployeeRoles

Roles
│
└── EmployeeRoles

---

# Version 1 Database Scope

Tables:

1. Departments
2. Roles
3. Employees
4. EmployeeRoles

Future Tables:

* LeaveRequests
* Candidates
* Interviews
* PerformanceReviews
* OnboardingTasks
* OffboardingTasks
* AuditLogs
