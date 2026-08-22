import { formatNumber } from '@/shared/utils/formatters'
import { useCallback, useEffect, useRef, useState } from 'react'

interface AnimatedCounterProps {
  value: number
  prefix?: string
  suffix?: string
  duration?: number
}

export const AnimatedCounter = ({
  value,
  prefix = '',
  suffix = '',
  duration = 2000,
}: AnimatedCounterProps) => {
  const [displayValue, setDisplayValue] = useState(0)
  const ref = useRef<HTMLSpanElement>(null)
  const animationTimerRef = useRef<ReturnType<typeof setInterval> | null>(null)

  const animateValue = useCallback(() => {
    const steps = 60
    const increment = value / steps
    let current = 0

    animationTimerRef.current = setInterval(
      () => {
        current += increment
        if (current >= value) {
          setDisplayValue(value)
          if (animationTimerRef.current) {
            clearInterval(animationTimerRef.current)
            animationTimerRef.current = null
          }
        } else {
          setDisplayValue(Math.floor(current))
        }
      },
      Math.max(1, duration / steps)
    )
  }, [value, duration])

  useEffect(() => {
    if (!ref.current) {
      return
    }

    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          animateValue()
          observer.disconnect()
        }
      },
      { threshold: 0.5 }
    )

    observer.observe(ref.current)
    return () => {
      observer.disconnect()
      if (animationTimerRef.current) {
        clearInterval(animationTimerRef.current)
        animationTimerRef.current = null
      }
    }
  }, [animateValue])

  return (
    <span ref={ref}>
      {prefix}
      {formatNumber(displayValue)}
      {suffix}
    </span>
  )
}
