import { ReactNode } from 'react'
import { LucideIcon, Inbox } from 'lucide-react'
import Button from './Button'

interface EmptyStateProps {
  icon?: LucideIcon
  title: string
  description: string
  actionLabel?: string
  onAction?: () => void
  actionIcon?: ReactNode
  secondaryActionLabel?: string
  onSecondaryAction?: () => void
  className?: string
}

export default function EmptyState({
  icon: Icon = Inbox,
  title,
  description,
  actionLabel,
  onAction,
  actionIcon,
  secondaryActionLabel,
  onSecondaryAction,
  className = '',
}: EmptyStateProps) {
  return (
    <div
      className={`flex flex-col items-center justify-center p-8 text-center rounded-xl border border-dashed border-neutral-300 bg-neutral-50/50 ${className}`}
    >
      <div className="w-12 h-12 rounded-full bg-primary-50 text-primary-600 flex items-center justify-center mb-4 shadow-sm">
        <Icon className="w-6 h-6" />
      </div>
      <h3 className="text-base font-semibold text-neutral-900 mb-1">{title}</h3>
      <p className="text-sm text-neutral-500 max-w-sm mb-6">{description}</p>
      {(actionLabel || secondaryActionLabel) && (
        <div className="flex items-center gap-3">
          {secondaryActionLabel && onSecondaryAction && (
            <Button variant="secondary" onClick={onSecondaryAction}>
              {secondaryActionLabel}
            </Button>
          )}
          {actionLabel && onAction && (
            <Button variant="primary" onClick={onAction}>
              {actionIcon && <span className="mr-2">{actionIcon}</span>}
              {actionLabel}
            </Button>
          )}
        </div>
      )}
    </div>
  )
}
