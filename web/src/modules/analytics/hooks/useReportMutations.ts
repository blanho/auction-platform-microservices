import { useMutation, useQueryClient } from '@tanstack/react-query'
import { reportsApi } from '../api'
import type { CreateReportRequest, UpdateReportStatusRequest } from '../types'
import { analyticsKeys } from './useAnalytics'

export const useCreateReport = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateReportRequest) => reportsApi.createReport(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: analyticsKeys.reports() })
    },
  })
}

export const useUpdateReportStatus = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateReportStatusRequest }) =>
      reportsApi.updateReportStatus(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: analyticsKeys.reportDetail(id) })
      queryClient.invalidateQueries({ queryKey: analyticsKeys.reports() })
    },
  })
}

export const useDeleteReport = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => reportsApi.deleteReport(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: analyticsKeys.reports() })
    },
  })
}
