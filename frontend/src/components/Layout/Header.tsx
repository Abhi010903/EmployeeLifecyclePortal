import { useState, useRef, useEffect } from 'react'
import {
  Menu,
  LogOut,
  Bell,
  Search,
  CheckCircle2,
  Calendar,
  CreditCard,
  User,
  Shield,
} from 'lucide-react'
import { useAuthStore } from '@/store/authStore'
import { useNavigate } from 'react-router-dom'

interface HeaderProps {
  onToggleSidebar?: () => void
  isCollapsed?: boolean
}

export default function Header({ onToggleSidebar }: HeaderProps) {
  const { user, logout } = useAuthStore()
  const [isMenuOpen, setIsMenuOpen] = useState(false)
  const [isNotificationOpen, setIsNotificationOpen] = useState(false)
  const [searchQuery, setSearchQuery] = useState('')
  const navigate = useNavigate()

  const menuRef = useRef<HTMLDivElement>(null)
  const notifRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setIsMenuOpen(false)
      }
      if (notifRef.current && !notifRef.current.contains(event.target as Node)) {
        setIsNotificationOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (searchQuery.trim()) {
      navigate(`/employees?search=${encodeURIComponent(searchQuery.trim())}`)
      setSearchQuery('')
    }
  }

  const notifications = [
    {
      id: 1,
      icon: <Calendar className="w-4 h-4 text-blue-500" />,
      title: 'Upcoming Holiday',
      desc: 'Independence Day on August 15 (National Holiday)',
      time: 'Upcoming',
    },
    {
      id: 2,
      icon: <CreditCard className="w-4 h-4 text-emerald-500" />,
      title: 'Payroll Cycle Active',
      desc: 'August 2026 payroll batch is ready for processing & review',
      time: 'Active',
    },
    {
      id: 3,
      icon: <CheckCircle2 className="w-4 h-4 text-indigo-500" />,
      title: 'System Health',
      desc: 'All database and attendance sync workflows are operational',
      time: 'Live',
    },
  ]

  const userRole = user?.role || 'Employee'
  const roleBadgeColor =
    userRole === 'Admin'
      ? 'bg-purple-100 text-purple-800 border-purple-200'
      : userRole === 'Manager' || userRole === 'Team Lead' || userRole === 'TeamLead'
      ? 'bg-blue-100 text-blue-800 border-blue-200'
      : 'bg-emerald-100 text-emerald-800 border-emerald-200'

  return (
    <header className="bg-white/95 backdrop-blur-md border-b border-neutral-200/80 sticky top-0 z-30 shadow-xs">
      <div className="max-w-full mx-auto px-4 sm:px-6 py-2.5 flex justify-between items-center gap-4">
        {/* Left: Hamburger & Brand */}
        <div className="flex items-center gap-3 shrink-0">
          <button
            type="button"
            aria-label="Toggle navigation"
            title="Toggle navigation"
            onClick={onToggleSidebar}
            className="p-2 rounded-lg hover:bg-neutral-100 text-neutral-600 hover:text-neutral-900 transition cursor-pointer"
          >
            <Menu className="w-5 h-5" />
          </button>
          <div className="flex items-center gap-2">
            <span className="text-xl font-bold bg-gradient-to-r from-primary-600 to-indigo-600 bg-clip-text text-transparent tracking-tight select-none">
              HRMS
            </span>
            <span className="text-[10px] font-semibold tracking-wider uppercase px-2 py-0.5 rounded-full bg-primary-50 text-primary-700 border border-primary-100 hidden sm:inline-block">
              Enterprise
            </span>
          </div>
        </div>

        {/* Center: Search Bar */}
        <div className="hidden md:flex flex-1 max-w-md mx-auto">
          <form onSubmit={handleSearchSubmit} className="w-full relative">
            <Search className="w-4 h-4 text-neutral-400 absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              placeholder="Search employees, departments, roles..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-9 pr-4 py-1.5 text-sm bg-neutral-50/80 hover:bg-neutral-100/80 focus:bg-white border border-neutral-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500/20 focus:border-primary-500 transition shadow-2xs"
            />
          </form>
        </div>

        {/* Right: Notifications & Profile Menu */}
        <div className="flex items-center gap-3 shrink-0">
          {/* Notifications Popover */}
          <div className="relative" ref={notifRef}>
            <button
              type="button"
              aria-label="Notifications"
              onClick={() => setIsNotificationOpen(!isNotificationOpen)}
              className="relative p-2 text-neutral-600 hover:text-primary-600 hover:bg-neutral-100 rounded-lg transition cursor-pointer"
            >
              <Bell className="w-5 h-5" />
              <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-primary-600 rounded-full ring-2 ring-white animate-pulse" />
            </button>

            {isNotificationOpen && (
              <div className="absolute right-0 mt-2 w-80 sm:w-96 bg-white border border-neutral-200 rounded-xl shadow-xl py-2 z-50 animate-in fade-in zoom-in-95 duration-100">
                <div className="px-4 py-2.5 border-b border-neutral-100 flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <h4 className="text-sm font-semibold text-neutral-900">Notifications</h4>
                    <span className="text-xs bg-primary-100 text-primary-700 font-bold px-1.5 py-0.5 rounded-full">
                      {notifications.length}
                    </span>
                  </div>
                  <span className="text-xs text-neutral-400">Live feed</span>
                </div>
                <div className="divide-y divide-neutral-100 max-h-72 overflow-y-auto">
                  {notifications.map((n) => (
                    <div key={n.id} className="p-3.5 hover:bg-neutral-50 flex items-start gap-3 transition">
                      <div className="p-2 rounded-lg bg-neutral-100 shrink-0 mt-0.5">{n.icon}</div>
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center justify-between">
                          <p className="text-xs font-semibold text-neutral-900">{n.title}</p>
                          <span className="text-[10px] text-neutral-400 font-mono">{n.time}</span>
                        </div>
                        <p className="text-xs text-neutral-600 mt-0.5 line-clamp-2">{n.desc}</p>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>

          {/* Profile Dropdown */}
          <div className="relative" ref={menuRef}>
            <button
              type="button"
              onClick={() => setIsMenuOpen(!isMenuOpen)}
              className="flex items-center gap-2.5 p-1 sm:px-3 sm:py-1.5 hover:bg-neutral-100 rounded-xl border border-transparent hover:border-neutral-200/80 transition cursor-pointer"
            >
              <div className="w-8 h-8 rounded-full bg-gradient-to-tr from-primary-600 to-indigo-600 text-white flex items-center justify-center font-bold text-xs shadow-xs ring-2 ring-white">
                {(user?.name || 'A').charAt(0).toUpperCase()}
              </div>
              <div className="text-left hidden sm:block">
                <p className="text-xs font-semibold text-neutral-900 leading-tight truncate max-w-[120px]">
                  {user?.name}
                </p>
                <span
                  className={`text-[10px] px-1.5 py-0.2 rounded border font-medium inline-block mt-0.5 ${roleBadgeColor}`}
                >
                  {userRole}
                </span>
              </div>
            </button>

            {isMenuOpen && (
              <div className="absolute right-0 mt-2 w-56 bg-white border border-neutral-200 rounded-xl shadow-xl py-1.5 z-50 animate-in fade-in zoom-in-95 duration-100">
                <div className="px-4 py-2.5 border-b border-neutral-100">
                  <p className="text-sm font-semibold text-neutral-900">{user?.name}</p>
                  <p className="text-xs text-neutral-500 truncate">{user?.email}</p>
                  <div className="flex items-center gap-1.5 mt-2 text-[11px] text-neutral-600">
                    <Shield className="w-3.5 h-3.5 text-primary-600" />
                    <span>Role: <strong>{userRole}</strong></span>
                  </div>
                </div>

                <div className="py-1">
                  <button
                    type="button"
                    onClick={() => {
                      setIsMenuOpen(false)
                      navigate('/dashboard')
                    }}
                    className="w-full text-left px-4 py-2 hover:bg-neutral-50 text-neutral-700 flex items-center gap-2.5 text-xs font-medium transition cursor-pointer"
                  >
                    <User className="w-4 h-4 text-neutral-400" />
                    My Dashboard
                  </button>
                </div>

                <div className="border-t border-neutral-100 pt-1">
                  <button
                    type="button"
                    onClick={() => {
                      logout()
                      window.location.href = '/login'
                    }}
                    className="w-full text-left px-4 py-2 hover:bg-red-50 text-red-600 flex items-center gap-2.5 text-xs font-medium transition cursor-pointer"
                  >
                    <LogOut className="w-4 h-4" />
                    Sign Out
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </header>
  )
}
