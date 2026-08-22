import { useEffect, useState, useCallback } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import Modal from '@/components/Common/Modal'
import { Package, Users, Wrench, AlertCircle, Laptop, Plus } from 'lucide-react'
import { assetsApi } from '@/api/assets'
import { employeesApi } from '@/api/employees'
import { AssetDto, AssetAssignmentDto, AssetMaintenanceDto, Employee } from '@/types'
import { useAuthStore } from '@/store/authStore'
import { formatDateIST, formatCurrencyINR } from '@/utils/format'
import toast from 'react-hot-toast'

type TabType = 'inventory' | 'assignments' | 'maintenance'

const ASSET_TYPES = ['Laptop', 'Desktop', 'Monitor', 'Phone', 'ID Card', 'Accessories']

export default function AssetPage() {
  const { user } = useAuthStore()
  const employeeId = user?.id || localStorage.getItem('userId') || ''
  const [activeTab, setActiveTab] = useState<TabType>('inventory')
  const [assets, setAssets] = useState<AssetDto[]>([])
  const [assignments, setAssignments] = useState<AssetAssignmentDto[]>([])
  const [maintenance, setMaintenance] = useState<AssetMaintenanceDto[]>([])
  const [employees, setEmployees] = useState<Employee[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Add Asset Modal State
  const [isAddAssetModalOpen, setIsAddAssetModalOpen] = useState(false)
  const [assetName, setAssetName] = useState('')
  const [assetType, setAssetType] = useState('Laptop')
  const [serialNumber, setSerialNumber] = useState('')
  const [purchaseValue, setPurchaseValue] = useState('65000')
  const [isSubmittingAsset, setIsSubmittingAsset] = useState(false)

  // Assign Asset Modal State
  const [isAssignModalOpen, setIsAssignModalOpen] = useState(false)
  const [selectedAssetId, setSelectedAssetId] = useState('')
  const [targetEmployeeId, setTargetEmployeeId] = useState('')
  const [assignNotes, setAssignNotes] = useState('')
  const [isSubmittingAssign, setIsSubmittingAssign] = useState(false)

  const fetchAssets = useCallback(async () => {
    try {
      const response = await assetsApi.getAll()
      setAssets(Array.isArray(response.data) ? response.data : [])
    } catch (err) {
      setError('Failed to load assets')
    }
  }, [])

  const fetchAssignments = useCallback(async () => {
    try {
      if (user?.role === 'Admin' || user?.role === 'Manager' || user?.role === 'HR') {
        const response = await assetsApi.getAllAssignments()
        setAssignments(Array.isArray(response.data) ? response.data : [])
      } else if (employeeId) {
        const response = await assetsApi.getEmployeeAssets(employeeId)
        setAssignments(Array.isArray(response.data) ? response.data : [])
      } else {
        setAssignments([])
      }
    } catch (err) {
      console.error('Failed to load assignments:', err)
    }
  }, [employeeId, user?.role])

  const fetchEmployees = useCallback(async () => {
    try {
      const emps = await employeesApi.getAllSimple()
      setEmployees(Array.isArray(emps) ? emps : [])
    } catch (err) {
      console.error('Failed to load employees for assignment:', err)
    }
  }, [])

  const fetchMaintenance = useCallback(async () => {
    try {
      if (assets.length > 0) {
        const maintenanceData: AssetMaintenanceDto[] = []
        for (const asset of assets.slice(0, 3)) {
          const response = await assetsApi.getMaintenanceHistory(asset.id)
          maintenanceData.push(...(response.data || []))
        }
        setMaintenance(maintenanceData.slice(0, 10))
      }
    } catch (err) {
      setError('Failed to load maintenance history')
    }
  }, [assets])

  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true)
        await Promise.all([fetchAssets(), fetchAssignments(), fetchEmployees()])
      } catch (err) {
        setError('Failed to load asset data')
      } finally {
        setLoading(false)
      }
    }

    loadData()
  }, [fetchAssets, fetchAssignments, fetchEmployees])

  useEffect(() => {
    if (activeTab === 'maintenance') {
      fetchMaintenance()
    }
  }, [activeTab, fetchMaintenance])

  const handleCreateAsset = async () => {
    if (!assetName.trim() || !serialNumber.trim()) {
      toast.error('Please enter asset name and serial number.')
      return
    }

    try {
      setIsSubmittingAsset(true)
      const code = 'AST-' + Math.floor(100000 + Math.random() * 900000)
      await assetsApi.create({
        assetCode: code,
        assetName: assetName.trim(),
        assetType,
        serialNumber: serialNumber.trim(),
        purchaseValue: parseFloat(purchaseValue) || 0,
        purchaseDateUtc: new Date().toISOString(),
      })
      toast.success('Asset created successfully!')
      setIsAddAssetModalOpen(false)
      setAssetName('')
      setSerialNumber('')
      fetchAssets()
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Failed to create asset.')
    } finally {
      setIsSubmittingAsset(false)
    }
  }

  const handleAssignAsset = async () => {
    if (!selectedAssetId || !targetEmployeeId) {
      toast.error('Please select an asset and an employee.')
      return
    }

    try {
      setIsSubmittingAssign(true)
      await assetsApi.assignAsset(selectedAssetId, {
        employeeId: targetEmployeeId,
        notes: assignNotes.trim() || undefined,
      })
      toast.success('Asset assigned successfully!')
      setIsAssignModalOpen(false)
      setSelectedAssetId('')
      setTargetEmployeeId('')
      setAssignNotes('')
      fetchAssets()
      fetchAssignments()
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Failed to assign asset.')
    } finally {
      setIsSubmittingAssign(false)
    }
  }

  const totalAssets = assets.length
  const availableAssets = assets.filter(a => a.status === 'Available').length
  const assignedAssets = assets.filter(a => a.status === 'Assigned').length

  return (
    <MainLayout>
      <div className="space-y-6">
        <div className="flex justify-between items-start">
          <div>
            <h1 className="text-3xl font-bold text-neutral-900">Asset Management</h1>
            <p className="text-neutral-600 mt-1">Manage equipment and asset assignments</p>
          </div>
          <div className="flex gap-3">
            <Button variant="outline" onClick={() => setIsAssignModalOpen(true)}>
              <Users className="w-4 h-4 mr-2" />
              Assign Asset
            </Button>
            <Button onClick={() => setIsAddAssetModalOpen(true)}>
              <Plus className="w-4 h-4 mr-2" />
              Add Asset
            </Button>
          </div>
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 flex items-start gap-3">
            <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
            <p className="text-sm text-red-900">{error}</p>
          </div>
        )}

        {/* Statistics Cards */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
          <Card className="bg-blue-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Total Assets</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">{totalAssets}</p>
              </div>
              <Package className="w-8 h-8 text-blue-600" />
            </div>
          </Card>

          <Card className="bg-green-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Available</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">{availableAssets}</p>
              </div>
              <Laptop className="w-8 h-8 text-green-600" />
            </div>
          </Card>

          <Card className="bg-amber-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Assigned</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">{assignedAssets}</p>
              </div>
              <Users className="w-8 h-8 text-amber-600" />
            </div>
          </Card>

          <Card className="bg-purple-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">My Assets</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">{assignments.length}</p>
              </div>
              <Wrench className="w-8 h-8 text-purple-600" />
            </div>
          </Card>
        </div>

        {/* Tabs */}
        <div className="flex gap-2 border-b border-neutral-200">
          {(['inventory', 'assignments', 'maintenance'] as const).map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              className={`px-4 py-2 font-medium border-b-2 transition-colors ${
                activeTab === tab
                  ? 'border-blue-600 text-blue-600'
                  : 'border-transparent text-neutral-600 hover:text-neutral-900'
              }`}
            >
              {tab === 'inventory' && 'Inventory'}
              {tab === 'assignments' && 'My Assets'}
              {tab === 'maintenance' && 'Maintenance'}
            </button>
          ))}
        </div>

        {/* Content */}
        {loading ? (
          <Card>
            <div className="text-center py-12">
              <p className="text-neutral-500">Loading asset data...</p>
            </div>
          </Card>
        ) : activeTab === 'inventory' ? (
          <InventorySection assets={assets} />
        ) : activeTab === 'assignments' ? (
          <AssignmentsSection assignments={assignments} onRefresh={fetchAssignments} />
        ) : (
          <MaintenanceSection maintenance={maintenance} />
        )}
      </div>

      {/* Add Asset Modal */}
      <Modal
        isOpen={isAddAssetModalOpen}
        onClose={() => setIsAddAssetModalOpen(false)}
        title="Add New Asset"
        footer={
          <>
            <Button variant="outline" onClick={() => setIsAddAssetModalOpen(false)} disabled={isSubmittingAsset}>
              Cancel
            </Button>
            <Button onClick={handleCreateAsset} disabled={isSubmittingAsset}>
              {isSubmittingAsset ? 'Creating...' : 'Create Asset'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Asset Name *</label>
            <input
              type="text"
              placeholder="e.g., MacBook Pro 16 M3"
              value={assetName}
              onChange={(e) => setAssetName(e.target.value)}
              className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">Asset Type *</label>
              <select
                value={assetType}
                onChange={(e) => setAssetType(e.target.value)}
                className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
              >
                {ASSET_TYPES.map((t) => (
                  <option key={t} value={t}>{t}</option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">Purchase Value (₹) *</label>
              <input
                type="number"
                value={purchaseValue}
                onChange={(e) => setPurchaseValue(e.target.value)}
                className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Serial Number *</label>
            <input
              type="text"
              placeholder="e.g., C02G1234MD6R"
              value={serialNumber}
              onChange={(e) => setSerialNumber(e.target.value)}
              className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
          </div>
        </div>
      </Modal>

      {/* Assign Asset Modal */}
      <Modal
        isOpen={isAssignModalOpen}
        onClose={() => setIsAssignModalOpen(false)}
        title="Assign Asset to Employee"
        footer={
          <>
            <Button variant="outline" onClick={() => setIsAssignModalOpen(false)} disabled={isSubmittingAssign}>
              Cancel
            </Button>
            <Button onClick={handleAssignAsset} disabled={isSubmittingAssign}>
              {isSubmittingAssign ? 'Assigning...' : 'Assign Asset'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Select Available Asset *</label>
            <select
              value={selectedAssetId}
              onChange={(e) => setSelectedAssetId(e.target.value)}
              className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              <option value="">Choose an available asset...</option>
              {assets
                .filter((a) => a.status === 'Available')
                .map((a) => (
                  <option key={a.id} value={a.id}>
                    {a.assetName} ({a.assetCode} - {a.assetType})
                  </option>
                ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Select Employee *</label>
            <select
              value={targetEmployeeId}
              onChange={(e) => setTargetEmployeeId(e.target.value)}
              className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              <option value="">Choose an employee...</option>
              {employees.map((emp) => (
                <option key={emp.id} value={emp.id}>
                  {emp.firstName} {emp.lastName} ({emp.employeeCode})
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Assignment Notes</label>
            <textarea
              rows={2}
              placeholder="e.g., Primary developer laptop"
              value={assignNotes}
              onChange={(e) => setAssignNotes(e.target.value)}
              className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
          </div>
        </div>
      </Modal>
    </MainLayout>
  )
}

interface InventorySectionProps {
  assets: AssetDto[]
}

function InventorySection({ assets }: InventorySectionProps) {
  const [selectedType, setSelectedType] = useState<string | null>(null)

  const filteredAssets = selectedType
    ? assets.filter(a => a.assetType === selectedType)
    : assets

  return (
    <Card>
      <div className="mb-4 flex gap-2 flex-wrap">
        <button
          onClick={() => setSelectedType(null)}
          className={`px-3 py-1 rounded-full text-sm font-medium ${
            selectedType === null
              ? 'bg-blue-600 text-white'
              : 'bg-neutral-100 text-neutral-700 hover:bg-neutral-200'
          }`}
        >
          All
        </button>
        {ASSET_TYPES.map(type => (
          <button
            key={type}
            onClick={() => setSelectedType(type)}
            className={`px-3 py-1 rounded-full text-sm font-medium ${
              selectedType === type
                ? 'bg-blue-600 text-white'
                : 'bg-neutral-100 text-neutral-700 hover:bg-neutral-200'
            }`}
          >
            {type}
          </button>
        ))}
      </div>

      {filteredAssets.length === 0 ? (
        <div className="text-center py-12">
          <Package className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
          <p className="text-neutral-500">No assets found</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-neutral-200 bg-neutral-50">
                <th className="px-4 py-3 text-left font-semibold text-neutral-900">Code</th>
                <th className="px-4 py-3 text-left font-semibold text-neutral-900">Asset Name</th>
                <th className="px-4 py-3 text-left font-semibold text-neutral-900">Type</th>
                <th className="px-4 py-3 text-left font-semibold text-neutral-900">Serial Number</th>
                <th className="px-4 py-3 text-left font-semibold text-neutral-900">Status</th>
                <th className="px-4 py-3 text-left font-semibold text-neutral-900">Value</th>
              </tr>
            </thead>
            <tbody>
              {filteredAssets.map(asset => (
                <tr key={asset.id} className="border-b border-neutral-200 hover:bg-neutral-50">
                  <td className="px-4 py-3 text-neutral-700 font-mono text-xs">{asset.assetCode}</td>
                  <td className="px-4 py-3 text-neutral-700 font-medium">{asset.assetName}</td>
                  <td className="px-4 py-3 text-neutral-700">{asset.assetType}</td>
                  <td className="px-4 py-3 text-neutral-700 font-mono text-xs">{asset.serialNumber}</td>
                  <td className="px-4 py-3">
                    <Badge
                      label={asset.status}
                      variant={
                        asset.status === 'Available'
                          ? 'success'
                          : asset.status === 'Assigned'
                            ? 'warning'
                            : 'neutral'
                      }
                    />
                  </td>
                  <td className="px-4 py-3 text-neutral-700 font-medium">{formatCurrencyINR(asset.purchaseValue)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </Card>
  )
}

interface AssignmentsSectionProps {
  assignments: AssetAssignmentDto[]
  onRefresh: () => Promise<void>
}

function AssignmentsSection({ assignments, onRefresh }: AssignmentsSectionProps) {
  const [isReturning, setIsReturning] = useState<string | null>(null)

  const handleReturn = async (assignmentId: string) => {
    try {
      setIsReturning(assignmentId)
      await assetsApi.returnAsset(assignmentId)
      toast.success('Asset returned successfully!')
      await onRefresh()
    } catch (err) {
      toast.error('Failed to return asset.')
    } finally {
      setIsReturning(null)
    }
  }

  return (
    <Card>
      {assignments.length === 0 ? (
        <div className="text-center py-12">
          <Users className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
          <p className="text-neutral-500">No assets assigned</p>
        </div>
      ) : (
        <div className="space-y-4">
          {assignments.map(assignment => (
            <div key={assignment.id} className="border border-neutral-200 rounded-lg p-4 hover:bg-neutral-50">
              <div className="flex justify-between items-start mb-3">
                <div>
                  <h3 className="font-semibold text-neutral-900">{assignment.assetName}</h3>
                  <p className="text-sm text-neutral-600">{assignment.assetType} • Assigned to: <strong className="text-neutral-800">{assignment.employeeName}</strong></p>
                </div>
                <Badge label={assignment.status} variant={assignment.status === 'Active' ? 'success' : 'neutral'} />
              </div>

              <div className="grid grid-cols-2 gap-4 text-sm text-neutral-600 mb-3">
                <div>
                  <span className="text-neutral-500">Assigned:</span>
                  <p className="font-medium text-neutral-900">{formatDateIST(assignment.assignedDateUtc)}</p>
                </div>
                {assignment.notes && (
                  <div>
                    <span className="text-neutral-500">Notes:</span>
                    <p className="font-medium text-neutral-900">{assignment.notes}</p>
                  </div>
                )}
              </div>

              {assignment.status === 'Active' && (
                <Button
                  onClick={() => handleReturn(assignment.id)}
                  disabled={isReturning === assignment.id}
                  className="w-full bg-red-600 hover:bg-red-700 text-sm"
                >
                  {isReturning === assignment.id ? 'Returning...' : 'Return Asset'}
                </Button>
              )}
            </div>
          ))}
        </div>
      )}
    </Card>
  )
}

interface MaintenanceSectionProps {
  maintenance: AssetMaintenanceDto[]
}

function MaintenanceSection({ maintenance }: MaintenanceSectionProps) {
  return (
    <Card>
      {maintenance.length === 0 ? (
        <div className="text-center py-12">
          <Wrench className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
          <p className="text-neutral-500">No maintenance records</p>
        </div>
      ) : (
        <div className="space-y-4">
          {maintenance.map(record => (
            <div key={record.id} className="border border-neutral-200 rounded-lg p-4 hover:bg-neutral-50">
              <div className="flex justify-between items-start mb-2">
                <h3 className="font-semibold text-neutral-900">{record.assetName}</h3>
                <Badge label={formatDateIST(record.maintenanceDateUtc)} variant="info" />
              </div>

              <p className="text-sm text-neutral-600 mb-3">{record.description}</p>

              <div className="grid grid-cols-2 gap-4 text-sm text-neutral-600">
                <div>
                  <span className="text-neutral-500">Cost:</span>
                  <p className="font-medium text-neutral-900">{formatCurrencyINR(record.cost)}</p>
                </div>
                <div>
                  <span className="text-neutral-500">Provider:</span>
                  <p className="font-medium text-neutral-900">{record.serviceProvider}</p>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </Card>
  )
}
