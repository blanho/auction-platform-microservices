import { expect, test } from '@playwright/test'

for (const outcome of ['Completed', 'Failed'] as const) {
  test(`buy-now purchase moves from processing to ${outcome}`, async ({ page }) => {
    const errors: string[] = []
    page.on('pageerror', (error) => errors.push(error.message))
    const auctionId = '11111111-1111-1111-1111-111111111111'
    const userId = '22222222-2222-2222-2222-222222222222'
    let purchased = false
    let polls = 0
    let purchaseRequests = 0
    const receipt = (status: string) => ({
      auctionId,
      correlationId: 'correlation',
      orderId: status === 'Completed' ? 'order' : '',
      status,
      success: status === 'Completed',
    })
    await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'))
    await page.route('**/*', async (route) => {
      const url = new URL(route.request().url())
      if (url.origin !== 'http://127.0.0.1:3100') {
        await route.abort()
        return
      }
      if (!url.pathname.startsWith('/api/') && !url.pathname.startsWith('/hubs/')) {
        await route.continue()
        return
      }
      let body: unknown = []
      let status = 200
      const path = url.pathname
      if (path.startsWith('/hubs/')) {
        status = 503
        body = {}
      } else if (path === '/api/auth/refresh') {
        body = { accessToken: 'fixture-token', expiresIn: 3600 }
      } else if (path === '/api/auth/me') {
        body = {
          id: userId,
          userId,
          username: 'buyer',
          displayName: 'buyer',
          email: 'buyer@example.test',
          roles: ['User'],
        }
      } else if (path === `/api/auctions/${auctionId}/buy-now`) {
        purchaseRequests++
        expect(route.request().method()).toBe('POST')
        expect(route.request().headers()['idempotency-key']).toBeTruthy()
        purchased = true
        status = 202
        body = receipt('Processing')
      } else if (path === `/api/auctions/${auctionId}/purchase`) {
        body = purchased ? receipt(++polls <= 2 ? 'Processing' : outcome) : null
      } else if (path === `/api/auctions/${auctionId}`) {
        body = {
          id: auctionId,
          title: 'Fixture auction watch',
          description: 'A test auction',
          reservePrice: 100,
          buyNowPrice: 250,
          currentHighBid: 100,
          currency: 'USD',
          seller: 'seller',
          sellerId: 'seller',
          status: 'Active',
          files: [],
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
          auctionEnd: new Date(Date.now() + 86400000).toISOString(),
          categoryName: 'Watches',
        }
      } else if (path.includes('/bids/auction/')) {
        body = { items: [], totalCount: 0, page: 1, pageSize: 20 }
      } else if (path.includes('/autobids/')) {
        body = null
      } else if (path.includes('/watchlist/check/')) {
        body = false
      } else if (path.includes('/count')) {
        body = 0
      } else if (path.includes('/reviews')) {
        body = { items: [], totalCount: 0, averageRating: 0 }
      } else if (path.includes('/orders/buyer/')) {
        body =
          outcome === 'Completed'
            ? [
                {
                  id: 'order',
                  auctionId,
                  itemTitle: 'Fixture auction watch',
                  auctionTitle: 'Fixture auction watch',
                  buyerId: userId,
                  buyerUsername: 'buyer',
                  sellerId: 'seller',
                  sellerUsername: 'seller',
                  winningBid: 250,
                  totalAmount: 250,
                  status: 'payment_pending',
                  paymentStatus: 'pending',
                  createdAt: new Date().toISOString(),
                },
              ]
            : []
      }
      await route.fulfill({
        status,
        contentType: 'application/json',
        headers: { 'x-total-count': '1' },
        body: JSON.stringify(body),
      })
    })
    await page.goto(`/auctions/${auctionId}`)
    await expect(page.getByRole('heading', { name: 'Fixture auction watch' })).toBeVisible()
    await page.getByRole('button', { name: /Buy Now/i }).click()
    await page
      .getByRole('dialog')
      .getByRole('button', { name: /Confirm Purchase/i })
      .click()
    await expect(page.getByText(/Your purchase is being confirmed/).first()).toBeVisible()
    if (outcome === 'Completed') {
      await expect(page).toHaveURL(/\/orders$/, { timeout: 15000 })
      await expect(page.getByText('Fixture auction watch').first()).toBeVisible()
    } else {
      await expect(
        page.getByText(/purchase.*failed|could not.*purchase|could not be completed/i).first()
      ).toBeVisible({ timeout: 10000 })
      await expect(page).toHaveURL(new RegExp(`/auctions/${auctionId}$`))
    }
    expect(errors).toEqual([])
    expect(purchaseRequests).toBe(1)
    expect(polls).toBeGreaterThanOrEqual(3)
  })
}
