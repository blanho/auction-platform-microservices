export const ITEM_CONDITIONS = [
  { value: 'new', label: 'New', description: 'Brand new, never used' },
  { value: 'like-new', label: 'Like New', description: 'Barely used, excellent condition' },
  { value: 'excellent', label: 'Excellent', description: 'Very good condition, minor wear' },
  { value: 'good', label: 'Good', description: 'Normal wear, fully functional' },
  { value: 'fair', label: 'Fair', description: 'Visible wear, some defects' },
] as const

export const CURRENCIES = [
  { value: 'USD', label: 'USD ($)', symbol: '$' },
  { value: 'EUR', label: 'EUR (€)', symbol: '€' },
  { value: 'GBP', label: 'GBP (£)', symbol: '£' },
  { value: 'VND', label: 'VND (₫)', symbol: '₫' },
] as const

export const FORM_STEPS = ['Basic Info', 'Item Details', 'Pricing & Duration', 'Review'] as const

export const AUCTION_DURATIONS = [
  { value: 1, label: '1 Day' },
  { value: 3, label: '3 Days' },
  { value: 5, label: '5 Days' },
  { value: 7, label: '7 Days' },
  { value: 10, label: '10 Days' },
  { value: 14, label: '14 Days' },
] as const

const currentYear = new Date().getFullYear()
export const YEAR_OPTIONS = Array.from({ length: currentYear - 1900 + 1 }, (_, i) => currentYear - i)

export type ItemCondition = (typeof ITEM_CONDITIONS)[number]['value']
export type Currency = (typeof CURRENCIES)[number]['value']
