import { useState, useEffect } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import { Users, Clock, FileText, DollarSign, TrendingUp, AlertCircle } from 'lucide-react'
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

// Types for dashboard data
interface DashboardSummary {
  totalEmployees: number
  activeEmployees: number
  inactiveEmployees: number
  totalDepartments: number
  totalRoles: number
  employeesOnLeave: number
  pendingLeaveRequests: number
  todayAttendance: number
  totalPayrollDue: number
  employeeTrend: number
  attendanceTrend: number
  leaveTrend: number
  payrollTrend: number
}

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

interface StatCard {
  title: string
  value: string
  icon: React.ReactNode
  trend?: string
  trendType?: 'positive' | 'negative' | 'neutral'
  color: string
}

export default function DashboardPage() {
  // State for summary stats
  const [summary, setSummary] = useState<DashboardSummary | null>(null)
  const [summaryLoading, setSummaryLoading] = useState(true)
  const [summaryError, setSummaryError] = useState<string | null>(null)

  // State for growth trend chart
  const [growthData, setGrowthData] = useState<EmployeeGrowthChartData[]>([])
  const [growthLoading, setGrowthLoading] = useState(true)
  const [growthError, setGrowthError] = useState<string | null>(null)

  // State for department headcount chart
  const [departmentData, setDepartmentData] = useState<DepartmentHeadcount[]>([])
  const [departmentLoading, setDepartmentLoading] = useState(true)
  const [departmentError, setDepartmentError] = useState<string | null>(null)

  // State for recent activity
  const [activities, setActivities] = useState<RecentActivity[]>([])
  const [activitiesLoading, setActivitiesLoading] = useState(true)
  const [activitiesError, setActivitiesError] = useState<string | null>(null)

  // Fetch dashboard summary
  useEffect(() => {
    const fetchSummary = async () => {
      try {
        setSummaryLoading(true)
        setSummaryError(null)
        const response = await apiClient.get('/dashboard/summary')
        setSummary(response.data)
      } catch (error) {
        console.error('Failed to fetch dashboard summary:', error)
        setSummaryError('Failed to load dashboard summary')
      } finally {
        setSummaryLoading(false)
      }
    }

    fetchSummary()
  }, [])

  // Fetch growth trend data
  useEffect(() => {
    const fetchGrowthTrend = async () => {
      try {
        setGrowthLoading(true)
        setGrowthError(null)
        const response = await apiClient.get('/dashboard/growth-trend')
        setGrowthData(response.data)
      } catch (error) {
        console.error('Failed to fetch growth trend:', error)
        setGrowthError('Failed to load growth trend')
      } finally {
        setGrowthLoading(false)
      }
    }

    fetchGrowthTrend()
  }, [])

  // Fetch department headcount data
  useEffect(() => {
    const fetchDepartmentHeadcount = async () => {
      try {
        setDepartmentLoading(true)
        setDepartmentError(null)
        const response = await apiClient.get('/dashboard/department-headcount')
        setDepartmentData(response.data)
      } catch (error) {
        console.error('Failed to fetch department headcount:', error)
        setDepartmentError('Failed to load department headcount')
      } finally {
        setDepartmentLoading(false)
      }
    }

    fetchDepartmentHeadcount()
  }, [])

  // Fetch recent activities
  useEffect(() => {
    const fetchRecentActivity = async () => {
      try {
        setActivitiesLoading(true)
        setActivitiesError(null)
        const response = await apiClient.get('/dashboard/recent-activity?limit=10')
        setActivities(response.data)
      } catch (error) {
        console.error('Failed to fetch recent activity:', error)
        setActivitiesError('Failed to load recent activity')
      } finally {
        setActivitiesLoading(false)
      }
    }

    fetchRecentActivity()
  }, [])

  // Format currency with Indian rupee and proper formatting
  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(value)
  }

  // Format date to Indian format
  const formatDate = (dateString: string): string => {
    const date = new Date(dateString)
    return new Intl.DateTimeFormat('en-IN', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }).format(date)
  }

  // Determine trend color
  const getTrendColor = (value: number): 'positive' | 'negative' | 'neutral' => {
    if (value > 0) return 'positive'
    if (value < 0) return 'negative'
    return 'neutral'
  }

  // Format trend percentage
  const formatTrend = (value: number): string => {
    const prefix = value > 0 ? '+' : ''
    return `${prefix}${value.toFixed(1)}%`
  }

  // Build stat cards from summary data
  const getStatCards = (): StatCard[] => {
    if (!summary) return []

    return [
      {
        title: 'Total Employees',
        value: summary.totalEmployees.toString(),
        icon: <Users className="w-8 h-8" />,
        trend: formatTrend(summary.employeeTrend),
        trendType: getTrendColor(summary.employeeTrend),
        color: 'bg-blue-50',
      },
      {
        title: 'Present Today',
        value: summary.todayAttendance.toString(),
        icon: <Clock className="w-8 h-8" />,
        trend: formatTrend(summary.attendanceTrend),
        trendType: getTrendColor(summary.attendanceTrend),
        color: 'bg-green-50',
      },
      {
        title: 'Pending Leave',
        value: summary.pendingLeaveRequests.toString(),
        icon: <FileText className="w-8 h-8" />,
        trend: formatTrend(summary.leaveTrend),
        trendType: getTrendColor(summary.leaveTrend),
        color: 'bg-yellow-50',
      },
      {
        title: 'Payroll Due',
        value: formatCurrency(summary.totalPayrollDue),
        icon: <DollarSign className="w-8 h-8" />,
        trend: formatTrend(summary.payrollTrend),
        trendType: getTrendColor(summary.payrollTrend),
        color: 'bg-purple-50',
      },
    ]
  }

  const statCards = getStatCards()

  // Helper to get trend text color class
  const getTrendColorClass = (trendType?: 'positive' | 'negative' | 'neutral'): string => {
    switch (trendType) {
      case 'positive':
        return 'text-green-600'
      case 'negative':
        return 'text-red-600'
      default:
        return 'text-gray-600'
    }
  }

  return (
    <MainLayout>
      <div className="space-y-8">
        {/* Header */}
        <div>
          <h1 className="text-3xl font-bold text-neutral-900">Dashboard</h1>
          <p className="text-neutral-600 mt-1">Welcome back! Here's your HR summary.</p>
        </div>

        {/* Summary Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {summaryLoading ? (
            <div className="col-span-full flex justify-center py-8">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            </div>
          ) : summaryError ? (
            <div className="col-span-full bg-red-50 p-4 rounded-lg text-red-700">{summaryError}</div>
          ) : !summary ? (
            <div className="col-span-full text-center py-8 text-gray-500">No data available</div>
          ) : (
            statCards.map((stat, index) => (
              <Card key={index} className={stat.color}>
                <div className="flex justify-between items-start">
                  <div>
                    <p className="text-neutral-600 text-sm font-medium">{stat.title}</p>
                    <p className="text-3xl font-bold text-neutral-900 mt-2">{stat.value}</p>
                    {stat.trend && (
                      <p className={`text-xs font-semibold mt-2 ${getTrendColorClass(stat.trendType)}`}>
                        {stat.trend}
                      </p>
                    )}
                  </div>
                  <div className="text-neutral-400">{stat.icon}</div>
                </div>
              </Card>
            ))
          )}
        </div>

        {/* Charts Section */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Employee Growth & Attendance Trend */}
          <Card>
            <h2 className="text-lg font-semibold text-neutral-900 mb-4">Employee & Attendance Trend</h2>
            {growthLoading ? (
              <div className="flex justify-center py-12">
                <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
              </div>
            ) : growthError ? (
              <div className="bg-red-50 p-4 rounded-lg text-red-700 text-sm">{growthError}</div>
            ) : growthData.length === 0 ? (
              <div className="text-center py-12 text-gray-500">No data available</div>
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={growthData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="month" />
                  <YAxis />
                  <Tooltip />
                  <Line
                    type="monotone"
                    dataKey="totalEmployees"
                    stroke="#0284c7"
                    strokeWidth={2}
                    name="Total Employees"
                  />
                  <Line
                    type="monotone"
                    dataKey="totalAttendance"
                    stroke="#f59e0b"
                    strokeWidth={2}
                    name="Attendance"
                  />
                </LineChart>
              </ResponsiveContainer>
            )}
          </Card>

          {/* Department Headcount */}
          <Card>
            <h2 className="text-lg font-semibold text-neutral-900 mb-4">Department Headcount</h2>
            {departmentLoading ? (
              <div className="flex justify-center py-12">
                <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
              </div>
            ) : departmentError ? (
              <div className="bg-red-50 p-4 rounded-lg text-red-700 text-sm">{departmentError}</div>
            ) : departmentData.length === 0 ? (
              <div className="text-center py-12 text-gray-500">No departments available</div>
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={departmentData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="departmentName" />
                  <YAxis />
                  <Tooltip />
                  <Bar dataKey="employeeCount" fill="#0284c7" name="Employee Count" />
                </BarChart>
              </ResponsiveContainer>
            )}
          </Card>
        </div>

        {/* Quick Stats */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <Card>
            <div className="flex items-center gap-4">
              <div className="bg-blue-100 p-3 rounded-lg">
                <Users className="w-6 h-6 text-blue-600" />
              </div>
              <div>
                <p className="text-neutral-600 text-sm">Active Employees</p>
                <p className="text-2xl font-bold text-neutral-900">
                  {summary ? summary.activeEmployees : '-'}
                </p>
              </div>
            </div>
          </Card>

          <Card>
            <div className="flex items-center gap-4">
              <div className="bg-yellow-100 p-3 rounded-lg">
                <AlertCircle className="w-6 h-6 text-yellow-600" />
              </div>
              <div>
                <p className="text-neutral-600 text-sm">On Leave Today</p>
                <p className="text-2xl font-bold text-neutral-900">
                  {summary ? summary.employeesOnLeave : '-'}
                </p>
              </div>
            </div>
          </Card>

          <Card>
            <div className="flex items-center gap-4">
              <div className="bg-green-100 p-3 rounded-lg">
                <TrendingUp className="w-6 h-6 text-green-600" />
              </div>
              <div>
                <p className="text-neutral-600 text-sm">Total Departments</p>
                <p className="text-2xl font-bold text-neutral-900">
                  {summary ? summary.totalDepartments : '-'}
                </p>
              </div>
            </div>
          </Card>
        </div>

        {/* Recent Activity */}
        <Card>
          <h2 className="text-lg font-semibold text-neutral-900 mb-4">Recent Activity</h2>
          {activitiesLoading ? (
            <div className="flex justify-center py-8">
              <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
            </div>
          ) : activitiesError ? (
            <div className="bg-red-50 p-4 rounded-lg text-red-700 text-sm">{activitiesError}</div>
          ) : activities.length === 0 ? (
            <div className="text-center py-8 text-gray-500">No recent activities</div>
          ) : (
            <div className="space-y-4">
              {activities.map((activity) => (
                <div key={activity.id} className="flex items-start gap-4 border-b pb-4 last:border-b-0">
                  <div className="flex-shrink-0">
                    <div className="flex items-center justify-center h-8 w-8 rounded-full bg-blue-100">
                      <span className="text-xs font-bold text-blue-600">
                        {activity.category.substring(0, 1).toUpperCase()}
                      </span>
                    </div>
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-neutral-900">{activity.activityType}</p>
                    <p className="text-sm text-neutral-600">{activity.description}</p>
                    <p className="text-xs text-neutral-500 mt-1">
                      {activity.performedBy} • {formatDate(activity.occurredAtUtc)}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </Card>
      </div>
    </MainLayout>
  )
}
