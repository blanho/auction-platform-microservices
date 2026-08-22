import { usePlaceBid } from '@/modules/bidding/hooks/useBidding'
import { useSnackbar } from '@/shared/hooks/useSnackbar'
import { useCallback, useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { useBuyNow } from './useAuctions'
import { useIsInWatchlist, useToggleWatchlist } from './useBookmarks'

interface UseAuctionDetailActionsReturn {
  snackbar: ReturnType<typeof useSnackbar>
  buyNowDialogOpen: boolean
  setBuyNowDialogOpen: (open: boolean) => void
  buyNowMutation: ReturnType<typeof useBuyNow>
  isInWatchlist: boolean
  handleToggleFavorite: () => void
  handleShare: () => Promise<void>
  handlePlaceBid: (amount: number) => Promise<void>
  handleBuyNow: () => void
  confirmBuyNow: () => Promise<void>
  handleSellerContact: () => void
}

export function useAuctionDetailActions(
  auctionId: string | undefined
): UseAuctionDetailActionsReturn {
  const { t } = useTranslation('auctions')
  const navigate = useNavigate()
  const snackbar = useSnackbar()

  const { data: isInWatchlist = false } = useIsInWatchlist(auctionId ?? '')
  const toggleWatchlistMutation = useToggleWatchlist()
  const buyNowMutation = useBuyNow()
  const placeBidMutation = usePlaceBid()
  const [buyNowDialogOpen, setBuyNowDialogOpen] = useState(false)
  const orderRedirectTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  useEffect(
    () => () => {
      if (orderRedirectTimerRef.current) {
        clearTimeout(orderRedirectTimerRef.current)
      }
    },
    []
  )

  const handleToggleFavorite = useCallback(() => {
    if (!auctionId) {
      return
    }

    toggleWatchlistMutation.mutate(
      { auctionId, isInWatchlist },
      {
        onSuccess: () =>
          snackbar.show(
            isInWatchlist ? t('messages.removedFromWatchlist') : t('messages.addedToWatchlist'),
            'success'
          ),
        onError: () => snackbar.show(t('watchlist.updateFailed'), 'error'),
      }
    )
  }, [auctionId, isInWatchlist, toggleWatchlistMutation, snackbar, t])

  const handleShare = useCallback(async () => {
    try {
      await navigator.clipboard.writeText(globalThis.location.href)
      snackbar.show(t('messages.linkCopied'), 'success')
    } catch {
      snackbar.show(t('messages.shareFailed'), 'error')
    }
  }, [snackbar, t])

  const handlePlaceBid = useCallback(
    async (amount: number) => {
      if (!auctionId) {
        return
      }

      try {
        await placeBidMutation.mutateAsync({ auctionId, amount })
        snackbar.show(t('messages.bidPlaced'), 'success')
      } catch (error) {
        snackbar.show(error instanceof Error ? error.message : t('messages.bidFailed'), 'error')
        throw error
      }
    },
    [auctionId, placeBidMutation, snackbar, t]
  )

  const handleBuyNow = useCallback(() => {
    setBuyNowDialogOpen(true)
  }, [])

  const confirmBuyNow = useCallback(async () => {
    if (!auctionId) {
      return
    }

    try {
      await buyNowMutation.mutateAsync(auctionId)
      setBuyNowDialogOpen(false)
      snackbar.show(t('messages.purchaseSuccess'), 'success')
      orderRedirectTimerRef.current = setTimeout(() => navigate('/orders'), 2000)
    } catch {
      setBuyNowDialogOpen(false)
      snackbar.show(t('messages.purchaseFailed'), 'error')
    }
  }, [auctionId, buyNowMutation, navigate, snackbar, t])

  const handleSellerContact = useCallback(() => {
    snackbar.show(t('messages.openingChat'), 'info')
  }, [snackbar, t])

  return {
    snackbar,
    buyNowDialogOpen,
    setBuyNowDialogOpen,
    buyNowMutation,
    isInWatchlist,
    handleToggleFavorite,
    handleShare,
    handlePlaceBid,
    handleBuyNow,
    confirmBuyNow,
    handleSellerContact,
  }
}
