import { apiClient } from './client'
import type { Employee, EmployeeProfile, PaginatedResponse } from '@/types'

export const employeesApi = {
  getAll: async (pageNumber = 1, pageSize = 10): Promise<PaginatedResponse<Employee>> => {
    const response = await apiClient.get(`/employees?pageNumber=${pageNumber}&pageSize=${pageSize}`)
    const data = response.data
    if (Array.isArray(data)) {
      return {
        items: data,
        totalCount: data.length,
        pageNumber,
        pageSize,
        totalPages: Math.ceil(data.length / pageSize) || 1,
      }
    }
    return data
  },

  getAllSimple: async (): Promise<Employee[]> => {
    const response = await apiClient.get('/employees')
    const data = response.data
    if (Array.isArray(data)) return data
    if (data && Array.isArray(data.items)) return data.items
    return []
  },

  getById: async (id: string): Promise<Employee> => {
    const response = await apiClient.get(`/employees/${id}`)
    return response.data
  },

  getProfile: async (id: string): Promise<EmployeeProfile> => {
    try {
      const response = await apiClient.get(`/employees/${id}/profile`)
      return response.data
    } catch {
      const fallbackResponse = await apiClient.get(`/employees/${id}`)
      return fallbackResponse.data
    }
  },

  create: async (employee: Omit<Employee, 'id' | 'createdAtUtc'>) => {
    const response = await apiClient.post('/employees', employee)
    return response.data
  },

  update: async (id: string, employee: Partial<Employee>) => {
    const response = await apiClient.put(`/employees/${id}`, employee)
    return response.data
  },

  delete: async (id: string) => {
    await apiClient.delete(`/employees/${id}`)
  },

  activate: async (id: string) => {
    const response = await apiClient.post(`/employees/${id}/activate`)
    return response.data
  },

  deactivate: async (id: string) => {
    const response = await apiClient.post(`/employees/${id}/deactivate`)
    return response.data
  },

  terminate: async (id: string) => {
    const response = await apiClient.post(`/employees/${id}/terminate`)
    return response.data
  },

  getTimeline: async (id: string) => {
    const response = await apiClient.get(`/employees/${id}/timeline`)
    return response.data
  },

  getRoles: async (id: string) => {
    const response = await apiClient.get(`/employees/${id}/roles`)
    return response.data
  },
}
