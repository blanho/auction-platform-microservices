import { useMutation, useQueryClient } from '@tanstack/react-query'
import { settingsApi } from '../api'
import type { CreateSettingRequest, UpdateSettingRequest, BulkUpdateSettingsRequest } from '../types'
import { analyticsKeys } from './useAnalytics'

export const useCreateSetting = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateSettingRequest) => settingsApi.createSetting(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: analyticsKeys.settings() })
    },
  })
}

export const useUpdateSetting = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSettingRequest }) =>
      settingsApi.updateSetting(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: analyticsKeys.settings() })
    },
  })
}

export const useDeleteSetting = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => settingsApi.deleteSetting(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: analyticsKeys.settings() })
    },
  })
}

export const useBulkUpdateSettings = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: BulkUpdateSettingsRequest) => settingsApi.bulkUpdateSettings(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: analyticsKeys.settings() })
    },
  })
}
