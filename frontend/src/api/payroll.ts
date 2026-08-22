import { apiClient } from './client'
import {
  SalaryStructureDto,
  PayslipDto,
  PayrollSummaryDto,
  ReimbursementDto,
  CreateReimbursementDto,
} from '@/types'

export const payrollApi = {
  // Summary & Runs
  getSummary: (month?: number, year?: number) => {
    const params = new URLSearchParams()
    if (month) params.append('month', month.toString())
    if (year) params.append('year', year.toString())
    const query = params.toString() ? `?${params.toString()}` : ''
    return apiClient.get<PayrollSummaryDto>(`/payroll/summary${query}`)
  },

  processPayrollRun: (month: number, year: number) =>
    apiClient.post<PayrollSummaryDto>('/payroll/run', { month, year }),

  approvePayrollRun: (month: number, year: number) =>
    apiClient.post<{ success: boolean; message: string }>('/payroll/approve-run', { month, year }),

  // Salary Structure
  getSalaryStructure: (employeeId: string) =>
    apiClient.get<SalaryStructureDto>(`/payroll/salary-structure/${employeeId}`),

  updateSalaryStructure: (employeeId: string, data: { baseSalary: number }) =>
    apiClient.put(`/payroll/salary-structure/${employeeId}`, data),

  // Payslips
  getPayslips: (employeeId?: string) => {
    const url = employeeId ? `/payroll/payslips?employeeId=${employeeId}` : '/payroll/payslips'
    return apiClient.get<PayslipDto[]>(url)
  },

  getPayslipsByMonth: (month: number, year: number) =>
    apiClient.get<PayslipDto[]>(`/payroll/payslips/${year}/${month}`),

  generatePayslip: (data: {
    employeeId: string
    month: number
    year: number
  }) =>
    apiClient.post<PayslipDto>('/payroll/generate-payslip', data),

  // Reimbursements
  getReimbursements: (employeeId?: string, status?: string) => {
    const params = new URLSearchParams()
    if (employeeId) params.append('employeeId', employeeId)
    if (status && status !== 'All') params.append('status', status)
    const query = params.toString() ? `?${params.toString()}` : ''
    return apiClient.get<ReimbursementDto[]>(`/payroll/reimbursements${query}`)
  },

  createReimbursement: (data: CreateReimbursementDto) =>
    apiClient.post<ReimbursementDto>('/payroll/reimbursements', data),

  approveReimbursement: (id: string) =>
    apiClient.put<{ success: boolean; message: string }>(`/payroll/reimbursements/${id}/approve`, {}),

  rejectReimbursement: (id: string, reason?: string) =>
    apiClient.put<{ success: boolean; message: string }>(`/payroll/reimbursements/${id}/reject`, { reason }),
}
