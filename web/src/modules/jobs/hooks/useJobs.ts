import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { jobsApi } from '../api'
import type { JobFilterParams, JobItemFilterParams } from '../types'

export const jobKeys = {
  all: ['jobs'] as const,
  lists: () => [...jobKeys.all, 'list'] as const,
  list: (params?: JobFilterParams) => [...jobKeys.lists(), params] as const,
  details: () => [...jobKeys.all, 'detail'] as const,
  detail: (id: string) => [...jobKeys.details(), id] as const,
  items: (jobId: string) => [...jobKeys.all, 'items', jobId] as const,
  itemList: (jobId: string, params?: JobItemFilterParams) =>
    [...jobKeys.items(jobId), params] as const,
}

const ACTIVE_POLL_INTERVAL = 3000
const IDLE_POLL_INTERVAL = 30000

const ACTIVE_STATUSES = new Set(['Initializing', 'Pending', 'Processing'])

export const useJobs = (params?: JobFilterParams) => {
  return useQuery({
    queryKey: jobKeys.list(params),
    queryFn: () => jobsApi.getJobs(params),
    staleTime: 10000,
  })
}

export const useJobsPolling = (params?: JobFilterParams, hasActiveJobs = false) => {
  return useQuery({
    queryKey: jobKeys.list(params),
    queryFn: () => jobsApi.getJobs(params),
    refetchInterval: hasActiveJobs ? ACTIVE_POLL_INTERVAL : IDLE_POLL_INTERVAL,
    staleTime: 5000,
  })
}

export const useJob = (id: string) => {
  return useQuery({
    queryKey: jobKeys.detail(id),
    queryFn: () => jobsApi.getJob(id),
    enabled: Boolean(id),
    refetchInterval: (query) => {
      const status = query.state.data?.status
      if (status && ACTIVE_STATUSES.has(status)) {
        return ACTIVE_POLL_INTERVAL
      }
      return false
    },
  })
}

export const useJobItems = (jobId: string, params?: JobItemFilterParams) => {
  return useQuery({
    queryKey: jobKeys.itemList(jobId, params),
    queryFn: () => jobsApi.getJobItems(jobId, params),
    enabled: Boolean(jobId),
  })
}

export const useCancelJob = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => jobsApi.cancelJob(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: jobKeys.all })
    },
  })
}

export const useRetryJob = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => jobsApi.retryJob(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: jobKeys.all })
    },
  })
}
