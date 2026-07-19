import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import { ArrowLeft, Edit2, Trash2 } from 'lucide-react'
import { rolesApi } from '@/api/roles'
import type { Role } from '@/types'
import toast from 'react-hot-toast'

const PERMISSIONS = [
  'Dashboard',
  'Employees',
  'Departments',
  'Attendance',
  'Leave',
  'Payroll',
  'Reports',
  'Settings',
]

export default function RoleDetailsPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [role, setRole] = useState<Role | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false)

  useEffect(() => {
    if (id) {
      loadRole()
    }
  }, [id])

  const loadRole = async () => {
    try {
      setIsLoading(true)
      const data = await rolesApi.getById(id!)
      setRole(data)
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to load role')
      navigate('/roles')
    } finally {
      setIsLoading(false)
    }
  }

  const handleDelete = async () => {
    try {
      await rolesApi.delete(id!)
      toast.success('Role deleted successfully')
      navigate('/roles')
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to delete role')
    }
  }

  const formatDate = (date: string) => {
    return new Intl.DateTimeFormat('en-IN', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    }).format(new Date(date))
  }

  if (isLoading) {
    return (
      <MainLayout>
        <div className="flex items-center justify-center py-12">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
        </div>
      </MainLayout>
    )
  }

  if (!role) {
    return (
      <MainLayout>
        <div className="text-center py-12">
          <p className="text-neutral-600">Role not found</p>
        </div>
      </MainLayout>
    )
  }

  return (
    <MainLayout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center gap-4">
          <button
            onClick={() => navigate('/roles')}
            className="p-2 hover:bg-neutral-100 rounded-lg transition-colors"
          >
            <ArrowLeft className="w-5 h-5" />
          </button>
          <div className="flex-1">
            <h1 className="text-3xl font-bold text-neutral-900">{role.name}</h1>
            <p className="text-neutral-600 mt-1">{role.description}</p>
          </div>
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => navigate(`/roles/${id}/edit`)}>
              <Edit2 className="w-4 h-4 mr-2" />
              Edit
            </Button>
            <Button variant="danger" onClick={() => setIsDeleteConfirmOpen(true)}>
              <Trash2 className="w-4 h-4 mr-2" />
              Delete
            </Button>
          </div>
        </div>

        {/* Details Grid */}
        <div className="grid grid-cols-3 gap-6">
          {/* Information */}
          <Card>
            <div className="pb-4 border-b border-neutral-200 mb-4">
              <h3 className="font-semibold text-neutral-900">Information</h3>
            </div>
            <div className="space-y-4">
              <div>
                <p className="text-sm text-neutral-600">Created On</p>
                <p className="text-lg font-medium text-neutral-900 mt-1">
                  {formatDate(role.createdAtUtc)}
                </p>
              </div>
              <div>
                <p className="text-sm text-neutral-600">Created By</p>
                <p className="text-lg font-medium text-neutral-900 mt-1">
                  {role.createdBy || 'System'}
                </p>
              </div>
            </div>
          </Card>

          {/* Permissions Matrix */}
          <Card className="col-span-2">
            <div className="pb-4 border-b border-neutral-200 mb-4">
              <h3 className="font-semibold text-neutral-900">Permissions</h3>
            </div>
            <div className="grid grid-cols-2 gap-4">
              {PERMISSIONS.map((permission) => (
                <div key={permission} className="flex items-center gap-2 p-3 bg-neutral-50 rounded-lg">
                  <div className="w-4 h-4 bg-green-500 rounded"></div>
                  <span className="text-sm text-neutral-700">{permission}</span>
                </div>
              ))}
            </div>
          </Card>
        </div>

        {/* Access Levels */}
        <Card>
          <div className="pb-4 border-b border-neutral-200 mb-4">
            <h3 className="font-semibold text-neutral-900">Access Levels</h3>
          </div>
          <div className="grid grid-cols-4 gap-4">
            {['Admin', 'HR', 'Manager', 'Employee'].map((level) => (
              <div key={level} className="p-4 border border-neutral-200 rounded-lg text-center">
                <p className="font-medium text-neutral-900">{level}</p>
                <p className="text-sm text-neutral-600 mt-1">Full Access</p>
              </div>
            ))}
          </div>
        </Card>
      </div>

      {/* Delete Confirmation Modal */}
      {isDeleteConfirmOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
          <div className="absolute inset-0 bg-black bg-opacity-50" onClick={() => setIsDeleteConfirmOpen(false)} />
          <Card className="relative max-w-md w-full mx-4">
            <div className="pb-4 border-b border-neutral-200 mb-4">
              <h2 className="text-lg font-semibold text-neutral-900">Delete Role</h2>
            </div>
            <p className="text-neutral-600 mb-6">
              Are you sure you want to delete <strong>{role.name}</strong>? This action cannot be undone.
            </p>
            <div className="flex gap-4 justify-end">
              <Button variant="outline" onClick={() => setIsDeleteConfirmOpen(false)}>
                Cancel
              </Button>
              <Button variant="danger" onClick={handleDelete}>
                Delete
              </Button>
            </div>
          </Card>
        </div>
      )}
    </MainLayout>
  )
}
