import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Input from '@/components/Common/Input'
import { ArrowLeft } from 'lucide-react'
import { employeesApi } from '@/api/employees'
import type { Employee } from '@/types'
import toast from 'react-hot-toast'

export default function EmployeeEditPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    departmentId: '',
  })

  useEffect(() => {
    if (id) {
      loadEmployee()
    }
  }, [id])

  const loadEmployee = async () => {
    try {
      setIsLoading(true)
      const employee = await employeesApi.getById(id!)
      setFormData({
        firstName: employee.firstName,
        lastName: employee.lastName,
        email: employee.email,
        phoneNumber: employee.phoneNumber || '',
        departmentId: employee.departmentId,
      })
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to load employee')
      navigate('/employees')
    } finally {
      setIsLoading(false)
    }
  }

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {}

    if (!formData.firstName.trim()) newErrors.firstName = 'First name is required'
    if (!formData.lastName.trim()) newErrors.lastName = 'Last name is required'
    if (!formData.email.trim()) newErrors.email = 'Email is required'
    if (formData.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = 'Invalid email format'
    }
    if (!formData.departmentId.trim()) newErrors.departmentId = 'Department is required'

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!validateForm()) return

    try {
      setIsSubmitting(true)
      await employeesApi.update(id!, {
        firstName: formData.firstName,
        lastName: formData.lastName,
        email: formData.email,
        phoneNumber: formData.phoneNumber,
        departmentId: formData.departmentId,
      } as Partial<Employee>)
      toast.success('Employee updated successfully')
      navigate(`/employees/${id}`)
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to update employee')
    } finally {
      setIsSubmitting(false)
    }
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

  return (
    <MainLayout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center gap-4">
          <button
            onClick={() => navigate(`/employees/${id}`)}
            className="p-2 hover:bg-neutral-100 rounded-lg transition-colors"
          >
            <ArrowLeft className="w-5 h-5" />
          </button>
          <div>
            <h1 className="text-3xl font-bold text-neutral-900">Edit Employee</h1>
            <p className="text-neutral-600 mt-1">Update employee information</p>
          </div>
        </div>

        {/* Form */}
        <Card>
          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="grid grid-cols-2 gap-6">
              <Input
                label="First Name"
                value={formData.firstName}
                onChange={(e) => {
                  setFormData({ ...formData, firstName: e.target.value })
                  if (errors.firstName) setErrors({ ...errors, firstName: '' })
                }}
                error={errors.firstName}
                disabled={isSubmitting}
              />

              <Input
                label="Last Name"
                value={formData.lastName}
                onChange={(e) => {
                  setFormData({ ...formData, lastName: e.target.value })
                  if (errors.lastName) setErrors({ ...errors, lastName: '' })
                }}
                error={errors.lastName}
                disabled={isSubmitting}
              />
            </div>

            <Input
              label="Email"
              type="email"
              value={formData.email}
              onChange={(e) => {
                setFormData({ ...formData, email: e.target.value })
                if (errors.email) setErrors({ ...errors, email: '' })
              }}
              error={errors.email}
              disabled={isSubmitting}
            />

            <Input
              label="Phone Number"
              value={formData.phoneNumber}
              onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
              disabled={isSubmitting}
            />

            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-2">Department</label>
              <select
                value={formData.departmentId}
                onChange={(e) => {
                  setFormData({ ...formData, departmentId: e.target.value })
                  if (errors.departmentId) setErrors({ ...errors, departmentId: '' })
                }}
                disabled={isSubmitting}
                className={`w-full px-4 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 disabled:bg-neutral-50 ${
                  errors.departmentId ? 'border-red-500' : 'border-neutral-300'
                }`}
              >
                <option value="">Select Department</option>
                {/* TODO: Load departments from API */}
                <option value="550e8400-e29b-41d4-a716-446655440000">Engineering</option>
                <option value="550e8400-e29b-41d4-a716-446655440001">HR</option>
                <option value="550e8400-e29b-41d4-a716-446655440002">Finance</option>
              </select>
              {errors.departmentId && <p className="text-red-500 text-xs mt-1">{errors.departmentId}</p>}
            </div>

            <div className="flex gap-4 pt-4 border-t border-neutral-200">
              <Button
                type="button"
                variant="outline"
                onClick={() => navigate(`/employees/${id}`)}
                disabled={isSubmitting}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? 'Saving...' : 'Save Changes'}
              </Button>
            </div>
          </form>
        </Card>
      </div>
    </MainLayout>
  )
}
