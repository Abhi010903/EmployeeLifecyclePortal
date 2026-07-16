import { ReactNode } from 'react'

interface CardProps {
  children: ReactNode
  className?: string
  onClick?: () => void
}

export default function Card({ children, className = '', onClick }: CardProps) {
  return (
    <div
      onClick={onClick}
      className={`
        bg-white border border-neutral-200 rounded-lg p-6
        ${onClick ? 'cursor-pointer hover:shadow-md' : ''}
        shadow-sm transition-shadow
        ${className}
      `}
    >
      {children}
    </div>
  )
}
