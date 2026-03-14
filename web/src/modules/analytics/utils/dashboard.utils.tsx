import { CheckCircle, Warning, Error as ErrorIcon } from '@mui/icons-material'

export const getHealthIcon = (status: string) => {
  switch (status.toLowerCase()) {
    case 'healthy':
    case 'connected':
      return <CheckCircle sx={{ color: 'success.main', fontSize: 20 }} />
    case 'degraded':
    case 'unknown':
      return <Warning sx={{ color: 'warning.main', fontSize: 20 }} />
    default:
      return <ErrorIcon sx={{ color: 'error.main', fontSize: 20 }} />
  }
}

export const getHealthColor = (status: string) => {
  switch (status.toLowerCase()) {
    case 'healthy':
    case 'connected':
      return 'success.main'
    case 'degraded':
    case 'unknown':
      return 'warning.main'
    default:
      return 'error.main'
  }
}
