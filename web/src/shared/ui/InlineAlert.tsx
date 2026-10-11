import type { AlertProps } from '@mui/material'
import { Alert, AlertTitle } from '@mui/material'
import { forwardRef } from 'react'
import type { ReactNode } from 'react'

interface InlineAlertProps extends Omit<AlertProps, 'title' | 'children'> {
  severity: NonNullable<AlertProps['severity']>
  title?: string
  children: ReactNode
}

export const InlineAlert = forwardRef<HTMLDivElement, InlineAlertProps>(function InlineAlert(
  { title, children, ...props },
  ref
) {
  return (
    <Alert ref={ref} {...props}>
      {title && <AlertTitle>{title}</AlertTitle>}
      {children}
    </Alert>
  )
})
