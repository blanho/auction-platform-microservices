const REDIRECT_KEY = 'auction_redirect_url'

export function saveRedirectUrl(url: string) {
  sessionStorage.setItem(REDIRECT_KEY, url)
}

export function getAndClearRedirectUrl(): string | null {
  const url = sessionStorage.getItem(REDIRECT_KEY)
  sessionStorage.removeItem(REDIRECT_KEY)
  return url
}
