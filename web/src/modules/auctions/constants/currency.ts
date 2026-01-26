export const DEFAULT_CURRENCY = 'USD'
export const DEFAULT_LOCALE = 'en-US'

export const CURRENCY_FORMAT_OPTIONS: Intl.NumberFormatOptions = {
  style: 'currency',
  currency: DEFAULT_CURRENCY,
  minimumFractionDigits: 0,
  maximumFractionDigits: 2,
}
