export interface ExportAuctionDto {
  id: string
  title: string
  description: string
  condition?: string
  yearManufactured?: number
  reservePrice: number
  currency: string
  seller: string
  winner?: string
  soldAmount?: number
  currentHighBid?: number
  createdAt: string
  auctionEnd: string
  status: string
}

export interface ImportAuctionDto {
  title: string
  description: string
  condition?: string
  yearManufactured?: number
  attributes?: Record<string, string>
  reservePrice: number
  currency?: string
  auctionEnd: string
}

export interface ImportResultItem {
  rowNumber: number
  success: boolean
  auctionId?: string
  error?: string
}

export interface ImportAuctionsResult {
  totalRows: number
  successCount: number
  failureCount: number
  results: ImportResultItem[]
}

export interface ExportFilters {
  status?: string
  seller?: string
  startDate?: string
  endDate?: string
  format: 'json' | 'csv' | 'excel'
}

export type BulkImportStatus = 
  | 'Pending'
  | 'Counting'
  | 'Processing'
  | 'Completed'
  | 'Failed'
  | 'Cancelled'

export interface BulkImportError {
  rowNumber: number
  error: string
}

export interface BulkImportProgress {
  jobId: string
  status: BulkImportStatus
  totalRows: number
  processedRows: number
  successCount: number
  failureCount: number
  progressPercentage: number
  estimatedSecondsRemaining: number
  recentErrors: BulkImportError[]
  errorMessage?: string
}

export interface BulkImportStartResponse {
  jobId: string
  message: string
}
