import { apiClient } from './client'
import type { AuthResponse } from '@/types'

export const authApi = {
  login: async (
    email: string,
    password: string
  ): Promise<AuthResponse> => {
    const response = await apiClient.post('/auth/login', {
      email,
      password,
    })

    return response.data
  },

  register: async (
    email: string,
    password: string,
    firstName: string,
    lastName: string
  ) => {
    const response = await apiClient.post('/auth/register', {
      email,
      password,
      firstName,
      lastName,
    })

    return response.data
  },

  logout: async () => {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
  },
}