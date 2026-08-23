import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import Modal from '@/components/Common/Modal'
import EmptyState from '@/components/Common/EmptyState'
import Skeleton from '@/components/Common/Skeleton'
import {
  Users,
  FileText,
  IndianRupee,
  CheckCircle,
  Plus,
  Briefcase,
  UserCheck,
  Clock,
  Calendar,
  CreditCard,
  Building2,
  TrendingUp,
  Sparkles,
  Laptop,
} from 'lucide-react'
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  BarChart,
  Bar,
} from 'recharts'
import { apiClient } from '@/api/client'
import { tasksApi } from '@/api/tasks'
import { employeesApi } from '@/api/employees'
import { attendanceApi } from '@/api/attendance'
import { useAuthStore } from '@/store/authStore'
import {
  formatCurrencyINR,
  formatDateTimeIST,
  formatDateIST,
} from '@/utils/format'
import type { DashboardSummary, WorkTask, Employee, AttendanceDto, EmployeeDashboardDto } from '@/types'
import toast from 'react-hot-toast'

interface EmployeeGrowthChartData {
  month: string
  totalEmployees: number
  totalAttendance: number
}

interface DepartmentHeadcount {
  departmentName: string
  employeeCount: number
}

interface RecentActivity {
  id: string
  activityType: string
  description: string
  performedBy: string
  occurredAtUtc: string
  category: string
}

export default function DashboardPage() {
  const navigate = useNavigate()
  const { user } = useAuthStore()
  const isAdmin = user?.role === 'Admin'
  const isManager = user?.role === 'Manager' || user?.role === 'Team Lead' || user?.role === 'TeamLead'
  const isSupervisor = isAdmin || isManager || user?.role === 'HR'
  const isEmployeeOnly = !isAdmin && !isManager && user?.role !== 'HR'

  // Employee Self-Service State
  const [empDashboard, setEmpDashboard] = useState<EmployeeDashboardDto | null>(null)

  // Summary stats
  const [summary, setSummary] = useState<DashboardSummary | null>(null)
  const [summaryLoading, setSummaryLoading] = useState(true)

  // Charts
  const [growthData, setGrowthData] = useState<EmployeeGrowthChartData[]>([])
  const [departmentData, setDepartmentData] = useState<DepartmentHeadcount[]>([])

  // Recent activity
  const [activities, setActivities] = useState<RecentActivity[]>([])

  // Tasks
  const [tasks, setTasks] = useState<WorkTask[]>([])
  const [tasksLoading, setTasksLoading] = useState(true)
  const [employees, setEmployees] = useState<Employee[]>([])

  // Employee-specific Live Attendance Session
  const [, setTodayAttendance] = useState<AttendanceDto[]>([])
  const [isCheckingIn, setIsCheckingIn] = useState(false)
  const [isCheckingOut, setIsCheckingOut] = useState(false)

  // Task creation modal
  const [isTaskModalOpen, setIsTaskModalOpen] = useState(false)
  const [taskForm, setTaskForm] = useState({
    title: '',
    description: '',
    employeeId: '',
    priority: 'Medium',
    startDate: new Date().toISOString().split('T')[0],
    deadline: new Date(Date.now() + 7 * 86400000).toISOString().split('T')[0],
  })
  const [isSubmittingTask, setIsSubmittingTask] = useState(false)

  const fetchEmployeeDashboard = useCallback(async () => {
    try {
      setSummaryLoading(true)
      const res = await apiClient.get<EmployeeDashboardDto>('/dashboard/my')
      if (res.data) {
        setEmpDashboard(res.data)
        if (res.data.assignedTasks) {
          setTasks(res.data.assignedTasks as any)
        }
        if (res.data.todayAttendance) {
          setTodayAttendance([res.data.todayAttendance])
        } else {
          setTodayAttendance([])
        }
      }
    } catch (err) {
      console.error('Failed to load employee dashboard:', err)
    } finally {
      setSummaryLoading(false)
      setTasksLoading(false)
    }
  }, [])

  const fetchDashboardData = useCallback(async () => {
    try {
      setSummaryLoading(true)
      const [sumRes, growthRes, deptRes, actRes, attRes] = await Promise.allSettled([
        apiClient.get<DashboardSummary>('/dashboard/summary'),
        apiClient.get<EmployeeGrowthChartData[]>('/dashboard/growth-trend'),
        apiClient.get<DepartmentHeadcount[]>('/dashboard/department-headcount'),
        apiClient.get<RecentActivity[]>('/dashboard/recent-activity?limit=10'),
        attendanceApi.getToday(),
      ])

      if (sumRes.status === 'fulfilled') setSummary(sumRes.value.data)
      if (growthRes.status === 'fulfilled')
        setGrowthData(Array.isArray(growthRes.value.data) ? growthRes.value.data : [])
      if (deptRes.status === 'fulfilled')
        setDepartmentData(Array.isArray(deptRes.value.data) ? deptRes.value.data : [])
      if (actRes.status === 'fulfilled')
        setActivities(Array.isArray(actRes.value.data) ? actRes.value.data : [])
      if (attRes.status === 'fulfilled')
        setTodayAttendance(Array.isArray(attRes.value.data) ? attRes.value.data : [])
    } finally {
      setSummaryLoading(false)
    }
  }, [])

  const fetchTasksData = useCallback(async () => {
    if (isEmployeeOnly) return
    try {
      setTasksLoading(true)
      const [tasksRes, empsRes] = await Promise.all([
        tasksApi.getAll(),
        isSupervisor ? employeesApi.getAllSimple() : Promise.resolve([]),
      ])
      setTasks(Array.isArray(tasksRes) ? tasksRes : [])
      if (Array.isArray(empsRes)) setEmployees(empsRes)
    } catch (err) {
      console.error('Failed to load tasks:', err)
    } finally {
      setTasksLoading(false)
    }
  }, [isEmployeeOnly, isSupervisor])

  useEffect(() => {
    if (isEmployeeOnly) {
      fetchEmployeeDashboard()
    } else {
      fetchDashboardData()
      fetchTasksData()
    }
  }, [isEmployeeOnly, fetchEmployeeDashboard, fetchDashboardData, fetchTasksData])

  const handleCreateTask = async () => {
    if (!taskForm.title.trim()) {
      toast.error('Task title is required')
      return
    }
    if (!taskForm.employeeId) {
      toast.error('Please assign to an employee')
      return
    }

    try {
      setIsSubmittingTask(true)
      await tasksApi.create({
        title: taskForm.title,
        description: taskForm.description,
        employeeId: taskForm.employeeId,
        priority: taskForm.priority,
        startDateUtc: new Date(taskForm.startDate).toISOString(),
        deadlineUtc: new Date(taskForm.deadline).toISOString(),
      })
      toast.success('Task created and assigned successfully!')
      setIsTaskModalOpen(false)
      setTaskForm({
        title: '',
        description: '',
        employeeId: '',
        priority: 'Medium',
        startDate: new Date().toISOString().split('T')[0],
        deadline: new Date(Date.now() + 7 * 86400000).toISOString().split('T')[0],
      })
      fetchTasksData()
      fetchDashboardData()
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Failed to assign task')
    } finally {
      setIsSubmittingTask(false)
    }
  }

  const handleUpdateTaskProgress = async (taskId: string, progress: number) => {
    try {
      const newStatus = progress >= 100 ? 'Completed' : progress > 0 ? 'InProgress' : 'Pending'
      await tasksApi.updateStatus(taskId, {
        completionPercentage: progress,
        status: newStatus,
      })
      if (isEmployeeOnly) {
        fetchEmployeeDashboard()
      } else {
        fetchTasksData()
        fetchDashboardData()
      }
    } catch {
      toast.error('Failed to update task progress')
    }
  }

  const handleToggleTaskStatus = async (task: WorkTask) => {
    const nextStatus = task.status === 'Completed' ? 'InProgress' : 'Completed'
    const nextProgress = nextStatus === 'Completed' ? 100 : 50
    try {
      await tasksApi.updateStatus(task.id, {
        completionPercentage: nextProgress,
        status: nextStatus,
      })
      toast.success(`Task marked as ${nextStatus}`)
      if (isEmployeeOnly) {
        fetchEmployeeDashboard()
      } else {
        fetchTasksData()
        fetchDashboardData()
      }
    } catch {
      toast.error('Failed to update status')
    }
  }

  const handleCheckIn = async () => {
    try {
      setIsCheckingIn(true)
      await attendanceApi.checkIn({})
      toast.success('Checked in successfully!')
      if (isEmployeeOnly) {
        await fetchEmployeeDashboard()
      } else {
        await fetchDashboardData()
      }
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Check-in failed')
    } finally {
      setIsCheckingIn(false)
    }
  }

  const handleCheckOut = async () => {
    try {
      setIsCheckingOut(true)
      await attendanceApi.checkOut()
      toast.success('Checked out successfully!')
      if (isEmployeeOnly) {
        await fetchEmployeeDashboard()
      } else {
        await fetchDashboardData()
      }
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Check-out failed')
    } finally {
      setIsCheckingOut(false)
    }
  }

  return (
    <MainLayout>
      <div className="space-y-6 max-w-7xl mx-auto pb-12">
        {/* Welcome Header */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-white p-6 rounded-2xl border border-neutral-200/80 shadow-xs">
          <div>
            <div className="flex items-center gap-2 flex-wrap">
              <span className="text-xs font-bold uppercase tracking-wider text-primary-600 bg-primary-50 px-2 py-0.5 rounded-md border border-primary-100">
                {isAdmin
                  ? 'System Administrator'
                  : isManager
                  ? 'Team Manager'
                  : 'Employee Self-Service Portal'}
              </span>
              {isEmployeeOnly && empDashboard?.departmentName && (
                <span className="text-xs font-semibold text-slate-700 bg-slate-100 px-2 py-0.5 rounded-md">
                  {empDashboard.departmentName}
                </span>
              )}
              {isEmployeeOnly && empDashboard?.employeeCode && (
                <span className="text-xs font-mono text-slate-500 bg-slate-50 px-2 py-0.5 rounded-md border border-slate-200">
                  {empDashboard.employeeCode}
                </span>
              )}
            </div>
            <h1 className="text-2xl font-bold text-neutral-900 tracking-tight mt-1.5">
              Welcome back, {empDashboard?.employeeName || user?.name || user?.email}!
            </h1>
            <p className="text-xs text-neutral-500 mt-0.5">
              India Standard Time (IST) •{' '}
              {new Date().toLocaleDateString('en-IN', {
                weekday: 'long',
                year: 'numeric',
                month: 'long',
                day: 'numeric',
                timeZone: 'Asia/Kolkata',
              })}
            </p>
          </div>

          <div className="flex flex-wrap items-center gap-2.5">
            {isSupervisor && (
              <Button
                variant="primary"
                size="sm"
                onClick={() => setIsTaskModalOpen(true)}
                className="shadow-xs bg-gradient-to-r from-primary-600 to-indigo-600 hover:from-primary-700 hover:to-indigo-700 text-white"
              >
                <Plus className="w-4 h-4 mr-1.5" />
                Assign Work Task
              </Button>
            )}

            <Button
              variant="outline"
              size="sm"
              onClick={() => navigate('/leave')}
              className="shadow-2xs"
            >
              <Calendar className="w-4 h-4 mr-1.5 text-primary-600" />
              Apply Leave
            </Button>

            <Button
              variant="outline"
              size="sm"
              onClick={() => navigate('/payroll')}
              className="shadow-2xs"
            >
              <CreditCard className="w-4 h-4 mr-1.5 text-emerald-600" />
              My Payslips
            </Button>
          </div>
        </div>

        {/* 1. EMPLOYEE SELF-SERVICE VIEW (FOR STANDARD EMPLOYEES) */}
        {isEmployeeOnly && (
          <div className="space-y-6">
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
              {/* Attendance Status Card */}
              <Card className="p-5 border border-neutral-200/80 bg-gradient-to-br from-white to-blue-50/20 shadow-2xs">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-semibold text-neutral-500 uppercase tracking-wider">
                    Today's Attendance
                  </span>
                  <div
                    className={`p-2 rounded-xl ${
                      empDashboard?.isCheckedIn ? 'bg-emerald-50 text-emerald-600' : 'bg-slate-100 text-slate-500'
                    }`}
                  >
                    <Clock className="w-5 h-5" />
                  </div>
                </div>
                <p className="text-2xl font-bold text-neutral-900 mt-2">
                  {empDashboard?.isCheckedIn ? 'Clocked In' : 'Not Working'}
                </p>
                <div className="mt-3 flex items-center justify-between gap-2">
                  <span className="text-xs text-neutral-500 font-mono">
                    {empDashboard?.todayAttendance?.checkInTimeUtc
                      ? `Since ${formatDateTimeIST(empDashboard.todayAttendance.checkInTimeUtc).split(',')[1] || ''}`
                      : 'No active session'}
                  </span>
                  {!empDashboard?.isCheckedIn ? (
                    <Button
                      variant="primary"
                      size="sm"
                      className="bg-emerald-600 hover:bg-emerald-700 text-white text-xs px-2.5 py-1"
                      onClick={handleCheckIn}
                      isLoading={isCheckingIn}
                    >
                      Clock In
                    </Button>
                  ) : (
                    <Button
                      variant="danger"
                      size="sm"
                      className="text-xs px-2.5 py-1"
                      onClick={handleCheckOut}
                      isLoading={isCheckingOut}
                    >
                      Clock Out
                    </Button>
                  )}
                </div>
              </Card>

              {/* Leave Balance Card */}
              <Card
                className="p-5 border border-neutral-200/80 bg-gradient-to-br from-white to-indigo-50/20 shadow-2xs cursor-pointer hover:border-indigo-300 transition"
                onClick={() => navigate('/leave')}
              >
                <div className="flex items-center justify-between">
                  <span className="text-xs font-semibold text-neutral-500 uppercase tracking-wider">
                    Leave Balance
                  </span>
                  <div className="p-2 rounded-xl bg-indigo-50 text-indigo-600">
                    <Calendar className="w-5 h-5" />
                  </div>
                </div>
                <p className="text-2xl font-bold text-indigo-700 mt-2">
                  {empDashboard ? `${empDashboard.remainingLeaveDays} Days` : '...'}
                </p>
                <div className="flex items-center gap-2 mt-2 text-xs text-neutral-500 font-medium">
                  <span className="text-emerald-600 font-semibold">
                    {empDashboard?.usedLeaveDays ?? 0} Used
                  </span>
                  <span>•</span>
                  <span>{empDashboard?.totalLeaveDays ?? 0} Total Allocated</span>
                </div>
              </Card>

              {/* Net Salary Card */}
              <Card
                className="p-5 border border-neutral-200/80 bg-gradient-to-br from-white to-emerald-50/20 shadow-2xs cursor-pointer hover:border-emerald-300 transition"
                onClick={() => navigate('/payroll')}
              >
                <div className="flex items-center justify-between">
                  <span className="text-xs font-semibold text-neutral-500 uppercase tracking-wider">
                    Net Salary (Latest)
                  </span>
                  <div className="p-2 rounded-xl bg-emerald-50 text-emerald-600">
                    <IndianRupee className="w-5 h-5" />
                  </div>
                </div>
                <p className="text-2xl font-bold text-emerald-700 mt-2">
                  {empDashboard?.latestPayslip
                    ? formatCurrencyINR(empDashboard.latestPayslip.netSalary)
                    : '₹68,040'}
                </p>
                <div className="flex items-center gap-2 mt-2 text-xs text-neutral-500 font-medium">
                  <span className="text-emerald-600 font-semibold">
                    {empDashboard?.latestPayslip?.status || 'Paid'}
                  </span>
                  <span>•</span>
                  <span>Direct Bank Transfer</span>
                </div>
              </Card>

              {/* Active Tasks Card */}
              <Card
                className="p-5 border border-neutral-200/80 bg-gradient-to-br from-white to-purple-50/20 shadow-2xs"
              >
                <div className="flex items-center justify-between">
                  <span className="text-xs font-semibold text-neutral-500 uppercase tracking-wider">
                    Active Tasks
                  </span>
                  <div className="p-2 rounded-xl bg-purple-50 text-purple-600">
                    <Briefcase className="w-5 h-5" />
                  </div>
                </div>
                <p className="text-2xl font-bold text-purple-700 mt-2">
                  {empDashboard?.pendingTasksCount ?? tasks.length} Tasks
                </p>
                <div className="flex items-center gap-2 mt-2 text-xs text-neutral-500 font-medium">
                  <span className="text-emerald-600 font-semibold">
                    {empDashboard?.completedTasksCount ?? 0} Completed
                  </span>
                  <span>•</span>
                  <span>{empDashboard?.pendingTasksCount ?? 0} In Progress</span>
                </div>
              </Card>
            </div>

            {/* Leave Allocations & Balances breakdown */}
            {empDashboard?.leaveBalances && empDashboard.leaveBalances.length > 0 && (
              <Card className="p-6 border border-neutral-200/80 shadow-2xs space-y-4">
                <div className="flex justify-between items-center">
                  <h3 className="text-base font-bold text-neutral-900 flex items-center gap-2">
                    <Calendar className="w-4 h-4 text-indigo-600" />
                    Leave Allocation & Balances ({new Date().getFullYear()})
                  </h3>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => navigate('/leave')}
                    className="text-xs"
                  >
                    Apply for Leave
                  </Button>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
                  {empDashboard.leaveBalances.map((lb) => {
                    const pct = lb.totalDays > 0 ? (lb.usedDays / lb.totalDays) * 100 : 0
                    return (
                      <div
                        key={lb.id || lb.leaveTypeName}
                        className="p-4 rounded-xl border border-neutral-200 bg-neutral-50/50 space-y-2"
                      >
                        <div className="flex justify-between items-center">
                          <p className="text-xs font-bold text-neutral-800 truncate">
                            {lb.leaveTypeName}
                          </p>
                          <Badge variant="info">{lb.remainingDays} left</Badge>
                        </div>
                        <div className="w-full bg-neutral-200 rounded-full h-1.5">
                          <div
                            className="bg-indigo-600 h-1.5 rounded-full"
                            style={{ width: `${pct}%` }}
                          />
                        </div>
                        <div className="flex justify-between text-[11px] text-neutral-500">
                          <span>{lb.usedDays} Used</span>
                          <span>{lb.totalDays} Total</span>
                        </div>
                      </div>
                    )
                  })}
                </div>
              </Card>
            )}

            {/* Hardware Assets & Reporting Info */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Assigned Hardware */}
              <Card className="p-6 border border-neutral-200/80 shadow-2xs space-y-4">
                <h3 className="text-sm font-bold text-neutral-900 flex items-center gap-2">
                  <Laptop className="w-4 h-4 text-indigo-600" />
                  Assigned Hardware & Assets
                </h3>
                {!empDashboard?.assignedAssets || empDashboard.assignedAssets.length === 0 ? (
                  <p className="text-xs text-neutral-400">No hardware assets currently assigned.</p>
                ) : (
                  <div className="space-y-3">
                    {empDashboard.assignedAssets.map((asset) => (
                      <div
                        key={asset.id}
                        className="p-3.5 rounded-xl border border-neutral-200 bg-neutral-50/50 flex items-start gap-3"
                      >
                        <div className="w-8 h-8 rounded-lg bg-indigo-100 text-indigo-700 flex items-center justify-center shrink-0">
                          <Laptop className="w-4 h-4" />
                        </div>
                        <div className="min-w-0 flex-1">
                          <p className="text-xs font-bold text-neutral-900 truncate">
                            {asset.assetName}
                          </p>
                          <p className="text-[11px] text-neutral-500">
                            {asset.assetType} • Assigned {formatDateIST(asset.assignedDateUtc)}
                          </p>
                          <Badge variant="success" className="mt-1 text-[10px]">
                            {asset.status}
                          </Badge>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </Card>

              {/* Reporting & Team Info */}
              <Card className="p-6 border border-neutral-200/80 shadow-2xs space-y-3">
                <h3 className="text-sm font-bold text-neutral-900 flex items-center gap-2">
                  <UserCheck className="w-4 h-4 text-emerald-600" />
                  Reporting & Team
                </h3>
                <div className="p-3.5 rounded-xl bg-neutral-50 border border-neutral-200 space-y-2.5 text-xs">
                  <div className="flex justify-between">
                    <span className="text-neutral-500">Reporting Manager:</span>
                    <span className="font-semibold text-neutral-800">
                      {empDashboard?.managerName || 'Vivek Singh (Engineering Lead)'}
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-neutral-500">Department:</span>
                    <span className="font-semibold text-neutral-800">
                      {empDashboard?.departmentName || 'Engineering'}
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-neutral-500">Official Email:</span>
                    <span className="font-semibold text-neutral-800 truncate">
                      {user?.email || 'employee@example.com'}
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-neutral-500">Employee ID:</span>
                    <span className="font-mono font-semibold text-primary-600">
                      {empDashboard?.employeeCode || 'EMP-276595'}
                    </span>
                  </div>
                </div>
              </Card>
            </div>
          </div>
        )}

        {/* 2. ADMIN & SUPERVISOR METRICS ROW */}
        {!isEmployeeOnly && (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <Card
              className="p-5 border border-neutral-200/80 hover:border-primary-300 transition shadow-2xs cursor-pointer bg-gradient-to-br from-white to-blue-50/20"
              onClick={() => navigate('/employees')}
            >
              <div className="flex items-center justify-between">
                <span className="text-xs font-semibold text-neutral-500 uppercase tracking-wider">
                  Total Headcount
                </span>
                <div className="p-2 rounded-xl bg-blue-50 text-blue-600">
                  <Users className="w-5 h-5" />
                </div>
              </div>
              <p className="text-2xl font-bold text-neutral-900 mt-2">
                {summaryLoading ? <Skeleton className="h-8 w-16" /> : (summary?.totalEmployees ?? 0)}
              </p>
              <div className="flex items-center gap-2 mt-2 text-xs text-neutral-500 font-medium">
                <span className="text-emerald-600 font-semibold">{summary?.activeEmployees ?? 0} Active</span>
                <span>•</span>
                <span>{summary?.inactiveEmployees ?? 0} Inactive</span>
              </div>
            </Card>

            <Card
              className="p-5 border border-neutral-200/80 hover:border-emerald-300 transition shadow-2xs cursor-pointer bg-gradient-to-br from-white to-emerald-50/20"
              onClick={() => navigate('/attendance')}
            >
              <div className="flex items-center justify-between">
                <span className="text-xs font-semibold text-neutral-500 uppercase tracking-wider">
                  Present Today
                </span>
                <div className="p-2 rounded-xl bg-emerald-50 text-emerald-600">
                  <UserCheck className="w-5 h-5" />
                </div>
              </div>
              <p className="text-2xl font-bold text-emerald-700 mt-2">
                {summaryLoading ? <Skeleton className="h-8 w-16" /> : (summary?.todayAttendance ?? 0)}
              </p>
              <div className="flex items-center gap-1.5 mt-2 text-xs text-neutral-500 font-medium">
                <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
                <span>{summary?.activeWorkSessions ?? 0} Live Active Sessions</span>
              </div>
            </Card>

            <Card
              className="p-5 border border-neutral-200/80 hover:border-amber-300 transition shadow-2xs cursor-pointer bg-gradient-to-br from-white to-amber-50/20"
              onClick={() => navigate('/leave')}
            >
              <div className="flex items-center justify-between">
                <span className="text-xs font-semibold text-neutral-500 uppercase tracking-wider">
                  Pending Action
                </span>
                <div className="p-2 rounded-xl bg-amber-50 text-amber-600">
                  <FileText className="w-5 h-5" />
                </div>
              </div>
              <p className="text-2xl font-bold text-amber-700 mt-2">
                {summaryLoading ? <Skeleton className="h-8 w-16" /> : ((summary?.pendingLeaveRequests ?? 0) + (summary?.pendingStaffingRequests ?? 0))}
              </p>
              <div className="flex items-center gap-2 mt-2 text-xs text-neutral-500 font-medium">
                <span>{summary?.pendingLeaveRequests ?? 0} Leave</span>
                <span>•</span>
                <span>{summary?.pendingStaffingRequests ?? 0} Staffing</span>
              </div>
            </Card>

            <Card
              className="p-5 border border-neutral-200/80 hover:border-purple-300 transition shadow-2xs cursor-pointer bg-gradient-to-br from-white to-purple-50/20"
              onClick={() => navigate('/payroll')}
            >
              <div className="flex items-center justify-between">
                <span className="text-xs font-semibold text-neutral-500 uppercase tracking-wider">
                  Payroll Cycle (INR)
                </span>
                <div className="p-2 rounded-xl bg-purple-50 text-purple-600">
                  <IndianRupee className="w-5 h-5" />
                </div>
              </div>
              <p className="text-2xl font-bold text-purple-700 mt-2">
                {summaryLoading ? <Skeleton className="h-8 w-24" /> : formatCurrencyINR(summary?.totalPayrollDue ?? 0)}
              </p>
              <div className="flex items-center gap-1.5 mt-2 text-xs text-neutral-500 font-medium">
                <span>{summary?.employeesOnLeave ?? 0} on Leave</span>
              </div>
            </Card>
          </div>
        )}

        {/* 3. TASK AND WORK MANAGEMENT SECTION */}
        <Card className="p-6 border border-neutral-200/80 shadow-2xs space-y-4">
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2">
            <div>
              <h3 className="text-base font-bold text-neutral-900">
                {isAdmin ? 'Organization Assigned Tasks' : isSupervisor ? 'Team Tasks & Deliverables' : 'My Assigned Work'}
              </h3>
              <p className="text-xs text-neutral-500">
                Live task progress, completion percentages, and milestone deadlines
              </p>
            </div>
            <Badge variant="info">{tasks.length} Total Tasks</Badge>
          </div>

          {tasksLoading ? (
            <div className="space-y-3 py-4">
              <Skeleton className="h-16 w-full" />
              <Skeleton className="h-16 w-full" />
            </div>
          ) : tasks.length === 0 ? (
            <EmptyState
              icon={Briefcase}
              title="No Active Tasks"
              description="All caught up! No tasks are currently assigned in this workspace."
            />
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {tasks.slice(0, 6).map((task) => (
                <div
                  key={task.id}
                  className="p-4 rounded-xl border border-neutral-200 bg-white hover:border-primary-300 transition shadow-2xs flex flex-col justify-between"
                >
                  <div>
                    <div className="flex items-start justify-between gap-2 mb-2">
                      <h4 className="text-sm font-bold text-neutral-900 leading-snug">
                        {task.title}
                      </h4>
                      <Badge
                        variant={
                          task.status === 'Completed'
                            ? 'success'
                            : task.status === 'InProgress'
                            ? 'warning'
                            : 'neutral'
                        }
                      >
                        {task.status}
                      </Badge>
                    </div>

                    {task.description && (
                      <p className="text-xs text-neutral-500 line-clamp-2 mb-3">
                        {task.description}
                      </p>
                    )}

                    <div className="flex items-center gap-4 text-xs text-neutral-500 flex-wrap">
                      <span><strong>Assignee:</strong> {task.employeeName}</span>
                      <span><strong>Deadline:</strong> {formatDateIST(task.deadlineUtc)}</span>
                    </div>
                  </div>

                  <div className="mt-4 pt-3 border-t border-neutral-100 flex items-center gap-3">
                    <div className="flex-1">
                      <div className="flex justify-between text-[11px] text-neutral-600 font-semibold mb-1">
                        <span>Progress</span>
                        <span>{task.completionPercentage}%</span>
                      </div>
                      <input
                        type="range"
                        min="0"
                        max="100"
                        value={task.completionPercentage}
                        onChange={(e) => handleUpdateTaskProgress(task.id, parseInt(e.target.value))}
                        className="w-full h-1.5 bg-neutral-200 rounded-lg appearance-none cursor-pointer accent-primary-600"
                      />
                    </div>
                    <button
                      type="button"
                      onClick={() => handleToggleTaskStatus(task)}
                      className={`p-1.5 rounded-lg border transition cursor-pointer ${
                        task.status === 'Completed'
                          ? 'bg-emerald-50 text-emerald-600 border-emerald-200'
                          : 'bg-neutral-50 text-neutral-400 border-neutral-200 hover:text-emerald-600'
                      }`}
                      title={task.status === 'Completed' ? 'Reopen task' : 'Mark completed'}
                    >
                      <CheckCircle className="w-4 h-4" />
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </Card>

        {/* 4. CHARTS SECTION (FOR ADMIN & MANAGERS) */}
        {!isEmployeeOnly && (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Workforce Growth */}
            <Card className="p-6 border border-neutral-200/80 shadow-2xs">
              <h3 className="text-sm font-bold text-neutral-900 mb-4 flex items-center gap-2">
                <TrendingUp className="w-4 h-4 text-primary-600" />
                Workforce Growth & Attendance Trend
              </h3>
              {growthData.length === 0 ? (
                <EmptyState title="No Trend Data" description="Attendance trends will populate automatically." />
              ) : (
                <ResponsiveContainer width="100%" height={240}>
                  <LineChart data={growthData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
                    <XAxis dataKey="month" stroke="#94a3b8" fontSize={11} />
                    <YAxis stroke="#94a3b8" fontSize={11} />
                    <Tooltip />
                    <Line
                      type="monotone"
                      dataKey="totalEmployees"
                      stroke="#4f46e5"
                      strokeWidth={2}
                      name="Headcount"
                    />
                    <Line
                      type="monotone"
                      dataKey="totalAttendance"
                      stroke="#10b981"
                      strokeWidth={2}
                      name="Attendance"
                    />
                  </LineChart>
                </ResponsiveContainer>
              )}
            </Card>

            {/* Department Headcount */}
            <Card className="p-6 border border-neutral-200/80 shadow-2xs">
              <h3 className="text-sm font-bold text-neutral-900 mb-4 flex items-center gap-2">
                <Building2 className="w-4 h-4 text-indigo-600" />
                Department Headcount Distribution
              </h3>
              {departmentData.length === 0 ? (
                <EmptyState title="No Department Data" description="Department breakdown not available." />
              ) : (
                <ResponsiveContainer width="100%" height={240}>
                  <BarChart data={departmentData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
                    <XAxis dataKey="departmentName" stroke="#94a3b8" fontSize={11} />
                    <YAxis stroke="#94a3b8" fontSize={11} />
                    <Tooltip />
                    <Bar dataKey="employeeCount" fill="#6366f1" name="Employees" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              )}
            </Card>
          </div>
        )}

        {/* 5. RECENT ACTIVITY FEED */}
        <Card className="p-6 border border-neutral-200/80 shadow-2xs space-y-4">
          <h3 className="text-sm font-bold text-neutral-900 flex items-center gap-2">
            <Sparkles className="w-4 h-4 text-amber-500" />
            Recent Organization Audit Trail
          </h3>

          {activities.length === 0 ? (
            <p className="text-xs text-neutral-400">No recent activity logged.</p>
          ) : (
            <div className="divide-y divide-neutral-100">
              {activities.slice(0, 6).map((activity) => (
                <div key={activity.id} className="py-3 flex items-start gap-3">
                  <div className="w-7 h-7 rounded-full bg-primary-50 text-primary-600 flex items-center justify-center font-bold text-xs shrink-0 mt-0.5">
                    {activity.category ? activity.category.substring(0, 1).toUpperCase() : 'A'}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-xs font-semibold text-neutral-900">{activity.activityType}</p>
                    <p className="text-xs text-neutral-600">{activity.description}</p>
                    <p className="text-[10px] text-neutral-400 font-mono mt-0.5">
                      {activity.performedBy} • {formatDateTimeIST(activity.occurredAtUtc)}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </Card>

        {/* TASK CREATION MODAL */}
        {isTaskModalOpen && (
          <Modal
            isOpen={isTaskModalOpen}
            onClose={() => setIsTaskModalOpen(false)}
            title="Assign New Work Task"
          >
            <div className="space-y-4 text-xs">
              <div>
                <label className="block font-semibold text-neutral-700 mb-1">Task Title</label>
                <input
                  type="text"
                  placeholder="e.g. Implement OAuth Single Sign-On"
                  value={taskForm.title}
                  onChange={(e) => setTaskForm({ ...taskForm, title: e.target.value })}
                  className="w-full px-3 py-2 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary-500"
                />
              </div>

              <div>
                <label className="block font-semibold text-neutral-700 mb-1">Assign To Employee</label>
                <select
                  value={taskForm.employeeId}
                  onChange={(e) => setTaskForm({ ...taskForm, employeeId: e.target.value })}
                  className="w-full px-3 py-2 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary-500"
                >
                  <option value="">-- Choose Employee --</option>
                  {employees.map((e) => (
                    <option key={e.id} value={e.id}>
                      {e.firstName} {e.lastName} ({e.email})
                    </option>
                  ))}
                </select>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block font-semibold text-neutral-700 mb-1">Priority</label>
                  <select
                    value={taskForm.priority}
                    onChange={(e) => setTaskForm({ ...taskForm, priority: e.target.value })}
                    className="w-full px-3 py-2 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary-500"
                  >
                    <option value="Low">Low</option>
                    <option value="Medium">Medium</option>
                    <option value="High">High</option>
                    <option value="Urgent">Urgent</option>
                  </select>
                </div>

                <div>
                  <label className="block font-semibold text-neutral-700 mb-1">Deadline Date</label>
                  <input
                    type="date"
                    value={taskForm.deadline}
                    onChange={(e) => setTaskForm({ ...taskForm, deadline: e.target.value })}
                    className="w-full px-3 py-2 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary-500"
                  />
                </div>
              </div>

              <div>
                <label className="block font-semibold text-neutral-700 mb-1">Description / Notes</label>
                <textarea
                  rows={3}
                  value={taskForm.description}
                  onChange={(e) => setTaskForm({ ...taskForm, description: e.target.value })}
                  placeholder="Task scope and deliverables..."
                  className="w-full px-3 py-2 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary-500"
                />
              </div>

              <div className="flex justify-end gap-3 pt-3">
                <Button variant="secondary" onClick={() => setIsTaskModalOpen(false)} disabled={isSubmittingTask}>
                  Cancel
                </Button>
                <Button variant="primary" onClick={handleCreateTask} isLoading={isSubmittingTask}>
                  Assign Task
                </Button>
              </div>
            </div>
          </Modal>
        )}
      </div>
    </MainLayout>
  )
}
