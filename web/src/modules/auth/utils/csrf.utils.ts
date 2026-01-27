const CSRF_COOKIE_NAME = 'XSRF-TOKEN'
const CSRF_HEADER_NAME = 'X-XSRF-TOKEN'

export function getCsrfToken(): string | null {
  if (typeof document === 'undefined') return null

  const cookies = document.cookie.split(';')
  for (const cookie of cookies) {
    const [name, value] = cookie.trim().split('=')
    if (name === CSRF_COOKIE_NAME && value) {
      return decodeURIComponent(value)
    }
  }
  return null
}

export function getCsrfHeader(): Record<string, string> {
  const token = getCsrfToken()
  if (!token) return {}
  return { [CSRF_HEADER_NAME]: token }
}

export function attachCsrfToRequest(headers: Record<string, string>): Record<string, string> {
  const token = getCsrfToken()
  if (token) {
    return { ...headers, [CSRF_HEADER_NAME]: token }
  }
  return headers
}

export function isSafeMethod(method: string): boolean {
  const safeMethods = ['GET', 'HEAD', 'OPTIONS']
  return safeMethods.includes(method.toUpperCase())
}

export function shouldIncludeCsrf(method: string): boolean {
  return !isSafeMethod(method)
}
