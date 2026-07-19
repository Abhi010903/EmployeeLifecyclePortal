import { useState, useEffect, useMemo } from 'react'
import { useNavigate } from 'react-router-dom'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Input from '@/components/Common/Input'
import Modal from '@/components/Common/Modal'
import { Plus, Search, Edit2, Trash2, Eye, ChevronLeft, ChevronRight } from 'lucide-react'
import { departmentsApi } from '@/api/departments'
import type { Department } from '@/types'
import toast from 'react-hot-toast'

const ITEMS_PER_PAGE_OPTIONS = [10, 25, 50, 100]

export default function DepartmentsPage() {
  const navigate = useNavigate()
  const [departments, setDepartments] = useState<Department[]>([])
  const [searchTerm, setSearchTerm] = useState('')
  const [sortBy, setSortBy] = useState<keyof Department>('name')
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc')
  const [currentPage, setCurrentPage] = useState(1)
  const [itemsPerPage, setItemsPerPage] = useState(10)
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

  useEffect(() => {
    loadDepartments()
  }, [])

  const loadDepartments = async () => {
    try {
      const response = await departmentsApi.getAllSimple()
      setDepartments(response)
      setCurrentPage(1)
    } catch (error) {
      toast.error('Failed to load departments')
    }
  }

  // Filter and sort departments
  const filteredAndSortedDepartments = useMemo(() => {
    let filtered = departments

    // Apply search filter
    if (searchTerm) {
      const term = searchTerm.toLowerCase()
      filtered = filtered.filter(
        (dept) =>
          dept.name.toLowerCase().includes(term) ||
          dept.description.toLowerCase().includes(term)
      )
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
  }, [departments, searchTerm, sortBy, sortOrder])

  // Paginate
  const totalPages = Math.ceil(filteredAndSortedDepartments.length / itemsPerPage)
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

  const handleSort = (column: keyof Department) => {
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
            <h1 className="text-3xl font-bold text-neutral-900">Departments</h1>
            <p className="text-neutral-600 mt-1">Manage organizational departments ({filteredAndSortedDepartments.length} total)</p>
          </div>
          <Button onClick={handleOpenCreate}>
            <Plus className="w-4 h-4 mr-2" />
            Add Department
          </Button>
        </div>

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
        </Card>

        {/* Departments Table */}
        <Card>
          {paginatedDepartments.length === 0 ? (
            <div className="py-12 text-center">
              <p className="text-neutral-500">No departments found</p>
            </div>
          ) : (
            <>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-neutral-200">
                      <th
                        className="px-6 py-3 text-left text-xs font-medium text-neutral-600 uppercase tracking-wider cursor-pointer hover:bg-neutral-50"
                        onClick={() => handleSort('name')}
                      >
                        Name {sortBy === 'name' && (sortOrder === 'asc' ? '↑' : '↓')}
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-neutral-600 uppercase tracking-wider">
                        Description
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-neutral-600 uppercase tracking-wider">
                        Created
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-neutral-600 uppercase tracking-wider">
                        Actions
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {paginatedDepartments.map((dept) => (
                      <tr key={dept.id} className="border-b border-neutral-200 hover:bg-neutral-50">
                        <td className="px-6 py-4 text-sm font-medium text-neutral-900">{dept.name}</td>
                        <td className="px-6 py-4 text-sm text-neutral-600">{dept.description}</td>
                        <td className="px-6 py-4 text-sm text-neutral-600">{formatDate(dept.createdAtUtc)}</td>
                        <td className="px-6 py-4 text-sm">
                          <div className="flex gap-2">
                            <button
                              onClick={() => handleViewDetails(dept.id)}
                              className="p-1 hover:bg-blue-100 rounded text-blue-600 transition-colors"
                              title="View Details"
                            >
                              <Eye className="w-4 h-4" />
                            </button>
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
                    {Array.from({ length: totalPages }, (_, i) => i + 1)
                      .slice(Math.max(0, currentPage - 2), currentPage + 1)
                      .map((page) => (
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
        title={selectedDepartment ? 'Edit Department' : 'Add Department'}
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
          <Input
            label="Department Name"
            value={formData.name}
            onChange={(e) => {
              setFormData({ ...formData, name: e.target.value })
              if (errors.name) setErrors({ ...errors, name: '' })
            }}
            error={errors.name}
          />
          <Input
            label="Description"
            value={formData.description}
            onChange={(e) => {
              setFormData({ ...formData, description: e.target.value })
              if (errors.description) setErrors({ ...errors, description: '' })
            }}
            error={errors.description}
          />
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
