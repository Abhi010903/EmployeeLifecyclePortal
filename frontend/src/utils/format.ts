/**
 * Centralized formatting utility for Indian Standard Time (IST / Asia/Kolkata)
 * and Indian Rupee (INR ₹) currency formatting.
 */

const IST_TIMEZONE = 'Asia/Kolkata'

/**
 * Safely parses any date string as UTC if no offset is present.
 */
export function parseUtcDate(dateInput?: string | Date | null): Date | null {
  if (!dateInput) return null
  if (dateInput instanceof Date) return isNaN(dateInput.getTime()) ? null : dateInput
  let str = String(dateInput).trim()
  if (!str) return null
  // If string contains 'T' but no timezone offset (Z or + or -hh:mm), treat as UTC by appending 'Z'
  if (str.includes('T') && !str.endsWith('Z') && !str.includes('+') && !/-\d{2}:\d{2}$/.test(str)) {
    str += 'Z'
  }
  const d = new Date(str)
  return isNaN(d.getTime()) ? null : d
}

/**
 * Format ISO timestamp or Date to IST Date string (e.g., "23 Aug 2026")
 */
export function formatDateIST(dateInput?: string | Date | null): string {
  const d = parseUtcDate(dateInput)
  if (!d) return '-'
  try {
    return new Intl.DateTimeFormat('en-IN', {
      timeZone: IST_TIMEZONE,
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    }).format(d)
  } catch {
    return '-'
  }
}

/**
 * Format ISO timestamp or Date to IST Time string (e.g., "02:04:12 AM IST")
 */
export function formatTimeIST(dateInput?: string | Date | null, includeSuffix = true): string {
  const d = parseUtcDate(dateInput)
  if (!d) return '-'
  try {
    const formatted = new Intl.DateTimeFormat('en-IN', {
      timeZone: IST_TIMEZONE,
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: true,
    }).format(d)
    return includeSuffix ? `${formatted} IST` : formatted
  } catch {
    return '-'
  }
}

/**
 * Format ISO timestamp or Date to IST DateTime string (e.g., "23 Aug 2026, 02:04:12 AM IST")
 */
export function formatDateTimeIST(dateInput?: string | Date | null, includeSuffix = true): string {
  const d = parseUtcDate(dateInput)
  if (!d) return '-'
  try {
    const datePart = new Intl.DateTimeFormat('en-IN', {
      timeZone: IST_TIMEZONE,
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    }).format(d)
    const timePart = new Intl.DateTimeFormat('en-IN', {
      timeZone: IST_TIMEZONE,
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: true,
    }).format(d)
    return includeSuffix ? `${datePart}, ${timePart} IST` : `${datePart}, ${timePart}`
  } catch {
    return '-'
  }
}

/**
 * Format number to Indian Rupee currency (e.g., "₹75,000")
 */
export function formatCurrencyINR(amount?: number | null, fractionDigits = 0): string {
  if (amount === undefined || amount === null || isNaN(amount)) return '₹0'
  try {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      minimumFractionDigits: fractionDigits,
      maximumFractionDigits: fractionDigits,
    }).format(amount)
  } catch {
    return `₹${amount}`
  }
}

/**
 * Calculate human-readable duration between two timestamps (e.g., "4h 30m" or "18s")
 */
export function formatDuration(checkInUtc?: string | null, checkOutUtc?: string | null): string {
  if (!checkInUtc) return '-'
  if (!checkOutUtc) return 'In Progress'
  try {
    const start = parseUtcDate(checkInUtc)?.getTime()
    const end = parseUtcDate(checkOutUtc)?.getTime()
    if (!start || !end || end < start) return '-'
    const totalSeconds = Math.floor((end - start) / 1000)
    const hours = Math.floor(totalSeconds / 3600)
    const minutes = Math.floor((totalSeconds % 3600) / 60)
    const seconds = totalSeconds % 60
    if (hours > 0) return `${hours}h ${minutes}m`
    if (minutes > 0) return `${minutes}m ${seconds}s`
    return `${seconds}s`
  } catch {
    return '-'
  }
}
