# Employee Lifecycle Portal
# API Design Specification

Version: 1.0

---

# Purpose

This document defines the API architecture, REST standards, endpoint conventions, request/response formats, versioning strategy, pagination, filtering, error handling, and API lifecycle for the Employee Lifecycle Portal.

The objective is to ensure that every API remains consistent, maintainable, secure, and production-ready.

---

# API Architecture

Architecture Style

- RESTful API

Communication

- HTTPS

Data Format

- JSON

Authentication

- JWT Bearer Token

Authorization

- Policy Based Authorization

Documentation

- OpenAPI / Swagger

Versioning

- URL Versioning

Example

/api/v1/employees

---

# HTTP Methods

GET

Retrieve resources.

POST

Create new resources.

PUT

Replace existing resources.

PATCH

Partially update resources.

DELETE

Remove resources.

---

# HTTP Status Codes

200 OK

201 Created

202 Accepted

204 No Content

400 Bad Request

401 Unauthorized

403 Forbidden

404 Not Found

409 Conflict

422 Unprocessable Entity

429 Too Many Requests

500 Internal Server Error

503 Service Unavailable

---

# Endpoint Naming

Correct

/api/v1/employees

/api/v1/departments

/api/v1/roles

Incorrect

/api/getEmployee

/api/deleteDepartment

---

# Resource Design

Employee

GET /employees

GET /employees/{id}

POST /employees

PUT /employees/{id}

DELETE /employees/{id}

PATCH /employees/{id}/activate

PATCH /employees/{id}/deactivate

PATCH /employees/{id}/terminate

---

# Request Standards

Every request should:

- Validate input
- Authenticate user
- Authorize access
- Log request
- Use DTOs

---

# Response Standards

Every successful response should be consistent.

Example

{
    "statusCode": 200,
    "message": "Success",
    "data": {}
}

---

# Error Response

Example

{
    "statusCode":404,
    "message":"Employee not found.",
    "errors":[]
}

Validation Errors

{
    "statusCode":400,
    "message":"Validation Failed",
    "errors":[
        "Email is required.",
        "Password must contain one uppercase letter."
    ]
}

---

# Pagination

Query Parameters

?page=1&pageSize=20

Response

{
    "page":1,
    "pageSize":20,
    "totalRecords":530,
    "totalPages":27,
    "items":[]
}

---

# Filtering

Examples

GET /employees?department=IT

GET /employees?status=Active

GET /employees?role=Manager

---

# Sorting

GET /employees?sortBy=firstName

GET /employees?sortBy=joiningDate

GET /employees?sortBy=salary

---

# Searching

GET /employees?search=abhijit

Supports

- Employee Code

- Name

- Email

- Phone

---

# API Versioning

Current Version

v1

Future

v2

v3

Older versions remain supported according to deprecation policy.

---

# Authentication Flow

Client

↓

Login

↓

JWT

↓

Bearer Token

↓

API Requests

↓

Authorization

↓

Business Logic

---

# Authorization

Policies

Admin

Manager

Employee

Future

Permission Based Authorization

---

# DTO Rules

Never expose entities.

Always use DTOs.

Separate Request DTOs and Response DTOs.

---

# Validation

FluentValidation

Validation Pipeline

Business Validation

---

# Exception Handling

Global Middleware

Problem Details

Consistent JSON Responses

---

# Logging

Log

- Request
- Response
- Duration
- Correlation Id
- User
- Exception

---

# Performance

Async APIs

Pagination

Compression

Caching

Indexes

---

# Security

HTTPS

JWT

Rate Limiting

Input Validation

CORS

Security Headers

---

# Future APIs

GraphQL

gRPC

Public API

Webhook Support

Bulk APIs

File Upload APIs

AI APIs

---

# API Development Checklist

Before creating an endpoint ensure:

- DTO Created
- Validator Created
- Command/Query Created
- Handler Created
- Repository Updated
- Authorization Applied
- Logging Included
- Tests Written
- Swagger Verified


---

# Audit Trail API

## Purpose

The audit trail API provides complete visibility into all changes made to entities in the system.

Every create, update, and delete operation is automatically recorded with:
- Who performed the operation (user ID)
- When it happened (UTC timestamp)
- What changed (old/new values)
- Which properties were modified

Access is restricted to Admin users only for security and compliance.

## Endpoints

### 1. Get Audit Logs (Paginated)

**GET /api/auditlogs**

Retrieve paginated audit logs with optional filtering by entity type and/or operation.

Query Parameters:
- `entityType` (optional): Filter by entity type (e.g., "Employee", "Department", "Role")
- `operation` (optional): Filter by operation ("Created", "Updated", "Deleted")
- `pageNumber` (optional, default: 1): Page number for pagination (1-based)
- `pageSize` (optional, default: 50, max: 250): Number of items per page

Example Request:
```
GET /api/auditlogs?entityType=Employee&operation=Updated&pageNumber=1&pageSize=50
```

Response (200 OK):
```json
{
  "items": [
    {
      "id": "12345678-1234-1234-1234-123456789012",
      "operatedAtUtc": "2026-07-16T10:30:45.123Z",
      "operatedBy": "user-id-guid",
      "entityType": "Employee",
      "entityId": "emp-id-guid",
      "operation": "Updated",
      "oldValues": "{\"firstName\":\"John\",\"email\":\"john@old.com\"}",
      "newValues": "{\"firstName\":\"Jonathan\",\"email\":\"jonathan@new.com\"}",
      "changedColumns": "[\"firstName\",\"email\"]",
      "timeAgo": "5 minutes ago"
    },
    {
      "id": "12345678-1234-1234-1234-123456789013",
      "operatedAtUtc": "2026-07-16T10:25:00.000Z",
      "operatedBy": "user-id-guid",
      "entityType": "Department",
      "entityId": "dept-id-guid",
      "operation": "Created",
      "oldValues": null,
      "newValues": "{\"name\":\"Engineering\",\"description\":\"Engineering Department\"}",
      "changedColumns": "[\"name\",\"description\"]",
      "timeAgo": "10 minutes ago"
    }
  ],
  "totalCount": 156,
  "pageNumber": 1,
  "pageSize": 50,
  "totalPages": 4,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

Authorization: Requires Admin policy

### 2. Get Entity History

**GET /api/auditlogs/history/{entityType}/{entityId}**

Retrieve the complete modification history of a specific entity.

Shows all changes to one entity from creation to last modification, ordered chronologically from newest to oldest.

Path Parameters:
- `entityType` (required): Type of the entity (e.g., "Employee", "Department", "Role")
- `entityId` (required): Primary key of the entity (GUID as string)

Example Request:
```
GET /api/auditlogs/history/Employee/12345678-1234-1234-1234-123456789012
```

Response (200 OK):
```json
{
  "entityType": "Employee",
  "entityId": "12345678-1234-1234-1234-123456789012",
  "modifications": [
    {
      "id": "aud-id-1",
      "operatedAtUtc": "2026-07-16T10:30:45.123Z",
      "operatedBy": "admin-user-id",
      "entityType": "Employee",
      "entityId": "12345678-1234-1234-1234-123456789012",
      "operation": "Updated",
      "oldValues": "{\"phoneNumber\":null}",
      "newValues": "{\"phoneNumber\":\"+1-555-0100\"}",
      "changedColumns": "[\"phoneNumber\"]",
      "timeAgo": "2 hours ago"
    },
    {
      "id": "aud-id-2",
      "operatedAtUtc": "2026-07-16T08:15:00.000Z",
      "operatedBy": "hr-user-id",
      "entityType": "Employee",
      "entityId": "12345678-1234-1234-1234-123456789012",
      "operation": "Created",
      "oldValues": null,
      "newValues": "{\"employeeCode\":\"EMP001\",\"firstName\":\"John\",\"lastName\":\"Doe\",\"email\":\"john.doe@company.com\"}",
      "changedColumns": "[\"employeeCode\",\"firstName\",\"lastName\",\"email\"]",
      "timeAgo": "4 hours ago"
    }
  ],
  "totalModifications": 2,
  "createdAtUtc": "2026-07-16T08:15:00.000Z",
  "lastModifiedAtUtc": "2026-07-16T10:30:45.123Z"
}
```

Authorization: Requires Admin policy

## Audit Log Fields

| Field | Type | Description |
|-------|------|-------------|
| id | GUID | Unique identifier for the audit log entry |
| operatedAtUtc | DateTime | Timestamp when the operation occurred (UTC) |
| operatedBy | string | User ID who performed the operation |
| entityType | string | Type name of the modified entity |
| entityId | string | Primary key of the modified entity |
| operation | string | Type of operation: "Created", "Updated", or "Deleted" |
| oldValues | JSON | Previous values (null for Create operations) |
| newValues | JSON | New values (null for Delete operations) |
| changedColumns | JSON array | List of property names that were modified |
| timeAgo | string | Friendly elapsed time display |

## Data Storage

Audit logs are stored in the `AuditLogs` table with the following structure:

```sql
CREATE TABLE dbo.AuditLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    OperatedAtUtc DATETIME2 NOT NULL,
    OperatedBy NVARCHAR(256) NOT NULL,
    EntityType NVARCHAR(256) NOT NULL,
    EntityId NVARCHAR(256) NOT NULL,
    Operation NVARCHAR(50) NOT NULL,
    OldValues NVARCHAR(MAX) NULL,
    NewValues NVARCHAR(MAX) NULL,
    ChangedColumns NVARCHAR(MAX) NULL,
    
    -- Indexes for optimal query performance
    INDEX IX_EntityType (EntityType),
    INDEX IX_EntityId (EntityId),
    INDEX IX_EntityType_EntityId (EntityType, EntityId),
    INDEX IX_OperatedAtUtc (OperatedAtUtc),
    INDEX IX_OperatedBy (OperatedBy),
    INDEX IX_Operation (Operation)
);
```

## Implementation Details

### Automatic Capture

Audit logs are captured automatically in the `ApplicationDbContext.SaveChangesAsync()` method:

1. Before persisting changes, the `IAuditService` analyzes all tracked entities
2. For each auditable entity (inherits from `AuditableEntity`), it detects:
   - Added entities → "Created" operation
   - Modified entities → "Updated" operation (with property-level change tracking)
   - Deleted entities → "Deleted" operation
3. Audit metadata fields (CreatedAtUtc, CreatedBy, LastModifiedAtUtc, LastModifiedBy) are skipped to avoid recursive auditing
4. Old/new values and changed columns are JSON-serialized
5. Audit log entries are added to the change tracker and persisted

### Exclusions

The following are NOT audited:
- AuditLog entities themselves (prevents recursive auditing)
- Non-auditable entities (entities not inheriting from `AuditableEntity`)
- Unchanged entries
- Audit metadata properties (CreatedAtUtc, CreatedBy, LastModifiedAtUtc, LastModifiedBy)

### User Attribution

The user ID is captured from the current request context via `ICurrentUserService.GetCurrentUserId()`:
- For authenticated requests: Uses the authenticated user's ID from JWT claims
- For system operations: Uses "system" as the user ID
- For unauthenticated contexts: Defaults to "system"

## Compliance & Security

- **Access Control**: Audit logs are accessible only to Admin users
- **Immutability**: Audit logs themselves cannot be modified or deleted (read-only)
- **Data Retention**: Audit logs are retained indefinitely for compliance
- **Performance**: Indexes on EntityType, EntityId, OperatedAtUtc, and OperatedBy ensure fast queries
- **JSON Storage**: Old/new values are stored as JSON for flexibility and ease of comparison

## Future Enhancements

- Audit log retention policies
- Bulk audit log export (CSV/Excel)
- Audit log archival to separate storage
- Real-time audit event streaming
- Audit log analytics dashboard
- Role-based audit log visibility
- Sensitive data masking in audit logs
