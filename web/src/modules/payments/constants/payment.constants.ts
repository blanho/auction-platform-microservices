export const PAYMENT_METHODS = {
  CARD: 'card',
  BANK_ACCOUNT: 'bank_account',
  WALLET: 'wallet',
} as const

export const PAYMENT_METHOD_LABELS: Record<string, string> = {
  card: 'Credit/Debit Card',
  bank_account: 'Bank Account',
  wallet: 'Wallet Balance',
}

export const CARD_BRANDS: Record<string, string> = {
  visa: 'Visa',
  mastercard: 'Mastercard',
  amex: 'American Express',
  discover: 'Discover',
  diners: 'Diners Club',
  jcb: 'JCB',
  unionpay: 'UnionPay',
}

export const STRIPE_CONFIG = {
  MIN_AMOUNT_CENTS: 50,
  MAX_AMOUNT_CENTS: 99999999,
  DEFAULT_CURRENCY: 'usd',
} as const
