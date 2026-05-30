import { Controller } from 'react-hook-form'
import type { Control, FieldErrors } from 'react-hook-form'
import {
  Grid,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormControlLabel,
  Switch,
  Divider,
  Typography,
  Box,
  Button,
  InputAdornment,
} from '@mui/material'
import { useTranslation } from 'react-i18next'
import { InlineAlert } from '@/shared/ui'
import { CURRENCIES, AUCTION_DURATIONS } from '../constants'
import type { CreateAuctionFormData, UpdateAuctionFormData } from '../schemas'

interface PricingStepProps {
  control: Control<CreateAuctionFormData | UpdateAuctionFormData>
  errors: FieldErrors<CreateAuctionFormData | UpdateAuctionFormData>
  isEditMode: boolean
  enableBuyNow: boolean
  onToggleBuyNow: (enabled: boolean) => void
  onSetDuration: (days: number) => void
}

export function PricingStep({
  control,
  errors,
  isEditMode,
  enableBuyNow,
  onToggleBuyNow,
  onSetDuration,
}: PricingStepProps) {
  const { t } = useTranslation('auctions')

  if (isEditMode) {
    return <InlineAlert severity="info">{t('form.pricingNotModifiable')}</InlineAlert>
  }

  const typedErrors = errors as Record<string, { message?: string }>

  return (
    <Grid container spacing={3}>
      <Grid size={{ xs: 12, md: 6 }}>
        <Controller
          name={'reservePrice' as keyof CreateAuctionFormData}
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              fullWidth
              label={t('form.startingPrice')}
              type="number"
              slotProps={{
                input: {
                  startAdornment: <InputAdornment position="start">$</InputAdornment>,
                },
              }}
              onChange={(e) => field.onChange(Number(e.target.value))}
              error={Boolean(typedErrors.reservePrice)}
              helperText={typedErrors.reservePrice?.message || t('form.startingPriceHelper')}
              required
            />
          )}
        />
      </Grid>

      <Grid size={{ xs: 12, md: 6 }}>
        <Controller
          name={'currency' as keyof CreateAuctionFormData}
          control={control}
          render={({ field }) => (
            <FormControl fullWidth>
              <InputLabel id="currency-label">{t('form.currency')}</InputLabel>
              <Select {...field} labelId="currency-label" label={t('form.currency')}>
                {CURRENCIES.map((cur) => (
                  <MenuItem key={cur.value} value={cur.value}>
                    {cur.label}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          )}
        />
      </Grid>

      <Grid size={{ xs: 12 }}>
        <FormControlLabel
          control={
            <Switch checked={enableBuyNow} onChange={(e) => onToggleBuyNow(e.target.checked)} />
          }
          label={t('form.enableBuyNow')}
        />
        {enableBuyNow && (
          <Controller
            name={'buyNowPrice' as keyof CreateAuctionFormData}
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                fullWidth
                label={t('form.buyNowPrice')}
                type="number"
                slotProps={{
                  input: {
                    startAdornment: <InputAdornment position="start">$</InputAdornment>,
                  },
                }}
                onChange={(e) => field.onChange(Number(e.target.value))}
                error={Boolean(typedErrors.buyNowPrice)}
                helperText={typedErrors.buyNowPrice?.message || t('form.buyNowPriceHelper')}
                sx={{ mt: 2 }}
              />
            )}
          />
        )}
      </Grid>

      <Grid size={{ xs: 12 }}>
        <Divider sx={{ my: 2 }} />
        <Typography variant="subtitle1" gutterBottom>
          {t('form.auctionDuration')}
        </Typography>
        <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 2 }}>
          {AUCTION_DURATIONS.map((duration) => (
            <Button
              key={duration.value}
              variant="outlined"
              size="small"
              onClick={() => onSetDuration(duration.value)}
              sx={{ minWidth: 80 }}
            >
              {duration.label}
            </Button>
          ))}
        </Box>
        <Controller
          name={'auctionEnd' as keyof CreateAuctionFormData}
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              fullWidth
              label={t('form.auctionEnd')}
              type="datetime-local"
              slotProps={{ inputLabel: { shrink: true } }}
              error={Boolean(typedErrors.auctionEnd)}
              helperText={typedErrors.auctionEnd?.message || t('form.auctionEndHelper')}
              required
            />
          )}
        />
      </Grid>
    </Grid>
  )
}
