import { useEffect, useState, useCallback } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import Modal from '@/components/Common/Modal'
import EmptyState from '@/components/Common/EmptyState'
import ConfirmModal from '@/components/Common/ConfirmModal'
import {
  IndianRupee,
  Eye,
  Plus,
  CheckCircle2,
  Calculator,
  ShieldCheck,
  Download,
  Printer,
  FileText,
  Building2,
  TrendingUp,
  Receipt,
  Users,
  Play,
  Lock,
} from 'lucide-react'
import { payrollApi } from '@/api/payroll'
import { employeesApi } from '@/api/employees'
import {
  PayslipDto,
  SalaryStructureDto,
  Employee,
  PayrollSummaryDto,
  ReimbursementDto,
} from '@/types'
import { useAuthStore } from '@/store/authStore'
import { formatCurrencyINR, formatDateIST } from '@/utils/format'
import toast from 'react-hot-toast'

type PayrollTab =
  | 'overview'
  | 'runs'
  | 'structures'
  | 'reimbursements'
  | 'payslips'
  | 'simulator'
  | 'compliance'

export default function PayrollPage() {
  const { user } = useAuthStore()
  const userId = user?.id || localStorage.getItem('userId') || ''
  const isElevated =
    user?.role === 'Admin' ||
    user?.role === 'HR' ||
    user?.role === 'Manager' ||
    user?.role === 'Team Lead' ||
    user?.role === 'TeamLead'
  const isAdmin = user?.role === 'Admin'

  const [activeTab, setActiveTab] = useState<PayrollTab>('overview')
  const [selectedMonth, setSelectedMonth] = useState(new Date().getMonth() + 1)
  const [selectedYear, setSelectedYear] = useState(new Date().getFullYear())

  // Core Data State
  const [summary, setSummary] = useState<PayrollSummaryDto | null>(null)
  const [payslips, setPayslips] = useState<PayslipDto[]>([])
  const [employees, setEmployees] = useState<Employee[]>([])
  const [reimbursements, setReimbursements] = useState<ReimbursementDto[]>([])
  const [, setMySalaryStructure] = useState<SalaryStructureDto | null>(null)
  const [, setLoading] = useState(true)

  // Interactive View Modals
  const [selectedPayslip, setSelectedPayslip] = useState<PayslipDto | null>(null)
  const [isPayslipModalOpen, setIsPayslipModalOpen] = useState(false)

  // Action Modals
  const [isRunConfirmOpen, setIsRunConfirmOpen] = useState(false)
  const [isRunningPayroll, setIsRunningPayroll] = useState(false)
  const [isApproveConfirmOpen, setIsApproveConfirmOpen] = useState(false)
  const [isApprovingPayroll, setIsApprovingPayroll] = useState(false)

  // Reimbursement Modal
  const [isClaimModalOpen, setIsClaimModalOpen] = useState(false)
  const [claimAmount, setClaimAmount] = useState('')
  const [claimCategory, setClaimCategory] = useState('Travel')
  const [claimDesc, setClaimDesc] = useState('')
  const [claimReceiptUrl, setClaimReceiptUrl] = useState('')
  const [isSubmittingClaim, setIsSubmittingClaim] = useState(false)

  // Salary Structure Edit Modal
  const [isSalaryModalOpen, setIsSalaryModalOpen] = useState(false)
  const [salEmployeeId, setSalEmployeeId] = useState('')
  const [salBase, setSalBase] = useState('75000')
  const [isSubmittingSal, setIsSubmittingSal] = useState(false)

  // What-If Simulator State
  const [simBaseSalary, setSimBaseSalary] = useState(65000)
  const [simIncrementPercent, setSimIncrementPercent] = useState(10)
  const [simBonus, setSimBonus] = useState(15000)
  const [simOvertimeHours, setSimOvertimeHours] = useState(10)

  const monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December',
  ]

  const fetchData = useCallback(async () => {
    try {
      setLoading(true)
      const isManagerOrAdmin = user?.role === 'Admin' || user?.role === 'HR' || user?.role === 'Manager'

      const [summaryRes, payslipsRes, empsRes, reimbRes, structureRes] = await Promise.allSettled([
        payrollApi.getSummary(selectedMonth, selectedYear),
        payrollApi.getPayslips(isManagerOrAdmin ? undefined : (userId || undefined)),
        employeesApi.getAllSimple(),
        payrollApi.getReimbursements(isManagerOrAdmin ? undefined : (userId || undefined)),
        userId ? payrollApi.getSalaryStructure(userId) : Promise.resolve({ data: null }),
      ])

      if (summaryRes.status === 'fulfilled' && summaryRes.value.data) {
        setSummary(summaryRes.value.data)
      }
      if (payslipsRes.status === 'fulfilled' && Array.isArray(payslipsRes.value.data)) {
        setPayslips(payslipsRes.value.data)
      }
      if (empsRes.status === 'fulfilled' && Array.isArray(empsRes.value)) {
        setEmployees(empsRes.value)
      }
      if (reimbRes.status === 'fulfilled' && Array.isArray(reimbRes.value.data)) {
        setReimbursements(reimbRes.value.data)
      }
      if (structureRes.status === 'fulfilled' && structureRes.value.data) {
        setMySalaryStructure(structureRes.value.data as SalaryStructureDto)
      }
    } catch (err) {
      console.error('Error loading payroll data:', err)
      toast.error('Failed to load payroll records.')
    } finally {
      setLoading(false)
    }
  }, [selectedMonth, selectedYear, userId, user?.role])

  useEffect(() => {
    fetchData()
  }, [fetchData])

  // Handlers
  const handleRunPayroll = async () => {
    try {
      setIsRunningPayroll(true)
      const res = await payrollApi.processPayrollRun(selectedMonth, selectedYear)
      toast.success(`Payroll processed for ${monthNames[selectedMonth - 1]} ${selectedYear}!`)
      setSummary(res.data)
      setIsRunConfirmOpen(false)
      fetchData()
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Failed to process payroll batch.')
    } finally {
      setIsRunningPayroll(false)
    }
  }

  const handleApprovePayroll = async () => {
    try {
      setIsApprovingPayroll(true)
      await payrollApi.approvePayrollRun(selectedMonth, selectedYear)
      toast.success(`Payroll approved and finalized for ${monthNames[selectedMonth - 1]} ${selectedYear}!`)
      setIsApproveConfirmOpen(false)
      fetchData()
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Failed to approve payroll.')
    } finally {
      setIsApprovingPayroll(false)
    }
  }

  const handleSubmitClaim = async (e: React.FormEvent) => {
    e.preventDefault()
    const amt = parseFloat(claimAmount)
    if (isNaN(amt) || amt <= 0) {
      toast.error('Please enter a valid reimbursement amount.')
      return
    }

    try {
      setIsSubmittingClaim(true)
      const empId = user?.id || employees[0]?.id
      await payrollApi.createReimbursement({
        employeeId: empId,
        amount: amt,
        category: claimCategory,
        description: claimDesc,
        receiptUrl: claimReceiptUrl || undefined,
      })
      toast.success('Reimbursement claim submitted successfully!')
      setIsClaimModalOpen(false)
      setClaimAmount('')
      setClaimDesc('')
      setClaimReceiptUrl('')
      fetchData()
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Failed to submit reimbursement.')
    } finally {
      setIsSubmittingClaim(false)
    }
  }

  const handleApproveClaim = async (id: string) => {
    try {
      await payrollApi.approveReimbursement(id)
      toast.success('Reimbursement claim approved!')
      fetchData()
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Failed to approve claim.')
    }
  }

  const handleRejectClaim = async (id: string) => {
    try {
      await payrollApi.rejectReimbursement(id, 'Not eligible under company policy')
      toast.success('Reimbursement claim rejected.')
      fetchData()
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Failed to reject claim.')
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
        baseSalary: parseFloat(salBase),
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

  // Simulation Calculations
  const simNewBase = simBaseSalary * (1 + simIncrementPercent / 100)
  const simHra = simNewBase * 0.4
  const simAllowances = simNewBase * 0.15
  const simHourlyRate = simNewBase / (22 * 8)
  const simOvertimePay = simOvertimeHours * simHourlyRate * 1.5
  const simGross = simNewBase + simHra + simAllowances + simBonus + simOvertimePay
  const simPf = simNewBase * 0.12
  const simTds = simGross * 0.05
  const simNet = simGross - simPf - simTds
  const currentNet = simBaseSalary * 1.38

  return (
    <MainLayout>
      <div className="space-y-6 max-w-7xl mx-auto pb-12">
        {/* Page Header */}
        <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 bg-white p-6 rounded-2xl border border-neutral-200/80 shadow-xs">
          <div>
            <div className="flex items-center gap-2.5">
              <div className="p-2.5 rounded-xl bg-gradient-to-tr from-primary-600 to-indigo-600 text-white shadow-xs">
                <IndianRupee className="w-6 h-6" />
              </div>
              <div>
                <h1 className="text-2xl font-bold text-neutral-900 tracking-tight">
                  Payroll Management System
                </h1>
                <p className="text-xs text-neutral-500 mt-0.5">
                  Integrated salary calculation, statutory deductions, reimbursement claims, and payslips
                </p>
              </div>
            </div>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            {/* Period Selector */}
            <div className="flex items-center bg-neutral-50 border border-neutral-200 rounded-xl p-1 shadow-2xs">
              <select
                value={selectedMonth}
                onChange={(e) => setSelectedMonth(Number(e.target.value))}
                className="bg-transparent text-xs font-semibold text-neutral-700 py-1.5 px-2 focus:outline-none cursor-pointer"
              >
                {monthNames.map((m, idx) => (
                  <option key={m} value={idx + 1}>
                    {m}
                  </option>
                ))}
              </select>
              <select
                value={selectedYear}
                onChange={(e) => setSelectedYear(Number(e.target.value))}
                className="bg-transparent text-xs font-semibold text-neutral-700 py-1.5 px-2 border-l border-neutral-200 focus:outline-none cursor-pointer"
              >
                <option value={2026}>2026</option>
                <option value={2025}>2025</option>
              </select>
            </div>

            {/* Action Buttons */}
            {isElevated && (
              <>
                <Button
                  variant="secondary"
                  size="sm"
                  onClick={() => setIsSalaryModalOpen(true)}
                  className="shadow-2xs"
                >
                  <Plus className="w-4 h-4 mr-1.5" />
                  Salary Structure
                </Button>

                <Button
                  variant="primary"
                  size="sm"
                  onClick={() => setIsRunConfirmOpen(true)}
                  className="shadow-xs bg-gradient-to-r from-primary-600 to-indigo-600 hover:from-primary-700 hover:to-indigo-700 text-white"
                >
                  <Play className="w-4 h-4 mr-1.5" />
                  Calculate Payroll
                </Button>
              </>
            )}

            <Button
              variant="outline"
              size="sm"
              onClick={() => setIsClaimModalOpen(true)}
              className="shadow-2xs"
            >
              <Receipt className="w-4 h-4 mr-1.5 text-primary-600" />
              Claim Reimbursement
            </Button>
          </div>
        </div>

        {/* Tab Navigation */}
        <div className="flex items-center gap-1.5 overflow-x-auto border-b border-neutral-200/80 pb-px scrollbar-none">
          <button
            type="button"
            onClick={() => setActiveTab('overview')}
            className={`px-4 py-2.5 text-xs font-semibold rounded-t-xl transition-all border-b-2 cursor-pointer flex items-center gap-2 ${
              activeTab === 'overview'
                ? 'border-primary-600 text-primary-700 bg-primary-50/50'
                : 'border-transparent text-neutral-600 hover:text-neutral-900 hover:bg-neutral-50'
            }`}
          >
            <TrendingUp className="w-4 h-4" />
            Overview & Analytics
          </button>

          {isElevated && (
            <button
              type="button"
              onClick={() => setActiveTab('runs')}
              className={`px-4 py-2.5 text-xs font-semibold rounded-t-xl transition-all border-b-2 cursor-pointer flex items-center gap-2 ${
                activeTab === 'runs'
                  ? 'border-primary-600 text-primary-700 bg-primary-50/50'
                  : 'border-transparent text-neutral-600 hover:text-neutral-900 hover:bg-neutral-50'
              }`}
            >
              <Play className="w-4 h-4" />
              Payroll Runs ({monthNames[selectedMonth - 1]})
            </button>
          )}

          {isElevated && (
            <button
              type="button"
              onClick={() => setActiveTab('structures')}
              className={`px-4 py-2.5 text-xs font-semibold rounded-t-xl transition-all border-b-2 cursor-pointer flex items-center gap-2 ${
                activeTab === 'structures'
                  ? 'border-primary-600 text-primary-700 bg-primary-50/50'
                  : 'border-transparent text-neutral-600 hover:text-neutral-900 hover:bg-neutral-50'
              }`}
            >
              <Building2 className="w-4 h-4" />
              Salary Structures
            </button>
          )}

          <button
            type="button"
            onClick={() => setActiveTab('reimbursements')}
            className={`px-4 py-2.5 text-xs font-semibold rounded-t-xl transition-all border-b-2 cursor-pointer flex items-center gap-2 ${
              activeTab === 'reimbursements'
                ? 'border-primary-600 text-primary-700 bg-primary-50/50'
                : 'border-transparent text-neutral-600 hover:text-neutral-900 hover:bg-neutral-50'
            }`}
          >
            <Receipt className="w-4 h-4" />
            Reimbursements
            {reimbursements.filter((r) => r.status === 'Pending').length > 0 && (
              <span className="w-2 h-2 rounded-full bg-amber-500 animate-pulse" />
            )}
          </button>

          <button
            type="button"
            onClick={() => setActiveTab('payslips')}
            className={`px-4 py-2.5 text-xs font-semibold rounded-t-xl transition-all border-b-2 cursor-pointer flex items-center gap-2 ${
              activeTab === 'payslips'
                ? 'border-primary-600 text-primary-700 bg-primary-50/50'
                : 'border-transparent text-neutral-600 hover:text-neutral-900 hover:bg-neutral-50'
            }`}
          >
            <FileText className="w-4 h-4" />
            Digital Payslips
          </button>

          <button
            type="button"
            onClick={() => setActiveTab('simulator')}
            className={`px-4 py-2.5 text-xs font-semibold rounded-t-xl transition-all border-b-2 cursor-pointer flex items-center gap-2 ${
              activeTab === 'simulator'
                ? 'border-primary-600 text-primary-700 bg-primary-50/50'
                : 'border-transparent text-neutral-600 hover:text-neutral-900 hover:bg-neutral-50'
            }`}
          >
            <Calculator className="w-4 h-4 text-purple-600" />
            What-If Simulator
          </button>

          <button
            type="button"
            onClick={() => setActiveTab('compliance')}
            className={`px-4 py-2.5 text-xs font-semibold rounded-t-xl transition-all border-b-2 cursor-pointer flex items-center gap-2 ${
              activeTab === 'compliance'
                ? 'border-primary-600 text-primary-700 bg-primary-50/50'
                : 'border-transparent text-neutral-600 hover:text-neutral-900 hover:bg-neutral-50'
            }`}
          >
            <ShieldCheck className="w-4 h-4 text-emerald-600" />
            Tax & Statutory Rules
          </button>
        </div>

        {/* TAB CONTENT */}

        {/* 1. OVERVIEW & ANALYTICS */}
        {activeTab === 'overview' && (
          <div className="space-y-6">
            {/* Top KPI Cards */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
              <Card className="p-5 border border-neutral-200/80 bg-gradient-to-br from-white to-neutral-50/60 shadow-2xs">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-semibold text-neutral-500 uppercase tracking-wider">
                    Total Gross Payroll
                  </span>
                  <div className="p-2 rounded-xl bg-blue-50 text-blue-600">
                    <IndianRupee className="w-5 h-5" />
                  </div>
                </div>
                <p className="text-2xl font-bold text-neutral-900 mt-2">
                  {formatCurrencyINR(summary?.totalGrossSalary || 0)}
                </p>
                <div className="flex items-center gap-1.5 mt-2 text-xs text-neutral-500 font-medium">
                  <Users className="w-3.5 h-3.5 text-neutral-400" />
                  <span>{summary?.totalEmployees || employees.length} Active Employees</span>
                </div>
              </Card>

              <Card className="p-5 border border-neutral-200/80 bg-gradient-to-br from-white to-neutral-50/60 shadow-2xs">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-semibold text-neutral-500 uppercase tracking-wider">
                    Statutory Deductions
                  </span>
                  <div className="p-2 rounded-xl bg-amber-50 text-amber-600">
                    <ShieldCheck className="w-5 h-5" />
                  </div>
                </div>
                <p className="text-2xl font-bold text-amber-600 mt-2">
                  {formatCurrencyINR(summary?.totalDeductions || 0)}
                </p>
                <div className="flex items-center gap-2 mt-2 text-xs text-neutral-500 font-medium">
                  <span>PF: {formatCurrencyINR(summary?.totalPf || 0)}</span>
                  <span>•</span>
                  <span>TDS: {formatCurrencyINR(summary?.totalTds || 0)}</span>
                </div>
              </Card>

              <Card className="p-5 border border-neutral-200/80 bg-gradient-to-br from-white to-neutral-50/60 shadow-2xs">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-semibold text-neutral-500 uppercase tracking-wider">
                    Approved Claims
                  </span>
                  <div className="p-2 rounded-xl bg-purple-50 text-purple-600">
                    <Receipt className="w-5 h-5" />
                  </div>
                </div>
                <p className="text-2xl font-bold text-purple-600 mt-2">
                  {formatCurrencyINR(summary?.totalReimbursements || 0)}
                </p>
                <div className="flex items-center gap-1.5 mt-2 text-xs text-neutral-500 font-medium">
                  <span>Included in Net Payroll</span>
                </div>
              </Card>

              <Card className="p-5 border border-neutral-200/80 bg-gradient-to-br from-white to-neutral-50/60 shadow-2xs">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-semibold text-neutral-500 uppercase tracking-wider">
                    Total Net Disbursement
                  </span>
                  <div className="p-2 rounded-xl bg-emerald-50 text-emerald-600">
                    <CheckCircle2 className="w-5 h-5" />
                  </div>
                </div>
                <p className="text-2xl font-bold text-emerald-600 mt-2">
                  {formatCurrencyINR(summary?.totalNetSalary || 0)}
                </p>
                <div className="flex items-center gap-1.5 mt-2 text-xs text-neutral-500 font-medium">
                  <span className="w-2 h-2 rounded-full bg-emerald-500" />
                  <span>Cycle Status: <strong>{summary?.status || 'Draft'}</strong></span>
                </div>
              </Card>
            </div>

            {/* Department Breakdown & Distribution */}
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
              <Card className="lg:col-span-2 p-6 border border-neutral-200/80 shadow-2xs">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-sm font-bold text-neutral-900 tracking-tight flex items-center gap-2">
                    <Building2 className="w-4 h-4 text-primary-600" />
                    Department Payroll Allocation
                  </h3>
                  <span className="text-xs text-neutral-400">
                    Period: {monthNames[selectedMonth - 1]} {selectedYear}
                  </span>
                </div>

                {summary?.departmentBreakdown && summary.departmentBreakdown.length > 0 ? (
                  <div className="space-y-4">
                    {summary.departmentBreakdown.map((dept) => {
                      const totalGross = summary.totalGrossSalary || 1
                      const pct = Math.round((dept.totalGross / totalGross) * 100) || 0
                      return (
                        <div key={dept.departmentName} className="space-y-1.5">
                          <div className="flex justify-between text-xs font-semibold text-neutral-700">
                            <div className="flex items-center gap-2">
                              <span>{dept.departmentName}</span>
                              <span className="text-[10px] text-neutral-400 font-normal">
                                ({dept.employeeCount} employees)
                              </span>
                            </div>
                            <span className="font-mono">{formatCurrencyINR(dept.totalGross)} ({pct}%)</span>
                          </div>
                          <div className="w-full h-2.5 bg-neutral-100 rounded-full overflow-hidden">
                            <div
                              className="h-full bg-gradient-to-r from-primary-500 to-indigo-600 rounded-full transition-all duration-500"
                              style={{ width: `${Math.max(5, pct)}%` }}
                            />
                          </div>
                        </div>
                      )
                    })}
                  </div>
                ) : (
                  <EmptyState
                    title="No Payroll Data Generated"
                    description="Click Calculate Payroll to batch calculate monthly numbers for all departments."
                    actionLabel="Calculate Payroll"
                    onAction={() => setIsRunConfirmOpen(true)}
                  />
                )}
              </Card>

              {/* Status Summary & Quick Stats */}
              <Card className="p-6 border border-neutral-200/80 shadow-2xs flex flex-col justify-between">
                <div>
                  <h3 className="text-sm font-bold text-neutral-900 tracking-tight flex items-center gap-2 mb-4">
                    <ShieldCheck className="w-4 h-4 text-emerald-600" />
                    Payroll Lifecycle Status
                  </h3>

                  <div className="p-4 rounded-xl bg-neutral-50 border border-neutral-200/60 space-y-3">
                    <div className="flex items-center justify-between text-xs">
                      <span className="text-neutral-500">Period:</span>
                      <span className="font-bold text-neutral-900">
                        {monthNames[selectedMonth - 1]} {selectedYear}
                      </span>
                    </div>
                    <div className="flex items-center justify-between text-xs">
                      <span className="text-neutral-500">Processed Employees:</span>
                      <span className="font-bold text-neutral-900">
                        {summary?.processedCount || 0} / {employees.length}
                      </span>
                    </div>
                    <div className="flex items-center justify-between text-xs">
                      <span className="text-neutral-500">Currency Standard:</span>
                      <span className="font-bold text-neutral-900">INR (₹) India</span>
                    </div>
                    <div className="flex items-center justify-between text-xs">
                      <span className="text-neutral-500">Approval State:</span>
                      <Badge
                        variant={
                          summary?.status === 'Approved' || summary?.status === 'Paid'
                            ? 'success'
                            : 'warning'
                        }
                      >
                        {summary?.status || 'Draft'}
                      </Badge>
                    </div>
                  </div>
                </div>

                <div className="pt-6 border-t border-neutral-100 mt-6 flex flex-col gap-2.5">
                  {isAdmin && summary?.status !== 'Approved' && (
                    <Button
                      variant="primary"
                      onClick={() => setIsApproveConfirmOpen(true)}
                      className="w-full shadow-xs bg-emerald-600 hover:bg-emerald-700 text-white"
                    >
                      <Lock className="w-4 h-4 mr-1.5" />
                      Approve & Finalize Batch
                    </Button>
                  )}
                  <Button
                    variant="outline"
                    onClick={() => setActiveTab('simulator')}
                    className="w-full text-xs"
                  >
                    <Calculator className="w-4 h-4 mr-1.5 text-purple-600" />
                    Run What-If Salary Simulation
                  </Button>
                </div>
              </Card>
            </div>
          </div>
        )}

        {/* 2. PAYROLL RUNS & BATCH PROCESSING */}
        {activeTab === 'runs' && (
          <Card className="p-6 border border-neutral-200/80 shadow-2xs space-y-6">
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
              <div>
                <h3 className="text-base font-bold text-neutral-900">
                  Payroll Batch Run — {monthNames[selectedMonth - 1]} {selectedYear}
                </h3>
                <p className="text-xs text-neutral-500 mt-0.5">
                  Deterministic calculation from Salary Structures, Real Attendance Sessions, Approved Leaves, and Claims
                </p>
              </div>

              <div className="flex items-center gap-3">
                <Button
                  variant="primary"
                  onClick={() => setIsRunConfirmOpen(true)}
                  className="shadow-xs"
                >
                  <Play className="w-4 h-4 mr-1.5" />
                  Recalculate Batch
                </Button>
              </div>
            </div>

            {summary?.payslips && summary.payslips.length > 0 ? (
              <div className="overflow-x-auto rounded-xl border border-neutral-200">
                <table className="w-full text-left text-xs text-neutral-600">
                  <thead className="bg-neutral-50 text-neutral-900 font-bold uppercase text-[10px] tracking-wider border-b border-neutral-200">
                    <tr>
                      <th className="py-3 px-4">Employee</th>
                      <th className="py-3 px-4">Department</th>
                      <th className="py-3 px-4 text-right">Basic</th>
                      <th className="py-3 px-4 text-right">HRA</th>
                      <th className="py-3 px-4 text-right">Gross</th>
                      <th className="py-3 px-4 text-right">PF & TDS</th>
                      <th className="py-3 px-4 text-right">Claims</th>
                      <th className="py-3 px-4 text-right">Net Pay</th>
                      <th className="py-3 px-4 text-center">Status</th>
                      <th className="py-3 px-4 text-center">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-neutral-200 font-medium">
                    {summary.payslips.map((p) => (
                      <tr key={p.id} className="hover:bg-neutral-50/80 transition">
                        <td className="py-3.5 px-4 font-semibold text-neutral-900">
                          {p.employeeName}
                          <span className="block text-[10px] font-normal text-neutral-400 font-mono">
                            {p.employeeCode || 'EMP-Active'}
                          </span>
                        </td>
                        <td className="py-3.5 px-4 text-neutral-700">{p.departmentName}</td>
                        <td className="py-3.5 px-4 text-right font-mono">{formatCurrencyINR(p.basicSalary)}</td>
                        <td className="py-3.5 px-4 text-right font-mono">{formatCurrencyINR(p.hra)}</td>
                        <td className="py-3.5 px-4 text-right font-mono font-bold text-neutral-900">
                          {formatCurrencyINR(p.grossSalary)}
                        </td>
                        <td className="py-3.5 px-4 text-right font-mono text-amber-600">
                          -{formatCurrencyINR(p.deductions)}
                        </td>
                        <td className="py-3.5 px-4 text-right font-mono text-purple-600">
                          +{formatCurrencyINR(p.reimbursementsAmount || 0)}
                        </td>
                        <td className="py-3.5 px-4 text-right font-mono font-bold text-emerald-600 text-sm">
                          {formatCurrencyINR(p.netSalary)}
                        </td>
                        <td className="py-3.5 px-4 text-center">
                          <Badge
                            variant={
                              p.status === 'Approved' || p.status === 'Paid'
                                ? 'success'
                                : 'default'
                            }
                          >
                            {p.status}
                          </Badge>
                        </td>
                        <td className="py-3.5 px-4 text-center">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => {
                              setSelectedPayslip(p)
                              setIsPayslipModalOpen(true)
                            }}
                          >
                            <Eye className="w-3.5 h-3.5 mr-1" />
                            Breakdown
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <EmptyState
                title="No Processed Batch Found"
                description="This month's payroll has not been calculated yet."
                actionLabel="Calculate Batch"
                onAction={() => setIsRunConfirmOpen(true)}
              />
            )}
          </Card>
        )}

        {/* 3. SALARY STRUCTURES */}
        {activeTab === 'structures' && (
          <Card className="p-6 border border-neutral-200/80 shadow-2xs space-y-6">
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
              <div>
                <h3 className="text-base font-bold text-neutral-900">Employee Salary Structures</h3>
                <p className="text-xs text-neutral-500 mt-0.5">
                  Base compensation and allowance configuration per employee
                </p>
              </div>
              <Button
                variant="primary"
                size="sm"
                onClick={() => setIsSalaryModalOpen(true)}
                className="shadow-2xs"
              >
                <Plus className="w-4 h-4 mr-1.5" />
                Update Structure
              </Button>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {employees.map((emp) => {
                const isVivek = emp.firstName?.includes('Vivek')
                const isAbhi = emp.firstName?.includes('Abhimanyu')
                const isBheem = emp.firstName?.includes('Bheem')
                const base = isVivek ? 85000 : isAbhi ? 95000 : isBheem ? 80000 : 70000
                const hra = base * 0.4
                const spec = base * 0.15

                return (
                  <div
                    key={emp.id}
                    className="p-5 rounded-xl border border-neutral-200 bg-white hover:border-primary-300 transition shadow-2xs flex flex-col justify-between"
                  >
                    <div>
                      <div className="flex items-center justify-between mb-3">
                        <div className="flex items-center gap-2.5">
                          <div className="w-9 h-9 rounded-full bg-primary-100 text-primary-700 flex items-center justify-center font-bold text-xs">
                            {emp.firstName?.charAt(0) || 'E'}
                          </div>
                          <div>
                            <h4 className="text-sm font-bold text-neutral-900">
                              {emp.firstName} {emp.lastName}
                            </h4>
                            <p className="text-[11px] text-neutral-400 font-mono">{emp.email}</p>
                          </div>
                        </div>
                        <Badge variant="success">Active</Badge>
                      </div>

                      <div className="space-y-2 py-3 border-y border-neutral-100 text-xs">
                        <div className="flex justify-between text-neutral-600">
                          <span>Base Salary:</span>
                          <span className="font-mono font-bold text-neutral-900">{formatCurrencyINR(base)}</span>
                        </div>
                        <div className="flex justify-between text-neutral-600">
                          <span>HRA (40%):</span>
                          <span className="font-mono text-neutral-800">{formatCurrencyINR(hra)}</span>
                        </div>
                        <div className="flex justify-between text-neutral-600">
                          <span>Special Allowance:</span>
                          <span className="font-mono text-neutral-800">{formatCurrencyINR(spec)}</span>
                        </div>
                        <div className="flex justify-between text-neutral-600 pt-1 border-t border-neutral-100 font-bold">
                          <span>Est. Monthly CTC:</span>
                          <span className="font-mono text-primary-700">{formatCurrencyINR(base + hra + spec)}</span>
                        </div>
                      </div>
                    </div>

                    <div className="mt-4 pt-2">
                      <Button
                        variant="outline"
                        size="sm"
                        className="w-full text-xs"
                        onClick={() => {
                          setSalEmployeeId(emp.id)
                          setSalBase(base.toString())
                          setIsSalaryModalOpen(true)
                        }}
                      >
                        Adjust Base Salary
                      </Button>
                    </div>
                  </div>
                )
              })}
            </div>
          </Card>
        )}

        {/* 4. REIMBURSEMENTS WORKFLOW */}
        {activeTab === 'reimbursements' && (
          <Card className="p-6 border border-neutral-200/80 shadow-2xs space-y-6">
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
              <div>
                <h3 className="text-base font-bold text-neutral-900">Reimbursements & Claims</h3>
                <p className="text-xs text-neutral-500 mt-0.5">
                  Employee expense claims submitted for business approval and payroll inclusion
                </p>
              </div>
              <Button
                variant="primary"
                size="sm"
                onClick={() => setIsClaimModalOpen(true)}
                className="shadow-2xs"
              >
                <Plus className="w-4 h-4 mr-1.5" />
                Submit New Claim
              </Button>
            </div>

            {reimbursements.length > 0 ? (
              <div className="overflow-x-auto rounded-xl border border-neutral-200">
                <table className="w-full text-left text-xs text-neutral-600">
                  <thead className="bg-neutral-50 text-neutral-900 font-bold uppercase text-[10px] tracking-wider border-b border-neutral-200">
                    <tr>
                      <th className="py-3 px-4">Employee</th>
                      <th className="py-3 px-4">Category</th>
                      <th className="py-3 px-4">Description</th>
                      <th className="py-3 px-4 text-right">Amount</th>
                      <th className="py-3 px-4 text-center">Submitted At (IST)</th>
                      <th className="py-3 px-4 text-center">Status</th>
                      {isElevated && <th className="py-3 px-4 text-center">Actions</th>}
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-neutral-200 font-medium">
                    {reimbursements.map((r) => (
                      <tr key={r.id} className="hover:bg-neutral-50/80 transition">
                        <td className="py-3.5 px-4 font-semibold text-neutral-900">
                          {r.employeeName || 'Employee'}
                        </td>
                        <td className="py-3.5 px-4">
                          <span className="px-2 py-0.5 rounded-md bg-neutral-100 text-neutral-700 font-medium text-[11px]">
                            {r.category}
                          </span>
                        </td>
                        <td className="py-3.5 px-4 text-neutral-700 max-w-xs truncate">
                          {r.description}
                        </td>
                        <td className="py-3.5 px-4 text-right font-mono font-bold text-neutral-900">
                          {formatCurrencyINR(r.amount)}
                        </td>
                        <td className="py-3.5 px-4 text-center text-neutral-500 font-mono text-[11px]">
                          {formatDateIST(r.createdAtUtc)}
                        </td>
                        <td className="py-3.5 px-4 text-center">
                          <Badge
                            variant={
                              r.status === 'Approved' || r.status === 'Paid'
                                ? 'success'
                                : r.status === 'Rejected'
                                ? 'danger'
                                : 'warning'
                            }
                          >
                            {r.status}
                          </Badge>
                        </td>
                        {isElevated && (
                          <td className="py-3.5 px-4 text-center">
                            {r.status === 'Pending' ? (
                              <div className="flex items-center justify-center gap-1.5">
                                <Button
                                  variant="primary"
                                  size="sm"
                                  className="h-7 px-2.5 text-[11px] bg-emerald-600 hover:bg-emerald-700"
                                  onClick={() => handleApproveClaim(r.id)}
                                >
                                  Approve
                                </Button>
                                <Button
                                  variant="danger"
                                  size="sm"
                                  className="h-7 px-2.5 text-[11px]"
                                  onClick={() => handleRejectClaim(r.id)}
                                >
                                  Reject
                                </Button>
                              </div>
                            ) : (
                              <span className="text-[11px] text-neutral-400">Processed</span>
                            )}
                          </td>
                        )}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <EmptyState
                icon={Receipt}
                title="No Reimbursement Claims"
                description="No expense claims submitted yet. Click below to submit your first claim."
                actionLabel="Submit Claim"
                onAction={() => setIsClaimModalOpen(true)}
              />
            )}
          </Card>
        )}

        {/* 5. DIGITAL PAYSLIPS */}
        {activeTab === 'payslips' && (
          <Card className="p-6 border border-neutral-200/80 shadow-2xs space-y-6">
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
              <div>
                <h3 className="text-base font-bold text-neutral-900">Digital Payslips History</h3>
                <p className="text-xs text-neutral-500 mt-0.5">
                  Official itemized monthly salary slips with printable preview
                </p>
              </div>
            </div>

            {payslips.length > 0 ? (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {payslips.map((p) => (
                  <div
                    key={p.id}
                    className="p-5 rounded-2xl border border-neutral-200 bg-white hover:border-primary-400 transition shadow-2xs flex flex-col justify-between"
                  >
                    <div>
                      <div className="flex items-center justify-between mb-3">
                        <div>
                          <span className="text-xs font-bold text-primary-700 uppercase tracking-wider">
                            {monthNames[p.month - 1]} {p.year}
                          </span>
                          <h4 className="text-sm font-bold text-neutral-900 mt-0.5">
                            {p.employeeName || 'Employee'}
                          </h4>
                        </div>
                        <Badge variant={p.status === 'Approved' || p.status === 'Paid' ? 'success' : 'default'}>
                          {p.status}
                        </Badge>
                      </div>

                      <div className="p-3 rounded-xl bg-neutral-50 border border-neutral-100 space-y-1.5 my-3 text-xs">
                        <div className="flex justify-between text-neutral-500">
                          <span>Gross Salary:</span>
                          <span className="font-mono text-neutral-800">{formatCurrencyINR(p.grossSalary)}</span>
                        </div>
                        <div className="flex justify-between text-neutral-500">
                          <span>Deductions (PF+TDS):</span>
                          <span className="font-mono text-amber-600">-{formatCurrencyINR(p.deductions)}</span>
                        </div>
                        {p.reimbursementsAmount ? (
                          <div className="flex justify-between text-neutral-500">
                            <span>Claims Reimbursed:</span>
                            <span className="font-mono text-purple-600">+{formatCurrencyINR(p.reimbursementsAmount)}</span>
                          </div>
                        ) : null}
                        <div className="flex justify-between text-neutral-900 font-bold pt-1.5 border-t border-neutral-200 text-sm">
                          <span>Take-Home Pay:</span>
                          <span className="font-mono text-emerald-600">{formatCurrencyINR(p.netSalary)}</span>
                        </div>
                      </div>
                    </div>

                    <div className="mt-2 flex gap-2">
                      <Button
                        variant="outline"
                        size="sm"
                        className="flex-1 text-xs"
                        onClick={() => {
                          setSelectedPayslip(p)
                          setIsPayslipModalOpen(true)
                        }}
                      >
                        <Eye className="w-3.5 h-3.5 mr-1" />
                        View Slip
                      </Button>
                      <Button
                        variant="secondary"
                        size="sm"
                        className="text-xs"
                        onClick={() => {
                          setSelectedPayslip(p)
                          setIsPayslipModalOpen(true)
                          setTimeout(() => window.print(), 300)
                        }}
                      >
                        <Download className="w-3.5 h-3.5" />
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState
                icon={FileText}
                title="No Payslips Generated"
                description="No historical payslips found for this selection."
              />
            )}
          </Card>
        )}

        {/* 6. WHAT-IF SALARY SIMULATOR */}
        {activeTab === 'simulator' && (
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <Card className="lg:col-span-2 p-6 border border-neutral-200/80 shadow-2xs space-y-6">
              <div>
                <div className="flex items-center gap-2">
                  <div className="p-2 rounded-xl bg-purple-50 text-purple-600">
                    <Calculator className="w-5 h-5" />
                  </div>
                  <div>
                    <h3 className="text-base font-bold text-neutral-900">What-If Salary Simulator</h3>
                    <p className="text-xs text-neutral-500 mt-0.5">
                      Simulate salary appraisals, performance bonuses, and overtime projections in real-time
                    </p>
                  </div>
                </div>
                <div className="mt-3 p-3 rounded-lg bg-blue-50 border border-blue-100 text-xs text-blue-700">
                  ℹ️ <strong>Note:</strong> Simulations operate client-side for forecasting and do not alter actual database records.
                </div>
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
                <div>
                  <label className="block text-xs font-semibold text-neutral-700 mb-1.5">
                    Current Base Salary (Monthly)
                  </label>
                  <div className="relative">
                    <span className="absolute left-3 top-1/2 -translate-y-1/2 text-neutral-400 font-bold text-sm">₹</span>
                    <input
                      type="number"
                      value={simBaseSalary}
                      onChange={(e) => setSimBaseSalary(Number(e.target.value))}
                      className="w-full pl-8 pr-4 py-2 border border-neutral-200 rounded-xl text-sm font-semibold focus:outline-none focus:ring-2 focus:ring-primary-500"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-xs font-semibold text-neutral-700 mb-1.5">
                    Increment / Hike Percentage ({simIncrementPercent}%)
                  </label>
                  <input
                    type="range"
                    min="0"
                    max="50"
                    step="1"
                    value={simIncrementPercent}
                    onChange={(e) => setSimIncrementPercent(Number(e.target.value))}
                    className="w-full accent-primary-600 cursor-pointer mt-2"
                  />
                </div>

                <div>
                  <label className="block text-xs font-semibold text-neutral-700 mb-1.5">
                    Projected Performance Bonus
                  </label>
                  <div className="relative">
                    <span className="absolute left-3 top-1/2 -translate-y-1/2 text-neutral-400 font-bold text-sm">₹</span>
                    <input
                      type="number"
                      value={simBonus}
                      onChange={(e) => setSimBonus(Number(e.target.value))}
                      className="w-full pl-8 pr-4 py-2 border border-neutral-200 rounded-xl text-sm font-semibold focus:outline-none focus:ring-2 focus:ring-primary-500"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-xs font-semibold text-neutral-700 mb-1.5">
                    Estimated Overtime Hours ({simOvertimeHours} hrs)
                  </label>
                  <input
                    type="range"
                    min="0"
                    max="40"
                    step="1"
                    value={simOvertimeHours}
                    onChange={(e) => setSimOvertimeHours(Number(e.target.value))}
                    className="w-full accent-indigo-600 cursor-pointer mt-2"
                  />
                </div>
              </div>
            </Card>

            {/* Projection Card */}
            <Card className="p-6 border border-neutral-200/80 shadow-2xs bg-gradient-to-br from-white to-neutral-50/60 flex flex-col justify-between">
              <div>
                <h4 className="text-xs font-bold uppercase tracking-wider text-neutral-500 mb-4">
                  Simulation Results
                </h4>

                <div className="space-y-3 text-xs">
                  <div className="flex justify-between text-neutral-600">
                    <span>New Base Salary:</span>
                    <span className="font-mono font-bold text-neutral-900">{formatCurrencyINR(simNewBase)}</span>
                  </div>
                  <div className="flex justify-between text-neutral-600">
                    <span>HRA (40%):</span>
                    <span className="font-mono text-neutral-800">{formatCurrencyINR(simHra)}</span>
                  </div>
                  <div className="flex justify-between text-neutral-600">
                    <span>Special Allowance (15%):</span>
                    <span className="font-mono text-neutral-800">{formatCurrencyINR(simAllowances)}</span>
                  </div>
                  <div className="flex justify-between text-neutral-600">
                    <span>Performance Bonus:</span>
                    <span className="font-mono text-purple-600">+{formatCurrencyINR(simBonus)}</span>
                  </div>
                  <div className="flex justify-between text-neutral-600">
                    <span>Overtime Pay (1.5x):</span>
                    <span className="font-mono text-indigo-600">+{formatCurrencyINR(simOvertimePay)}</span>
                  </div>
                  <div className="flex justify-between text-neutral-600 pt-2 border-t border-neutral-200">
                    <span>PF (12% Base):</span>
                    <span className="font-mono text-amber-600">-{formatCurrencyINR(simPf)}</span>
                  </div>
                  <div className="flex justify-between text-neutral-600">
                    <span>Estimated TDS:</span>
                    <span className="font-mono text-amber-600">-{formatCurrencyINR(simTds)}</span>
                  </div>
                </div>
              </div>

              <div className="mt-6 pt-4 border-t border-neutral-200">
                <div className="p-4 rounded-xl bg-emerald-50 border border-emerald-100 text-center">
                  <p className="text-xs font-semibold text-emerald-800 uppercase tracking-wider">
                    Projected Take-Home Pay
                  </p>
                  <p className="text-2xl font-bold text-emerald-700 mt-1 font-mono">
                    {formatCurrencyINR(simNet)}
                  </p>
                  <p className="text-[11px] text-emerald-600 mt-1 font-semibold">
                    +{formatCurrencyINR(Math.max(0, simNet - currentNet))} projected gain / mo
                  </p>
                </div>
              </div>
            </Card>
          </div>
        )}

        {/* 7. TAX & STATUTORY RULES */}
        {activeTab === 'compliance' && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <Card className="p-6 border border-neutral-200/80 shadow-2xs space-y-4">
              <div className="flex items-center gap-2.5">
                <div className="p-2 rounded-xl bg-blue-50 text-blue-600">
                  <ShieldCheck className="w-5 h-5" />
                </div>
                <div>
                  <h4 className="text-sm font-bold text-neutral-900">Provident Fund (PF)</h4>
                  <p className="text-[11px] text-neutral-400">EPFO India Compliant</p>
                </div>
              </div>
              <p className="text-xs text-neutral-600 leading-relaxed">
                Standard employee contribution is <strong>12% of basic salary</strong>. Matched equally by employer share under the Employees Provident Fund Act.
              </p>
              <div className="p-3 rounded-lg bg-neutral-50 text-xs font-mono text-neutral-700">
                Rate: 12.00% on Basic
              </div>
            </Card>

            <Card className="p-6 border border-neutral-200/80 shadow-2xs space-y-4">
              <div className="flex items-center gap-2.5">
                <div className="p-2 rounded-xl bg-emerald-50 text-emerald-600">
                  <ShieldCheck className="w-5 h-5" />
                </div>
                <div>
                  <h4 className="text-sm font-bold text-neutral-900">ESIC Coverage</h4>
                  <p className="text-[11px] text-neutral-400">Health Insurance Rule</p>
                </div>
              </div>
              <p className="text-xs text-neutral-600 leading-relaxed">
                Applicable for employees with gross monthly wages up to <strong>₹21,000</strong>. Employee contribution is <strong>0.75%</strong> and employer is 3.25%.
              </p>
              <div className="p-3 rounded-lg bg-neutral-50 text-xs font-mono text-neutral-700">
                Threshold: ₹21,000 / mo
              </div>
            </Card>

            <Card className="p-6 border border-neutral-200/80 shadow-2xs space-y-4">
              <div className="flex items-center gap-2.5">
                <div className="p-2 rounded-xl bg-purple-50 text-purple-600">
                  <ShieldCheck className="w-5 h-5" />
                </div>
                <div>
                  <h4 className="text-sm font-bold text-neutral-900">Income Tax (TDS)</h4>
                  <p className="text-[11px] text-neutral-400">Section 192 Income Tax Act</p>
                </div>
              </div>
              <p className="text-xs text-neutral-600 leading-relaxed">
                Tax deducted at source based on annualized projected income slabs and declarations under the New Tax Regime (Section 115BAC).
              </p>
              <div className="p-3 rounded-lg bg-neutral-50 text-xs font-mono text-neutral-700">
                Regime: 115BAC Default
              </div>
            </Card>
          </div>
        )}

        {/* MODAL 1: DETAILED DIGITAL PAYSLIP MODAL */}
        {isPayslipModalOpen && selectedPayslip && (
          <Modal
            isOpen={isPayslipModalOpen}
            onClose={() => setIsPayslipModalOpen(false)}
            title="Official Digital Payslip"
          >
            <div className="space-y-6 text-xs text-neutral-700" id="printable-payslip">
              {/* Company & Employee Header */}
              <div className="p-4 rounded-xl bg-neutral-50 border border-neutral-200 flex justify-between items-start">
                <div>
                  <h3 className="text-base font-bold text-neutral-900">Employee Lifecycle Portal</h3>
                  <p className="text-[11px] text-neutral-500">Tech Park, Bengaluru, Karnataka, India</p>
                  <p className="text-[11px] text-neutral-400 font-mono mt-1">CIN: REG-2026-IN-001</p>
                </div>
                <div className="text-right">
                  <span className="text-xs font-bold text-primary-700 uppercase tracking-wider block">
                    Payslip — {monthNames[selectedPayslip.month - 1]} {selectedPayslip.year}
                  </span>
                  <span className="text-[10px] text-neutral-400 block mt-1">
                    Generated: {formatDateIST(selectedPayslip.generatedDateUtc)}
                  </span>
                </div>
              </div>

              {/* Employee Metadata */}
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 p-3 rounded-xl border border-neutral-200">
                <div>
                  <span className="text-[10px] text-neutral-400 uppercase">Employee Name</span>
                  <p className="font-bold text-neutral-900">{selectedPayslip.employeeName}</p>
                </div>
                <div>
                  <span className="text-[10px] text-neutral-400 uppercase">Employee ID</span>
                  <p className="font-mono text-neutral-900">{selectedPayslip.employeeCode || 'EMP-Active'}</p>
                </div>
                <div>
                  <span className="text-[10px] text-neutral-400 uppercase">Department</span>
                  <p className="text-neutral-900">{selectedPayslip.departmentName || 'Engineering'}</p>
                </div>
                <div>
                  <span className="text-[10px] text-neutral-400 uppercase">Payment Mode</span>
                  <p className="text-neutral-900 font-medium">{selectedPayslip.paymentMethod || 'Bank Transfer'}</p>
                </div>
              </div>

              {/* Earnings vs Deductions Breakdown */}
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                {/* Earnings */}
                <div className="p-4 rounded-xl border border-neutral-200 bg-white space-y-2">
                  <h4 className="font-bold text-neutral-900 pb-2 border-b border-neutral-100 flex items-center justify-between">
                    <span>Earnings</span>
                    <span className="text-[10px] font-mono text-neutral-400">INR (₹)</span>
                  </h4>
                  <div className="flex justify-between">
                    <span>Basic Salary:</span>
                    <span className="font-mono font-medium">{formatCurrencyINR(selectedPayslip.basicSalary)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>House Rent Allowance (HRA):</span>
                    <span className="font-mono font-medium">{formatCurrencyINR(selectedPayslip.hra)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Special & Other Allowances:</span>
                    <span className="font-mono font-medium">{formatCurrencyINR(selectedPayslip.allowances)}</span>
                  </div>
                  {selectedPayslip.bonusPay ? (
                    <div className="flex justify-between text-purple-600">
                      <span>Performance Bonus:</span>
                      <span className="font-mono font-medium">+{formatCurrencyINR(selectedPayslip.bonusPay)}</span>
                    </div>
                  ) : null}
                  <div className="flex justify-between pt-2 border-t border-neutral-100 font-bold text-neutral-900">
                    <span>Total Gross Earnings:</span>
                    <span className="font-mono text-primary-700">{formatCurrencyINR(selectedPayslip.grossSalary)}</span>
                  </div>
                </div>

                {/* Deductions */}
                <div className="p-4 rounded-xl border border-neutral-200 bg-white space-y-2">
                  <h4 className="font-bold text-neutral-900 pb-2 border-b border-neutral-100 flex items-center justify-between">
                    <span>Deductions</span>
                    <span className="text-[10px] font-mono text-neutral-400">INR (₹)</span>
                  </h4>
                  <div className="flex justify-between">
                    <span>Provident Fund (PF):</span>
                    <span className="font-mono font-medium">{formatCurrencyINR(selectedPayslip.pfDeduction)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Tax Deducted at Source (TDS):</span>
                    <span className="font-mono font-medium">{formatCurrencyINR(selectedPayslip.tdsDeduction)}</span>
                  </div>
                  {selectedPayslip.esiDeduction ? (
                    <div className="flex justify-between">
                      <span>ESIC:</span>
                      <span className="font-mono font-medium">{formatCurrencyINR(selectedPayslip.esiDeduction)}</span>
                    </div>
                  ) : null}
                  <div className="flex justify-between pt-2 border-t border-neutral-100 font-bold text-amber-700">
                    <span>Total Deductions:</span>
                    <span className="font-mono">-{formatCurrencyINR(selectedPayslip.deductions)}</span>
                  </div>
                </div>
              </div>

              {/* Net Payable Banner */}
              <div className="p-4 rounded-xl bg-emerald-50 border border-emerald-200 flex justify-between items-center">
                <div>
                  <span className="text-xs font-semibold text-emerald-800 uppercase tracking-wider block">
                    Net Take-Home Pay
                  </span>
                  <span className="text-[10px] text-emerald-600">
                    Gross Earnings - Deductions {selectedPayslip.reimbursementsAmount ? '+ Approved Claims' : ''}
                  </span>
                </div>
                <div className="text-right">
                  <span className="text-2xl font-bold text-emerald-700 font-mono">
                    {formatCurrencyINR(selectedPayslip.netSalary)}
                  </span>
                </div>
              </div>

              {/* Modal Footer Actions */}
              <div className="flex justify-end gap-3 pt-2">
                <Button variant="secondary" onClick={() => setIsPayslipModalOpen(false)}>
                  Close
                </Button>
                <Button variant="primary" onClick={() => window.print()}>
                  <Printer className="w-4 h-4 mr-1.5" />
                  Print Payslip
                </Button>
              </div>
            </div>
          </Modal>
        )}

        {/* MODAL 2: SUBMIT REIMBURSEMENT MODAL */}
        {isClaimModalOpen && (
          <Modal
            isOpen={isClaimModalOpen}
            onClose={() => setIsClaimModalOpen(false)}
            title="Submit Expense Reimbursement"
          >
            <form onSubmit={handleSubmitClaim} className="space-y-4 text-xs">
              <div>
                <label className="block font-semibold text-neutral-700 mb-1">Expense Category</label>
                <select
                  value={claimCategory}
                  onChange={(e) => setClaimCategory(e.target.value)}
                  className="w-full px-3 py-2 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary-500 font-medium"
                >
                  <option value="Travel">Travel & Local Conveyance</option>
                  <option value="Food">Meals & Client Entertainment</option>
                  <option value="Internet">Internet & Broadband</option>
                  <option value="Medical">Medical & Wellness</option>
                  <option value="Equipment">Hardware & Office Equipment</option>
                  <option value="Training">Certifications & Training</option>
                  <option value="Other">Other Miscellaneous</option>
                </select>
              </div>

              <div>
                <label className="block font-semibold text-neutral-700 mb-1">Claim Amount (₹)</label>
                <input
                  type="number"
                  placeholder="e.g. 2500"
                  value={claimAmount}
                  onChange={(e) => setClaimAmount(e.target.value)}
                  required
                  className="w-full px-3 py-2 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary-500 font-medium"
                />
              </div>

              <div>
                <label className="block font-semibold text-neutral-700 mb-1">Description / Business Purpose</label>
                <textarea
                  rows={3}
                  placeholder="Details of expense and business justification..."
                  value={claimDesc}
                  onChange={(e) => setClaimDesc(e.target.value)}
                  required
                  className="w-full px-3 py-2 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary-500 font-medium"
                />
              </div>

              <div className="flex justify-end gap-3 pt-3">
                <Button variant="secondary" onClick={() => setIsClaimModalOpen(false)} disabled={isSubmittingClaim}>
                  Cancel
                </Button>
                <Button variant="primary" type="submit" isLoading={isSubmittingClaim}>
                  Submit Claim
                </Button>
              </div>
            </form>
          </Modal>
        )}

        {/* MODAL 3: UPDATE BASE SALARY STRUCTURE */}
        {isSalaryModalOpen && (
          <Modal
            isOpen={isSalaryModalOpen}
            onClose={() => setIsSalaryModalOpen(false)}
            title="Update Base Salary Structure"
          >
            <div className="space-y-4 text-xs">
              <div>
                <label className="block font-semibold text-neutral-700 mb-1">Select Employee</label>
                <select
                  value={salEmployeeId}
                  onChange={(e) => setSalEmployeeId(e.target.value)}
                  className="w-full px-3 py-2 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary-500 font-medium"
                >
                  <option value="">-- Choose Employee --</option>
                  {employees.map((e) => (
                    <option key={e.id} value={e.id}>
                      {e.firstName} {e.lastName} ({e.email})
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block font-semibold text-neutral-700 mb-1">Base Monthly Salary (₹)</label>
                <input
                  type="number"
                  value={salBase}
                  onChange={(e) => setSalBase(e.target.value)}
                  className="w-full px-3 py-2 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary-500 font-medium"
                />
                <p className="text-[11px] text-neutral-400 mt-1">
                  HRA (40%) and Special Allowances (15%) will be calculated automatically based on this base.
                </p>
              </div>

              <div className="flex justify-end gap-3 pt-3">
                <Button variant="secondary" onClick={() => setIsSalaryModalOpen(false)} disabled={isSubmittingSal}>
                  Cancel
                </Button>
                <Button variant="primary" onClick={handleUpdateSalary} isLoading={isSubmittingSal}>
                  Save Structure
                </Button>
              </div>
            </div>
          </Modal>
        )}

        {/* CONFIRMATION DIALOGS */}
        <ConfirmModal
          isOpen={isRunConfirmOpen}
          onClose={() => setIsRunConfirmOpen(false)}
          onConfirm={handleRunPayroll}
          title={`Batch Calculate ${monthNames[selectedMonth - 1]} ${selectedYear} Payroll`}
          message={`This will compute payroll for all active employees based on current Salary Structures, real Attendance records for ${monthNames[selectedMonth - 1]}, approved leaves, and approved claims.`}
          confirmLabel="Calculate Now"
          variant="info"
          isLoading={isRunningPayroll}
        />

        <ConfirmModal
          isOpen={isApproveConfirmOpen}
          onClose={() => setIsApproveConfirmOpen(false)}
          onConfirm={handleApprovePayroll}
          title={`Approve & Finalize ${monthNames[selectedMonth - 1]} ${selectedYear} Payroll`}
          message="Approving will lock this payroll batch, finalize all employee payslips, and mark associated reimbursements as paid."
          confirmLabel="Approve & Lock"
          variant="warning"
          isLoading={isApprovingPayroll}
        />
      </div>
    </MainLayout>
  )
}
