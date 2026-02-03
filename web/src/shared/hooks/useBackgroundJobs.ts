import { useState, useCallback, useEffect, useRef } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { backgroundJobsApi } from '@/services/background-jobs.api'
import type {
  BackgroundJobProgress,
  ExportJobRequest,
  ReportJobRequest,
} from '@/shared/types/background-jobs.types'

const POLL_INTERVAL = 1000

export function useBackgroundJobs(type?: string) {
  return useQuery({
    queryKey: ['background-jobs', type],
    queryFn: () => backgroundJobsApi.getMyJobs(type),
    refetchInterval: 5000,
  })
}

export function useJobProgress(jobId: string | null) {
  const [progress, setProgress] = useState<BackgroundJobProgress | null>(null)
  const pollIntervalRef = useRef<ReturnType<typeof setInterval> | null>(null)

  const stopPolling = useCallback(() => {
    if (pollIntervalRef.current) {
      clearInterval(pollIntervalRef.current)
      pollIntervalRef.current = null
    }
  }, [])

  const fetchProgress = useCallback(async () => {
    if (!jobId) return
    try {
      const data = await backgroundJobsApi.getJobStatus(jobId)
      setProgress(data)
      if (['Completed', 'Failed', 'Cancelled'].includes(data.status)) {
        stopPolling()
      }
    } catch (error) {
      console.error('Failed to fetch job progress:', error)
    }
  }, [jobId, stopPolling])

  useEffect(() => {
    if (!jobId) {
      setProgress(null)
      return
    }

    fetchProgress()
    pollIntervalRef.current = setInterval(fetchProgress, POLL_INTERVAL)

    return () => {
      stopPolling()
    }
  }, [jobId, fetchProgress, stopPolling])

  return { progress, stopPolling }
}

export function useStartExportJob() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: ExportJobRequest) => backgroundJobsApi.startExportJob(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['background-jobs'] })
    },
  })
}

export function useStartReportJob() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: ReportJobRequest) => backgroundJobsApi.startReportJob(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['background-jobs'] })
    },
  })
}

export function useCancelJob() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (jobId: string) => backgroundJobsApi.cancelJob(jobId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['background-jobs'] })
    },
  })
}

export function useDownloadJobResult() {
  return useMutation({
    mutationFn: async ({ jobId, filename }: { jobId: string; filename: string }) => {
      const blob = await backgroundJobsApi.downloadJobResult(jobId)
      backgroundJobsApi.downloadFile(blob, filename)
    },
  })
}
