import { componentStyles } from '@/shared/theme/component-styles'
import { palette } from '@/shared/theme/tokens'
import { Close } from '@mui/icons-material'
import {
  Box,
  Button,
  Checkbox,
  FormControlLabel,
  FormGroup,
  IconButton,
  Typography,
} from '@mui/material'
import { useTranslation } from 'react-i18next'
import type { Category } from '../api/categories.api'

interface AuctionBrowseFiltersProps {
  categories: Category[]
  selectedCategoryId?: string
  onCategoryChange: (categoryId?: string) => void
  onClose?: () => void
}

export function AuctionBrowseFilters({
  categories,
  selectedCategoryId,
  onCategoryChange,
  onClose,
}: Readonly<AuctionBrowseFiltersProps>) {
  const { t } = useTranslation('auctions')

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h6" sx={{ fontWeight: 500, color: palette.neutral[900] }}>
          {t('filter.title')}
        </Typography>
        {onClose && (
          <IconButton onClick={onClose} aria-label={t('common:actions.close')}>
            <Close />
          </IconButton>
        )}
      </Box>

      {selectedCategoryId && (
        <Button
          size="small"
          onClick={() => onCategoryChange(undefined)}
          sx={{ ...componentStyles.textBrandButton, mb: 2 }}
        >
          {t('filter.clearAll')}
        </Button>
      )}

      <Typography variant="subtitle2" sx={componentStyles.sectionLabel}>
        {t('filter.category')}
      </Typography>
      <FormGroup>
        {categories.map((category) => (
          <FormControlLabel
            key={category.id}
            control={
              <Checkbox
                size="small"
                checked={selectedCategoryId === category.id}
                onChange={(event) =>
                  onCategoryChange(event.target.checked ? category.id : undefined)
                }
                sx={componentStyles.brandCheckbox}
              />
            }
            label={
              <Box sx={{ display: 'flex', justifyContent: 'space-between', width: '100%' }}>
                <Typography variant="body2" sx={{ color: palette.neutral[700] }}>
                  {category.name}
                </Typography>
                {category.auctionCount !== undefined && (
                  <Typography variant="body2" sx={{ color: palette.neutral[500] }}>
                    ({category.auctionCount})
                  </Typography>
                )}
              </Box>
            }
            sx={{ width: '100%', mr: 0, '& .MuiFormControlLabel-label': { flex: 1 } }}
          />
        ))}
      </FormGroup>
    </Box>
  )
}
