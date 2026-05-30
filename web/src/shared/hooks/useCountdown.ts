import { useState, useEffect } from 'react'

const MS_PER_SECOND = 1_000
const MS_PER_MINUTE = 60 * MS_PER_SECOND
const MS_PER_HOUR = 60 * MS_PER_MINUTE
const MS_PER_DAY = 24 * MS_PER_HOUR

const URGENT_THRESHOLD_MS = MS_PER_HOUR // < 1 hour remaining

interface CountdownState {
  timeLeft: string
  isExpired: boolean
  isUrgent: boolean
}

function computeCountdown(endTime: string): CountdownState {
  const diff = new Date(endTime).getTime() - Date.now()

  if (diff <= 0) {
    return { timeLeft: '', isExpired: true, isUrgent: false }
  }

  const days = Math.floor(diff / MS_PER_DAY)
  const hours = Math.floor((diff % MS_PER_DAY) / MS_PER_HOUR)
  const minutes = Math.floor((diff % MS_PER_HOUR) / MS_PER_MINUTE)
  const seconds = Math.floor((diff % MS_PER_MINUTE) / MS_PER_SECOND)
  const isUrgent = diff < URGENT_THRESHOLD_MS

  let timeLeft: string
  if (days > 0) {
    timeLeft = `${days}d ${hours}h ${minutes}m`
  } else if (hours > 0) {
    timeLeft = `${hours}h ${minutes}m ${seconds}s`
  } else if (minutes > 0) {
    timeLeft = `${minutes}m ${seconds}s`
  } else {
    timeLeft = `${seconds}s`
  }

  return { timeLeft, isExpired: false, isUrgent }
}

/**
 * Provides a live countdown to a given end time.
 * Updates every second by default; pass intervalMs=60_000 for minute-precision cards.
 */
export function useCountdown(endTime: string, intervalMs = MS_PER_SECOND): CountdownState {
  const [state, setState] = useState<CountdownState>(() => computeCountdown(endTime))

  useEffect(() => {
    const tick = () => setState(computeCountdown(endTime))
    tick()
    const timerId = setInterval(tick, intervalMs)
    return () => clearInterval(timerId)
  }, [endTime, intervalMs])

  return state
}
