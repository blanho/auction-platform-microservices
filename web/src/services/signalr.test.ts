import { afterEach, expect, it, vi } from 'vitest'

const mocks = vi.hoisted(() => ({ withUrl: vi.fn(), start: vi.fn().mockResolvedValue(undefined) }))
vi.mock('@/i18n', () => ({ default: { t: (key: string) => key } }))
vi.mock('@/modules/auth/utils/token.utils', () => ({ getAccessToken: () => 'test-token' }))
vi.mock('@/shared/lib/logger', () => ({
  signalRLogger: { warn: vi.fn(), info: vi.fn(), error: vi.fn(), debug: vi.fn() },
}))
vi.mock('@microsoft/signalr', () => ({
  HubConnectionState: { Connected: 'Connected', Disconnected: 'Disconnected' },
  HttpTransportType: { WebSockets: 1, ServerSentEvents: 2 },
  LogLevel: { Warning: 3 },
  HubConnectionBuilder: class {
    withUrl(...args: unknown[]) {
      mocks.withUrl(...args)
      return this
    }
    withAutomaticReconnect() {
      return this
    }
    configureLogging() {
      return this
    }
    build() {
      return {
        state: 'Disconnected',
        start: mocks.start,
        stop: vi.fn(),
        onreconnecting: vi.fn(),
        onreconnected: vi.fn(),
        onclose: vi.fn(),
      }
    }
  },
}))

afterEach(() => {
  vi.unstubAllEnvs()
  vi.resetModules()
  vi.clearAllMocks()
})

it('negotiates on the same origin by default and permits Azure SignalR redirects', async () => {
  vi.stubEnv('VITE_SIGNALR_URL', '')
  const { signalRService } = await import('./signalr')
  await signalRService.connect()
  const [url, options] = mocks.withUrl.mock.calls[0]
  expect(url).toBe('/hubs/notifications')
  expect(options.skipNegotiation).not.toBe(true)
  expect(options.accessTokenFactory()).toBe('test-token')
  expect(mocks.start).toHaveBeenCalledOnce()
})

it('honors an explicitly configured hub endpoint', async () => {
  vi.stubEnv('VITE_SIGNALR_URL', 'https://api.example.com/hubs/notifications')
  const { signalRService } = await import('./signalr')
  await signalRService.connect()
  expect(mocks.withUrl.mock.calls[0][0]).toBe('https://api.example.com/hubs/notifications')
})
