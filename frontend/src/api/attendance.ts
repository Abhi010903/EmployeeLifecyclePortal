import { apiClient } from './client'
import {
  AttendanceDto,
  LeaveTypeDto,
  LeaveRequestDto,
  LeaveBalanceDto,
} from '@/types'

// Attendance API
export const attendanceApi = {
  checkIn: (data: { employeeId?: string; notes?: string }) =>
    apiClient.post('/attendance/check-in', data),

  checkOut: (params?: { attendanceId?: string; employeeId?: string } | string) => {
    const payload = typeof params === 'string' ? { attendanceId: params } : (params || {})
    return apiClient.post('/attendance/check-out', payload)
  },

  getAll: () =>
    apiClient.get<AttendanceDto[]>('/attendance'),

  getToday: () =>
    apiClient.get<AttendanceDto[]>('/attendance/today'),

  getMyToday: () =>
    apiClient.get<AttendanceDto[]>('/attendance/my/today'),

  getMyAttendance: () =>
    apiClient.get<AttendanceDto[]>('/attendance/my'),

  getByEmployee: (employeeId: string) =>
    apiClient.get<AttendanceDto[]>(`/attendance/employee/${employeeId}`),
}

// Leave API
export const leaveApi = {
  apply: (data: {
    employeeId?: string
    leaveTypeId: string
    startDate: string
    endDate: string
    reason?: string
  }) =>
    apiClient.post('/attendance/leave/apply', data),

  approve: (data: {
    leaveRequestId: string
    approvedByUserId: string
  }) =>
    apiClient.post('/attendance/leave/approve', data),

  reject: (leaveRequestId: string) =>
    apiClient.post('/attendance/leave/reject', { leaveRequestId }),

  getTypes: () =>
    apiClient.get<LeaveTypeDto[]>('/attendance/leave/types'),

  getBalance: (employeeId: string) =>
    apiClient.get<LeaveBalanceDto[]>(`/attendance/leave/balance/${employeeId}`),

  getMyBalance: () =>
    apiClient.get<LeaveBalanceDto[]>('/attendance/leave/balance/my'),

  getRequests: (employeeId?: string, status?: string) => {
    const params = new URLSearchParams()
    if (employeeId) params.append('employeeId', employeeId)
    if (status) params.append('status', status)
    return apiClient.get<LeaveRequestDto[]>(
      `/attendance/leave/requests${params.toString() ? '?' + params.toString() : ''}`
    )
  },

  getMyRequests: (status?: string) => {
    const params = new URLSearchParams()
    if (status) params.append('status', status)
    return apiClient.get<LeaveRequestDto[]>(
      `/attendance/leave/requests/my${params.toString() ? '?' + params.toString() : ''}`
    )
  },
}
