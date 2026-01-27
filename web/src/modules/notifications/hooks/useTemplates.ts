import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { templatesApi } from '../api'
import type { CreateTemplateDto, UpdateTemplateDto } from '../types/template.types'

export const templateKeys = {
  all: ['notification-templates'] as const,
  lists: () => [...templateKeys.all, 'list'] as const,
  list: (page: number, pageSize: number) => [...templateKeys.lists(), { page, pageSize }] as const,
  active: () => [...templateKeys.all, 'active'] as const,
  byKey: (key: string) => [...templateKeys.all, 'by-key', key] as const,
  byId: (id: string) => [...templateKeys.all, 'by-id', id] as const,
}

export const useTemplates = (page: number = 1, pageSize: number = 20) => {
  return useQuery({
    queryKey: templateKeys.list(page, pageSize),
    queryFn: () => templatesApi.getTemplates(page, pageSize),
  })
}

export const useActiveTemplates = () => {
  return useQuery({
    queryKey: templateKeys.active(),
    queryFn: () => templatesApi.getActiveTemplates(),
  })
}

export const useTemplateByKey = (key: string) => {
  return useQuery({
    queryKey: templateKeys.byKey(key),
    queryFn: () => templatesApi.getTemplateByKey(key),
    enabled: !!key,
  })
}

export const useTemplateById = (id: string) => {
  return useQuery({
    queryKey: templateKeys.byId(id),
    queryFn: () => templatesApi.getTemplateById(id),
    enabled: !!id,
  })
}

export const useCreateTemplate = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (dto: CreateTemplateDto) => templatesApi.createTemplate(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: templateKeys.all })
    },
  })
}

export const useUpdateTemplate = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, dto }: { id: string; dto: UpdateTemplateDto }) =>
      templatesApi.updateTemplate(id, dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: templateKeys.all })
    },
  })
}

export const useDeleteTemplate = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => templatesApi.deleteTemplate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: templateKeys.all })
    },
  })
}

export const useCheckTemplateExists = (key: string) => {
  return useQuery({
    queryKey: [...templateKeys.all, 'exists', key],
    queryFn: () => templatesApi.checkTemplateExists(key),
    enabled: !!key,
  })
}
