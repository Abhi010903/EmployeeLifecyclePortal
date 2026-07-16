// Authentication types
export interface User {
  id: string
  email: string
  role: 'Admin' | 'Manager' | 'HR' | 'Employee'
  name: string
}

export interface AuthResponse {
  token: string
  user: User
}

// Employee types
export interface Employee {
  id: string
  employeeCode: string
  firstName: string
  lastName: string
  email: string
  phoneNumber?: string
  status: 'Active' | 'Inactive' | 'Terminated'
  departmentId: string
  managerId?: string
  createdAtUtc: string
}

export interface EmployeeProfile extends Employee {
  departmentName?: string
  managerName?: string
  roles: Role[]
  timelineEventsCount: number
  documentsCount: number
  subordinatesCount: number
}

// Department types
export interface Department {
  id: string
  name: string
  description: string
  headOfDepartmentId?: string
  totalBudget: number
  averageSalary: number
}

// Role types
export interface Role {
  id: string
  name: string
  description: string
}

// Attendance types
export interface Attendance {
  id: string
  employeeId: string
  checkInTimeUtc: string
  checkOutTimeUtc?: string
  status: string
}

// Leave types
export interface LeaveRequest {
  id: string
  employeeId: string
  startDateUtc: string
  endDateUtc: string
  status: 'Pending' | 'Approved' | 'Rejected'
  reason: string
}

// Common response wrapper
export interface ApiResponse<T> {
  statusCode: number
  message: string
  data?: T
  errors?: string[]
}

export interface PaginatedResponse<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}
