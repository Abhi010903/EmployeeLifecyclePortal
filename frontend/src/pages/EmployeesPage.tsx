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
import { departmentsApi } from '@/api/departments'
import { rolesApi } from '@/api/roles'
import type { Employee, Department, Role } from '@/types'
import { formatDateIST } from '@/utils/format'
import toast from 'react-hot-toast'

const ITEMS_PER_PAGE_OPTIONS = [10, 25, 50, 100]
const STATUSES = ['Active', 'Inactive', 'Terminated']

export default function EmployeesPage() {
  const navigate = useNavigate()
  const [employees, setEmployees] = useState<Employee[]>([])
  const [departments, setDepartments] = useState<Department[]>([])
  const [roles, setRoles] = useState<Role[]>([])
  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState<string>('All')
  const [departmentFilter, setDepartmentFilter] = useState<string>('All')
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
    roleId: '',
    managerId: '',
    teamLeadId: '',
  })

  useEffect(() => {
    loadEmployees()
    loadDepartments()
    loadRoles()
  }, [])

  const loadDepartments = async () => {
    try {
      const depts = await departmentsApi.getAllSimple()
      setDepartments(Array.isArray(depts) ? depts : [])
    } catch (error) {
      console.error('Failed to load departments:', error)
    }
  }

  const loadRoles = async () => {
    try {
      const allRoles = await rolesApi.getAllSimple()
      setRoles(Array.isArray(allRoles) ? allRoles : [])
    } catch (error) {
      console.error('Failed to load roles:', error)
    }
  }

  const loadEmployees = async () => {
    try {
      const response = await employeesApi.getAll(1, 1000)
      const list = Array.isArray(response) ? response : (response?.items || [])
      setEmployees(list)
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

    // Apply department filter
    if (departmentFilter !== 'All') {
      filtered = filtered.filter((emp) => emp.departmentId === departmentFilter)
    }

    // Apply sorting
    filtered.sort((a, b) => {
      let aVal = a[sortBy] ?? ''
      let bVal = b[sortBy] ?? ''
      if (typeof aVal === 'string') aVal = aVal.toLowerCase()
      if (typeof bVal === 'string') bVal = bVal.toLowerCase()
      if (aVal < bVal) return sortOrder === 'asc' ? -1 : 1
      if (aVal > bVal) return sortOrder === 'asc' ? 1 : -1
      return 0
    })

    return filtered
  }, [employees, searchTerm, statusFilter, departmentFilter, sortBy, sortOrder])

  const totalPages = Math.ceil(filteredAndSortedEmployees.length / itemsPerPage) || 1
  const paginatedEmployees = useMemo(() => {
    const start = (currentPage - 1) * itemsPerPage
    return filteredAndSortedEmployees.slice(start, start + itemsPerPage)
  }, [filteredAndSortedEmployees, currentPage, itemsPerPage])

  const validateForm = () => {
    const newErrors: Record<string, string> = {}
    if (!formData.firstName.trim()) newErrors.firstName = 'First name is required'
    if (!formData.lastName.trim()) newErrors.lastName = 'Last name is required'
    if (!formData.email.trim()) {
      newErrors.email = 'Email is required'
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = 'Invalid email address'
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
      roleId: '',
      managerId: '',
      teamLeadId: '',
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
      roleId: employee.roleId || '',
      managerId: employee.managerId || '',
      teamLeadId: employee.teamLeadId || '',
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
          roleId: formData.roleId || undefined,
          managerId: formData.managerId || undefined,
          teamLeadId: formData.teamLeadId || undefined,
        } as Partial<Employee>)
        toast.success('Employee updated successfully')
      } else {
        await employeesApi.create({
          employeeCode: 'EMP-' + Math.floor(100000 + Math.random() * 900000),
          firstName: formData.firstName,
          lastName: formData.lastName,
          email: formData.email,
          phoneNumber: formData.phoneNumber,
          departmentId: formData.departmentId,
          roleId: formData.roleId || undefined,
          managerId: formData.managerId || undefined,
          teamLeadId: formData.teamLeadId || undefined,
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
            <div className="flex gap-4 flex-wrap md:flex-nowrap">
              <div className="flex-1 min-w-[200px]">
                <Input
                  icon={<Search className="w-4 h-4" />}
                  placeholder="Search by name, email, code..."
                  value={searchTerm}
                  onChange={(e) => {
                    setSearchTerm(e.target.value)
                    setCurrentPage(1)
                  }}
                />
              </div>
              <div className="w-44">
                <select
                  value={departmentFilter}
                  onChange={(e) => {
                    setDepartmentFilter(e.target.value)
                    setCurrentPage(1)
                  }}
                  className="w-full px-4 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white"
                >
                  <option value="All">All Departments</option>
                  {departments.map((dept) => (
                    <option key={dept.id} value={dept.id}>
                      {dept.name}
                    </option>
                  ))}
                </select>
              </div>
              <div className="w-36">
                <select
                  value={statusFilter}
                  onChange={(e) => {
                    setStatusFilter(e.target.value)
                    setCurrentPage(1)
                  }}
                  className="w-full px-4 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white"
                >
                  <option value="All">All Status</option>
                  {STATUSES.map((status) => (
                    <option key={status} value={status}>
                      {status}
                    </option>
                  ))}
                </select>
              </div>
              <div className="w-28">
                <select
                  value={itemsPerPage}
                  onChange={(e) => {
                    setItemsPerPage(Number(e.target.value))
                    setCurrentPage(1)
                  }}
                  className="w-full px-4 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white"
                >
                  {ITEMS_PER_PAGE_OPTIONS.map((opt) => (
                    <option key={opt} value={opt}>
                      {opt} / page
                    </option>
                  ))}
                </select>
              </div>
            </div>
          </div>
        </Card>

        {/* Employees Table */}
        <Card>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-neutral-200 bg-neutral-50">
                  <th
                    onClick={() => handleSort('employeeCode')}
                    className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider cursor-pointer hover:bg-neutral-100"
                  >
                    Code
                  </th>
                  <th
                    onClick={() => handleSort('firstName')}
                    className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider cursor-pointer hover:bg-neutral-100"
                  >
                    Name
                  </th>
                  <th
                    onClick={() => handleSort('departmentName')}
                    className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider cursor-pointer hover:bg-neutral-100"
                  >
                    Department
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                    Role
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                    Reporting Manager / Lead
                  </th>
                  <th
                    onClick={() => handleSort('status')}
                    className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider cursor-pointer hover:bg-neutral-100"
                  >
                    Status
                  </th>
                  <th
                    onClick={() => handleSort('createdAtUtc')}
                    className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider cursor-pointer hover:bg-neutral-100"
                  >
                    Joined
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-neutral-200">
                {paginatedEmployees.length === 0 ? (
                  <tr>
                    <td colSpan={8} className="px-6 py-8 text-center text-neutral-500">
                      No employees found.
                    </td>
                  </tr>
                ) : (
                  paginatedEmployees.map((employee) => (
                    <tr key={employee.id} className="hover:bg-neutral-50 transition-colors">
                      <td className="px-6 py-4 text-sm font-medium text-primary-600">{employee.employeeCode}</td>
                      <td className="px-6 py-4 text-sm text-neutral-900">
                        <div className="font-semibold">{employee.firstName} {employee.lastName}</div>
                        <div className="text-xs text-neutral-500">{employee.email}</div>
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-600">{employee.departmentName || '-'}</td>
                      <td className="px-6 py-4 text-sm">
                        {employee.roles && employee.roles.length > 0 ? (
                          <div className="flex flex-wrap gap-1">
                            {employee.roles.map((r, i) => (
                              <Badge key={i} label={typeof r === 'string' ? r : r.name} variant="info" />
                            ))}
                          </div>
                        ) : employee.roleName ? (
                          <Badge label={employee.roleName} variant="info" />
                        ) : (
                          <span className="text-neutral-400 text-xs">No role</span>
                        )}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-600">
                        {employee.managerName ? (
                          <div className="text-xs">
                            <span className="font-medium text-neutral-800">Mgr:</span> {employee.managerName}
                          </div>
                        ) : null}
                        {employee.teamLeadName ? (
                          <div className="text-xs text-neutral-500">
                            <span className="font-medium text-neutral-800">Lead:</span> {employee.teamLeadName}
                          </div>
                        ) : null}
                        {!employee.managerName && !employee.teamLeadName && <span className="text-neutral-400 text-xs">-</span>}
                      </td>
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
                      <td className="px-6 py-4 text-sm text-neutral-600">{formatDateIST(employee.createdAtUtc)}</td>
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
                  ))
                )}
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
        </Card>
      </div>

      {/* Create/Edit Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={selectedEmployee ? 'Edit Employee' : 'Add New Employee'}
        footer={
          <>
            <Button variant="outline" onClick={() => setIsModalOpen(false)} disabled={isSubmitting}>
              Cancel
            </Button>
            <Button onClick={handleSave} disabled={isSubmitting}>
              {isSubmitting ? 'Saving...' : 'Save Employee'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <Input
                label="First Name *"
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
                label="Last Name *"
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
            label="Email *"
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

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-2">Department *</label>
              <select
                value={formData.departmentId}
                onChange={(e) => {
                  setFormData({ ...formData, departmentId: e.target.value })
                  if (errors.departmentId) setErrors({ ...errors, departmentId: '' })
                }}
                className={`w-full px-4 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white ${
                  errors.departmentId ? 'border-red-500' : 'border-neutral-300'
                }`}
              >
                <option value="">Select Department</option>
                {departments.map((dept) => (
                  <option key={dept.id} value={dept.id}>
                    {dept.name}
                  </option>
                ))}
              </select>
              {errors.departmentId && <p className="text-red-500 text-xs mt-1">{errors.departmentId}</p>}
            </div>

            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-2">Role / Designation</label>
              <select
                value={formData.roleId}
                onChange={(e) => setFormData({ ...formData, roleId: e.target.value })}
                className="w-full px-4 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white"
              >
                <option value="">Select Role (Optional)</option>
                {roles.map((role) => (
                  <option key={role.id} value={role.id}>
                    {role.name}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-2">Reporting Manager</label>
              <select
                value={formData.managerId}
                onChange={(e) => setFormData({ ...formData, managerId: e.target.value })}
                className="w-full px-4 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white"
              >
                <option value="">None / Direct Report</option>
                {employees
                  .filter((emp) => !selectedEmployee || emp.id !== selectedEmployee.id)
                  .map((emp) => (
                    <option key={emp.id} value={emp.id}>
                      {emp.firstName} {emp.lastName} ({emp.employeeCode})
                    </option>
                  ))}
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-2">Team Lead</label>
              <select
                value={formData.teamLeadId}
                onChange={(e) => setFormData({ ...formData, teamLeadId: e.target.value })}
                className="w-full px-4 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white"
              >
                <option value="">None</option>
                {employees
                  .filter((emp) => !selectedEmployee || emp.id !== selectedEmployee.id)
                  .map((emp) => (
                    <option key={emp.id} value={emp.id}>
                      {emp.firstName} {emp.lastName} ({emp.employeeCode})
                    </option>
                  ))}
              </select>
            </div>
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
