import { useEffect, useState, useCallback } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import { Clock, LogIn, LogOut, AlertCircle } from 'lucide-react'
import { attendanceApi } from '@/api/attendance'
import { AttendanceDto } from '@/types'

export default function AttendancePage() {
  const [todayRecords, setTodayRecords] = useState<AttendanceDto[]>([])
  const [allRecords, setAllRecords] = useState<AttendanceDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [hasCheckedInToday, setHasCheckedInToday] = useState(false)
  const [currentCheckInId, setCurrentCheckInId] = useState<string | null>(null)

  const fetchTodayRecords = useCallback(async () => {
    try {
      setLoading(true)
      const response = await attendanceApi.getToday()
      setTodayRecords(response.data || [])
      // Check if current user has checked in today
      setHasCheckedInToday((response.data || []).length > 0)
      if ((response.data || []).length > 0 && !currentCheckInId) {
        setCurrentCheckInId(((response.data || [])[0] as AttendanceDto).id)
      }
    } catch (err) {
      setError('Failed to load today\'s attendance')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }, [currentCheckInId])

  const fetchAllRecords = useCallback(async () => {
    try {
      const response = await attendanceApi.getAll()
      setAllRecords(response.data || [])
    } catch (err) {
      console.error('Failed to load all records:', err)
    }
  }, [])

  useEffect(() => {
    fetchTodayRecords()
    fetchAllRecords()
  }, [fetchTodayRecords, fetchAllRecords])

  const handleCheckIn = async () => {
    try {
      setLoading(true)
      const userId = localStorage.getItem('userId') || ''
      await attendanceApi.checkIn({ employeeId: userId })
      setHasCheckedInToday(true)
      await fetchTodayRecords()
      setError(null)
    } catch (err) {
      setError('Failed to check in')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  const handleCheckOut = async () => {
    if (!currentCheckInId) {
      setError('No check-in record found')
      return
    }

    try {
      setLoading(true)
      await attendanceApi.checkOut(currentCheckInId)
      await fetchTodayRecords()
      setCurrentCheckInId(null)
      setError(null)
    } catch (err) {
      setError('Failed to check out')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  const formatTime = (dateString: string) => {
    return new Date(dateString).toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    })
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    })
  }

  const calculateDuration = (checkIn: string, checkOut?: string) => {
    if (!checkOut) return '-'
    const start = new Date(checkIn).getTime()
    const end = new Date(checkOut).getTime()
    const hours = Math.floor((end - start) / 3600000)
    const minutes = Math.floor(((end - start) % 3600000) / 60000)
    return `${hours}h ${minutes}m`
  }

  const presentCount = todayRecords.length
  const checkedOutCount = todayRecords.filter((r) => r.checkOutTimeUtc).length
  const stillCheckedInCount = todayRecords.filter((r) => !r.checkOutTimeUtc).length

  return (
    <MainLayout>
      <div className="space-y-6">
        <div className="flex justify-between items-start">
          <div>
            <h1 className="text-3xl font-bold text-neutral-900">Attendance</h1>
            <p className="text-neutral-600 mt-1">Track employee check-ins and check-outs</p>
          </div>
          <div className="flex gap-2">
            <Button
              onClick={handleCheckIn}
              disabled={hasCheckedInToday || loading}
              className="bg-green-600 hover:bg-green-700"
            >
              <LogIn className="w-4 h-4 mr-2" />
              Check In
            </Button>
            <Button
              onClick={handleCheckOut}
              disabled={!hasCheckedInToday || !currentCheckInId || loading}
              className="bg-red-600 hover:bg-red-700"
            >
              <LogOut className="w-4 h-4 mr-2" />
              Check Out
            </Button>
          </div>
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 flex items-start gap-3">
            <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
            <div>
              <p className="text-sm font-medium text-red-900">{error}</p>
            </div>
          </div>
        )}

        {/* Quick Stats */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
          <Card className="bg-green-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Present Today</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">{presentCount}</p>
              </div>
              <Clock className="w-8 h-8 text-green-600" />
            </div>
          </Card>

          <Card className="bg-blue-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Checked Out</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">{checkedOutCount}</p>
              </div>
              <LogOut className="w-8 h-8 text-blue-600" />
            </div>
          </Card>

          <Card className="bg-yellow-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Still Checked In</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">{stillCheckedInCount}</p>
              </div>
              <Clock className="w-8 h-8 text-yellow-600" />
            </div>
          </Card>

          <Card className="bg-purple-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Total Records</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">{allRecords.length}</p>
              </div>
              <Clock className="w-8 h-8 text-purple-600" />
            </div>
          </Card>
        </div>

        {/* Today's Records */}
        <Card>
          <h2 className="text-lg font-semibold text-neutral-900 mb-4">Today's Attendance</h2>

          {loading && todayRecords.length === 0 ? (
            <div className="text-center py-12">
              <Clock className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
              <p className="text-neutral-500">Loading attendance data...</p>
            </div>
          ) : todayRecords.length === 0 ? (
            <div className="text-center py-12">
              <Clock className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
              <p className="text-neutral-500">No attendance records for today</p>
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
                  </tr>
                </thead>
                <tbody>
                  {todayRecords.map((record) => (
                    <tr key={record.id} className="border-b border-neutral-200 hover:bg-neutral-50">
                      <td className="px-6 py-4 text-sm text-neutral-700">
                        {record.employeeName || 'Unknown'}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700 flex items-center gap-2">
                        <LogIn className="w-4 h-4 text-green-600" />
                        {formatTime(record.checkInTimeUtc)}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700">
                        {record.checkOutTimeUtc ? (
                          <div className="flex items-center gap-2">
                            <LogOut className="w-4 h-4 text-red-600" />
                            {formatTime(record.checkOutTimeUtc)}
                          </div>
                        ) : (
                          <span className="text-neutral-400">-</span>
                        )}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700">
                        {calculateDuration(record.checkInTimeUtc, record.checkOutTimeUtc)}
                      </td>
                      <td className="px-6 py-4 text-sm">
                        <Badge
                          label={record.status}
                          variant={
                            record.status === 'Present'
                              ? 'success'
                              : record.status === 'Absent'
                                ? 'danger'
                                : 'info'
                          }
                        />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Card>

        {/* Recent Records */}
        {allRecords.length > todayRecords.length && (
          <Card>
            <h2 className="text-lg font-semibold text-neutral-900 mb-4">Recent Attendance Records</h2>

            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-neutral-200 bg-neutral-50">
                    <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                      Date
                    </th>
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
                      Hours Worked
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {allRecords.slice(0, 10).map((record) => (
                    <tr key={record.id} className="border-b border-neutral-200 hover:bg-neutral-50">
                      <td className="px-6 py-4 text-sm text-neutral-700">
                        {formatDate(record.checkInTimeUtc)}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700">
                        {record.employeeName || 'Unknown'}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700">
                        {formatTime(record.checkInTimeUtc)}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700">
                        {record.checkOutTimeUtc ? formatTime(record.checkOutTimeUtc) : '-'}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700">
                        {record.hoursWorked.toFixed(2)}h
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Card>
        )}
      </div>
    </MainLayout>
  )
}
