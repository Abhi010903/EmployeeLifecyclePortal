import { useState, useEffect } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import { Users, Clock, FileText, DollarSign, TrendingUp, AlertCircle } from 'lucide-react'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, BarChart, Bar } from 'recharts'

interface StatCard {
  title: string
  value: string
  icon: React.ReactNode
  trend?: string
  color: string
}

export default function DashboardPage() {
  const [stats, setStats] = useState<StatCard[]>([])

  useEffect(() => {
    setStats([
      {
        title: 'Total Employees',
        value: '2,543',
        icon: <Users className="w-8 h-8" />,
        trend: '+5.2%',
        color: 'bg-blue-50',
      },
      {
        title: 'Present Today',
        value: '2,187',
        icon: <Clock className="w-8 h-8" />,
        trend: '+2.1%',
        color: 'bg-green-50',
      },
      {
        title: 'Pending Leave',
        value: '124',
        icon: <FileText className="w-8 h-8" />,
        trend: '-1.2%',
        color: 'bg-yellow-50',
      },
      {
        title: 'Payroll Due',
        value: '$2.4M',
        icon: <DollarSign className="w-8 h-8" />,
        trend: '+12.5%',
        color: 'bg-purple-50',
      },
    ])
  }, [])

  const chartData = [
    { name: 'Jan', employees: 2100, attendance: 1950 },
    { name: 'Feb', employees: 2200, attendance: 2050 },
    { name: 'Mar', employees: 2300, attendance: 2150 },
    { name: 'Apr', employees: 2400, attendance: 2200 },
    { name: 'May', employees: 2500, attendance: 2350 },
    { name: 'Jun', employees: 2543, attendance: 2187 },
  ]

  return (
    <MainLayout>
      <div className="space-y-8">
        {/* Header */}
        <div>
          <h1 className="text-3xl font-bold text-neutral-900">Dashboard</h1>
          <p className="text-neutral-600 mt-1">Welcome back! Here's your HR summary.</p>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {stats.map((stat, index) => (
            <Card key={index} className={stat.color}>
              <div className="flex justify-between items-start">
                <div>
                  <p className="text-neutral-600 text-sm font-medium">{stat.title}</p>
                  <p className="text-3xl font-bold text-neutral-900 mt-2">{stat.value}</p>
                  {stat.trend && (
                    <p className="text-green-600 text-xs font-semibold mt-2">{stat.trend}</p>
                  )}
                </div>
                <div className="text-neutral-400">{stat.icon}</div>
              </div>
            </Card>
          ))}
        </div>

        {/* Charts */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Line Chart */}
          <Card>
            <h2 className="text-lg font-semibold text-neutral-900 mb-4">Employee & Attendance Trend</h2>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={chartData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" />
                <YAxis />
                <Tooltip />
                <Line type="monotone" dataKey="employees" stroke="#0284c7" strokeWidth={2} />
                <Line type="monotone" dataKey="attendance" stroke="#f59e0b" strokeWidth={2} />
              </LineChart>
            </ResponsiveContainer>
          </Card>

          {/* Bar Chart */}
          <Card>
            <h2 className="text-lg font-semibold text-neutral-900 mb-4">Department Headcount</h2>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={[
                { department: 'Sales', count: 450 },
                { department: 'Engineering', count: 650 },
                { department: 'HR', count: 120 },
                { department: 'Finance', count: 200 },
              ]}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="department" />
                <YAxis />
                <Tooltip />
                <Bar dataKey="count" fill="#0284c7" />
              </BarChart>
            </ResponsiveContainer>
          </Card>
        </div>

        {/* Quick Stats */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <Card>
            <div className="flex items-center gap-4">
              <div className="bg-green-100 p-3 rounded-lg">
                <TrendingUp className="w-6 h-6 text-green-600" />
              </div>
              <div>
                <p className="text-neutral-600 text-sm">Retention Rate</p>
                <p className="text-2xl font-bold text-neutral-900">94.2%</p>
              </div>
            </div>
          </Card>

          <Card>
            <div className="flex items-center gap-4">
              <div className="bg-yellow-100 p-3 rounded-lg">
                <AlertCircle className="w-6 h-6 text-yellow-600" />
              </div>
              <div>
                <p className="text-neutral-600 text-sm">Attrition This Month</p>
                <p className="text-2xl font-bold text-neutral-900">2 employees</p>
              </div>
            </div>
          </Card>

          <Card>
            <div className="flex items-center gap-4">
              <div className="bg-blue-100 p-3 rounded-lg">
                <Users className="w-6 h-6 text-blue-600" />
              </div>
              <div>
                <p className="text-neutral-600 text-sm">New Hires This Year</p>
                <p className="text-2xl font-bold text-neutral-900">187 employees</p>
              </div>
            </div>
          </Card>
        </div>
      </div>
    </MainLayout>
  )
}
