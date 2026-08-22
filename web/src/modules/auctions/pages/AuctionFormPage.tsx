import { ACCEPTED_IMAGE_TYPES } from '@/shared/constants/storage.constants'
import { useFileUpload } from '@/shared/hooks/useFileUpload'
import { fadeInUp, staggerContainer } from '@/shared/lib/animations'
import { palette } from '@/shared/theme/tokens'
import { ArrowBack, ArrowForward, Save } from '@mui/icons-material'
import {
  Box,
  Breadcrumbs,
  Button,
  Card,
  CircularProgress,
  Container,
  Grid,
  Link as MuiLink,
  Skeleton,
  Step,
  StepLabel,
  Stepper,
  Typography,
} from '@mui/material'
import { motion } from 'framer-motion'
import { useCallback } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { FORM_STEPS } from '../constants'
import { BasicInfoStep, ItemDetailsStep, PricingStep, ReviewStep } from '../forms'
import { useActiveBrands, useActiveCategories } from '../hooks'
import { useAuctionForm } from '../hooks/useAuctionForm'
import { useMultiStepForm } from '../hooks/useMultiStepForm'
import type { CreateAuctionFormData } from '../schemas'
import { addDays, formatDateTimeLocal } from '../utils/date.utils'

const STEP_FIELDS: Record<number, (keyof CreateAuctionFormData)[]> = {
  0: ['title', 'description', 'categoryId'],
  1: ['condition', 'yearManufactured'],
  2: ['reservePrice', 'auctionEnd'],
  3: [],
}

function AuctionFormSkeleton() {
  return (
    <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
      <Skeleton variant="text" width={300} height={40} sx={{ mb: 2 }} />
      <Skeleton variant="text" width={200} height={24} sx={{ mb: 4 }} />
      <Card sx={{ p: 4 }}>
        <Skeleton variant="rectangular" height={60} sx={{ mb: 4 }} />
        <Grid container spacing={3}>
          {[1, 2, 3, 4].map((i) => (
            <Grid key={i} size={{ xs: 12, md: i > 2 ? 6 : 12 }}>
              <Skeleton variant="rectangular" height={i === 2 ? 120 : 56} />
            </Grid>
          ))}
        </Grid>
      </Card>
    </Container>
  )
}

export function AuctionFormPage() {
  const { t } = useTranslation('auctions')
  const { id } = useParams<{ id: string }>()

  const { data: categories = [] } = useActiveCategories()
  const { data: brands = [] } = useActiveBrands()

  const {
    form,
    isEditMode,
    isFetchingAuctionData,
    fetchError,
    isSubmitting,
    enableBuyNow,
    setEnableBuyNow,
    handleSubmit,
  } = useAuctionForm(id)

  const { uploads, attachments, uploadFiles, removeAttachment, setPrimaryAttachment, isUploading } =
    useFileUpload({
      subFolder: 'auctions',
      acceptedTypes: ACCEPTED_IMAGE_TYPES,
    })

  const { activeStep, isFirstStep, isLastStep, goToNext, goToPrev } = useMultiStepForm({
    totalSteps: FORM_STEPS.length,
    trigger: form.trigger,
    stepFields: STEP_FIELDS,
  })

  const handleDurationPreset = useCallback(
    (days: number) => {
      form.setValue(
        'auctionEnd' as keyof CreateAuctionFormData,
        formatDateTimeLocal(addDays(new Date(), days)) as never
      )
    },
    [form]
  )

  const onSubmit = form.handleSubmit(async (data) => {
    await handleSubmit(data, attachments)
  })

  if (isFetchingAuctionData) {
    return <AuctionFormSkeleton />
  }

  if (isEditMode && fetchError) {
    return (
      <Container maxWidth="lg" sx={{ py: 6 }}>
        <Typography color="error">{t('form.fetchError')}</Typography>
      </Container>
    )
  }

  const formValues = form.watch()
  const errors = form.formState.errors

  const steps = [
    <BasicInfoStep
      key="basic"
      control={form.control}
      errors={errors}
      isEditMode={isEditMode}
      categories={categories}
      brands={brands}
    />,
    <ItemDetailsStep
      key="details"
      control={form.control}
      isEditMode={isEditMode}
      attachments={attachments}
      uploads={uploads}
      isUploading={isUploading}
      onFilesSelected={uploadFiles}
      onRemoveAttachment={removeAttachment}
      onSetPrimaryAttachment={setPrimaryAttachment}
    />,
    <PricingStep
      key="pricing"
      control={form.control}
      errors={errors}
      isEditMode={isEditMode}
      enableBuyNow={enableBuyNow}
      onToggleBuyNow={setEnableBuyNow}
      onSetDuration={handleDurationPreset}
    />,
    <ReviewStep
      key="review"
      formValues={formValues}
      isEditMode={isEditMode}
      categories={categories}
      brands={brands}
      attachments={attachments}
      enableBuyNow={enableBuyNow}
    />,
  ]

  let submitButtonText = t('form.publishAuction')
  if (isSubmitting) {
    submitButtonText = t('form.saving')
  } else if (isEditMode) {
    submitButtonText = t('form.saveChanges')
  }

  return (
    <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 } }}>
      <Breadcrumbs sx={{ mb: 2 }}>
        <MuiLink component={Link} to="/" underline="hover" color="inherit">
          {t('common:nav.home')}
        </MuiLink>
        <MuiLink component={Link} to="/my-auctions" underline="hover" color="inherit">
          {t('myAuctions')}
        </MuiLink>
        <Typography color="text.primary">
          {isEditMode ? t('form.editAuction') : t('form.createAuction')}
        </Typography>
      </Breadcrumbs>

      <Box sx={{ mb: 4 }}>
        <Typography
          variant="h4"
          component="h1"
          sx={{
            fontFamily: '"Playfair Display", serif',
            fontWeight: 700,
            color: palette.neutral[900],
            mb: 1,
          }}
        >
          {isEditMode ? t('form.editAuction') : t('form.createAuction')}
        </Typography>
        <Typography variant="body1" color="text.secondary">
          {isEditMode ? t('form.editDescription') : t('form.createDescription')}
        </Typography>
      </Box>

      <Card sx={{ p: { xs: 3, md: 4 } }}>
        <Stepper activeStep={activeStep} sx={{ mb: 4 }}>
          {FORM_STEPS.map((label) => (
            <Step key={label}>
              <StepLabel>
                {t(`formSteps.${label.toLowerCase().replace(/ & /g, '').replace(/ /g, '')}`, {
                  defaultValue: label,
                })}
              </StepLabel>
            </Step>
          ))}
        </Stepper>

        <Box
          component={motion.div}
          key={activeStep}
          variants={staggerContainer}
          initial="initial"
          animate="animate"
        >
          <Box component={motion.div} variants={fadeInUp}>
            <form onSubmit={onSubmit}>
              {steps[activeStep]}

              <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 4 }}>
                <Button
                  startIcon={<ArrowBack />}
                  onClick={goToPrev}
                  disabled={isFirstStep || isSubmitting}
                  sx={{ textTransform: 'none' }}
                >
                  {t('common:actions.back')}
                </Button>

                {isLastStep ? (
                  <Button
                    type="submit"
                    variant="contained"
                    startIcon={
                      isSubmitting ? <CircularProgress size={18} color="inherit" /> : <Save />
                    }
                    disabled={isSubmitting}
                    sx={{
                      textTransform: 'none',
                      fontWeight: 600,
                      px: 4,
                      bgcolor: palette.brand?.primary,
                      '&:hover': { bgcolor: '#A16207' },
                    }}
                  >
                    {submitButtonText}
                  </Button>
                ) : (
                  <Button
                    type="button"
                    variant="contained"
                    endIcon={<ArrowForward />}
                    onClick={goToNext}
                    sx={{ textTransform: 'none', fontWeight: 600, px: 4 }}
                  >
                    {t('common:actions.next')}
                  </Button>
                )}
              </Box>
            </form>
          </Box>
        </Box>
      </Card>
    </Container>
  )
}
