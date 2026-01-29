import { useState, useEffect } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import { useQuery, useMutation } from '@tanstack/react-query'
import {
  Container,
  Typography,
  Box,
  Card,
  Grid,
  Button,
  TextField,
  Divider,
  CircularProgress,
  Skeleton,
  Stepper,
  Step,
  StepLabel,
  Stack,
  Chip,
} from '@mui/material'
import {
  ArrowBack,
  LocalShipping,
  Payment,
  CheckCircle,
  ShoppingCart,
  Lock,
} from '@mui/icons-material'
import { palette } from '@/shared/theme/tokens'
import { useForm, Controller } from 'react-hook-form'
import { InlineAlert } from '@/shared/ui'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { ordersApi } from '../api'
import type { ShippingAddress } from '../types'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'

const shippingSchema = z.object({
  fullName: z.string().min(2, 'Full name is required'),
  addressLine1: z.string().min(5, 'Address is required'),
  addressLine2: z.string().optional(),
  city: z.string().min(2, 'City is required'),
  state: z.string().min(2, 'State is required'),
  postalCode: z.string().min(3, 'Postal code is required'),
  country: z.string().min(2, 'Country is required'),
  phone: z.string().optional(),
})

type ShippingFormData = z.infer<typeof shippingSchema>

const steps = ['Shipping', 'Payment', 'Confirmation']

export function CheckoutPage() {
  const { auctionId } = useParams<{ auctionId: string }>()
  const navigate = useNavigate()
  const [activeStep, setActiveStep] = useState(0)
  const [orderId, setOrderId] = useState<string | null>(null)

  const { data: existingOrder, isLoading: checkingOrder } = useQuery({
    queryKey: ['order', 'auction', auctionId],
    queryFn: () => ordersApi.getOrderByAuctionId(auctionId!),
    enabled: !!auctionId,
    retry: false,
  })

  const createOrderMutation = useMutation({
    mutationFn: (shippingAddress: ShippingAddress) =>
      ordersApi.createOrder({ auctionId: auctionId!, shippingAddress }),
    onSuccess: (order) => {
      setOrderId(order.id)
      setActiveStep(1)
    },
  })

  const processPaymentMutation = useMutation({
    mutationFn: (id: string) => ordersApi.processPayment(id),
    onSuccess: () => {
      setActiveStep(2)
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
      country: 'United States',
      phone: '',
    },
  })

  useEffect(() => {
    if (existingOrder && existingOrder.status !== 'pending') {
      navigate(`/orders/${existingOrder.id}`)
    }
  }, [existingOrder, navigate])

  const onSubmitShipping = (data: ShippingFormData) => {
    createOrderMutation.mutate(data as ShippingAddress)
  }

  const handlePayment = () => {
    if (orderId) {
      processPaymentMutation.mutate(orderId)
    }
  }

  if (checkingOrder) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      </Container>
    )
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <motion.div variants={staggerContainer} initial="initial" animate="animate">
        <motion.div variants={fadeInUp}>
          <Button
            startIcon={<ArrowBack />}
            component={Link}
            to={`/auctions/${auctionId}`}
            sx={{ mb: 3, color: 'text.secondary' }}
          >
            Back to Auction
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
            Checkout
          </Typography>
        </motion.div>

        <motion.div variants={staggerItem}>
          <Card sx={{ p: 3, mb: 4 }}>
            <Stepper activeStep={activeStep} alternativeLabel>
              {steps.map((label, index) => (
                <Step key={label}>
                  <StepLabel
                    StepIconComponent={() => (
                      <Box
                        sx={{
                          width: 40,
                          height: 40,
                          borderRadius: '50%',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          bgcolor: index <= activeStep ? 'primary.main' : 'grey.300',
                          color: 'white',
                        }}
                      >
                        {index === 0 && <LocalShipping />}
                        {index === 1 && <Payment />}
                        {index === 2 && <CheckCircle />}
                      </Box>
                    )}
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
                    Shipping Address
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
                              label="Full Name"
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
                              label="Address Line 1"
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
                            <TextField {...field} label="Address Line 2 (Optional)" fullWidth />
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
                              label="City"
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
                              label="State / Province"
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
                              label="Postal Code"
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
                              label="Country"
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
                              label="Phone (Optional)"
                              fullWidth
                              helperText="For delivery notifications"
                            />
                          )}
                        />
                      </Grid>
                    </Grid>

                    {createOrderMutation.error && (
                      <InlineAlert severity="error" sx={{ mt: 3 }}>
                        Failed to create order. Please try again.
                      </InlineAlert>
                    )}

                    <Box sx={{ mt: 4, display: 'flex', justifyContent: 'flex-end' }}>
                      <Button
                        type="submit"
                        variant="contained"
                        size="large"
                        disabled={createOrderMutation.isPending}
                        sx={{
                          px: 4,
                          bgcolor: 'primary.main',
                          '&:hover': { bgcolor: 'primary.dark' },
                        }}
                      >
                        {createOrderMutation.isPending ? (
                          <CircularProgress size={24} />
                        ) : (
                          'Continue to Payment'
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
                    Payment
                  </Typography>
                  <Divider sx={{ mb: 3 }} />

                  <InlineAlert severity="info" sx={{ mb: 3 }}>
                    You will be redirected to our secure payment provider to complete your purchase.
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
                      Secure Payment
                    </Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                      Your payment information is encrypted and secure
                    </Typography>

                    {processPaymentMutation.error && (
                      <InlineAlert severity="error" sx={{ mb: 3 }}>
                        Payment failed. Please try again.
                      </InlineAlert>
                    )}

                    <Button
                      variant="contained"
                      size="large"
                      onClick={handlePayment}
                      disabled={processPaymentMutation.isPending}
                      sx={{
                        px: 6,
                        py: 1.5,
                        bgcolor: palette.brand.primary,
                        '&:hover': { bgcolor: '#A16207' },
                      }}
                    >
                      {processPaymentMutation.isPending ? (
                        <CircularProgress size={24} />
                      ) : (
                        'Pay Now'
                      )}
                    </Button>
                  </Box>
                </Card>
              </motion.div>
            )}

            {activeStep === 2 && (
              <motion.div
                variants={staggerItem}
                initial={{ opacity: 0, scale: 0.9 }}
                animate={{ opacity: 1, scale: 1 }}
              >
                <Card sx={{ p: 6, textAlign: 'center' }}>
                  <motion.div
                    initial={{ scale: 0 }}
                    animate={{ scale: 1 }}
                    transition={{ type: 'spring', stiffness: 200, damping: 15 }}
                  >
                    <Box
                      sx={{
                        width: 80,
                        height: 80,
                        borderRadius: '50%',
                        bgcolor: 'success.light',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        mx: 'auto',
                        mb: 3,
                      }}
                    >
                      <CheckCircle sx={{ fontSize: 48, color: 'success.main' }} />
                    </Box>
                  </motion.div>

                  <Typography
                    variant="h4"
                    sx={{
                      fontFamily: '"Playfair Display", serif',
                      fontWeight: 700,
                      color: 'primary.main',
                      mb: 1,
                    }}
                  >
                    Order Confirmed!
                  </Typography>
                  <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
                    Thank you for your purchase. You will receive a confirmation email shortly.
                  </Typography>

                  <Stack direction="row" spacing={2} justifyContent="center">
                    <Button variant="outlined" component={Link} to="/orders">
                      View Orders
                    </Button>
                    <Button variant="contained" component={Link} to="/auctions">
                      Continue Shopping
                    </Button>
                  </Stack>
                </Card>
              </motion.div>
            )}
          </Grid>

          <Grid size={{ xs: 12, md: 4 }}>
            <motion.div variants={staggerItem}>
              <Card sx={{ p: 3, position: 'sticky', top: 100 }}>
                <Typography variant="h6" fontWeight={600} gutterBottom>
                  Order Summary
                </Typography>
                <Divider sx={{ mb: 2 }} />

                {checkingOrder && (
                  <Stack spacing={2}>
                    <Skeleton variant="rectangular" height={80} />
                    <Skeleton width="60%" />
                    <Skeleton width="40%" />
                  </Stack>
                )}
                {!checkingOrder && existingOrder && (
                  <>
                    <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
                      <Box
                        component="img"
                        src={existingOrder.auctionImageUrl || '/placeholder.jpg'}
                        alt={existingOrder.auctionTitle}
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
                          {existingOrder.auctionTitle}
                        </Typography>
                        <Chip label="Won" color="success" size="small" sx={{ mt: 1 }} />
                      </Box>
                    </Box>

                    <Stack spacing={1.5}>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant="body2" color="text.secondary">
                          Winning Bid
                        </Typography>
                        <Typography variant="body2">
                          $
                          {(
                            existingOrder.winningBidAmount ||
                            existingOrder.winningBid ||
                            0
                          ).toLocaleString()}
                        </Typography>
                      </Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant="body2" color="text.secondary">
                          Platform Fee
                        </Typography>
                        <Typography variant="body2">
                          ${(existingOrder.platformFee || 0).toLocaleString()}
                        </Typography>
                      </Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant="body2" color="text.secondary">
                          Shipping
                        </Typography>
                        <Typography variant="body2">
                          ${(existingOrder.shippingCost || 0).toLocaleString()}
                        </Typography>
                      </Box>
                      <Divider />
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant="subtitle1" fontWeight={600}>
                          Total
                        </Typography>
                        <Typography variant="subtitle1" fontWeight={700} color="primary.main">
                          ${existingOrder.totalAmount.toLocaleString()}
                        </Typography>
                      </Box>
                    </Stack>
                  </>
                )}
                {!existingOrderLoading && !existingOrder && (
                  <Box sx={{ py: 4, textAlign: 'center' }}>
                    <ShoppingCart sx={{ fontSize: 48, color: 'grey.300', mb: 2 }} />
                    <Typography variant="body2" color="text.secondary">
                      Order details will appear here
                    </Typography>
                  </Box>
                )}
              </Card>
            </motion.div>
          </Grid>
        </Grid>
      </motion.div>
    </Container>
  )
}
