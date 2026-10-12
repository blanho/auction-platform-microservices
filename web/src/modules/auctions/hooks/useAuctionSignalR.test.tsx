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

function setup(id = 'a', enabled = true) {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false, staleTime: Infinity } },
  })
  const auction = vi.fn(async () => ({ price: 10, status: 'Active' }))
  const bids = vi.fn(async () => ['first'])
  const wrapper = ({ children }: PropsWithChildren) => (
    <QueryClientProvider client={client}>{children}</QueryClientProvider>
  )
  const initialProps: { auctionId: string; enabled?: boolean } = { auctionId: id, enabled }
  const hook = renderHook(
    ({ auctionId, enabled }) => {
      useAuctionSignalR({ auctionId, enabled })
      return {
        auction: useQuery({ queryKey: auctionKeys.detail(auctionId), queryFn: auction }).data,
        bids: useQuery({ queryKey: bidKeys.forAuction(auctionId), queryFn: bids }).data,
      }
    },
    { initialProps, wrapper }
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
  it('waits for an initial connection before joining and fetches the current state', async () => {
    hub.state = HubConnectionState.Disconnected
    const view = setup()
    await waitFor(() => expect(view.result.current.auction?.price).toBe(10))
    expect(hub.joinAuctionRoom).not.toHaveBeenCalled()
    expect(hub.handlers.size).toBe(0)

    view.auction.mockResolvedValue({ price: 60, status: 'Ended' })
    view.bids.mockResolvedValue(['missed bid'])
    hub.state = HubConnectionState.Connected
    view.rerender({ auctionId: 'a' })

    await waitFor(() => expect(view.result.current.auction?.price).toBe(60))
    await waitFor(() => expect(view.result.current.bids).toEqual(['missed bid']))
    expect(hub.joinAuctionRoom).toHaveBeenCalledExactlyOnceWith('a')
  })

  it('subscribes only while enabled and removes every listener when disabled', async () => {
    const view = setup('a', false)
    await waitFor(() => expect(view.result.current.auction).toBeDefined())
    expect(hub.joinAuctionRoom).not.toHaveBeenCalled()
    expect(hub.handlers.size).toBe(0)

    view.rerender({ auctionId: 'a', enabled: true })
    await waitFor(() => expect(hub.joinAuctionRoom).toHaveBeenCalledExactlyOnceWith('a'))
    await act(async () => {
      await hub.joinAuctionRoom.mock.results[0].value
    })
    view.rerender({ auctionId: 'a', enabled: false })

    expect(hub.leaveAuctionRoom).toHaveBeenCalledExactlyOnceWith('a')
    for (const event of ['BidPlaced', 'AuctionEnded', 'AuctionExtended']) {
      expect(hub.handlers.get(event)?.size).toBe(0)
    }
    const requests = view.auction.mock.calls.length
    emit('BidPlaced', 'a')
    expect(view.auction).toHaveBeenCalledTimes(requests)
  })

  it('leaves an in-flight room join that completes after unmount', async () => {
    let complete!: () => void
    hub.joinAuctionRoom.mockImplementationOnce(
      () =>
        new Promise<void>((resolve) => {
          complete = resolve
        })
    )
    const view = setup()
    view.unmount()
    await act(async () => complete())

    expect(hub.leaveAuctionRoom).toHaveBeenCalledExactlyOnceWith('a')
    for (const event of ['BidPlaced', 'AuctionEnded', 'AuctionExtended']) {
      expect(hub.handlers.get(event)?.size).toBe(0)
    }
  })

  it('recovers from a rejected room join when the connection is restored', async () => {
    hub.joinAuctionRoom.mockRejectedValueOnce(new Error('transport closed during join'))
    const view = setup()
    await waitFor(() => expect(view.result.current.auction).toBeDefined())
    expect(hub.leaveAuctionRoom).not.toHaveBeenCalled()

    hub.state = HubConnectionState.Reconnecting
    view.rerender({ auctionId: 'a' })
    view.auction.mockResolvedValue({ price: 70, status: 'Active' })
    view.bids.mockResolvedValue(['recovered bid'])
    hub.state = HubConnectionState.Connected
    view.rerender({ auctionId: 'a' })

    await waitFor(() => expect(view.result.current.auction?.price).toBe(70))
    await waitFor(() => expect(view.result.current.bids).toEqual(['recovered bid']))
    expect(hub.joinAuctionRoom).toHaveBeenCalledTimes(2)
    for (const event of ['BidPlaced', 'AuctionEnded', 'AuctionExtended']) {
      expect(hub.handlers.get(event)?.size).toBe(1)
    }
  })

  it('refreshes auction timing without refetching unchanged bid history on extension', async () => {
    const view = setup()
    await waitFor(() => expect(view.result.current.bids).toEqual(['first']))
    const previousBids = view.bids.mock.calls.length
    view.auction.mockResolvedValue({ price: 10, status: 'Extended' })
    view.bids.mockResolvedValue(['should not be fetched'])

    emit('AuctionExtended', 'a')

    await waitFor(() => expect(view.result.current.auction?.status).toBe('Extended'))
    expect(view.bids).toHaveBeenCalledTimes(previousBids)
    expect(view.result.current.bids).toEqual(['first'])
  })

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
