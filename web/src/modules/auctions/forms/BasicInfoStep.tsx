import {
  Autocomplete,
  FormControl,
  FormHelperText,
  Grid,
  InputLabel,
  MenuItem,
  Select,
  TextField,
} from '@mui/material'
import type { Control, FieldErrors } from 'react-hook-form'
import { Controller } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import type { Brand } from '../api/brands.api'
import type { Category } from '../api/categories.api'
import type { CreateAuctionFormData, UpdateAuctionFormData } from '../schemas'

interface BasicInfoStepProps {
  control: Control<CreateAuctionFormData | UpdateAuctionFormData>
  errors: FieldErrors<CreateAuctionFormData | UpdateAuctionFormData>
  isEditMode: boolean
  categories: Category[]
  brands: Brand[]
}

export function BasicInfoStep({
  control,
  errors,
  isEditMode,
  categories,
  brands,
}: BasicInfoStepProps) {
  const { t } = useTranslation('auctions')

  return (
    <Grid container spacing={3}>
      <Grid size={{ xs: 12 }}>
        <Controller
          name="title"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              fullWidth
              label={t('form.title')}
              placeholder={t('form.titlePlaceholder')}
              error={Boolean(errors.title)}
              helperText={errors.title?.message || t('form.titleHelper')}
              required
              slotProps={{ htmlInput: { maxLength: 200 } }}
            />
          )}
        />
      </Grid>

      <Grid size={{ xs: 12 }}>
        <Controller
          name="description"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              fullWidth
              label={t('form.description')}
              placeholder={t('form.descriptionPlaceholder')}
              multiline
              rows={6}
              error={Boolean(errors.description)}
              helperText={errors.description?.message || t('form.descriptionHelper')}
              required
              slotProps={{ htmlInput: { maxLength: 4000 } }}
            />
          )}
        />
      </Grid>

      <Grid size={{ xs: 12, md: 6 }}>
        <Controller
          name="categoryId"
          control={control}
          render={({ field }) => (
            <FormControl fullWidth error={Boolean(errors.categoryId)} required>
              <InputLabel id="category-label">{t('form.category')}</InputLabel>
              <Select {...field} labelId="category-label" label={t('form.category')}>
                {categories.map((cat) => (
                  <MenuItem key={cat.id} value={cat.id}>
                    {cat.name}
                  </MenuItem>
                ))}
              </Select>
              {errors.categoryId && (
                <FormHelperText role="alert">{errors.categoryId.message}</FormHelperText>
              )}
            </FormControl>
          )}
        />
      </Grid>

      {!isEditMode && (
        <Grid size={{ xs: 12, md: 6 }}>
          <Controller
            name={'brandId' as keyof CreateAuctionFormData}
            control={control}
            render={({ field }) => (
              <Autocomplete
                options={brands}
                getOptionLabel={(option) => option.name}
                value={brands.find((b) => b.id === field.value) || null}
                onChange={(_, newValue) => field.onChange(newValue?.id || '')}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label={t('form.brandOptional')}
                    placeholder={t('form.selectBrand')}
                  />
                )}
              />
            )}
          />
        </Grid>
      )}
    </Grid>
  )
}
