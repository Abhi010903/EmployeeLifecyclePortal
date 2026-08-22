import { useEffect, useState, useCallback, useMemo } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import { Clock, LogIn, LogOut, AlertCircle, UserCheck } from 'lucide-react'
import { attendanceApi } from '@/api/attendance'
import { employeesApi } from '@/api/employees'
import { AttendanceDto, Employee } from '@/types'
import { useAuthStore } from '@/store/authStore'
import { formatDateIST, formatTimeIST, formatDuration } from '@/utils/format'
import toast from 'react-hot-toast'

export default function AttendancePage() {
  const { user } = useAuthStore()
  const userId = user?.id || localStorage.getItem('userId') || ''
  const isAdmin = user?.role === 'Admin'

  const [employees, setEmployees] = useState<Employee[]>([])
  const [selectedEmployeeId, setSelectedEmployeeId] = useState<string>('')
  const [todayRecords, setTodayRecords] = useState<AttendanceDto[]>([])
  const [allRecords, setAllRecords] = useState<AttendanceDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchEmployees = useCallback(async () => {
    try {
      const emps = await employeesApi.getAllSimple()
      const list = Array.isArray(emps) ? emps : []
      setEmployees(list)
      if (list.length > 0 && !selectedEmployeeId) {
        setSelectedEmployeeId(list[0].id)
      }
    } catch (err) {
      console.error('Failed to load employees for attendance:', err)
    }
  }, [selectedEmployeeId])

  const fetchAttendanceData = useCallback(async () => {
    try {
      setLoading(true)
      const [todayRes, allRes] = await Promise.all([
        attendanceApi.getToday(),
        attendanceApi.getAll(),
      ])

      const todayList = Array.isArray(todayRes.data) ? todayRes.data : []
      const allList = Array.isArray(allRes.data) ? allRes.data : []

      setTodayRecords(todayList)
      setAllRecords(allList)
      setError(null)
    } catch (err) {
      setError('Failed to load attendance records')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchAttendanceData()
    if (isAdmin) {
      fetchEmployees()
    } else if (userId) {
      setSelectedEmployeeId(userId)
    }
  }, [fetchAttendanceData, fetchEmployees, isAdmin, userId])

  // Determine active target employee (Admin can select; Manager/Employee uses self)
  const effectiveEmployeeId = isAdmin ? selectedEmployeeId : (userId || selectedEmployeeId)

  // Check if current target employee has an active unclosed session
  const activeSessionForTarget = useMemo(() => {
    if (!effectiveEmployeeId) return null
    return todayRecords.find(
      (r) => r.employeeId === effectiveEmployeeId && !r.checkOutTimeUtc
    )
  }, [todayRecords, effectiveEmployeeId])

  const hasActiveCheckIn = !!activeSessionForTarget

  const handleCheckIn = async () => {
    if (!effectiveEmployeeId) {
      toast.error('Please select an employee before checking in.')
      return
    }

    try {
      setLoading(true)
      await attendanceApi.checkIn({ employeeId: effectiveEmployeeId })
      toast.success('Checked in successfully!')
      setError(null)
      await fetchAttendanceData()
    } catch (err: any) {
      const msg = err?.response?.data?.message || err?.response?.data || 'Failed to check in'
      toast.error(typeof msg === 'string' ? msg : 'Failed to check in')
      setError(typeof msg === 'string' ? msg : 'Failed to check in')
    } finally {
      setLoading(false)
    }
  }

  const handleCheckOut = async () => {
    if (!effectiveEmployeeId) {
      toast.error('Please select an employee before checking out.')
      return
    }

    try {
      setLoading(true)
      await attendanceApi.checkOut({
        attendanceId: activeSessionForTarget?.id,
        employeeId: effectiveEmployeeId,
      })
      toast.success('Checked out successfully!')
      setError(null)
      await fetchAttendanceData()
    } catch (err: any) {
      const msg = err?.response?.data?.message || err?.response?.data || 'Failed to check out'
      toast.error(typeof msg === 'string' ? msg : 'Failed to check out')
      setError(typeof msg === 'string' ? msg : 'Failed to check out')
    } finally {
      setLoading(false)
    }
  }

  // Calculate unique employees present today (DISTINCT EmployeeId)
  const uniquePresentCount = useMemo(() => {
    const uniqueEmpIds = new Set(todayRecords.map((r) => r.employeeId))
    return uniqueEmpIds.size
  }, [todayRecords])

  const checkedOutCount = useMemo(() => {
    return todayRecords.filter((r) => r.checkOutTimeUtc).length
  }, [todayRecords])

  const stillCheckedInCount = useMemo(() => {
    return todayRecords.filter((r) => !r.checkOutTimeUtc).length
  }, [todayRecords])

  return (
    <MainLayout>
      <div className="space-y-6">
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div>
            <h1 className="text-3xl font-bold text-neutral-900">Attendance</h1>
            <p className="text-neutral-600 mt-1">Track employee check-ins and check-outs (IST / Asia/Kolkata)</p>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            {isAdmin && (
              <div className="flex items-center gap-2">
                <label className="text-sm font-medium text-neutral-700 whitespace-nowrap">
                  Employee:
                </label>
                <select
                  value={selectedEmployeeId}
                  onChange={(e) => setSelectedEmployeeId(e.target.value)}
                  className="px-3 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white"
                >
                  <option value="">Select Employee</option>
                  {employees.map((emp) => (
                    <option key={emp.id} value={emp.id}>
                      {emp.firstName} {emp.lastName} ({emp.employeeCode})
                    </option>
                  ))}
                </select>
              </div>
            )}

            <div className="flex gap-2">
              <Button
                onClick={handleCheckIn}
                disabled={hasActiveCheckIn || loading || !effectiveEmployeeId}
                className="bg-green-600 hover:bg-green-700 cursor-pointer"
              >
                <LogIn className="w-4 h-4 mr-2" />
                Check In
              </Button>
              <Button
                onClick={handleCheckOut}
                disabled={!hasActiveCheckIn || loading || !effectiveEmployeeId}
                className="bg-red-600 hover:bg-red-700 cursor-pointer"
              >
                <LogOut className="w-4 h-4 mr-2" />
                Check Out
              </Button>
            </div>
          </div>
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 flex items-start gap-3">
            <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
            <p className="text-sm text-red-900">{error}</p>
          </div>
        )}

        {/* Quick Stats */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <Card className="bg-blue-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                  Present Today (Unique)
                </p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">{uniquePresentCount}</p>
                <p className="text-xs text-neutral-500 mt-1">Distinct employees logged in</p>
              </div>
              <div className="p-3 bg-blue-100 rounded-lg text-blue-600">
                <UserCheck className="w-6 h-6" />
              </div>
            </div>
          </Card>

          <Card className="bg-green-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                  Currently Working
                </p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">{stillCheckedInCount}</p>
                <p className="text-xs text-neutral-500 mt-1">Active unclosed work sessions</p>
              </div>
              <div className="p-3 bg-green-100 rounded-lg text-green-600">
                <Clock className="w-6 h-6" />
              </div>
            </div>
          </Card>

          <Card className="bg-amber-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                  Completed Sessions
                </p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">{checkedOutCount}</p>
                <p className="text-xs text-neutral-500 mt-1">Checked out today</p>
              </div>
              <div className="p-3 bg-amber-100 rounded-lg text-amber-600">
                <LogOut className="w-6 h-6" />
              </div>
            </div>
          </Card>
        </div>

        {/* Today's Attendance Sessions */}
        <Card>
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-lg font-semibold text-neutral-900">
              Today's Attendance Sessions ({todayRecords.length})
            </h2>
            <Badge label="Live UTC → IST" variant="info" />
          </div>

          {todayRecords.length === 0 ? (
            <div className="text-center py-12">
              <Clock className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
              <p className="text-neutral-500">No attendance sessions recorded today.</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-neutral-200 bg-neutral-50">
                    <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                      Employee
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                      Check-In (IST)
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                      Check-Out (IST)
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                      Duration
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                      Session Status
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-neutral-200">
                  {todayRecords.map((record) => (
                    <tr key={record.id} className="hover:bg-neutral-50 transition-colors">
                      <td className="px-6 py-4 text-sm font-semibold text-neutral-900">
                        {record.employeeName || 'Employee'}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700 font-medium">
                        {formatTimeIST(record.checkInTimeUtc)}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700">
                        {record.checkOutTimeUtc ? (
                          <span className="font-medium text-neutral-700">
                            {formatTimeIST(record.checkOutTimeUtc)}
                          </span>
                        ) : (
                          <span className="text-amber-600 font-medium italic">Active In Progress</span>
                        )}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700 font-medium">
                        {formatDuration(record.checkInTimeUtc, record.checkOutTimeUtc)}
                      </td>
                      <td className="px-6 py-4 text-sm">
                        <Badge
                          label={record.checkOutTimeUtc ? 'Completed' : 'Working Now'}
                          variant={record.checkOutTimeUtc ? 'success' : 'warning'}
                        />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Card>

        {/* All Attendance History */}
        <Card>
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-lg font-semibold text-neutral-900">Attendance History</h2>
          </div>

          {allRecords.length === 0 ? (
            <div className="text-center py-12">
              <Clock className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
              <p className="text-neutral-500">No attendance history records.</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-neutral-200 bg-neutral-50">
                    <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                      Date (IST)
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                      Employee
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                      Check-In (IST)
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                      Check-Out (IST)
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                      Duration
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                      Status
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-neutral-200">
                  {allRecords.slice(0, 50).map((record) => (
                    <tr key={record.id} className="hover:bg-neutral-50 transition-colors">
                      <td className="px-6 py-4 text-sm text-neutral-900 font-medium">
                        {formatDateIST(record.checkInTimeUtc)}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-900 font-semibold">
                        {record.employeeName || 'Employee'}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700">
                        {formatTimeIST(record.checkInTimeUtc)}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700">
                        {record.checkOutTimeUtc ? formatTimeIST(record.checkOutTimeUtc) : '-'}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700 font-medium">
                        {formatDuration(record.checkInTimeUtc, record.checkOutTimeUtc)}
                      </td>
                      <td className="px-6 py-4 text-sm">
                        <Badge
                          label={record.checkOutTimeUtc ? 'Completed' : 'Working'}
                          variant={record.checkOutTimeUtc ? 'success' : 'warning'}
                        />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Card>
      </div>
    </MainLayout>
  )
}
