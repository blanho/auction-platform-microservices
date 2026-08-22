import type { FileAttachment } from '@/shared/types/storage.types'
import { InlineAlert } from '@/shared/ui'
import { formatCurrency, formatDateTime } from '@/shared/utils/formatters'
import { Box, Grid, Typography } from '@mui/material'
import { useTranslation } from 'react-i18next'
import type { Brand } from '../api/brands.api'
import type { Category } from '../api/categories.api'
import { CURRENCIES, ITEM_CONDITIONS } from '../constants'
import type { CreateAuctionFormData, UpdateAuctionFormData } from '../schemas'

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
  const condition = ITEM_CONDITIONS.find((c) => c.value === formValues.condition)?.value
  const conditionLabel = condition
    ? t(`condition.${condition === 'like-new' ? 'likeNew' : condition}`)
    : undefined
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
                  {formatCurrency(Number(formValues.reservePrice), formValues.currency)}
                </Typography>
              }
              size={{ xs: 12, md: 6 }}
            />

            {enableBuyNow && formValues.buyNowPrice && (
              <ReviewField
                label={t('form.buyNowPrice')}
                value={
                  <Typography sx={{ fontWeight: 600, color: 'success.main' }}>
                    {formatCurrency(Number(formValues.buyNowPrice), formValues.currency)}
                  </Typography>
                }
                size={{ xs: 12, md: 6 }}
              />
            )}

            <ReviewField
              label={t('form.auctionEnd')}
              value={formValues.auctionEnd ? formatDateTime(formValues.auctionEnd) : '-'}
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
