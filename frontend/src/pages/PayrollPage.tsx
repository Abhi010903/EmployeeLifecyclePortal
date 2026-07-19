import { useEffect, useState, useCallback } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import Modal from '@/components/Common/Modal'
import { DollarSign, Download, Eye, AlertCircle } from 'lucide-react'
import { payrollApi } from '@/api/payroll'
import { PayslipDto, SalaryStructureDto } from '@/types'

// Indian currency formatter
const formatIndianCurrency = (amount: number) => {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(amount)
}

export default function PayrollPage() {
  const [payslips, setPayslips] = useState<PayslipDto[]>([])
  const [salaryStructure, setSalaryStructure] = useState<SalaryStructureDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [selectedPayslip, setSelectedPayslip] = useState<PayslipDto | null>(null)
  const [isModalOpen, setIsModalOpen] = useState(false)

  const userId = localStorage.getItem('userId') || ''

  const fetchData = useCallback(async () => {
    try {
      setLoading(true)
      const [payslipsRes, structureRes] = await Promise.all([
        payrollApi.getPayslips(userId),
        payrollApi.getSalaryStructure(userId),
      ])

      setPayslips(payslipsRes.data || [])
      setSalaryStructure(structureRes.data || null)
      setError(null)
    } catch (err) {
      setError('Failed to load payroll data')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }, [userId])

  useEffect(() => {
    if (userId) {
      fetchData()
    }
  }, [userId, fetchData])

  const handleViewPayslip = (payslip: PayslipDto) => {
    setSelectedPayslip(payslip)
    setIsModalOpen(true)
  }

  const statusColors: Record<string, 'success' | 'warning' | 'danger'> = {
    Generated: 'warning',
    Processed: 'success',
    Paid: 'success',
    Rejected: 'danger',
  }

  const getMonthName = (month: number) => {
    const months = [
      'January',
      'February',
      'March',
      'April',
      'May',
      'June',
      'July',
      'August',
      'September',
      'October',
      'November',
      'December',
    ]
    return months[month - 1] || `Month ${month}`
  }

  const totalGross = payslips.reduce((sum, p) => sum + p.grossSalary, 0)
  const totalDeductions = payslips.reduce((sum, p) => sum + p.deductions, 0)
  const totalNet = payslips.reduce((sum, p) => sum + p.netSalary, 0)
  const paidCount = payslips.filter((p) => p.status === 'Paid').length
  const pendingCount = payslips.filter((p) => p.status === 'Generated' || p.status === 'Processed').length

  return (
    <MainLayout>
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold text-neutral-900">Payroll Management</h1>
            <p className="text-neutral-600 mt-1">View and manage employee salaries</p>
          </div>
          <Button>
            <Download className="w-4 h-4 mr-2" />
            Export Payroll
          </Button>
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 flex items-start gap-3">
            <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
            <p className="text-sm text-red-900">{error}</p>
          </div>
        )}

        {/* Salary Structure Info */}
        {salaryStructure && (
          <Card className="bg-gradient-to-r from-primary-50 to-blue-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Current Salary Structure</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">
                  {formatIndianCurrency(salaryStructure.baseSalary)}
                </p>
                <p className="text-neutral-500 text-xs mt-1">
                  Effective from {new Date(salaryStructure.effectiveFromUtc).toLocaleDateString('en-IN')}
                </p>
              </div>
              <DollarSign className="w-12 h-12 text-primary-600" />
            </div>
          </Card>
        )}

        {/* Summary Cards */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
          <Card className="bg-green-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Total Gross</p>
                <p className="text-2xl font-bold text-neutral-900 mt-2">
                  {formatIndianCurrency(totalGross)}
                </p>
              </div>
              <DollarSign className="w-8 h-8 text-green-600" />
            </div>
          </Card>

          <Card className="bg-red-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Total Deductions</p>
                <p className="text-2xl font-bold text-neutral-900 mt-2">
                  {formatIndianCurrency(totalDeductions)}
                </p>
              </div>
              <DollarSign className="w-8 h-8 text-red-600" />
            </div>
          </Card>

          <Card className="bg-blue-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Total Net</p>
                <p className="text-2xl font-bold text-neutral-900 mt-2">
                  {formatIndianCurrency(totalNet)}
                </p>
              </div>
              <DollarSign className="w-8 h-8 text-blue-600" />
            </div>
          </Card>

          <Card className="bg-yellow-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Payslips</p>
                <p className="text-2xl font-bold text-neutral-900 mt-2">{payslips.length}</p>
                <p className="text-neutral-500 text-xs mt-1">
                  {paidCount} paid, {pendingCount} pending
                </p>
              </div>
              <DollarSign className="w-8 h-8 text-yellow-600" />
            </div>
          </Card>
        </div>

        {/* Payslips Table */}
        <Card>
          <h2 className="text-lg font-semibold text-neutral-900 mb-4">Payslip History</h2>

          {loading && payslips.length === 0 ? (
            <div className="text-center py-12">
              <DollarSign className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
              <p className="text-neutral-500">Loading payslip data...</p>
            </div>
          ) : payslips.length === 0 ? (
            <div className="text-center py-12">
              <DollarSign className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
              <p className="text-neutral-500">No payslips available</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-neutral-200 bg-neutral-50">
                    <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                      Month
                    </th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                      Gross Salary
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
                  {payslips.map((payslip) => (
                    <tr key={payslip.id} className="border-b border-neutral-200 hover:bg-neutral-50">
                      <td className="px-6 py-4 text-sm font-medium text-neutral-900">
                        {getMonthName(payslip.month)} {payslip.year}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700">
                        {formatIndianCurrency(payslip.grossSalary)}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700">
                        {formatIndianCurrency(payslip.deductions)}
                      </td>
                      <td className="px-6 py-4 text-sm font-semibold text-neutral-900">
                        {formatIndianCurrency(payslip.netSalary)}
                      </td>
                      <td className="px-6 py-4 text-sm">
                        <Badge
                          label={payslip.status}
                          variant={statusColors[payslip.status] as any}
                        />
                      </td>
                      <td className="px-6 py-4 text-sm">
                        <button
                          onClick={() => handleViewPayslip(payslip)}
                          className="p-1 hover:bg-neutral-100 rounded text-primary-600 transition"
                        >
                          <Eye className="w-4 h-4" />
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Card>
      </div>

      {/* Payslip Detail Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => {
          setIsModalOpen(false)
          setSelectedPayslip(null)
        }}
        title={
          selectedPayslip
            ? `Payslip - ${getMonthName(selectedPayslip.month)} ${selectedPayslip.year}`
            : 'Payslip'
        }
        footer={
          <>
            <Button
              variant="outline"
              onClick={() => {
                setIsModalOpen(false)
                setSelectedPayslip(null)
              }}
            >
              Close
            </Button>
            <Button>
              <Download className="w-4 h-4 mr-2" />
              Download PDF
            </Button>
          </>
        }
      >
        {selectedPayslip && (
          <div className="space-y-6">
            {/* Header */}
            <div className="border-b border-neutral-200 pb-4">
              <h3 className="text-lg font-semibold text-neutral-900">
                Payslip for {getMonthName(selectedPayslip.month)} {selectedPayslip.year}
              </h3>
              <p className="text-sm text-neutral-600 mt-1">
                Generated on {new Date(selectedPayslip.generatedDateUtc).toLocaleDateString('en-IN')}
              </p>
            </div>

            {/* Salary Breakdown */}
            <div className="space-y-3">
              <div className="flex justify-between items-center pb-2 border-b border-neutral-100">
                <span className="text-neutral-700">Gross Salary</span>
                <span className="font-semibold text-neutral-900">
                  {formatIndianCurrency(selectedPayslip.grossSalary)}
                </span>
              </div>

              <div className="flex justify-between items-center pb-2 border-b border-neutral-100">
                <span className="text-neutral-700">Deductions</span>
                <span className="font-semibold text-red-600">
                  -{formatIndianCurrency(selectedPayslip.deductions)}
                </span>
              </div>

              <div className="flex justify-between items-center pt-2 pb-4 bg-neutral-50 px-3 rounded">
                <span className="font-semibold text-neutral-900">Net Salary</span>
                <span className="text-lg font-bold text-green-600">
                  {formatIndianCurrency(selectedPayslip.netSalary)}
                </span>
              </div>
            </div>

            {/* Status */}
            <div className="flex items-center justify-between">
              <span className="text-neutral-700">Status</span>
              <Badge label={selectedPayslip.status} variant={statusColors[selectedPayslip.status] as any} />
            </div>
          </div>
        )}
      </Modal>
    </MainLayout>
  )
}
