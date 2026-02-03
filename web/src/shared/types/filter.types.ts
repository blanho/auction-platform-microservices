export interface FilterFieldOption<T = string> {
  value: T
  label: string
  disabled?: boolean
}

export interface BaseFilterField {
  key: string
  label: string
  placeholder?: string
  gridSize?: {
    xs?: number
    sm?: number
    md?: number
    lg?: number
  }
}

export interface TextFilterField extends BaseFilterField {
  type: 'text'
  debounceMs?: number
}

export interface SelectFilterField extends BaseFilterField {
  type: 'select'
  options: FilterFieldOption[]
  multiple?: boolean
  clearable?: boolean
}

export interface DateFilterField extends BaseFilterField {
  type: 'date'
  minDate?: Date
  maxDate?: Date
}

export interface DateRangeFilterField extends BaseFilterField {
  type: 'dateRange'
  startKey: string
  endKey: string
  minDate?: Date
  maxDate?: Date
}

export interface NumberRangeFilterField extends BaseFilterField {
  type: 'numberRange'
  minKey: string
  maxKey: string
  min?: number
  max?: number
  step?: number
}

export interface BooleanFilterField extends BaseFilterField {
  type: 'boolean'
  trueLabel?: string
  falseLabel?: string
}

export type FilterField =
  | TextFilterField
  | SelectFilterField
  | DateFilterField
  | DateRangeFilterField
  | NumberRangeFilterField
  | BooleanFilterField

export interface FilterPanelConfig {
  fields: FilterField[]
  collapsible?: boolean
  defaultExpanded?: boolean
  showClearButton?: boolean
  showApplyButton?: boolean
}

export interface SortField {
  key: string
  label: string
  defaultOrder?: 'asc' | 'desc'
}

export interface ColumnConfig<T> {
  key: keyof T | string
  header: string
  sortable?: boolean
  sortKey?: string
  width?: number | string
  minWidth?: number
  align?: 'left' | 'center' | 'right'
  render?: (value: unknown, row: T, index: number) => React.ReactNode
  hidden?: boolean
}

export interface AuctionFilter {
  search?: string
  status?: string
  categoryId?: string
  sellerId?: string
  minPrice?: number
  maxPrice?: number
  startDateFrom?: string
  startDateTo?: string
  endDateFrom?: string
  endDateTo?: string
}

export interface BidFilter {
  auctionId?: string
  bidderId?: string
  status?: string
  minAmount?: number
  maxAmount?: number
  dateFrom?: string
  dateTo?: string
}

export interface OrderFilter {
  buyerUsername?: string
  sellerUsername?: string
  search?: string
  status?: string
  paymentStatus?: string
  minAmount?: number
  maxAmount?: number
  dateFrom?: string
  dateTo?: string
}

export interface WalletTransactionFilter {
  username?: string
  transactionType?: string
  status?: string
  minAmount?: number
  maxAmount?: number
  dateFrom?: string
  dateTo?: string
}

export interface NotificationRecordFilter {
  userId?: string
  channel?: string
  status?: string
  templateKey?: string
  dateFrom?: string
  dateTo?: string
}

export interface UserFilter {
  search?: string
  role?: string
  status?: string
  isEmailVerified?: boolean
  createdFrom?: string
  createdTo?: string
}

export interface ReviewFilter {
  auctionId?: string
  reviewerId?: string
  revieweeId?: string
  minRating?: number
  maxRating?: number
  dateFrom?: string
  dateTo?: string
}
