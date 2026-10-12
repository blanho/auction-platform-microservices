import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { act, cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import type * as ReactI18next from 'react-i18next'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { auctionsApi } from '@/modules/auctions/api'
import type { AuctionListItem } from '@/modules/auctions/types'
import { MyAuctionsPage } from './MyAuctionsPage'

vi.mock('@/modules/auctions/components', () => ({
  BulkImportDialog: () => null,
  ExportAuctionsDialog: () => null,
  ImportAuctionsDialog: () => null,
}))
vi.mock('react-i18next', async (importOriginal) => ({
  ...(await importOriginal<typeof ReactI18next>()),
  useTranslation: () => ({ t: translate }),
}))

function translate(key: string) {
  return key
}

const clients: QueryClient[] = []
const auction: AuctionListItem = {
  id: 'auction-123',
  title: 'Vintage watch',
  currentBid: 100,
  startingPrice: 50,
  status: 'active',
  endTime: '2030-01-01T00:00:00Z',
  bidCount: 1,
  categoryName: 'Watches',
  sellerName: 'Seller',
}
const actions = {
  delete: {
    menu: 'myAuctions.delete',
    confirm: 'myAuctions.delete',
    success: 'deleted',
    error: 'deleteError',
  },
  cancel: {
    menu: 'myAuctions.cancelAuction',
    confirm: 'myAuctions.cancelAuction',
    success: 'cancelled',
    error: 'cancelError',
  },
  extend: {
    menu: 'myAuctions.extendTime',
    confirm: 'myAuctions.extendAuction',
    success: 'extended',
    error: 'extendError',
  },
} as const
type Action = keyof typeof actions

function renderSeller() {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false, staleTime: Infinity } },
  })
  clients.push(client)
  return render(
    <QueryClientProvider client={client}>
      <MemoryRouter>
        <MyAuctionsPage />
      </MemoryRouter>
    </QueryClientProvider>
  )
}

async function openAction(action: Action) {
  const title = await screen.findByText(auction.title)
  const row = title.closest('tr')!
  fireEvent.click(within(row).getByRole('button'))
  fireEvent.click(await screen.findByRole('menuitem', { name: actions[action].menu }))
  return screen.findByRole('dialog')
}

function fillAction(dialog: HTMLElement, action: Action) {
  if (action === 'cancel') {
    fireEvent.change(within(dialog).getByLabelText('myAuctions.cancelReason'), {
      target: { value: 'Item unavailable' },
    })
  } else if (action === 'extend') {
    fireEvent.change(within(dialog).getByLabelText('myAuctions.newEndTime'), {
      target: { value: '2030-01-02T12:00' },
    })
  }
}

beforeEach(() => {
  vi.spyOn(auctionsApi, 'getMyAuctions').mockResolvedValue({
    items: [auction],
    page: 1,
    pageSize: 10,
    totalCount: 1,
    totalPages: 1,
    hasNextPage: false,
    hasPreviousPage: false,
  })
  vi.spyOn(auctionsApi, 'deleteAuction').mockResolvedValue(undefined)
  vi.spyOn(auctionsApi, 'cancelAuction').mockResolvedValue(undefined)
  vi.spyOn(auctionsApi, 'extendAuction').mockResolvedValue({
    auctionId: auction.id,
    newEndTime: '2030-01-02T12:00',
  })
})

afterEach(() => {
  cleanup()
  clients.splice(0).forEach((client) => client.clear())
  vi.restoreAllMocks()
})

describe('seller auction confirmations', () => {
  const cases = (Object.keys(actions) as Action[]).flatMap((action) => [
    { action, succeeds: true },
    { action, succeeds: false },
  ])

  it.each(cases)('$action confirmation handles success=$succeeds', async ({ action, succeeds }) => {
    const api = {
      delete: auctionsApi.deleteAuction,
      cancel: auctionsApi.cancelAuction,
      extend: auctionsApi.extendAuction,
    }[action]
    if (!succeeds) {
      vi.mocked(api).mockRejectedValueOnce(new Error('Unavailable'))
    }
    renderSeller()
    const dialog = await openAction(action)
    fillAction(dialog, action)
    fireEvent.click(within(dialog).getByRole('button', { name: actions[action].confirm }))
    await screen.findByText(
      `myAuctions.messages.${succeeds ? actions[action].success : actions[action].error}`
    )
    if (action === 'delete') {
      expect(api).toHaveBeenCalledExactlyOnceWith(auction.id)
    } else if (action === 'cancel') {
      expect(api).toHaveBeenCalledExactlyOnceWith(auction.id, 'Item unavailable')
    } else {
      expect(api).toHaveBeenCalledExactlyOnceWith(auction.id, '2030-01-02T12:00')
    }
    if (succeeds) {
      await waitFor(() => expect(screen.queryByRole('dialog')).toBeNull())
    } else {
      expect(screen.getByRole('dialog')).toBe(dialog)
      if (action !== 'delete') {
        const field = action === 'cancel' ? 'myAuctions.cancelReason' : 'myAuctions.newEndTime'
        expect((within(dialog).getByLabelText(field) as HTMLInputElement).value).toBe(
          action === 'cancel' ? 'Item unavailable' : '2030-01-02T12:00'
        )
      }
    }
  })

  it.each(['cancel', 'extend'] as const)('resets %s inputs when dismissed', async (action) => {
    renderSeller()
    const dialog = await openAction(action)
    fillAction(dialog, action)
    fireEvent.click(
      within(dialog).getByRole('button', {
        name: action === 'cancel' ? 'myAuctions.keepAuction' : 'profile.cancel',
      })
    )
    await waitFor(() => expect(screen.queryByRole('dialog')).toBeNull())
    const reopened = await openAction(action)
    const field = action === 'cancel' ? 'myAuctions.cancelReason' : 'myAuctions.newEndTime'
    expect((within(reopened).getByLabelText(field) as HTMLInputElement).value).toBe('')
    expect(auctionsApi.cancelAuction).not.toHaveBeenCalled()
    expect(auctionsApi.extendAuction).not.toHaveBeenCalled()
  })

  it('reports a pending cancellation that completes after the dialog is dismissed', async () => {
    let finish!: () => void
    vi.mocked(auctionsApi.cancelAuction).mockImplementation(
      () =>
        new Promise((resolve) => {
          finish = resolve
        })
    )
    renderSeller()
    const dialog = await openAction('cancel')
    fireEvent.click(within(dialog).getByRole('button', { name: actions.cancel.confirm }))
    await waitFor(() =>
      expect(auctionsApi.cancelAuction).toHaveBeenCalledExactlyOnceWith(auction.id, undefined)
    )
    fireEvent.click(within(dialog).getByRole('button', { name: 'myAuctions.keepAuction' }))
    await waitFor(() => expect(screen.queryByRole('dialog')).toBeNull())
    await act(async () => finish())
    await screen.findByText('myAuctions.messages.cancelled')
  })
})
