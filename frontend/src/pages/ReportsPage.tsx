import { useState } from 'react'
import MainLayout from '@/components/Layout/MainLayout'
import Card from '@/components/Common/Card'
import Button from '@/components/Common/Button'
import Badge from '@/components/Common/Badge'
import { BarChart, LineChart, PieChart, Download, AlertCircle } from 'lucide-react'
import { reportsApi } from '@/api/reports'
import { ReportDataDto } from '@/types'

type ReportType = 'employees' | 'attendance' | 'payroll' | 'department'

export default function ReportsPage() {
  const [reportType, setReportType] = useState<ReportType>('employees')
  const [reportData, setReportData] = useState<ReportDataDto | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [month, setMonth] = useState(new Date().getMonth() + 1)
  const [year, setYear] = useState(new Date().getFullYear())
  const [startDate, setStartDate] = useState(new Date(new Date().setDate(1)).toISOString().split('T')[0])
  const [endDate, setEndDate] = useState(new Date().toISOString().split('T')[0])

  const generateReport = async () => {
    try {
      setLoading(true)
      setError(null)
      let data: ReportDataDto | null = null

      switch (reportType) {
        case 'employees':
          const empRes = await reportsApi.getEmployeeReport()
          data = empRes.data || null
          break
        case 'attendance':
          const attRes = await reportsApi.getAttendanceReport(startDate, endDate)
          data = attRes.data || null
          break
        case 'payroll':
          const payRes = await reportsApi.getPayrollReport(month, year)
          data = payRes.data || null
          break
        case 'department':
          const deptRes = await reportsApi.getDepartmentReport(month, year)
          data = deptRes.data || null
          break
      }

      setReportData(data)
    } catch (err) {
      setError('Failed to generate report')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  const handleExport = async (format: 'csv' | 'excel' | 'pdf') => {
    try {
      if (!reportData) return
      const exportFn = format === 'csv' ? reportsApi.exportCsv : 
                      format === 'excel' ? reportsApi.exportExcel : 
                      reportsApi.exportPdf
      await exportFn(reportData.data)
    } catch (err) {
      setError(`Failed to export ${format.toUpperCase()}`)
    }
  }

  return (
    <MainLayout>
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-neutral-900">Reports</h1>
          <p className="text-neutral-600 mt-1">Generate and export business reports with charts</p>
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 flex items-start gap-3">
            <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
            <p className="text-sm text-red-900">{error}</p>
          </div>
        )}

        {/* Report Type Selection */}
        <Card>
          <h2 className="text-lg font-semibold text-neutral-900 mb-4">Select Report Type</h2>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mb-4">
            {[
              { id: 'employees', label: 'Employees', icon: '👥' },
              { id: 'attendance', label: 'Attendance', icon: '📋' },
              { id: 'payroll', label: 'Payroll', icon: '💰' },
              { id: 'department', label: 'Department', icon: '🏢' },
            ].map(report => (
              <button
                key={report.id}
                onClick={() => setReportType(report.id as ReportType)}
                className={`p-3 rounded-lg border-2 transition ${
                  reportType === report.id
                    ? 'border-blue-600 bg-blue-50'
                    : 'border-neutral-200 hover:border-blue-300'
                }`}
              >
                <div className="text-2xl mb-1">{report.icon}</div>
                <div className="text-sm font-medium text-neutral-900">{report.label}</div>
              </button>
            ))}
          </div>

          {/* Date/Time Filters */}
          <div className="space-y-4 mb-4">
            {reportType === 'attendance' && (
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-neutral-700 mb-1">Start Date</label>
                  <input
                    type="date"
                    value={startDate}
                    onChange={e => setStartDate(e.target.value)}
                    className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-neutral-700 mb-1">End Date</label>
                  <input
                    type="date"
                    value={endDate}
                    onChange={e => setEndDate(e.target.value)}
                    className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              </div>
            )}

            {(reportType === 'payroll' || reportType === 'department') && (
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-neutral-700 mb-1">Month</label>
                  <select
                    value={month}
                    onChange={e => setMonth(parseInt(e.target.value))}
                    className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  >
                    {Array.from({ length: 12 }, (_, i) => (
                      <option key={i + 1} value={i + 1}>
                        {new Date(2024, i).toLocaleDateString('en-US', { month: 'long' })}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-neutral-700 mb-1">Year</label>
                  <select
                    value={year}
                    onChange={e => setYear(parseInt(e.target.value))}
                    className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  >
                    {Array.from({ length: 5 }, (_, i) => {
                      const y = new Date().getFullYear() - 2 + i
                      return <option key={y} value={y}>{y}</option>
                    })}
                  </select>
                </div>
              </div>
            )}
          </div>

          <Button onClick={generateReport} disabled={loading} className="w-full bg-blue-600 hover:bg-blue-700">
            {loading ? 'Generating...' : 'Generate Report'}
          </Button>
        </Card>

        {/* Report Results */}
        {reportData && (
          <>
            {/* Summary Stats */}
            {reportData.summary && (
              <Card className="bg-gradient-to-r from-blue-50 to-blue-100">
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div>
                    <p className="text-neutral-600 text-sm">Total Records</p>
                    <p className="text-3xl font-bold text-neutral-900 mt-2">{reportData.summary.totalRecords}</p>
                  </div>
                  {reportData.summary.keyMetric && (
                    <div>
                      <p className="text-neutral-600 text-sm">Key Metric</p>
                      <p className="text-2xl font-bold text-neutral-900 mt-2">{reportData.summary.keyMetric}</p>
                    </div>
                  )}
                  {reportData.summary.totalAmount && (
                    <div>
                      <p className="text-neutral-600 text-sm">Total Amount</p>
                      <p className="text-2xl font-bold text-neutral-900 mt-2">₹{reportData.summary.totalAmount.toLocaleString('en-IN')}</p>
                    </div>
                  )}
                </div>
              </Card>
            )}

            {/* Charts */}
            {reportData.chartData && reportData.chartData.length > 0 && (
              <Card>
                <h2 className="text-lg font-semibold text-neutral-900 mb-4">Charts</h2>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {reportData.chartData.map((chart, idx) => (
                    <div key={idx} className="border border-neutral-200 rounded-lg p-4">
                      <div className="flex items-center justify-between mb-3">
                        <h3 className="font-medium text-neutral-900">{chart.label}</h3>
                        <Badge label={chart.type} variant="info" />
                      </div>
                      <div className="bg-neutral-50 rounded-lg p-4 min-h-[200px] flex items-center justify-center">
                        <div className="text-center">
                          {chart.type === 'bar' && <BarChart className="w-12 h-12 text-blue-600 mx-auto mb-2" />}
                          {chart.type === 'line' && <LineChart className="w-12 h-12 text-blue-600 mx-auto mb-2" />}
                          {(chart.type === 'pie' || chart.type === 'doughnut') && <PieChart className="w-12 h-12 text-blue-600 mx-auto mb-2" />}
                          <p className="text-sm text-neutral-600">
                            {chart.labels.length} data points
                          </p>
                          <div className="mt-3 text-xs text-neutral-500 space-y-1">
                            {chart.labels.slice(0, 3).map((label, i) => (
                              <div key={i}>
                                {label}: {chart.values[i]}
                              </div>
                            ))}
                          </div>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </Card>
            )}

            {/* Data Summary */}
            {reportData.data && Object.keys(reportData.data).length > 0 && (
              <Card>
                <h2 className="text-lg font-semibold text-neutral-900 mb-4">Data Summary</h2>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  {Object.entries(reportData.data).map(([key, value]) => (
                    <div key={key} className="border border-neutral-200 rounded-lg p-4">
                      <p className="text-neutral-600 text-sm capitalize">{key.replace(/([A-Z])/g, ' $1')}</p>
                      <p className="text-2xl font-bold text-neutral-900 mt-2">
                        {typeof value === 'number' && value > 1000
                          ? `₹${(value as number).toLocaleString('en-IN')}`
                          : String(value)}
                      </p>
                    </div>
                  ))}
                </div>
              </Card>
            )}

            {/* Export Options */}
            <Card>
              <h2 className="text-lg font-semibold text-neutral-900 mb-4">Export Report</h2>
              <div className="flex gap-2 flex-wrap">
                <Button
                  onClick={() => handleExport('csv')}
                  className="bg-green-600 hover:bg-green-700 flex items-center gap-2"
                >
                  <Download className="w-4 h-4" />
                  CSV
                </Button>
                <Button
                  onClick={() => handleExport('excel')}
                  className="bg-emerald-600 hover:bg-emerald-700 flex items-center gap-2"
                >
                  <Download className="w-4 h-4" />
                  Excel
                </Button>
                <Button
                  onClick={() => handleExport('pdf')}
                  className="bg-red-600 hover:bg-red-700 flex items-center gap-2"
                >
                  <Download className="w-4 h-4" />
                  PDF
                </Button>
              </div>
            </Card>
          </>
        )}
      </div>
    </MainLayout>
  )
}
