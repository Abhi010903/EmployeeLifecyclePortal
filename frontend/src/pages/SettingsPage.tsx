import { useState, useEffect } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import { Settings, Building, User, Calendar, Clock, Mail, AlertCircle } from 'lucide-react'
import { settingsApi, CompanyProfile, Shift, HolidayCalendar, EmailConfiguration } from '@/api/settings'
import { useAuthStore } from '@/store/authStore'
import toast from 'react-hot-toast'

type TabType = 'company' | 'user' | 'holidays' | 'shifts' | 'email'

export default function SettingsPage() {
  const { user } = useAuthStore()
  const [activeTab, setActiveTab] = useState<TabType>('company')
  const [loading, setLoading] = useState(false)
  const [fetching, setFetching] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Company State
  const [company, setCompany] = useState<CompanyProfile>({
    companyName: 'Employee Lifecycle Portal',
    companyCode: 'ELP-001',
    industryType: 'Technology & Software',
    address: 'Tech Park, Innovation Boulevard',
    city: 'Bengaluru',
    state: 'Karnataka',
    country: 'India',
    postalCode: '560100',
    phoneNumber: '+91 80 1234 5678',
    email: 'hr@employeeportal.com',
    website: 'https://employeeportal.com',
    registrationNumber: 'CIN-U72200KA2026PTC123456',
  })

  // User Preferences State
  const [userPrefs, setUserPrefs] = useState({
    theme: 'light',
    language: 'en',
    timeZone: 'UTC',
    emailNotifications: true,
    smsNotifications: false,
    pushNotifications: true,
  })

  // Holidays and Shifts
  const [holidays, setHolidays] = useState<HolidayCalendar[]>([])
  const [shifts, setShifts] = useState<Shift[]>([])

  // Email Config State
  const [emailConfig, setEmailConfig] = useState<EmailConfiguration>({
    smtpServer: 'smtp.gmail.com',
    smtpPort: 587,
    senderEmail: 'noreply@employeeportal.com',
    senderName: 'ELP Notifications',
    enableSsl: true,
    username: 'noreply@employeeportal.com',
  })

  useEffect(() => {
    loadAllSettings()
  }, [])

  const loadAllSettings = async () => {
    try {
      setFetching(true)
      const [compRes, holRes, shiftRes, emailRes] = await Promise.allSettled([
        settingsApi.getCompanyProfile(),
        settingsApi.getHolidays(2026),
        settingsApi.getShifts(),
        settingsApi.getEmailConfig(),
      ])

      if (compRes.status === 'fulfilled' && compRes.value.data) {
        setCompany(compRes.value.data)
      }
      if (holRes.status === 'fulfilled' && Array.isArray(holRes.value.data)) {
        setHolidays(holRes.value.data)
      }
      if (shiftRes.status === 'fulfilled' && Array.isArray(shiftRes.value.data)) {
        setShifts(shiftRes.value.data)
      }
      if (emailRes.status === 'fulfilled' && emailRes.value.data) {
        setEmailConfig(emailRes.value.data)
      }

      if (user?.id) {
        try {
          const userRes = await settingsApi.getUserSettings(user.id)
          if (userRes.data) {
            setUserPrefs({
              theme: userRes.data.theme || 'light',
              language: userRes.data.language || 'en',
              timeZone: userRes.data.timeZone || 'UTC',
              emailNotifications: userRes.data.emailNotifications,
              smsNotifications: userRes.data.smsNotifications,
              pushNotifications: userRes.data.pushNotifications,
            })
          }
        } catch {
          // Defaults are ok
        }
      }
      setError(null)
    } catch (err) {
      console.error(err)
      setError('Could not fetch some settings.')
    } finally {
      setFetching(false)
    }
  }

  const handleSaveCompany = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      setLoading(true)
      await settingsApi.updateCompanyProfile(company)
      toast.success('Company profile updated successfully!')
    } catch (err) {
      toast.error('Failed to update company profile.')
    } finally {
      setLoading(false)
    }
  }

  const handleSaveUserPrefs = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!user?.id) {
      toast.success('Preferences saved locally.')
      return
    }

    try {
      setLoading(true)
      await settingsApi.updateUserSettings(user.id, {
        userId: user.id,
        theme: userPrefs.theme,
        language: userPrefs.language,
        timeZone: userPrefs.timeZone,
        emailNotifications: userPrefs.emailNotifications,
        smsNotifications: userPrefs.smsNotifications,
        pushNotifications: userPrefs.pushNotifications,
        twoFactorEnabled: false,
      })
      toast.success('User preferences updated successfully!')
    } catch (err) {
      toast.error('Failed to update preferences.')
    } finally {
      setLoading(false)
    }
  }

  const handleSaveEmailConfig = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      setLoading(true)
      await settingsApi.updateEmailConfig(emailConfig)
      toast.success('Email configuration updated successfully!')
    } catch (err) {
      toast.error('Failed to update email configuration.')
    } finally {
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

        {fetching ? (
          <div className="flex items-center justify-center py-12">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
          </div>
        ) : (
          <>
            {/* Company Settings */}
            {activeTab === 'company' && (
              <Card>
                <h2 className="text-lg font-semibold text-neutral-900 mb-4">Company Profile</h2>
                <form onSubmit={handleSaveCompany} className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-neutral-700 mb-1">Company Name *</label>
                      <input
                        type="text"
                        value={company.companyName || ''}
                        onChange={(e) => setCompany({ ...company, companyName: e.target.value })}
                        className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                        required
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-neutral-700 mb-1">Company Code</label>
                      <input
                        type="text"
                        value={company.companyCode || ''}
                        onChange={(e) => setCompany({ ...company, companyCode: e.target.value })}
                        className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-neutral-700 mb-1">Email</label>
                      <input
                        type="email"
                        value={company.email || ''}
                        onChange={(e) => setCompany({ ...company, email: e.target.value })}
                        className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-neutral-700 mb-1">Phone</label>
                      <input
                        type="tel"
                        value={company.phoneNumber || ''}
                        onChange={(e) => setCompany({ ...company, phoneNumber: e.target.value })}
                        className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-neutral-700 mb-1">Address</label>
                      <input
                        type="text"
                        value={company.address || ''}
                        onChange={(e) => setCompany({ ...company, address: e.target.value })}
                        className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-neutral-700 mb-1">City</label>
                      <input
                        type="text"
                        value={company.city || ''}
                        onChange={(e) => setCompany({ ...company, city: e.target.value })}
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
                <form onSubmit={handleSaveUserPrefs} className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-neutral-700 mb-1">Theme</label>
                      <select
                        value={userPrefs.theme}
                        onChange={(e) => setUserPrefs({ ...userPrefs, theme: e.target.value })}
                        className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                      >
                        <option value="light">Light</option>
                        <option value="dark">Dark</option>
                      </select>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-neutral-700 mb-1">Language</label>
                      <select
                        value={userPrefs.language}
                        onChange={(e) => setUserPrefs({ ...userPrefs, language: e.target.value })}
                        className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                      >
                        <option value="en">English</option>
                        <option value="hi">Hindi</option>
                        <option value="es">Spanish</option>
                      </select>
                    </div>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-neutral-700 mb-2">Notifications</label>
                    <div className="space-y-2">
                      <label className="flex items-center">
                        <input
                          type="checkbox"
                          checked={userPrefs.emailNotifications}
                          onChange={(e) => setUserPrefs({ ...userPrefs, emailNotifications: e.target.checked })}
                          className="w-4 h-4 rounded border-neutral-300"
                        />
                        <span className="ml-2 text-sm text-neutral-700">Email Notifications</span>
                      </label>
                      <label className="flex items-center">
                        <input
                          type="checkbox"
                          checked={userPrefs.smsNotifications}
                          onChange={(e) => setUserPrefs({ ...userPrefs, smsNotifications: e.target.checked })}
                          className="w-4 h-4 rounded border-neutral-300"
                        />
                        <span className="ml-2 text-sm text-neutral-700">SMS Notifications</span>
                      </label>
                      <label className="flex items-center">
                        <input
                          type="checkbox"
                          checked={userPrefs.pushNotifications}
                          onChange={(e) => setUserPrefs({ ...userPrefs, pushNotifications: e.target.checked })}
                          className="w-4 h-4 rounded border-neutral-300"
                        />
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
                <h2 className="text-lg font-semibold text-neutral-900 mb-4">Holiday Calendar 2026</h2>
                <div className="space-y-2">
                  {holidays.length === 0 ? (
                    <p className="text-neutral-500 text-sm">No holidays recorded for 2026.</p>
                  ) : (
                    holidays.map((holiday, idx) => (
                      <div key={holiday.id || idx} className="flex justify-between items-center p-3 border border-neutral-200 rounded-lg">
                        <div>
                          <p className="font-medium text-neutral-900">{holiday.holidayName}</p>
                          <p className="text-sm text-neutral-600">{new Date(holiday.holidayDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}</p>
                        </div>
                        <Badge label="Holiday" variant="info" />
                      </div>
                    ))
                  )}
                </div>
              </Card>
            )}

            {/* Shifts */}
            {activeTab === 'shifts' && (
              <Card>
                <h2 className="text-lg font-semibold text-neutral-900 mb-4">Work Shifts</h2>
                <div className="space-y-2">
                  {shifts.length === 0 ? (
                    <p className="text-neutral-500 text-sm">No shifts configured.</p>
                  ) : (
                    shifts.map((shift, idx) => (
                      <div key={shift.id || idx} className="flex justify-between items-center p-3 border border-neutral-200 rounded-lg">
                        <div>
                          <p className="font-medium text-neutral-900">{shift.shiftName}</p>
                          <p className="text-sm text-neutral-600">{shift.startTime} - {shift.endTime} ({shift.workingHours}h)</p>
                        </div>
                        <Badge label="Active" variant="success" />
                      </div>
                    ))
                  )}
                </div>
              </Card>
            )}

            {/* Email Configuration */}
            {activeTab === 'email' && (
              <Card>
                <h2 className="text-lg font-semibold text-neutral-900 mb-4">Email Configuration</h2>
                <form onSubmit={handleSaveEmailConfig} className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-neutral-700 mb-1">SMTP Server *</label>
                      <input
                        type="text"
                        value={emailConfig.smtpServer || ''}
                        onChange={(e) => setEmailConfig({ ...emailConfig, smtpServer: e.target.value })}
                        className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                        required
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-neutral-700 mb-1">SMTP Port *</label>
                      <input
                        type="number"
                        value={emailConfig.smtpPort || 587}
                        onChange={(e) => setEmailConfig({ ...emailConfig, smtpPort: parseInt(e.target.value) || 587 })}
                        className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                        required
                      />
                    </div>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-neutral-700 mb-1">Sender Email *</label>
                    <input
                      type="email"
                      value={emailConfig.senderEmail || ''}
                      onChange={(e) => setEmailConfig({ ...emailConfig, senderEmail: e.target.value })}
                      className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                      required
                    />
                  </div>

                  <div>
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={emailConfig.enableSsl}
                        onChange={(e) => setEmailConfig({ ...emailConfig, enableSsl: e.target.checked })}
                        className="w-4 h-4 rounded border-neutral-300"
                      />
                      <span className="ml-2 text-sm text-neutral-700">Enable SSL/TLS</span>
                    </label>
                  </div>

                  <Button type="submit" disabled={loading} className="w-full bg-blue-600 hover:bg-blue-700">
                    {loading ? 'Saving...' : 'Save Email Configuration'}
                  </Button>
                </form>
              </Card>
            )}
          </>
        )}
      </div>
    </MainLayout>
  )
}
