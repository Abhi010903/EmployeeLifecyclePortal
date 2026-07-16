import { useState, useEffect } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Input from '@/components/Common/Input'
import Table from '@/components/Common/Table'
import Badge from '@/components/Common/Badge'
import Modal from '@/components/Common/Modal'
import { Plus, Search, Edit2, Trash2 } from 'lucide-react'
import { employeesApi } from '@/api/employees'
import type { Employee } from '@/types'
import toast from 'react-hot-toast'

export default function EmployeesPage() {
  const [employees, setEmployees] = useState<Employee[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [selectedEmployee, setSelectedEmployee] = useState<Employee | null>(null)
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
      setIsLoading(true)
      const response = await employeesApi.getAll(1, 50)
      setEmployees(response.items)
    } catch (error) {
      toast.error('Failed to load employees')
    } finally {
      setIsLoading(false)
    }
  }

  const filteredEmployees = employees.filter((emp) =>
    `${emp.firstName} ${emp.lastName}`.toLowerCase().includes(searchTerm.toLowerCase()) ||
    emp.email.toLowerCase().includes(searchTerm.toLowerCase())
  )

  const handleEdit = (employee: Employee) => {
    setSelectedEmployee(employee)
    setFormData({
      firstName: employee.firstName,
      lastName: employee.lastName,
      email: employee.email,
      phoneNumber: employee.phoneNumber || '',
      departmentId: employee.departmentId,
    })
    setIsModalOpen(true)
  }

  const handleSave = async () => {
    try {
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
        await employeesApi.create(formData as any)
        toast.success('Employee created successfully')
      }
      setIsModalOpen(false)
      loadEmployees()
    } catch (error) {
      toast.error('Failed to save employee')
    }
  }

  const handleDelete = async (id: string) => {
    if (confirm('Are you sure?')) {
      try {
        await employeesApi.delete(id)
        toast.success('Employee deleted successfully')
        loadEmployees()
      } catch (error) {
        toast.error('Failed to delete employee')
      }
    }
  }

  return (
    <MainLayout>
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold text-neutral-900">Employees</h1>
            <p className="text-neutral-600 mt-1">Manage your workforce</p>
          </div>
          <Button
            onClick={() => {
              setSelectedEmployee(null)
              setFormData({
                firstName: '',
                lastName: '',
                email: '',
                phoneNumber: '',
                departmentId: '',
              })
              setIsModalOpen(true)
            }}
          >
            <Plus className="w-4 h-4 mr-2" />
            Add Employee
          </Button>
        </div>

        {/* Search and Filter */}
        <Card>
          <Input
            icon={<Search className="w-5 h-5" />}
            placeholder="Search by name or email..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </Card>

        {/* Employees Table */}
        <Card>
          <Table<Employee>
            columns={[
              { key: 'firstName', label: 'Name', render: (_, row) => `${row.firstName} ${row.lastName}` },
              { key: 'email', label: 'Email' },
              { key: 'phoneNumber', label: 'Phone' },
              { key: 'status', label: 'Status', render: (value) => <Badge label={value} variant={value === 'Active' ? 'success' : 'danger'} /> },
              {
                key: 'id',
                label: 'Actions',
                render: (_, row) => (
                  <div className="flex gap-2">
                    <button
                      onClick={() => handleEdit(row)}
                      className="p-1 hover:bg-neutral-100 rounded text-primary-600"
                    >
                      <Edit2 className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => handleDelete(row.id)}
                      className="p-1 hover:bg-neutral-100 rounded text-red-600"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                ),
              },
            ]}
            data={filteredEmployees}
            isLoading={isLoading}
          />
        </Card>
      </div>

      {/* Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={selectedEmployee ? 'Edit Employee' : 'Add Employee'}
        footer={
          <>
            <Button variant="outline" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleSave}>
              Save
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <Input
            label="First Name"
            value={formData.firstName}
            onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
          />
          <Input
            label="Last Name"
            value={formData.lastName}
            onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
          />
          <Input
            label="Email"
            type="email"
            value={formData.email}
            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
          />
          <Input
            label="Phone"
            value={formData.phoneNumber}
            onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
          />
        </div>
      </Modal>
    </MainLayout>
  )
}
