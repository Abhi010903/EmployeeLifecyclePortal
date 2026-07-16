import { BarChart3, Users, FileText, Clock, DollarSign, Briefcase, Trophy, Settings, Home } from 'lucide-react'
import { Link, useLocation } from 'react-router-dom'
import { useState } from 'react'
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
  { label: 'Attendance', icon: <Clock className="w-5 h-5" />, path: '/attendance' },
  { label: 'Leave', icon: <FileText className="w-5 h-5" />, path: '/leave' },
  { label: 'Payroll', icon: <DollarSign className="w-5 h-5" />, path: '/payroll' },
  { label: 'Performance', icon: <Trophy className="w-5 h-5" />, path: '/performance' },
  { label: 'Reports', icon: <BarChart3 className="w-5 h-5" />, path: '/reports', roles: ['Admin', 'HR'] },
  { label: 'Settings', icon: <Settings className="w-5 h-5" />, path: '/settings', roles: ['Admin'] },
]

export default function Sidebar() {
  const location = useLocation()
  const { user } = useAuthStore()
  const [isCollapsed, setIsCollapsed] = useState(false)

  const filteredItems = menuItems.filter(
    (item) => !item.roles || item.roles.includes(user?.role || '')
  )

  return (
    <aside
      className={`bg-neutral-900 text-white transition-all duration-300 ${
        isCollapsed ? 'w-20' : 'w-64'
      } min-h-screen border-r border-neutral-800`}
    >
      <div className="p-4 border-b border-neutral-800 flex justify-between items-center">
        {!isCollapsed && <span className="font-bold text-lg">Menu</span>}
        <button
          onClick={() => setIsCollapsed(!isCollapsed)}
          className="p-1 hover:bg-neutral-800 rounded"
        >
          →
        </button>
      </div>

      <nav className="p-4 space-y-2">
        {filteredItems.map((item) => (
          <Link
            key={item.path}
            to={item.path}
            className={`flex items-center gap-3 px-4 py-2 rounded-lg transition-colors ${
              location.pathname === item.path
                ? 'bg-primary-600 text-white'
                : 'text-neutral-300 hover:bg-neutral-800'
            }`}
          >
            {item.icon}
            {!isCollapsed && <span className="text-sm font-medium">{item.label}</span>}
          </Link>
        ))}
      </nav>
    </aside>
  )
}
