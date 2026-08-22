const CSRF_COOKIE_NAME = 'XSRF-TOKEN'

export function getCsrfToken(): string | null {
  if (typeof document === 'undefined') {
    return null
  }

  const cookies = document.cookie.split(';')
  for (const cookie of cookies) {
    const [name, value] = cookie.trim().split('=')
    if (name === CSRF_COOKIE_NAME && value) {
      try {
        return decodeURIComponent(value)
      } catch {
        return value
      }
    }
  }
  return null
}
