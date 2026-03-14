import type { CreateAuctionFormData } from '../schemas'
import { addDays, formatDateTimeLocal } from './date.utils'

export function getDefaultCreateValues(): CreateAuctionFormData {
  return {
    title: '',
    description: '',
    categoryId: '',
    brandId: '',
    condition: '',
    yearManufactured: undefined,
    reservePrice: 0,
    buyNowPrice: undefined,
    auctionEnd: formatDateTimeLocal(addDays(new Date(), 7)),
    currency: 'USD',
    isFeatured: false,
  }
}
