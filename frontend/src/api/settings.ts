import { apiClient } from './client'

export interface CompanyProfile {
  id?: string
  companyName: string
  companyCode?: string
  industryType?: string
  address?: string
  city?: string
  state?: string
  country?: string
  postalCode?: string
  phoneNumber?: string
  email?: string
  website?: string
  logoPath?: string
  registrationNumber?: string
  foundedDate?: string
}

export interface UserSettings {
  id?: string
  userId: string
  theme?: string
  language?: string
  timeZone?: string
  emailNotifications: boolean
  smsNotifications: boolean
  pushNotifications: boolean
  twoFactorEnabled: boolean
}

export interface HolidayCalendar {
  id?: string
  holidayName: string
  holidayDate: string
  description?: string
  isOptional: boolean
  year: number
}

export interface Shift {
  id?: string
  shiftName: string
  startTime: string
  endTime: string
  workingHours: number
  isActive: boolean
}

export interface EmailConfiguration {
  id?: string
  smtpServer: string
  smtpPort: number
  senderEmail: string
  senderName?: string
  enableSsl: boolean
  username?: string
}

export const settingsApi = {
  getCompanyProfile: () => apiClient.get<CompanyProfile>('/settings/company'),
  updateCompanyProfile: (data: CompanyProfile) => apiClient.put<CompanyProfile>('/settings/company', data),
  getUserSettings: (userId: string) => apiClient.get<UserSettings>(`/settings/user/${userId}`),
  updateUserSettings: (userId: string, data: UserSettings) => apiClient.put<UserSettings>(`/settings/user/${userId}`, data),
  getHolidays: (year: number) => apiClient.get<HolidayCalendar[]>(`/settings/holidays/${year}`),
  createHoliday: (data: HolidayCalendar) => apiClient.post<HolidayCalendar>('/settings/holidays', data),
  getShifts: () => apiClient.get<Shift[]>('/settings/shifts'),
  createShift: (data: Partial<Shift>) => apiClient.post<Shift>('/settings/shifts', data),
  getEmailConfig: () => apiClient.get<EmailConfiguration>('/settings/email-config'),
  updateEmailConfig: (data: EmailConfiguration) => apiClient.put<EmailConfiguration>('/settings/email-config', data),
}
