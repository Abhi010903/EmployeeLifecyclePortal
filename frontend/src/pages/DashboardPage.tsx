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
  ArrowRight,
  Sparkles,
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
  formatDuration,
} from '@/utils/format'
import type { DashboardSummary, WorkTask, Employee, AttendanceDto } from '@/types'
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
  const userId = user?.id || localStorage.getItem('userId') || ''
  const isAdmin = user?.role === 'Admin'
  const isManager = user?.role === 'Manager' || user?.role === 'Team Lead' || user?.role === 'TeamLead'
  const isSupervisor = isAdmin || isManager || user?.role === 'HR'
  const isEmployeeOnly = !isAdmin && !isManager && user?.role !== 'HR'

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
  const [todayAttendance, setTodayAttendance] = useState<AttendanceDto[]>([])
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
  }, [isSupervisor])

  useEffect(() => {
    fetchDashboardData()
    fetchTasksData()
  }, [fetchDashboardData, fetchTasksData])

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
      fetchTasksData()
      fetchDashboardData()
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
      fetchTasksData()
      fetchDashboardData()
    } catch {
      toast.error('Failed to update status')
    }
  }

  const handleCheckIn = async () => {
    if (!userId) {
      toast.error('Employee account not found')
      return
    }
    try {
      setIsCheckingIn(true)
      await attendanceApi.checkIn({ employeeId: userId })
      toast.success('Checked in successfully!')
      fetchDashboardData()
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Check-in failed')
    } finally {
      setIsCheckingIn(false)
    }
  }

  const handleCheckOut = async () => {
    if (!userId) {
      toast.error('Employee account not found')
      return
    }
    try {
      setIsCheckingOut(true)
      await attendanceApi.checkOut(userId)
      toast.success('Checked out successfully!')
      fetchDashboardData()
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Check-out failed')
    } finally {
      setIsCheckingOut(false)
    }
  }

  const activeSession = todayAttendance.find((a) => !a.checkOutTimeUtc)

  return (
    <MainLayout>
      <div className="space-y-6 max-w-7xl mx-auto pb-12">
        {/* Welcome Header */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-white p-6 rounded-2xl border border-neutral-200/80 shadow-xs">
          <div>
            <div className="flex items-center gap-2">
              <span className="text-xs font-bold uppercase tracking-wider text-primary-600 bg-primary-50 px-2 py-0.5 rounded-md border border-primary-100">
                {isAdmin ? 'System Administrator' : isManager ? 'Team Manager' : 'Employee Self-Service'}
              </span>
            </div>
            <h1 className="text-2xl font-bold text-neutral-900 tracking-tight mt-1">
              Welcome back, {user?.name || user?.email}!
            </h1>
            <p className="text-xs text-neutral-500 mt-0.5">
              India Standard Time (IST) • {new Date().toLocaleDateString('en-IN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric', timeZone: 'Asia/Kolkata' })}
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
              Request Leave
            </Button>
          </div>
        </div>

        {/* 1. EMPLOYEE SELF-SERVICE VIEW (FOR STANDARD EMPLOYEES) */}
        {isEmployeeOnly && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {/* Personal Attendance Session Widget */}
            <Card className="p-6 border border-neutral-200/80 bg-gradient-to-br from-white to-neutral-50/50 shadow-2xs space-y-4">
              <div className="flex items-center justify-between">
                <h3 className="text-sm font-bold text-neutral-900 flex items-center gap-2">
                  <Clock className="w-4 h-4 text-primary-600" />
                  Today's Attendance Session
                </h3>
                <Badge variant={activeSession ? 'success' : 'neutral'}>
                  {activeSession ? 'Clocked In' : 'Not Working'}
                </Badge>
              </div>

              <div className="p-4 rounded-xl bg-neutral-100/70 border border-neutral-200/60 space-y-2 text-xs">
                <div className="flex justify-between">
                  <span className="text-neutral-500">Check-In Time:</span>
                  <span className="font-mono font-bold text-neutral-800">
                    {activeSession ? formatDateTimeIST(activeSession.checkInTimeUtc) : 'Not Checked In'}
                  </span>
                </div>
                {activeSession && (
                  <div className="flex justify-between">
                    <span className="text-neutral-500">Current Session Duration:</span>
                    <span className="font-mono text-emerald-600 font-bold">
                      {formatDuration(activeSession.checkInTimeUtc)}
                    </span>
                  </div>
                )}
              </div>

              <div className="flex gap-2 pt-2">
                {!activeSession ? (
                  <Button
                    variant="primary"
                    size="sm"
                    className="w-full bg-emerald-600 hover:bg-emerald-700 text-white"
                    onClick={handleCheckIn}
                    isLoading={isCheckingIn}
                  >
                    Check In Now
                  </Button>
                ) : (
                  <Button
                    variant="danger"
                    size="sm"
                    className="w-full"
                    onClick={handleCheckOut}
                    isLoading={isCheckingOut}
                  >
                    Check Out Now
                  </Button>
                )}
              </div>
            </Card>

            {/* Quick Shortcuts Widget */}
            <Card className="p-6 border border-neutral-200/80 shadow-2xs space-y-4">
              <h3 className="text-sm font-bold text-neutral-900 flex items-center gap-2">
                <CreditCard className="w-4 h-4 text-emerald-600" />
                My Financial & Leave Hub
              </h3>
              <div className="space-y-2">
                <Button
                  variant="outline"
                  size="sm"
                  className="w-full justify-between text-xs"
                  onClick={() => navigate('/payroll')}
                >
                  <span className="flex items-center gap-2">
                    <IndianRupee className="w-3.5 h-3.5 text-primary-600" />
                    Download Monthly Payslips
                  </span>
                  <ArrowRight className="w-3.5 h-3.5" />
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  className="w-full justify-between text-xs"
                  onClick={() => navigate('/leave')}
                >
                  <span className="flex items-center gap-2">
                    <FileText className="w-3.5 h-3.5 text-indigo-600" />
                    Check Leave Balances
                  </span>
                  <ArrowRight className="w-3.5 h-3.5" />
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  className="w-full justify-between text-xs"
                  onClick={() => navigate('/payroll')}
                >
                  <span className="flex items-center gap-2">
                    <Sparkles className="w-3.5 h-3.5 text-purple-600" />
                    Submit Expense Claim
                  </span>
                  <ArrowRight className="w-3.5 h-3.5" />
                </Button>
              </div>
            </Card>

            {/* Upcoming National Holiday Widget */}
            <Card className="p-6 border border-neutral-200/80 shadow-2xs space-y-3">
              <h3 className="text-sm font-bold text-neutral-900 flex items-center gap-2">
                <Calendar className="w-4 h-4 text-blue-600" />
                Upcoming Holiday
              </h3>
              <div className="p-3.5 rounded-xl bg-blue-50/80 border border-blue-100">
                <p className="text-xs font-bold text-blue-900">Independence Day</p>
                <p className="text-[11px] text-blue-700 mt-0.5">15 August 2026 • National Holiday</p>
                <span className="inline-block mt-2 text-[10px] uppercase font-bold bg-blue-200/80 text-blue-900 px-2 py-0.5 rounded">
                  Paid Holiday
                </span>
              </div>
            </Card>
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
