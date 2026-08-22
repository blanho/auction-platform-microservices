import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import { palette } from '@/shared/theme/tokens'
import { InlineAlert } from '@/shared/ui'
import { getSafeHttpUrl } from '@/shared/utils'
import { formatCurrency } from '@/shared/utils/formatters'
import { zodResolver } from '@hookform/resolvers/zod'
import { ArrowBack, LocalShipping, Lock, Payment } from '@mui/icons-material'
import {
  Box,
  Button,
  Card,
  Chip,
  CircularProgress,
  Container,
  Divider,
  Grid,
  Stack,
  Step,
  StepLabel,
  Stepper,
  TextField,
  Typography,
} from '@mui/material'
import type { StepIconProps } from '@mui/material/StepIcon'
import { useMutation, useQuery } from '@tanstack/react-query'
import { motion } from 'framer-motion'
import type { TFunction } from 'i18next'
import { useEffect, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { z } from 'zod'
import { ordersApi, paymentsApi } from '../api'
import type { ShippingAddress } from '../types'
function CheckoutStepIcon(props: Readonly<StepIconProps>) {
  const stepIndex = Number(props.icon) - 1
  const isActive = props.active || props.completed

  return (
    <Box
      sx={{
        width: 40,
        height: 40,
        borderRadius: '50%',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        bgcolor: isActive ? 'primary.main' : 'grey.300',
        color: 'white',
      }}
    >
      {stepIndex === 0 && <LocalShipping />}
      {stepIndex === 1 && <Payment />}
    </Box>
  )
}

const createShippingSchema = (t: TFunction) =>
  z.object({
    fullName: z.string().min(2, t('checkout.validation.fullName')),
    addressLine1: z.string().min(5, t('checkout.validation.address')),
    addressLine2: z.string().optional(),
    city: z.string().min(2, t('checkout.validation.city')),
    state: z.string().min(2, t('checkout.validation.state')),
    postalCode: z.string().min(3, t('checkout.validation.postalCode')),
    country: z.string().min(2, t('checkout.validation.country')),
    phone: z.string().optional(),
  })

type ShippingFormData = z.infer<ReturnType<typeof createShippingSchema>>

export function CheckoutPage() {
  const { t } = useTranslation('payments')
  const { auctionId } = useParams<{ auctionId: string }>()
  const navigate = useNavigate()
  const [activeStep, setActiveStep] = useState(0)
  const [orderId, setOrderId] = useState<string | null>(null)
  const shippingSchema = createShippingSchema(t)
  const steps = [t('checkout.steps.shipping'), t('checkout.steps.payment')]

  const { data: existingOrder, isLoading: checkingOrder } = useQuery({
    queryKey: ['order', 'auction', auctionId],
    queryFn: () => {
      if (!auctionId) {
        throw new Error(t('checkout.auctionIdRequired'))
      }
      return ordersApi.getOrderByAuctionId(auctionId)
    },
    enabled: !!auctionId,
    retry: 3,
    retryDelay: 1000,
  })

  const prepareCheckoutMutation = useMutation({
    mutationFn: (shippingAddress: ShippingAddress) => {
      if (!existingOrder) {
        throw new Error(t('checkout.orderUnavailable'))
      }
      return ordersApi.prepareCheckout(existingOrder.id, { shippingAddress })
    },
    onSuccess: (order) => {
      setOrderId(order.id)
      setActiveStep(1)
    },
  })

  const checkoutSessionMutation = useMutation({
    mutationFn: async (id: string) => {
      const session = await paymentsApi.createOrderCheckoutSession(id)
      const checkoutUrl = getSafeHttpUrl(session.url)
      if (!checkoutUrl) {
        throw new Error(t('checkout.invalidPaymentUrl'))
      }
      window.location.assign(checkoutUrl)
    },
  })

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<ShippingFormData>({
    resolver: zodResolver(shippingSchema),
    defaultValues: {
      fullName: '',
      addressLine1: '',
      addressLine2: '',
      city: '',
      state: '',
      postalCode: '',
      country: t('checkout.defaultCountry'),
      phone: '',
    },
  })

  useEffect(() => {
    if (
      existingOrder &&
      existingOrder.status !== 'pending' &&
      existingOrder.status !== 'payment_pending'
    ) {
      navigate(`/orders/${existingOrder.id}`)
    }
  }, [existingOrder, navigate])

  const onSubmitShipping = (data: ShippingFormData) => {
    prepareCheckoutMutation.mutate(data)
  }

  const handlePayment = () => {
    if (orderId) {
      checkoutSessionMutation.mutate(orderId)
    }
  }

  if (checkingOrder) {
    return (
      <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      </Container>
    )
  }

  if (!existingOrder) {
    return (
      <Container maxWidth="sm" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
        <InlineAlert severity="error" sx={{ mb: 3 }}>
          {t('checkout.orderUnavailable')}
        </InlineAlert>
        <Button startIcon={<ArrowBack />} component={Link} to={`/auctions/${auctionId}`}>
          {t('checkout.backToAuction')}
        </Button>
      </Container>
    )
  }

  return (
    <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
      <motion.div variants={staggerContainer} initial="initial" animate="animate">
        <motion.div variants={fadeInUp}>
          <Button
            startIcon={<ArrowBack />}
            component={Link}
            to={`/auctions/${auctionId}`}
            sx={{ mb: 3, color: 'text.secondary' }}
          >
            {t('checkout.backToAuction')}
          </Button>
        </motion.div>

        <motion.div variants={staggerItem}>
          <Typography
            variant="h4"
            sx={{
              fontFamily: '"Playfair Display", serif',
              fontWeight: 700,
              color: 'primary.main',
              mb: 4,
            }}
          >
            {t('checkout.title')}
          </Typography>
        </motion.div>

        <motion.div variants={staggerItem}>
          <Card sx={{ p: 3, mb: 4 }}>
            <Stepper activeStep={activeStep} alternativeLabel>
              {steps.map((label, _index) => (
                <Step key={label}>
                  <StepLabel
                    slots={{
                      stepIcon: CheckoutStepIcon,
                    }}
                  >
                    {label}
                  </StepLabel>
                </Step>
              ))}
            </Stepper>
          </Card>
        </motion.div>

        <Grid container spacing={4}>
          <Grid size={{ xs: 12, md: 8 }}>
            {activeStep === 0 && (
              <motion.div variants={staggerItem}>
                <Card sx={{ p: 4 }}>
                  <Typography variant="h6" fontWeight={600} gutterBottom>
                    {t('checkout.shippingAddress')}
                  </Typography>
                  <Divider sx={{ mb: 3 }} />

                  <form onSubmit={handleSubmit(onSubmitShipping)}>
                    <Grid container spacing={3}>
                      <Grid size={{ xs: 12 }}>
                        <Controller
                          name="fullName"
                          control={control}
                          render={({ field }) => (
                            <TextField
                              {...field}
                              label={t('checkout.fullName')}
                              fullWidth
                              error={!!errors.fullName}
                              helperText={errors.fullName?.message}
                            />
                          )}
                        />
                      </Grid>

                      <Grid size={{ xs: 12 }}>
                        <Controller
                          name="addressLine1"
                          control={control}
                          render={({ field }) => (
                            <TextField
                              {...field}
                              label={t('checkout.addressLine1')}
                              fullWidth
                              error={!!errors.addressLine1}
                              helperText={errors.addressLine1?.message}
                            />
                          )}
                        />
                      </Grid>

                      <Grid size={{ xs: 12 }}>
                        <Controller
                          name="addressLine2"
                          control={control}
                          render={({ field }) => (
                            <TextField {...field} label={t('checkout.addressLine2')} fullWidth />
                          )}
                        />
                      </Grid>

                      <Grid size={{ xs: 12, sm: 6 }}>
                        <Controller
                          name="city"
                          control={control}
                          render={({ field }) => (
                            <TextField
                              {...field}
                              label={t('checkout.city')}
                              fullWidth
                              error={!!errors.city}
                              helperText={errors.city?.message}
                            />
                          )}
                        />
                      </Grid>

                      <Grid size={{ xs: 12, sm: 6 }}>
                        <Controller
                          name="state"
                          control={control}
                          render={({ field }) => (
                            <TextField
                              {...field}
                              label={t('checkout.state')}
                              fullWidth
                              error={!!errors.state}
                              helperText={errors.state?.message}
                            />
                          )}
                        />
                      </Grid>

                      <Grid size={{ xs: 12, sm: 6 }}>
                        <Controller
                          name="postalCode"
                          control={control}
                          render={({ field }) => (
                            <TextField
                              {...field}
                              label={t('checkout.postalCode')}
                              fullWidth
                              error={!!errors.postalCode}
                              helperText={errors.postalCode?.message}
                            />
                          )}
                        />
                      </Grid>

                      <Grid size={{ xs: 12, sm: 6 }}>
                        <Controller
                          name="country"
                          control={control}
                          render={({ field }) => (
                            <TextField
                              {...field}
                              label={t('checkout.country')}
                              fullWidth
                              error={!!errors.country}
                              helperText={errors.country?.message}
                            />
                          )}
                        />
                      </Grid>

                      <Grid size={{ xs: 12 }}>
                        <Controller
                          name="phone"
                          control={control}
                          render={({ field }) => (
                            <TextField
                              {...field}
                              label={t('checkout.phone')}
                              fullWidth
                              helperText={t('checkout.phoneHelper')}
                            />
                          )}
                        />
                      </Grid>
                    </Grid>

                    {prepareCheckoutMutation.error && (
                      <InlineAlert severity="error" sx={{ mt: 3 }}>
                        {t('checkout.createFailed')}
                      </InlineAlert>
                    )}

                    <Box sx={{ mt: 4, display: 'flex', justifyContent: 'flex-end' }}>
                      <Button
                        type="submit"
                        variant="contained"
                        size="large"
                        disabled={prepareCheckoutMutation.isPending}
                        sx={{
                          px: 4,
                          bgcolor: 'primary.main',
                          '&:hover': { bgcolor: 'primary.dark' },
                        }}
                      >
                        {prepareCheckoutMutation.isPending ? (
                          <CircularProgress size={24} />
                        ) : (
                          t('checkout.continueToPayment')
                        )}
                      </Button>
                    </Box>
                  </form>
                </Card>
              </motion.div>
            )}

            {activeStep === 1 && (
              <motion.div variants={staggerItem}>
                <Card sx={{ p: 4 }}>
                  <Typography variant="h6" fontWeight={600} gutterBottom>
                    {t('checkout.steps.payment')}
                  </Typography>
                  <Divider sx={{ mb: 3 }} />

                  <InlineAlert severity="info" sx={{ mb: 3 }}>
                    {t('checkout.paymentRedirect')}
                  </InlineAlert>

                  <Box
                    sx={{
                      p: 4,
                      bgcolor: 'grey.50',
                      borderRadius: 2,
                      textAlign: 'center',
                    }}
                  >
                    <Lock sx={{ fontSize: 48, color: 'primary.main', mb: 2 }} />
                    <Typography variant="h6" gutterBottom>
                      {t('checkout.securePayment')}
                    </Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                      {t('checkout.secureDescription')}
                    </Typography>

                    {checkoutSessionMutation.error && (
                      <InlineAlert severity="error" sx={{ mb: 3 }}>
                        {t('messages.paymentFailed')}
                      </InlineAlert>
                    )}

                    <Button
                      variant="contained"
                      size="large"
                      onClick={handlePayment}
                      disabled={checkoutSessionMutation.isPending}
                      sx={{
                        px: 6,
                        py: 1.5,
                        bgcolor: palette.brand.primary,
                        '&:hover': { bgcolor: '#A16207' },
                      }}
                    >
                      {checkoutSessionMutation.isPending ? (
                        <CircularProgress size={24} />
                      ) : (
                        t('checkout.payNow')
                      )}
                    </Button>
                  </Box>
                </Card>
              </motion.div>
            )}
          </Grid>

          <Grid size={{ xs: 12, md: 4 }}>
            <motion.div variants={staggerItem}>
              <Card sx={{ p: 3, position: 'sticky', top: 100 }}>
                <Typography variant="h6" fontWeight={600} gutterBottom>
                  {t('checkout.orderSummary')}
                </Typography>
                <Divider sx={{ mb: 2 }} />

                <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
                  <Box
                    component="img"
                    src={existingOrder.auctionImageUrl || '/placeholder.jpg'}
                    alt={existingOrder.auctionTitle || existingOrder.itemTitle}
                    sx={{
                      width: 80,
                      height: 80,
                      objectFit: 'cover',
                      borderRadius: 1,
                      bgcolor: 'grey.100',
                    }}
                  />
                  <Box sx={{ flex: 1 }}>
                    <Typography variant="subtitle2" fontWeight={600} noWrap>
                      {existingOrder.auctionTitle || existingOrder.itemTitle}
                    </Typography>
                    <Chip label={t('checkout.won')} color="success" size="small" sx={{ mt: 1 }} />
                  </Box>
                </Box>

                <Stack spacing={1.5}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="body2" color="text.secondary">
                      {t('checkout.winningBid')}
                    </Typography>
                    <Typography variant="body2">
                      {formatCurrency(
                        existingOrder.winningBidAmount || existingOrder.winningBid || 0
                      )}
                    </Typography>
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="body2" color="text.secondary">
                      {t('checkout.platformFee')}
                    </Typography>
                    <Typography variant="body2">
                      {formatCurrency(existingOrder.platformFee || 0)}
                    </Typography>
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="body2" color="text.secondary">
                      {t('checkout.shipping')}
                    </Typography>
                    <Typography variant="body2">
                      {formatCurrency(existingOrder.shippingCost || 0)}
                    </Typography>
                  </Box>
                  <Divider />
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="subtitle1" fontWeight={600}>
                      {t('checkout.total')}
                    </Typography>
                    <Typography variant="subtitle1" fontWeight={700} color="primary.main">
                      {formatCurrency(existingOrder.totalAmount)}
                    </Typography>
                  </Box>
                </Stack>
              </Card>
            </motion.div>
          </Grid>
        </Grid>
      </motion.div>
    </Container>
  )
}
