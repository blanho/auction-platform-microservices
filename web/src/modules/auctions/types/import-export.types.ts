export interface ImportAuctionsResult {
  totalRows: number
  successCount: number
  failureCount: number
  errors: ImportAuctionError[]
}

export interface ImportAuctionError {
  row: number
  field: string
  message: string
}
