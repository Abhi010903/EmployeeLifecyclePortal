import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import Modal from '@/components/Common/Modal'
import { ArrowLeft, Edit2, Trash2, CheckCircle2, XCircle, AlertCircle, FileText, Upload, Download, Clock } from 'lucide-react'
import { employeesApi } from '@/api/employees'
import { documentsApi, EmployeeDocument } from '@/api/documents'
import type { EmployeeProfile } from '@/types'
import toast from 'react-hot-toast'

export default function EmployeeDetailsPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [employee, setEmployee] = useState<EmployeeProfile | null>(null)
  const [documents, setDocuments] = useState<EmployeeDocument[]>([])
  const [timeline, setTimeline] = useState<any[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false)
  const [isDeleting, setIsDeleting] = useState(false)

  // Document Upload State
  const [isUploadModalOpen, setIsUploadModalOpen] = useState(false)
  const [docType, setDocType] = useState('Contract')
  const [docFile, setDocFile] = useState<File | null>(null)
  const [docNotes, setDocNotes] = useState('')
  const [isUploading, setIsUploading] = useState(false)

  useEffect(() => {
    if (id) {
      loadEmployeeData()
    }
  }, [id])

  const loadEmployeeData = async () => {
    try {
      setIsLoading(true)
      const [profileData, docsData, timelineData] = await Promise.allSettled([
        employeesApi.getProfile(id!),
        documentsApi.getDocuments(id!),
        employeesApi.getTimeline(id!),
      ])

      if (profileData.status === 'fulfilled') {
        setEmployee(profileData.value)
      }
      if (docsData.status === 'fulfilled') {
        setDocuments(docsData.value)
      }
      if (timelineData.status === 'fulfilled' && Array.isArray(timelineData.value)) {
        setTimeline(timelineData.value)
      }
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to load employee')
      navigate('/employees')
    } finally {
      setIsLoading(false)
    }
  }

  const handleDelete = async () => {
    if (!employee) return

    try {
      setIsDeleting(true)
      await employeesApi.delete(employee.id)
      toast.success('Employee deleted successfully')
      navigate('/employees')
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to delete employee')
    } finally {
      setIsDeleting(false)
      setIsDeleteConfirmOpen(false)
    }
  }

  const handleStatusChange = async (action: 'activate' | 'deactivate' | 'terminate') => {
    if (!employee) return
    try {
      if (action === 'activate') await employeesApi.activate(employee.id)
      else if (action === 'deactivate') await employeesApi.deactivate(employee.id)
      else if (action === 'terminate') await employeesApi.terminate(employee.id)

      toast.success(`Employee ${action}d successfully`)
      loadEmployeeData()
    } catch (error: any) {
      toast.error(error?.response?.data?.message || `Failed to ${action} employee`)
    }
  }

  const handleUploadDocument = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!docFile || !id) {
      toast.error('Please select a file to upload.')
      return
    }

    try {
      setIsUploading(true)
      const formData = new FormData()
      formData.append('File', docFile)
      formData.append('DocumentType', docType)
      if (docNotes) formData.append('Notes', docNotes)

      await documentsApi.uploadDocument(id, formData)
      toast.success('Document uploaded successfully!')
      setIsUploadModalOpen(false)
      setDocFile(null)
      setDocNotes('')
      const updatedDocs = await documentsApi.getDocuments(id)
      setDocuments(updatedDocs)
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to upload document')
    } finally {
      setIsUploading(false)
    }
  }

  // Document Delete State
  const [isDeleteDocConfirmOpen, setIsDeleteDocConfirmOpen] = useState(false)
  const [docToDelete, setDocToDelete] = useState<EmployeeDocument | null>(null)
  const [isDeletingDoc, setIsDeletingDoc] = useState(false)

  const handleOpenDeleteDoc = (doc: EmployeeDocument) => {
    setDocToDelete(doc)
    setIsDeleteDocConfirmOpen(true)
  }

  const handleConfirmDeleteDoc = async () => {
    if (!id || !docToDelete) return
    try {
      setIsDeletingDoc(true)
      await documentsApi.deleteDocument(id, docToDelete.id)
      toast.success('Document deleted successfully!')
      setIsDeleteDocConfirmOpen(false)
      setDocToDelete(null)
      const updatedDocs = await documentsApi.getDocuments(id)
      setDocuments(updatedDocs)
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Failed to delete document')
    } finally {
      setIsDeletingDoc(false)
    }
  }

  const handleDownloadDocument = async (doc: EmployeeDocument) => {
    if (!id) return
    try {
      await documentsApi.downloadDocument(id, doc.id, doc.fileName)
    } catch (error) {
      toast.error('Failed to download document')
    }
  }

  const formatDate = (date: string) => {
    return new Intl.DateTimeFormat('en-IN', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    }).format(new Date(date))
  }

  if (isLoading) {
    return (
      <MainLayout>
        <div className="flex items-center justify-center py-12">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
        </div>
      </MainLayout>
    )
  }

  if (!employee) {
    return (
      <MainLayout>
        <div className="text-center py-12">
          <p className="text-neutral-600">Employee not found</p>
        </div>
      </MainLayout>
    )
  }

  return (
    <MainLayout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            <button
              onClick={() => navigate('/employees')}
              className="p-2 hover:bg-neutral-100 rounded-lg transition-colors"
            >
              <ArrowLeft className="w-5 h-5" />
            </button>
            <div>
              <h1 className="text-3xl font-bold text-neutral-900">
                {employee.firstName} {employee.lastName}
              </h1>
              <p className="text-neutral-600 mt-1">Employee Code: {employee.employeeCode}</p>
            </div>
          </div>

          <div className="flex items-center gap-2">
            {employee.status !== 'Active' && (
              <Button size="sm" variant="outline" onClick={() => handleStatusChange('activate')}>
                Activate
              </Button>
            )}
            {employee.status === 'Active' && (
              <Button size="sm" variant="outline" onClick={() => handleStatusChange('deactivate')}>
                Deactivate
              </Button>
            )}
            {employee.status !== 'Terminated' && (
              <Button size="sm" variant="outline" onClick={() => handleStatusChange('terminate')}>
                Terminate
              </Button>
            )}
            <Button
              variant="outline"
              onClick={() => navigate(`/employees/${employee.id}/edit`)}
            >
              <Edit2 className="w-4 h-4 mr-2" />
              Edit
            </Button>
            <Button
              variant="danger"
              onClick={() => setIsDeleteConfirmOpen(true)}
            >
              <Trash2 className="w-4 h-4 mr-2" />
              Delete
            </Button>
          </div>
        </div>

        {/* Main Information */}
        <div className="grid grid-cols-3 gap-6">
          {/* Personal Information */}
          <Card className="col-span-2">
            <div className="pb-4 border-b border-neutral-200">
              <h2 className="text-xl font-semibold text-neutral-900">Personal Information</h2>
            </div>
            <div className="space-y-4 mt-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-xs text-neutral-500 font-medium">First Name</p>
                  <p className="text-base font-semibold text-neutral-900 mt-1">{employee.firstName}</p>
                </div>
                <div>
                  <p className="text-xs text-neutral-500 font-medium">Last Name</p>
                  <p className="text-base font-semibold text-neutral-900 mt-1">{employee.lastName}</p>
                </div>
              </div>

              <div>
                <p className="text-xs text-neutral-500 font-medium">Email</p>
                <p className="text-base font-medium text-neutral-900 mt-1">{employee.email}</p>
              </div>

              <div>
                <p className="text-xs text-neutral-500 font-medium">Phone Number</p>
                <p className="text-base font-medium text-neutral-900 mt-1">{employee.phoneNumber || '-'}</p>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-xs text-neutral-500 font-medium">Department</p>
                  <p className="text-base font-medium text-neutral-900 mt-1">{employee.departmentName || '-'}</p>
                </div>
                <div>
                  <p className="text-xs text-neutral-500 font-medium">Reporting Manager</p>
                  <p className="text-base font-medium text-neutral-900 mt-1">{employee.managerName || '-'}</p>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-xs text-neutral-500 font-medium">Team Lead</p>
                  <p className="text-base font-medium text-neutral-900 mt-1">{employee.teamLeadName || '-'}</p>
                </div>
                <div>
                  <p className="text-xs text-neutral-500 font-medium">Created On</p>
                  <p className="text-sm font-medium text-neutral-900 mt-1">{formatDate(employee.createdAtUtc)}</p>
                </div>
              </div>
            </div>
          </Card>

          {/* Status and Quick Info */}
          <div className="space-y-6">
            <Card>
              <div className="pb-3 border-b border-neutral-200 mb-3">
                <h3 className="font-semibold text-neutral-900">Status</h3>
              </div>
              <div className="flex items-center gap-3">
                {employee.status === 'Active' ? (
                  <CheckCircle2 className="w-5 h-5 text-green-600" />
                ) : employee.status === 'Inactive' ? (
                  <AlertCircle className="w-5 h-5 text-amber-600" />
                ) : (
                  <XCircle className="w-5 h-5 text-red-600" />
                )}
                <Badge
                  label={employee.status}
                  variant={
                    employee.status === 'Active'
                      ? 'success'
                      : employee.status === 'Inactive'
                        ? 'warning'
                        : 'danger'
                  }
                />
              </div>
            </Card>

            <Card>
              <div className="pb-3 border-b border-neutral-200 mb-3">
                <h3 className="font-semibold text-neutral-900">Roles</h3>
              </div>
              <div className="space-y-2">
                {employee.roles && employee.roles.length > 0 ? (
                  employee.roles.map((role: any, idx) => (
                    <span key={role.id || role.roleId || idx} className="inline-block mr-2 mb-2 px-3 py-1 bg-blue-100 text-blue-700 text-xs font-medium rounded-full">
                      {role.name || role.roleName || 'Employee'}
                    </span>
                  ))
                ) : (
                  <p className="text-neutral-500 text-sm">No specific roles assigned</p>
                )}
              </div>
            </Card>
          </div>
        </div>

        {/* Documents Section */}
        <Card>
          <div className="flex justify-between items-center pb-4 border-b border-neutral-200">
            <div>
              <h3 className="text-lg font-semibold text-neutral-900">Documents ({documents.length})</h3>
              <p className="text-xs text-neutral-500">Contracts, identity proofs, and certifications</p>
            </div>
            <Button size="sm" onClick={() => setIsUploadModalOpen(true)}>
              <Upload className="w-4 h-4 mr-2" />
              Upload Document
            </Button>
          </div>

          <div className="mt-4">
            {documents.length === 0 ? (
              <div className="text-center py-6">
                <FileText className="w-10 h-10 text-neutral-300 mx-auto mb-2" />
                <p className="text-sm text-neutral-500">No documents uploaded yet</p>
              </div>
            ) : (
              <div className="divide-y divide-neutral-200">
                {documents.map((doc) => (
                  <div key={doc.id} className="py-3 flex justify-between items-center">
                    <div>
                      <p className="font-medium text-neutral-900 text-sm">{doc.fileName}</p>
                      <p className="text-xs text-neutral-500">
                        Type: {doc.documentType} • Size: {doc.formattedFileSize || `${doc.fileSizeBytes} B`} • Uploaded: {formatDate(doc.createdAtUtc)}
                      </p>
                    </div>
                    <div className="flex items-center gap-2">
                      <Button size="sm" variant="outline" onClick={() => handleDownloadDocument(doc)}>
                        <Download className="w-4 h-4 mr-1" />
                        Download
                      </Button>
                      <Button
                        size="sm"
                        variant="outline"
                        className="text-red-600 hover:bg-red-50 hover:border-red-300"
                        onClick={() => handleOpenDeleteDoc(doc)}
                      >
                        <Trash2 className="w-4 h-4 mr-1" />
                        Delete
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </Card>

        {/* Timeline / Activity Events */}
        {timeline.length > 0 && (
          <Card>
            <div className="pb-3 border-b border-neutral-200 mb-4">
              <h3 className="text-lg font-semibold text-neutral-900">Activity Timeline</h3>
            </div>
            <div className="space-y-4">
              {timeline.map((evt, idx) => (
                <div key={evt.id || idx} className="flex items-start gap-3 text-sm">
                  <Clock className="w-4 h-4 text-blue-600 mt-0.5" />
                  <div>
                    <p className="font-medium text-neutral-900">{evt.title || evt.eventType}</p>
                    <p className="text-xs text-neutral-500">{evt.description}</p>
                  </div>
                </div>
              ))}
            </div>
          </Card>
        )}
      </div>

      {/* Upload Document Modal */}
      <Modal
        isOpen={isUploadModalOpen}
        onClose={() => setIsUploadModalOpen(false)}
        title="Upload Employee Document"
        footer={
          <>
            <Button variant="outline" onClick={() => setIsUploadModalOpen(false)} disabled={isUploading}>
              Cancel
            </Button>
            <Button onClick={handleUploadDocument} disabled={isUploading}>
              {isUploading ? 'Uploading...' : 'Upload'}
            </Button>
          </>
        }
      >
        <form onSubmit={handleUploadDocument} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Document Type *</label>
            <select
              value={docType}
              onChange={(e) => setDocType(e.target.value)}
              className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              <option value="Contract">Contract / Employment Agreement</option>
              <option value="Certificate">Certificate</option>
              <option value="OfferLetter">Offer Letter</option>
              <option value="Payslip">Payslip</option>
              <option value="TrainingRecord">Training Record</option>
              <option value="Evaluation">Evaluation</option>
              <option value="Other">Other</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">File *</label>
            <input
              type="file"
              onChange={(e) => setDocFile(e.target.files ? e.target.files[0] : null)}
              className="w-full text-sm text-neutral-700 file:mr-4 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-sm file:font-semibold file:bg-primary-50 file:text-primary-700 hover:file:bg-primary-100"
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-700 mb-1">Notes (Optional)</label>
            <textarea
              rows={2}
              value={docNotes}
              onChange={(e) => setDocNotes(e.target.value)}
              placeholder="Any additional notes about this document..."
              className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
          </div>
        </form>
      </Modal>

      {/* Delete Document Confirmation Modal */}
      <Modal
        isOpen={isDeleteDocConfirmOpen}
        onClose={() => setIsDeleteDocConfirmOpen(false)}
        title="Delete Document"
        footer={
          <>
            <Button variant="outline" onClick={() => setIsDeleteDocConfirmOpen(false)} disabled={isDeletingDoc}>
              Cancel
            </Button>
            <Button variant="danger" onClick={handleConfirmDeleteDoc} disabled={isDeletingDoc}>
              {isDeletingDoc ? 'Deleting...' : 'Delete'}
            </Button>
          </>
        }
      >
        <p className="text-neutral-600">
          Are you sure you want to delete <strong>{docToDelete?.fileName}</strong>? This action will permanently delete the document metadata and file.
        </p>
      </Modal>

      {/* Delete Employee Confirmation Modal */}
      <Modal
        isOpen={isDeleteConfirmOpen}
        onClose={() => setIsDeleteConfirmOpen(false)}
        title="Delete Employee"
        footer={
          <>
            <Button variant="outline" onClick={() => setIsDeleteConfirmOpen(false)} disabled={isDeleting}>
              Cancel
            </Button>
            <Button variant="danger" onClick={handleDelete} disabled={isDeleting}>
              {isDeleting ? 'Deleting...' : 'Delete'}
            </Button>
          </>
        }
      >
        <p className="text-neutral-600">
          Are you sure you want to delete <strong>{employee.firstName} {employee.lastName}</strong>? This action cannot be undone.
        </p>
      </Modal>
    </MainLayout>
  )
}
