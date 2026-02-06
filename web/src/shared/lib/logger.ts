type LogLevel = 'debug' | 'info' | 'warn' | 'error'

interface LoggerConfig {
  enabledInProduction: boolean
  prefix?: string
}

const defaultConfig: LoggerConfig = {
  enabledInProduction: false,
}

function shouldLog(level: LogLevel, config: LoggerConfig): boolean {
  if (level === 'error' || level === 'warn') {
    return true
  }

  if (import.meta.env.DEV) {
    return true
  }

  return config.enabledInProduction
}

function formatMessage(prefix: string | undefined, ...args: unknown[]): unknown[] {
  if (prefix) {
    return [`[${prefix}]`, ...args]
  }
  return args
}

export function createLogger(config: Partial<LoggerConfig> = {}) {
  const mergedConfig = { ...defaultConfig, ...config }

  return {
    debug: (...args: unknown[]): void => {
      if (shouldLog('debug', mergedConfig)) {
        // eslint-disable-next-line no-console
        console.debug(...formatMessage(mergedConfig.prefix, ...args))
      }
    },

    info: (...args: unknown[]): void => {
      if (shouldLog('info', mergedConfig)) {
        // eslint-disable-next-line no-console
        console.info(...formatMessage(mergedConfig.prefix, ...args))
      }
    },

    warn: (...args: unknown[]): void => {
      if (shouldLog('warn', mergedConfig)) {
        console.warn(...formatMessage(mergedConfig.prefix, ...args))
      }
    },

    error: (...args: unknown[]): void => {
      if (shouldLog('error', mergedConfig)) {
        console.error(...formatMessage(mergedConfig.prefix, ...args))
      }
    },
  }
}

export const logger = createLogger()

export const signalRLogger = createLogger({ prefix: 'SignalR' })
export const authLogger = createLogger({ prefix: 'Auth' })
export const apiLogger = createLogger({ prefix: 'API' })
