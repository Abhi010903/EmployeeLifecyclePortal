import { apiClient } from './client'
import { ReportDataDto } from '@/types'

export const reportsApi = {
  getEmployeeReport: (departmentId?: number, status?: string) => {
    const params = new URLSearchParams()
    if (departmentId) params.append('departmentId', departmentId.toString())
    if (status) params.append('status', status)
    return apiClient.get<ReportDataDto>(
      `/reports/employees?${params.toString()}`
    )
  },

  getAttendanceReport: (startDate: string, endDate: string, employeeId?: string) => {
    const params = new URLSearchParams()
    params.append('startDate', startDate)
    params.append('endDate', endDate)
    if (employeeId) params.append('employeeId', employeeId)
    return apiClient.get<ReportDataDto>(
      `/reports/attendance?${params.toString()}`
    )
  },

  getLeaveReport: (year?: number, employeeId?: string) => {
    const params = new URLSearchParams()
    if (year) params.append('year', year.toString())
    if (employeeId) params.append('employeeId', employeeId)
    return apiClient.get<ReportDataDto>(
      `/reports/leave?${params.toString()}`
    )
  },

  getPayrollReport: (month: number, year: number, employeeId?: string) => {
    const params = new URLSearchParams()
    params.append('month', month.toString())
    params.append('year', year.toString())
    if (employeeId) params.append('employeeId', employeeId)
    return apiClient.get<ReportDataDto>(
      `/reports/payroll?${params.toString()}`
    )
  },

  getDepartmentReport: (month?: number, year?: number) => {
    const params = new URLSearchParams()
    if (month) params.append('month', month.toString())
    if (year) params.append('year', year.toString())
    return apiClient.get<ReportDataDto>(
      `/reports/department?${params.toString()}`
    )
  },

  exportCsv: (data: Record<string, any>) =>
    apiClient.post('/reports/export/csv', { data: JSON.stringify(data) }),

  exportExcel: (data: Record<string, any>) =>
    apiClient.post('/reports/export/excel', { data: JSON.stringify(data) }),

  exportPdf: (data: Record<string, any>) =>
    apiClient.post('/reports/export/pdf', { data: JSON.stringify(data) }),
}
