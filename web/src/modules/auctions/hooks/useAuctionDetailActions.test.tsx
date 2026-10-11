import type * as ReactI18next from 'react-i18next'
import { act, renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter, useLocation } from 'react-router-dom'
import type { PropsWithChildren } from 'react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { auctionsApi } from '../api'
import { biddingApi } from '@/modules/bidding/api'
import { useAuctionDetailActions } from './useAuctionDetailActions'
import type { BuyNowPurchase } from '../types/backend-dto.types'

vi.mock('../api', () => ({ auctionsApi: { buyNow: vi.fn(), getMyPurchase: vi.fn() } }))
vi.mock('@/modules/bidding/api', () => ({ biddingApi: { placeBid: vi.fn() } }))
vi.mock('@/app/hooks/useAuth', () => ({
  useAuth: () => ({ isAuthenticated: true, user: { id: 'buyer' } }),
}))
vi.mock('./useBookmarks', () => ({
  useIsInWatchlist: () => ({ data: false }),
  useToggleWatchlist: () => ({ mutate: vi.fn() }),
}))
vi.mock('react-i18next', async (importOriginal) => ({
  ...(await importOriginal<typeof ReactI18next>()),
  useTranslation: () => ({ t: translate }),
}))
function translate(key: string) {
  return key
}

function receipt(status: BuyNowPurchase['status']): BuyNowPurchase {
  return {
    auctionId: 'a',
    correlationId: 'c',
    orderId: status === 'Completed' ? 'order' : '',
    status,
    success: status === 'Completed',
  }
}
function setup() {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })
  const wrapper = ({ children }: PropsWithChildren) => (
    <MemoryRouter initialEntries={['/auctions/a']}>
      <QueryClientProvider client={client}>{children}</QueryClientProvider>
    </MemoryRouter>
  )
  return {
    client,
    ...renderHook(() => ({ actions: useAuctionDetailActions('a'), path: useLocation().pathname }), {
      wrapper,
    }),
  }
}

beforeEach(() => {
  vi.mocked(auctionsApi.getMyPurchase).mockReset().mockResolvedValue(null)
  vi.mocked(auctionsApi.buyNow).mockReset().mockResolvedValue(receipt('Processing'))
  vi.mocked(biddingApi.placeBid).mockReset()
})

describe('auction purchase actions with real query hooks', () => {
  it('shows rejected bids and never reports success', async () => {
    vi.mocked(biddingApi.placeBid).mockRejectedValue(new Error('Auction has closed'))
    const view = setup()
    await act(async () => {
      await expect(view.result.current.actions.handlePlaceBid(50)).rejects.toThrow(
        'Auction has closed'
      )
    })
    expect(view.result.current.actions.snackbar).toMatchObject({
      severity: 'error',
      message: 'Auction has closed',
    })
  })

  it.each(['Completed', 'Failed'] as const)(
    'polls a processing purchase until %s',
    async (terminal) => {
      let current = receipt('Processing')
      vi.mocked(auctionsApi.buyNow).mockImplementation(async () => {
        vi.mocked(auctionsApi.getMyPurchase).mockImplementation(async () => current)
        return current
      })
      const view = setup()
      await waitFor(() => expect(auctionsApi.getMyPurchase).toHaveBeenCalled())
      act(() => view.result.current.actions.handleBuyNow())
      await act(async () => view.result.current.actions.confirmBuyNow())
      await waitFor(() =>
        expect(view.result.current.actions.purchaseStatus?.status).toBe('Processing')
      )
      expect(view.result.current.actions.buyNowDialogOpen).toBe(false)
      expect(view.result.current.path).toBe('/auctions/a')
      current = receipt(terminal)
      await waitFor(
        () => expect(view.result.current.actions.purchaseStatus?.status).toBe(terminal),
        { timeout: 2500 }
      )
      expect(view.result.current.actions.snackbar.severity).toBe(
        terminal === 'Completed' ? 'success' : 'error'
      )
      if (terminal === 'Completed') {
        await waitFor(() => expect(view.result.current.path).toBe('/orders'), { timeout: 3000 })
      } else {
        expect(view.result.current.path).toBe('/auctions/a')
      }
      const calls = vi.mocked(auctionsApi.getMyPurchase).mock.calls.length
      await new Promise((resolve) => setTimeout(resolve, 1100))
      expect(auctionsApi.getMyPurchase).toHaveBeenCalledTimes(calls)
    },
    10000
  )

  it('handles a rejected buy-now request without redirecting', async () => {
    vi.mocked(auctionsApi.buyNow).mockRejectedValue(new Error('Already reserved'))
    const view = setup()
    await act(async () => view.result.current.actions.confirmBuyNow())
    expect(view.result.current.actions.snackbar).toMatchObject({
      severity: 'error',
      message: 'messages.purchaseFailed',
    })
    expect(view.result.current.path).toBe('/auctions/a')
  })
})
