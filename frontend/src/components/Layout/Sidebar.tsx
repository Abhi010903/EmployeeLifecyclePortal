import {
  BarChart3,
  Users,
  FileText,
  Clock,
  Briefcase,
  Trophy,
  Settings,
  Home,
  Package,
  UserCheck,
  Shield,
  CreditCard,
  ChevronLeft,
  ChevronRight,
  X,
  Sparkles,
} from 'lucide-react'
import { Link, useLocation } from 'react-router-dom'
import { useAuthStore } from '@/store/authStore'

interface MenuItem {
  label: string
  icon: React.ReactNode
  path: string
  roles?: string[]
  badge?: string
  section?: string
}

const menuItems: MenuItem[] = [
  // Core
  { label: 'Dashboard', icon: <Home className="w-5 h-5" />, path: '/dashboard', section: 'Main' },

  // People & Workflows
  { label: 'Employees', icon: <Users className="w-5 h-5" />, path: '/employees', section: 'People' },
  { label: 'Departments', icon: <Briefcase className="w-5 h-5" />, path: '/departments', section: 'People' },
  { label: 'Roles & Access', icon: <Shield className="w-5 h-5" />, path: '/roles', roles: ['Admin'], section: 'People' },
  { label: 'Attendance', icon: <Clock className="w-5 h-5" />, path: '/attendance', section: 'Time & Attendance' },
  { label: 'Leave Requests', icon: <FileText className="w-5 h-5" />, path: '/leave', section: 'Time & Attendance' },

  // Finance & Operations
  { label: 'Payroll & Salary', icon: <CreditCard className="w-5 h-5" />, path: '/payroll', section: 'Finance' },
  { label: 'Performance', icon: <Trophy className="w-5 h-5" />, path: '/performance', section: 'Operations' },
  { label: 'Company Assets', icon: <Package className="w-5 h-5" />, path: '/assets', section: 'Operations' },
  { label: 'Recruitment', icon: <UserCheck className="w-5 h-5" />, path: '/recruitment', roles: ['Admin', 'HR', 'Manager', 'Team Lead', 'TeamLead'], section: 'Operations' },

  // Intelligence & Administration
  { label: 'Analytics & Reports', icon: <BarChart3 className="w-5 h-5" />, path: '/reports', roles: ['Admin', 'HR', 'Manager'], section: 'Analytics' },
  { label: 'Settings', icon: <Settings className="w-5 h-5" />, path: '/settings', roles: ['Admin'], section: 'System' },
]

interface SidebarProps {
  isCollapsed?: boolean
  onToggleCollapse?: () => void
  isMobileOpen?: boolean
  onCloseMobile?: () => void
}

export default function Sidebar({
  isCollapsed = false,
  onToggleCollapse,
  isMobileOpen = false,
  onCloseMobile,
}: SidebarProps) {
  const location = useLocation()
  const { user } = useAuthStore()

  const filteredItems = menuItems.filter(
    (item) => !item.roles || item.roles.includes(user?.role || '')
  )

  let lastSection = ''

  return (
    <>
      {/* Mobile Backdrop */}
      {isMobileOpen && (
        <div
          className="fixed inset-0 bg-neutral-900/60 z-40 lg:hidden backdrop-blur-sm transition-opacity"
          onClick={onCloseMobile}
          aria-hidden="true"
        />
      )}

      <aside
        className={`bg-slate-950 text-slate-200 transition-all duration-300 flex flex-col z-50
          ${isCollapsed ? 'w-20' : 'w-64'} 
          min-h-screen border-r border-slate-800/80 shadow-xl
          fixed lg:static inset-y-0 left-0
          ${isMobileOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'}
        `}
      >
        {/* Brand Header */}
        <div className="p-4 border-b border-slate-800/80 flex justify-between items-center h-16 bg-slate-950/80">
          {!isCollapsed && (
            <div className="flex items-center gap-2.5">
              <div className="w-9 h-9 rounded-xl bg-gradient-to-tr from-primary-600 via-indigo-600 to-primary-400 flex items-center justify-center text-white shadow-md shadow-primary-900/30">
                <Sparkles className="w-5 h-5" />
              </div>
              <div>
                <span className="font-bold text-base text-white tracking-tight">Antigravity</span>
                <span className="text-[10px] block font-semibold text-primary-400 -mt-0.5 tracking-wider uppercase">
                  HRMS Portal
                </span>
              </div>
            </div>
          )}
          {isCollapsed && (
            <div className="w-9 h-9 mx-auto rounded-xl bg-gradient-to-tr from-primary-600 via-indigo-600 to-primary-400 flex items-center justify-center text-white shadow-md shadow-primary-900/30">
              <Sparkles className="w-5 h-5" />
            </div>
          )}
          <button
            type="button"
            aria-label={isCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
            onClick={onToggleCollapse}
            className="hidden lg:flex p-1.5 hover:bg-slate-800 text-slate-400 hover:text-white rounded-lg transition cursor-pointer"
          >
            {isCollapsed ? <ChevronRight className="w-5 h-5" /> : <ChevronLeft className="w-5 h-5" />}
          </button>
          <button
            type="button"
            aria-label="Close sidebar drawer"
            onClick={onCloseMobile}
            className="lg:hidden p-1.5 hover:bg-slate-800 text-slate-400 hover:text-white rounded-lg transition cursor-pointer"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Navigation List */}
        <nav className="p-3 space-y-1 flex-1 overflow-y-auto scrollbar-thin scrollbar-thumb-slate-800">
          {filteredItems.map((item) => {
            const isActive = location.pathname === item.path
            const showSection = !isCollapsed && item.section && item.section !== lastSection
            if (item.section) lastSection = item.section

            return (
              <div key={item.path}>
                {showSection && (
                  <p className="text-[11px] uppercase tracking-wider font-bold text-slate-500 px-3 pt-3.5 pb-1">
                    {item.section}
                  </p>
                )}
                <Link
                  to={item.path}
                  title={isCollapsed ? item.label : undefined}
                  onClick={onCloseMobile}
                  className={`group relative flex items-center gap-3 px-3 py-2 rounded-xl transition-all duration-150 text-sm font-medium ${
                    isActive
                      ? 'bg-gradient-to-r from-primary-600 to-indigo-600 text-white shadow-md shadow-primary-950'
                      : 'text-slate-400 hover:bg-slate-900 hover:text-slate-100'
                  } ${isCollapsed ? 'justify-center px-0' : ''}`}
                >
                  <div
                    className={`shrink-0 transition-transform group-hover:scale-110 ${
                      isActive ? 'text-white' : 'text-slate-400 group-hover:text-slate-200'
                    }`}
                  >
                    {item.icon}
                  </div>
                  {!isCollapsed && (
                    <span className="flex-1 truncate">{item.label}</span>
                  )}
                  {isActive && !isCollapsed && (
                    <span className="w-1.5 h-1.5 rounded-full bg-white animate-pulse" />
                  )}
                </Link>
              </div>
            )
          })}
        </nav>

        {/* User Card in Footer */}
        {!isCollapsed && (
          <div className="p-3 border-t border-slate-800/80 bg-slate-950/60">
            <div className="flex items-center gap-3 p-2 rounded-xl bg-slate-900/80 border border-slate-800">
              <div className="w-8 h-8 rounded-full bg-gradient-to-br from-indigo-500 to-purple-600 text-white flex items-center justify-center font-bold text-xs shrink-0 shadow-xs">
                {(user?.name || 'A').charAt(0).toUpperCase()}
              </div>
              <div className="min-w-0 flex-1">
                <p className="text-xs font-semibold text-slate-200 truncate">{user?.name}</p>
                <span className="text-[10px] text-slate-400 font-mono block truncate">{user?.role}</span>
              </div>
            </div>
          </div>
        )}
      </aside>
    </>
  )
}
