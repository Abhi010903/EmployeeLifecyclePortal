import { useState } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import { Settings, Building, User, Calendar, Clock, Mail, AlertCircle } from 'lucide-react'

type TabType = 'company' | 'user' | 'holidays' | 'shifts' | 'email'

export default function SettingsPage() {
  const [activeTab, setActiveTab] = useState<TabType>('company')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSave = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    try {
      setLoading(true)
      setError(null)
      // API call would go here
      setTimeout(() => {
        setLoading(false)
        // Show success message
      }, 500)
    } catch (err) {
      setError('Failed to save settings')
      setLoading(false)
    }
  }

  return (
    <MainLayout>
      <div className="space-y-6">
        <div className="flex justify-between items-start">
          <div>
            <h1 className="text-3xl font-bold text-neutral-900">Settings</h1>
            <p className="text-neutral-600 mt-1">Manage system, organization, and user settings</p>
          </div>
          <Settings className="w-8 h-8 text-blue-600" />
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 flex items-start gap-3">
            <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
            <p className="text-sm text-red-900">{error}</p>
          </div>
        )}

        {/* Tabs */}
        <div className="flex gap-2 border-b border-neutral-200 overflow-x-auto">
          {[
            { id: 'company', label: 'Company', icon: Building },
            { id: 'user', label: 'User Profile', icon: User },
            { id: 'holidays', label: 'Holidays', icon: Calendar },
            { id: 'shifts', label: 'Shifts', icon: Clock },
            { id: 'email', label: 'Email', icon: Mail },
          ].map(tab => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id as TabType)}
              className={`px-4 py-2 font-medium border-b-2 transition-colors flex items-center gap-2 ${
                activeTab === tab.id
                  ? 'border-blue-600 text-blue-600'
                  : 'border-transparent text-neutral-600 hover:text-neutral-900'
              }`}
            >
              <tab.icon className="w-4 h-4" />
              {tab.label}
            </button>
          ))}
        </div>

        {/* Company Settings */}
        {activeTab === 'company' && (
          <Card>
            <h2 className="text-lg font-semibold text-neutral-900 mb-4">Company Profile</h2>
            <form onSubmit={handleSave} className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-neutral-700 mb-1">Company Name *</label>
                  <input
                    type="text"
                    defaultValue="Your Company"
                    className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-neutral-700 mb-1">Company Code</label>
                  <input
                    type="text"
                    defaultValue="COMP001"
                    className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-neutral-700 mb-1">Email</label>
                  <input
                    type="email"
                    defaultValue="info@company.com"
                    className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-neutral-700 mb-1">Phone</label>
                  <input
                    type="tel"
                    defaultValue="+1 (555) 000-0000"
                    className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-neutral-700 mb-1">Address</label>
                  <input
                    type="text"
                    defaultValue="123 Business St"
                    className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-neutral-700 mb-1">City</label>
                  <input
                    type="text"
                    defaultValue="New York"
                    className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              </div>

              <Button type="submit" disabled={loading} className="w-full bg-blue-600 hover:bg-blue-700">
                {loading ? 'Saving...' : 'Save Company Profile'}
              </Button>
            </form>
          </Card>
        )}

        {/* User Settings */}
        {activeTab === 'user' && (
          <Card>
            <h2 className="text-lg font-semibold text-neutral-900 mb-4">User Preferences</h2>
            <form onSubmit={handleSave} className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-neutral-700 mb-1">Theme</label>
                  <select className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500">
                    <option>Light</option>
                    <option>Dark</option>
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-neutral-700 mb-1">Language</label>
                  <select className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500">
                    <option>English</option>
                    <option>Hindi</option>
                    <option>Spanish</option>
                  </select>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-neutral-700 mb-2">Notifications</label>
                <div className="space-y-2">
                  <label className="flex items-center">
                    <input type="checkbox" defaultChecked className="w-4 h-4 rounded border-neutral-300" />
                    <span className="ml-2 text-sm text-neutral-700">Email Notifications</span>
                  </label>
                  <label className="flex items-center">
                    <input type="checkbox" className="w-4 h-4 rounded border-neutral-300" />
                    <span className="ml-2 text-sm text-neutral-700">SMS Notifications</span>
                  </label>
                  <label className="flex items-center">
                    <input type="checkbox" defaultChecked className="w-4 h-4 rounded border-neutral-300" />
                    <span className="ml-2 text-sm text-neutral-700">Push Notifications</span>
                  </label>
                </div>
              </div>

              <Button type="submit" disabled={loading} className="w-full bg-blue-600 hover:bg-blue-700">
                {loading ? 'Saving...' : 'Save Preferences'}
              </Button>
            </form>
          </Card>
        )}

        {/* Holidays */}
        {activeTab === 'holidays' && (
          <Card>
            <h2 className="text-lg font-semibold text-neutral-900 mb-4">Holiday Calendar 2024</h2>
            <div className="space-y-2">
              {[
                { name: 'New Year', date: '2024-01-01' },
                { name: 'Independence Day', date: '2024-08-15' },
                { name: 'Christmas', date: '2024-12-25' },
              ].map((holiday, idx) => (
                <div key={idx} className="flex justify-between items-center p-3 border border-neutral-200 rounded-lg">
                  <div>
                    <p className="font-medium text-neutral-900">{holiday.name}</p>
                    <p className="text-sm text-neutral-600">{new Date(holiday.date).toLocaleDateString()}</p>
                  </div>
                  <Badge label="Holiday" variant="info" />
                </div>
              ))}
            </div>
          </Card>
        )}

        {/* Shifts */}
        {activeTab === 'shifts' && (
          <Card>
            <h2 className="text-lg font-semibold text-neutral-900 mb-4">Work Shifts</h2>
            <div className="space-y-2">
              {[
                { name: 'Morning Shift', time: '09:00 - 17:00', hours: 8 },
                { name: 'Evening Shift', time: '17:00 - 01:00', hours: 8 },
                { name: 'Night Shift', time: '01:00 - 09:00', hours: 8 },
              ].map((shift, idx) => (
                <div key={idx} className="flex justify-between items-center p-3 border border-neutral-200 rounded-lg">
                  <div>
                    <p className="font-medium text-neutral-900">{shift.name}</p>
                    <p className="text-sm text-neutral-600">{shift.time} ({shift.hours}h)</p>
                  </div>
                  <Badge label="Active" variant="success" />
                </div>
              ))}
            </div>
          </Card>
        )}

        {/* Email Configuration */}
        {activeTab === 'email' && (
          <Card>
            <h2 className="text-lg font-semibold text-neutral-900 mb-4">Email Configuration</h2>
            <form onSubmit={handleSave} className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-neutral-700 mb-1">SMTP Server *</label>
                  <input
                    type="text"
                    defaultValue="smtp.gmail.com"
                    className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-neutral-700 mb-1">SMTP Port *</label>
                  <input
                    type="number"
                    defaultValue="587"
                    className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-neutral-700 mb-1">Sender Email *</label>
                <input
                  type="email"
                  defaultValue="noreply@company.com"
                  className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="flex items-center">
                  <input type="checkbox" defaultChecked className="w-4 h-4 rounded border-neutral-300" />
                  <span className="ml-2 text-sm text-neutral-700">Enable SSL/TLS</span>
                </label>
              </div>

              <Button type="submit" disabled={loading} className="w-full bg-blue-600 hover:bg-blue-700">
                {loading ? 'Saving...' : 'Save Email Configuration'}
              </Button>
            </form>
          </Card>
        )}
      </div>
    </MainLayout>
  )
}
