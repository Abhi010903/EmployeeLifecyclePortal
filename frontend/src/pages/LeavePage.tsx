import { useEffect, useState, useCallback } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import Modal from '@/components/Common/Modal'
import { Plus, Calendar, AlertCircle } from 'lucide-react'
import { leaveApi } from '@/api/attendance'
import { LeaveBalanceDto, LeaveRequestDto, LeaveTypeDto } from '@/types'
import toast from 'react-hot-toast'

export default function LeavePage() {
  const [balances, setBalances] = useState<LeaveBalanceDto[]>([])
  const [requests, setRequests] = useState<LeaveRequestDto[]>([])
  const [leaveTypes, setLeaveTypes] = useState<LeaveTypeDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [formData, setFormData] = useState({
    leaveTypeId: '',
    startDate: '',
    endDate: '',
    reason: '',
  })
  const [formErrors, setFormErrors] = useState<Record<string, string>>({})

  const userId = localStorage.getItem('userId') || ''

  const fetchData = useCallback(async () => {
    try {
      setLoading(true)
      const [typesRes, balancesRes, requestsRes] = await Promise.all([
        leaveApi.getTypes(),
        leaveApi.getBalance(userId),
        leaveApi.getRequests(userId),
      ])

      setLeaveTypes(typesRes.data || [])
      setBalances(balancesRes.data || [])
      setRequests(requestsRes.data || [])
      setError(null)
    } catch (err) {
      setError('Failed to load leave data')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }, [userId])

  useEffect(() => {
    if (userId) {
      fetchData()
    }
  }, [userId, fetchData])

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {}

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

  const handleSubmit = async () => {
    if (!validateForm()) return

    try {
      setIsSubmitting(true)
      await leaveApi.apply({
        employeeId: userId,
        leaveTypeId: formData.leaveTypeId,
        startDate: formData.startDate,
        endDate: formData.endDate,
        reason: formData.reason,
      })

      toast.success('Leave request submitted successfully')
      setIsModalOpen(false)
      setFormData({
        leaveTypeId: '',
        startDate: '',
        endDate: '',
        reason: '',
      })
      await fetchData()
    } catch (err) {
      toast.error('Failed to submit leave request')
      console.error(err)
    } finally {
      setIsSubmitting(false)
    }
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    })
  }

  const getLeaveTypeColor = (index: number): string => {
    const colors = ['bg-primary-50', 'bg-green-50', 'bg-yellow-50', 'bg-purple-50', 'bg-pink-50']
    return colors[index % colors.length]
  }

  const getLeaveTypeIconColor = (index: number): string => {
    const colors = ['text-primary-600', 'text-green-600', 'text-yellow-600', 'text-purple-600', 'text-pink-600']
    return colors[index % colors.length]
  }

  const statusColors: Record<string, 'success' | 'warning' | 'danger'> = {
    Approved: 'success',
    Pending: 'warning',
    Rejected: 'danger',
  }

  return (
    <MainLayout>
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold text-neutral-900">Leave Management</h1>
            <p className="text-neutral-600 mt-1">Request and manage your leaves</p>
          </div>
          <Button onClick={() => setIsModalOpen(true)}>
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

        {/* Leave Balance */}
        {loading && balances.length === 0 ? (
          <div className="text-center py-12">
            <Calendar className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
            <p className="text-neutral-500">Loading leave balances...</p>
          </div>
        ) : balances.length === 0 ? (
          <div className="text-center py-12">
            <Calendar className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
            <p className="text-neutral-500">No leave balances available</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {balances.map((balance, idx) => {
              const usagePercent = (balance.usedDays / balance.totalDays) * 100
              return (
                <Card key={balance.id} className={getLeaveTypeColor(idx)}>
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-neutral-600 text-sm">{balance.leaveTypeName}</p>
                      <p className="text-2xl font-bold text-neutral-900 mt-2">{balance.remainingDays}</p>
                      <p className="text-neutral-500 text-xs mt-1">
                        {balance.usedDays} of {balance.totalDays} used
                      </p>
                    </div>
                    <Calendar className={`w-8 h-8 ${getLeaveTypeIconColor(idx)}`} />
                  </div>
                  <div className="mt-4 bg-neutral-200 rounded-full h-2">
                    <div
                      className="bg-gradient-to-r from-primary-600 to-primary-500 h-2 rounded-full transition-all"
                      style={{ width: `${usagePercent}%` }}
                    />
                  </div>
                </Card>
              )
            })}
          </div>
        )}

        {/* Leave Requests */}
        <Card>
          <h2 className="text-lg font-semibold text-neutral-900 mb-4">My Leave Requests</h2>

          {requests.length === 0 ? (
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
                      Leave Type
                    </th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                      Period
                    </th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                      Days
                    </th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                      Reason
                    </th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                      Status
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {requests.map((request) => (
                    <tr key={request.id} className="border-b border-neutral-200 hover:bg-neutral-50">
                      <td className="px-6 py-4 text-sm text-neutral-700 font-medium">
                        {request.leaveTypeName}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700">
                        {formatDate(request.startDateUtc)} to {formatDate(request.endDateUtc)}
                      </td>
                      <td className="px-6 py-4 text-sm text-neutral-700">{request.daysRequested}</td>
                      <td className="px-6 py-4 text-sm text-neutral-700">{request.reason || '-'}</td>
                      <td className="px-6 py-4 text-sm">
                        <Badge label={request.status} variant={statusColors[request.status] as any} />
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
          setFormData({
            leaveTypeId: '',
            startDate: '',
            endDate: '',
            reason: '',
          })
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
          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">
              Leave Type *
            </label>
            <select
              value={formData.leaveTypeId}
              onChange={(e) => {
                setFormData({ ...formData, leaveTypeId: e.target.value })
                setFormErrors({ ...formErrors, leaveTypeId: '' })
              }}
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              <option value="">Select a leave type</option>
              {leaveTypes.map((type) => (
                <option key={type.id} value={type.id}>
                  {type.name} ({type.daysPerYear} days/year)
                </option>
              ))}
            </select>
            {formErrors.leaveTypeId && (
              <p className="text-red-500 text-sm mt-1">{formErrors.leaveTypeId}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">
              Start Date *
            </label>
            <input
              type="date"
              value={formData.startDate}
              onChange={(e) => {
                setFormData({ ...formData, startDate: e.target.value })
                setFormErrors({ ...formErrors, startDate: '' })
              }}
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
            {formErrors.startDate && (
              <p className="text-red-500 text-sm mt-1">{formErrors.startDate}</p>
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
                setFormErrors({ ...formErrors, endDate: '' })
              }}
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
            {formErrors.endDate && (
              <p className="text-red-500 text-sm mt-1">{formErrors.endDate}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Reason *</label>
            <textarea
              rows={3}
              placeholder="Enter reason for leave..."
              value={formData.reason}
              onChange={(e) => {
                setFormData({ ...formData, reason: e.target.value })
                setFormErrors({ ...formErrors, reason: '' })
              }}
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
            {formErrors.reason && (
              <p className="text-red-500 text-sm mt-1">{formErrors.reason}</p>
            )}
          </div>
        </div>
      </Modal>
    </MainLayout>
  )
}
