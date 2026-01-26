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
  Alert,
  AlertTitle,
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
import { ArrowBack, ArrowForward, Save } from '@mui/icons-material'
import { motion } from 'framer-motion'
import { fadeInUp, staggerContainer } from '@/shared/lib/animations'
import { createAuctionSchema, updateAuctionSchema, type CreateAuctionFormData, type UpdateAuctionFormData } from '../schemas'
import { useAuction, useCreateAuction, useUpdateAuction, useActiveCategories, useActiveBrands } from '../hooks'
import type { CreateAuctionRequest, UpdateAuctionRequest } from '../types'
import { addDays, formatDateTimeLocal } from '../utils/date.utils'
import { ITEM_CONDITIONS, CURRENCIES, FORM_STEPS, AUCTION_DURATIONS, YEAR_OPTIONS } from '../constants'

const getDefaultCreateValues = (): CreateAuctionFormData => ({
  title: '',
  description: '',
  categoryId: '',
  brandId: '',
  condition: '',
  yearManufactured: undefined,
  reservePrice: 0,
  buyNowPrice: undefined,
  auctionEnd: formatDateTimeLocal(addDays(new Date(), 7)),
  currency: 'USD',
  isFeatured: false,
})

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
  const { data: existingAuction, isLoading: isFetchingAuction, error: fetchError } = useAuction(id ?? '')
  const createMutation = useCreateAuction()
  const updateMutation = useUpdateAuction()

  const isSubmitting = createMutation.isPending || updateMutation.isPending
  const isLoading = isCategoriesLoading || isBrandsLoading || (isEditMode && isFetchingAuction)

  const schema = useMemo(() => isEditMode ? updateAuctionSchema : createAuctionSchema, [isEditMode])

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

      if (auctionData.buyNowPrice) setEnableBuyNow(true)
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
          files: [],
        }
        await createMutation.mutateAsync(createData)
      }
      navigate('/my-auctions')
    } catch {
      /* Error handled by mutation */
    }
  }

  const handleNext = async () => {
    const createFields: (keyof CreateAuctionFormData)[] = activeStep === 0
      ? ['title', 'description', 'categoryId']
      : activeStep === 1
        ? ['condition']
        : activeStep === 2
          ? ['reservePrice', 'auctionEnd']
          : []

    const editFields: (keyof UpdateAuctionFormData)[] = activeStep === 0
      ? ['title', 'description', 'categoryId']
      : []

    const fieldsToValidate = isEditMode ? editFields : createFields
    const isStepValid = await trigger(fieldsToValidate as (keyof (CreateAuctionFormData | UpdateAuctionFormData))[])
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
        <Alert
          severity="error"
          role="alert"
          action={
            <Button color="inherit" size="small" onClick={() => window.location.reload()}>
              Retry
            </Button>
          }
        >
          <AlertTitle>Failed to Load Auction</AlertTitle>
          Unable to load auction data. Please try again.
        </Alert>
      </Container>
    )
  }

  const pageTitle = isEditMode ? 'Edit Auction' : 'Create New Auction'
  const submitButtonText = isEditMode
    ? isSubmitting ? 'Updating...' : 'Update Auction'
    : isSubmitting ? 'Creating...' : 'Create Auction'

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
                    <Select
                      {...field}
                      labelId="category-label"
                      label="Category"
                    >
                      {categories.map((cat) => (
                        <MenuItem key={cat.id} value={cat.id}>
                          {cat.name}
                        </MenuItem>
                      ))}
                    </Select>
                    {errors.categoryId && (
                      <FormHelperText role="alert">
                        {errors.categoryId.message}
                      </FormHelperText>
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
                    <Select
                      {...field}
                      labelId="condition-label"
                      label="Condition"
                    >
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
                    getOptionLabel={(option) => String(option)}
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
              <Alert severity="info" sx={{ mt: 1 }}>
                <AlertTitle>Item Details</AlertTitle>
                Providing accurate condition and year helps buyers make informed decisions and can increase trust in your listing.
              </Alert>
            </Grid>
          </Grid>
        )

      case 2:
        if (isEditMode) {
          return (
            <Alert severity="info">
              Pricing and duration cannot be modified after auction creation.
            </Alert>
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
                    helperText={(errors as Record<string, { message?: string }>).reservePrice?.message || 'Minimum bid amount'}
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
                    <Select
                      {...field}
                      labelId="currency-label"
                      label="Currency"
                    >
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
                      helperText={(errors as Record<string, { message?: string }>).buyNowPrice?.message || 'Price for immediate purchase (must be higher than starting price)'}
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
                    helperText={(errors as Record<string, { message?: string }>).auctionEnd?.message || 'When the auction will end'}
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
            <Alert severity="info" sx={{ mb: 3 }}>
              <AlertTitle>Review Your Auction</AlertTitle>
              Please review all the details before {isEditMode ? 'updating' : 'creating'} your auction.
            </Alert>
            <Grid container spacing={2}>
              <Grid size={{ xs: 12 }}>
                <Typography variant="subtitle2" color="text.secondary">Title</Typography>
                <Typography variant="body1" sx={{ fontWeight: 500 }}>{watchedValues.title || '-'}</Typography>
              </Grid>
              <Grid size={{ xs: 12 }}>
                <Typography variant="subtitle2" color="text.secondary">Description</Typography>
                <Typography variant="body1" sx={{ whiteSpace: 'pre-wrap' }}>
                  {watchedValues.description || '-'}
                </Typography>
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <Typography variant="subtitle2" color="text.secondary">Category</Typography>
                <Typography variant="body1">
                  {categories.find(c => c.id === watchedValues.categoryId)?.name || '-'}
                </Typography>
              </Grid>
              {!isEditMode && 'brandId' in watchedValues && watchedValues.brandId && (
                <Grid size={{ xs: 12, md: 6 }}>
                  <Typography variant="subtitle2" color="text.secondary">Brand</Typography>
                  <Typography variant="body1">
                    {brands.find(b => b.id === (watchedValues as CreateAuctionFormData).brandId)?.name || '-'}
                  </Typography>
                </Grid>
              )}
              {watchedValues.condition && (
                <Grid size={{ xs: 12, md: 6 }}>
                  <Typography variant="subtitle2" color="text.secondary">Condition</Typography>
                  <Typography variant="body1">
                    {ITEM_CONDITIONS.find(c => c.value === watchedValues.condition)?.label || '-'}
                  </Typography>
                </Grid>
              )}
              {watchedValues.yearManufactured && (
                <Grid size={{ xs: 12, md: 6 }}>
                  <Typography variant="subtitle2" color="text.secondary">Year Manufactured</Typography>
                  <Typography variant="body1">{watchedValues.yearManufactured}</Typography>
                </Grid>
              )}
              {!isEditMode && 'reservePrice' in watchedValues && (
                <>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Typography variant="subtitle2" color="text.secondary">Starting Price</Typography>
                    <Typography variant="body1" sx={{ fontWeight: 600, color: 'primary.main' }}>
                      ${(watchedValues as CreateAuctionFormData).reservePrice}
                    </Typography>
                  </Grid>
                  {enableBuyNow && (watchedValues as CreateAuctionFormData).buyNowPrice && (
                    <Grid size={{ xs: 12, md: 6 }}>
                      <Typography variant="subtitle2" color="text.secondary">Buy Now Price</Typography>
                      <Typography variant="body1" sx={{ fontWeight: 600, color: 'success.main' }}>
                        ${(watchedValues as CreateAuctionFormData).buyNowPrice}
                      </Typography>
                    </Grid>
                  )}
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Typography variant="subtitle2" color="text.secondary">Auction End</Typography>
                    <Typography variant="body1">
                      {new Date((watchedValues as CreateAuctionFormData).auctionEnd).toLocaleString()}
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Typography variant="subtitle2" color="text.secondary">Currency</Typography>
                    <Typography variant="body1">
                      {CURRENCIES.find(c => c.value === (watchedValues as CreateAuctionFormData).currency)?.label || 'USD'}
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
            <Typography color="text.primary">
              {isEditMode ? 'Edit' : 'Create'}
            </Typography>
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
              <Box sx={{ minHeight: 300 }}>
                {renderStepContent(activeStep)}
              </Box>

              <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 4, pt: 2, borderTop: 1, borderColor: 'divider' }}>
                <Button
                  disabled={activeStep === 0}
                  onClick={handleBack}
                  startIcon={<ArrowBack />}
                  sx={{ visibility: activeStep === 0 ? 'hidden' : 'visible' }}
                >
                  Back
                </Button>
                <Box sx={{ display: 'flex', gap: 2 }}>
                  <Button
                    variant="outlined"
                    onClick={() => navigate('/my-auctions')}
                  >
                    Cancel
                  </Button>
                  {activeStep === FORM_STEPS.length - 1 ? (
                    <Button
                      type="submit"
                      variant="contained"
                      disabled={isSubmitting}
                      startIcon={isSubmitting ? <CircularProgress size={20} color="inherit" /> : <Save />}
                      sx={{
                        bgcolor: '#CA8A04',
                        '&:hover': { bgcolor: '#A16207' },
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
                        bgcolor: '#1C1917',
                        '&:hover': { bgcolor: '#44403C' },
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
