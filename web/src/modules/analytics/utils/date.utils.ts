export type TimeRange = '7d' | '30d' | '90d' | '1y'

export const getDateRange = (range: TimeRange) => {
  const end = new Date()
  const start = new Date()

  switch (range) {
    case '7d':
      start.setDate(start.getDate() - 7)
      break
    case '30d':
      start.setDate(start.getDate() - 30)
      break
    case '90d':
      start.setDate(start.getDate() - 90)
      break
    case '1y':
      start.setFullYear(start.getFullYear() - 1)
      break
  }

  return {
    startDate: start.toISOString(),
    endDate: end.toISOString(),
  }
}

export const formatTimeRangeLabel = (range: TimeRange): string => {
  const labels: Record<TimeRange, string> = {
    '7d': 'Last 7 Days',
    '30d': 'Last 30 Days',
    '90d': 'Last 90 Days',
    '1y': 'Last Year',
  }
  return labels[range]
}
