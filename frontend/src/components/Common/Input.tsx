import { ReactNode } from 'react'

interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string
  error?: string
  icon?: ReactNode
  endAdornment?: ReactNode
}

export default function Input({
  label,
  error,
  icon,
  endAdornment,
  className = '',
  ...props
}: InputProps) {
  return (
    <div className="w-full">
      {label && (
        <label className="block text-sm font-medium text-neutral-700 mb-1">
          {label}
        </label>
      )}
      <div className="relative">
        {icon && (
          <div className="absolute left-3 top-1/2 transform -translate-y-1/2 text-neutral-400">
            {icon}
          </div>
        )}
        <input
          className={`
            w-full px-4 py-2 border rounded-lg
            ${icon ? 'pl-10' : ''}
            ${endAdornment ? 'pr-10' : ''}
            ${
              error
                ? 'border-red-500 focus:ring-red-500'
                : 'border-neutral-300 focus:ring-primary-500'
            }
            focus:outline-none focus:ring-2 focus:ring-offset-0
            ${className}
          `}
          {...props}
        />
        {endAdornment && (
          <div className="absolute right-3 top-1/2 transform -translate-y-1/2 text-neutral-500 flex items-center">
            {endAdornment}
          </div>
        )}
      </div>
      {error && <p className="text-red-600 text-sm mt-1">{error}</p>}
    </div>
  )
}
