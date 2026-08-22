import { create } from 'zustand'
import type { User } from '@/types'

interface AuthStore {
  user: User | null
  token: string | null
  isAuthenticated: boolean

  setAuth: (user: User, token: string) => void

  logout: () => void

  checkAuth: () => void
}

export const useAuthStore = create<AuthStore>((set) => ({
  user: null,

  token: null,

  isAuthenticated: false,

  setAuth: (user, token) => {
    localStorage.setItem('token', token)
    localStorage.setItem('user', JSON.stringify(user))
    if (user && user.id) {
      localStorage.setItem('userId', user.id)
    }

    set({
      user,
      token,
      isAuthenticated: true,
    })
  },

  logout: () => {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    localStorage.removeItem('userId')

    set({
      user: null,
      token: null,
      isAuthenticated: false,
    })
  },

  checkAuth: () => {
    const token = localStorage.getItem('token')
    const user = localStorage.getItem('user')

    if (token && user) {
      try {
        const parsedUser = JSON.parse(user)
        if (parsedUser && parsedUser.id) {
          localStorage.setItem('userId', parsedUser.id)
        }
        set({
          token,
          user: parsedUser,
          isAuthenticated: true,
        })
      } catch {
        localStorage.removeItem('token')
        localStorage.removeItem('user')
        localStorage.removeItem('userId')
      }
    }
  },
}))