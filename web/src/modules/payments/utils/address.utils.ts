import type { ShippingAddress } from '../types/order.types'

export function serializeShippingAddress(address: ShippingAddress): string {
  return JSON.stringify(address)
}

export function parseShippingAddress(addressString: string | ShippingAddress | undefined): ShippingAddress | null {
  if (!addressString) {
    return null
  }

  if (typeof addressString === 'object') {
    return addressString as ShippingAddress
  }

  try {
    return JSON.parse(addressString) as ShippingAddress
  } catch {
    return {
      fullName: '',
      addressLine1: addressString,
      city: '',
      state: '',
      postalCode: '',
      country: '',
    }
  }
}

export function formatShippingAddressForDisplay(address: ShippingAddress | string | undefined): string {
  if (!address) {
    return 'No address provided'
  }

  const parsed = typeof address === 'string' ? parseShippingAddress(address) : address

  if (!parsed) {
    return 'Invalid address'
  }

  const lines = [
    parsed.fullName,
    parsed.addressLine1,
    parsed.addressLine2,
    `${parsed.city}, ${parsed.state} ${parsed.postalCode}`,
    parsed.country,
    parsed.phone ? `Phone: ${parsed.phone}` : null,
  ].filter(Boolean)

  return lines.join('\n')
}
