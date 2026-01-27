import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { bookmarksApi } from '../api/bookmarks.api'
import type { AddToWatchlistRequest, UpdateWatchlistRequest, WatchlistFilters } from '../api/bookmarks.api'

export const bookmarkKeys = {
  all: ['bookmarks'] as const,
  watchlist: () => [...bookmarkKeys.all, 'watchlist'] as const,
  watchlistFiltered: (filters: WatchlistFilters) =>
    [...bookmarkKeys.watchlist(), filters] as const,
  check: (auctionId: string) => [...bookmarkKeys.all, 'check', auctionId] as const,
  count: () => [...bookmarkKeys.all, 'count'] as const,
}

export const useWatchlist = (filters?: WatchlistFilters) => {
  return useQuery({
    queryKey: bookmarkKeys.watchlistFiltered(filters || {}),
    queryFn: () => bookmarksApi.getWatchlist(filters),
  })
}

export const useWatchlistCount = () => {
  return useQuery({
    queryKey: bookmarkKeys.count(),
    queryFn: () => bookmarksApi.getWatchlistCount(),
  })
}

export const useIsInWatchlist = (auctionId: string) => {
  return useQuery({
    queryKey: bookmarkKeys.check(auctionId),
    queryFn: () => bookmarksApi.isInWatchlist(auctionId),
    enabled: !!auctionId,
  })
}

export const useAddToWatchlist = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: AddToWatchlistRequest) => bookmarksApi.addToWatchlist(data),
    onSuccess: (_, { auctionId }) => {
      queryClient.invalidateQueries({ queryKey: bookmarkKeys.watchlist() })
      queryClient.invalidateQueries({ queryKey: bookmarkKeys.count() })
      queryClient.setQueryData(bookmarkKeys.check(auctionId), true)
    },
  })
}

export const useRemoveFromWatchlist = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (auctionId: string) => bookmarksApi.removeFromWatchlist(auctionId),
    onSuccess: (_, auctionId) => {
      queryClient.invalidateQueries({ queryKey: bookmarkKeys.watchlist() })
      queryClient.invalidateQueries({ queryKey: bookmarkKeys.count() })
      queryClient.setQueryData(bookmarkKeys.check(auctionId), false)
    },
  })
}

export const useToggleWatchlist = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ auctionId, isInWatchlist }: { auctionId: string; isInWatchlist: boolean }) => {
      if (isInWatchlist) {
        return bookmarksApi.removeFromWatchlist(auctionId)
      }
      return bookmarksApi.addToWatchlist({ auctionId })
    },
    onSuccess: (_, { auctionId, isInWatchlist }) => {
      queryClient.invalidateQueries({ queryKey: bookmarkKeys.watchlist() })
      queryClient.invalidateQueries({ queryKey: bookmarkKeys.count() })
      queryClient.setQueryData(bookmarkKeys.check(auctionId), !isInWatchlist)
    },
  })
}

export const useUpdateWatchlistNotifications = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ auctionId, data }: { auctionId: string; data: UpdateWatchlistRequest }) =>
      bookmarksApi.updateNotificationSettings(auctionId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: bookmarkKeys.watchlist() })
    },
  })
}
