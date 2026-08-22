import { useEffect, useState, useCallback } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import Modal from '@/components/Common/Modal'
import { Plus, Calendar, AlertCircle, CheckCircle, XCircle } from 'lucide-react'
import { leaveApi } from '@/api/attendance'
import { employeesApi } from '@/api/employees'
import { LeaveBalanceDto, LeaveRequestDto, LeaveTypeDto, Employee } from '@/types'
import { useAuthStore } from '@/store/authStore'
import { formatDateIST } from '@/utils/format'
import toast from 'react-hot-toast'

export default function LeavePage() {
  const [balances, setBalances] = useState<LeaveBalanceDto[]>([])
  const [requests, setRequests] = useState<LeaveRequestDto[]>([])
  const [leaveTypes, setLeaveTypes] = useState<LeaveTypeDto[]>([])
  const [employees, setEmployees] = useState<Employee[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [formData, setFormData] = useState({
    employeeId: '',
    leaveTypeId: '',
    startDate: '',
    endDate: '',
    reason: '',
  })
  const [formErrors, setFormErrors] = useState<Record<string, string>>({})
  const { user } = useAuthStore()
  const userId = user?.id || localStorage.getItem('userId') || ''
  const isAdmin = user?.role === 'Admin'
  const isSupervisor = user?.role === 'Admin' || user?.role === 'Manager' || user?.role === 'Team Lead' || user?.role === 'TeamLead' || user?.role === 'HR'

  const fetchData = useCallback(async () => {
    try {
      setLoading(true)
      const [typesRes, balancesRes, requestsRes, empsRes] = await Promise.all([
        leaveApi.getTypes(),
        userId ? leaveApi.getBalance(userId) : Promise.resolve({ data: [] }),
        isSupervisor
          ? leaveApi.getRequests()
          : (userId ? leaveApi.getRequests(userId) : Promise.resolve({ data: [] })),
        isSupervisor ? employeesApi.getAllSimple() : Promise.resolve([]),
      ])

      setLeaveTypes(Array.isArray(typesRes.data) ? typesRes.data : [])
      setBalances(Array.isArray(balancesRes.data) ? balancesRes.data : [])
      setRequests(Array.isArray(requestsRes.data) ? requestsRes.data : [])
      if (Array.isArray(empsRes)) {
        setEmployees(empsRes)
      }
      setError(null)
    } catch (err) {
      setError('Failed to load leave data')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }, [userId, isSupervisor])

  useEffect(() => {
    fetchData()
  }, [fetchData])

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {}

    if (isSupervisor && !formData.employeeId) {
      newErrors.employeeId = 'Employee is required'
    }
    if (!formData.leaveTypeId) newErrors.leaveTypeId = 'Leave type is required'
    if (!formData.startDate) newErrors.startDate = 'Start date is required'
    if (!formData.endDate) newErrors.endDate = 'End date is required'
    if (!formData.reason.trim()) newErrors.reason = 'Reason is required'

    if (formData.startDate && formData.endDate) {
      const start = new Date(formData.startDate)
      const end = new Date(formData.endDate)
      if (start > end) {
        newErrors.endDate = 'End date must be after start date'
      }
    }

    setFormErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleOpenModal = () => {
    setFormErrors({})
    setFormData({
      employeeId: isSupervisor ? (employees.length > 0 ? employees[0].id : '') : userId,
      leaveTypeId: leaveTypes.length > 0 ? leaveTypes[0].id : '',
      startDate: new Date().toISOString().split('T')[0],
      endDate: new Date().toISOString().split('T')[0],
      reason: '',
    })
    setIsModalOpen(true)
  }

  const handleSubmit = async () => {
    if (!validateForm()) return

    try {
      setIsSubmitting(true)
      const targetEmpId = isSupervisor ? formData.employeeId : userId

      await leaveApi.apply({
        employeeId: targetEmpId,
        leaveTypeId: formData.leaveTypeId,
        startDate: formData.startDate,
        endDate: formData.endDate,
        reason: formData.reason,
      })

      toast.success('Leave request submitted successfully')
      setIsModalOpen(false)
      setFormData({
        employeeId: '',
        leaveTypeId: '',
        startDate: '',
        endDate: '',
        reason: '',
      })
      await fetchData()
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Failed to submit leave request')
      console.error(err)
    } finally {
      setIsSubmitting(false)
    }
  }

  const getLeaveTypeColor = (index: number): string => {
    const colors = ['bg-primary-50', 'bg-green-50', 'bg-yellow-50', 'bg-purple-50', 'bg-pink-50']
    return colors[index % colors.length]
  }

  const getLeaveTypeIconColor = (index: number): string => {
    const colors = ['text-primary-600', 'text-green-600', 'text-yellow-600', 'text-purple-600', 'text-pink-600']
    return colors[index % colors.length]
  }

  const handleApprove = async (requestId: string) => {
    try {
      await leaveApi.approve({ leaveRequestId: requestId, approvedByUserId: userId })
      toast.success('Leave request approved successfully')
      fetchData()
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Failed to approve leave request')
    }
  }

  const handleReject = async (requestId: string) => {
    try {
      await leaveApi.reject(requestId)
      toast.success('Leave request rejected')
      fetchData()
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Failed to reject leave request')
    }
  }

  const getStatusBadge = (status: string) => {
    if (status === 'Approved') return <Badge label="Approved" variant="success" />
    if (status === 'ManagerApproved') return <Badge label="Manager Approved — Pending Admin" variant="info" />
    if (status === 'Rejected') return <Badge label="Rejected" variant="danger" />
    return <Badge label="Pending Manager Review" variant="warning" />
  }

  return (
    <MainLayout>
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold text-neutral-900">Leave Management</h1>
            <p className="text-neutral-600 mt-1">Track and manage leave requests with multi-stage approval workflow</p>
          </div>
          <Button onClick={handleOpenModal}>
            <Plus className="w-4 h-4 mr-2" />
            Request Leave
          </Button>
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 flex items-start gap-3">
            <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
            <p className="text-sm text-red-900">{error}</p>
          </div>
        )}

        {/* Leave Balances */}
        {balances.length > 0 && (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {balances.map((balance, index) => {
              const usagePercent =
                balance.totalDays > 0 ? (balance.usedDays / balance.totalDays) * 100 : 0

              return (
                <Card key={balance.id || index} className={getLeaveTypeColor(index)}>
                  <div className="flex items-start justify-between">
                    <div>
                      <p className="text-xs font-semibold text-neutral-600 uppercase tracking-wider">
                        {balance.leaveTypeName}
                      </p>
                      <p className="text-2xl font-bold text-neutral-900 mt-1">
                        {balance.remainingDays}{' '}
                        <span className="text-sm font-normal text-neutral-500">
                          / {balance.totalDays} days
                        </span>
                      </p>
                    </div>
                    <div className={`p-2 rounded-lg bg-white ${getLeaveTypeIconColor(index)}`}>
                      <Calendar className="w-5 h-5" />
                    </div>
                  </div>

                  <div className="mt-4 bg-neutral-200 rounded-full h-2">
                    <div
                      className="bg-gradient-to-r from-primary-600 to-primary-500 h-2 rounded-full transition-all"
                      style={{ width: `${Math.min(usagePercent, 100)}%` }}
                    />
                  </div>

                  <div className="mt-3 flex justify-between text-xs text-neutral-600">
                    <span>Used: {balance.usedDays} days</span>
                    <span>Remaining: {balance.remainingDays} days</span>
                  </div>
                </Card>
              )
            })}
          </div>
        )}

        {/* Leave Requests */}
        <Card>
          <h2 className="text-lg font-semibold text-neutral-900 mb-4">Leave Requests</h2>

          {loading && requests.length === 0 ? (
            <div className="text-center py-12">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
              <p className="text-neutral-500 mt-3">Loading leave data...</p>
            </div>
          ) : requests.length === 0 ? (
            <div className="text-center py-12">
              <Calendar className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
              <p className="text-neutral-500">No leave requests yet</p>
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
                      Leave Type
                    </th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                      Period (IST)
                    </th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                      Days
                    </th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                      Reason
                    </th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                      Workflow Stage
                    </th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {requests.map((request) => (
                    <tr key={request.id} className="border-b border-neutral-200 hover:bg-neutral-50">
                      <td className="px-6 py-4 text-sm text-neutral-900 font-medium">
                        {request.employeeName || 'Employee'}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700 font-medium">
                        {request.leaveTypeName}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700">
                        {formatDateIST(request.startDateUtc)} to {formatDateIST(request.endDateUtc)}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700 font-medium">{request.daysRequested}</td>
                      <td className="px-6 py-4 text-sm text-neutral-700 max-w-xs truncate">{request.reason || '-'}</td>
                      <td className="px-6 py-4 text-sm">
                        {getStatusBadge(request.status)}
                      </td>
                      <td className="px-6 py-4 text-sm">
                        {isSupervisor && (request.status === 'Pending' || (isAdmin && request.status === 'ManagerApproved')) ? (
                          <div className="flex gap-2">
                            <button
                              onClick={() => handleApprove(request.id)}
                              className="px-2.5 py-1 text-xs bg-green-100 text-green-700 font-medium rounded hover:bg-green-200 transition flex items-center gap-1 cursor-pointer"
                              title={request.status === 'ManagerApproved' ? 'Final Admin Approval' : 'Approve Leave'}
                            >
                              <CheckCircle className="w-3.5 h-3.5" />
                              {isAdmin && request.status === 'ManagerApproved' ? 'Final Approve' : 'Approve'}
                            </button>
                            <button
                              onClick={() => handleReject(request.id)}
                              className="px-2.5 py-1 text-xs bg-red-100 text-red-700 font-medium rounded hover:bg-red-200 transition flex items-center gap-1 cursor-pointer"
                              title="Reject Leave"
                            >
                              <XCircle className="w-3.5 h-3.5" />
                              Reject
                            </button>
                          </div>
                        ) : (
                          <span className="text-neutral-400 text-xs">
                            {request.status === 'Approved' ? 'Completed' : request.status === 'Rejected' ? 'Rejected' : 'Awaiting Review'}
                          </span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Card>
      </div>

      {/* Request Leave Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => {
          setIsModalOpen(false)
          setFormErrors({})
        }}
        title="Request Leave"
        footer={
          <>
            <Button
              variant="outline"
              onClick={() => setIsModalOpen(false)}
              disabled={isSubmitting}
            >
              Cancel
            </Button>
            <Button onClick={handleSubmit} disabled={isSubmitting}>
              {isSubmitting ? 'Submitting...' : 'Submit Request'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          {isSupervisor && (
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">
                Employee *
              </label>
              <select
                value={formData.employeeId}
                onChange={(e) => {
                  setFormData({ ...formData, employeeId: e.target.value })
                  if (formErrors.employeeId) {
                    setFormErrors({ ...formErrors, employeeId: '' })
                  }
                }}
                className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white ${
                  formErrors.employeeId ? 'border-red-500' : 'border-neutral-300'
                }`}
              >
                <option value="">Select Employee</option>
                {employees.map((emp) => (
                  <option key={emp.id} value={emp.id}>
                    {emp.firstName} {emp.lastName} ({emp.employeeCode})
                  </option>
                ))}
              </select>
              {formErrors.employeeId && (
                <p className="text-red-500 text-xs mt-1">{formErrors.employeeId}</p>
              )}
            </div>
          )}

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">
              Leave Type *
            </label>
            <select
              value={formData.leaveTypeId}
              onChange={(e) => {
                setFormData({ ...formData, leaveTypeId: e.target.value })
                if (formErrors.leaveTypeId) {
                  setFormErrors({ ...formErrors, leaveTypeId: '' })
                }
              }}
              className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white ${
                formErrors.leaveTypeId ? 'border-red-500' : 'border-neutral-300'
              }`}
            >
              <option value="">Select Leave Type</option>
              {leaveTypes.map((type) => (
                <option key={type.id} value={type.id}>
                  {type.name} ({type.daysPerYear} days/year)
                </option>
              ))}
            </select>
            {formErrors.leaveTypeId && (
              <p className="text-red-500 text-xs mt-1">{formErrors.leaveTypeId}</p>
            )}
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">
                Start Date *
              </label>
              <input
                type="date"
                value={formData.startDate}
                onChange={(e) => {
                  setFormData({ ...formData, startDate: e.target.value })
                  if (formErrors.startDate) {
                    setFormErrors({ ...formErrors, startDate: '' })
                  }
                }}
                className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white ${
                  formErrors.startDate ? 'border-red-500' : 'border-neutral-300'
                }`}
              />
              {formErrors.startDate && (
                <p className="text-red-500 text-xs mt-1">{formErrors.startDate}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">
                End Date *
              </label>
              <input
                type="date"
                value={formData.endDate}
                onChange={(e) => {
                  setFormData({ ...formData, endDate: e.target.value })
                  if (formErrors.endDate) {
                    setFormErrors({ ...formErrors, endDate: '' })
                  }
                }}
                className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white ${
                  formErrors.endDate ? 'border-red-500' : 'border-neutral-300'
                }`}
              />
              {formErrors.endDate && (
                <p className="text-red-500 text-xs mt-1">{formErrors.endDate}</p>
              )}
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">
              Reason *
            </label>
            <textarea
              rows={3}
              placeholder="Reason for leave..."
              value={formData.reason}
              onChange={(e) => {
                setFormData({ ...formData, reason: e.target.value })
                if (formErrors.reason) {
                  setFormErrors({ ...formErrors, reason: '' })
                }
              }}
              className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white ${
                formErrors.reason ? 'border-red-500' : 'border-neutral-300'
              }`}
            />
            {formErrors.reason && (
              <p className="text-red-500 text-xs mt-1">{formErrors.reason}</p>
            )}
          </div>
        </div>
      </Modal>
    </MainLayout>
  )
}
