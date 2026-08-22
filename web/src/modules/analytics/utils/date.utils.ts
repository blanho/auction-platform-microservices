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
