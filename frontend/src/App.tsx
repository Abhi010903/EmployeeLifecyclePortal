import { useEffect } from 'react'
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import { useAuthStore } from '@/store/authStore'
import ProtectedRoute from '@/utils/ProtectedRoute'

// Pages
import LoginPage from '@/pages/LoginPage'
import DashboardPage from '@/pages/DashboardPage'
import EmployeesPage from '@/pages/EmployeesPage'
import EmployeeDetailsPage from '@/pages/EmployeeDetailsPage'
import EmployeeEditPage from '@/pages/EmployeeEditPage'
import DepartmentsPage from '@/pages/DepartmentsPage'
import DepartmentDetailsPage from '@/pages/DepartmentDetailsPage'
import RolesPage from '@/pages/RolesPage'
import RoleDetailsPage from '@/pages/RoleDetailsPage'
import AttendancePage from '@/pages/AttendancePage'
import LeavePage from '@/pages/LeavePage'
import PayrollPage from '@/pages/PayrollPage'
import PerformancePage from '@/pages/PerformancePage'
import AssetPage from '@/pages/AssetPage'
import RecruitmentPage from '@/pages/RecruitmentPage'
import ReportsPage from '@/pages/ReportsPage'
import SettingsPage from '@/pages/SettingsPage'
import AuthCallbackPage from '@/pages/AuthCallbackPage'

export default function App() {
  const { checkAuth } = useAuthStore()

  useEffect(() => {
    checkAuth()
  }, [])

  return (
    <>
      <Router>
        <Routes>
          {/* Public Routes */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/auth/callback" element={<AuthCallbackPage />} />

          {/* Protected Routes */}
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <DashboardPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/employees"
            element={
              <ProtectedRoute requiredRole={['Admin', 'HR', 'Manager', 'Team Lead', 'TeamLead']}>
                <EmployeesPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/employees/:id"
            element={
              <ProtectedRoute requiredRole={['Admin', 'HR', 'Manager', 'Team Lead', 'TeamLead']}>
                <EmployeeDetailsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/employees/:id/edit"
            element={
              <ProtectedRoute requiredRole={['Admin', 'HR', 'Manager', 'Team Lead', 'TeamLead']}>
                <EmployeeEditPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/departments"
            element={
              <ProtectedRoute requiredRole={['Admin', 'HR', 'Manager', 'Team Lead', 'TeamLead']}>
                <DepartmentsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/departments/:id"
            element={
              <ProtectedRoute requiredRole={['Admin', 'HR', 'Manager', 'Team Lead', 'TeamLead']}>
                <DepartmentDetailsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/roles"
            element={
              <ProtectedRoute requiredRole={['Admin']}>
                <RolesPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/roles/:id"
            element={
              <ProtectedRoute requiredRole={['Admin']}>
                <RoleDetailsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/attendance"
            element={
              <ProtectedRoute>
                <AttendancePage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/leave"
            element={
              <ProtectedRoute>
                <LeavePage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/payroll"
            element={
              <ProtectedRoute>
                <PayrollPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/performance"
            element={
              <ProtectedRoute>
                <PerformancePage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/assets"
            element={
              <ProtectedRoute>
                <AssetPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/recruitment"
            element={
              <ProtectedRoute requiredRole={['Admin', 'HR', 'Manager', 'Team Lead', 'TeamLead']}>
                <RecruitmentPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/reports"
            element={
              <ProtectedRoute requiredRole={['Admin', 'HR', 'Manager']}>
                <ReportsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/settings"
            element={
              <ProtectedRoute requiredRole={['Admin']}>
                <SettingsPage />
              </ProtectedRoute>
            }
          />

          {/* Redirect to dashboard */}
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </Router>

      {/* Toast notifications */}
      <Toaster position="top-right" />
    </>
  )
}
