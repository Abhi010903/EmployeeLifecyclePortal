import { apiClient } from './client'
import {
  PerformanceGoalDto,
  PerformanceReviewDto,
  KPIDto,
} from '@/types'

// Performance Goals API
export const performanceApi = {
  goals: {
    create: (data: {
      employeeId: string
      title: string
      description: string
      startDate: string
      endDate: string
    }) =>
      apiClient.post<PerformanceGoalDto>('/performance/goals', data),

    getAll: () =>
      apiClient.get<PerformanceGoalDto[]>('/performance/goals'),

    getByEmployee: (employeeId: string) =>
      apiClient.get<PerformanceGoalDto[]>(
        `/performance/goals/employee/${employeeId}`
      ),

    updateProgress: (goalId: string, progressPercentage: number) =>
      apiClient.put<PerformanceGoalDto>(
        `/performance/goals/${goalId}/progress`,
        { progressPercentage }
      ),

    complete: (goalId: string) =>
      apiClient.put<PerformanceGoalDto>(
        `/performance/goals/${goalId}/complete`,
        {}
      ),
  },

  reviews: {
    submit: (data: {
      employeeId: string
      year: number
      quarter: number
      rating: number
      comments?: string
    }) =>
      apiClient.post<PerformanceReviewDto>('/performance/reviews', data),

    getAll: () =>
      apiClient.get<PerformanceReviewDto[]>('/performance/reviews'),

    getByEmployee: (employeeId: string) =>
      apiClient.get<PerformanceReviewDto[]>(
        `/performance/reviews/employee/${employeeId}`
      ),

    approve: (reviewId: string, reviewedByUserId: string) =>
      apiClient.put<PerformanceReviewDto>(
        `/performance/reviews/${reviewId}/approve`,
        { reviewedByUserId }
      ),
  },

  kpis: {
    create: (data: {
      employeeId: string
      name: string
      target: number
      year: number
    }) =>
      apiClient.post<KPIDto>('/performance/kpis', data),

    getAll: () =>
      apiClient.get<KPIDto[]>('/performance/kpis'),

    getByEmployee: (employeeId: string) =>
      apiClient.get<KPIDto[]>(
        `/performance/kpis/employee/${employeeId}`
      ),

    updateAchievement: (kpiId: string, achieved: number) =>
      apiClient.put<KPIDto>(
        `/performance/kpis/${kpiId}/achievement`,
        { achieved }
      ),
  },
}
