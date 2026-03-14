import type { JobStatus } from '../types'

export function getJobProgressLabel(completed: number, total: number): string {
  if (total === 0) {
    return '0 / 0'
  }
  return `${completed.toLocaleString()} / ${total.toLocaleString()}`
}

export function isJobActive(status: JobStatus): boolean {
  return status === 'Initializing' || status === 'Pending' || status === 'Processing'
}

export function isJobTerminal(status: JobStatus): boolean {
  return (
    status === 'Completed' ||
    status === 'CompletedWithErrors' ||
    status === 'Failed' ||
    status === 'Cancelled'
  )
}

export function getJobDuration(startedAt?: string, completedAt?: string): string {
  if (!startedAt) {
    return '—'
  }
  const start = new Date(startedAt).getTime()
  const end = completedAt ? new Date(completedAt).getTime() : Date.now()
  const seconds = Math.floor((end - start) / 1000)

  if (seconds < 60) {
    return `${seconds}s`
  }
  if (seconds < 3600) {
    const minutes = Math.floor(seconds / 60)
    const remaining = seconds % 60
    return `${minutes}m ${remaining}s`
  }
  const hours = Math.floor(seconds / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  return `${hours}h ${minutes}m`
}
