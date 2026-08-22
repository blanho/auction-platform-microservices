import { createLogger } from '@/shared/lib/logger'
import { ErrorState } from '@/shared/ui'
import i18next from 'i18next'
import type { ErrorInfo, ReactNode } from 'react'
import { Component } from 'react'

const errorBoundaryLogger = createLogger({ prefix: 'ErrorBoundary' })

interface ErrorBoundaryProps {
  children: ReactNode
  fallback?: ReactNode
  onError?: (error: Error, errorInfo: ErrorInfo) => void
}

interface ErrorBoundaryState {
  hasError: boolean
  error?: Error
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props)
    this.state = { hasError: false }
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error }
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    this.props.onError?.(error, errorInfo)

    if (import.meta.env.DEV) {
      errorBoundaryLogger.error('Caught an error:', error, errorInfo.componentStack)
    }
  }

  handleReset = (): void => {
    this.setState({ hasError: false, error: undefined })
  }

  render(): ReactNode {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback
      }

      return (
        <ErrorState
          title={i18next.t('errors.boundaryTitle')}
          message={i18next.t('errors.boundaryMessage')}
          onRetry={this.handleReset}
          showHomeButton
        />
      )
    }

    return this.props.children
  }
}
