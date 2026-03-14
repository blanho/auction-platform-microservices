import { useMutation } from '@tanstack/react-query'
import { http } from '@/services/http'
import type { ImportAuctionsResult } from '../types/import-export.types'

export function useImportAuctionsFile() {
  return useMutation({
    mutationFn: async (file: File): Promise<ImportAuctionsResult> => {
      const formData = new FormData()
      formData.append('file', file)

      const response = await http.post<ImportAuctionsResult>(
        '/auctions/import',
        formData,
        { headers: { 'Content-Type': 'multipart/form-data' } }
      )
      return response.data
    },
  })
}
