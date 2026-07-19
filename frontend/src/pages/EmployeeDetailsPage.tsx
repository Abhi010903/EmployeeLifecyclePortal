import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import Modal from '@/components/Common/Modal'
import { ArrowLeft, Edit2, Trash2, CheckCircle2, XCircle, AlertCircle } from 'lucide-react'
import { employeesApi } from '@/api/employees'
import type { EmployeeProfile } from '@/types'
import toast from 'react-hot-toast'

export default function EmployeeDetailsPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [employee, setEmployee] = useState<EmployeeProfile | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false)
  const [isDeleting, setIsDeleting] = useState(false)

  useEffect(() => {
    if (id) {
      loadEmployee()
    }
  }, [id])

  const loadEmployee = async () => {
    try {
      setIsLoading(true)
      const data = await employeesApi.getProfile(id!)
      setEmployee(data)
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to load employee')
      navigate('/employees')
    } finally {
      setIsLoading(false)
    }
  }

  const handleDelete = async () => {
    if (!employee) return

    try {
      setIsDeleting(true)
      await employeesApi.delete(employee.id)
      toast.success('Employee deleted successfully')
      navigate('/employees')
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to delete employee')
    } finally {
      setIsDeleting(false)
      setIsDeleteConfirmOpen(false)
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

  if (!employee) {
    return (
      <MainLayout>
        <div className="text-center py-12">
          <p className="text-neutral-600">Employee not found</p>
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
            onClick={() => navigate('/employees')}
            className="p-2 hover:bg-neutral-100 rounded-lg transition-colors"
          >
            <ArrowLeft className="w-5 h-5" />
          </button>
          <div className="flex-1">
            <h1 className="text-3xl font-bold text-neutral-900">
              {employee.firstName} {employee.lastName}
            </h1>
            <p className="text-neutral-600 mt-1">Employee Code: {employee.employeeCode}</p>
          </div>
          <div className="flex gap-2">
            <Button
              variant="outline"
              onClick={() => navigate(`/employees/${employee.id}/edit`)}
            >
              <Edit2 className="w-4 h-4 mr-2" />
              Edit
            </Button>
            <Button
              variant="danger"
              onClick={() => setIsDeleteConfirmOpen(true)}
            >
              <Trash2 className="w-4 h-4 mr-2" />
              Delete
            </Button>
          </div>
        </div>

        {/* Main Information */}
        <div className="grid grid-cols-3 gap-6">
          {/* Personal Information */}
          <Card className="col-span-2">
            <div className="pb-6 border-b border-neutral-200">
              <h2 className="text-xl font-semibold text-neutral-900">Personal Information</h2>
            </div>
            <div className="space-y-6 mt-6">
              <div className="grid grid-cols-2 gap-6">
                <div>
                  <p className="text-sm text-neutral-600">First Name</p>
                  <p className="text-lg font-medium text-neutral-900 mt-1">{employee.firstName}</p>
                </div>
                <div>
                  <p className="text-sm text-neutral-600">Last Name</p>
                  <p className="text-lg font-medium text-neutral-900 mt-1">{employee.lastName}</p>
                </div>
              </div>

              <div>
                <p className="text-sm text-neutral-600">Email</p>
                <p className="text-lg font-medium text-neutral-900 mt-1">{employee.email}</p>
              </div>

              <div>
                <p className="text-sm text-neutral-600">Phone Number</p>
                <p className="text-lg font-medium text-neutral-900 mt-1">{employee.phoneNumber || '-'}</p>
              </div>

              <div className="grid grid-cols-2 gap-6">
                <div>
                  <p className="text-sm text-neutral-600">Department</p>
                  <p className="text-lg font-medium text-neutral-900 mt-1">{employee.departmentName || '-'}</p>
                </div>
                <div>
                  <p className="text-sm text-neutral-600">Manager</p>
                  <p className="text-lg font-medium text-neutral-900 mt-1">{employee.managerName || '-'}</p>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-6">
                <div>
                  <p className="text-sm text-neutral-600">Created On</p>
                  <p className="text-lg font-medium text-neutral-900 mt-1">{formatDate(employee.createdAtUtc)}</p>
                </div>
              </div>
            </div>
          </Card>

          {/* Status and Quick Info */}
          <div className="space-y-6">
            <Card>
              <div className="pb-4 border-b border-neutral-200 mb-4">
                <h3 className="font-semibold text-neutral-900">Status</h3>
              </div>
              <div className="flex items-center gap-3">
                {employee.status === 'Active' ? (
                  <CheckCircle2 className="w-5 h-5 text-green-600" />
                ) : employee.status === 'Inactive' ? (
                  <AlertCircle className="w-5 h-5 text-amber-600" />
                ) : (
                  <XCircle className="w-5 h-5 text-red-600" />
                )}
                <Badge
                  label={employee.status}
                  variant={
                    employee.status === 'Active'
                      ? 'success'
                      : employee.status === 'Inactive'
                        ? 'warning'
                        : 'danger'
                  }
                />
              </div>
            </Card>

            <Card>
              <div className="pb-4 border-b border-neutral-200 mb-4">
                <h3 className="font-semibold text-neutral-900">Quick Info</h3>
              </div>
              <div className="space-y-4">
                <div>
                  <p className="text-sm text-neutral-600">Roles</p>
                  <div className="mt-2 space-y-1">
                    {employee.roles && employee.roles.length > 0 ? (
                      employee.roles.map((role) => (
                        <div key={role.id} className="inline-block mr-2 mb-2">
                          <span className="px-3 py-1 bg-blue-100 text-blue-700 text-xs font-medium rounded-full">
                            {role.name}
                          </span>
                        </div>
                      ))
                    ) : (
                      <p className="text-neutral-600 text-sm">No roles assigned</p>
                    )}
                  </div>
                </div>
              </div>
            </Card>
          </div>
        </div>

        {/* Documents and Timeline */}
        <div className="grid grid-cols-2 gap-6">
          <Card>
            <div className="pb-4 border-b border-neutral-200 mb-4">
              <h3 className="font-semibold text-neutral-900">Documents</h3>
            </div>
            <div className="text-center py-6">
              <p className="text-neutral-600">{employee.documentsCount || 0} documents</p>
            </div>
          </Card>

          <Card>
            <div className="pb-4 border-b border-neutral-200 mb-4">
              <h3 className="font-semibold text-neutral-900">Direct Reports</h3>
            </div>
            <div className="text-center py-6">
              <p className="text-neutral-600">{employee.subordinatesCount || 0} subordinates</p>
            </div>
          </Card>
        </div>
      </div>

      {/* Delete Confirmation Modal */}
      <Modal
        isOpen={isDeleteConfirmOpen}
        onClose={() => setIsDeleteConfirmOpen(false)}
        title="Delete Employee"
        footer={
          <>
            <Button variant="outline" onClick={() => setIsDeleteConfirmOpen(false)} disabled={isDeleting}>
              Cancel
            </Button>
            <Button variant="danger" onClick={handleDelete} disabled={isDeleting}>
              {isDeleting ? 'Deleting...' : 'Delete'}
            </Button>
          </>
        }
      >
        <p className="text-neutral-600">
          Are you sure you want to delete <strong>{employee.firstName} {employee.lastName}</strong>? This action cannot be undone.
        </p>
      </Modal>
    </MainLayout>
  )
}
