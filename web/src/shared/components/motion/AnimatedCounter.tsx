import { useState, useEffect, useRef, useCallback } from 'react'

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
  const [hasAnimated, setHasAnimated] = useState(false)
  const ref = useRef<HTMLSpanElement>(null)

  const animateValue = useCallback(() => {
    const steps = 60
    const increment = value / steps
    let current = 0

    const timer = setInterval(() => {
      current += increment
      if (current >= value) {
        setDisplayValue(value)
        clearInterval(timer)
      } else {
        setDisplayValue(Math.floor(current))
      }
    }, duration / steps)
  }, [value, duration])

  useEffect(() => {
    if (hasAnimated || !ref.current) return

    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting && !hasAnimated) {
          setHasAnimated(true)
          animateValue()
          observer.disconnect()
        }
      },
      { threshold: 0.5 }
    )

    observer.observe(ref.current)
    return () => observer.disconnect()
  }, [hasAnimated, animateValue])

  return (
    <span ref={ref}>
      {prefix}
      {displayValue.toLocaleString()}
      {suffix}
    </span>
  )
}
