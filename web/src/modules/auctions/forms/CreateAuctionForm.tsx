import { FormField } from '@/shared/ui'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Box,
  Button,
  FormControl,
  FormHelperText,
  Grid,
  InputAdornment,
  InputLabel,
  MenuItem,
  Select,
  TextField,
} from '@mui/material'
import { Controller, useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { CURRENCIES, ITEM_CONDITIONS } from '../constants'
import { useActiveCategories } from '../hooks'
import { createAuctionSchema, type CreateAuctionFormData } from '../schemas'

interface CreateAuctionFormProps {
  onSubmit: (data: CreateAuctionFormData) => void
  isLoading?: boolean
}

export const CreateAuctionForm = ({ onSubmit, isLoading }: CreateAuctionFormProps) => {
  const { t } = useTranslation('auctions')
  const { data: categories = [] } = useActiveCategories()
  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<CreateAuctionFormData>({
    resolver: zodResolver(createAuctionSchema(t)),
    defaultValues: {
      currency: 'USD',
      isFeatured: false,
    },
  })

  return (
    <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={3}>
        <Grid size={{ xs: 12 }}>
          <FormField
            name="title"
            register={register}
            errors={errors}
            fullWidth
            label={t('form.title')}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <FormField
            name="description"
            register={register}
            errors={errors}
            fullWidth
            label={t('form.description')}
            multiline
            rows={4}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="categoryId"
            control={control}
            render={({ field }) => (
              <FormControl fullWidth error={!!errors.categoryId}>
                <InputLabel>{t('form.category')}</InputLabel>
                <Select {...field} label={t('form.category')}>
                  {categories.map((cat) => (
                    <MenuItem key={cat.id} value={cat.id}>
                      {cat.name}
                    </MenuItem>
                  ))}
                </Select>
                {errors.categoryId && <FormHelperText>{errors.categoryId.message}</FormHelperText>}
              </FormControl>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="condition"
            control={control}
            render={({ field }) => (
              <FormControl fullWidth>
                <InputLabel>{t('form.condition')}</InputLabel>
                <Select {...field} label={t('form.condition')}>
                  {ITEM_CONDITIONS.map((cond) => (
                    <MenuItem key={cond.value} value={cond.value}>
                      {t(`condition.${cond.value === 'like-new' ? 'likeNew' : cond.value}`)}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            fullWidth
            label={t('form.reservePrice')}
            type="number"
            slotProps={{
              input: {
                startAdornment: <InputAdornment position="start">$</InputAdornment>,
              },
            }}
            {...register('reservePrice', { valueAsNumber: true })}
            error={!!errors.reservePrice}
            helperText={errors.reservePrice?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            fullWidth
            label={t('form.buyNowPriceOptional')}
            type="number"
            slotProps={{
              input: {
                startAdornment: <InputAdornment position="start">$</InputAdornment>,
              },
            }}
            {...register('buyNowPrice', { valueAsNumber: true })}
            error={!!errors.buyNowPrice}
            helperText={errors.buyNowPrice?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="currency"
            control={control}
            render={({ field }) => (
              <FormControl fullWidth>
                <InputLabel>{t('form.currency')}</InputLabel>
                <Select {...field} label={t('form.currency')}>
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
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            fullWidth
            label={t('form.auctionEnd')}
            type="datetime-local"
            slotProps={{ inputLabel: { shrink: true } }}
            {...register('auctionEnd')}
            error={!!errors.auctionEnd}
            helperText={errors.auctionEnd?.message}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <Button type="submit" variant="contained" size="large" fullWidth disabled={isLoading}>
            {t(isLoading ? 'form.creating' : 'createAuction')}
          </Button>
        </Grid>
      </Grid>
    </Box>
  )
}
