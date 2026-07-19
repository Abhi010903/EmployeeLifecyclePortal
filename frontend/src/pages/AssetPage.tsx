import { useEffect, useState, useCallback } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import { Package, Users, Wrench, AlertCircle, Laptop } from 'lucide-react'
import { assetsApi } from '@/api/assets'
import { AssetDto, AssetAssignmentDto, AssetMaintenanceDto } from '@/types'

type TabType = 'inventory' | 'assignments' | 'maintenance'

const ASSET_TYPES = ['Laptop', 'Desktop', 'Monitor', 'Phone', 'ID Card', 'Accessories']

export default function AssetPage() {
  const [activeTab, setActiveTab] = useState<TabType>('inventory')
  const [assets, setAssets] = useState<AssetDto[]>([])
  const [assignments, setAssignments] = useState<AssetAssignmentDto[]>([])
  const [maintenance, setMaintenance] = useState<AssetMaintenanceDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const employeeId = localStorage.getItem('userId') || ''

  const fetchAssets = useCallback(async () => {
    try {
      const response = await assetsApi.getAll()
      setAssets(response.data || [])
    } catch (err) {
      setError('Failed to load assets')
    }
  }, [])

  const fetchAssignments = useCallback(async () => {
    try {
      const response = await assetsApi.getEmployeeAssets(employeeId)
      setAssignments(response.data || [])
    } catch (err) {
      setError('Failed to load assignments')
    }
  }, [employeeId])

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
        await fetchAssets()
        await fetchAssignments()
      } catch (err) {
        setError('Failed to load asset data')
      } finally {
        setLoading(false)
      }
    }

    loadData()
  }, [fetchAssets, fetchAssignments])

  useEffect(() => {
    if (activeTab === 'maintenance') {
      fetchMaintenance()
    }
  }, [activeTab, fetchMaintenance])

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
                  <td className="px-4 py-3 text-neutral-700">{asset.assetCode}</td>
                  <td className="px-4 py-3 text-neutral-700">{asset.assetName}</td>
                  <td className="px-4 py-3 text-neutral-700">{asset.assetType}</td>
                  <td className="px-4 py-3 text-neutral-700 font-mono text-xs">{asset.serialNumber}</td>
                  <td className="px-4 py-3">
                    <Badge label={asset.status} variant={asset.status === 'Available' ? 'success' : 'info'} />
                  </td>
                  <td className="px-4 py-3 text-neutral-700">₹{asset.purchaseValue.toLocaleString('en-IN')}</td>
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
      await onRefresh()
    } catch (err) {
      console.error('Failed to return asset:', err)
    } finally {
      setIsReturning(null)
    }
  }

  return (
    <Card>
      {assignments.length === 0 ? (
        <div className="text-center py-12">
          <Users className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
          <p className="text-neutral-500">No assets assigned to you</p>
        </div>
      ) : (
        <div className="space-y-4">
          {assignments.map(assignment => (
            <div key={assignment.id} className="border border-neutral-200 rounded-lg p-4 hover:bg-neutral-50">
              <div className="flex justify-between items-start mb-3">
                <div>
                  <h3 className="font-semibold text-neutral-900">{assignment.assetName}</h3>
                  <p className="text-sm text-neutral-600">{assignment.assetType}</p>
                </div>
                <Badge label={assignment.status} variant="success" />
              </div>

              <div className="grid grid-cols-2 gap-4 text-sm text-neutral-600 mb-3">
                <div>
                  <span className="text-neutral-500">Assigned:</span>
                  <p className="font-medium text-neutral-900">{new Date(assignment.assignedDateUtc).toLocaleDateString()}</p>
                </div>
                {assignment.notes && (
                  <div>
                    <span className="text-neutral-500">Notes:</span>
                    <p className="font-medium text-neutral-900">{assignment.notes}</p>
                  </div>
                )}
              </div>

              <Button
                onClick={() => handleReturn(assignment.id)}
                disabled={isReturning === assignment.id}
                className="w-full bg-red-600 hover:bg-red-700 text-sm"
              >
                Return Asset
              </Button>
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
                <Badge label={new Date(record.maintenanceDateUtc).toLocaleDateString()} variant="info" />
              </div>

              <p className="text-sm text-neutral-600 mb-3">{record.description}</p>

              <div className="grid grid-cols-2 gap-4 text-sm text-neutral-600">
                <div>
                  <span className="text-neutral-500">Cost:</span>
                  <p className="font-medium text-neutral-900">₹{record.cost.toLocaleString('en-IN')}</p>
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
