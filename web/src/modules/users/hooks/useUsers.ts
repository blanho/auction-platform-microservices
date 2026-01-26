import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { usersApi } from '../api'
import type {
  UpdateProfileRequest,
  ChangePasswordRequest,
  UserFilters,
  UpdateUserRolesRequest,
} from '../types'

export const userKeys = {
  all: ['users'] as const,
  profile: () => [...userKeys.all, 'profile'] as const,
  sellerStatus: () => [...userKeys.all, 'seller-status'] as const,
  lists: () => [...userKeys.all, 'list'] as const,
  list: (filters: UserFilters) => [...userKeys.lists(), filters] as const,
  detail: (id: string) => [...userKeys.all, 'detail', id] as const,
  stats: () => [...userKeys.all, 'stats'] as const,
  user2FAStatus: (id: string) => [...userKeys.all, '2fa-status', id] as const,
}

export const useProfile = () => {
  return useQuery({
    queryKey: userKeys.profile(),
    queryFn: () => usersApi.getProfile(),
  })
}

export const useUpdateProfile = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: UpdateProfileRequest) => usersApi.updateProfile(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userKeys.profile() })
    },
  })
}

export const useChangePassword = () => {
  return useMutation({
    mutationFn: (data: ChangePasswordRequest) => usersApi.changePassword(data),
  })
}

export const useEnableTwoFactor = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () => usersApi.enableTwoFactor(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userKeys.profile() })
    },
  })
}

export const useDisableTwoFactor = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () => usersApi.disableTwoFactor(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userKeys.profile() })
    },
  })
}

export const useSellerStatus = () => {
  return useQuery({
    queryKey: userKeys.sellerStatus(),
    queryFn: () => usersApi.getSellerStatus(),
  })
}

export const useApplyForSeller = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (acceptTerms: boolean) => usersApi.applyForSeller(acceptTerms),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userKeys.sellerStatus() })
    },
  })
}

export const useUsers = (filters: UserFilters) => {
  return useQuery({
    queryKey: userKeys.list(filters),
    queryFn: () => usersApi.getUsers(filters),
  })
}

export const useUser = (id: string) => {
  return useQuery({
    queryKey: userKeys.detail(id),
    queryFn: () => usersApi.getUserById(id),
    enabled: !!id,
  })
}

export const useSuspendUser = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) => usersApi.suspendUser(id, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userKeys.lists() })
    },
  })
}

export const useUnsuspendUser = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => usersApi.unsuspendUser(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userKeys.lists() })
    },
  })
}

export const useUploadAvatar = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (file: File) => usersApi.uploadAvatar(file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userKeys.profile() })
    },
  })
}

export const useActivateUser = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => usersApi.activateUser(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: userKeys.lists() })
      queryClient.invalidateQueries({ queryKey: userKeys.detail(id) })
    },
  })
}

export const useDeactivateUser = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => usersApi.deactivateUser(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: userKeys.lists() })
      queryClient.invalidateQueries({ queryKey: userKeys.detail(id) })
    },
  })
}

export const useUpdateUserRoles = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateUserRolesRequest }) =>
      usersApi.updateUserRoles(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: userKeys.lists() })
      queryClient.invalidateQueries({ queryKey: userKeys.detail(id) })
    },
  })
}

export const useDeleteUser = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => usersApi.deleteUser(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userKeys.lists() })
    },
  })
}

export const useUserStats = () => {
  return useQuery({
    queryKey: userKeys.stats(),
    queryFn: () => usersApi.getUserStats(),
    staleTime: 60000,
  })
}

export const useUser2FAStatus = (id: string) => {
  return useQuery({
    queryKey: userKeys.user2FAStatus(id),
    queryFn: () => usersApi.getUser2FAStatus(id),
    enabled: !!id,
  })
}

export const useResetUser2FA = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => usersApi.resetUser2FA(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: userKeys.user2FAStatus(id) })
      queryClient.invalidateQueries({ queryKey: userKeys.detail(id) })
    },
  })
}

export const useDisableUser2FA = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => usersApi.disableUser2FA(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: userKeys.user2FAStatus(id) })
      queryClient.invalidateQueries({ queryKey: userKeys.detail(id) })
    },
  })
}
