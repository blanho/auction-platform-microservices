import { useState, useEffect, useMemo } from 'react'
import { useNavigate, useParams, Link } from 'react-router-dom'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Container,
  Typography,
  Box,
  Card,
  Button,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormHelperText,
  Stepper,
  Step,
  StepLabel,
  Divider,
  CircularProgress,
  InputAdornment,
  Switch,
  FormControlLabel,
  Breadcrumbs,
  Link as MuiLink,
  Skeleton,
  Grid,
  Autocomplete,
} from '@mui/material'
import { InlineAlert } from '@/shared/ui'
import { ArrowBack, ArrowForward, Save } from '@mui/icons-material'
import { motion } from 'framer-motion'
import { palette } from '@/shared/theme/tokens'
import { fadeInUp, staggerContainer } from '@/shared/lib/animations'
import {
  createAuctionSchema,
  updateAuctionSchema,
  type CreateAuctionFormData,
  type UpdateAuctionFormData,
} from '../schemas'
import {
  useAuction,
  useCreateAuction,
  useUpdateAuction,
  useActiveCategories,
  useActiveBrands,
} from '../hooks'
import { useFileUpload } from '@/shared/hooks/useFileUpload'
import type { CreateAuctionRequest, UpdateAuctionRequest } from '../types'
import { getDefaultCreateValues } from '../utils'
import { FileUploadZone } from '@/shared/components/upload'
import { ACCEPTED_IMAGE_TYPES } from '@/shared/constants/storage.constants'
import { addDays, formatDateTimeLocal } from '../utils/date.utils'
import {
  ITEM_CONDITIONS,
  CURRENCIES,
  FORM_STEPS,
  AUCTION_DURATIONS,
  YEAR_OPTIONS,
} from '../constants'

function AuctionFormSkeleton() {
  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Skeleton variant="text" width={300} height={40} sx={{ mb: 2 }} />
      <Skeleton variant="text" width={200} height={24} sx={{ mb: 4 }} />
      <Card sx={{ p: 4 }}>
        <Skeleton variant="rectangular" height={60} sx={{ mb: 4 }} />
        <Grid container spacing={3}>
          <Grid size={{ xs: 12 }}>
            <Skeleton variant="rectangular" height={56} />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <Skeleton variant="rectangular" height={120} />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <Skeleton variant="rectangular" height={56} />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <Skeleton variant="rectangular" height={56} />
          </Grid>
        </Grid>
      </Card>
    </Container>
  )
}

export function AuctionFormPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const isEditMode = Boolean(id)

  const [activeStep, setActiveStep] = useState(0)
  const [enableBuyNow, setEnableBuyNow] = useState(false)

  const { data: categories = [], isLoading: isCategoriesLoading } = useActiveCategories()
  const { data: brands = [], isLoading: isBrandsLoading } = useActiveBrands()
  const {
    data: existingAuction,
    isLoading: isFetchingAuction,
    error: fetchError,
  } = useAuction(id ?? '')
  const createMutation = useCreateAuction()
  const updateMutation = useUpdateAuction()

  const {
    uploads,
    attachments,
    uploadFiles,
    removeAttachment,
    setPrimaryAttachment,
    isUploading,
  } = useFileUpload({
    subFolder: 'auctions',
    acceptedTypes: ACCEPTED_IMAGE_TYPES,
  })

  const isSubmitting = createMutation.isPending || updateMutation.isPending || isUploading
  const isLoading = isCategoriesLoading || isBrandsLoading || (isEditMode && isFetchingAuction)

  const schema = useMemo(
    () => (isEditMode ? updateAuctionSchema : createAuctionSchema),
    [isEditMode]
  )

  const {
    control,
    handleSubmit,
    trigger,
    reset,
    watch,
    setValue,
    formState: { errors },
  } = useForm<CreateAuctionFormData | UpdateAuctionFormData>({
    resolver: zodResolver(schema),
    defaultValues: isEditMode ? undefined : getDefaultCreateValues(),
    mode: 'onBlur',
  })

  useEffect(() => {
    if (isEditMode && existingAuction) {
      const auctionData = existingAuction as {
        title: string
        description: string
        categoryId: string
        condition?: string
        yearManufactured?: number
        buyNowPrice?: number
      }
      reset({
        title: auctionData.title,
        description: auctionData.description,
        categoryId: auctionData.categoryId,
        condition: auctionData.condition,
        yearManufactured: auctionData.yearManufactured,
      } as UpdateAuctionFormData)

      if (auctionData.buyNowPrice) {setEnableBuyNow(true)}
    }
  }, [existingAuction, isEditMode, reset])

  const onSubmit = async (data: CreateAuctionFormData | UpdateAuctionFormData) => {
    try {
      if (isEditMode && id) {
        const updateData: UpdateAuctionRequest = {
          title: data.title,
          description: data.description,
          condition: data.condition,
          yearManufactured: data.yearManufactured,
        }
        await updateMutation.mutateAsync({ id, data: updateData })
      } else {
        const formData = data as CreateAuctionFormData
        const createData: CreateAuctionRequest = {
          title: formData.title,
          description: formData.description,
          condition: formData.condition,
          yearManufactured: formData.yearManufactured,
          reservePrice: formData.reservePrice,
          buyNowPrice: enableBuyNow ? formData.buyNowPrice : undefined,
          auctionEnd: new Date(formData.auctionEnd).toISOString(),
          categoryId: formData.categoryId,
          brandId: formData.brandId || undefined,
          currency: formData.currency,
          isFeatured: formData.isFeatured,
          files: attachments.map((a) => ({
            fileId: a.fileId,
            fileType: a.fileType,
            displayOrder: a.displayOrder,
            isPrimary: a.isPrimary,
          })),
        }
        await createMutation.mutateAsync(createData)
      }
      navigate('/my-auctions')
    } catch {
      /* Error handled by mutation */
    }
  }

  const handleNext = async () => {
    const getCreateFields = (): (keyof CreateAuctionFormData)[] => {
      switch (activeStep) {
        case 0:
          return ['title', 'description', 'categoryId']
        case 1:
          return ['condition']
        case 2:
          return ['reservePrice', 'auctionEnd']
        default:
          return []
      }
    }

    const getEditFields = (): (keyof UpdateAuctionFormData)[] => {
      if (activeStep === 0) {
        return ['title', 'description', 'categoryId']
      }
      return []
    }

    const fieldsToValidate = isEditMode ? getEditFields() : getCreateFields()
    const isStepValid = await trigger(
      fieldsToValidate as (keyof (CreateAuctionFormData | UpdateAuctionFormData))[]
    )
    if (isStepValid) {
      setActiveStep((prev) => Math.min(prev + 1, FORM_STEPS.length - 1))
    }
  }

  const handleBack = () => {
    setActiveStep((prev) => Math.max(prev - 1, 0))
  }

  const handleDurationSelect = (days: number) => {
    const endDate = addDays(new Date(), days)
    setValue('auctionEnd' as keyof CreateAuctionFormData, formatDateTimeLocal(endDate))
  }

  if (isLoading) {
    return <AuctionFormSkeleton />
  }

  if (isEditMode && fetchError) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <InlineAlert
          severity="error"
          title="Failed to Load Auction"
        >
          Unable to load auction data. Please try again.
          <Button color="inherit" size="small" onClick={() => globalThis.location.reload()} sx={{ ml: 1 }}>
            Retry
          </Button>
        </InlineAlert>
      </Container>
    )
  }

  const pageTitle = isEditMode ? 'Edit Auction' : 'Create New Auction'
  
  const getSubmitButtonText = () => {
    if (isEditMode) {
      return isSubmitting ? 'Updating...' : 'Update Auction'
    }
    return isSubmitting ? 'Creating...' : 'Create Auction'
  }
  const submitButtonText = getSubmitButtonText()

  // eslint-disable-next-line react-hooks/incompatible-library
  const watchedValues = watch()

  const renderStepContent = (step: number) => {
    switch (step) {
      case 0:
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
                    label="Auction Title"
                    placeholder="Enter a descriptive title for your auction"
                    error={Boolean(errors.title)}
                    helperText={errors.title?.message || 'Min 10 characters, max 200'}
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
                    label="Description"
                    placeholder="Describe your item in detail - condition, history, specifications..."
                    multiline
                    rows={6}
                    error={Boolean(errors.description)}
                    helperText={errors.description?.message || 'Min 50 characters, max 4000'}
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
                    <InputLabel id="category-label">Category</InputLabel>
                    <Select {...field} labelId="category-label" label="Category">
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
                          label="Brand (Optional)"
                          placeholder="Select or search brand"
                        />
                      )}
                    />
                  )}
                />
              </Grid>
            )}
          </Grid>
        )

      case 1:
        return (
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, md: 6 }}>
              <Controller
                name="condition"
                control={control}
                render={({ field }) => (
                  <FormControl fullWidth>
                    <InputLabel id="condition-label">Condition</InputLabel>
                    <Select {...field} labelId="condition-label" label="Condition">
                      {ITEM_CONDITIONS.map((cond) => (
                        <MenuItem key={cond.value} value={cond.value}>
                          <Box>
                            <Typography variant="body1">{cond.label}</Typography>
                            <Typography variant="caption" color="text.secondary">
                              {cond.description}
                            </Typography>
                          </Box>
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
                        label="Year Manufactured (Optional)"
                        placeholder="Select year"
                      />
                    )}
                  />
                )}
              />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <InlineAlert severity="info" title="Item Details" sx={{ mt: 1 }}>
                Providing accurate condition and year helps buyers make informed decisions and can
                increase trust in your listing.
              </InlineAlert>
            </Grid>
            {!isEditMode && (
              <Grid size={{ xs: 12 }}>
                <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1 }}>
                  Photos & Files
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  Add photos of your item. The first image will be used as the cover photo.
                </Typography>
                <FileUploadZone
                  attachments={attachments}
                  uploads={uploads}
                  isUploading={isUploading}
                  onFilesSelected={uploadFiles}
                  onRemove={removeAttachment}
                  onSetPrimary={setPrimaryAttachment}
                  acceptedTypes={ACCEPTED_IMAGE_TYPES}
                />
              </Grid>
            )}
          </Grid>
        )

      case 2:
        if (isEditMode) {
          return (
            <InlineAlert severity="info">
              Pricing and duration cannot be modified after auction creation.
            </InlineAlert>
          )
        }
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
                    label="Starting Price"
                    type="number"
                    slotProps={{
                      input: {
                        startAdornment: <InputAdornment position="start">$</InputAdornment>,
                      },
                    }}
                    onChange={(e) => field.onChange(Number(e.target.value))}
                    error={Boolean((errors as Record<string, { message?: string }>).reservePrice)}
                    helperText={
                      (errors as Record<string, { message?: string }>).reservePrice?.message ||
                      'Minimum bid amount'
                    }
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
                    <InputLabel id="currency-label">Currency</InputLabel>
                    <Select {...field} labelId="currency-label" label="Currency">
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
                  <Switch
                    checked={enableBuyNow}
                    onChange={(e) => setEnableBuyNow(e.target.checked)}
                  />
                }
                label="Enable Buy Now Price"
              />
              {enableBuyNow && (
                <Controller
                  name={'buyNowPrice' as keyof CreateAuctionFormData}
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label="Buy Now Price"
                      type="number"
                      slotProps={{
                        input: {
                          startAdornment: <InputAdornment position="start">$</InputAdornment>,
                        },
                      }}
                      onChange={(e) => field.onChange(Number(e.target.value))}
                      error={Boolean((errors as Record<string, { message?: string }>).buyNowPrice)}
                      helperText={
                        (errors as Record<string, { message?: string }>).buyNowPrice?.message ||
                        'Price for immediate purchase (must be higher than starting price)'
                      }
                      sx={{ mt: 2 }}
                    />
                  )}
                />
              )}
            </Grid>
            <Grid size={{ xs: 12 }}>
              <Divider sx={{ my: 2 }} />
              <Typography variant="subtitle1" gutterBottom>
                Auction Duration
              </Typography>
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 2 }}>
                {AUCTION_DURATIONS.map((duration) => (
                  <Button
                    key={duration.value}
                    variant="outlined"
                    size="small"
                    onClick={() => handleDurationSelect(duration.value)}
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
                    label="Auction End Time"
                    type="datetime-local"
                    slotProps={{
                      inputLabel: { shrink: true },
                    }}
                    error={Boolean((errors as Record<string, { message?: string }>).auctionEnd)}
                    helperText={
                      (errors as Record<string, { message?: string }>).auctionEnd?.message ||
                      'When the auction will end'
                    }
                    required
                  />
                )}
              />
            </Grid>
          </Grid>
        )

      case 3:
        return (
          <Box>
            <InlineAlert severity="info" title="Review Your Auction" sx={{ mb: 3 }}>
              Please review all the details before {isEditMode ? 'updating' : 'creating'} your
              auction.
            </InlineAlert>
            <Grid container spacing={2}>
              <Grid size={{ xs: 12 }}>
                <Typography variant="subtitle2" color="text.secondary">
                  Title
                </Typography>
                <Typography variant="body1" sx={{ fontWeight: 500 }}>
                  {watchedValues.title || '-'}
                </Typography>
              </Grid>
              <Grid size={{ xs: 12 }}>
                <Typography variant="subtitle2" color="text.secondary">
                  Description
                </Typography>
                <Typography variant="body1" sx={{ whiteSpace: 'pre-wrap' }}>
                  {watchedValues.description || '-'}
                </Typography>
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <Typography variant="subtitle2" color="text.secondary">
                  Category
                </Typography>
                <Typography variant="body1">
                  {categories.find((c) => c.id === watchedValues.categoryId)?.name || '-'}
                </Typography>
              </Grid>
              {!isEditMode && 'brandId' in watchedValues && watchedValues.brandId && (
                <Grid size={{ xs: 12, md: 6 }}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Brand
                  </Typography>
                  <Typography variant="body1">
                    {brands.find((b) => b.id === watchedValues.brandId)
                      ?.name || '-'}
                  </Typography>
                </Grid>
              )}
              {watchedValues.condition && (
                <Grid size={{ xs: 12, md: 6 }}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Condition
                  </Typography>
                  <Typography variant="body1">
                    {ITEM_CONDITIONS.find((c) => c.value === watchedValues.condition)?.label || '-'}
                  </Typography>
                </Grid>
              )}
              {watchedValues.yearManufactured && (
                <Grid size={{ xs: 12, md: 6 }}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Year Manufactured
                  </Typography>
                  <Typography variant="body1">{watchedValues.yearManufactured}</Typography>
                </Grid>
              )}
              {!isEditMode && 'reservePrice' in watchedValues && (
                <>
                  {attachments.length > 0 && (
                    <Grid size={{ xs: 12, md: 6 }}>
                      <Typography variant="subtitle2" color="text.secondary">
                        Files Attached
                      </Typography>
                      <Typography variant="body1">
                        {`${attachments.length} file(s)`}
                      </Typography>
                    </Grid>
                  )}
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Typography variant="subtitle2" color="text.secondary">
                      Starting Price
                    </Typography>
                    <Typography variant="body1" sx={{ fontWeight: 600, color: 'primary.main' }}>
                      ${watchedValues.reservePrice}
                    </Typography>
                  </Grid>
                  {enableBuyNow && watchedValues.buyNowPrice && (
                    <Grid size={{ xs: 12, md: 6 }}>
                      <Typography variant="subtitle2" color="text.secondary">
                        Buy Now Price
                      </Typography>
                      <Typography variant="body1" sx={{ fontWeight: 600, color: 'success.main' }}>
                        ${watchedValues.buyNowPrice}
                      </Typography>
                    </Grid>
                  )}
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Typography variant="subtitle2" color="text.secondary">
                      Auction End
                    </Typography>
                    <Typography variant="body1">
                      {new Date(
                        watchedValues.auctionEnd
                      ).toLocaleString()}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Typography variant="subtitle2" color="text.secondary">
                      Currency
                    </Typography>
                    <Typography variant="body1">
                      {CURRENCIES.find(
                        (c) => c.value === watchedValues.currency
                      )?.label || 'USD'}
                    </Typography>
                  </Grid>
                </>
              )}
            </Grid>
          </Box>
        )

      default:
        return null
    }
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <motion.div variants={staggerContainer} initial="initial" animate="animate">
        <motion.div variants={fadeInUp}>
          <Breadcrumbs sx={{ mb: 2 }}>
            <MuiLink component={Link} to="/" color="inherit" underline="hover">
              Home
            </MuiLink>
            <MuiLink component={Link} to="/my-auctions" color="inherit" underline="hover">
              My Auctions
            </MuiLink>
            <Typography color="text.primary">{isEditMode ? 'Edit' : 'Create'}</Typography>
          </Breadcrumbs>

          <Typography
            variant="h4"
            sx={{
              fontFamily: '"Playfair Display", serif',
              fontWeight: 700,
              color: 'primary.main',
              mb: 1,
            }}
          >
            {pageTitle}
          </Typography>
          <Typography color="text.secondary" sx={{ mb: 4 }}>
            {isEditMode
              ? 'Update your auction details below'
              : 'Fill in the details below to list your item for auction'}
          </Typography>
        </motion.div>

        <motion.div variants={fadeInUp}>
          <Card sx={{ p: 4 }}>
            <Stepper activeStep={activeStep} sx={{ mb: 4 }} alternativeLabel>
              {FORM_STEPS.map((label) => (
                <Step key={label}>
                  <StepLabel>{label}</StepLabel>
                </Step>
              ))}
            </Stepper>

            <Divider sx={{ mb: 4 }} />

            <form onSubmit={handleSubmit(onSubmit)}>
              <Box sx={{ minHeight: 300 }}>{renderStepContent(activeStep)}</Box>

              <Box
                sx={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  mt: 4,
                  pt: 2,
                  borderTop: 1,
                  borderColor: 'divider',
                }}
              >
                <Button
                  disabled={activeStep === 0}
                  onClick={handleBack}
                  startIcon={<ArrowBack />}
                  sx={{ visibility: activeStep === 0 ? 'hidden' : 'visible' }}
                >
                  Back
                </Button>
                <Box sx={{ display: 'flex', gap: 2 }}>
                  <Button variant="outlined" onClick={() => navigate('/my-auctions')}>
                    Cancel
                  </Button>
                  {activeStep === FORM_STEPS.length - 1 ? (
                    <Button
                      type="submit"
                      variant="contained"
                      disabled={isSubmitting}
                      startIcon={
                        isSubmitting ? <CircularProgress size={20} color="inherit" /> : <Save />
                      }
                      sx={{
                        bgcolor: palette.brand.primary,
                        '&:hover': { bgcolor: palette.brand.hover },
                      }}
                    >
                      {submitButtonText}
                    </Button>
                  ) : (
                    <Button
                      variant="contained"
                      onClick={handleNext}
                      endIcon={<ArrowForward />}
                      sx={{
                        bgcolor: palette.neutral[900],
                        '&:hover': { bgcolor: palette.neutral[700] },
                      }}
                    >
                      Next
                    </Button>
                  )}
                </Box>
              </Box>
            </form>
          </Card>
        </motion.div>
      </motion.div>
    </Container>
  )
}
