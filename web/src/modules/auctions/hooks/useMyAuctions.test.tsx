import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { act, renderHook, waitFor } from '@testing-library/react'
import type { PropsWithChildren } from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { auctionsApi } from '../api'
import type { AuctionFilters, AuctionListItem } from '../types'
import {
  useActivateAuction,
  useCancelAuction,
  useCreateAuction,
  useDeactivateAuction,
  useDeleteAuction,
  useExtendAuction,
  useMyAuctions,
  useUpdateAuction,
} from './useAuctions'

afterEach(() => vi.restoreAllMocks())

describe('seller auction updates', () => {
  const actions = [
    'create',
    'update',
    'activate',
    'deactivate',
    'delete',
    'cancel',
    'extend',
  ] as const
  const cases = actions.flatMap((action) => [
    { action, filtered: false },
    { action, filtered: true },
  ])

  it.each(cases)(
    'refreshes the seller list after $action (filtered=$filtered)',
    async ({ action, filtered }) => {
      const initial: AuctionListItem = {
        id: 'auction-123',
        title: 'Vintage watch',
        currentBid: 100,
        startingPrice: 50,
        status: action === 'activate' ? 'draft' : 'active',
        endTime: '2030-01-01T00:00:00Z',
        bidCount: 1,
        categoryName: 'Watches',
        sellerName: 'Seller',
      }
      const filters: AuctionFilters = filtered
        ? { status: initial.status, searchTerm: 'watch', page: 1, pageSize: 10 }
        : {}
      let items = action === 'create' ? [] : [initial]
      const visibleItems = () =>
        items.filter((item) => !filters.status || item.status === filters.status)
      vi.spyOn(auctionsApi, 'getMyAuctions').mockImplementation(async () => ({
        items: visibleItems(),
        page: 1,
        pageSize: 10,
        totalCount: visibleItems().length,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false,
      }))
      vi.spyOn(auctionsApi, 'createAuction').mockImplementation(async () => {
        items = [initial]
        return { id: initial.id }
      })
      vi.spyOn(auctionsApi, 'updateAuction').mockImplementation(async () => {
        items = [{ ...initial, title: 'Updated watch' }]
      })
      vi.spyOn(auctionsApi, 'activateAuction').mockImplementation(async () => {
        items = [{ ...initial, status: 'active' }]
      })
      vi.spyOn(auctionsApi, 'deactivateAuction').mockImplementation(async () => {
        items = [{ ...initial, status: 'pending' }]
      })
      vi.spyOn(auctionsApi, 'deleteAuction').mockImplementation(async () => {
        items = []
      })
      vi.spyOn(auctionsApi, 'cancelAuction').mockImplementation(async () => {
        items = [{ ...initial, status: 'cancelled' }]
      })
      vi.spyOn(auctionsApi, 'extendAuction').mockImplementation(async (id, newEndTime) => {
        items = [{ ...initial, endTime: newEndTime }]
        return { auctionId: id, newEndTime }
      })
      const client = new QueryClient({
        defaultOptions: { queries: { retry: false, staleTime: Infinity } },
      })
      const wrapper = ({ children }: PropsWithChildren) => (
        <QueryClientProvider client={client}>{children}</QueryClientProvider>
      )
      const view = renderHook(
        () => ({
          auctions: useMyAuctions(filters),
          create: useCreateAuction(),
          update: useUpdateAuction(),
          activate: useActivateAuction(),
          deactivate: useDeactivateAuction(),
          delete: useDeleteAuction(),
          cancel: useCancelAuction(),
          extend: useExtendAuction(),
        }),
        { wrapper }
      )

      try {
        await waitFor(() => expect(view.result.current.auctions.isSuccess).toBe(true))
        expect(view.result.current.auctions.data?.items).toEqual(items)
        await act(async () => {
          switch (action) {
            case 'create':
              await view.result.current.create.mutateAsync({
                title: initial.title,
                description: 'Test watch',
                reservePrice: initial.startingPrice,
                auctionEnd: initial.endTime,
              })
              break
            case 'update':
              await view.result.current.update.mutateAsync({
                id: initial.id,
                data: { title: 'Updated watch' },
              })
              break
            case 'activate':
            case 'deactivate':
            case 'delete':
              await view.result.current[action].mutateAsync(initial.id)
              break
            case 'cancel':
              await view.result.current.cancel.mutateAsync({
                id: initial.id,
                reason: 'Unavailable',
              })
              break
            case 'extend':
              await view.result.current.extend.mutateAsync({
                id: initial.id,
                newEndTime: '2030-01-02T00:00:00Z',
              })
          }
        })
        await waitFor(() =>
          expect(view.result.current.auctions.data?.items).toEqual(visibleItems())
        )
      } finally {
        view.unmount()
        client.clear()
      }
    }
  )
})
