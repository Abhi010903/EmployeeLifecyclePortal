import { useState, useEffect } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import { Clock, LogIn, LogOut } from 'lucide-react'
import { format } from 'date-fns'
import toast from 'react-hot-toast'

interface AttendanceRecord {
  id: string
  employeeName: string
  checkIn: string
  checkOut?: string
  duration?: string
  status: 'Present' | 'Absent' | 'Late' | 'Early Leave'
}

export default function AttendancePage() {
  const [records, setRecords] = useState<AttendanceRecord[]>([
    {
      id: '1',
      employeeName: 'John Doe',
      checkIn: '09:00 AM',
      checkOut: '06:30 PM',
      duration: '9h 30m',
      status: 'Present',
    },
    {
      id: '2',
      employeeName: 'Jane Smith',
      checkIn: '08:45 AM',
      checkOut: '06:00 PM',
      duration: '9h 15m',
      status: 'Present',
    },
    {
      id: '3',
      employeeName: 'Bob Johnson',
      checkIn: '09:30 AM',
      checkOut: undefined,
      status: 'Late',
    },
  ])

  const statusColors: Record<string, 'success' | 'warning' | 'danger' | 'info'> = {
    Present: 'success',
    Absent: 'danger',
    Late: 'warning',
    'Early Leave': 'info',
  }

  return (
    <MainLayout>
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-neutral-900">Attendance</h1>
          <p className="text-neutral-600 mt-1">Track employee check-ins and check-outs</p>
        </div>

        {/* Quick Stats */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
          <Card className="bg-green-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Present Today</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">2,187</p>
              </div>
              <Clock className="w-8 h-8 text-green-600" />
            </div>
          </Card>

          <Card className="bg-red-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Absent Today</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">156</p>
              </div>
              <Clock className="w-8 h-8 text-red-600" />
            </div>
          </Card>

          <Card className="bg-yellow-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Late Arrivals</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">43</p>
              </div>
              <Clock className="w-8 h-8 text-yellow-600" />
            </div>
          </Card>

          <Card className="bg-blue-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">On Leave</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">157</p>
              </div>
              <Clock className="w-8 h-8 text-blue-600" />
            </div>
          </Card>
        </div>

        {/* Today's Records */}
        <Card>
          <h2 className="text-lg font-semibold text-neutral-900 mb-4">Today's Attendance</h2>

          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-neutral-200 bg-neutral-50">
                  <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                    Employee
                  </th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                    Check In
                  </th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                    Check Out
                  </th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                    Duration
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
                {records.map((record) => (
                  <tr key={record.id} className="border-b border-neutral-200 hover:bg-neutral-50">
                    <td className="px-6 py-4 text-sm text-neutral-700">{record.employeeName}</td>
                    <td className="px-6 py-4 text-sm text-neutral-700 flex items-center gap-2">
                      <LogIn className="w-4 h-4 text-green-600" />
                      {record.checkIn}
                    </td>
                    <td className="px-6 py-4 text-sm text-neutral-700">
                      {record.checkOut ? (
                        <div className="flex items-center gap-2">
                          <LogOut className="w-4 h-4 text-red-600" />
                          {record.checkOut}
                        </div>
                      ) : (
                        <span className="text-neutral-400">-</span>
                      )}
                    </td>
                    <td className="px-6 py-4 text-sm text-neutral-700">
                      {record.duration || '-'}
                    </td>
                    <td className="px-6 py-4 text-sm">
                      <Badge label={record.status} variant={statusColors[record.status]} />
                    </td>
                    <td className="px-6 py-4 text-sm">
                      <Button size="sm" variant="outline">
                        Edit
                      </Button>
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
