import { expect, test } from '@playwright/test'

test('seller ships an order and buyer confirms delivery', async ({ page }) => {
  const errors: string[] = []
  page.on('pageerror', (error) => errors.push(error.message))
  let userId = 'seller'
  let shipmentRequests = 0
  let deliveryRequests = 0
  let order = {
    id: 'order',
    auctionId: 'auction',
    itemTitle: 'Fulfillment fixture watch',
    buyerId: 'buyer',
    buyerUsername: 'buyer',
    sellerId: 'seller',
    sellerUsername: 'seller',
    winningBid: 100,
    totalAmount: 100,
    status: 'paid',
    paymentStatus: 'completed',
    trackingNumber: '',
    shippingCarrier: '',
    createdAt: new Date().toISOString(),
  }

  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'))
  await page.route('**/*', async (route) => {
    const request = route.request()
    const url = new URL(request.url())
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
    if (url.pathname.startsWith('/hubs/')) {
      status = 503
      body = {}
    } else if (url.pathname === '/api/auth/refresh') {
      body = { accessToken: 'fixture-token', expiresIn: 3600 }
    } else if (url.pathname === '/api/auth/me') {
      body = {
        id: userId,
        userId,
        username: userId,
        displayName: userId,
        email: `${userId}@example.test`,
        roles: ['User'],
      }
    } else if (url.pathname === '/api/orders/order/ship') {
      expect(userId).toBe('seller')
      expect(request.method()).toBe('POST')
      expect(request.postDataJSON()).toEqual({
        trackingNumber: 'TRACK-123',
        shippingCarrier: 'Fixture carrier',
      })
      shipmentRequests++
      order = {
        ...order,
        status: 'shipped',
        trackingNumber: 'TRACK-123',
        shippingCarrier: 'Fixture carrier',
      }
      body = order
    } else if (url.pathname === '/api/orders/order/deliver') {
      expect(userId).toBe('buyer')
      expect(request.method()).toBe('POST')
      deliveryRequests++
      order = { ...order, status: 'delivered' }
      body = order
    } else if (url.pathname === '/api/orders/order') {
      body = order
    } else if (url.pathname.includes('/count')) {
      body = 0
    }
    await route.fulfill({ status, contentType: 'application/json', body: JSON.stringify(body) })
  })

  await page.goto('/orders/order')
  await page.getByRole('button', { name: 'Mark as Shipped' }).click()
  const dialog = page.getByRole('dialog')
  await dialog.getByLabel('Shipping Carrier').fill('Fixture carrier')
  await dialog.getByLabel('Tracking Number').fill('TRACK-123')
  await dialog.getByRole('button', { name: 'Confirm Shipment' }).click()
  await expect(dialog).toBeHidden()
  await expect(page.getByText('TRACK-123', { exact: true })).toBeVisible()
  await expect(page.getByRole('button', { name: 'Mark as Shipped' })).toHaveCount(0)
  await expect(page.getByRole('button', { name: 'Mark as Delivered' })).toHaveCount(0)

  userId = 'buyer'
  await page.goto('/orders/order')
  await page.getByRole('button', { name: 'Mark as Delivered' }).click()
  await expect(page.getByText('Order marked as delivered')).toBeVisible()
  await expect(page.getByRole('button', { name: 'Mark as Delivered' })).toHaveCount(0)
  expect(shipmentRequests).toBe(1)
  expect(deliveryRequests).toBe(1)
  expect(errors).toEqual([])
})
