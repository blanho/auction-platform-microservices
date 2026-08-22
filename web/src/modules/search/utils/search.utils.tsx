import { palette } from '@/shared/theme/tokens'
import { Category, Gavel, Store } from '@mui/icons-material'
import type { SearchResultType } from '../types'

export const getResultIcon = (type: SearchResultType) => {
  switch (type) {
    case 'auction':
      return <Gavel sx={{ color: palette.brand.primary }} />
    case 'category':
      return <Category sx={{ color: palette.semantic.info }} />
    case 'seller':
      return <Store sx={{ color: palette.semantic.success }} />
  }
}
