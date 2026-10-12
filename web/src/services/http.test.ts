import type * as HttpModule from './http'
import axios from 'axios'
import MockAdapter from 'axios-mock-adapter'
import { afterEach, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest'
import { clearAuthStorage, getAccessToken, setAccessToken } from '@/modules/auth/utils/token.utils'

const adapter = new MockAdapter(axios)
let http: typeof HttpModule.http
beforeAll(async () => {
  const module = await import('./http')
  http = module.http
})

beforeEach(() => {
  clearAuthStorage()
  setAccessToken('expired', 60)
})
afterEach(() => {
  adapter.reset()
  clearAuthStorage()
  vi.useRealTimers()
  vi.restoreAllMocks()
})

describe('transient request failures', () => {
  it.each([408, 429, 503])('retries a GET after HTTP %s', async (status) => {
    vi.useFakeTimers()
    adapter.onGet('/resource').replyOnce(status).onGet('/resource').reply(200, { ok: true })

    const response = http.get('/resource')
    await vi.runAllTimersAsync()

    expect((await response).data).toEqual({ ok: true })
    expect(adapter.history.get).toHaveLength(2)
    expect(adapter.history.post).toHaveLength(0)
  })

  it('limits a GET to three retries and propagates the final failure', async () => {
    vi.useFakeTimers()
    adapter.onGet('/resource').reply(503)

    const outcome = http.get('/resource').catch((error: unknown) => error)
    await vi.runAllTimersAsync()

    expect(axios.isAxiosError(await outcome)).toBe(true)
    expect(adapter.history.get).toHaveLength(4)
  })

  it.each(['post', 'patch'] as const)('does not retry a failed %s mutation', async (method) => {
    adapter.onAny('/resource').reply(503)

    await expect(http[method]('/resource', {})).rejects.toMatchObject({ response: { status: 503 } })

    expect(adapter.history[method]).toHaveLength(1)
  })

  it('does not retry a validation failure', async () => {
    adapter.onGet('/resource').reply(400)

    await expect(http.get('/resource')).rejects.toMatchObject({ response: { status: 400 } })

    expect(adapter.history.get).toHaveLength(1)
  })
})

describe('authentication refresh', () => {
  it('shares one refresh for concurrent 401 responses and retries both requests', async () => {
    adapter.onPost('/api/auth/refresh').reply(async () => {
      await new Promise((resolve) => setTimeout(resolve, 20))
      return [200, { accessToken: 'renewed', expiresIn: 3600 }]
    })
    adapter
      .onGet('/protected')
      .reply((config) =>
        config.headers?.Authorization === 'Bearer renewed' ? [200, { ok: true }] : [401]
      )
    const responses = await Promise.all([http.get('/protected'), http.get('/protected')])
    expect(responses.every((response) => response.status === 200)).toBe(true)
    expect(adapter.history.post).toHaveLength(1)
    expect(getAccessToken()).toBe('renewed')
  })

  it('clears authentication and rejects every waiting request when refresh fails', async () => {
    adapter.onGet('/protected').reply(401)
    adapter.onPost('/api/auth/refresh').reply(async () => {
      await new Promise((resolve) => setTimeout(resolve, 20))
      return [401]
    })
    const results = await Promise.allSettled([http.get('/protected'), http.get('/protected')])
    expect(results.every((result) => result.status === 'rejected')).toBe(true)
    expect(adapter.history.post).toHaveLength(1)
    expect(getAccessToken()).toBeNull()
  })
  it('does not refresh again when renewed credentials are also rejected', async () => {
    adapter.onGet('/protected').reply(401)
    adapter.onPost('/api/auth/refresh').reply(async () => {
      await new Promise((resolve) => setTimeout(resolve, 20))
      return [200, { accessToken: 'renewed', expiresIn: 3600 }]
    })
    const results = await Promise.allSettled([http.get('/protected'), http.get('/protected')])
    expect(results.every((result) => result.status === 'rejected')).toBe(true)
    expect(adapter.history.post).toHaveLength(1)
  })
})
