import apiClient from './client'
import { WorkTask, CreateWorkTaskDto, UpdateWorkTaskStatusDto } from '@/types'

export const tasksApi = {
  getAll: async (params?: { employeeId?: string; departmentId?: string; status?: string }) => {
    const response = await apiClient.get<WorkTask[]>('/tasks', { params })
    return response.data
  },

  create: async (data: CreateWorkTaskDto) => {
    const response = await apiClient.post<WorkTask>('/tasks', data)
    return response.data
  },

  updateStatus: async (id: string, data: UpdateWorkTaskStatusDto) => {
    const response = await apiClient.put<WorkTask>(`/tasks/${id}/status`, data)
    return response.data
  },
}
