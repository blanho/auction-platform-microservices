"use client";

import { useEffect, useRef, useCallback, useState } from "react";

export function useInterval(
  callback: () => void,
  delay: number | null,
  options: { immediate?: boolean } = {}
): void {
  const { immediate = false } = options;
  const savedCallback = useRef(callback);
  const intervalRef = useRef<NodeJS.Timeout | null>(null);

  useEffect(() => {
    savedCallback.current = callback;
  }, [callback]);

  useEffect(() => {
    if (delay === null) {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
      return;
    }

    if (immediate) {
      savedCallback.current();
    }

    intervalRef.current = setInterval(() => {
      savedCallback.current();
    }, delay);

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
    };
  }, [delay, immediate]);
}

export function useTimeout(
  callback: () => void,
  delay: number | null
): { reset: () => void; clear: () => void } {
  const savedCallback = useRef(callback);
  const timeoutRef = useRef<NodeJS.Timeout | null>(null);

  useEffect(() => {
    savedCallback.current = callback;
  }, [callback]);

  const clear = useCallback(() => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
      timeoutRef.current = null;
    }
  }, []);

  const reset = useCallback(() => {
    clear();
    if (delay !== null) {
      timeoutRef.current = setTimeout(() => {
        savedCallback.current();
      }, delay);
    }
  }, [delay, clear]);

  useEffect(() => {
    if (delay === null) return;

    timeoutRef.current = setTimeout(() => {
      savedCallback.current();
    }, delay);

    return clear;
  }, [delay, clear]);

  return { reset, clear };
}

export function useCarouselInterval(
  itemCount: number,
  interval: number,
  options: { enabled?: boolean; onIndexChange?: (index: number) => void } = {}
): {
  activeIndex: number;
  setActiveIndex: (index: number) => void;
  next: () => void;
  prev: () => void;
  pause: () => void;
  resume: () => void;
} {
  const { enabled = true, onIndexChange } = options;
  const [activeIndex, setActiveIndexState] = useState(0);
  const [isPaused, setIsPaused] = useState(false);
  const savedOnIndexChange = useRef(onIndexChange);

  useEffect(() => {
    savedOnIndexChange.current = onIndexChange;
  }, [onIndexChange]);

  const setActiveIndex = useCallback((index: number) => {
    setActiveIndexState(index);
    savedOnIndexChange.current?.(index);
  }, []);

  const next = useCallback(() => {
    if (itemCount === 0) return;
    setActiveIndex((activeIndex + 1) % itemCount);
  }, [activeIndex, itemCount, setActiveIndex]);

  const prev = useCallback(() => {
    if (itemCount === 0) return;
    setActiveIndex((activeIndex - 1 + itemCount) % itemCount);
  }, [activeIndex, itemCount, setActiveIndex]);

  const pause = useCallback(() => setIsPaused(true), []);
  const resume = useCallback(() => setIsPaused(false), []);

  useInterval(
    next,
    enabled && !isPaused && itemCount > 1 ? interval : null
  );

  return { activeIndex, setActiveIndex, next, prev, pause, resume };
}
