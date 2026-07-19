import { apiClient } from './client'
import type { Role, PaginatedResponse } from '@/types'

export const rolesApi = {
  getAll: async (pageNumber = 1, pageSize = 10): Promise<PaginatedResponse<Role>> => {
    const response = await apiClient.get(`/roles?pageNumber=${pageNumber}&pageSize=${pageSize}`)
    return response.data
  },

  getAllSimple: async (): Promise<Role[]> => {
    const response = await apiClient.get('/roles')
    return response.data
  },

  getById: async (id: string): Promise<Role> => {
    const response = await apiClient.get(`/roles/${id}`)
    return response.data
  },

  create: async (data: { name: string; description: string }) => {
    const response = await apiClient.post('/roles', {
      name: data.name,
      description: data.description,
    })
    return response.data
  },

  update: async (id: string, data: { name: string; description: string }) => {
    const response = await apiClient.put(`/roles/${id}`, {
      name: data.name,
      description: data.description,
    })
    return response.data
  },

  delete: async (id: string) => {
    await apiClient.delete(`/roles/${id}`)
  },
}
