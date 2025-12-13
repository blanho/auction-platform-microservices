"use client";

import { useState, useEffect, useCallback, useRef, useMemo } from "react";

export interface TimeLeft {
  days: number;
  hours: number;
  minutes: number;
  seconds: number;
  total: number;
  isExpired: boolean;
}

export interface UseCountdownOptions {
  interval?: number;
  onExpire?: () => void;
  autoStart?: boolean;
}

const DEFAULT_INTERVAL = 1000;

function calculateTimeLeft(endDate: Date): TimeLeft {
  const now = new Date().getTime();
  const end = endDate.getTime();
  const difference = end - now;

  if (difference <= 0) {
    return {
      days: 0,
      hours: 0,
      minutes: 0,
      seconds: 0,
      total: 0,
      isExpired: true,
    };
  }

  return {
    days: Math.floor(difference / (1000 * 60 * 60 * 24)),
    hours: Math.floor((difference / (1000 * 60 * 60)) % 24),
    minutes: Math.floor((difference / 1000 / 60) % 60),
    seconds: Math.floor((difference / 1000) % 60),
    total: difference,
    isExpired: false,
  };
}

function getInitialTimeLeft(endDate: string | Date | null): TimeLeft | null {
  if (!endDate) return null;
  const end = typeof endDate === "string" ? new Date(endDate) : endDate;
  return calculateTimeLeft(end);
}

export function useCountdown(
  endDate: string | Date | null,
  options: UseCountdownOptions = {}
): TimeLeft | null {
  const { interval = DEFAULT_INTERVAL, onExpire, autoStart = true } = options;
  
  const initialTimeLeft = useMemo(() => getInitialTimeLeft(endDate), [endDate]);
  const [timeLeft, setTimeLeft] = useState<TimeLeft | null>(initialTimeLeft);
  const intervalRef = useRef<NodeJS.Timeout | null>(null);
  const hasExpiredRef = useRef(initialTimeLeft?.isExpired ?? false);

  const clearTimer = useCallback(() => {
    if (intervalRef.current) {
      clearInterval(intervalRef.current);
      intervalRef.current = null;
    }
  }, []);

  useEffect(() => {
    if (!endDate || !autoStart) {
      clearTimer();
      return;
    }

    const end = typeof endDate === "string" ? new Date(endDate) : endDate;
    const currentTimeLeft = calculateTimeLeft(end);
    
    if (currentTimeLeft.isExpired) {
      if (!hasExpiredRef.current) {
        hasExpiredRef.current = true;
        setTimeLeft(currentTimeLeft);
        onExpire?.();
      }
      return;
    }

    hasExpiredRef.current = false;

    const tick = () => {
      const newTimeLeft = calculateTimeLeft(end);
      setTimeLeft(newTimeLeft);

      if (newTimeLeft.isExpired && !hasExpiredRef.current) {
        hasExpiredRef.current = true;
        clearTimer();
        onExpire?.();
      }
    };

    intervalRef.current = setInterval(tick, interval);

    return () => {
      clearTimer();
    };
  }, [endDate, interval, autoStart, onExpire, clearTimer]);

  return timeLeft;
}

export function formatTimeLeft(timeLeft: TimeLeft | null): string {
  if (!timeLeft || timeLeft.isExpired) {
    return "Ended";
  }

  const pad = (n: number) => String(n).padStart(2, "0");

  if (timeLeft.days > 0) {
    return `${timeLeft.days}d ${pad(timeLeft.hours)}:${pad(timeLeft.minutes)}:${pad(timeLeft.seconds)}`;
  }

  return `${pad(timeLeft.hours)}:${pad(timeLeft.minutes)}:${pad(timeLeft.seconds)}`;
}

export function getUrgencyLevel(
  timeLeft: TimeLeft | null
): "expired" | "critical" | "warning" | "normal" {
  if (!timeLeft || timeLeft.isExpired) {
    return "expired";
  }

  const hoursRemaining = timeLeft.days * 24 + timeLeft.hours;

  if (hoursRemaining < 1) {
    return "critical";
  }

  if (hoursRemaining < 24) {
    return "warning";
  }

  return "normal";
}
