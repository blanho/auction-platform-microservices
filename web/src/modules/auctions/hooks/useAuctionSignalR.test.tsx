import { act, renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider, useQuery } from '@tanstack/react-query'
import { HubConnectionState } from '@microsoft/signalr'
import type { PropsWithChildren } from 'react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { auctionKeys } from './useAuctions'
import { bidKeys } from '@/modules/bidding/hooks/useBidding'
import { useAuctionSignalR } from './useAuctionSignalR'

const hub = vi.hoisted(() => ({
  state: 'Connected',
  handlers: new Map<string, Set<(data: unknown) => void>>(),
  joinAuctionRoom: vi.fn<() => Promise<void>>(),
  leaveAuctionRoom: vi.fn<() => Promise<void>>(),
}))
vi.mock('@/shared/hooks', () => ({ useSignalRState: () => hub.state }))
vi.mock('@/services/signalr', () => ({
  signalRService: {
    joinAuctionRoom: hub.joinAuctionRoom,
    leaveAuctionRoom: hub.leaveAuctionRoom,
    on: (event: string, callback: (data: unknown) => void) => {
      if (!hub.handlers.has(event)) {
        hub.handlers.set(event, new Set())
      }
      hub.handlers.get(event)!.add(callback)
    },
    off: (event: string, callback: (data: unknown) => void) =>
      hub.handlers.get(event)?.delete(callback),
  },
}))

function emit(event: string, auctionId: string) {
  act(() => hub.handlers.get(event)?.forEach((callback) => callback({ auctionId, bidId: 'bid' })))
}

function setup(id = 'a') {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false, staleTime: Infinity } },
  })
  const auction = vi.fn(async () => ({ price: 10, status: 'Active' }))
  const bids = vi.fn(async () => ['first'])
  const wrapper = ({ children }: PropsWithChildren) => (
    <QueryClientProvider client={client}>{children}</QueryClientProvider>
  )
  const hook = renderHook(
    ({ auctionId }) => {
      useAuctionSignalR({ auctionId })
      return {
        auction: useQuery({ queryKey: auctionKeys.detail(auctionId), queryFn: auction }).data,
        bids: useQuery({ queryKey: bidKeys.forAuction(auctionId), queryFn: bids }).data,
      }
    },
    { initialProps: { auctionId: id }, wrapper }
  )
  return { ...hook, client, auction, bids }
}

beforeEach(() => {
  hub.state = HubConnectionState.Connected
  hub.handlers.clear()
  hub.joinAuctionRoom.mockReset().mockResolvedValue(undefined)
  hub.leaveAuctionRoom.mockReset().mockResolvedValue(undefined)
})

describe('auction live updates', () => {
  it.each(['BidPlaced', 'AuctionEnded', 'AuctionExtended'])(
    'refreshes active queries on %s',
    async (event) => {
      const view = setup()
      await waitFor(() => expect(view.result.current.auction).toBeDefined())
      const previous = view.auction.mock.calls.length
      const previousBids = view.bids.mock.calls.length
      view.auction.mockResolvedValue({ price: 20, status: 'Ended' })
      view.bids.mockResolvedValue(['first', 'second'])
      emit(event, 'a')
      await waitFor(() => expect(view.auction.mock.calls.length).toBeGreaterThan(previous))
      if (event !== 'AuctionExtended') {
        await waitFor(() => expect(view.bids.mock.calls.length).toBeGreaterThan(previousBids))
      }
      await waitFor(() => expect(view.result.current.auction?.price).toBe(20))
    }
  )

  it('recovers missed state after reconnect and rejoins once', async () => {
    const view = setup()
    await waitFor(() => expect(view.result.current.auction).toBeDefined())
    hub.state = HubConnectionState.Reconnecting
    view.rerender({ auctionId: 'a' })
    view.auction.mockResolvedValue({ price: 50, status: 'Ended' })
    view.bids.mockResolvedValue(['winner'])
    hub.state = HubConnectionState.Connected
    view.rerender({ auctionId: 'a' })
    await waitFor(() => expect(view.result.current.auction?.price).toBe(50))
    await waitFor(() => expect(view.result.current.bids).toEqual(['winner']))
    expect(hub.joinAuctionRoom).toHaveBeenCalledTimes(2)
    expect(hub.handlers.get('BidPlaced')?.size).toBe(1)
  })

  it('ignores other auctions and cleans up on navigation and unmount', async () => {
    const view = setup()
    await waitFor(() => expect(view.result.current.auction).toBeDefined())
    const before = view.auction.mock.calls.length
    emit('BidPlaced', 'other')
    expect(view.auction).toHaveBeenCalledTimes(before)
    view.rerender({ auctionId: 'b' })
    await waitFor(() => expect(hub.joinAuctionRoom).toHaveBeenCalledTimes(2))
    await waitFor(() => expect(hub.handlers.get('BidPlaced')?.size).toBe(1))
    expect(hub.leaveAuctionRoom).toHaveBeenCalledWith('a')
    view.unmount()
    expect(hub.leaveAuctionRoom).toHaveBeenCalledWith('b')
    expect(hub.handlers.get('BidPlaced')?.size).toBe(0)
  })

  it('leaves a room when an obsolete join completes after navigation', async () => {
    let complete!: () => void
    hub.joinAuctionRoom.mockImplementationOnce(
      () =>
        new Promise<void>((resolve) => {
          complete = resolve
        })
    )
    const view = setup()
    view.rerender({ auctionId: 'b' })
    await waitFor(() => expect(hub.handlers.get('BidPlaced')?.size).toBe(1))
    await act(async () => complete())
    expect(hub.leaveAuctionRoom).toHaveBeenCalledWith('a')
    expect(hub.handlers.get('BidPlaced')?.size).toBe(1)
  })
  it('keeps two independent viewers current, including a viewer that reconnects', async () => {
    const first = setup()
    const second = setup()
    await waitFor(() => expect(first.result.current.auction).toBeDefined())
    await waitFor(() => expect(second.result.current.auction).toBeDefined())
    first.auction.mockResolvedValue({ price: 80, status: 'Active' })
    second.auction.mockResolvedValue({ price: 80, status: 'Active' })
    emit('BidPlaced', 'a')
    await waitFor(() => expect(first.result.current.auction?.price).toBe(80))
    await waitFor(() => expect(second.result.current.auction?.price).toBe(80))
    hub.state = HubConnectionState.Reconnecting
    second.rerender({ auctionId: 'a' })
    first.auction.mockResolvedValue({ price: 100, status: 'Ended' })
    second.auction.mockResolvedValue({ price: 100, status: 'Ended' })
    emit('AuctionEnded', 'a')
    await waitFor(() => expect(first.result.current.auction?.status).toBe('Ended'))
    expect(second.result.current.auction?.status).toBe('Active')
    hub.state = HubConnectionState.Connected
    second.rerender({ auctionId: 'a' })
    await waitFor(() => expect(second.result.current.auction).toEqual(first.result.current.auction))
  })

  it('receives an event arriving before the room join acknowledgement', async () => {
    let complete!: () => void
    hub.joinAuctionRoom.mockImplementationOnce(
      () =>
        new Promise<void>((resolve) => {
          complete = resolve
        })
    )
    const view = setup()
    await waitFor(() => expect(view.result.current.auction).toBeDefined())
    view.auction.mockResolvedValue({ price: 90, status: 'Active' })
    emit('BidPlaced', 'a')
    await waitFor(() => expect(view.result.current.auction?.price).toBe(90))
    await act(async () => complete())
  })
})
