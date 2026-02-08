const REDIRECT_KEY = 'auction_redirect_url'

export function saveRedirectUrl(url: string) {
  sessionStorage.setItem(REDIRECT_KEY, url)
}

export function getRedirectUrl(): string | null {
  return sessionStorage.getItem(REDIRECT_KEY)
}

export function clearRedirectUrl(): void {
  sessionStorage.removeItem(REDIRECT_KEY)
}

export function getAndClearRedirectUrl(): string | null {
  const url = sessionStorage.getItem(REDIRECT_KEY)
  sessionStorage.removeItem(REDIRECT_KEY)
  return url
}
