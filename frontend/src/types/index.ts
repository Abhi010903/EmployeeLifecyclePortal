// Authentication types
export interface User {
  id: string
  email: string
  role: 'Admin' | 'Manager' | 'Team Lead' | 'TeamLead' | 'HR' | 'Employee' | string
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
  departmentName?: string
  managerId?: string
  managerName?: string
  teamLeadId?: string
  teamLeadName?: string
  roleId?: string
  roleName?: string
  roles?: Role[] | string[]
  createdAtUtc: string
}

export interface EmployeeProfile extends Employee {
  departmentName?: string
  managerName?: string
  teamLeadName?: string
  roles: Role[]
  timelineEventsCount: number
  documentsCount: number
  subordinatesCount: number
  lastModifiedAtUtc?: string
}

// Department types
export interface Department {
  id: string
  name: string
  description: string
  headOfDepartmentId?: string
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

// Role types
export interface Role {
  id: string
  name: string
  description: string
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

// Attendance types
export interface Attendance {
  id: string
  employeeId: string
  checkInTimeUtc: string
  checkOutTimeUtc?: string
  status: string
}

export interface AttendanceDto {
  id: string
  employeeId: string
  employeeName?: string
  checkInTimeUtc: string
  checkOutTimeUtc?: string
  status: string
  isApproved: boolean
  notes?: string
  hoursWorked: number
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

export interface LeaveTypeDto {
  id: string
  name: string
  daysPerYear: number
  isPaid: boolean
  description: string
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

export interface LeaveRequestDto {
  id: string
  employeeId: string
  employeeName?: string
  leaveTypeId: string
  leaveTypeName?: string
  startDateUtc: string
  endDateUtc: string
  daysRequested: number
  status: 'Pending' | 'ManagerApproved' | 'Approved' | 'Rejected'
  reason?: string
  approvedByUserId?: string
  approvedByName?: string
  managerApprovedByUserId?: string
  managerApprovedByName?: string
  managerApprovedAtUtc?: string
  finalApprovedByUserId?: string
  finalApprovedByName?: string
  finalApprovedAtUtc?: string
  rejectedByUserId?: string
  rejectedByName?: string
  rejectedAtUtc?: string
  rejectionReason?: string
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

export interface LeaveBalanceDto {
  id: string
  employeeId: string
  employeeName?: string
  leaveTypeId: string
  leaveTypeName?: string
  totalDays: number
  usedDays: number
  remainingDays: number
  year: number
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

// Tasks & Work management
export interface WorkTask {
  id: string
  title: string
  description: string
  employeeId: string
  employeeName?: string
  departmentId?: string
  departmentName?: string
  managerId?: string
  managerName?: string
  priority: 'Low' | 'Medium' | 'High' | 'Urgent' | string
  startDateUtc: string
  deadlineUtc: string
  status: 'Pending' | 'InProgress' | 'Blocked' | 'Completed' | 'Overdue' | string
  completionPercentage: number
  comments?: string
  completedAtUtc?: string
  createdAtUtc: string
}

export interface CreateWorkTaskDto {
  title: string
  description: string
  employeeId: string
  priority: string
  startDateUtc: string
  deadlineUtc: string
}

export interface UpdateWorkTaskStatusDto {
  completionPercentage: number
  status: string
  comments?: string
}

// Staffing Requests
export interface StaffingRequest {
  id: string
  departmentId: string
  departmentName?: string
  requestedByUserId: string
  requestedByName?: string
  currentHeadcount: number
  requiredCount: number
  reason: string
  status: 'Pending' | 'Approved' | 'Rejected' | string
  adminComments?: string
  createdAtUtc: string
  resolvedAtUtc?: string
}

export interface CreateStaffingRequestDto {
  departmentId: string
  requiredCount: number
  reason: string
}

export interface ResolveStaffingRequestDto {
  approve: boolean
  comments?: string
}

// Dashboard Summary
export interface DashboardSummary {
  totalEmployees: number
  activeEmployees: number
  inactiveEmployees: number
  totalDepartments: number
  totalRoles: number
  employeesOnLeave: number
  pendingLeaveRequests: number
  todayAttendance: number
  activeWorkSessions: number
  pendingStaffingRequests: number
  pendingTasks: number
  inProgressTasks: number
  overdueTasks: number
  completedTasks: number
  totalPayrollDue: number
  employeeTrend: number
  attendanceTrend: number
  leaveTrend: number
  payrollTrend: number
}

// Leave types (old, kept for backward compatibility)
export interface LeaveRequest {
  id: string
  employeeId: string
  startDateUtc: string
  endDateUtc: string
  status: 'Pending' | 'Approved' | 'Rejected'
  reason: string
}

// Payroll types
export interface SalaryStructureDto {
  id: string
  employeeId: string
  employeeName?: string
  baseSalary: number
  currency: string
  effectiveFromUtc: string
  effectiveToUtc?: string
  isActive: boolean
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

export interface SalaryComponentDto {
  id: string
  name: string
  type: 'Earning' | 'Deduction'
  amount: number
  isVariable: boolean
  salaryStructureId: string
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

export interface PayslipDto {
  id: string
  employeeId: string
  employeeName?: string
  month: number
  year: number
  baseSalary: number
  grossSalary?: number
  allowances: number
  deductions: number
  netSalary: number
  status: 'Generated' | 'Paid' | 'Cancelled' | 'Processed' | string
  generatedDateUtc: string
  paidDateUtc?: string
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

// Asset types
export interface AssetDto {
  id: string
  assetTag: string
  name: string
  category: string
  serialNumber?: string
  status: 'Available' | 'Assigned' | 'UnderMaintenance' | 'Retired' | string
  purchasePrice: number
  purchaseDateUtc: string
  currentAssigneeId?: string
  currentAssigneeName?: string
  assetName?: string
  assetCode?: string
  assetType?: string
  purchaseValue?: number
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

export interface AssetAssignmentDto {
  id: string
  assetId: string
  assetName?: string
  assetTag?: string
  assetType?: string
  employeeId: string
  employeeName?: string
  assignedDateUtc: string
  returnDateUtc?: string
  status: 'Active' | 'Returned' | string
  notes?: string
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

export interface AssetMaintenanceDto {
  id: string
  assetId: string
  assetName?: string
  maintenanceType: string
  description: string
  cost: number
  maintenanceDateUtc: string
  nextMaintenanceDateUtc?: string
  performedBy: string
  serviceProvider?: string
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

// Report types
export interface ReportDto {
  id: string
  title: string
  type: 'Attendance' | 'Leave' | 'Payroll' | 'Employees' | 'Performance' | 'Custom'
  parametersJson?: string
  dataJson?: string
  generatedByUserId: string
  generatedByName?: string
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

export interface ReportDataDto {
  data: Record<string, any>
  chartData: ReportChartDataDto[]
  summary?: ReportSummaryDto
}

export interface ReportChartDataDto {
  label: string
  type: 'bar' | 'line' | 'pie' | 'doughnut'
  labels: string[]
  values: number[]
  color?: string
}

export interface ReportSummaryDto {
  totalRecords: number
  keyMetric?: string
  totalAmount?: number
  averageAmount?: number
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

// Performance types
export interface PerformanceGoalDto {
  id: string
  employeeId: string
  employeeName?: string
  title: string
  description: string
  startDateUtc: string
  endDateUtc: string
  status: 'Active' | 'Completed'
  progressPercentage: number
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

export interface PerformanceReviewDto {
  id: string
  employeeId: string
  employeeName?: string
  reviewedByUserId?: string
  reviewedByName?: string
  year: number
  quarter: number
  rating: number
  comments: string
  status: 'Draft' | 'Submitted' | 'Approved'
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

export interface KPIDto {
  id: string
  employeeId: string
  employeeName?: string
  name: string
  target: number
  achieved: number
  year: number
  achievementPercentage: number
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

// Recruitment types
export interface JobPostingDto {
  id: string
  title: string
  description: string
  departmentId: string
  departmentName?: string
  status: 'Open' | 'Closed'
  postedDateUtc: string
  closedDateUtc?: string
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

export interface CandidateDto {
  id: string
  firstName: string
  lastName: string
  fullName?: string
  email: string
  phoneNumber?: string
  status: 'Applied' | 'Interview' | 'Rejected' | 'Hired'
  jobPostingId: string
  jobPostingTitle?: string
  resumePath?: string
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

export interface InterviewDto {
  id: string
  candidateId: string
  candidateName?: string
  scheduledDateUtc: string
  interviewerName: string
  status: 'Scheduled' | 'Completed' | 'Cancelled'
  rating?: number
  feedback?: string
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}

export interface JobOfferDto {
  id: string
  candidateId: string
  candidateName?: string
  offeredSalary: number
  offerDateUtc: string
  expiryDateUtc?: string
  status: 'Pending' | 'Accepted' | 'Rejected'
  createdAtUtc: string
  createdBy?: string
  lastModifiedAtUtc?: string
  lastModifiedBy?: string
}
