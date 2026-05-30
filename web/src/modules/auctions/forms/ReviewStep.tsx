import { Box, Grid, Typography } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { InlineAlert } from '@/shared/ui'
import { ITEM_CONDITIONS, CURRENCIES } from '../constants'
import type { CreateAuctionFormData, UpdateAuctionFormData } from '../schemas'
import type { Category } from '../api/categories.api'
import type { Brand } from '../api/brands.api'
import type { FileAttachment } from '@/shared/types/storage.types'

interface ReviewStepProps {
  formValues: Partial<CreateAuctionFormData & UpdateAuctionFormData>
  isEditMode: boolean
  categories: Category[]
  brands: Brand[]
  attachments: FileAttachment[]
  enableBuyNow: boolean
}

interface ReviewFieldProps {
  label: string
  value: React.ReactNode
  size?: { xs: number; md?: number }
}

function ReviewField({ label, value, size = { xs: 12 } }: ReviewFieldProps) {
  return (
    <Grid size={size}>
      <Typography variant="subtitle2" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="body1">{value || '-'}</Typography>
    </Grid>
  )
}

export function ReviewStep({
  formValues,
  isEditMode,
  categories,
  brands,
  attachments,
  enableBuyNow,
}: ReviewStepProps) {
  const { t } = useTranslation('auctions')

  const categoryName = categories.find((c) => c.id === formValues.categoryId)?.name
  const brandName = brands.find((b) => b.id === formValues.brandId)?.name
  const conditionLabel = ITEM_CONDITIONS.find((c) => c.value === formValues.condition)?.label
  const currencyLabel = CURRENCIES.find((c) => c.value === formValues.currency)?.label

  const isCreateMode = !isEditMode && 'reservePrice' in formValues

  return (
    <Box>
      <InlineAlert severity="info" title={t('formSteps.review')} sx={{ mb: 3 }}>
        {isEditMode ? t('form.reviewUpdateDescription') : t('form.reviewCreateDescription')}
      </InlineAlert>

      <Grid container spacing={2}>
        <ReviewField label={t('form.title')} value={formValues.title} />
        <ReviewField
          label={t('form.description')}
          value={
            <Typography sx={{ whiteSpace: 'pre-wrap' }}>{formValues.description || '-'}</Typography>
          }
        />
        <ReviewField label={t('form.category')} value={categoryName} size={{ xs: 12, md: 6 }} />

        {!isEditMode && brandName && (
          <ReviewField label={t('form.brand')} value={brandName} size={{ xs: 12, md: 6 }} />
        )}

        {formValues.condition && (
          <ReviewField
            label={t('form.condition')}
            value={conditionLabel}
            size={{ xs: 12, md: 6 }}
          />
        )}

        {formValues.yearManufactured && (
          <ReviewField
            label={t('detail.yearManufactured')}
            value={formValues.yearManufactured}
            size={{ xs: 12, md: 6 }}
          />
        )}

        {isCreateMode && (
          <>
            {attachments.length > 0 && (
              <ReviewField
                label={t('form.filesAttached')}
                value={t('form.fileCount', { count: attachments.length })}
                size={{ xs: 12, md: 6 }}
              />
            )}

            <ReviewField
              label={t('form.startingPrice')}
              value={
                <Typography sx={{ fontWeight: 600, color: 'primary.main' }}>
                  ${formValues.reservePrice}
                </Typography>
              }
              size={{ xs: 12, md: 6 }}
            />

            {enableBuyNow && formValues.buyNowPrice && (
              <ReviewField
                label={t('form.buyNowPrice')}
                value={
                  <Typography sx={{ fontWeight: 600, color: 'success.main' }}>
                    ${formValues.buyNowPrice}
                  </Typography>
                }
                size={{ xs: 12, md: 6 }}
              />
            )}

            <ReviewField
              label={t('form.auctionEnd')}
              value={formValues.auctionEnd ? new Date(formValues.auctionEnd).toLocaleString() : '-'}
              size={{ xs: 12, md: 6 }}
            />

            <ReviewField
              label={t('form.currency')}
              value={currencyLabel || 'USD'}
              size={{ xs: 12, md: 6 }}
            />
          </>
        )}
      </Grid>
    </Box>
  )
}
