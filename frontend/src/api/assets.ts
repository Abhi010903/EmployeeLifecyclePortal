import { apiClient } from './client'
import { AssetDto, AssetAssignmentDto, AssetMaintenanceDto } from '@/types'

export const assetsApi = {
  // Assets management
  getAll: () =>
    apiClient.get<AssetDto[]>('/assets'),

  getAvailable: () =>
    apiClient.get<AssetDto[]>('/assets/available'),

  create: (data: {
    assetCode: string
    assetName: string
    assetType: string
    serialNumber: string
    purchaseValue: number
    purchaseDateUtc: string
  }) =>
    apiClient.post<AssetDto>('/assets', data),

  // Assignments
  getAllAssignments: () =>
    apiClient.get<AssetAssignmentDto[]>('/assets/assignments'),

  getEmployeeAssets: (employeeId: string) =>
    apiClient.get<AssetAssignmentDto[]>(
      `/assets/employee/${employeeId}`
    ),

  assignAsset: (assetId: string, data: {
    employeeId: string
    notes?: string
  }) =>
    apiClient.post<AssetAssignmentDto>(
      `/assets/${assetId}/assign`,
      data
    ),

  returnAsset: (assignmentId: string) =>
    apiClient.post<AssetAssignmentDto>(
      `/assets/${assignmentId}/return`,
      {}
    ),

  getAssetHistory: (assetId: string) =>
    apiClient.get<AssetAssignmentDto[]>(
      `/assets/${assetId}/history`
    ),

  // Maintenance
  getMaintenanceHistory: (assetId: string) =>
    apiClient.get<AssetMaintenanceDto[]>(
      `/assets/${assetId}/maintenance`
    ),

  scheduleMaintenance: (assetId: string, data: {
    description: string
    cost: number
    serviceProvider: string
  }) =>
    apiClient.post<AssetMaintenanceDto>(
      `/assets/${assetId}/maintenance`,
      data
    ),
}
