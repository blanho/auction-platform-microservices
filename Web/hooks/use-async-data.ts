"use client";

import { useState, useEffect, useCallback, useRef } from "react";

export interface UseAsyncDataOptions<T> {
  enabled?: boolean;
  onSuccess?: (data: T) => void;
  onError?: (error: Error) => void;
  initialData?: T | null;
}

export interface UseAsyncDataResult<T> {
  data: T | null;
  isLoading: boolean;
  error: Error | null;
  refetch: () => Promise<void>;
}

export function useAsyncData<T>(
  fetchFn: () => Promise<T>,
  deps: unknown[] = [],
  options: UseAsyncDataOptions<T> = {}
): UseAsyncDataResult<T> {
  const { enabled = true, onSuccess, onError, initialData = null } = options;
  
  const [data, setData] = useState<T | null>(initialData);
  const [isLoading, setIsLoading] = useState(enabled);
  const [error, setError] = useState<Error | null>(null);
  const isMountedRef = useRef(true);

  const fetchData = useCallback(async () => {
    if (!enabled) return;
    
    setIsLoading(true);
    setError(null);
    
    try {
      const result = await fetchFn();
      if (isMountedRef.current) {
        setData(result);
        onSuccess?.(result);
      }
    } catch (err) {
      if (isMountedRef.current) {
        const error = err instanceof Error ? err : new Error(String(err));
        setError(error);
        onError?.(error);
      }
    } finally {
      if (isMountedRef.current) {
        setIsLoading(false);
      }
    }
  }, [enabled, fetchFn, onSuccess, onError]);

  useEffect(() => {
    isMountedRef.current = true;
    fetchData();
    
    return () => {
      isMountedRef.current = false;
    };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...deps, enabled]);

  const refetch = useCallback(async () => {
    await fetchData();
  }, [fetchData]);

  return { data, isLoading, error, refetch };
}

export function useParallelData<T extends Record<string, unknown>>(
  fetchers: { [K in keyof T]: () => Promise<T[K]> },
  deps: unknown[] = [],
  options: { enabled?: boolean } = {}
): {
  data: Partial<T>;
  isLoading: boolean;
  errors: Partial<Record<keyof T, Error>>;
  refetch: () => Promise<void>;
} {
  const { enabled = true } = options;
  
  const [data, setData] = useState<Partial<T>>({});
  const [isLoading, setIsLoading] = useState(enabled);
  const [errors, setErrors] = useState<Partial<Record<keyof T, Error>>>({});
  const isMountedRef = useRef(true);

  const fetchAll = useCallback(async () => {
    if (!enabled) return;
    
    setIsLoading(true);
    
    const keys = Object.keys(fetchers) as (keyof T)[];
    const results = await Promise.allSettled(
      keys.map(key => fetchers[key]())
    );
    
    if (!isMountedRef.current) return;
    
    const newData: Partial<T> = {};
    const newErrors: Partial<Record<keyof T, Error>> = {};
    
    results.forEach((result, index) => {
      const key = keys[index];
      if (result.status === "fulfilled") {
        newData[key] = result.value;
      } else {
        newErrors[key] = result.reason instanceof Error 
          ? result.reason 
          : new Error(String(result.reason));
      }
    });
    
    setData(newData);
    setErrors(newErrors);
    setIsLoading(false);
  }, [enabled, fetchers]);

  useEffect(() => {
    isMountedRef.current = true;
    fetchAll();
    
    return () => {
      isMountedRef.current = false;
    };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...deps, enabled]);

  return { data, isLoading, errors, refetch: fetchAll };
}
