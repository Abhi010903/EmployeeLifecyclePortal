import { apiClient } from './client'
import type { Department, PaginatedResponse, StaffingRequest, CreateStaffingRequestDto, ResolveStaffingRequestDto } from '@/types'

export const departmentsApi = {
  getAll: async (pageNumber = 1, pageSize = 10): Promise<PaginatedResponse<Department>> => {
    const response = await apiClient.get(`/departments?pageNumber=${pageNumber}&pageSize=${pageSize}`)
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

  getAllSimple: async (): Promise<Department[]> => {
    const response = await apiClient.get('/departments')
    const data = response.data
    if (Array.isArray(data)) return data
    if (data && Array.isArray(data.items)) return data.items
    return []
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

  getStaffingRequests: async (departmentId?: string): Promise<StaffingRequest[]> => {
    const response = await apiClient.get<StaffingRequest[]>('/departments/staffing-requests', {
      params: { departmentId },
    })
    return response.data
  },

  createStaffingRequest: async (data: CreateStaffingRequestDto): Promise<StaffingRequest> => {
    const response = await apiClient.post<StaffingRequest>('/departments/staffing-requests', data)
    return response.data
  },

  resolveStaffingRequest: async (id: string, data: ResolveStaffingRequestDto): Promise<StaffingRequest> => {
    const response = await apiClient.put<StaffingRequest>(`/departments/staffing-requests/${id}/resolve`, data)
    return response.data
  },
}
