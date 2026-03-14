import { DEFAULT_LOCALE, CURRENCY_FORMAT_OPTIONS } from '../constants'

export function formatCurrency(
  amount: number,
  locale: string = DEFAULT_LOCALE,
  options: Intl.NumberFormatOptions = CURRENCY_FORMAT_OPTIONS
): string {
  return new Intl.NumberFormat(locale, options).format(amount)
}

export function formatCompactCurrency(amount: number): string {
  if (amount >= 1_000_000) {
    return `$${(amount / 1_000_000).toFixed(1)}M`
  }
  if (amount >= 1_000) {
    return `$${(amount / 1_000).toFixed(1)}K`
  }
  return formatCurrency(amount)
}
