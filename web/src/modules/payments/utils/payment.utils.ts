import { CARD_BRANDS, STRIPE_CONFIG } from '../constants'

export function formatCardBrand(brand?: string): string {
  if (!brand) return 'Card'
  return CARD_BRANDS[brand.toLowerCase()] || brand
}

export function formatCardNumber(last4: string, brand?: string): string {
  return `${formatCardBrand(brand)} •••• ${last4}`
}

export function formatCardExpiry(month?: number, year?: number): string {
  if (!month || !year) return ''
  const formattedMonth = month.toString().padStart(2, '0')
  const formattedYear = year.toString().slice(-2)
  return `${formattedMonth}/${formattedYear}`
}

export function isCardExpired(month?: number, year?: number): boolean {
  if (!month || !year) return false
  const now = new Date()
  const expiryDate = new Date(year, month)
  return expiryDate < now
}

export function centsToAmount(cents: number): number {
  return cents / 100
}

export function amountToCents(amount: number): number {
  return Math.round(amount * 100)
}

export function isValidPaymentAmount(amountInCents: number): boolean {
  return (
    amountInCents >= STRIPE_CONFIG.MIN_AMOUNT_CENTS &&
    amountInCents <= STRIPE_CONFIG.MAX_AMOUNT_CENTS
  )
}

export function formatPaymentAmount(amountInCents: number, currency: string = 'USD'): string {
  const amount = centsToAmount(amountInCents)
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: currency.toUpperCase(),
  }).format(amount)
}
