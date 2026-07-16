import { useState, useEffect } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Input from '@/components/Common/Input'
import { Plus, Search, Edit2, Trash2 } from 'lucide-react'
import type { Department } from '@/types'
import toast from 'react-hot-toast'

export default function DepartmentsPage() {
  const [departments, setDepartments] = useState<Department[]>([
    {
      id: '1',
      name: 'Engineering',
      description: 'Software Development Team',
      totalBudget: 500000,
      averageSalary: 95000,
    },
    {
      id: '2',
      name: 'Human Resources',
      description: 'HR and Employee Management',
      totalBudget: 200000,
      averageSalary: 65000,
    },
    {
      id: '3',
      name: 'Sales',
      description: 'Sales and Business Development',
      totalBudget: 300000,
      averageSalary: 75000,
    },
  ])
  const [searchTerm, setSearchTerm] = useState('')

  const filteredDepartments = departments.filter((dept) =>
    dept.name.toLowerCase().includes(searchTerm.toLowerCase())
  )

  return (
    <MainLayout>
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold text-neutral-900">Departments</h1>
            <p className="text-neutral-600 mt-1">Organize and manage departments</p>
          </div>
          <Button>
            <Plus className="w-4 h-4 mr-2" />
            Add Department
          </Button>
        </div>

        {/* Search */}
        <Card>
          <Input
            icon={<Search className="w-5 h-5" />}
            placeholder="Search departments..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </Card>

        {/* Departments Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredDepartments.map((dept) => (
            <Card key={dept.id}>
              <div className="flex justify-between items-start mb-4">
                <div>
                  <h3 className="text-lg font-semibold text-neutral-900">{dept.name}</h3>
                  <p className="text-neutral-600 text-sm mt-1">{dept.description}</p>
                </div>
                <div className="flex gap-2">
                  <button className="p-1 hover:bg-neutral-100 rounded text-primary-600">
                    <Edit2 className="w-4 h-4" />
                  </button>
                  <button className="p-1 hover:bg-neutral-100 rounded text-red-600">
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>

              <div className="space-y-3 border-t border-neutral-200 pt-4">
                <div className="flex justify-between">
                  <span className="text-neutral-600 text-sm">Total Budget</span>
                  <span className="font-semibold text-neutral-900">
                    ${(dept.totalBudget / 1000).toFixed(0)}K
                  </span>
                </div>
                <div className="flex justify-between">
                  <span className="text-neutral-600 text-sm">Avg Salary</span>
                  <span className="font-semibold text-neutral-900">
                    ${(dept.averageSalary / 1000).toFixed(0)}K
                  </span>
                </div>
              </div>
            </Card>
          ))}
        </div>
      </div>
    </MainLayout>
  )
}
