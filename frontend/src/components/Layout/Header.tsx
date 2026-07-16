import { Menu, LogOut, Bell, User } from 'lucide-react'
import { useAuthStore } from '@/store/authStore'
import { useState } from 'react'

export default function Header() {
  const { user, logout } = useAuthStore()
  const [isMenuOpen, setIsMenuOpen] = useState(false)

  return (
    <header className="bg-white border-b border-neutral-200 sticky top-0 z-50">
      <div className="max-w-full mx-auto px-6 py-4 flex justify-between items-center">
        <div className="flex items-center gap-3">
          <Menu className="w-6 h-6 text-neutral-600 cursor-pointer" />
          <h1 className="text-2xl font-bold text-primary-600">HRMS</h1>
        </div>

        <div className="flex items-center gap-6">
          <Bell className="w-6 h-6 text-neutral-600 cursor-pointer hover:text-primary-600" />
          
          <div className="relative">
            <button
              onClick={() => setIsMenuOpen(!isMenuOpen)}
              className="flex items-center gap-2 p-2 hover:bg-neutral-100 rounded-lg"
            >
              <User className="w-5 h-5 text-neutral-600" />
              <span className="text-sm font-medium text-neutral-900">{user?.name}</span>
            </button>

            {isMenuOpen && (
              <div className="absolute right-0 mt-2 w-48 bg-white border border-neutral-200 rounded-lg shadow-lg">
                <button className="w-full text-left px-4 py-2 hover:bg-neutral-50 flex items-center gap-2">
                  <User className="w-4 h-4" />
                  Profile
                </button>
                <button
                  onClick={() => {
                    logout()
                    window.location.href = '/login'
                  }}
                  className="w-full text-left px-4 py-2 hover:bg-neutral-50 text-red-600 flex items-center gap-2"
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
