import { apiClient } from './client'
import type { Employee, EmployeeProfile, PaginatedResponse } from '@/types'

export const employeesApi = {
  getAll: async (pageNumber = 1, pageSize = 10): Promise<PaginatedResponse<Employee>> => {
    const response = await apiClient.get(`/employees?pageNumber=${pageNumber}&pageSize=${pageSize}`)
    return response.data.data
  },

  getById: async (id: string): Promise<Employee> => {
    const response = await apiClient.get(`/employees/${id}`)
    return response.data.data
  },

  getProfile: async (id: string): Promise<EmployeeProfile> => {
    const response = await apiClient.get(`/employeeprofile/${id}`)
    return response.data.data
  },

  create: async (employee: Omit<Employee, 'id' | 'createdAtUtc'>) => {
    const response = await apiClient.post('/employees', employee)
    return response.data.data
  },

  update: async (id: string, employee: Partial<Employee>) => {
    const response = await apiClient.put(`/employees/${id}`, employee)
    return response.data.data
  },

  delete: async (id: string) => {
    await apiClient.delete(`/employees/${id}`)
  },

  getTimeline: async (id: string) => {
    const response = await apiClient.get(`/employeeprofile/${id}/timeline`)
    return response.data.data
  },
}
