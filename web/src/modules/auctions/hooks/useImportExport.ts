import { useMutation } from '@tanstack/react-query'
import { auctionsApi } from '../api/auctions.api'
import type { ExportFilters, ImportAuctionDto } from '../types/import-export.types'

export function useExportAuctions() {
  return useMutation({
    mutationFn: async (filters: ExportFilters) => {
      const result = await auctionsApi.exportAuctions(filters)

      if (result instanceof Blob) {
        const extension = filters.format === 'csv' ? 'csv' : 'xlsx'
        const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, 19)
        auctionsApi.downloadExport(result, `auctions-export-${timestamp}.${extension}`)
        return { downloaded: true, count: 0 }
      }

      return { downloaded: false, count: result.length, data: result }
    },
  })
}

export function useImportAuctionsFile() {
  return useMutation({
    mutationFn: (file: File) => auctionsApi.importAuctionsFile(file),
  })
}

export function useImportAuctionsJson() {
  return useMutation({
    mutationFn: (auctions: ImportAuctionDto[]) => auctionsApi.importAuctionsJson(auctions),
  })
}
