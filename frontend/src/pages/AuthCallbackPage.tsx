import { useEffect } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { useAuthStore } from '@/store/authStore'
import toast from 'react-hot-toast'

export default function AuthCallbackPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const { setAuth } = useAuthStore()

  useEffect(() => {
    const token = searchParams.get('token')
    const error = searchParams.get('error')

    if (error) {
      toast.error(error)
      navigate('/login')
      return
    }

    if (token) {
      try {
        // Decode payload to extract user info
        const payloadBase64 = token.split('.')[1]
        const decodedJson = JSON.parse(atob(payloadBase64))
        const user = {
          id: decodedJson.nameid || decodedJson.sub || '',
          email: decodedJson.email || '',
          role: decodedJson.role || 'Employee',
          name: decodedJson.unique_name || decodedJson.name || decodedJson.email?.split('@')[0] || 'User',
        }

        setAuth(user, token)
        toast.success('Single Sign-On successful!')
        navigate('/dashboard')
      } catch (err) {
        toast.error('Failed to parse authentication token.')
        navigate('/login')
      }
    } else {
      toast.error('No authentication token received.')
      navigate('/login')
    }
  }, [searchParams, navigate, setAuth])

  return (
    <div className="min-h-screen flex items-center justify-center bg-neutral-100">
      <div className="text-center p-8 bg-white rounded-xl shadow-md">
        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-primary-600 mx-auto mb-4" />
        <p className="text-neutral-700 font-medium">Completing Single Sign-On...</p>
        <p className="text-neutral-400 text-xs mt-1">Please wait while we establish your session</p>
      </div>
    </div>
  )
}
