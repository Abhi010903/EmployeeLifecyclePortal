import { apiClient } from './client'

export interface EmployeeDocument {
  id: string
  employeeId: string
  documentType: string
  fileName: string
  filePath: string
  fileType: string
  fileSizeBytes: number
  formattedFileSize?: string
  expirationDateUtc?: string
  notes?: string
  isArchived: boolean
  isExpired: boolean
  createdAtUtc: string
  createdBy?: string
}

export const documentsApi = {
  getDocuments: async (employeeId: string, includeArchived = false): Promise<EmployeeDocument[]> => {
    const response = await apiClient.get(`/employees/${employeeId}/documents?includeArchived=${includeArchived}`)
    return Array.isArray(response.data) ? response.data : []
  },

  uploadDocument: async (employeeId: string, formData: FormData): Promise<EmployeeDocument> => {
    const response = await apiClient.post(`/employees/${employeeId}/documents`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return response.data
  },

  downloadDocument: async (employeeId: string, documentId: string, fileName: string) => {
    const response = await apiClient.get(`/employees/${employeeId}/documents/${documentId}/download`, {
      responseType: 'blob',
    })
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', fileName)
    document.body.appendChild(link)
    link.click()
    link.remove()
  },

  deleteDocument: async (employeeId: string, documentId: string): Promise<void> => {
    await apiClient.delete(`/employees/${employeeId}/documents/${documentId}`)
  },
}
