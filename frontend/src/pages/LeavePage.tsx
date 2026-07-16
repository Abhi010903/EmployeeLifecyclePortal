import { useState } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import Modal from '@/components/Common/Modal'
import Input from '@/components/Common/Input'
import { Plus, Calendar, CheckCircle, XCircle } from 'lucide-react'
import { format } from 'date-fns'

interface LeaveRecord {
  id: string
  employeeName: string
  type: string
  startDate: string
  endDate: string
  days: number
  status: 'Pending' | 'Approved' | 'Rejected'
  reason: string
}

export default function LeavePage() {
  const [records, setRecords] = useState<LeaveRecord[]>([
    {
      id: '1',
      employeeName: 'John Doe',
      type: 'Annual Leave',
      startDate: '2024-01-15',
      endDate: '2024-01-19',
      days: 5,
      status: 'Approved',
      reason: 'Vacation',
    },
    {
      id: '2',
      employeeName: 'Jane Smith',
      type: 'Sick Leave',
      startDate: '2024-01-10',
      endDate: '2024-01-10',
      days: 1,
      status: 'Approved',
      reason: 'Medical appointment',
    },
    {
      id: '3',
      employeeName: 'Bob Johnson',
      type: 'Annual Leave',
      startDate: '2024-01-20',
      endDate: '2024-01-25',
      days: 6,
      status: 'Pending',
      reason: 'Family trip',
    },
  ])
  const [isModalOpen, setIsModalOpen] = useState(false)

  const statusColors: Record<string, 'success' | 'warning' | 'danger'> = {
    Approved: 'success',
    Pending: 'warning',
    Rejected: 'danger',
  }

  const leaveBalance = {
    annual: { used: 12, total: 20 },
    sick: { used: 2, total: 10 },
    casual: { used: 1, total: 5 },
  }

  return (
    <MainLayout>
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold text-neutral-900">Leave Management</h1>
            <p className="text-neutral-600 mt-1">Request and manage employee leaves</p>
          </div>
          <Button onClick={() => setIsModalOpen(true)}>
            <Plus className="w-4 h-4 mr-2" />
            Request Leave
          </Button>
        </div>

        {/* Leave Balance */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <Card>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Annual Leave</p>
                <p className="text-2xl font-bold text-neutral-900 mt-2">
                  {leaveBalance.annual.total - leaveBalance.annual.used} days
                </p>
                <p className="text-neutral-500 text-xs mt-1">
                  {leaveBalance.annual.used} of {leaveBalance.annual.total} used
                </p>
              </div>
              <Calendar className="w-8 h-8 text-primary-600" />
            </div>
            <div className="mt-4 bg-neutral-100 rounded-full h-2">
              <div
                className="bg-primary-600 h-2 rounded-full"
                style={{ width: `${(leaveBalance.annual.used / leaveBalance.annual.total) * 100}%` }}
              />
            </div>
          </Card>

          <Card>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Sick Leave</p>
                <p className="text-2xl font-bold text-neutral-900 mt-2">
                  {leaveBalance.sick.total - leaveBalance.sick.used} days
                </p>
                <p className="text-neutral-500 text-xs mt-1">
                  {leaveBalance.sick.used} of {leaveBalance.sick.total} used
                </p>
              </div>
              <Calendar className="w-8 h-8 text-green-600" />
            </div>
            <div className="mt-4 bg-neutral-100 rounded-full h-2">
              <div
                className="bg-green-600 h-2 rounded-full"
                style={{ width: `${(leaveBalance.sick.used / leaveBalance.sick.total) * 100}%` }}
              />
            </div>
          </Card>

          <Card>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Casual Leave</p>
                <p className="text-2xl font-bold text-neutral-900 mt-2">
                  {leaveBalance.casual.total - leaveBalance.casual.used} days
                </p>
                <p className="text-neutral-500 text-xs mt-1">
                  {leaveBalance.casual.used} of {leaveBalance.casual.total} used
                </p>
              </div>
              <Calendar className="w-8 h-8 text-yellow-600" />
            </div>
            <div className="mt-4 bg-neutral-100 rounded-full h-2">
              <div
                className="bg-yellow-600 h-2 rounded-full"
                style={{ width: `${(leaveBalance.casual.used / leaveBalance.casual.total) * 100}%` }}
              />
            </div>
          </Card>
        </div>

        {/* Leave Requests */}
        <Card>
          <h2 className="text-lg font-semibold text-neutral-900 mb-4">Leave Requests</h2>

          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-neutral-200 bg-neutral-50">
                  <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                    Employee
                  </th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                    Type
                  </th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                    Period
                  </th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                    Days
                  </th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">
                    Status
                  </th>
                </tr>
              </thead>
              <tbody>
                {records.map((record) => (
                  <tr key={record.id} className="border-b border-neutral-200 hover:bg-neutral-50">
                    <td className="px-6 py-4 text-sm text-neutral-700">{record.employeeName}</td>
                    <td className="px-6 py-4 text-sm text-neutral-700">{record.type}</td>
                    <td className="px-6 py-4 text-sm text-neutral-700">
                      {record.startDate} to {record.endDate}
                    </td>
                    <td className="px-6 py-4 text-sm text-neutral-700">{record.days}</td>
                    <td className="px-6 py-4 text-sm">
                      <Badge label={record.status} variant={statusColors[record.status]} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      </div>

      {/* Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title="Request Leave"
        footer={
          <>
            <Button variant="outline" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={() => setIsModalOpen(false)}>
              Submit Request
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">
              Leave Type
            </label>
            <select className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500">
              <option>Annual Leave</option>
              <option>Sick Leave</option>
              <option>Casual Leave</option>
            </select>
          </div>
          <Input label="Start Date" type="date" />
          <Input label="End Date" type="date" />
          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Reason</label>
            <textarea
              rows={3}
              placeholder="Enter reason for leave..."
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
          </div>
        </div>
      </Modal>
    </MainLayout>
  )
}
