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
} from 'lucide-react'
import { Link, useLocation } from 'react-router-dom'
import { useAuthStore } from '@/store/authStore'

interface MenuItem {
  label: string
  icon: React.ReactNode
  path: string
  roles?: string[]
}

const menuItems: MenuItem[] = [
  { label: 'Dashboard', icon: <Home className="w-5 h-5" />, path: '/dashboard' },
  { label: 'Employees', icon: <Users className="w-5 h-5" />, path: '/employees' },
  { label: 'Departments', icon: <Briefcase className="w-5 h-5" />, path: '/departments' },
  { label: 'Roles', icon: <Shield className="w-5 h-5" />, path: '/roles', roles: ['Admin'] },
  { label: 'Attendance', icon: <Clock className="w-5 h-5" />, path: '/attendance' },
  { label: 'Leave', icon: <FileText className="w-5 h-5" />, path: '/leave' },
  { label: 'Payroll', icon: <CreditCard className="w-5 h-5" />, path: '/payroll' },
  { label: 'Performance', icon: <Trophy className="w-5 h-5" />, path: '/performance' },
  { label: 'Assets', icon: <Package className="w-5 h-5" />, path: '/assets' },
  { label: 'Recruitment', icon: <UserCheck className="w-5 h-5" />, path: '/recruitment', roles: ['Admin', 'HR', 'Manager', 'Team Lead', 'TeamLead'] },
  { label: 'Reports', icon: <BarChart3 className="w-5 h-5" />, path: '/reports', roles: ['Admin', 'HR', 'Manager'] },
  { label: 'Settings', icon: <Settings className="w-5 h-5" />, path: '/settings', roles: ['Admin'] },
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

  return (
    <>
      {/* Mobile Backdrop */}
      {isMobileOpen && (
        <div
          className="fixed inset-0 bg-black/50 z-40 lg:hidden backdrop-blur-sm"
          onClick={onCloseMobile}
          aria-hidden="true"
        />
      )}

      <aside
        className={`bg-neutral-900 text-white transition-all duration-300 flex flex-col z-50
          ${isCollapsed ? 'w-20' : 'w-64'} 
          min-h-screen border-r border-neutral-800
          fixed lg:static inset-y-0 left-0
          ${isMobileOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'}
        `}
      >
        <div className="p-4 border-b border-neutral-800 flex justify-between items-center h-16">
          {!isCollapsed && (
            <div className="flex items-center gap-2">
              <span className="font-bold text-lg text-white">HRMS</span>
              <span className="text-[10px] uppercase font-bold tracking-wider bg-primary-900 text-primary-300 px-1.5 py-0.5 rounded">Portal</span>
            </div>
          )}
          {isCollapsed && (
            <span className="font-bold text-lg text-primary-400 mx-auto">H</span>
          )}
          <button
            type="button"
            aria-label={isCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
            onClick={onToggleCollapse}
            className="hidden lg:flex p-1.5 hover:bg-neutral-800 text-neutral-400 hover:text-white rounded-lg transition"
          >
            {isCollapsed ? <ChevronRight className="w-5 h-5" /> : <ChevronLeft className="w-5 h-5" />}
          </button>
          <button
            type="button"
            aria-label="Close sidebar drawer"
            onClick={onCloseMobile}
            className="lg:hidden p-1.5 hover:bg-neutral-800 text-neutral-400 hover:text-white rounded-lg transition"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <nav className="p-3 space-y-1 flex-1 overflow-y-auto">
          {filteredItems.map((item) => {
            const isActive = location.pathname === item.path
            return (
              <Link
                key={item.path}
                to={item.path}
                title={isCollapsed ? item.label : undefined}
                onClick={onCloseMobile}
                className={`flex items-center gap-3 px-3 py-2.5 rounded-lg transition-all text-sm font-medium ${
                  isActive
                    ? 'bg-primary-600 text-white shadow-sm'
                    : 'text-neutral-300 hover:bg-neutral-800 hover:text-white'
                } ${isCollapsed ? 'justify-center px-0' : ''}`}
              >
                <div className="flex-shrink-0">{item.icon}</div>
                {!isCollapsed && <span>{item.label}</span>}
              </Link>
            )
          })}
        </nav>
      </aside>
    </>
  )
}
