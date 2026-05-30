import { Controller } from 'react-hook-form'
import type { Control } from 'react-hook-form'
import {
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Typography,
  Autocomplete,
  TextField,
} from '@mui/material'
import { useTranslation } from 'react-i18next'
import { InlineAlert } from '@/shared/ui'
import { FileUploadZone } from '@/shared/components/upload'
import { ACCEPTED_IMAGE_TYPES } from '@/shared/constants/storage.constants'
import { ITEM_CONDITIONS, YEAR_OPTIONS } from '../constants'
import type { CreateAuctionFormData, UpdateAuctionFormData } from '../schemas'
import type { FileAttachment, FileUploadProgress } from '@/shared/types/storage.types'

interface ItemDetailsStepProps {
  control: Control<CreateAuctionFormData | UpdateAuctionFormData>
  isEditMode: boolean
  attachments: FileAttachment[]
  uploads: FileUploadProgress[]
  isUploading: boolean
  onFilesSelected: (files: File[]) => Promise<unknown>
  onRemoveAttachment: (fileId: string) => void
  onSetPrimaryAttachment: (fileId: string) => void
}

export function ItemDetailsStep({
  control,
  isEditMode,
  attachments,
  uploads,
  isUploading,
  onFilesSelected,
  onRemoveAttachment,
  onSetPrimaryAttachment,
}: ItemDetailsStepProps) {
  const { t } = useTranslation('auctions')

  return (
    <Grid container spacing={3}>
      <Grid size={{ xs: 12, md: 6 }}>
        <Controller
          name="condition"
          control={control}
          render={({ field }) => (
            <FormControl fullWidth>
              <InputLabel id="condition-label">{t('form.condition')}</InputLabel>
              <Select {...field} labelId="condition-label" label={t('form.condition')}>
                {ITEM_CONDITIONS.map((cond) => (
                  <MenuItem key={cond.value} value={cond.value}>
                    <Typography variant="body1">{cond.label}</Typography>
                    <Typography variant="caption" color="text.secondary">
                      {cond.description}
                    </Typography>
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          )}
        />
      </Grid>

      <Grid size={{ xs: 12, md: 6 }}>
        <Controller
          name="yearManufactured"
          control={control}
          render={({ field }) => (
            <Autocomplete
              options={YEAR_OPTIONS}
              getOptionLabel={String}
              value={field.value || null}
              onChange={(_, newValue) => field.onChange(newValue || undefined)}
              renderInput={(params) => (
                <TextField
                  {...params}
                  label={t('form.yearManufacturedOptional')}
                  placeholder={t('form.selectYear')}
                />
              )}
            />
          )}
        />
      </Grid>

      <Grid size={{ xs: 12 }}>
        <InlineAlert severity="info" title={t('form.itemDetails')} sx={{ mt: 1 }}>
          {t('form.itemDetailsHelper')}
        </InlineAlert>
      </Grid>

      {!isEditMode && (
        <Grid size={{ xs: 12 }}>
          <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1 }}>
            {t('form.photosFiles')}
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            {t('form.photosFilesHelper')}
          </Typography>
          <FileUploadZone
            attachments={attachments}
            uploads={uploads}
            isUploading={isUploading}
            onFilesSelected={onFilesSelected}
            onRemove={onRemoveAttachment}
            onSetPrimary={onSetPrimaryAttachment}
            acceptedTypes={ACCEPTED_IMAGE_TYPES}
          />
        </Grid>
      )}
    </Grid>
  )
}
