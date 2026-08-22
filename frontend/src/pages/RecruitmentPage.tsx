import { useEffect, useState, useCallback } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import Modal from '@/components/Common/Modal'
import { Plus, Briefcase, Users, Calendar, AlertCircle, CheckCircle, XCircle } from 'lucide-react'
import { recruitmentApi } from '@/api/recruitment'
import { departmentsApi } from '@/api/departments'
import { JobPostingDto, CandidateDto, InterviewDto, JobOfferDto, Department } from '@/types'
import { formatDateIST, formatCurrencyINR } from '@/utils/format'
import toast from 'react-hot-toast'

export default function RecruitmentPage() {
  const [jobPostings, setJobPostings] = useState<JobPostingDto[]>([])
  const [candidates, setCandidates] = useState<CandidateDto[]>([])
  const [interviews, setInterviews] = useState<InterviewDto[]>([])
  const [offers, setOffers] = useState<JobOfferDto[]>([])
  const [departments, setDepartments] = useState<Department[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  
  // Post Job Modal State
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [jobTitle, setJobTitle] = useState('')
  const [jobDescription, setJobDescription] = useState('')
  const [jobDepartmentId, setJobDepartmentId] = useState('')
  const [isSubmittingJob, setIsSubmittingJob] = useState(false)

  // Candidate Modal State
  const [isCandidateModalOpen, setIsCandidateModalOpen] = useState(false)
  const [candFirstName, setCandFirstName] = useState('')
  const [candLastName, setCandLastName] = useState('')
  const [candEmail, setCandEmail] = useState('')
  const [candPhone, setCandPhone] = useState('')
  const [candJobPostingId, setCandJobPostingId] = useState('')
  const [isSubmittingCandidate, setIsSubmittingCandidate] = useState(false)

  const [activeTab, setActiveTab] = useState<'postings' | 'candidates' | 'interviews' | 'offers'>('postings')

  const fetchData = useCallback(async () => {
    try {
      setLoading(true)
      const [postingsRes, candidatesRes, interviewsRes, offersRes, deptsRes] = await Promise.all([
        recruitmentApi.getJobPostings(),
        recruitmentApi.getCandidates(),
        recruitmentApi.getInterviews(),
        recruitmentApi.getJobOffers(),
        departmentsApi.getAllSimple(),
      ])

      setJobPostings(Array.isArray(postingsRes.data) ? postingsRes.data : [])
      setCandidates(Array.isArray(candidatesRes.data) ? candidatesRes.data : [])
      setInterviews(Array.isArray(interviewsRes.data) ? interviewsRes.data : [])
      setOffers(Array.isArray(offersRes.data) ? offersRes.data : [])
      setDepartments(Array.isArray(deptsRes) ? deptsRes : [])
      setError(null)
    } catch (err) {
      setError('Failed to load recruitment data')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchData()
  }, [fetchData])

  const handleCreateJob = async () => {
    if (!jobTitle.trim() || !jobDescription.trim() || !jobDepartmentId) {
      toast.error('Please fill in all required fields.')
      return
    }

    try {
      setIsSubmittingJob(true)
      await recruitmentApi.createJobPosting({
        title: jobTitle.trim(),
        description: jobDescription.trim(),
        departmentId: jobDepartmentId,
      })
      toast.success('Job posting created successfully!')
      setIsModalOpen(false)
      setJobTitle('')
      setJobDescription('')
      setJobDepartmentId('')
      fetchData()
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Failed to create job posting.')
    } finally {
      setIsSubmittingJob(false)
    }
  }

  const handleCloseJob = async (id: string) => {
    try {
      await recruitmentApi.closeJobPosting(id)
      toast.success('Job posting closed.')
      fetchData()
    } catch (err: any) {
      toast.error('Failed to close job posting.')
    }
  }

  const handleCreateCandidate = async () => {
    if (!candFirstName.trim() || !candLastName.trim() || !candEmail.trim() || !candJobPostingId) {
      toast.error('Please fill in all required fields.')
      return
    }

    try {
      setIsSubmittingCandidate(true)
      await recruitmentApi.createCandidate({
        firstName: candFirstName.trim(),
        lastName: candLastName.trim(),
        email: candEmail.trim(),
        phoneNumber: candPhone.trim() || undefined,
        jobPostingId: candJobPostingId,
      })
      toast.success('Candidate registered successfully!')
      setIsCandidateModalOpen(false)
      setCandFirstName('')
      setCandLastName('')
      setCandEmail('')
      setCandPhone('')
      setCandJobPostingId('')
      fetchData()
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Failed to add candidate.')
    } finally {
      setIsSubmittingCandidate(false)
    }
  }

  const handleAcceptOffer = async (id: string) => {
    try {
      await recruitmentApi.acceptJobOffer(id)
      toast.success('Job offer accepted!')
      fetchData()
    } catch (err) {
      toast.error('Failed to accept offer.')
    }
  }

  const handleRejectOffer = async (id: string) => {
    try {
      await recruitmentApi.rejectJobOffer(id)
      toast.success('Job offer rejected.')
      fetchData()
    } catch (err) {
      toast.error('Failed to reject offer.')
    }
  }

  const formatDate = (dateString: string) => {
    return formatDateIST(dateString)
  }

  const statusColors: Record<string, 'success' | 'warning' | 'danger' | 'info'> = {
    Open: 'success',
    Closed: 'danger',
    Applied: 'info',
    Interview: 'warning',
    Rejected: 'danger',
    Hired: 'success',
    Scheduled: 'warning',
    Completed: 'success',
    Cancelled: 'danger',
    Pending: 'warning',
    Accepted: 'success',
  }

  const openPostings = jobPostings.filter((jp) => jp.status === 'Open').length
  const totalCandidates = candidates.length
  const scheduledInterviews = interviews.filter((i) => i.status === 'Scheduled').length
  const pendingOffers = offers.filter((o) => o.status === 'Pending').length

  return (
    <MainLayout>
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold text-neutral-900">Recruitment</h1>
            <p className="text-neutral-600 mt-1">Manage job postings, candidates, and interviews</p>
          </div>
          <div className="flex gap-3">
            <Button variant="outline" onClick={() => setIsCandidateModalOpen(true)}>
              <Plus className="w-4 h-4 mr-2" />
              Add Candidate
            </Button>
            <Button onClick={() => setIsModalOpen(true)}>
              <Plus className="w-4 h-4 mr-2" />
              Post Job
            </Button>
          </div>
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 flex items-start gap-3">
            <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
            <p className="text-sm text-red-900">{error}</p>
          </div>
        )}

        {/* Summary Stats */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
          <Card className="bg-blue-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-blue-600">Open Positions</p>
                <p className="text-2xl font-bold text-blue-900 mt-1">{openPostings}</p>
              </div>
              <Briefcase className="w-8 h-8 text-blue-600 opacity-80" />
            </div>
          </Card>

          <Card className="bg-green-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-green-600">Total Candidates</p>
                <p className="text-2xl font-bold text-green-900 mt-1">{totalCandidates}</p>
              </div>
              <Users className="w-8 h-8 text-green-600 opacity-80" />
            </div>
          </Card>

          <Card className="bg-amber-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-amber-600">Scheduled Interviews</p>
                <p className="text-2xl font-bold text-amber-900 mt-1">{scheduledInterviews}</p>
              </div>
              <Calendar className="w-8 h-8 text-amber-600 opacity-80" />
            </div>
          </Card>

          <Card className="bg-purple-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-purple-600">Pending Offers</p>
                <p className="text-2xl font-bold text-purple-900 mt-1">{pendingOffers}</p>
              </div>
              <Briefcase className="w-8 h-8 text-purple-600 opacity-80" />
            </div>
          </Card>
        </div>

        {/* Tabs */}
        <div className="flex border-b border-neutral-200">
          <button
            onClick={() => setActiveTab('postings')}
            className={`px-6 py-3 font-medium text-sm border-b-2 transition ${
              activeTab === 'postings'
                ? 'border-primary-600 text-primary-600'
                : 'border-transparent text-neutral-600 hover:text-neutral-900'
            }`}
          >
            Job Postings ({jobPostings.length})
          </button>
          <button
            onClick={() => setActiveTab('candidates')}
            className={`px-6 py-3 font-medium text-sm border-b-2 transition ${
              activeTab === 'candidates'
                ? 'border-primary-600 text-primary-600'
                : 'border-transparent text-neutral-600 hover:text-neutral-900'
            }`}
          >
            Candidates ({candidates.length})
          </button>
          <button
            onClick={() => setActiveTab('interviews')}
            className={`px-6 py-3 font-medium text-sm border-b-2 transition ${
              activeTab === 'interviews'
                ? 'border-primary-600 text-primary-600'
                : 'border-transparent text-neutral-600 hover:text-neutral-900'
            }`}
          >
            Interviews ({interviews.length})
          </button>
          <button
            onClick={() => setActiveTab('offers')}
            className={`px-6 py-3 font-medium text-sm border-b-2 transition ${
              activeTab === 'offers'
                ? 'border-primary-600 text-primary-600'
                : 'border-transparent text-neutral-600 hover:text-neutral-900'
            }`}
          >
            Offers ({offers.length})
          </button>
        </div>

        {/* Tab Content */}
        {loading ? (
          <div className="flex items-center justify-center py-12">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
          </div>
        ) : activeTab === 'postings' ? (
          <Card>
            {jobPostings.length === 0 ? (
              <div className="text-center py-8">
                <Briefcase className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
                <p className="text-neutral-500">No job postings found</p>
              </div>
            ) : (
              <div className="space-y-4">
                {jobPostings.map((job) => (
                  <div key={job.id} className="border border-neutral-200 rounded-lg p-4 hover:bg-neutral-50 transition">
                    <div className="flex justify-between items-start">
                      <div className="flex-1">
                        <h3 className="font-semibold text-neutral-900">{job.title}</h3>
                        <p className="text-sm text-neutral-600 mt-1">{job.description}</p>
                        <div className="flex gap-4 mt-3 text-xs text-neutral-500">
                          <span>Department: {job.departmentName || 'General'}</span>
                          <span>Posted: {formatDate(job.postedDateUtc)}</span>
                        </div>
                      </div>
                      <div className="flex items-center gap-3">
                        <Badge label={job.status} variant={statusColors[job.status] as any} />
                        {job.status === 'Open' && (
                          <Button variant="outline" size="sm" onClick={() => handleCloseJob(job.id)}>
                            Close Job
                          </Button>
                        )}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </Card>
        ) : activeTab === 'candidates' ? (
          <Card>
            {candidates.length === 0 ? (
              <div className="text-center py-8">
                <Users className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
                <p className="text-neutral-500">No candidates found</p>
              </div>
            ) : (
              <div className="space-y-4">
                {candidates.map((cand) => (
                  <div key={cand.id} className="border border-neutral-200 rounded-lg p-4 hover:bg-neutral-50 transition">
                    <div className="flex justify-between items-start">
                      <div className="flex-1">
                        <h3 className="font-semibold text-neutral-900">{cand.fullName || `${cand.firstName} ${cand.lastName}`}</h3>
                        <div className="grid grid-cols-3 gap-4 mt-2 text-sm text-neutral-600">
                          <span>Email: {cand.email}</span>
                          <span>Phone: {cand.phoneNumber || 'N/A'}</span>
                          <span>Applied for: {cand.jobPostingTitle || 'General'}</span>
                        </div>
                      </div>
                      <Badge label={cand.status} variant={statusColors[cand.status] as any} />
                    </div>
                  </div>
                ))}
              </div>
            )}
          </Card>
        ) : activeTab === 'interviews' ? (
          <Card>
            {interviews.length === 0 ? (
              <div className="text-center py-8">
                <Calendar className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
                <p className="text-neutral-500">No interviews scheduled</p>
              </div>
            ) : (
              <div className="space-y-4">
                {interviews.map((interview) => (
                  <div key={interview.id} className="border border-neutral-200 rounded-lg p-4 hover:bg-neutral-50 transition">
                    <div className="flex justify-between items-start">
                      <div className="flex-1">
                        <h3 className="font-semibold text-neutral-900">{interview.candidateName}</h3>
                        <div className="grid grid-cols-2 gap-4 mt-2 text-sm text-neutral-600">
                          <span>Scheduled: {formatDate(interview.scheduledDateUtc)}</span>
                          <span>Interviewer: {interview.interviewerName}</span>
                        </div>
                        {interview.feedback && (
                          <p className="mt-2 text-sm text-neutral-700 bg-neutral-100 p-2 rounded">
                            Feedback: {interview.feedback} (Rating: {interview.rating}/5)
                          </p>
                        )}
                      </div>
                      <Badge label={interview.status} variant={statusColors[interview.status] as any} />
                    </div>
                  </div>
                ))}
              </div>
            )}
          </Card>
        ) : (
          <Card>
            {offers.length === 0 ? (
              <div className="text-center py-8">
                <Briefcase className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
                <p className="text-neutral-500">No job offers</p>
              </div>
            ) : (
              <div className="space-y-4">
                {offers.map((offer) => (
                  <div key={offer.id} className="border border-neutral-200 rounded-lg p-4 hover:bg-neutral-50 transition">
                    <div className="flex justify-between items-start">
                      <div className="flex-1">
                        <h3 className="font-semibold text-neutral-900">{offer.candidateName}</h3>
                        <div className="grid grid-cols-2 gap-4 mt-2 text-sm text-neutral-600">
                          <span>Offered: {formatCurrencyINR(offer.offeredSalary)}</span>
                          <span>Expires: {offer.expiryDateUtc ? formatDateIST(offer.expiryDateUtc) : 'N/A'}</span>
                        </div>
                      </div>
                      <div className="flex items-center gap-3">
                        <Badge label={offer.status} variant={statusColors[offer.status] as any} />
                        {offer.status === 'Pending' && (
                          <div className="flex gap-2">
                            <Button size="sm" variant="outline" onClick={() => handleAcceptOffer(offer.id)}>
                              <CheckCircle className="w-4 h-4 mr-1 text-green-600" />
                              Accept
                            </Button>
                            <Button size="sm" variant="outline" onClick={() => handleRejectOffer(offer.id)}>
                              <XCircle className="w-4 h-4 mr-1 text-red-600" />
                              Reject
                            </Button>
                          </div>
                        )}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </Card>
        )}
      </div>

      {/* Post Job Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title="Post New Job"
        footer={
          <>
            <Button variant="outline" onClick={() => setIsModalOpen(false)} disabled={isSubmittingJob}>
              Cancel
            </Button>
            <Button onClick={handleCreateJob} disabled={isSubmittingJob}>
              {isSubmittingJob ? 'Posting...' : 'Post Job'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">
              Job Title *
            </label>
            <input
              type="text"
              placeholder="e.g., Senior Full-Stack Engineer"
              value={jobTitle}
              onChange={(e) => setJobTitle(e.target.value)}
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">
              Description *
            </label>
            <textarea
              rows={4}
              placeholder="Job description and requirements..."
              value={jobDescription}
              onChange={(e) => setJobDescription(e.target.value)}
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">
              Department *
            </label>
            <select
              value={jobDepartmentId}
              onChange={(e) => setJobDepartmentId(e.target.value)}
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              <option value="">Select Department</option>
              {departments.map((dept) => (
                <option key={dept.id} value={dept.id}>
                  {dept.name}
                </option>
              ))}
            </select>
          </div>
        </div>
      </Modal>

      {/* Add Candidate Modal */}
      <Modal
        isOpen={isCandidateModalOpen}
        onClose={() => setIsCandidateModalOpen(false)}
        title="Add Candidate"
        footer={
          <>
            <Button variant="outline" onClick={() => setIsCandidateModalOpen(false)} disabled={isSubmittingCandidate}>
              Cancel
            </Button>
            <Button onClick={handleCreateCandidate} disabled={isSubmittingCandidate}>
              {isSubmittingCandidate ? 'Saving...' : 'Add Candidate'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">First Name *</label>
              <input
                type="text"
                placeholder="John"
                value={candFirstName}
                onChange={(e) => setCandFirstName(e.target.value)}
                className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">Last Name *</label>
              <input
                type="text"
                placeholder="Doe"
                value={candLastName}
                onChange={(e) => setCandLastName(e.target.value)}
                className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Email *</label>
            <input
              type="email"
              placeholder="candidate@example.com"
              value={candEmail}
              onChange={(e) => setCandEmail(e.target.value)}
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Phone Number</label>
            <input
              type="tel"
              placeholder="+91 98765 43210"
              value={candPhone}
              onChange={(e) => setCandPhone(e.target.value)}
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Job Posting *</label>
            <select
              value={candJobPostingId}
              onChange={(e) => setCandJobPostingId(e.target.value)}
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              <option value="">Select Job Posting</option>
              {jobPostings.map((job) => (
                <option key={job.id} value={job.id}>
                  {job.title} ({job.departmentName || 'General'})
                </option>
              ))}
            </select>
          </div>
        </div>
      </Modal>
    </MainLayout>
  )
}
