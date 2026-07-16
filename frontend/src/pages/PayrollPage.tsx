import { useState } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import { DollarSign, Download, Eye } from 'lucide-react'

interface PayrollRecord {
  id: string
  employeeCode: string
  employeeName: string
  baseSalary: number
  allowances: number
  deductions: number
  netSalary: number
  month: string
  status: 'Processed' | 'Pending' | 'Paid'
}

export default function PayrollPage() {
  const [payrolls, setPayrolls] = useState<PayrollRecord[]>([
    {
      id: '1',
      employeeCode: 'EMP001',
      employeeName: 'John Doe',
      baseSalary: 50000,
      allowances: 5000,
      deductions: 3500,
      netSalary: 51500,
      month: 'January 2024',
      status: 'Paid',
    },
    {
      id: '2',
      employeeCode: 'EMP002',
      employeeName: 'Jane Smith',
      baseSalary: 55000,
      allowances: 6000,
      deductions: 4000,
      netSalary: 57000,
      month: 'January 2024',
      status: 'Paid',
    },
    {
      id: '3',
      employeeCode: 'EMP003',
      employeeName: 'Bob Johnson',
      baseSalary: 60000,
      allowances: 7000,
      deductions: 4500,
      netSalary: 62500,
      month: 'January 2024',
      status: 'Pending',
    },
  ])

  const statusColors: Record<string, 'success' | 'warning'> = {
    Paid: 'success',
    Pending: 'warning',
  }

  const totalPayroll = payrolls.reduce((sum, p) => sum + p.netSalary, 0)

  return (
    <MainLayout>
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold text-neutral-900">Payroll Management</h1>
            <p className="text-neutral-600 mt-1">Process and manage employee salaries</p>
          </div>
          <Button>
            <Download className="w-4 h-4 mr-2" />
            Export Payroll
          </Button>
        </div>

        {/* Summary Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <Card className="bg-green-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Total Payroll</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">
                  ${(totalPayroll / 1000).toFixed(1)}K
                </p>
              </div>
              <DollarSign className="w-8 h-8 text-green-600" />
            </div>
          </Card>

          <Card className="bg-blue-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Salaries Paid</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">
                  {payrolls.filter((p) => p.status === 'Paid').length}
                </p>
              </div>
              <DollarSign className="w-8 h-8 text-blue-600" />
            </div>
          </Card>

          <Card className="bg-yellow-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Pending Payroll</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">
                  {payrolls.filter((p) => p.status === 'Pending').length}
                </p>
              </div>
              <DollarSign className="w-8 h-8 text-yellow-600" />
            </div>
          </Card>
        </div>

        {/* Payroll Table */}
        <Card>
          <h2 className="text-lg font-semibold text-neutral-900 mb-4">January 2024 Payroll</h2>

          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-neutral-200 bg-neutral-50">
                  <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                    Employee
                  </th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                    Base Salary
                  </th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                    Allowances
                  </th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                    Deductions
                  </th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                    Net Salary
                  </th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                    Status
                  </th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody>
                {payrolls.map((payroll) => (
                  <tr key={payroll.id} className="border-b border-neutral-200 hover:bg-neutral-50">
                    <td className="px-6 py-4 text-sm text-neutral-700">
                      <div>
                        <p className="font-medium">{payroll.employeeName}</p>
                        <p className="text-neutral-500 text-xs">{payroll.employeeCode}</p>
                      </div>
                    </td>
                    <td className="px-6 py-4 text-sm text-neutral-700">
                      ${payroll.baseSalary.toLocaleString()}
                    </td>
                    <td className="px-6 py-4 text-sm text-neutral-700">
                      ${payroll.allowances.toLocaleString()}
                    </td>
                    <td className="px-6 py-4 text-sm text-neutral-700">
                      ${payroll.deductions.toLocaleString()}
                    </td>
                    <td className="px-6 py-4 text-sm font-semibold text-neutral-900">
                      ${payroll.netSalary.toLocaleString()}
                    </td>
                    <td className="px-6 py-4 text-sm">
                      <Badge label={payroll.status} variant={statusColors[payroll.status]} />
                    </td>
                    <td className="px-6 py-4 text-sm">
                      <button className="p-1 hover:bg-neutral-100 rounded text-primary-600">
                        <Eye className="w-4 h-4" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      </div>
    </MainLayout>
  )
}
