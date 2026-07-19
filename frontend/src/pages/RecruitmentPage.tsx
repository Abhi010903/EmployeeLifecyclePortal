import { useEffect, useState, useCallback } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import Modal from '@/components/Common/Modal'
import { Plus, Briefcase, Users, Calendar, AlertCircle } from 'lucide-react'
import { recruitmentApi } from '@/api/recruitment'
import { JobPostingDto, CandidateDto, InterviewDto, JobOfferDto } from '@/types'

export default function RecruitmentPage() {
  const [jobPostings, setJobPostings] = useState<JobPostingDto[]>([])
  const [candidates, setCandidates] = useState<CandidateDto[]>([])
  const [interviews, setInterviews] = useState<InterviewDto[]>([])
  const [offers, setOffers] = useState<JobOfferDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [activeTab, setActiveTab] = useState<'postings' | 'candidates' | 'interviews' | 'offers'>('postings')

  const fetchData = useCallback(async () => {
    try {
      setLoading(true)
      const [postingsRes, candidatesRes, interviewsRes, offersRes] = await Promise.all([
        recruitmentApi.getJobPostings(),
        recruitmentApi.getCandidates(),
        recruitmentApi.getInterviews(),
        recruitmentApi.getJobOffers(),
      ])

      setJobPostings(postingsRes.data || [])
      setCandidates(candidatesRes.data || [])
      setInterviews(interviewsRes.data || [])
      setOffers(offersRes.data || [])
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

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    })
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
          <Button onClick={() => setIsModalOpen(true)}>
            <Plus className="w-4 h-4 mr-2" />
            Post Job
          </Button>
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
                <p className="text-neutral-600 text-sm">Open Positions</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">{openPostings}</p>
              </div>
              <Briefcase className="w-8 h-8 text-blue-600" />
            </div>
          </Card>

          <Card className="bg-green-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Total Candidates</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">{totalCandidates}</p>
              </div>
              <Users className="w-8 h-8 text-green-600" />
            </div>
          </Card>

          <Card className="bg-yellow-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Scheduled Interviews</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">{scheduledInterviews}</p>
              </div>
              <Calendar className="w-8 h-8 text-yellow-600" />
            </div>
          </Card>

          <Card className="bg-purple-50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-neutral-600 text-sm">Pending Offers</p>
                <p className="text-3xl font-bold text-neutral-900 mt-2">{pendingOffers}</p>
              </div>
              <Briefcase className="w-8 h-8 text-purple-600" />
            </div>
          </Card>
        </div>

        {/* Tabs */}
        <div className="flex gap-2 border-b border-neutral-200">
          {(['postings', 'candidates', 'interviews', 'offers'] as const).map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              className={`px-4 py-3 font-medium text-sm border-b-2 transition ${
                activeTab === tab
                  ? 'border-primary-600 text-primary-600'
                  : 'border-transparent text-neutral-600 hover:text-neutral-900'
              }`}
            >
              {tab === 'postings' && 'Job Postings'}
              {tab === 'candidates' && 'Candidates'}
              {tab === 'interviews' && 'Interviews'}
              {tab === 'offers' && 'Offers'}
            </button>
          ))}
        </div>

        {/* Content */}
        {loading ? (
          <Card>
            <div className="text-center py-12">
              <Briefcase className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
              <p className="text-neutral-500">Loading recruitment data...</p>
            </div>
          </Card>
        ) : activeTab === 'postings' ? (
          <Card>
            <h2 className="text-lg font-semibold text-neutral-900 mb-4">Job Postings</h2>
            {jobPostings.length === 0 ? (
              <div className="text-center py-12">
                <Briefcase className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
                <p className="text-neutral-500">No job postings yet</p>
              </div>
            ) : (
              <div className="space-y-4">
                {jobPostings.map((posting) => (
                  <div key={posting.id} className="border border-neutral-200 rounded-lg p-4 hover:bg-neutral-50 transition">
                    <div className="flex justify-between items-start">
                      <div className="flex-1">
                        <h3 className="font-semibold text-neutral-900">{posting.title}</h3>
                        <p className="text-sm text-neutral-600 mt-1">{posting.description}</p>
                        <div className="flex gap-4 mt-3 text-xs text-neutral-500">
                          <span>Posted: {formatDate(posting.postedDateUtc)}</span>
                          <span>Department: {posting.departmentName || 'Unknown'}</span>
                        </div>
                      </div>
                      <Badge label={posting.status} variant={statusColors[posting.status] as any} />
                    </div>
                  </div>
                ))}
              </div>
            )}
          </Card>
        ) : activeTab === 'candidates' ? (
          <Card>
            <h2 className="text-lg font-semibold text-neutral-900 mb-4">Candidates</h2>
            {candidates.length === 0 ? (
              <div className="text-center py-12">
                <Users className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
                <p className="text-neutral-500">No candidates yet</p>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-neutral-200 bg-neutral-50">
                      <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">Name</th>
                      <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">Email</th>
                      <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">Position</th>
                      <th className="px-6 py-3 text-left text-sm font-semibold text-neutral-900">Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {candidates.map((candidate) => (
                      <tr key={candidate.id} className="border-b border-neutral-200 hover:bg-neutral-50">
                        <td className="px-6 py-4 text-sm text-neutral-700">
                          {candidate.firstName} {candidate.lastName}
                        </td>
                        <td className="px-6 py-4 text-sm text-neutral-700">{candidate.email}</td>
                        <td className="px-6 py-4 text-sm text-neutral-700">
                          {candidate.jobPostingTitle || 'Unknown'}
                        </td>
                        <td className="px-6 py-4 text-sm">
                          <Badge label={candidate.status} variant={statusColors[candidate.status] as any} />
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </Card>
        ) : activeTab === 'interviews' ? (
          <Card>
            <h2 className="text-lg font-semibold text-neutral-900 mb-4">Interviews</h2>
            {interviews.length === 0 ? (
              <div className="text-center py-12">
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
                          <span>Date: {formatDate(interview.scheduledDateUtc)}</span>
                          <span>Interviewer: {interview.interviewerName}</span>
                          {interview.rating && <span>Rating: {interview.rating}/5</span>}
                        </div>
                        {interview.feedback && (
                          <p className="text-sm text-neutral-600 mt-2">Feedback: {interview.feedback}</p>
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
            <h2 className="text-lg font-semibold text-neutral-900 mb-4">Job Offers</h2>
            {offers.length === 0 ? (
              <div className="text-center py-12">
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
                          <span>Offered: ₹{offer.offeredSalary.toLocaleString('en-IN')}</span>
                          <span>Expires: {offer.expiryDateUtc ? formatDate(offer.expiryDateUtc) : 'N/A'}</span>
                        </div>
                      </div>
                      <Badge label={offer.status} variant={statusColors[offer.status] as any} />
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
            <Button variant="outline" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={() => setIsModalOpen(false)}>Post Job</Button>
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
              placeholder="e.g., Senior Developer"
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
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">
              Department *
            </label>
            <select className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500">
              <option>Select Department</option>
              <option>Engineering</option>
              <option>Sales</option>
              <option>Marketing</option>
              <option>HR</option>
            </select>
          </div>
        </div>
      </Modal>
    </MainLayout>
  )
}
