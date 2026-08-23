import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Mail, Lock, Eye, EyeOff } from 'lucide-react'
import AuthLayout from '@/components/Layout/AuthLayout'
import Button from '@/components/Common/Button'
import Input from '@/components/Common/Input'
import { authApi } from '@/api/auth'
import { useAuthStore } from '@/store/authStore'
import toast from 'react-hot-toast'

export default function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  const navigate = useNavigate()
  const { setAuth } = useAuthStore()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setErrors({})
    setIsLoading(true)

    try {
      const response = await authApi.login(email, password)
      const user = {
        id: response.user.id,
        email: response.user.email,
        role: response.user.role,
        name: response.user.name,
      }

      setAuth(user, response.token)
      toast.success('Login successful!')
      navigate('/dashboard')
    } catch (error: any) {
      const errorMessage =
        error.response?.data?.message ??
        'Login failed'

      setErrors({
        form: errorMessage,
      })

      toast.error(errorMessage)
    } finally {
      setIsLoading(false)
    }
  }

  const handleExternalLogin = async (provider: 'Google' | 'Microsoft') => {
    try {
      const response = await fetch(`/api/auth/external-login?provider=${provider}`, {
        method: 'GET',
        headers: { 'Accept': 'application/json' },
      })

      if (response.redirected) {
        // Official OAuth provider account chooser redirect
        window.location.href = response.url
        return
      }

      if (!response.ok) {
        const data = await response.json().catch(() => ({}))
        const msg = data.message || `${provider} Single Sign-On is not configured on the server. Please sign in with your email and password.`
        toast.error(msg, { duration: 6000 })
        return
      }

      // If backend responded with 200/redirect
      window.location.href = `/api/auth/external-login?provider=${provider}`
    } catch (err) {
      toast.error(`${provider} Single Sign-On is currently not configured. Please use email and password.`)
    }
  }

  return (
    <AuthLayout>
      <div className="bg-white rounded-lg shadow-xl p-8">
        <h1 className="text-3xl font-bold text-center text-neutral-900 mb-2">
          Welcome Back
        </h1>

        <p className="text-center text-neutral-500 mb-6">
          Sign in to your Employee Lifecycle Portal account
        </p>

        {/* OAuth / SSO Providers */}
        <div className="space-y-3 mb-6">
          <button
            type="button"
            onClick={() => handleExternalLogin('Google')}
            className="w-full flex items-center justify-center gap-3 px-4 py-2.5 border border-neutral-300 rounded-lg text-sm font-medium text-neutral-700 bg-white hover:bg-neutral-50 transition shadow-sm cursor-pointer"
          >
            <svg className="w-5 h-5" viewBox="0 0 24 24">
              <path
                fill="#4285F4"
                d="M23.745 12.27c0-.7-.06-1.4-.19-2.07H12v4.51h6.6c-.29 1.52-1.14 2.82-2.4 3.68v3.05h3.88c2.27-2.09 3.665-5.17 3.665-9.17z"
              />
              <path
                fill="#34A853"
                d="M12 24c3.24 0 5.95-1.08 7.93-2.91l-3.88-3.05c-1.08.72-2.45 1.16-4.05 1.16-3.12 0-5.77-2.1-6.72-4.93H1.25v3.15C3.26 21.36 7.33 24 12 24z"
              />
              <path
                fill="#FBBC05"
                d="M5.28 14.27c-.25-.72-.38-1.49-.38-2.27s.13-1.55.38-2.27V6.58H1.25C.45 8.18 0 9.99 0 12s.45 3.82 1.25 5.42l4.03-3.15z"
              />
              <path
                fill="#EA4335"
                d="M12 4.75c1.77 0 3.35.61 4.6 1.8l3.42-3.42C17.95 1.19 15.24 0 12 0 7.33 0 3.26 2.64 1.25 6.58l4.03 3.15c.95-2.83 3.6-4.98 6.72-4.98z"
              />
            </svg>
            Continue with Google
          </button>

          <button
            type="button"
            onClick={() => handleExternalLogin('Microsoft')}
            className="w-full flex items-center justify-center gap-3 px-4 py-2.5 border border-neutral-300 rounded-lg text-sm font-medium text-neutral-700 bg-white hover:bg-neutral-50 transition shadow-sm cursor-pointer"
          >
            <svg className="w-5 h-5" viewBox="0 0 23 23">
              <path fill="#f35325" d="M1 1h10v10H1z" />
              <path fill="#81bc06" d="M12 1h10v10H12z" />
              <path fill="#05a6f0" d="M1 12h10v10H1z" />
              <path fill="#ffba08" d="M12 12h10v10H12z" />
            </svg>
            Continue with Microsoft
          </button>
        </div>

        <div className="relative my-6">
          <div className="absolute inset-0 flex items-center">
            <div className="w-full border-t border-neutral-300" />
          </div>
          <div className="relative flex justify-center text-xs uppercase">
            <span className="bg-white px-2 text-neutral-500 font-medium">Or sign in with email</span>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          {errors.form && (
            <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
              {errors.form}
            </div>
          )}

          <Input
            label="Email"
            type="email"
            icon={<Mail className="h-5 w-5" />}
            placeholder="admin@example.com"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            error={errors.email}
            required
          />

          <Input
            label="Password"
            type={showPassword ? 'text' : 'password'}
            icon={<Lock className="h-5 w-5" />}
            placeholder="Enter your password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            error={errors.password}
            required
            endAdornment={
              <button
                type="button"
                aria-label={showPassword ? 'Hide password' : 'Show password'}
                title={showPassword ? 'Hide password' : 'Show password'}
                onClick={() => setShowPassword(!showPassword)}
                className="p-1 text-neutral-400 hover:text-neutral-600 focus:outline-none transition cursor-pointer"
              >
                {showPassword ? <EyeOff className="h-5 w-5" /> : <Eye className="h-5 w-5" />}
              </button>
            }
          />

          <Button
            type="submit"
            fullWidth
            isLoading={isLoading}
            className="mt-6"
          >
            Sign In
          </Button>
        </form>
      </div>
    </AuthLayout>
  )
}