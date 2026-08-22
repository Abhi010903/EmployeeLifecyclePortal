import { useEffect, useState, useCallback } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import { Target, Award, TrendingUp, Plus, AlertCircle } from 'lucide-react'
import { performanceApi } from '@/api/performance'
import { employeesApi } from '@/api/employees'
import {
  PerformanceGoalDto,
  PerformanceReviewDto,
  KPIDto,
  Employee,
} from '@/types'
import { useAuthStore } from '@/store/authStore'
import { formatDateIST } from '@/utils/format'

type TabType = 'goals' | 'reviews' | 'kpis'

export default function PerformancePage() {
  const { user } = useAuthStore()
  const employeeId = user?.id || localStorage.getItem('userId') || ''
  const [employees, setEmployees] = useState<Employee[]>([])
  const [activeTab, setActiveTab] = useState<TabType>('goals')
  const [goals, setGoals] = useState<PerformanceGoalDto[]>([])
  const [reviews, setReviews] = useState<PerformanceReviewDto[]>([])
  const [kpis, setKpis] = useState<KPIDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [showGoalForm, setShowGoalForm] = useState(false)
  const [showReviewForm, setShowReviewForm] = useState(false)
  const [showKPIForm, setShowKPIForm] = useState(false)

  const fetchGoals = useCallback(async () => {
    try {
      const response = await performanceApi.goals.getAll()
      setGoals(Array.isArray(response.data) ? response.data : [])
    } catch (err) {
      console.error('Failed to load goals:', err)
    }
  }, [])

  const fetchReviews = useCallback(async () => {
    try {
      const response = await performanceApi.reviews.getAll()
      setReviews(Array.isArray(response.data) ? response.data : [])
    } catch (err) {
      console.error('Failed to load reviews:', err)
    }
  }, [])

  const fetchKPIs = useCallback(async () => {
    try {
      const response = await performanceApi.kpis.getAll()
      setKpis(Array.isArray(response.data) ? response.data : [])
    } catch (err) {
      console.error('Failed to load KPIs:', err)
    }
  }, [])

  const fetchEmployees = useCallback(async () => {
    try {
      const res = await employeesApi.getAll(1, 1000)
      const list = Array.isArray(res) ? res : (res?.items || [])
      setEmployees(list)
    } catch (err) {
      console.error('Failed to load employees:', err)
    }
  }, [])

  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true)
        await Promise.all([fetchGoals(), fetchReviews(), fetchKPIs(), fetchEmployees()])
        setError(null)
      } catch (err) {
        setError('Failed to load performance data')
        console.error(err)
      } finally {
        setLoading(false)
      }
    }

    loadData()
  }, [fetchGoals, fetchReviews, fetchKPIs, fetchEmployees])

  const handleCreateGoal = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    const formData = new FormData(e.currentTarget)
    const targetEmpId = (formData.get('employeeId') as string) || (employees.length > 0 ? employees[0].id : employeeId)

    if (!targetEmpId) {
      setError('Please select an employee')
      return
    }

    try {
      await performanceApi.goals.create({
        employeeId: targetEmpId,
        title: formData.get('title') as string,
        description: formData.get('description') as string,
        startDate: formData.get('startDate') as string,
        endDate: formData.get('endDate') as string,
      })
      setShowGoalForm(false)
      await fetchGoals()
      setError(null)
    } catch (err: any) {
      setError(err?.response?.data?.message || 'Failed to create goal')
      console.error(err)
    }
  }

  const handleCreateReview = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    const formData = new FormData(e.currentTarget)
    const targetEmpId = (formData.get('employeeId') as string) || (employees.length > 0 ? employees[0].id : employeeId)

    if (!targetEmpId) {
      setError('Please select an employee')
      return
    }

    try {
      await performanceApi.reviews.submit({
        employeeId: targetEmpId,
        year: parseInt(formData.get('year') as string),
        quarter: parseInt(formData.get('quarter') as string),
        rating: parseInt(formData.get('rating') as string),
        comments: formData.get('comments') as string,
      })
      setShowReviewForm(false)
      await fetchReviews()
      setError(null)
    } catch (err: any) {
      setError(err?.response?.data?.message || 'Failed to submit review')
      console.error(err)
    }
  }

  const handleCreateKPI = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    const formData = new FormData(e.currentTarget)
    const targetEmpId = (formData.get('employeeId') as string) || (employees.length > 0 ? employees[0].id : employeeId)

    if (!targetEmpId) {
      setError('Please select an employee')
      return
    }

    try {
      await performanceApi.kpis.create({
        employeeId: targetEmpId,
        name: formData.get('name') as string,
        target: parseFloat(formData.get('target') as string),
        year: parseInt(formData.get('year') as string),
      })
      setShowKPIForm(false)
      await fetchKPIs()
      setError(null)
    } catch (err: any) {
      setError(err?.response?.data?.message || 'Failed to create KPI')
      console.error(err)
    }
  }

  const handleUpdateGoalProgress = async (goalId: string, progress: number) => {
    try {
      await performanceApi.goals.updateProgress(goalId, progress)
      await fetchGoals()
    } catch (err) {
      setError('Failed to update goal progress')
      console.error(err)
    }
  }

  const handleCompleteGoal = async (goalId: string) => {
    try {
      await performanceApi.goals.complete(goalId)
      await fetchGoals()
    } catch (err) {
      setError('Failed to complete goal')
      console.error(err)
    }
  }

  const handleUpdateKPIAchievement = async (kpiId: string, achieved: number) => {
    try {
      await performanceApi.kpis.updateAchievement(kpiId, achieved)
      await fetchKPIs()
    } catch (err) {
      setError('Failed to update KPI achievement')
      console.error(err)
    }
  }

  const activeGoals = goals.filter((g) => g.status === 'Active').length
  const avgRating =
    reviews.length > 0
      ? (reviews.reduce((sum, r) => sum + r.rating, 0) / reviews.length).toFixed(1)
      : 0
  const avgKPIAchievement =
    kpis.length > 0
      ? (kpis.reduce((sum, k) => sum + k.achievementPercentage, 0) / kpis.length).toFixed(1)
      : 0

  return (
    <MainLayout>
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-neutral-900">Performance Management</h1>
          <p className="text-neutral-600 mt-1">Track goals, performance reviews, and KPIs</p>
        </div>

        {error && (
          <div className="p-4 bg-red-50 border border-red-200 rounded-lg flex items-center gap-3 text-red-800">
            <AlertCircle className="w-5 h-5 flex-shrink-0" />
            <p>{error}</p>
          </div>
        )}

        {/* Stats Row */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <Card>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-neutral-600">Active Goals</p>
                <p className="text-2xl font-bold text-neutral-900 mt-1">{activeGoals}</p>
              </div>
              <div className="p-3 bg-blue-50 rounded-lg">
                <Target className="w-6 h-6 text-blue-600" />
              </div>
            </div>
          </Card>

          <Card>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-neutral-600">Average Rating</p>
                <p className="text-2xl font-bold text-neutral-900 mt-1">{avgRating}/5</p>
              </div>
              <div className="p-3 bg-amber-50 rounded-lg">
                <Award className="w-6 h-6 text-amber-600" />
              </div>
            </div>
          </Card>

          <Card>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-neutral-600">Avg KPI Achievement</p>
                <p className="text-2xl font-bold text-neutral-900 mt-1">{avgKPIAchievement}%</p>
              </div>
              <div className="p-3 bg-green-50 rounded-lg">
                <TrendingUp className="w-6 h-6 text-green-600" />
              </div>
            </div>
          </Card>
        </div>

        {/* Tabs */}
        <div className="border-b border-neutral-200">
          <div className="flex gap-8">
            <button
              onClick={() => setActiveTab('goals')}
              className={`pb-4 px-1 font-medium text-sm border-b-2 transition-colors ${
                activeTab === 'goals'
                  ? 'border-blue-600 text-blue-600'
                  : 'border-transparent text-neutral-600 hover:text-neutral-900'
              }`}
            >
              Goals ({goals.length})
            </button>
            <button
              onClick={() => setActiveTab('reviews')}
              className={`pb-4 px-1 font-medium text-sm border-b-2 transition-colors ${
                activeTab === 'reviews'
                  ? 'border-blue-600 text-blue-600'
                  : 'border-transparent text-neutral-600 hover:text-neutral-900'
              }`}
            >
              Performance Reviews ({reviews.length})
            </button>
            <button
              onClick={() => setActiveTab('kpis')}
              className={`pb-4 px-1 font-medium text-sm border-b-2 transition-colors ${
                activeTab === 'kpis'
                  ? 'border-blue-600 text-blue-600'
                  : 'border-transparent text-neutral-600 hover:text-neutral-900'
              }`}
            >
              KPIs ({kpis.length})
            </button>
          </div>
        </div>

        {/* Content */}
        {loading ? (
          <Card>
            <div className="text-center py-12">
              <p className="text-neutral-500">Loading performance data...</p>
            </div>
          </Card>
        ) : activeTab === 'goals' ? (
          <GoalsSection
            goals={goals}
            employees={employees}
            onCreateClick={() => setShowGoalForm(!showGoalForm)}
            showForm={showGoalForm}
            onSubmit={handleCreateGoal}
            onUpdateProgress={handleUpdateGoalProgress}
            onComplete={handleCompleteGoal}
          />
        ) : activeTab === 'reviews' ? (
          <ReviewsSection
            reviews={reviews}
            employees={employees}
            onCreateClick={() => setShowReviewForm(!showReviewForm)}
            showForm={showReviewForm}
            onSubmit={handleCreateReview}
          />
        ) : (
          <KPIsSection
            kpis={kpis}
            employees={employees}
            onCreateClick={() => setShowKPIForm(!showKPIForm)}
            showForm={showKPIForm}
            onSubmit={handleCreateKPI}
            onUpdateAchievement={handleUpdateKPIAchievement}
          />
        )}
      </div>
    </MainLayout>
  )
}

// Goals Section Component
interface GoalsSectionProps {
  goals: PerformanceGoalDto[]
  employees: Employee[]
  onCreateClick: () => void
  showForm: boolean
  onSubmit: (e: React.FormEvent<HTMLFormElement>) => Promise<void>
  onUpdateProgress: (goalId: string, progress: number) => Promise<void>
  onComplete: (goalId: string) => Promise<void>
}

function GoalsSection({
  goals,
  employees,
  onCreateClick,
  showForm,
  onSubmit,
  onUpdateProgress,
  onComplete,
}: GoalsSectionProps) {
  return (
    <div className="space-y-4">
      <div className="flex justify-end">
        <Button onClick={onCreateClick} className="bg-blue-600 hover:bg-blue-700">
          <Plus className="w-4 h-4 mr-2" />
          New Goal
        </Button>
      </div>

      {showForm && (
        <Card className="bg-blue-50">
          <form onSubmit={onSubmit} className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-medium text-neutral-700 mb-1">Employee</label>
                <select
                  name="employeeId"
                  required
                  className="w-full px-3 py-2 border border-neutral-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                >
                  <option value="">Select Employee</option>
                  {employees.map((emp) => (
                    <option key={emp.id} value={emp.id}>
                      {emp.firstName} {emp.lastName} ({emp.employeeCode})
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-xs font-medium text-neutral-700 mb-1">Goal Title</label>
                <input
                  type="text"
                  name="title"
                  placeholder="Goal Title"
                  required
                  className="w-full px-3 py-2 border border-neutral-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                />
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-medium text-neutral-700 mb-1">Start Date</label>
                <input
                  type="date"
                  name="startDate"
                  required
                  className="w-full px-3 py-2 border border-neutral-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-neutral-700 mb-1">End Date</label>
                <input
                  type="date"
                  name="endDate"
                  required
                  className="w-full px-3 py-2 border border-neutral-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-medium text-neutral-700 mb-1">Description</label>
              <textarea
                name="description"
                placeholder="Description"
                required
                className="w-full px-3 py-2 border border-neutral-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none bg-white"
                rows={3}
              />
            </div>

            <div className="flex gap-2 justify-end">
              <Button type="submit" className="bg-green-600 hover:bg-green-700">
                Create
              </Button>
            </div>
          </form>
        </Card>
      )}

      {goals.length === 0 ? (
        <Card>
          <div className="text-center py-12">
            <Target className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
            <p className="text-neutral-500">No goals yet. Create your first goal!</p>
          </div>
        </Card>
      ) : (
        <div className="grid gap-4">
          {goals.map((goal) => (
            <Card key={goal.id} className="border-l-4 border-l-blue-600">
              <div className="flex justify-between items-start mb-3">
                <div className="flex-1">
                  <h3 className="font-semibold text-neutral-900">{goal.title}</h3>
                  <p className="text-xs text-blue-700 font-medium mt-0.5">Employee: {goal.employeeName || 'Assigned'}</p>
                  <p className="text-sm text-neutral-600 mt-1">{goal.description}</p>
                </div>
                <Badge
                  label={goal.status}
                  variant={goal.status === 'Active' ? 'info' : 'success'}
                />
              </div>

              <div className="space-y-3 mt-4">
                <div>
                  <div className="flex justify-between items-center mb-1">
                    <span className="text-sm font-medium text-neutral-700">Progress</span>
                    <span className="text-sm text-neutral-600">{goal.progressPercentage}%</span>
                  </div>
                  <div className="w-full bg-neutral-200 rounded-full h-2">
                    <div
                      className="bg-blue-600 h-2 rounded-full transition-all"
                      style={{ width: `${goal.progressPercentage}%` }}
                    />
                  </div>
                </div>

                <div className="flex gap-2 text-sm text-neutral-600">
                  <span>Start: {formatDateIST(goal.startDateUtc)}</span>
                  <span>•</span>
                  <span>End: {formatDateIST(goal.endDateUtc)}</span>
                </div>

                <div className="flex gap-2 pt-2">
                  <input
                    type="range"
                    min="0"
                    max="100"
                    value={goal.progressPercentage}
                    onChange={(e) => onUpdateProgress(goal.id, parseInt(e.target.value))}
                    className="flex-1"
                  />
                  {goal.status === 'Active' && (
                    <Button
                      onClick={() => onComplete(goal.id)}
                      className="bg-green-600 hover:bg-green-700 text-sm"
                    >
                      Complete
                    </Button>
                  )}
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}

// Reviews Section Component
interface ReviewsSectionProps {
  reviews: PerformanceReviewDto[]
  employees: Employee[]
  onCreateClick: () => void
  showForm: boolean
  onSubmit: (e: React.FormEvent<HTMLFormElement>) => Promise<void>
}

function ReviewsSection({ reviews, employees, onCreateClick, showForm, onSubmit }: ReviewsSectionProps) {
  return (
    <div className="space-y-4">
      <div className="flex justify-end">
        <Button onClick={onCreateClick} className="bg-blue-600 hover:bg-blue-700">
          <Plus className="w-4 h-4 mr-2" />
          Submit Review
        </Button>
      </div>

      {showForm && (
        <Card className="bg-blue-50">
          <form onSubmit={onSubmit} className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <label className="block text-xs font-medium text-neutral-700 mb-1">Employee</label>
                <select
                  name="employeeId"
                  required
                  className="w-full px-3 py-2 border border-neutral-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                >
                  <option value="">Select Employee</option>
                  {employees.map((emp) => (
                    <option key={emp.id} value={emp.id}>
                      {emp.firstName} {emp.lastName} ({emp.employeeCode})
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-xs font-medium text-neutral-700 mb-1">Year</label>
                <input
                  type="number"
                  name="year"
                  placeholder="Year"
                  min="2020"
                  max={new Date().getFullYear()}
                  defaultValue={new Date().getFullYear()}
                  required
                  className="w-full px-3 py-2 border border-neutral-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-neutral-700 mb-1">Quarter</label>
                <select
                  name="quarter"
                  required
                  className="w-full px-3 py-2 border border-neutral-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                >
                  <option value="">Select Quarter</option>
                  <option value="1">Q1</option>
                  <option value="2">Q2</option>
                  <option value="3">Q3</option>
                  <option value="4">Q4</option>
                </select>
              </div>
            </div>

            <div>
              <label className="block text-xs font-medium text-neutral-700 mb-1">Rating</label>
              <select
                name="rating"
                required
                className="w-full px-3 py-2 border border-neutral-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
              >
                <option value="">Select Rating</option>
                <option value="1">1 - Needs Improvement</option>
                <option value="2">2 - Below Average</option>
                <option value="3">3 - Average</option>
                <option value="4">4 - Good</option>
                <option value="5">5 - Excellent</option>
              </select>
            </div>

            <div>
              <label className="block text-xs font-medium text-neutral-700 mb-1">Comments</label>
              <textarea
                name="comments"
                placeholder="Comments (optional)"
                className="w-full px-3 py-2 border border-neutral-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none bg-white"
                rows={3}
              />
            </div>

            <div className="flex gap-2 justify-end">
              <Button type="submit" className="bg-green-600 hover:bg-green-700">
                Submit
              </Button>
            </div>
          </form>
        </Card>
      )}

      {reviews.length === 0 ? (
        <Card>
          <div className="text-center py-12">
            <Award className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
            <p className="text-neutral-500">No reviews yet. Submit your first review!</p>
          </div>
        </Card>
      ) : (
        <div className="grid gap-4">
          {reviews.map((review) => (
            <Card key={review.id} className="border-l-4 border-l-amber-600">
              <div className="flex justify-between items-start mb-3">
                <div className="flex-1">
                  <h3 className="font-semibold text-neutral-900">
                    {review.employeeName ? `${review.employeeName} — ` : ''}{review.year} Q{review.quarter}
                  </h3>
                  <p className="text-sm text-neutral-600 mt-1">{review.comments || 'No comments'}</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge
                    label={review.status}
                    variant={
                      review.status === 'Approved'
                        ? 'success'
                        : review.status === 'Submitted'
                          ? 'info'
                          : 'warning'
                    }
                  />
                  <div className="flex gap-1 ml-2">
                    {Array.from({ length: 5 }).map((_, i) => (
                      <div
                        key={i}
                        className={`w-4 h-4 rounded-full ${
                          i < review.rating ? 'bg-amber-400' : 'bg-neutral-200'
                        }`}
                      />
                    ))}
                  </div>
                </div>
              </div>

              <div className="flex gap-2 text-sm text-neutral-600 mt-3">
                <span>Rating: {review.rating}/5</span>
                {review.reviewedByName && <span>• Reviewed by: {review.reviewedByName}</span>}
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}

// KPIs Section Component
interface KPIsSectionProps {
  kpis: KPIDto[]
  employees: Employee[]
  onCreateClick: () => void
  showForm: boolean
  onSubmit: (e: React.FormEvent<HTMLFormElement>) => Promise<void>
  onUpdateAchievement: (kpiId: string, achieved: number) => Promise<void>
}

function KPIsSection({
  kpis,
  employees,
  onCreateClick,
  showForm,
  onSubmit,
  onUpdateAchievement,
}: KPIsSectionProps) {
  return (
    <div className="space-y-4">
      <div className="flex justify-end">
        <Button onClick={onCreateClick} className="bg-blue-600 hover:bg-blue-700">
          <Plus className="w-4 h-4 mr-2" />
          New KPI
        </Button>
      </div>

      {showForm && (
        <Card className="bg-blue-50">
          <form onSubmit={onSubmit} className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <label className="block text-xs font-medium text-neutral-700 mb-1">Employee</label>
                <select
                  name="employeeId"
                  required
                  className="w-full px-3 py-2 border border-neutral-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                >
                  <option value="">Select Employee</option>
                  {employees.map((emp) => (
                    <option key={emp.id} value={emp.id}>
                      {emp.firstName} {emp.lastName} ({emp.employeeCode})
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-xs font-medium text-neutral-700 mb-1">KPI Name</label>
                <input
                  type="text"
                  name="name"
                  placeholder="KPI Name"
                  required
                  className="w-full px-3 py-2 border border-neutral-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-neutral-700 mb-1">Year</label>
                <input
                  type="number"
                  name="year"
                  placeholder="Year"
                  min="2020"
                  max={new Date().getFullYear() + 1}
                  defaultValue={new Date().getFullYear()}
                  required
                  className="w-full px-3 py-2 border border-neutral-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-medium text-neutral-700 mb-1">Target Value</label>
              <input
                type="number"
                name="target"
                placeholder="Target"
                step="0.01"
                min="0"
                required
                className="w-full px-3 py-2 border border-neutral-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
              />
            </div>

            <div className="flex gap-2 justify-end">
              <Button type="submit" className="bg-green-600 hover:bg-green-700">
                Create
              </Button>
            </div>
          </form>
        </Card>
      )}

      {kpis.length === 0 ? (
        <Card>
          <div className="text-center py-12">
            <TrendingUp className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
            <p className="text-neutral-500">No KPIs yet. Create your first KPI!</p>
          </div>
        </Card>
      ) : (
        <div className="grid gap-4">
          {kpis.map((kpi) => (
            <Card key={kpi.id} className="border-l-4 border-l-green-600">
              <div className="flex justify-between items-start mb-3">
                <div className="flex-1">
                  <h3 className="font-semibold text-neutral-900">{kpi.name}</h3>
                  <p className="text-xs text-green-700 font-medium mt-0.5">Employee: {kpi.employeeName || 'Assigned'}</p>
                </div>
                <Badge
                  label={`${kpi.achievementPercentage}% Achieved`}
                  variant={kpi.achievementPercentage >= 100 ? 'success' : 'warning'}
                />
              </div>

              <div className="space-y-3 mt-4">
                <div className="flex justify-between text-sm text-neutral-600">
                  <span>Target: {kpi.target}</span>
                  <span>Achieved: {kpi.achieved}</span>
                </div>

                <div className="w-full bg-neutral-200 rounded-full h-2">
                  <div
                    className="bg-green-600 h-2 rounded-full transition-all"
                    style={{ width: `${Math.min(kpi.achievementPercentage, 100)}%` }}
                  />
                </div>

                <div className="flex gap-2 items-center pt-2">
                  <input
                    type="number"
                    placeholder="Update achieved"
                    step="0.01"
                    min="0"
                    onKeyDown={(e) => {
                      if (e.key === 'Enter') {
                        const val = parseFloat((e.target as HTMLInputElement).value)
                        if (!isNaN(val)) {
                          onUpdateAchievement(kpi.id, val)
                          ;(e.target as HTMLInputElement).value = ''
                        }
                      }
                    }}
                    className="w-36 px-2 py-1 border border-neutral-300 rounded text-sm bg-white"
                  />
                  <span className="text-xs text-neutral-400">Press Enter to update</span>
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}
