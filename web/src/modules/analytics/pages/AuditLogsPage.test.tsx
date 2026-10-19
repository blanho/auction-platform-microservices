import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import type * as ReactI18next from 'react-i18next'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { auditLogsApi } from '../api/analytics.api'
import { AuditLogsPage } from './AuditLogsPage'

vi.mock('react-i18next', async (importOriginal) => ({
  ...(await importOriginal<typeof ReactI18next>()),
  useTranslation: () => ({ t: (key: string) => key }),
}))

const clients: QueryClient[] = []
afterEach(() => {
  clients.splice(0).forEach((client) => client.clear())
  vi.restoreAllMocks()
})

describe('audit log contract', () => {
  it('renders named actions and the stored metadata in the details dialog', async () => {
    vi.spyOn(auditLogsApi, 'getAuditLogs').mockResolvedValue({
      items: [
        {
          id: 'audit-id',
          entityId: 'auction-id',
          entityType: 'AuctionAuditData',
          action: 'SoftDeleted',
          actionName: 'SoftDeleted',
          userId: 'user-id',
          username: 'seller',
          serviceName: 'auction-service',
          timestamp: '2026-10-19T00:00:00Z',
          metadata: { Reason: 'Item unavailable', Action: 'Cancelled' },
        },
      ],
      totalCount: 1,
      page: 1,
      pageSize: 20,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
    })
    renderPage()
    const seller = await screen.findByText('seller')
    fireEvent.click(seller.closest('tr')!)
    const dialog = await screen.findByRole('dialog')
    expect(within(dialog).getByText(/Item unavailable/)).toBeTruthy()
    expect(within(dialog).getByText(/Soft Deleted|論理削除/)).toBeTruthy()
  })

  it('sends persisted entity/service names and only supported actions as filters', async () => {
    const getLogs = vi.spyOn(auditLogsApi, 'getAuditLogs').mockResolvedValue({
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 20,
      totalPages: 0,
      hasNextPage: false,
      hasPreviousPage: false,
    })
    renderPage()
    const filters = await screen.findAllByRole('combobox')
    fireEvent.mouseDown(filters[0])
    fireEvent.click(await screen.findByRole('option', { name: /^Auction$|^オークション$/ }))
    await waitFor(() =>
      expect(getLogs).toHaveBeenLastCalledWith(
        expect.objectContaining({ entityType: 'AuctionAuditData' })
      )
    )
    fireEvent.mouseDown(filters[1])
    fireEvent.click(
      await screen.findByRole('option', { name: /^Auction Service$|^オークションサービス$/ })
    )
    await waitFor(() =>
      expect(getLogs).toHaveBeenLastCalledWith(
        expect.objectContaining({ serviceName: 'auction-service' })
      )
    )
    fireEvent.mouseDown(filters[2])
    expect(screen.queryByRole('option', { name: /Bid Placed/ })).toBeNull()
    fireEvent.click(await screen.findByRole('option', { name: /^Restored$|^復元$/ }))
    await waitFor(() =>
      expect(getLogs).toHaveBeenLastCalledWith(expect.objectContaining({ action: 'Restored' }))
    )
  })
})

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  clients.push(client)
  return render(
    <QueryClientProvider client={client}>
      <AuditLogsPage />
    </QueryClientProvider>
  )
}
