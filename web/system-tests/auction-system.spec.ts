import { expect, test } from '@playwright/test'
import type { APIRequestContext, Browser, Page, WebSocketRoute } from '@playwright/test'

const password = process.env.SYSTEM_TEST_PASSWORD || 'SeedUser@Dev123!'

async function login(api: APIRequestContext, username: string): Promise<string> {
  const send = () =>
    api.post('/api/auth/login', {
      data: { usernameOrEmail: username, password },
    })
  let response = await send()
  if (response.status() === 429) {
    await new Promise((resolve) => setTimeout(resolve, 61000))
    response = await send()
  }
  expect(response.status(), `login ${username}: ${await response.text()}`).toBe(200)
  const data = await response.json()
  expect(data.accessToken).toBeTruthy()
  return data.accessToken
}

async function createAuction(api: APIRequestContext, token: string) {
  const title = `System validation ${Date.now()}`
  const response = await api.post('/api/auctions', {
    headers: { Authorization: `Bearer ${token}` },
    data: {
      title,
      description: 'Disposable end to end validation auction.',
      reservePrice: 100,
      buyNowPrice: 250,
      auctionEnd: new Date(Date.now() + 86400000).toISOString(),
    },
  })
  expect(response.status(), await response.text()).toBe(201)
  const auction = await response.json()
  expect(auction.id).toBeTruthy()
  return { id: auction.id as string, title }
}

async function signInBrowser(browser: Browser, username: string, baseURL: string) {
  const context = await browser.newContext({ baseURL, locale: 'en-US' })
  const page = await context.newPage()
  const pendingJoins = new Map<string, string>()
  const joinedRooms: string[] = []
  let transport: { client: WebSocketRoute; server: WebSocketRoute } | undefined
  await page.routeWebSocket('**/hubs/notifications*', (socket) => {
    const server = socket.connectToServer()
    transport = { client: socket, server }
    socket.onMessage((payload) => {
      for (const frame of payload.toString().split('\u001e').filter(Boolean)) {
        const message = JSON.parse(frame)
        if (message.target === 'JoinAuctionRoom') {
          pendingJoins.set(message.invocationId, message.arguments[0])
        }
      }
      server.send(payload)
    })
    server.onMessage((payload) => {
      for (const frame of payload.toString().split('\u001e').filter(Boolean)) {
        const message = JSON.parse(frame)
        const room = pendingJoins.get(message.invocationId)
        if (message.type === 3 && room && !message.error) {
          joinedRooms.push(room)
          pendingJoins.delete(message.invocationId)
        }
      }
      socket.send(payload)
    })
  })
  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'))
  await page.goto('/login')
  await page
    .getByLabel(/username|email/i)
    .first()
    .fill(username)
  await page.getByLabel(/^password/i).fill(password)
  const submit = async () => {
    const response = page.waitForResponse((r) => r.url().endsWith('/api/auth/login'))
    await page.getByRole('button', { name: /sign in|log in/i }).click()
    return response
  }
  const response = await submit()
  if (response.status() === 429) {
    await page.waitForTimeout(61000)
    await expect(page.getByLabel(/username|email/i).first()).toHaveValue(username)
    await expect(page.getByLabel(/^password/i)).toHaveValue(password)
    expect((await submit()).status()).toBe(200)
  } else {
    expect(response.status(), await response.text()).toBe(200)
  }
  await expect(page).not.toHaveURL(/\/login/)
  const disconnectTransport = async () => {
    expect(transport).toBeDefined()
    await transport!.client.close({ code: 1001, reason: 'Temporary test network loss' })
    await transport!.server.close()
  }
  return { context, page, joinedRooms, disconnectTransport }
}

async function expectPrice(page: Page, amount: number) {
  const price = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  }).format(amount)
  await expect(page.getByRole('heading', { name: price, exact: true })).toBeVisible()
}

test('public gateway endpoints and authenticated profile work through nginx', async ({
  request,
}) => {
  for (const path of ['/api/auctions', '/api/categories', '/api/brands']) {
    const response = await request.get(path)
    expect(response.status(), `${path}: ${await response.text()}`).toBe(200)
  }
  const token = await login(request, 'alice')
  for (const path of ['/api/orders/buyer/me', '/api/notifications', '/api/notifications/summary']) {
    const response = await request.get(path, { headers: { Authorization: `Bearer ${token}` } })
    expect(response.status(), `${path}: ${await response.text()}`).toBe(200)
  }
  const me = await request.get('/api/auth/me', { headers: { Authorization: `Bearer ${token}` } })
  expect(me.status(), await me.text()).toBe(200)
  expect((await me.json()).username).toBe('alice')
  const refresh = await request.post('/api/auth/refresh')
  expect(refresh.status(), await refresh.text()).toBe(200)
  expect((await refresh.json()).accessToken).toBeTruthy()
  const admin = await login(request, 'admin')
  for (const path of [
    '/api/reports?page=1&pageSize=5&status=Pending',
    '/api/auditlogs?pageSize=5',
    '/api/users?pageSize=5&search=alice',
  ]) {
    const response = await request.get(path, { headers: { Authorization: `Bearer ${admin}` } })
    expect(response.status(), `${path}: ${await response.text()}`).toBe(200)
  }
})

test('two real browsers see bids, reject low bids and recover after disconnect', async ({
  request,
  browser,
  baseURL,
}) => {
  const seller = await login(request, 'seller')
  const bidder = await login(request, 'bob')
  const auction = await createAuction(request, seller)
  const alice = await signInBrowser(browser, 'alice', baseURL!)
  const bob = await signInBrowser(browser, 'bob', baseURL!)
  try {
    await alice.page.goto(`/auctions/${auction.id}`)
    await bob.page.goto(`/auctions/${auction.id}`)
    await expect(alice.page.getByRole('heading', { name: auction.title })).toBeVisible()
    await expect(bob.page.getByRole('heading', { name: auction.title })).toBeVisible()
    await expect.poll(() => alice.joinedRooms).toContain(auction.id)
    await expect.poll(() => bob.joinedRooms).toContain(auction.id)
    const key = crypto.randomUUID()
    const bid = () =>
      request.post('/api/bids', {
        headers: { Authorization: `Bearer ${bidder}`, 'Idempotency-Key': key },
        data: { auctionId: auction.id, amount: 130 },
      })
    const placed = await bid()
    expect(placed.status(), await placed.text()).toBe(201)
    const placedBid = await placed.json()
    const replay = await bid()
    expect(replay.status(), await replay.text()).toBe(400)
    expect((await replay.json()).title).toBe('Bid.DuplicateRequest')
    const history = await request.get(`/api/bids/auction/${auction.id}`)
    const rows = await history.json()
    expect(rows.items.filter((row: { id: string }) => row.id === placedBid.id)).toHaveLength(1)
    await expectPrice(alice.page, 130)
    await expectPrice(bob.page, 130)
    const low = await request.post('/api/bids', {
      headers: { Authorization: `Bearer ${bidder}` },
      data: { auctionId: auction.id, amount: 105 },
    })
    expect(low.status()).toBeGreaterThanOrEqual(400)
    expect(low.status()).toBeLessThan(500)
    await alice.context.setOffline(true)
    await alice.disconnectTransport()
    const next = await request.post('/api/bids', {
      headers: { Authorization: `Bearer ${bidder}` },
      data: { auctionId: auction.id, amount: 160 },
    })
    expect(next.status(), await next.text()).toBe(201)
    await expectPrice(bob.page, 160)
    await expectPrice(alice.page, 130)
    await alice.context.setOffline(false)
    await expect.poll(() => alice.joinedRooms.length, { timeout: 30000 }).toBeGreaterThan(1)
    await expectPrice(alice.page, 160)
  } finally {
    await alice.context.close()
    await bob.context.close()
  }
})

test('real buy-now saga produces an unpaid order visible in the browser', async ({
  request,
  browser,
  baseURL,
}) => {
  const seller = await login(request, 'seller')
  const auction = await createAuction(request, seller)
  const buyer = await signInBrowser(browser, 'alice', baseURL!)
  const observer = await signInBrowser(browser, 'bob', baseURL!)
  try {
    await buyer.page.goto(`/auctions/${auction.id}`)
    await observer.page.goto(`/auctions/${auction.id}`)
    await expect(buyer.page.getByRole('heading', { name: auction.title })).toBeVisible()
    await expect.poll(() => observer.joinedRooms).toContain(auction.id)
    await expect(observer.page.getByRole('button', { name: /Place Bid/i })).toBeVisible()
    const ordersResponse = buyer.page.waitForResponse(
      (response) => response.url().includes('/api/orders/buyer/me') && response.status() === 200
    )
    await buyer.page.getByRole('button', { name: /Buy Now/i }).click()
    await buyer.page
      .getByRole('dialog')
      .getByRole('button', { name: /Confirm Purchase/i })
      .click()
    await expect(buyer.page).toHaveURL(/\/orders$/, { timeout: 30000 })
    await expect(buyer.page.getByText(auction.title).first()).toBeVisible()
    await expect(observer.page.getByRole('button', { name: /Place Bid/i })).toBeHidden()
    await expect(observer.page.getByRole('button', { name: /Buy Now/i })).toBeHidden()
    const orders = await (await ordersResponse).json()
    expect(
      orders.find((order: { auctionId: string }) => order.auctionId === auction.id)
    ).toMatchObject({
      itemTitle: auction.title,
      paymentStatus: 'pending',
    })
  } finally {
    await buyer.context.close()
    await observer.context.close()
  }
})
