import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Mail, Lock } from 'lucide-react'
import AuthLayout from '@/components/Layout/AuthLayout'
import Button from '@/components/Common/Button'
import Input from '@/components/Common/Input'
import { authApi } from '@/api/auth'
import { useAuthStore } from '@/store/authStore'
import toast from 'react-hot-toast'

export default function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
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
      setAuth(response.user, response.token)
      toast.success('Login successful!')
      navigate('/dashboard')
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || 'Login failed'
      setErrors({ form: errorMessage })
      toast.error(errorMessage)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <AuthLayout>
      <div className="bg-white rounded-lg shadow-xl p-8">
        <h1 className="text-3xl font-bold text-center text-neutral-900 mb-2">
          Welcome Back
        </h1>
        <p className="text-center text-neutral-500 mb-8">
          Sign in to your HRMS account
        </p>

        <form onSubmit={handleSubmit} className="space-y-4">
          {errors.form && (
            <div className="p-3 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
              {errors.form}
            </div>
          )}

          <Input
            label="Email"
            type="email"
            icon={<Mail className="w-5 h-5" />}
            placeholder="you@example.com"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            error={errors.email}
            required
          />

          <Input
            label="Password"
            type="password"
            icon={<Lock className="w-5 h-5" />}
            placeholder="••••••••"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            error={errors.password}
            required
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

        <p className="text-center text-neutral-600 text-sm mt-6">
          Demo credentials: admin@example.com / password
        </p>
      </div>
    </AuthLayout>
  )
}
