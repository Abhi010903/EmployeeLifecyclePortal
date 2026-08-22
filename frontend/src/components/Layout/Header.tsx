import { Menu, LogOut, Bell } from 'lucide-react'
import { useAuthStore } from '@/store/authStore'
import { useState } from 'react'

interface HeaderProps {
  onToggleSidebar?: () => void
  isCollapsed?: boolean
}

export default function Header({ onToggleSidebar }: HeaderProps) {
  const { user, logout } = useAuthStore()
  const [isMenuOpen, setIsMenuOpen] = useState(false)

  return (
    <header className="bg-white border-b border-neutral-200 sticky top-0 z-30">
      <div className="max-w-full mx-auto px-6 py-3 flex justify-between items-center">
        <div className="flex items-center gap-3">
          <button
            type="button"
            aria-label="Toggle navigation"
            title="Toggle navigation"
            onClick={onToggleSidebar}
            className="p-2 rounded-lg hover:bg-neutral-100 focus:outline-none focus:ring-2 focus:ring-primary-500 text-neutral-700 transition cursor-pointer"
          >
            <Menu className="w-5 h-5" />
          </button>
          <span className="text-xl font-bold text-primary-600 tracking-tight select-none">HRMS</span>
        </div>

        <div className="flex items-center gap-4">
          <button
            type="button"
            aria-label="Notifications"
            className="p-2 text-neutral-600 hover:text-primary-600 hover:bg-neutral-100 rounded-lg transition"
          >
            <Bell className="w-5 h-5" />
          </button>

          <div className="relative">
            <button
              type="button"
              onClick={() => setIsMenuOpen(!isMenuOpen)}
              className="flex items-center gap-2 px-3 py-1.5 hover:bg-neutral-100 rounded-lg border border-transparent hover:border-neutral-200 transition"
            >
              <div className="w-7 h-7 rounded-full bg-primary-100 text-primary-700 flex items-center justify-center font-bold text-xs">
                {(user?.name || 'A').charAt(0).toUpperCase()}
              </div>
              <span className="text-sm font-medium text-neutral-900 hidden sm:inline">{user?.name}</span>
              <span className="text-xs bg-neutral-100 text-neutral-600 px-1.5 py-0.5 rounded font-mono hidden sm:inline">
                {user?.role}
              </span>
            </button>

            {isMenuOpen && (
              <div className="absolute right-0 mt-2 w-48 bg-white border border-neutral-200 rounded-lg shadow-lg py-1 z-50">
                <div className="px-4 py-2 border-b border-neutral-100">
                  <p className="text-sm font-medium text-neutral-900">{user?.name}</p>
                  <p className="text-xs text-neutral-500 truncate">{user?.email}</p>
                </div>
                <button
                  type="button"
                  onClick={() => {
                    logout()
                    window.location.href = '/login'
                  }}
                  className="w-full text-left px-4 py-2 hover:bg-red-50 text-red-600 flex items-center gap-2 text-sm transition"
                >
                  <LogOut className="w-4 h-4" />
                  Logout
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
    </header>
  )
}
