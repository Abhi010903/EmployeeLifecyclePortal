import { apiClient } from './client'
import type { Department, PaginatedResponse } from '@/types'

export const departmentsApi = {
  getAll: async (pageNumber = 1, pageSize = 10): Promise<PaginatedResponse<Department>> => {
    const response = await apiClient.get(`/departments?pageNumber=${pageNumber}&pageSize=${pageSize}`)
    return response.data
  },

  getAllSimple: async (): Promise<Department[]> => {
    const response = await apiClient.get('/departments')
    return response.data
  },

  getById: async (id: string): Promise<Department> => {
    const response = await apiClient.get(`/departments/${id}`)
    return response.data
  },

  getEmployees: async (id: string) => {
    const response = await apiClient.get(`/departments/${id}/employees`)
    return response.data
  },

  create: async (data: { name: string; description: string }) => {
    const response = await apiClient.post('/departments', {
      name: data.name,
      description: data.description,
    })
    return response.data
  },

  update: async (id: string, data: { name: string; description: string }) => {
    const response = await apiClient.put(`/departments/${id}`, {
      name: data.name,
      description: data.description,
    })
    return response.data
  },

  delete: async (id: string) => {
    await apiClient.delete(`/departments/${id}`)
  },
}
