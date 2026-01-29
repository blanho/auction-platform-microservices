export function formatTimeLeft(endTime: string): string {
  if (!endTime) {return '--'}
  const diff = new Date(endTime).getTime() - Date.now()
  if (diff <= 0) {return 'Ended'}
  const days = Math.floor(diff / (24 * 60 * 60 * 1000))
  const hours = Math.floor((diff % (24 * 60 * 60 * 1000)) / (60 * 60 * 1000))
  if (days > 0) {return `${days}d ${hours}h`}
  const minutes = Math.floor((diff % (60 * 60 * 1000)) / (60 * 1000))
  return `${hours}h ${minutes}m`
}
