import { useEffect, useState, useCallback } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import Modal from '@/components/Common/Modal'
import { IndianRupee, Eye, AlertCircle, Plus, FileSpreadsheet } from 'lucide-react'
import { payrollApi } from '@/api/payroll'
import { employeesApi } from '@/api/employees'
import { PayslipDto, SalaryStructureDto, Employee } from '@/types'
import { useAuthStore } from '@/store/authStore'
import { formatCurrencyINR, formatDateIST } from '@/utils/format'
import toast from 'react-hot-toast'

export default function PayrollPage() {
  const { user } = useAuthStore()
  const userId = user?.id || localStorage.getItem('userId') || ''
  const [payslips, setPayslips] = useState<PayslipDto[]>([])
  const [salaryStructure, setSalaryStructure] = useState<SalaryStructureDto | null>(null)
  const [employees, setEmployees] = useState<Employee[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [selectedPayslip, setSelectedPayslip] = useState<PayslipDto | null>(null)
  const [isModalOpen, setIsModalOpen] = useState(false)

  // Generate Payslip Modal State
  const [isGenerateModalOpen, setIsGenerateModalOpen] = useState(false)
  const [genEmployeeId, setGenEmployeeId] = useState('')
  const [genMonth, setGenMonth] = useState(new Date().getMonth() + 1)
  const [genYear, setGenYear] = useState(new Date().getFullYear())
  const [isSubmittingGen, setIsSubmittingGen] = useState(false)

  // Update Salary Modal State
  const [isSalaryModalOpen, setIsSalaryModalOpen] = useState(false)
  const [salEmployeeId, setSalEmployeeId] = useState('')
  const [salBase, setSalBase] = useState('75000')
  const [isSubmittingSal, setIsSubmittingSal] = useState(false)

  const fetchData = useCallback(async () => {
    try {
      setLoading(true)
      const isManager = user?.role === 'Admin' || user?.role === 'Manager' || user?.role === 'HR'
      const [payslipsRes, structureRes, empsRes] = await Promise.allSettled([
        payrollApi.getPayslips(isManager ? undefined : (userId || undefined)),
        userId ? payrollApi.getSalaryStructure(userId) : Promise.resolve({ data: null }),
        employeesApi.getAllSimple(),
      ])

      if (payslipsRes.status === 'fulfilled') {
        setPayslips(Array.isArray(payslipsRes.value.data) ? payslipsRes.value.data : [])
      }
      if (structureRes.status === 'fulfilled' && structureRes.value.data) {
        setSalaryStructure(structureRes.value.data as SalaryStructureDto)
      }
      if (empsRes.status === 'fulfilled') {
        setEmployees(Array.isArray(empsRes.value) ? empsRes.value : [])
      }
      setError(null)
    } catch (err) {
      setError('Failed to load payroll data')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }, [userId, user?.role])

  useEffect(() => {
    fetchData()
  }, [fetchData])

  const handleGeneratePayslip = async () => {
    if (!genEmployeeId) {
      toast.error('Please select an employee.')
      return
    }

    try {
      setIsSubmittingGen(true)
      await payrollApi.generatePayslip({
        employeeId: genEmployeeId,
        month: Number(genMonth),
        year: Number(genYear),
      })
      toast.success('Payslip generated successfully!')
      setIsGenerateModalOpen(false)
      fetchData()
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Failed to generate payslip.')
    } finally {
      setIsSubmittingGen(false)
    }
  }

  const handleUpdateSalary = async () => {
    if (!salEmployeeId || !salBase) {
      toast.error('Please select an employee and enter base salary.')
      return
    }

    try {
      setIsSubmittingSal(true)
      await payrollApi.updateSalaryStructure(salEmployeeId, {
        baseSalary: parseFloat(salBase) || 0,
      })
      toast.success('Salary structure updated successfully!')
      setIsSalaryModalOpen(false)
      fetchData()
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Failed to update salary.')
    } finally {
      setIsSubmittingSal(false)
    }
  }

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
      'January', 'February', 'March', 'April', 'May', 'June',
      'July', 'August', 'September', 'October', 'November', 'December',
    ]
    return months[month - 1] || `Month ${month}`
  }

  const totalGross = payslips.reduce((sum, p) => sum + (p.grossSalary ?? (p.baseSalary + (p.allowances || 0))), 0)
  const totalDeductions = payslips.reduce((sum, p) => sum + p.deductions, 0)
  const totalNet = payslips.reduce((sum, p) => sum + p.netSalary, 0)
  const paidCount = payslips.filter((p) => p.status === 'Paid').length
  const pendingCount = payslips.filter((p) => p.status === 'Generated' || p.status === 'Processed' || p.status === 'Pending').length

  return (
    <MainLayout>
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold text-neutral-900">Payroll Management</h1>
            <p className="text-neutral-600 mt-1">View and manage employee salaries and payslips</p>
          </div>
          <div className="flex gap-3">
            <Button variant="outline" onClick={() => setIsSalaryModalOpen(true)}>
              <IndianRupee className="w-4 h-4 mr-2" />
              Set Salary Structure
            </Button>
            <Button onClick={() => setIsGenerateModalOpen(true)}>
              <Plus className="w-4 h-4 mr-2" />
              Generate Payslip
            </Button>
          </div>
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
                <p className="text-neutral-600 text-sm">Active Salary Structure</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">
                  {formatCurrencyINR(salaryStructure.baseSalary)}
                </p>
                <p className="text-neutral-500 text-xs mt-1">
                  Effective from {formatDateIST(salaryStructure.effectiveFromUtc)}
                </p>
              </div>
              <IndianRupee className="w-12 h-12 text-primary-600" />
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
                  {formatCurrencyINR(totalGross)}
                </p>
              </div>
              <IndianRupee className="w-8 h-8 text-green-600" />
            </div>
          </Card>

          <Card className="bg-red-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Total Deductions</p>
                <p className="text-2xl font-bold text-neutral-900 mt-2">
                  {formatCurrencyINR(totalDeductions)}
                </p>
              </div>
              <IndianRupee className="w-8 h-8 text-red-600" />
            </div>
          </Card>

          <Card className="bg-blue-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Total Net</p>
                <p className="text-2xl font-bold text-neutral-900 mt-2">
                  {formatCurrencyINR(totalNet)}
                </p>
              </div>
              <IndianRupee className="w-8 h-8 text-blue-600" />
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
              <FileSpreadsheet className="w-8 h-8 text-yellow-600" />
            </div>
          </Card>
        </div>

        {/* Payslips Table */}
        <Card>
          <h2 className="text-lg font-semibold text-neutral-900 mb-4">Payslip History</h2>

          {loading && payslips.length === 0 ? (
            <div className="text-center py-12">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
              <p className="text-neutral-500 mt-3">Loading payslip data...</p>
            </div>
          ) : payslips.length === 0 ? (
            <div className="text-center py-12">
              <IndianRupee className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
              <p className="text-neutral-500">No payslips generated yet</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-neutral-200 bg-neutral-50">
                    <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                      Employee
                    </th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                      Period
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
                      View
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {payslips.map((payslip) => (
                    <tr key={payslip.id} className="border-b border-neutral-200 hover:bg-neutral-50">
                      <td className="px-6 py-4 text-sm font-medium text-neutral-900">
                        {payslip.employeeName || 'Employee'}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700">
                        {getMonthName(payslip.month)} {payslip.year}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700 font-medium">
                        {formatCurrencyINR(payslip.grossSalary ?? (payslip.baseSalary + (payslip.allowances || 0)))}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700">
                        {formatCurrencyINR(payslip.deductions)}
                      </td>
                      <td className="px-6 py-4 text-sm font-semibold text-neutral-900">
                        {formatCurrencyINR(payslip.netSalary)}
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

      {/* Generate Payslip Modal */}
      <Modal
        isOpen={isGenerateModalOpen}
        onClose={() => setIsGenerateModalOpen(false)}
        title="Generate Employee Payslip"
        footer={
          <>
            <Button variant="outline" onClick={() => setIsGenerateModalOpen(false)} disabled={isSubmittingGen}>
              Cancel
            </Button>
            <Button onClick={handleGeneratePayslip} disabled={isSubmittingGen}>
              {isSubmittingGen ? 'Generating...' : 'Generate Payslip'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Select Employee *</label>
            <select
              value={genEmployeeId}
              onChange={(e) => setGenEmployeeId(e.target.value)}
              className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              <option value="">Choose employee...</option>
              {employees.map((emp) => (
                <option key={emp.id} value={emp.id}>
                  {emp.firstName} {emp.lastName} ({emp.employeeCode})
                </option>
              ))}
            </select>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">Month *</label>
              <select
                value={genMonth}
                onChange={(e) => setGenMonth(parseInt(e.target.value))}
                className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
              >
                {Array.from({ length: 12 }, (_, i) => (
                  <option key={i + 1} value={i + 1}>
                    {getMonthName(i + 1)}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">Year *</label>
              <input
                type="number"
                value={genYear}
                onChange={(e) => setGenYear(parseInt(e.target.value) || 2026)}
                className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
              />
            </div>
          </div>
        </div>
      </Modal>

      {/* Set Salary Structure Modal */}
      <Modal
        isOpen={isSalaryModalOpen}
        onClose={() => setIsSalaryModalOpen(false)}
        title="Set Employee Salary Structure"
        footer={
          <>
            <Button variant="outline" onClick={() => setIsSalaryModalOpen(false)} disabled={isSubmittingSal}>
              Cancel
            </Button>
            <Button onClick={handleUpdateSalary} disabled={isSubmittingSal}>
              {isSubmittingSal ? 'Saving...' : 'Save Salary'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Select Employee *</label>
            <select
              value={salEmployeeId}
              onChange={(e) => setSalEmployeeId(e.target.value)}
              className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              <option value="">Choose employee...</option>
              {employees.map((emp) => (
                <option key={emp.id} value={emp.id}>
                  {emp.firstName} {emp.lastName} ({emp.employeeCode})
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Monthly Base Salary (₹) *</label>
            <input
              type="number"
              value={salBase}
              onChange={(e) => setSalBase(e.target.value)}
              className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
          </div>
        </div>
      </Modal>

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
          <Button
            variant="outline"
            onClick={() => {
              setIsModalOpen(false)
              setSelectedPayslip(null)
            }}
          >
            Close
          </Button>
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
                Employee: <strong>{selectedPayslip.employeeName}</strong>
              </p>
              <p className="text-xs text-neutral-500">
                Generated on {formatDateIST(selectedPayslip.generatedDateUtc)}
              </p>
            </div>

            {/* Salary Breakdown */}
            <div className="space-y-3">
              <div className="flex justify-between items-center pb-2 border-b border-neutral-100">
                <span className="text-neutral-700">Gross Base Salary</span>
                <span className="font-semibold text-neutral-900">
                  {formatCurrencyINR(selectedPayslip.grossSalary ?? (selectedPayslip.baseSalary + (selectedPayslip.allowances || 0)))}
                </span>
              </div>

              <div className="flex justify-between items-center pb-2 border-b border-neutral-100">
                <span className="text-neutral-700">Deductions (Tax / PF)</span>
                <span className="font-semibold text-red-600">
                  -{formatCurrencyINR(selectedPayslip.deductions)}
                </span>
              </div>

              <div className="flex justify-between items-center pt-2 pb-4 bg-neutral-50 px-3 rounded">
                <span className="font-semibold text-neutral-900">Net Take-Home Salary</span>
                <span className="text-lg font-bold text-green-600">
                  {formatCurrencyINR(selectedPayslip.netSalary)}
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
