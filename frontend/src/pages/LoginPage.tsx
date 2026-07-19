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

  return (
    <AuthLayout>
      <div className="bg-white rounded-lg shadow-xl p-8">
        <h1 className="text-3xl font-bold text-center text-neutral-900 mb-2">
          Welcome Back
        </h1>

        <p className="text-center text-neutral-500 mb-8">
          Sign in to your HRMS account
        </p>

        <form
          onSubmit={handleSubmit}
          className="space-y-4"
        >
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
            onChange={(e) =>
              setEmail(e.target.value)
            }
            error={errors.email}
            required
          />

          <Input
            label="Password"
            type="password"
            icon={<Lock className="h-5 w-5" />}
            placeholder="Enter your password"
            value={password}
            onChange={(e) =>
              setPassword(e.target.value)
            }
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

        <div className="mt-6 rounded-lg border border-blue-200 bg-blue-50 p-4">
          <p className="text-sm text-blue-700">
            <strong>Demo Credentials</strong>
          </p>

          <p className="mt-2 text-sm text-blue-600">
            Email:
            <strong> admin@example.com</strong>
          </p>

          <p className="text-sm text-blue-600">
            Password:
            <strong> Admin@123456</strong>
          </p>
        </div>
      </div>
    </AuthLayout>
  )
}