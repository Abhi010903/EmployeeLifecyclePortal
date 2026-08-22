import { ReactNode } from 'react'

interface BadgeProps {
  label?: string
  children?: ReactNode
  variant?: 'success' | 'warning' | 'danger' | 'info' | 'neutral' | 'default'
  className?: string
}

const variantClasses = {
  success: 'bg-emerald-100 text-emerald-800 border-emerald-200',
  warning: 'bg-amber-100 text-amber-800 border-amber-200',
  danger: 'bg-red-100 text-red-800 border-red-200',
  info: 'bg-blue-100 text-blue-800 border-blue-200',
  neutral: 'bg-neutral-100 text-neutral-800 border-neutral-200',
  default: 'bg-neutral-100 text-neutral-800 border-neutral-200',
}

export default function Badge({
  label,
  children,
  variant = 'neutral',
  className = '',
}: BadgeProps) {
  const content = children || label
  const variantClass = variantClasses[variant] || variantClasses.neutral

  return (
    <span
      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold border ${variantClass} ${className}`}
    >
      {content}
    </span>
  )
}
