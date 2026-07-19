import { apiClient } from './client'
import { SalaryStructureDto, PayslipDto } from '@/types'

export const payrollApi = {
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
    apiClient.post('/payroll/generate-payslip', data),
}
