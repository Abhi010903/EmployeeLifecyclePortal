import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Input from '@/components/Common/Input'
import { ArrowLeft, Edit2, Trash2 } from 'lucide-react'
import { departmentsApi } from '@/api/departments'
import type { Department } from '@/types'
import toast from 'react-hot-toast'

export default function DepartmentDetailsPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [department, setDepartment] = useState<Department | null>(null)
  const [employees, setEmployees] = useState<any[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isEditMode, setIsEditMode] = useState(false)
  const [formData, setFormData] = useState({ name: '', description: '' })
  const [errors, setErrors] = useState<Record<string, string>>({})
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false)

  useEffect(() => {
    if (id) {
      loadDepartment()
    }
  }, [id])

  const loadDepartment = async () => {
    try {
      setIsLoading(true)
      const data = await departmentsApi.getById(id!)
      setDepartment(data)
      setFormData({ name: data.name, description: data.description })

      // Load employees
      try {
        const empData = await departmentsApi.getEmployees(id!)
        setEmployees(Array.isArray(empData) ? empData : empData.items || [])
      } catch {
        setEmployees([])
      }
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to load department')
      navigate('/departments')
    } finally {
      setIsLoading(false)
    }
  }

  const handleSave = async () => {
    const newErrors: Record<string, string> = {}
    if (!formData.name.trim()) newErrors.name = 'Department name is required'
    if (!formData.description.trim()) newErrors.description = 'Description is required'
    
    setErrors(newErrors)
    if (Object.keys(newErrors).length > 0) return

    try {
      setIsSubmitting(true)
      await departmentsApi.update(id!, {
        name: formData.name,
        description: formData.description,
      })
      toast.success('Department updated successfully')
      setIsEditMode(false)
      loadDepartment()
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to update department')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleDelete = async () => {
    try {
      await departmentsApi.delete(id!)
      toast.success('Department deleted successfully')
      navigate('/departments')
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to delete department')
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

  if (!department) {
    return (
      <MainLayout>
        <div className="text-center py-12">
          <p className="text-neutral-600">Department not found</p>
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
            onClick={() => navigate('/departments')}
            className="p-2 hover:bg-neutral-100 rounded-lg transition-colors"
          >
            <ArrowLeft className="w-5 h-5" />
          </button>
          <div className="flex-1">
            <h1 className="text-3xl font-bold text-neutral-900">{department.name}</h1>
            <p className="text-neutral-600 mt-1">{department.description}</p>
          </div>
          <div className="flex gap-2">
            {!isEditMode && (
              <>
                <Button variant="outline" onClick={() => setIsEditMode(true)}>
                  <Edit2 className="w-4 h-4 mr-2" />
                  Edit
                </Button>
                <Button variant="danger" onClick={() => setIsDeleteConfirmOpen(true)}>
                  <Trash2 className="w-4 h-4 mr-2" />
                  Delete
                </Button>
              </>
            )}
          </div>
        </div>

        {/* Details Section */}
        {isEditMode ? (
          <Card>
            <div className="space-y-4">
              <Input
                label="Department Name"
                value={formData.name}
                onChange={(e) => {
                  setFormData({ ...formData, name: e.target.value })
                  if (errors.name) setErrors({ ...errors, name: '' })
                }}
                error={errors.name}
                disabled={isSubmitting}
              />
              <Input
                label="Description"
                value={formData.description}
                onChange={(e) => {
                  setFormData({ ...formData, description: e.target.value })
                  if (errors.description) setErrors({ ...errors, description: '' })
                }}
                error={errors.description}
                disabled={isSubmitting}
              />
              <div className="flex gap-4 pt-4 border-t border-neutral-200">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => setIsEditMode(false)}
                  disabled={isSubmitting}
                >
                  Cancel
                </Button>
                <Button onClick={handleSave} disabled={isSubmitting}>
                  {isSubmitting ? 'Saving...' : 'Save Changes'}
                </Button>
              </div>
            </div>
          </Card>
        ) : (
          <div className="grid grid-cols-3 gap-6">
            <Card>
              <div className="pb-4 border-b border-neutral-200 mb-4">
                <h3 className="font-semibold text-neutral-900">Information</h3>
              </div>
              <div className="space-y-4">
                <div>
                  <p className="text-sm text-neutral-600">Created On</p>
                  <p className="text-lg font-medium text-neutral-900 mt-1">
                    {formatDate(department.createdAtUtc)}
                  </p>
                </div>
                {department.lastModifiedAtUtc && (
                  <div>
                    <p className="text-sm text-neutral-600">Last Modified</p>
                    <p className="text-lg font-medium text-neutral-900 mt-1">
                      {formatDate(department.lastModifiedAtUtc)}
                    </p>
                  </div>
                )}
              </div>
            </Card>

            <Card className="col-span-2">
              <div className="pb-4 border-b border-neutral-200 mb-4">
                <h3 className="font-semibold text-neutral-900">Department Employees</h3>
              </div>
              {employees.length === 0 ? (
                <p className="text-neutral-500 text-center py-6">No employees in this department</p>
              ) : (
                <div className="space-y-2">
                  {employees.slice(0, 5).map((emp: any, idx) => (
                    <div key={idx} className="p-3 bg-neutral-50 rounded-lg">
                      <p className="font-medium text-neutral-900">{emp.firstName} {emp.lastName}</p>
                      <p className="text-sm text-neutral-600">{emp.email}</p>
                    </div>
                  ))}
                  {employees.length > 5 && (
                    <p className="text-neutral-600 text-sm pt-2">+ {employees.length - 5} more employees</p>
                  )}
                </div>
              )}
            </Card>
          </div>
        )}
      </div>

      {/* Delete Confirmation Modal */}
      {isDeleteConfirmOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
          <div className="absolute inset-0 bg-black bg-opacity-50" onClick={() => setIsDeleteConfirmOpen(false)} />
          <Card className="relative max-w-md w-full mx-4">
            <div className="pb-4 border-b border-neutral-200 mb-4">
              <h2 className="text-lg font-semibold text-neutral-900">Delete Department</h2>
            </div>
            <p className="text-neutral-600 mb-6">
              Are you sure you want to delete <strong>{department.name}</strong>? This action cannot be undone.
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
