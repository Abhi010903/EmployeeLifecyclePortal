import { apiClient } from './client'
import { JobPostingDto, CandidateDto, InterviewDto, JobOfferDto } from '@/types'

export const recruitmentApi = {
  // Job Postings
  getJobPostings: (status?: 'Open' | 'Closed') => {
    const url = status ? `/recruitment/job-postings?status=${status}` : '/recruitment/job-postings'
    return apiClient.get<JobPostingDto[]>(url)
  },

  getJobPostingById: (id: string) =>
    apiClient.get<JobPostingDto>(`/recruitment/job-postings/${id}`),

  createJobPosting: (data: {
    title: string
    description: string
    departmentId: string
  }) =>
    apiClient.post('/recruitment/job-postings', data),

  closeJobPosting: (id: string) =>
    apiClient.put(`/recruitment/job-postings/${id}/close`, {}),

  // Candidates
  getCandidates: (jobPostingId?: string) => {
    const url = jobPostingId 
      ? `/recruitment/candidates?jobPostingId=${jobPostingId}` 
      : '/recruitment/candidates'
    return apiClient.get<CandidateDto[]>(url)
  },

  getCandidateById: (id: string) =>
    apiClient.get<CandidateDto>(`/recruitment/candidates/${id}`),

  createCandidate: (data: {
    firstName: string
    lastName: string
    email: string
    phoneNumber?: string
    jobPostingId: string
  }) =>
    apiClient.post('/recruitment/candidates', data),

  updateCandidateStatus: (id: string, status: string) =>
    apiClient.put(`/recruitment/candidates/${id}/status`, { status }),

  // Interviews
  getInterviews: (candidateId?: string) => {
    const url = candidateId 
      ? `/recruitment/interviews?candidateId=${candidateId}` 
      : '/recruitment/interviews'
    return apiClient.get<InterviewDto[]>(url)
  },

  scheduleInterview: (data: {
    candidateId: string
    scheduledDateUtc: string
    interviewerName: string
  }) =>
    apiClient.post('/recruitment/interviews', data),

  completeInterview: (id: string, data: {
    rating: number
    feedback: string
  }) =>
    apiClient.put(`/recruitment/interviews/${id}/complete`, data),

  // Job Offers
  getJobOffers: (candidateId?: string) => {
    const url = candidateId 
      ? `/recruitment/offers?candidateId=${candidateId}` 
      : '/recruitment/offers'
    return apiClient.get<JobOfferDto[]>(url)
  },

  createJobOffer: (data: {
    candidateId: string
    offeredSalary: number
    expiryDateUtc: string
  }) =>
    apiClient.post('/recruitment/offers', data),

  acceptJobOffer: (id: string) =>
    apiClient.put(`/recruitment/offers/${id}/accept`, {}),

  rejectJobOffer: (id: string) =>
    apiClient.put(`/recruitment/offers/${id}/reject`, {}),
}
