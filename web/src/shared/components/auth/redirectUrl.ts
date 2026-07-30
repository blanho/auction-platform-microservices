const REDIRECT_KEY = 'auction_redirect_url'

export function isSafeInternalRedirect(url: string | null): url is string {
  return Boolean(url?.startsWith('/') && !url.startsWith('//') && !url.includes('\\'))
}

export function saveRedirectUrl(url: string) {
  if (isSafeInternalRedirect(url)) {
    sessionStorage.setItem(REDIRECT_KEY, url)
  }
}

export function getRedirectUrl(): string | null {
  const url = sessionStorage.getItem(REDIRECT_KEY)
  return isSafeInternalRedirect(url) ? url : null
}

export function clearRedirectUrl(): void {
  sessionStorage.removeItem(REDIRECT_KEY)
}

export function getAndClearRedirectUrl(): string | null {
  const url = sessionStorage.getItem(REDIRECT_KEY)
  sessionStorage.removeItem(REDIRECT_KEY)
  return isSafeInternalRedirect(url) ? url : null
}
