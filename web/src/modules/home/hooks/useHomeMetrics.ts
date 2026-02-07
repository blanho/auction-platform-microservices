import { useMemo } from 'react'
import { useAuctions } from '@/modules/auctions/hooks/useAuctions'
import { useActiveCategories } from '@/modules/auctions/hooks/useCategories'
import { useBrands } from '@/modules/auctions/hooks/useBrands'

export interface HomeMetrics {
  activeAuctionsCount: number
  totalAuctionsCount: number
  categoriesCount: number
  brandsCount: number
  featuredBrandsCount: number
}

export function useHomeMetrics(): HomeMetrics {
  const { data: activeAuctionsData } = useAuctions({ status: 'active', page: 1, pageSize: 1 })
  const { data: totalAuctionsData } = useAuctions({ page: 1, pageSize: 1 })
  const { data: categoriesData } = useActiveCategories()
  const { data: brandsData } = useBrands({ activeOnly: true, page: 1, pageSize: 1 })
  const { data: featuredBrandsData } = useBrands({ activeOnly: true, featuredOnly: true, page: 1, pageSize: 1 })

  return useMemo(
    () => ({
      activeAuctionsCount: activeAuctionsData?.totalCount ?? 0,
      totalAuctionsCount: totalAuctionsData?.totalCount ?? 0,
      categoriesCount: categoriesData?.length ?? 0,
      brandsCount: brandsData?.totalCount ?? 0,
      featuredBrandsCount: featuredBrandsData?.totalCount ?? 0,
    }),
    [
      activeAuctionsData?.totalCount,
      totalAuctionsData?.totalCount,
      categoriesData?.length,
      brandsData?.totalCount,
      featuredBrandsData?.totalCount,
    ]
  )
}
