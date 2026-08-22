import { useState, useEffect, useCallback } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import Modal from '@/components/Common/Modal'
import Input from '@/components/Common/Input'
import {
  Users,
  FileText,
  IndianRupee,
  CheckCircle,
  Plus,
  Briefcase,
  UserCheck,
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
import { useAuthStore } from '@/store/authStore'
import { formatCurrencyINR, formatDateTimeIST, formatDateIST } from '@/utils/format'
import type { DashboardSummary, WorkTask, Employee } from '@/types'
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
  const { user } = useAuthStore()
  const isAdmin = user?.role === 'Admin'
  const isSupervisor = user?.role === 'Admin' || user?.role === 'Manager' || user?.role === 'Team Lead' || user?.role === 'TeamLead' || user?.role === 'HR'

  // State for summary stats
  const [summary, setSummary] = useState<DashboardSummary | null>(null)
  const [summaryLoading, setSummaryLoading] = useState(true)

  // State for charts
  const [growthData, setGrowthData] = useState<EmployeeGrowthChartData[]>([])
  const [departmentData, setDepartmentData] = useState<DepartmentHeadcount[]>([])

  // State for recent activity
  const [activities, setActivities] = useState<RecentActivity[]>([])

  // State for tasks
  const [tasks, setTasks] = useState<WorkTask[]>([])
  const [tasksLoading, setTasksLoading] = useState(true)
  const [employees, setEmployees] = useState<Employee[]>([])

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
      const [sumRes, growthRes, deptRes, actRes] = await Promise.allSettled([
        apiClient.get<DashboardSummary>('/dashboard/summary'),
        apiClient.get<EmployeeGrowthChartData[]>('/dashboard/growth-trend'),
        apiClient.get<DepartmentHeadcount[]>('/dashboard/department-headcount'),
        apiClient.get<RecentActivity[]>('/dashboard/recent-activity?limit=10'),
      ])

      if (sumRes.status === 'fulfilled') setSummary(sumRes.value.data)
      if (growthRes.status === 'fulfilled') setGrowthData(Array.isArray(growthRes.value.data) ? growthRes.value.data : [])
      if (deptRes.status === 'fulfilled') setDepartmentData(Array.isArray(deptRes.value.data) ? deptRes.value.data : [])
      if (actRes.status === 'fulfilled') setActivities(Array.isArray(actRes.value.data) ? actRes.value.data : [])
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
    } catch (err: any) {
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

  return (
    <MainLayout>
      <div className="space-y-8">
        {/* Header with Welcome and Quick Action */}
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div>
            <h1 className="text-3xl font-bold text-neutral-900">
              {isAdmin ? 'Organization Dashboard' : isSupervisor ? 'Team Dashboard' : 'My Workspace Dashboard'}
            </h1>
            <p className="text-neutral-600 mt-1">
              Welcome, <span className="font-semibold text-neutral-800">{user?.name || user?.email}</span> ({user?.role})
            </p>
          </div>
          {isSupervisor && (
            <Button onClick={() => setIsTaskModalOpen(true)} className="bg-primary-600 hover:bg-primary-700">
              <Plus className="w-4 h-4 mr-2" />
              Assign Work / Task
            </Button>
          )}
        </div>

        {/* Primary Metrics Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {summaryLoading ? (
            <div className="col-span-full flex justify-center py-8">
              <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-primary-600"></div>
            </div>
          ) : (
            <>
              <Card className="bg-blue-50">
                <div className="flex justify-between items-start">
                  <div>
                    <p className="text-neutral-600 text-xs font-semibold uppercase tracking-wider">
                      {isAdmin ? 'Total Employees' : 'Team / Organization'}
                    </p>
                    <p className="text-3xl font-bold text-neutral-900 mt-2">
                      {summary?.totalEmployees ?? 0}
                    </p>
                    <p className="text-xs text-neutral-500 mt-1">
                      {summary?.activeEmployees ?? 0} Active • {summary?.inactiveEmployees ?? 0} Inactive
                    </p>
                  </div>
                  <div className="p-3 bg-blue-100 rounded-lg text-blue-600">
                    <Users className="w-6 h-6" />
                  </div>
                </div>
              </Card>

              <Card className="bg-green-50">
                <div className="flex justify-between items-start">
                  <div>
                    <p className="text-neutral-600 text-xs font-semibold uppercase tracking-wider">
                      Present Today
                    </p>
                    <p className="text-3xl font-bold text-neutral-900 mt-2">
                      {summary?.todayAttendance ?? 0}
                    </p>
                    <p className="text-xs text-green-700 mt-1 font-medium">
                      {summary?.activeWorkSessions ?? 0} Working Now (Active Sessions)
                    </p>
                  </div>
                  <div className="p-3 bg-green-100 rounded-lg text-green-600">
                    <UserCheck className="w-6 h-6" />
                  </div>
                </div>
              </Card>

              <Card className="bg-yellow-50">
                <div className="flex justify-between items-start">
                  <div>
                    <p className="text-neutral-600 text-xs font-semibold uppercase tracking-wider">
                      Pending Approvals
                    </p>
                    <p className="text-3xl font-bold text-neutral-900 mt-2">
                      {(summary?.pendingLeaveRequests ?? 0) + (summary?.pendingStaffingRequests ?? 0)}
                    </p>
                    <p className="text-xs text-amber-700 mt-1 font-medium">
                      {summary?.pendingLeaveRequests ?? 0} Leave • {summary?.pendingStaffingRequests ?? 0} Staffing
                    </p>
                  </div>
                  <div className="p-3 bg-yellow-100 rounded-lg text-yellow-600">
                    <FileText className="w-6 h-6" />
                  </div>
                </div>
              </Card>

              <Card className="bg-purple-50">
                <div className="flex justify-between items-start">
                  <div>
                    <p className="text-neutral-600 text-xs font-semibold uppercase tracking-wider">
                      Payroll Due (INR)
                    </p>
                    <p className="text-2xl font-bold text-neutral-900 mt-2">
                      {formatCurrencyINR(summary?.totalPayrollDue ?? 0)}
                    </p>
                    <p className="text-xs text-neutral-500 mt-1">
                      {summary?.employeesOnLeave ?? 0} Employees On Leave
                    </p>
                  </div>
                  <div className="p-3 bg-purple-100 rounded-lg text-purple-600">
                    <IndianRupee className="w-6 h-6" />
                  </div>
                </div>
              </Card>
            </>
          )}
        </div>

        {/* Task and Work Management Section */}
        <Card>
          <div className="flex justify-between items-center mb-4">
            <div>
              <h2 className="text-lg font-semibold text-neutral-900">
                {isAdmin ? 'Organization Assigned Work & Tasks' : isSupervisor ? 'Team Tasks & Deliverables' : 'My Assigned Tasks & Deliverables'}
              </h2>
              <p className="text-xs text-neutral-500">Live task tracking and progress reporting</p>
            </div>
            <Badge label={`${tasks.length} Total Tasks`} variant="info" />
          </div>

          {tasksLoading ? (
            <div className="text-center py-8">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
              <p className="text-neutral-500 text-sm mt-2">Loading tasks...</p>
            </div>
          ) : tasks.length === 0 ? (
            <div className="text-center py-8">
              <Briefcase className="w-10 h-10 text-neutral-300 mx-auto mb-2" />
              <p className="text-neutral-500 text-sm">No work tasks assigned yet.</p>
            </div>
          ) : (
            <div className="space-y-3">
              {tasks.slice(0, 8).map((task) => (
                <div key={task.id} className="p-4 border border-neutral-200 rounded-lg hover:bg-neutral-50 transition">
                  <div className="flex flex-col md:flex-row md:items-center justify-between gap-2">
                    <div className="flex-1">
                      <div className="flex items-center gap-2">
                        <h3 className="font-semibold text-neutral-900 text-sm">{task.title}</h3>
                        <Badge
                          label={task.priority}
                          variant={task.priority === 'Urgent' || task.priority === 'High' ? 'danger' : 'info'}
                        />
                        <Badge
                          label={task.status}
                          variant={task.status === 'Completed' ? 'success' : task.status === 'InProgress' ? 'warning' : 'info'}
                        />
                      </div>
                      {task.description && <p className="text-xs text-neutral-600 mt-1">{task.description}</p>}
                      <div className="flex gap-4 mt-2 text-xs text-neutral-500 flex-wrap">
                        <span><strong>Assignee:</strong> {task.employeeName}</span>
                        {task.departmentName && <span><strong>Dept:</strong> {task.departmentName}</span>}
                        <span><strong>Deadline:</strong> {formatDateIST(task.deadlineUtc)}</span>
                      </div>
                    </div>

                    <div className="flex items-center gap-4 min-w-[200px]">
                      <div className="flex-1">
                        <div className="flex justify-between text-xs text-neutral-600 mb-1">
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
                        onClick={() => handleToggleTaskStatus(task)}
                        className={`p-1.5 rounded-lg border transition ${
                          task.status === 'Completed'
                            ? 'bg-green-100 text-green-700 border-green-300'
                            : 'bg-neutral-100 text-neutral-600 border-neutral-300 hover:bg-green-50 hover:text-green-600'
                        }`}
                        title={task.status === 'Completed' ? 'Mark In Progress' : 'Mark Completed'}
                      >
                        <CheckCircle className="w-4 h-4" />
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </Card>

        {/* Charts Section */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Growth & Attendance Trend */}
          <Card>
            <h2 className="text-lg font-semibold text-neutral-900 mb-4">Workforce Growth & Attendance</h2>
            {growthData.length === 0 ? (
              <div className="text-center py-12 text-gray-500 text-sm">No growth trend data available</div>
            ) : (
              <ResponsiveContainer width="100%" height={260}>
                <LineChart data={growthData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="month" />
                  <YAxis />
                  <Tooltip />
                  <Line type="monotone" dataKey="totalEmployees" stroke="#0284c7" strokeWidth={2} name="Total Employees" />
                  <Line type="monotone" dataKey="totalAttendance" stroke="#10b981" strokeWidth={2} name="Attendance" />
                </LineChart>
              </ResponsiveContainer>
            )}
          </Card>

          {/* Department Headcount */}
          <Card>
            <h2 className="text-lg font-semibold text-neutral-900 mb-4">Department Headcount</h2>
            {departmentData.length === 0 ? (
              <div className="text-center py-12 text-gray-500 text-sm">No department data available</div>
            ) : (
              <ResponsiveContainer width="100%" height={260}>
                <BarChart data={departmentData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="departmentName" />
                  <YAxis />
                  <Tooltip />
                  <Bar dataKey="employeeCount" fill="#0284c7" name="Headcount" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            )}
          </Card>
        </div>

        {/* Recent Real Activity */}
        <Card>
          <h2 className="text-lg font-semibold text-neutral-900 mb-4">Recent Organization Activity</h2>
          {activities.length === 0 ? (
            <div className="text-center py-8 text-gray-500 text-sm">No recent activity logged.</div>
          ) : (
            <div className="space-y-3">
              {activities.map((activity) => (
                <div key={activity.id} className="flex items-start gap-4 border-b pb-3 last:border-b-0">
                  <div className="flex items-center justify-center h-8 w-8 rounded-full bg-blue-100 text-blue-600 font-bold text-xs flex-shrink-0">
                    {activity.category ? activity.category.substring(0, 1).toUpperCase() : 'A'}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-neutral-900">{activity.activityType}</p>
                    <p className="text-xs text-neutral-600">{activity.description}</p>
                    <p className="text-xs text-neutral-400 mt-0.5">
                      {activity.performedBy} • {formatDateTimeIST(activity.occurredAtUtc)}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </Card>
      </div>

      {/* Task Assignment Modal */}
      <Modal
        isOpen={isTaskModalOpen}
        onClose={() => setIsTaskModalOpen(false)}
        title="Assign Work / Task"
        footer={
          <>
            <Button variant="outline" onClick={() => setIsTaskModalOpen(false)} disabled={isSubmittingTask}>
              Cancel
            </Button>
            <Button onClick={handleCreateTask} disabled={isSubmittingTask}>
              {isSubmittingTask ? 'Assigning...' : 'Assign Task'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <Input
            label="Task Title *"
            placeholder="e.g., Complete Sprint 22 Database Review"
            value={taskForm.title}
            onChange={(e) => setTaskForm({ ...taskForm, title: e.target.value })}
          />

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Assign to Employee *</label>
            <select
              value={taskForm.employeeId}
              onChange={(e) => setTaskForm({ ...taskForm, employeeId: e.target.value })}
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white"
            >
              <option value="">Select Employee</option>
              {employees.map((emp) => (
                <option key={emp.id} value={emp.id}>
                  {emp.firstName} {emp.lastName} ({emp.employeeCode})
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Priority</label>
            <select
              value={taskForm.priority}
              onChange={(e) => setTaskForm({ ...taskForm, priority: e.target.value })}
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white"
            >
              <option value="Low">Low</option>
              <option value="Medium">Medium</option>
              <option value="High">High</option>
              <option value="Urgent">Urgent</option>
            </select>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">Start Date</label>
              <input
                type="date"
                value={taskForm.startDate}
                onChange={(e) => setTaskForm({ ...taskForm, startDate: e.target.value })}
                className="w-full px-4 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">Deadline Date</label>
              <input
                type="date"
                value={taskForm.deadline}
                onChange={(e) => setTaskForm({ ...taskForm, deadline: e.target.value })}
                className="w-full px-4 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white"
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Description & Requirements</label>
            <textarea
              rows={3}
              placeholder="Detail the work to be accomplished..."
              value={taskForm.description}
              onChange={(e) => setTaskForm({ ...taskForm, description: e.target.value })}
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white"
            />
          </div>
        </div>
      </Modal>
    </MainLayout>
  )
}
