import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { act, cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import { Link, MemoryRouter, Route, Routes } from 'react-router-dom'
import type * as ReactI18next from 'react-i18next'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ordersApi } from '../api'
import type { Order, OrderStatus } from '../types'
import { OrderDetailPage } from './OrderDetailPage'

const session = vi.hoisted(() => ({ user: { id: 'SELLER', roles: [] as string[] } }))
vi.mock('@/app/hooks/useAuth', () => ({ useAuth: () => session }))
vi.mock('react-i18next', async (importOriginal) => ({
  ...(await importOriginal<typeof ReactI18next>()),
  useTranslation: () => ({ t: translate }),
}))

function translate(key: string) {
  return key
}

const clients: QueryClient[] = []

function order(status: OrderStatus = 'paid'): Order {
  return {
    id: 'order-123',
    auctionId: 'auction-123',
    itemTitle: 'Vintage watch',
    buyerId: 'buyer',
    buyerUsername: 'buyer',
    sellerId: 'seller',
    sellerUsername: 'seller',
    winningBid: 100,
    totalAmount: 100,
    status,
    paymentStatus: 'completed',
    createdAt: '2026-01-01T00:00:00Z',
  }
}

function renderOrder() {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false, staleTime: Infinity }, mutations: { retry: false } },
  })
  clients.push(client)
  return render(
    <QueryClientProvider client={client}>
      <MemoryRouter initialEntries={['/orders/order-123']}>
        <Link to="/orders/order-456">Open next order</Link>
        <Routes>
          <Route path="/orders/:orderId" element={<OrderDetailPage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>
  )
}

async function openShipment() {
  fireEvent.click(await screen.findByRole('button', { name: 'orderDetail.markShipped' }))
  const dialog = await screen.findByRole('dialog')
  fireEvent.change(within(dialog).getByLabelText('orderDetail.shippingCarrier'), {
    target: { value: 'Test carrier' },
  })
  fireEvent.change(within(dialog).getByLabelText('orderDetail.trackingNumber'), {
    target: { value: 'TRACK-123' },
  })
  return dialog
}

beforeEach(() => {
  session.user = { id: 'SELLER', roles: [] }
  vi.spyOn(ordersApi, 'getOrderById').mockResolvedValue(order())
  vi.spyOn(ordersApi, 'shipOrder').mockResolvedValue(order('shipped'))
  vi.spyOn(ordersApi, 'markDelivered').mockResolvedValue(order('delivered'))
})

afterEach(() => {
  cleanup()
  clients.splice(0).forEach((client) => client.clear())
  vi.restoreAllMocks()
  vi.unstubAllGlobals()
})

describe('order fulfillment through the detail page', () => {
  it.each([
    ['paid', 'SELLER', [], true, false],
    ['paid', 'buyer', [], false, false],
    ['paid', 'other', ['AdMiN'], true, false],
    ['shipped', 'BUYER', [], false, true],
    ['shipped', 'seller', [], false, false],
    ['shipped', 'other', ['ADMIN'], false, true],
    ['completed', 'seller', ['admin'], false, false],
    ['cancelled', 'buyer', ['admin'], false, false],
  ] satisfies [OrderStatus, string, string[], boolean, boolean][])(
    'offers the correct actions for %s / %s / %j',
    async (status, id, roles, canShip, canDeliver) => {
      session.user = { id, roles }
      vi.mocked(ordersApi.getOrderById).mockResolvedValue(order(status))
      renderOrder()
      await screen.findByText('Vintage watch')
      expect(!!screen.queryByRole('button', { name: 'orderDetail.markShipped' })).toBe(canShip)
      expect(!!screen.queryByRole('button', { name: 'orderDetail.markDelivered' })).toBe(canDeliver)
    }
  )

  it('requires both shipment fields before submitting', async () => {
    renderOrder()
    fireEvent.click(await screen.findByRole('button', { name: 'orderDetail.markShipped' }))
    const dialog = await screen.findByRole('dialog')
    const confirm = within(dialog).getByRole('button', { name: 'orderDetail.confirmShipment' })
    expect((confirm as HTMLButtonElement).disabled).toBe(true)
    fireEvent.change(within(dialog).getByLabelText('orderDetail.shippingCarrier'), {
      target: { value: 'Test carrier' },
    })
    expect((confirm as HTMLButtonElement).disabled).toBe(true)
    fireEvent.click(confirm)
    expect(ordersApi.shipOrder).not.toHaveBeenCalled()
  })

  it('prevents repeated submission while saving and refreshes the shipped order', async () => {
    let finishShipment!: (value: Order) => void
    vi.mocked(ordersApi.shipOrder).mockImplementation(
      () => new Promise((resolve) => (finishShipment = resolve))
    )
    renderOrder()
    const dialog = await openShipment()
    const confirm = within(dialog).getByRole('button', { name: 'orderDetail.confirmShipment' })
    fireEvent.click(confirm)
    await waitFor(() => expect((confirm as HTMLButtonElement).disabled).toBe(true))
    fireEvent.click(confirm)
    expect(ordersApi.shipOrder).toHaveBeenCalledExactlyOnceWith('order-123', {
      trackingNumber: 'TRACK-123',
      shippingCarrier: 'Test carrier',
    })
    vi.mocked(ordersApi.getOrderById).mockResolvedValue({
      ...order('shipped'),
      trackingNumber: 'TRACK-123',
      shippingCarrier: 'Test carrier',
    })
    finishShipment(order('shipped'))
    await screen.findByText('orderDetail.markedShipped')
    await waitFor(() => expect(screen.queryByRole('dialog')).toBeNull())
    expect(await screen.findByText('TRACK-123')).toBeTruthy()
    expect(screen.queryByRole('button', { name: 'orderDetail.markShipped' })).toBeNull()
  })

  it('keeps shipment details after failure and allows a successful retry', async () => {
    vi.mocked(ordersApi.shipOrder).mockRejectedValueOnce(new Error('Unavailable'))
    renderOrder()
    const dialog = await openShipment()
    fireEvent.click(within(dialog).getByRole('button', { name: 'orderDetail.confirmShipment' }))
    await screen.findByText('orderDetail.updateFailed')
    expect(screen.getByRole('dialog')).toBe(dialog)
    expect(
      (within(dialog).getByLabelText('orderDetail.trackingNumber') as HTMLInputElement).value
    ).toBe('TRACK-123')
    expect(
      (within(dialog).getByLabelText('orderDetail.shippingCarrier') as HTMLInputElement).value
    ).toBe('Test carrier')
    vi.mocked(ordersApi.getOrderById).mockResolvedValue(order('shipped'))
    fireEvent.click(within(dialog).getByRole('button', { name: 'orderDetail.confirmShipment' }))
    await screen.findByText('orderDetail.markedShipped')
    expect(ordersApi.shipOrder).toHaveBeenCalledTimes(2)
  })

  it('retains an unfinished shipment when the dialog is dismissed and reopened', async () => {
    renderOrder()
    const dialog = await openShipment()
    fireEvent.click(within(dialog).getByRole('button', { name: 'common:cancel' }))
    await waitFor(() => expect(screen.queryByRole('dialog')).toBeNull())
    fireEvent.click(screen.getByRole('button', { name: 'orderDetail.markShipped' }))
    const reopened = await screen.findByRole('dialog')
    expect(
      (within(reopened).getByLabelText('orderDetail.trackingNumber') as HTMLInputElement).value
    ).toBe('TRACK-123')
    expect(ordersApi.shipOrder).not.toHaveBeenCalled()
  })

  it.each([true, false])('handles delivery confirmation with success=%s', async (succeeds) => {
    session.user.id = 'buyer'
    vi.mocked(ordersApi.getOrderById).mockResolvedValue(order('shipped'))
    vi.mocked(ordersApi.markDelivered).mockImplementation(async () => {
      if (!succeeds) {
        throw new Error('Unavailable')
      }
      vi.mocked(ordersApi.getOrderById).mockResolvedValue(order('delivered'))
      return order('delivered')
    })
    renderOrder()
    fireEvent.click(await screen.findByRole('button', { name: 'orderDetail.markDelivered' }))
    await screen.findByText(succeeds ? 'orderDetail.markedDelivered' : 'orderDetail.updateFailed')
    expect(ordersApi.markDelivered).toHaveBeenCalledExactlyOnceWith('order-123')
    await waitFor(() =>
      expect(!!screen.queryByRole('button', { name: 'orderDetail.markDelivered' })).toBe(!succeeds)
    )
  })

  it('starts with an empty shipment form when navigating to another order', async () => {
    vi.mocked(ordersApi.getOrderById).mockImplementation(async (id) => ({
      ...order(),
      id,
      itemTitle: id === 'order-123' ? 'Vintage watch' : 'Second watch',
    }))
    renderOrder()
    const dialog = await openShipment()
    fireEvent.click(within(dialog).getByRole('button', { name: 'common:cancel' }))
    await waitFor(() => expect(screen.queryByRole('dialog')).toBeNull())
    fireEvent.click(screen.getByRole('link', { name: 'Open next order' }))
    await screen.findByText('Second watch')
    fireEvent.click(screen.getByRole('button', { name: 'orderDetail.markShipped' }))
    const nextDialog = await screen.findByRole('dialog')
    expect(
      (within(nextDialog).getByLabelText('orderDetail.shippingCarrier') as HTMLInputElement).value
    ).toBe('')
    expect(
      (within(nextDialog).getByLabelText('orderDetail.trackingNumber') as HTMLInputElement).value
    ).toBe('')
    expect(ordersApi.shipOrder).not.toHaveBeenCalled()
  })

  it('does not let a previous order shipment close the current order form', async () => {
    vi.mocked(ordersApi.getOrderById).mockImplementation(async (id) => ({
      ...order(),
      id,
      itemTitle: id === 'order-123' ? 'Vintage watch' : 'Second watch',
    }))
    let finish!: (value: Order) => void
    vi.mocked(ordersApi.shipOrder).mockImplementation(
      () =>
        new Promise((resolve) => {
          finish = resolve
        })
    )
    renderOrder()
    const dialog = await openShipment()
    fireEvent.click(within(dialog).getByRole('button', { name: 'orderDetail.confirmShipment' }))
    await waitFor(() => expect(ordersApi.shipOrder).toHaveBeenCalledTimes(1))
    fireEvent.click(within(dialog).getByRole('button', { name: 'common:cancel' }))
    await waitFor(() => expect(screen.queryByRole('dialog')).toBeNull())
    fireEvent.click(screen.getByRole('link', { name: 'Open next order' }))
    await screen.findByText('Second watch')
    const nextDialog = await openShipment()
    fireEvent.change(within(nextDialog).getByLabelText('orderDetail.trackingNumber'), {
      target: { value: 'SECOND-ORDER' },
    })
    await act(async () => finish(order('shipped')))
    await waitFor(() =>
      expect(screen.queryByRole('button', { name: 'orderDetail.confirmShipment' })).not.toBeNull()
    )
    expect(screen.getByRole('dialog')).toBe(nextDialog)
    expect(
      (within(nextDialog).getByLabelText('orderDetail.trackingNumber') as HTMLInputElement).value
    ).toBe('SECOND-ORDER')
    expect(screen.queryByText('orderDetail.markedShipped')).toBeNull()
    expect(ordersApi.shipOrder).toHaveBeenCalledExactlyOnceWith('order-123', {
      trackingNumber: 'TRACK-123',
      shippingCarrier: 'Test carrier',
    })
  })

  it.each([true, false])('reports clipboard success=%s accurately', async (succeeds) => {
    const writeText = vi.fn().mockImplementation(async () => {
      if (!succeeds) {
        throw new Error('Permission denied')
      }
    })
    vi.stubGlobal('navigator', { ...navigator, clipboard: { writeText } })
    renderOrder()
    fireEvent.click(await screen.findByRole('button', { name: 'orderDetail.copy' }))
    await screen.findByText(succeeds ? 'orderDetail.copied' : 'orderDetail.copyFailed')
    expect(writeText).toHaveBeenCalledExactlyOnceWith('order-123')
  })
})
