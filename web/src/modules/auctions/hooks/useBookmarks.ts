import { useAuth } from '@/app/hooks/useAuth'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { WatchlistFilters } from '../api/bookmarks.api'
import { bookmarksApi } from '../api/bookmarks.api'

export const bookmarkKeys = {
  all: ['bookmarks'] as const,
  watchlist: () => [...bookmarkKeys.all, 'watchlist'] as const,
  watchlistFiltered: (filters: WatchlistFilters) => [...bookmarkKeys.watchlist(), filters] as const,
  check: (auctionId: string) => [...bookmarkKeys.all, 'check', auctionId] as const,
  count: () => [...bookmarkKeys.all, 'count'] as const,
}

export const useWatchlist = (filters?: WatchlistFilters) => {
  const { isAuthenticated } = useAuth()
  return useQuery({
    queryKey: bookmarkKeys.watchlistFiltered(filters || {}),
    queryFn: () => bookmarksApi.getWatchlist(filters),
    enabled: isAuthenticated,
  })
}

export const useWatchlistCount = () => {
  const { isAuthenticated } = useAuth()
  return useQuery({
    queryKey: bookmarkKeys.count(),
    queryFn: () => bookmarksApi.getWatchlistCount(),
    enabled: isAuthenticated,
  })
}

export const useIsInWatchlist = (auctionId: string) => {
  const { isAuthenticated } = useAuth()
  return useQuery({
    queryKey: bookmarkKeys.check(auctionId),
    queryFn: () => bookmarksApi.isInWatchlist(auctionId),
    enabled: !!auctionId && isAuthenticated,
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
    mutationFn: async ({
      auctionId,
      isInWatchlist,
    }: {
      auctionId: string
      isInWatchlist: boolean
    }) => {
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
