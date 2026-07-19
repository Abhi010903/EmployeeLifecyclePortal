import { useState, useEffect, useMemo } from 'react'
import { useNavigate } from 'react-router-dom'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Input from '@/components/Common/Input'
import Badge from '@/components/Common/Badge'
import Modal from '@/components/Common/Modal'
import { Plus, Search, Edit2, Trash2, Eye, ChevronLeft, ChevronRight } from 'lucide-react'
import { employeesApi } from '@/api/employees'
import type { Employee } from '@/types'
import toast from 'react-hot-toast'

const ITEMS_PER_PAGE_OPTIONS = [10, 25, 50, 100]
const STATUSES = ['Active', 'Inactive', 'Terminated']

export default function EmployeesPage() {
  const navigate = useNavigate()
  const [employees, setEmployees] = useState<Employee[]>([])
  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState<string>('All')
  const [sortBy, setSortBy] = useState<keyof Employee>('firstName')
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc')
  const [currentPage, setCurrentPage] = useState(1)
  const [itemsPerPage, setItemsPerPage] = useState(10)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [selectedEmployee, setSelectedEmployee] = useState<Employee | null>(null)
  const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<string | null>(null)
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
    loadEmployees()
  }, [])

  const loadEmployees = async () => {
    try {
      const response = await employeesApi.getAll(1, 1000)
      setEmployees(response.items)
      setCurrentPage(1)
    } catch (error) {
      toast.error('Failed to load employees')
    }
  }

  const filteredAndSortedEmployees = useMemo(() => {
    let filtered = employees

    // Apply search filter
    if (searchTerm) {
      const term = searchTerm.toLowerCase()
      filtered = filtered.filter(
        (emp) =>
          `${emp.firstName} ${emp.lastName}`.toLowerCase().includes(term) ||
          emp.email.toLowerCase().includes(term) ||
          emp.employeeCode.toLowerCase().includes(term)
      )
    }

    // Apply status filter
    if (statusFilter !== 'All') {
      filtered = filtered.filter((emp) => emp.status === statusFilter)
    }

    // Apply sorting
    filtered.sort((a, b) => {
      const aValue = a[sortBy]
      const bValue = b[sortBy]

      if (typeof aValue === 'string' && typeof bValue === 'string') {
        return sortOrder === 'asc' ? aValue.localeCompare(bValue) : bValue.localeCompare(aValue)
      }

      if (aValue === undefined || bValue === undefined) return 0
      return sortOrder === 'asc' ? (aValue > bValue ? 1 : -1) : bValue > aValue ? 1 : -1
    })

    return filtered
  }, [employees, searchTerm, statusFilter, sortBy, sortOrder])

  // Paginate
  const totalPages = Math.ceil(filteredAndSortedEmployees.length / itemsPerPage)
  const paginatedEmployees = filteredAndSortedEmployees.slice(
    (currentPage - 1) * itemsPerPage,
    currentPage * itemsPerPage
  )

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

  const handleOpenCreate = () => {
    setSelectedEmployee(null)
    setFormData({
      firstName: '',
      lastName: '',
      email: '',
      phoneNumber: '',
      departmentId: '',
    })
    setErrors({})
    setIsModalOpen(true)
  }

  const handleOpenEdit = (employee: Employee) => {
    setSelectedEmployee(employee)
    setFormData({
      firstName: employee.firstName,
      lastName: employee.lastName,
      email: employee.email,
      phoneNumber: employee.phoneNumber || '',
      departmentId: employee.departmentId,
    })
    setErrors({})
    setIsModalOpen(true)
  }

  const handleViewDetails = (id: string) => {
    navigate(`/employees/${id}`)
  }

  const handleSave = async () => {
    if (!validateForm()) return

    try {
      setIsSubmitting(true)
      if (selectedEmployee) {
        await employeesApi.update(selectedEmployee.id, {
          firstName: formData.firstName,
          lastName: formData.lastName,
          email: formData.email,
          phoneNumber: formData.phoneNumber,
          departmentId: formData.departmentId,
        } as Partial<Employee>)
        toast.success('Employee updated successfully')
      } else {
        await employeesApi.create({
          firstName: formData.firstName,
          lastName: formData.lastName,
          email: formData.email,
          phoneNumber: formData.phoneNumber,
          departmentId: formData.departmentId,
        } as any)
        toast.success('Employee created successfully')
      }
      setIsModalOpen(false)
      loadEmployees()
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to save employee')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleDeleteClick = (id: string) => {
    setDeleteTarget(id)
    setIsDeleteConfirmOpen(true)
  }

  const handleConfirmDelete = async () => {
    if (!deleteTarget) return

    try {
      await employeesApi.delete(deleteTarget)
      toast.success('Employee deleted successfully')
      setIsDeleteConfirmOpen(false)
      setDeleteTarget(null)
      loadEmployees()
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to delete employee')
    }
  }

  const handleSort = (column: keyof Employee) => {
    if (sortBy === column) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc')
    } else {
      setSortBy(column)
      setSortOrder('asc')
    }
  }

  const formatDate = (date: string) => {
    return new Intl.DateTimeFormat('en-IN', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    }).format(new Date(date))
  }

  return (
    <MainLayout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold text-neutral-900">Employees</h1>
            <p className="text-neutral-600 mt-1">Manage your workforce ({filteredAndSortedEmployees.length} total)</p>
          </div>
          <Button onClick={handleOpenCreate}>
            <Plus className="w-4 h-4 mr-2" />
            Add Employee
          </Button>
        </div>

        {/* Search and Filters */}
        <Card>
          <div className="space-y-4">
            <div className="flex gap-4">
              <div className="flex-1">
                <Input
                  icon={<Search className="w-5 h-5" />}
                  placeholder="Search by name, email, or code..."
                  value={searchTerm}
                  onChange={(e) => {
                    setSearchTerm(e.target.value)
                    setCurrentPage(1)
                  }}
                />
              </div>
              <select
                value={statusFilter}
                onChange={(e) => {
                  setStatusFilter(e.target.value)
                  setCurrentPage(1)
                }}
                className="px-4 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500"
              >
                <option value="All">All Status</option>
                {STATUSES.map((status) => (
                  <option key={status} value={status}>
                    {status}
                  </option>
                ))}
              </select>
              <select
                value={itemsPerPage}
                onChange={(e) => {
                  setItemsPerPage(Number(e.target.value))
                  setCurrentPage(1)
                }}
                className="px-4 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500"
              >
                {ITEMS_PER_PAGE_OPTIONS.map((size) => (
                  <option key={size} value={size}>
                    {size} per page
                  </option>
                ))}
              </select>
            </div>
          </div>
        </Card>

        {/* Employees Table */}
        <Card>
          {paginatedEmployees.length === 0 ? (
            <div className="py-12 text-center">
              <p className="text-neutral-500">No employees found</p>
            </div>
          ) : (
            <>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-neutral-200">
                      <th className="px-6 py-3 text-left text-xs font-medium text-neutral-600 uppercase tracking-wider cursor-pointer hover:bg-neutral-50" onClick={() => handleSort('employeeCode')}>
                        Code {sortBy === 'employeeCode' && (sortOrder === 'asc' ? '↑' : '↓')}
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-neutral-600 uppercase tracking-wider cursor-pointer hover:bg-neutral-50" onClick={() => handleSort('firstName')}>
                        Name {sortBy === 'firstName' && (sortOrder === 'asc' ? '↑' : '↓')}
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-neutral-600 uppercase tracking-wider cursor-pointer hover:bg-neutral-50" onClick={() => handleSort('email')}>
                        Email {sortBy === 'email' && (sortOrder === 'asc' ? '↑' : '↓')}
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-neutral-600 uppercase tracking-wider">Phone</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-neutral-600 uppercase tracking-wider cursor-pointer hover:bg-neutral-50" onClick={() => handleSort('status')}>
                        Status {sortBy === 'status' && (sortOrder === 'asc' ? '↑' : '↓')}
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-neutral-600 uppercase tracking-wider">Created</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-neutral-600 uppercase tracking-wider">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {paginatedEmployees.map((employee) => (
                      <tr key={employee.id} className="border-b border-neutral-200 hover:bg-neutral-50">
                        <td className="px-6 py-4 text-sm text-neutral-900">{employee.employeeCode}</td>
                        <td className="px-6 py-4 text-sm font-medium text-neutral-900">
                          {employee.firstName} {employee.lastName}
                        </td>
                        <td className="px-6 py-4 text-sm text-neutral-600">{employee.email}</td>
                        <td className="px-6 py-4 text-sm text-neutral-600">{employee.phoneNumber || '-'}</td>
                        <td className="px-6 py-4 text-sm">
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
                        </td>
                        <td className="px-6 py-4 text-sm text-neutral-600">{formatDate(employee.createdAtUtc)}</td>
                        <td className="px-6 py-4 text-sm">
                          <div className="flex gap-2">
                            <button
                              onClick={() => handleViewDetails(employee.id)}
                              className="p-1 hover:bg-blue-100 rounded text-blue-600 transition-colors"
                              title="View Details"
                            >
                              <Eye className="w-4 h-4" />
                            </button>
                            <button
                              onClick={() => handleOpenEdit(employee)}
                              className="p-1 hover:bg-amber-100 rounded text-amber-600 transition-colors"
                              title="Edit"
                            >
                              <Edit2 className="w-4 h-4" />
                            </button>
                            <button
                              onClick={() => handleDeleteClick(employee.id)}
                              className="p-1 hover:bg-red-100 rounded text-red-600 transition-colors"
                              title="Delete"
                            >
                              <Trash2 className="w-4 h-4" />
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {/* Pagination */}
              <div className="flex items-center justify-between mt-4 pt-4 border-t border-neutral-200">
                <div className="text-sm text-neutral-600">
                  Showing {(currentPage - 1) * itemsPerPage + 1} to {Math.min(currentPage * itemsPerPage, filteredAndSortedEmployees.length)} of{' '}
                  {filteredAndSortedEmployees.length}
                </div>
                <div className="flex gap-2">
                  <button
                    onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                    disabled={currentPage === 1}
                    className="p-2 hover:bg-neutral-100 rounded text-neutral-600 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <ChevronLeft className="w-4 h-4" />
                  </button>
                  <div className="flex items-center gap-1">
                    {Array.from({ length: totalPages }, (_, i) => i + 1).map((page) => (
                      <button
                        key={page}
                        onClick={() => setCurrentPage(page)}
                        className={`px-3 py-1 rounded text-sm ${
                          currentPage === page
                            ? 'bg-primary-600 text-white'
                            : 'hover:bg-neutral-100 text-neutral-600'
                        }`}
                      >
                        {page}
                      </button>
                    ))}
                  </div>
                  <button
                    onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
                    disabled={currentPage === totalPages}
                    className="p-2 hover:bg-neutral-100 rounded text-neutral-600 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <ChevronRight className="w-4 h-4" />
                  </button>
                </div>
              </div>
            </>
          )}
        </Card>
      </div>

      {/* Create/Edit Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={selectedEmployee ? 'Edit Employee' : 'Add Employee'}
        footer={
          <>
            <Button variant="outline" onClick={() => setIsModalOpen(false)} disabled={isSubmitting}>
              Cancel
            </Button>
            <Button onClick={handleSave} disabled={isSubmitting}>
              {isSubmitting ? 'Saving...' : 'Save'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <Input
                label="First Name"
                value={formData.firstName}
                onChange={(e) => {
                  setFormData({ ...formData, firstName: e.target.value })
                  if (errors.firstName) setErrors({ ...errors, firstName: '' })
                }}
                error={errors.firstName}
              />
            </div>
            <div>
              <Input
                label="Last Name"
                value={formData.lastName}
                onChange={(e) => {
                  setFormData({ ...formData, lastName: e.target.value })
                  if (errors.lastName) setErrors({ ...errors, lastName: '' })
                }}
                error={errors.lastName}
              />
            </div>
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
          />

          <Input
            label="Phone Number"
            value={formData.phoneNumber}
            onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
          />

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-2">Department</label>
            <select
              value={formData.departmentId}
              onChange={(e) => {
                setFormData({ ...formData, departmentId: e.target.value })
                if (errors.departmentId) setErrors({ ...errors, departmentId: '' })
              }}
              className={`w-full px-4 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 ${
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
        </div>
      </Modal>

      {/* Delete Confirmation Modal */}
      <Modal
        isOpen={isDeleteConfirmOpen}
        onClose={() => setIsDeleteConfirmOpen(false)}
        title="Delete Employee"
        footer={
          <>
            <Button variant="outline" onClick={() => setIsDeleteConfirmOpen(false)}>
              Cancel
            </Button>
            <Button variant="danger" onClick={handleConfirmDelete}>
              Delete
            </Button>
          </>
        }
      >
        <p className="text-neutral-600">Are you sure you want to delete this employee? This action cannot be undone.</p>
      </Modal>
    </MainLayout>
  )
}
