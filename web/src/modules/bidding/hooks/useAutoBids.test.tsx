import { act, renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import type { PropsWithChildren } from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { biddingApi } from '../api'
import type { AutoBidDetail } from '../types'
import {
  useAutoBidForAuction,
  useCancelAutoBid,
  useCreateAutoBid,
  useToggleAutoBid,
  useUpdateAutoBid,
} from './useAutoBids'

const auctionId = 'b635d0cd-e315-4c68-a1e4-6d71953f8789'

afterEach(() => vi.restoreAllMocks())

describe('auction auto-bid cache', () => {
  it.each(['create', 'update', 'toggle', 'cancel'] as const)(
    'refreshes the displayed auto-bid after %s succeeds',
    async (action) => {
      const initialBid: AutoBidDetail = {
        id: 'auto-bid',
        auctionId,
        userId: 'buyer',
        username: 'buyer',
        maxAmount: 100,
        currentBidAmount: 0,
        isActive: true,
        createdAt: '2026-01-01T00:00:00Z',
        auctionTitle: 'Auction',
        auctionStatus: 'Live',
        auctionEndTime: '2026-01-02T00:00:00Z',
        currentHighestBid: 0,
        remainingAmount: 100,
        totalBidsPlaced: 0,
      }
      const increasedBid = { ...initialBid, maxAmount: 200, remainingAmount: 200 }
      const initialData = action === 'create' ? null : initialBid
      const expectedData = {
        create: increasedBid,
        update: increasedBid,
        toggle: null,
        cancel: null,
      }[action]
      const fetchBid = vi.spyOn(biddingApi, 'getAutoBidForAuction').mockResolvedValue(initialData)
      vi.spyOn(biddingApi, 'createAutoBid').mockResolvedValue({
        autoBidId: 'auto-bid',
        message: '',
      })
      vi.spyOn(biddingApi, 'updateAutoBid').mockResolvedValue({ success: true, message: '' })
      vi.spyOn(biddingApi, 'toggleAutoBid').mockResolvedValue({ isActive: false, message: '' })
      vi.spyOn(biddingApi, 'cancelAutoBid').mockResolvedValue({ success: true, message: '' })

      const client = new QueryClient({
        defaultOptions: { queries: { retry: false, staleTime: Infinity } },
      })
      const wrapper = ({ children }: PropsWithChildren) => (
        <QueryClientProvider client={client}>{children}</QueryClientProvider>
      )
      const view = renderHook(
        () => ({
          bid: useAutoBidForAuction(auctionId),
          create: useCreateAutoBid(),
          update: useUpdateAutoBid(),
          toggle: useToggleAutoBid(),
          cancel: useCancelAutoBid(),
        }),
        { wrapper }
      )

      try {
        await waitFor(() => expect(view.result.current.bid.isSuccess).toBe(true))
        expect(view.result.current.bid.data).toEqual(initialData)
        fetchBid.mockResolvedValue(expectedData)

        await act(async () => {
          switch (action) {
            case 'create':
              await view.result.current.create.mutateAsync({ auctionId, maxAmount: 200 })
              break
            case 'update':
              await view.result.current.update.mutateAsync({
                autoBidId: initialBid.id,
                data: { maxAmount: 200 },
              })
              break
            case 'toggle':
              await view.result.current.toggle.mutateAsync({
                autoBidId: initialBid.id,
                activate: false,
              })
              break
            case 'cancel':
              await view.result.current.cancel.mutateAsync(initialBid.id)
          }
        })

        await waitFor(() => expect(view.result.current.bid.data).toEqual(expectedData))
      } finally {
        view.unmount()
        client.clear()
      }
    }
  )
})
