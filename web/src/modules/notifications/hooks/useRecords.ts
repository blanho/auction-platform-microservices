import { useQuery } from '@tanstack/react-query'
import { recordsApi } from '../api'
import type { NotificationRecordFilterDto } from '../types/template.types'

export const recordKeys = {
  all: ['notification-records'] as const,
  lists: () => [...recordKeys.all, 'list'] as const,
  list: (filter: NotificationRecordFilterDto) => [...recordKeys.lists(), filter] as const,
  byId: (id: string) => [...recordKeys.all, 'by-id', id] as const,
  byUser: (userId: string, limit: number) => [...recordKeys.all, 'by-user', userId, limit] as const,
  stats: () => [...recordKeys.all, 'stats'] as const,
}

export const useRecords = (filter: NotificationRecordFilterDto) => {
  return useQuery({
    queryKey: recordKeys.list(filter),
    queryFn: () => recordsApi.getRecords(filter),
  })
}

export const useRecordById = (id: string) => {
  return useQuery({
    queryKey: recordKeys.byId(id),
    queryFn: () => recordsApi.getRecordById(id),
    enabled: !!id,
  })
}

export const useRecordsByUser = (userId: string, limit: number = 50) => {
  return useQuery({
    queryKey: recordKeys.byUser(userId, limit),
    queryFn: () => recordsApi.getRecordsByUser(userId, limit),
    enabled: !!userId,
  })
}

export const useRecordStats = () => {
  return useQuery({
    queryKey: recordKeys.stats(),
    queryFn: () => recordsApi.getRecordStats(),
  })
}
