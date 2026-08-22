import { getCurrentLocale } from '@/i18n'
import { CURRENCY_FORMAT_OPTIONS } from '../constants'

export function formatCurrency(
  amount: number,
  locale: string = getCurrentLocale(),
  options: Intl.NumberFormatOptions = CURRENCY_FORMAT_OPTIONS
): string {
  return new Intl.NumberFormat(locale, options).format(amount)
}
