import { useState, useEffect, useMemo, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Input from '@/components/Common/Input'
import Badge from '@/components/Common/Badge'
import Modal from '@/components/Common/Modal'
import { Plus, Search, Edit2, Trash2, Eye, ChevronLeft, ChevronRight, UserPlus, CheckCircle, XCircle } from 'lucide-react'
import { departmentsApi } from '@/api/departments'
import type { Department, StaffingRequest } from '@/types'
import { useAuthStore } from '@/store/authStore'
import { formatDateIST } from '@/utils/format'
import toast from 'react-hot-toast'

const ITEMS_PER_PAGE_OPTIONS = [10, 25, 50, 100]

export default function DepartmentsPage() {
  const navigate = useNavigate()
  const { user } = useAuthStore()
  const isAdmin = user?.role === 'Admin'
  const isSupervisor = user?.role === 'Admin' || user?.role === 'Manager' || user?.role === 'Team Lead' || user?.role === 'TeamLead' || user?.role === 'HR'

  const [activeTab, setActiveTab] = useState<'departments' | 'staffing'>('departments')
  const [departments, setDepartments] = useState<Department[]>([])
  const [staffingRequests, setStaffingRequests] = useState<StaffingRequest[]>([])
  const [searchTerm, setSearchTerm] = useState('')
  const [sortBy, setSortBy] = useState<keyof Department>('name')
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc')
  const [currentPage, setCurrentPage] = useState(1)
  const [itemsPerPage, setItemsPerPage] = useState(10)

  // Department Modal State
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [selectedDepartment, setSelectedDepartment] = useState<Department | null>(null)
  const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})
  const [formData, setFormData] = useState({
    name: '',
    description: '',
  })

  // Staffing Request Modal State
  const [isStaffingModalOpen, setIsStaffingModalOpen] = useState(false)
  const [staffingForm, setStaffingForm] = useState({
    departmentId: '',
    requiredCount: 1,
    reason: '',
  })
  const [isSubmittingStaffing, setIsSubmittingStaffing] = useState(false)

  const loadDepartments = useCallback(async () => {
    try {
      const response = await departmentsApi.getAllSimple()
      setDepartments(response)
      setCurrentPage(1)
    } catch (error) {
      toast.error('Failed to load departments')
    }
  }, [])

  const loadStaffingRequests = useCallback(async () => {
    try {
      const requests = await departmentsApi.getStaffingRequests()
      setStaffingRequests(requests)
    } catch (error) {
      console.error('Failed to load staffing requests:', error)
    }
  }, [])

  useEffect(() => {
    loadDepartments()
    loadStaffingRequests()
  }, [loadDepartments, loadStaffingRequests])

  // Filter and sort departments
  const filteredAndSortedDepartments = useMemo(() => {
    let filtered = departments

    if (searchTerm) {
      const term = searchTerm.toLowerCase()
      filtered = filtered.filter(
        (dept) =>
          dept.name.toLowerCase().includes(term) ||
          dept.description.toLowerCase().includes(term)
      )
    }

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
  }, [departments, searchTerm, sortBy, sortOrder])

  // Paginate
  const totalPages = Math.ceil(filteredAndSortedDepartments.length / itemsPerPage) || 1
  const paginatedDepartments = filteredAndSortedDepartments.slice(
    (currentPage - 1) * itemsPerPage,
    currentPage * itemsPerPage
  )

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {}

    if (!formData.name.trim()) newErrors.name = 'Department name is required'
    if (!formData.description.trim()) newErrors.description = 'Description is required'

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleOpenCreate = () => {
    setSelectedDepartment(null)
    setFormData({ name: '', description: '' })
    setErrors({})
    setIsModalOpen(true)
  }

  const handleOpenEdit = (dept: Department) => {
    setSelectedDepartment(dept)
    setFormData({ name: dept.name, description: dept.description })
    setErrors({})
    setIsModalOpen(true)
  }

  const handleViewDetails = (id: string) => {
    navigate(`/departments/${id}`)
  }

  const handleSave = async () => {
    if (!validateForm()) return

    try {
      setIsSubmitting(true)
      if (selectedDepartment) {
        await departmentsApi.update(selectedDepartment.id, {
          name: formData.name,
          description: formData.description,
        })
        toast.success('Department updated successfully')
      } else {
        await departmentsApi.create({
          name: formData.name,
          description: formData.description,
        })
        toast.success('Department created successfully')
      }
      setIsModalOpen(false)
      loadDepartments()
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to save department')
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
      await departmentsApi.delete(deleteTarget)
      toast.success('Department deleted successfully')
      setIsDeleteConfirmOpen(false)
      setDeleteTarget(null)
      loadDepartments()
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to delete department')
    }
  }

  const handleCreateStaffingRequest = async () => {
    if (!staffingForm.departmentId) {
      toast.error('Please select a department')
      return
    }
    if (!staffingForm.reason.trim()) {
      toast.error('Please provide a reason')
      return
    }

    try {
      setIsSubmittingStaffing(true)
      await departmentsApi.createStaffingRequest({
        departmentId: staffingForm.departmentId,
        requiredCount: Number(staffingForm.requiredCount),
        reason: staffingForm.reason,
      })
      toast.success('Staffing request submitted to Administrator successfully!')
      setIsStaffingModalOpen(false)
      setStaffingForm({ departmentId: '', requiredCount: 1, reason: '' })
      loadStaffingRequests()
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to submit staffing request')
    } finally {
      setIsSubmittingStaffing(false)
    }
  }

  const handleResolveStaffing = async (id: string, approve: boolean) => {
    try {
      await departmentsApi.resolveStaffingRequest(id, { approve })
      toast.success(`Staffing request ${approve ? 'approved' : 'rejected'}`)
      loadStaffingRequests()
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to resolve staffing request')
    }
  }

  const handleSort = (column: keyof Department) => {
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
        <div className="flex justify-between items-center flex-wrap gap-4">
          <div>
            <h1 className="text-3xl font-bold text-neutral-900">Departments</h1>
            <p className="text-neutral-600 mt-1">Manage organizational departments and staffing requirements</p>
          </div>
          <div className="flex gap-2">
            {isSupervisor && (
              <Button variant="outline" onClick={() => setIsStaffingModalOpen(true)}>
                <UserPlus className="w-4 h-4 mr-2" />
                Raise Staffing Request
              </Button>
            )}
            {isAdmin && (
              <Button onClick={handleOpenCreate}>
                <Plus className="w-4 h-4 mr-2" />
                Add Department
              </Button>
            )}
          </div>
        </div>

        {/* Tab Switcher */}
        <div className="flex border-b border-neutral-200 gap-4">
          <button
            onClick={() => setActiveTab('departments')}
            className={`pb-3 text-sm font-semibold border-b-2 transition ${
              activeTab === 'departments'
                ? 'border-primary-600 text-primary-600'
                : 'border-transparent text-neutral-500 hover:text-neutral-700'
            }`}
          >
            Departments ({departments.length})
          </button>
          <button
            onClick={() => setActiveTab('staffing')}
            className={`pb-3 text-sm font-semibold border-b-2 transition ${
              activeTab === 'staffing'
                ? 'border-primary-600 text-primary-600'
                : 'border-transparent text-neutral-500 hover:text-neutral-700'
            }`}
          >
            Staffing Requests ({staffingRequests.length})
          </button>
        </div>

        {activeTab === 'departments' ? (
          <>
            {/* Search and Filters */}
            <Card>
              <div className="flex gap-4">
                <div className="flex-1">
                  <Input
                    icon={<Search className="w-5 h-5" />}
                    placeholder="Search by name or description..."
                    value={searchTerm}
                    onChange={(e) => {
                      setSearchTerm(e.target.value)
                      setCurrentPage(1)
                    }}
                  />
                </div>
                <div className="w-40">
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
            </Card>

            {/* Table */}
            <Card>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-neutral-200 bg-neutral-50">
                      <th
                        onClick={() => handleSort('name')}
                        className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider cursor-pointer hover:bg-neutral-100"
                      >
                        Department Name
                      </th>
                      <th
                        onClick={() => handleSort('description')}
                        className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider cursor-pointer hover:bg-neutral-100"
                      >
                        Description
                      </th>
                      <th
                        onClick={() => handleSort('createdAtUtc')}
                        className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider cursor-pointer hover:bg-neutral-100"
                      >
                        Created
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                        Actions
                      </th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-neutral-200">
                    {paginatedDepartments.length === 0 ? (
                      <tr>
                        <td colSpan={4} className="px-6 py-8 text-center text-neutral-500">
                          No departments found.
                        </td>
                      </tr>
                    ) : (
                      paginatedDepartments.map((dept) => (
                        <tr key={dept.id} className="hover:bg-neutral-50 transition-colors">
                          <td className="px-6 py-4 text-sm font-semibold text-neutral-900">{dept.name}</td>
                          <td className="px-6 py-4 text-sm text-neutral-600 max-w-md truncate">{dept.description}</td>
                          <td className="px-6 py-4 text-sm text-neutral-600">{formatDateIST(dept.createdAtUtc)}</td>
                          <td className="px-6 py-4 text-sm">
                            <div className="flex gap-2">
                              <button
                                onClick={() => handleViewDetails(dept.id)}
                                className="p-1 hover:bg-blue-100 rounded text-blue-600 transition-colors"
                                title="View Details"
                              >
                                <Eye className="w-4 h-4" />
                              </button>
                              {isAdmin && (
                                <>
                                  <button
                                    onClick={() => handleOpenEdit(dept)}
                                    className="p-1 hover:bg-amber-100 rounded text-amber-600 transition-colors"
                                    title="Edit"
                                  >
                                    <Edit2 className="w-4 h-4" />
                                  </button>
                                  <button
                                    onClick={() => handleDeleteClick(dept.id)}
                                    className="p-1 hover:bg-red-100 rounded text-red-600 transition-colors"
                                    title="Delete"
                                  >
                                    <Trash2 className="w-4 h-4" />
                                  </button>
                                </>
                              )}
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
                  Showing {(currentPage - 1) * itemsPerPage + 1} to {Math.min(currentPage * itemsPerPage, filteredAndSortedDepartments.length)} of{' '}
                  {filteredAndSortedDepartments.length}
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
          </>
        ) : (
          /* Staffing Requests Tab */
          <Card>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-neutral-200 bg-neutral-50">
                    <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                      Department
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                      Current Headcount
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                      Requested Additional
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                      Reason / Justification
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                      Status
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                      Requested Date
                    </th>
                    {isAdmin && (
                      <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                        Action
                      </th>
                    )}
                  </tr>
                </thead>
                <tbody className="divide-y divide-neutral-200">
                  {staffingRequests.length === 0 ? (
                    <tr>
                      <td colSpan={isAdmin ? 7 : 6} className="px-6 py-8 text-center text-neutral-500">
                        No staffing requests found.
                      </td>
                    </tr>
                  ) : (
                    staffingRequests.map((req) => (
                      <tr key={req.id} className="hover:bg-neutral-50 transition-colors">
                        <td className="px-6 py-4 text-sm font-semibold text-neutral-900">{req.departmentName}</td>
                        <td className="px-6 py-4 text-sm text-neutral-600">{req.currentHeadcount} employees</td>
                        <td className="px-6 py-4 text-sm font-medium text-primary-600">+{req.requiredCount}</td>
                        <td className="px-6 py-4 text-sm text-neutral-600 max-w-sm">{req.reason}</td>
                        <td className="px-6 py-4 text-sm">
                          <Badge
                            label={req.status}
                            variant={
                              req.status === 'Approved'
                                ? 'success'
                                : req.status === 'Pending'
                                  ? 'warning'
                                  : 'danger'
                            }
                          />
                        </td>
                        <td className="px-6 py-4 text-sm text-neutral-600">{formatDateIST(req.createdAtUtc)}</td>
                        {isAdmin && (
                          <td className="px-6 py-4 text-sm">
                            {req.status === 'Pending' && (
                              <div className="flex gap-2">
                                <Button size="sm" onClick={() => handleResolveStaffing(req.id, true)}>
                                  <CheckCircle className="w-4 h-4 mr-1" />
                                  Approve
                                </Button>
                                <Button size="sm" variant="outline" onClick={() => handleResolveStaffing(req.id, false)}>
                                  <XCircle className="w-4 h-4 mr-1 text-red-600" />
                                  Reject
                                </Button>
                              </div>
                            )}
                          </td>
                        )}
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </Card>
        )}
      </div>

      {/* Department Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={selectedDepartment ? 'Edit Department' : 'Add New Department'}
        footer={
          <>
            <Button variant="outline" onClick={() => setIsModalOpen(false)} disabled={isSubmitting}>
              Cancel
            </Button>
            <Button onClick={handleSave} disabled={isSubmitting}>
              {isSubmitting ? 'Saving...' : 'Save Department'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <Input
            label="Department Name *"
            value={formData.name}
            onChange={(e) => {
              setFormData({ ...formData, name: e.target.value })
              if (errors.name) setErrors({ ...errors, name: '' })
            }}
            error={errors.name}
          />
          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Description *</label>
            <textarea
              rows={4}
              value={formData.description}
              onChange={(e) => {
                setFormData({ ...formData, description: e.target.value })
                if (errors.description) setErrors({ ...errors, description: '' })
              }}
              className={`w-full px-4 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white ${
                errors.description ? 'border-red-500' : 'border-neutral-300'
              }`}
            />
            {errors.description && <p className="text-red-500 text-xs mt-1">{errors.description}</p>}
          </div>
        </div>
      </Modal>

      {/* Raise Staffing Request Modal */}
      <Modal
        isOpen={isStaffingModalOpen}
        onClose={() => setIsStaffingModalOpen(false)}
        title="Raise Department Staffing Request"
        footer={
          <>
            <Button variant="outline" onClick={() => setIsStaffingModalOpen(false)} disabled={isSubmittingStaffing}>
              Cancel
            </Button>
            <Button onClick={handleCreateStaffingRequest} disabled={isSubmittingStaffing}>
              {isSubmittingStaffing ? 'Submitting...' : 'Submit Request'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Target Department *</label>
            <select
              value={staffingForm.departmentId}
              onChange={(e) => setStaffingForm({ ...staffingForm, departmentId: e.target.value })}
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white"
            >
              <option value="">Select Department</option>
              {departments.map((dept) => (
                <option key={dept.id} value={dept.id}>
                  {dept.name}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Required Additional Staff Count *</label>
            <input
              type="number"
              min={1}
              max={100}
              value={staffingForm.requiredCount}
              onChange={(e) => setStaffingForm({ ...staffingForm, requiredCount: parseInt(e.target.value) || 1 })}
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Business Reason & Workload Justification *</label>
            <textarea
              rows={4}
              placeholder="e.g., Project workload increased due to upcoming Q3 deliverables."
              value={staffingForm.reason}
              onChange={(e) => setStaffingForm({ ...staffingForm, reason: e.target.value })}
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white"
            />
          </div>
        </div>
      </Modal>

      {/* Delete Confirmation Modal */}
      <Modal
        isOpen={isDeleteConfirmOpen}
        onClose={() => setIsDeleteConfirmOpen(false)}
        title="Delete Department"
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
        <p className="text-neutral-600">Are you sure you want to delete this department? This action cannot be undone.</p>
      </Modal>
    </MainLayout>
  )
}
