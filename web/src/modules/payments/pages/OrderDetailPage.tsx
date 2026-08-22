import { useAuth } from '@/app/hooks/useAuth'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import { palette } from '@/shared/theme/tokens'
import { InlineAlert } from '@/shared/ui'
import { getSafeHttpUrl } from '@/shared/utils'
import { formatCurrency, formatDate, formatDateTime } from '@/shared/utils/formatters'
import {
  ArrowBack,
  CheckCircle,
  ContentCopy,
  Inventory,
  LocalShipping,
  LocationOn,
  OpenInNew,
  Payment,
  Pending,
  Person,
  ShoppingBag,
} from '@mui/icons-material'
import {
  Alert,
  Avatar,
  Box,
  Button,
  Card,
  Chip,
  CircularProgress,
  Container,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  Grid,
  Skeleton,
  Snackbar,
  Stack,
  Step,
  StepConnector,
  stepConnectorClasses,
  StepLabel,
  Stepper,
  styled,
  TextField,
  Typography,
} from '@mui/material'
import type { StepIconProps } from '@mui/material/StepIcon'
import { motion } from 'framer-motion'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useMarkDelivered, useOrderById, useShipOrder } from '../hooks'
import type { ShippingAddress } from '../types'
import { getOrderActiveStep, getOrderStatusConfig } from '../utils'

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}

function isShippingAddress(value: unknown): value is ShippingAddress {
  return (
    isRecord(value) &&
    typeof value.fullName === 'string' &&
    typeof value.addressLine1 === 'string' &&
    typeof value.city === 'string' &&
    typeof value.state === 'string' &&
    typeof value.postalCode === 'string' &&
    typeof value.country === 'string' &&
    (value.addressLine2 === undefined || typeof value.addressLine2 === 'string') &&
    (value.phone === undefined || typeof value.phone === 'string')
  )
}

function parseShippingAddress(value: string | ShippingAddress | undefined): ShippingAddress | null {
  let address: unknown = value

  if (typeof value === 'string') {
    try {
      address = JSON.parse(value)
    } catch {
      return null
    }
  }

  return isShippingAddress(address) ? address : null
}

const ColorlibConnector = styled(StepConnector)(({ theme }) => ({
  [`&.${stepConnectorClasses.alternativeLabel}`]: {
    top: 22,
  },
  [`&.${stepConnectorClasses.active}`]: {
    [`& .${stepConnectorClasses.line}`]: {
      backgroundImage: `linear-gradient(95deg, ${palette.brand.primary} 0%, #A16207 100%)`,
    },
  },
  [`&.${stepConnectorClasses.completed}`]: {
    [`& .${stepConnectorClasses.line}`]: {
      backgroundImage: `linear-gradient(95deg, ${palette.semantic.success} 0%, #16A34A 100%)`,
    },
  },
  [`& .${stepConnectorClasses.line}`]: {
    height: 3,
    border: 0,
    backgroundColor: theme.palette.mode === 'dark' ? theme.palette.grey[800] : palette.neutral[200],
    borderRadius: 1,
  },
}))

const ColorlibStepIconRoot = styled('div')<{
  ownerState: { completed?: boolean; active?: boolean; error?: boolean }
}>(({ ownerState }) => ({
  backgroundColor: palette.neutral[200],
  zIndex: 1,
  color: palette.neutral[500],
  width: 50,
  height: 50,
  display: 'flex',
  borderRadius: '50%',
  justifyContent: 'center',
  alignItems: 'center',
  ...(ownerState.active && {
    backgroundImage: `linear-gradient(136deg, ${palette.brand.primary} 0%, #A16207 100%)`,
    boxShadow: `0 4px 10px 0 ${palette.brand.muted}`,
    color: 'white',
  }),
  ...(ownerState.completed && {
    backgroundImage: `linear-gradient(136deg, ${palette.semantic.success} 0%, #16A34A 100%)`,
    color: 'white',
  }),
  ...(ownerState.error && {
    backgroundImage: `linear-gradient(136deg, ${palette.semantic.error} 0%, #DC2626 100%)`,
    color: 'white',
  }),
}))

function ColorlibStepIcon(
  props: Readonly<{
    active: boolean
    completed: boolean
    error?: boolean
    icon: React.ReactNode
    className?: string
  }>
) {
  const { active, completed, error, className, icon } = props

  return (
    <ColorlibStepIconRoot ownerState={{ completed, active, error }} className={className}>
      {icon}
    </ColorlibStepIconRoot>
  )
}

interface OrderStepIconProps extends StepIconProps {
  icon: React.ReactNode
}

function OrderStepIcon({ icon, ...props }: Readonly<OrderStepIconProps>) {
  const { active = false, completed = false, error, className } = props
  return (
    <ColorlibStepIcon
      active={active}
      completed={completed}
      error={error}
      className={className}
      icon={icon}
    />
  )
}

export function OrderDetailPage() {
  const { t } = useTranslation('payments')
  const { user } = useAuth()
  const { orderId } = useParams<{ orderId: string }>()
  const navigate = useNavigate()
  const [showShipDialog, setShowShipDialog] = useState(false)
  const [trackingNumber, setTrackingNumber] = useState('')
  const [shippingCarrier, setShippingCarrier] = useState('')
  const [snackbar, setSnackbar] = useState<{
    open: boolean
    message: string
    severity: 'success' | 'error' | 'info'
  }>({ open: false, message: '', severity: 'info' })

  const { data: order, isLoading, error } = useOrderById(orderId ?? '')
  const shipOrder = useShipOrder()
  const markDelivered = useMarkDelivered()

  const handleCopyOrderId = async () => {
    if (!orderId) {
      return
    }

    try {
      await navigator.clipboard.writeText(orderId)
      setSnackbar({ open: true, message: t('orderDetail.copied'), severity: 'success' })
    } catch {
      setSnackbar({ open: true, message: t('orderDetail.copyFailed'), severity: 'error' })
    }
  }

  const handleShipOrder = async () => {
    if (!trackingNumber || !shippingCarrier || !orderId) {
      return
    }
    try {
      await shipOrder.mutateAsync({
        id: orderId,
        data: { trackingNumber, shippingCarrier },
      })
      setShowShipDialog(false)
      setTrackingNumber('')
      setShippingCarrier('')
      setSnackbar({ open: true, message: t('orderDetail.markedShipped'), severity: 'success' })
    } catch {
      setSnackbar({ open: true, message: t('orderDetail.updateFailed'), severity: 'error' })
    }
  }

  const handleMarkDelivered = async () => {
    if (!orderId) {
      return
    }
    try {
      await markDelivered.mutateAsync(orderId)
      setSnackbar({ open: true, message: t('orderDetail.markedDelivered'), severity: 'success' })
    } catch {
      setSnackbar({ open: true, message: t('orderDetail.updateFailed'), severity: 'error' })
    }
  }

  if (isLoading) {
    return <OrderDetailPageSkeleton />
  }

  if (error || !order) {
    return (
      <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
        <InlineAlert severity="error" sx={{ mb: 3 }}>
          {t('orderDetail.notFound')}
        </InlineAlert>
        <Button startIcon={<ArrowBack />} onClick={() => navigate('/orders')}>
          {t('orderDetail.back')}
        </Button>
      </Container>
    )
  }

  const steps = [
    {
      label: t('orderDetail.steps.placed'),
      icon: <ShoppingBag fontSize="small" />,
      date: order.createdAt,
    },
    {
      label: t('orderDetail.steps.payment'),
      icon: <Payment fontSize="small" />,
      date: order.paidAt,
    },
    {
      label: t('orderDetail.steps.shipped'),
      icon: <LocalShipping fontSize="small" />,
      date: order.shippedAt,
    },
    {
      label: t('orderDetail.steps.delivered'),
      icon: <Inventory fontSize="small" />,
      date: order.deliveredAt,
    },
    {
      label: t('orderDetail.steps.completed'),
      icon: <CheckCircle fontSize="small" />,
      date: order.completedAt,
    },
  ]

  const activeStep = getOrderActiveStep(order.status)
  const isCancelled = order.status === 'cancelled' || order.status === 'refunded'
  const currentUserId = (user?.userId || user?.id || '').toLowerCase()
  const isAdmin = user?.roles.some((role) => role.toLowerCase() === 'admin') ?? false
  const isBuyer = order.buyerId.toLowerCase() === currentUserId
  const isSeller = order.sellerId.toLowerCase() === currentUserId
  const canShip = order.status === 'paid' && (isSeller || isAdmin)
  const trackingUrl = getSafeHttpUrl(order.trackingUrl)
  const canMarkDelivered = order.status === 'shipped' && (isBuyer || isAdmin)

  const shippingAddress = parseShippingAddress(order.shippingAddress)

  return (
    <Box sx={{ bgcolor: palette.neutral[50], minHeight: '100vh', pb: 8 }}>
      <Container maxWidth="lg" sx={{ pt: 4 }}>
        <motion.div variants={staggerContainer} initial="initial" animate="animate">
          <motion.div variants={fadeInUp}>
            <Button
              startIcon={<ArrowBack />}
              component={Link}
              to="/orders"
              sx={{
                mb: 3,
                color: palette.neutral[500],
                '&:hover': { bgcolor: palette.neutral[100] },
              }}
            >
              {t('orderDetail.back')}
            </Button>
          </motion.div>

          <motion.div variants={staggerItem}>
            <Box
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'flex-start',
                mb: 4,
                flexWrap: 'wrap',
                gap: 2,
              }}
            >
              <Box>
                <Typography
                  variant="h4"
                  sx={{
                    fontFamily: '"Playfair Display", serif',
                    fontWeight: 700,
                    color: palette.neutral[900],
                    mb: 1,
                  }}
                >
                  {t('orderDetail.title')}
                </Typography>
                <Stack direction="row" alignItems="center" spacing={1}>
                  <Typography variant="body2" color="text.secondary">
                    {t('orderDetail.orderNumber', {
                      number: orderId?.slice(0, 8).toUpperCase(),
                    })}
                  </Typography>
                  <Button
                    size="small"
                    startIcon={<ContentCopy sx={{ fontSize: 16 }} />}
                    onClick={handleCopyOrderId}
                    sx={{
                      minWidth: 'auto',
                      px: 1,
                      color: palette.neutral[500],
                      textTransform: 'none',
                      fontSize: '0.75rem',
                    }}
                  >
                    {t('orderDetail.copy')}
                  </Button>
                </Stack>
              </Box>
              <Chip
                label={getOrderStatusConfig(order.status).label}
                color={getOrderStatusConfig(order.status).color}
                sx={{ fontWeight: 600 }}
              />
            </Box>
          </motion.div>

          {!isCancelled && (
            <motion.div variants={staggerItem}>
              <Card sx={{ p: 4, mb: 4, borderRadius: 2 }}>
                <Stepper alternativeLabel activeStep={activeStep} connector={<ColorlibConnector />}>
                  {steps.map((step, index) => (
                    <Step key={step.label}>
                      <StepLabel
                        slots={{
                          stepIcon: OrderStepIcon,
                        }}
                        slotProps={{
                          stepIcon: {
                            icon: step.icon,
                          },
                        }}
                      >
                        <Typography
                          sx={{
                            fontWeight: index <= activeStep ? 600 : 400,
                            color:
                              index <= activeStep ? palette.neutral[900] : palette.neutral[500],
                          }}
                        >
                          {step.label}
                        </Typography>
                        {step.date && (
                          <Typography variant="caption" color="text.secondary">
                            {formatDate(step.date)}
                          </Typography>
                        )}
                      </StepLabel>
                    </Step>
                  ))}
                </Stepper>
              </Card>
            </motion.div>
          )}

          {isCancelled && (
            <motion.div variants={staggerItem}>
              <InlineAlert severity="error" sx={{ mb: 4, borderRadius: 2 }}>
                {t('orderDetail.cancelledNotice', {
                  status: t(`orderStatuses.${order.status}`),
                })}
              </InlineAlert>
            </motion.div>
          )}

          <Grid container spacing={4}>
            <Grid size={{ xs: 12, md: 8 }}>
              <motion.div variants={staggerItem}>
                <Card sx={{ mb: 4, borderRadius: 2, overflow: 'hidden' }}>
                  <Box
                    sx={{
                      p: 3,
                      bgcolor: palette.neutral[50],
                      borderBottom: `1px solid ${palette.neutral[200]}`,
                    }}
                  >
                    <Typography variant="h6" fontWeight={600}>
                      {t('orderDetail.itemDetails')}
                    </Typography>
                  </Box>
                  <Box sx={{ p: 3 }}>
                    <Box sx={{ display: 'flex', gap: 3 }}>
                      <Avatar
                        variant="rounded"
                        src={order.auctionImageUrl}
                        sx={{ width: 120, height: 120 }}
                      >
                        <ShoppingBag sx={{ fontSize: 40 }} />
                      </Avatar>
                      <Box sx={{ flex: 1 }}>
                        <Typography
                          variant="h6"
                          component={Link}
                          to={`/auctions/${order.auctionId}`}
                          sx={{
                            fontWeight: 600,
                            color: palette.neutral[900],
                            textDecoration: 'none',
                            '&:hover': { color: palette.brand.primary },
                            display: 'flex',
                            alignItems: 'center',
                            gap: 0.5,
                          }}
                        >
                          {order.auctionTitle || order.itemTitle}
                          <OpenInNew sx={{ fontSize: 16 }} />
                        </Typography>
                        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                          {t('orderDetail.auctionId', { id: order.auctionId.slice(0, 8) })}
                        </Typography>
                        <Typography
                          variant="h5"
                          sx={{ fontWeight: 700, color: palette.brand.primary, mt: 2 }}
                        >
                          {formatCurrency(order.winningBid || order.winningBidAmount || 0)}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {t('checkout.winningBid')}
                        </Typography>
                      </Box>
                    </Box>
                  </Box>
                </Card>
              </motion.div>

              <motion.div variants={staggerItem}>
                <Card sx={{ mb: 4, borderRadius: 2, overflow: 'hidden' }}>
                  <Box
                    sx={{
                      p: 3,
                      bgcolor: palette.neutral[50],
                      borderBottom: `1px solid ${palette.neutral[200]}`,
                    }}
                  >
                    <Typography variant="h6" fontWeight={600}>
                      {t('orderDetail.shippingInformation')}
                    </Typography>
                  </Box>
                  <Box sx={{ p: 3 }}>
                    <Grid container spacing={3}>
                      <Grid size={{ xs: 12, sm: 6 }}>
                        <Stack spacing={2}>
                          <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1.5 }}>
                            <LocationOn sx={{ color: palette.neutral[500], mt: 0.3 }} />
                            <Box>
                              <Typography variant="subtitle2" fontWeight={600}>
                                {t('orderDetail.deliveryAddress')}
                              </Typography>
                              {shippingAddress ? (
                                <>
                                  <Typography variant="body2">
                                    {shippingAddress.fullName}
                                  </Typography>
                                  <Typography variant="body2" color="text.secondary">
                                    {shippingAddress.addressLine1}
                                    {shippingAddress.addressLine2 &&
                                      `, ${shippingAddress.addressLine2}`}
                                  </Typography>
                                  <Typography variant="body2" color="text.secondary">
                                    {shippingAddress.city}, {shippingAddress.state}{' '}
                                    {shippingAddress.postalCode}
                                  </Typography>
                                  <Typography variant="body2" color="text.secondary">
                                    {shippingAddress.country}
                                  </Typography>
                                  {shippingAddress.phone && (
                                    <Typography variant="body2" color="text.secondary">
                                      {shippingAddress.phone}
                                    </Typography>
                                  )}
                                </>
                              ) : (
                                <Typography variant="body2" color="text.secondary">
                                  {t('orderDetail.noAddress')}
                                </Typography>
                              )}
                            </Box>
                          </Box>
                        </Stack>
                      </Grid>
                      <Grid size={{ xs: 12, sm: 6 }}>
                        {order.trackingNumber ? (
                          <Stack spacing={2}>
                            <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1.5 }}>
                              <LocalShipping sx={{ color: palette.neutral[500], mt: 0.3 }} />
                              <Box>
                                <Typography variant="subtitle2" fontWeight={600}>
                                  {t('orderDetail.trackingInformation')}
                                </Typography>
                                <Typography variant="body2">
                                  {t('orderDetail.carrier', { carrier: order.shippingCarrier })}
                                </Typography>
                                <Typography
                                  variant="body2"
                                  sx={{
                                    color: palette.brand.primary,
                                    fontWeight: 500,
                                    display: 'flex',
                                    alignItems: 'center',
                                    gap: 0.5,
                                  }}
                                >
                                  {order.trackingNumber}
                                  {trackingUrl && (
                                    <Button
                                      size="small"
                                      component="a"
                                      href={trackingUrl}
                                      target="_blank"
                                      rel="noopener noreferrer"
                                      aria-label={t('orderDetail.openTracking')}
                                      sx={{ minWidth: 'auto', p: 0.5 }}
                                    >
                                      <OpenInNew sx={{ fontSize: 16 }} />
                                    </Button>
                                  )}
                                </Typography>
                              </Box>
                            </Box>
                          </Stack>
                        ) : (
                          <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1.5 }}>
                            <Pending sx={{ color: palette.neutral[500], mt: 0.3 }} />
                            <Box>
                              <Typography variant="subtitle2" fontWeight={600}>
                                {t('orderDetail.trackingInformation')}
                              </Typography>
                              <Typography variant="body2" color="text.secondary">
                                {t('orderDetail.trackingPending')}
                              </Typography>
                            </Box>
                          </Box>
                        )}
                      </Grid>
                    </Grid>
                  </Box>
                </Card>
              </motion.div>

              <motion.div variants={staggerItem}>
                <Card sx={{ borderRadius: 2, overflow: 'hidden' }}>
                  <Box
                    sx={{
                      p: 3,
                      bgcolor: palette.neutral[50],
                      borderBottom: `1px solid ${palette.neutral[200]}`,
                    }}
                  >
                    <Typography variant="h6" fontWeight={600}>
                      {t('orderDetail.timeline')}
                    </Typography>
                  </Box>
                  <Box sx={{ p: 3 }}>
                    <Stack spacing={2}>
                      {order.completedAt && (
                        <TimelineItem
                          icon={<CheckCircle sx={{ color: palette.semantic.success }} />}
                          title={t('orderDetail.timelineCompleted')}
                          date={order.completedAt}
                        />
                      )}
                      {order.deliveredAt && (
                        <TimelineItem
                          icon={<Inventory sx={{ color: palette.semantic.success }} />}
                          title={t('orderDetail.steps.delivered')}
                          date={order.deliveredAt}
                        />
                      )}
                      {order.shippedAt && (
                        <TimelineItem
                          icon={<LocalShipping sx={{ color: palette.semantic.info }} />}
                          title={t('orderDetail.shippedVia', {
                            carrier: order.shippingCarrier || t('orderDetail.carrierFallback'),
                          })}
                          date={order.shippedAt}
                          subtitle={
                            order.trackingNumber
                              ? t('orderDetail.tracking', { number: order.trackingNumber })
                              : undefined
                          }
                        />
                      )}
                      {order.paidAt && (
                        <TimelineItem
                          icon={<Payment sx={{ color: palette.semantic.success }} />}
                          title={t('orderDetail.paymentConfirmed')}
                          date={order.paidAt}
                        />
                      )}
                      <TimelineItem
                        icon={<ShoppingBag sx={{ color: palette.brand.primary }} />}
                        title={t('orderDetail.orderCreated')}
                        date={order.createdAt}
                      />
                    </Stack>
                  </Box>
                </Card>
              </motion.div>
            </Grid>

            <Grid size={{ xs: 12, md: 4 }}>
              <motion.div variants={staggerItem}>
                <Card sx={{ mb: 4, borderRadius: 2, overflow: 'hidden' }}>
                  <Box
                    sx={{
                      p: 3,
                      bgcolor: palette.neutral[50],
                      borderBottom: `1px solid ${palette.neutral[200]}`,
                    }}
                  >
                    <Typography variant="h6" fontWeight={600}>
                      {t('orderDetail.summary')}
                    </Typography>
                  </Box>
                  <Box sx={{ p: 3 }}>
                    <Stack spacing={2}>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography color="text.secondary">{t('checkout.winningBid')}</Typography>
                        <Typography fontWeight={500}>
                          {formatCurrency(order.winningBid || order.winningBidAmount || 0)}
                        </Typography>
                      </Box>
                      {(order.shippingCost ?? 0) > 0 && (
                        <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                          <Typography color="text.secondary">{t('checkout.shipping')}</Typography>
                          <Typography fontWeight={500}>
                            {formatCurrency(order.shippingCost || 0)}
                          </Typography>
                        </Box>
                      )}
                      {(order.platformFee ?? 0) > 0 && (
                        <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                          <Typography color="text.secondary">
                            {t('checkout.platformFee')}
                          </Typography>
                          <Typography fontWeight={500}>
                            {formatCurrency(order.platformFee || 0)}
                          </Typography>
                        </Box>
                      )}
                      <Divider />
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography fontWeight={600}>{t('checkout.total')}</Typography>
                        <Typography fontWeight={700} color={palette.brand.primary} variant="h6">
                          {formatCurrency(order.totalAmount)}
                        </Typography>
                      </Box>
                    </Stack>
                  </Box>
                </Card>
              </motion.div>

              <motion.div variants={staggerItem}>
                <Card sx={{ mb: 4, borderRadius: 2, overflow: 'hidden' }}>
                  <Box
                    sx={{
                      p: 3,
                      bgcolor: palette.neutral[50],
                      borderBottom: `1px solid ${palette.neutral[200]}`,
                    }}
                  >
                    <Typography variant="h6" fontWeight={600}>
                      {t('orderDetail.parties')}
                    </Typography>
                  </Box>
                  <Box sx={{ p: 3 }}>
                    <Stack spacing={3}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                        <Avatar sx={{ bgcolor: palette.neutral[200] }}>
                          <Person sx={{ color: palette.neutral[500] }} />
                        </Avatar>
                        <Box>
                          <Typography variant="caption" color="text.secondary">
                            {t('orders.buyer')}
                          </Typography>
                          <Typography fontWeight={500}>
                            {order.buyerName || order.buyerUsername}
                          </Typography>
                        </Box>
                      </Box>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                        <Avatar sx={{ bgcolor: palette.semantic.warningLight }}>
                          <Person sx={{ color: palette.brand.primary }} />
                        </Avatar>
                        <Box>
                          <Typography variant="caption" color="text.secondary">
                            {t('orders.seller')}
                          </Typography>
                          <Typography fontWeight={500}>
                            {order.sellerName || order.sellerUsername}
                          </Typography>
                        </Box>
                      </Box>
                    </Stack>
                  </Box>
                </Card>
              </motion.div>

              {!isCancelled && (canShip || canMarkDelivered) && (
                <motion.div variants={staggerItem}>
                  <Card sx={{ borderRadius: 2, overflow: 'hidden' }}>
                    <Box
                      sx={{
                        p: 3,
                        bgcolor: palette.neutral[50],
                        borderBottom: `1px solid ${palette.neutral[200]}`,
                      }}
                    >
                      <Typography variant="h6" fontWeight={600}>
                        {t('orderDetail.actions')}
                      </Typography>
                    </Box>
                    <Box sx={{ p: 3 }}>
                      <Stack spacing={2}>
                        {canShip && (
                          <Button
                            variant="contained"
                            fullWidth
                            startIcon={<LocalShipping />}
                            onClick={() => setShowShipDialog(true)}
                            sx={{
                              bgcolor: palette.brand.primary,
                              textTransform: 'none',
                              fontWeight: 600,
                              '&:hover': { bgcolor: '#A16207' },
                            }}
                          >
                            {t('orderDetail.markShipped')}
                          </Button>
                        )}
                        {canMarkDelivered && (
                          <Button
                            variant="contained"
                            fullWidth
                            startIcon={<Inventory />}
                            onClick={handleMarkDelivered}
                            disabled={markDelivered.isPending}
                            sx={{
                              bgcolor: palette.semantic.success,
                              textTransform: 'none',
                              fontWeight: 600,
                              '&:hover': { bgcolor: '#16A34A' },
                            }}
                          >
                            {markDelivered.isPending ? (
                              <CircularProgress size={20} color="inherit" />
                            ) : (
                              t('orderDetail.markDelivered')
                            )}
                          </Button>
                        )}
                      </Stack>
                    </Box>
                  </Card>
                </motion.div>
              )}
            </Grid>
          </Grid>
        </motion.div>
      </Container>

      <Dialog
        open={showShipDialog}
        onClose={() => setShowShipDialog(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle sx={{ fontWeight: 600 }}>{t('orderDetail.shipOrder')}</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
            {t('orderDetail.shipDescription')}
          </Typography>
          <TextField
            fullWidth
            label={t('orderDetail.shippingCarrier')}
            value={shippingCarrier}
            onChange={(e) => setShippingCarrier(e.target.value)}
            placeholder={t('orderDetail.carrierPlaceholder')}
            sx={{ mb: 2 }}
          />
          <TextField
            fullWidth
            label={t('orderDetail.trackingNumber')}
            value={trackingNumber}
            onChange={(e) => setTrackingNumber(e.target.value)}
            placeholder={t('orderDetail.trackingPlaceholder')}
          />
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button
            onClick={() => setShowShipDialog(false)}
            sx={{ color: palette.neutral[500], textTransform: 'none' }}
          >
            {t('common:cancel')}
          </Button>
          <Button
            variant="contained"
            onClick={handleShipOrder}
            disabled={!trackingNumber || !shippingCarrier || shipOrder.isPending}
            sx={{
              bgcolor: palette.brand.primary,
              textTransform: 'none',
              '&:hover': { bgcolor: '#A16207' },
            }}
          >
            {shipOrder.isPending ? (
              <CircularProgress size={20} color="inherit" />
            ) : (
              t('orderDetail.confirmShipment')
            )}
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={3000}
        onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert
          severity={snackbar.severity}
          onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}
          sx={{ width: '100%' }}
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  )
}

function TimelineItem({
  icon,
  title,
  date,
  subtitle,
}: Readonly<{
  icon: React.ReactNode
  title: string
  date: string
  subtitle?: string
}>) {
  return (
    <Box sx={{ display: 'flex', gap: 2 }}>
      <Box sx={{ pt: 0.3 }}>{icon}</Box>
      <Box>
        <Typography fontWeight={500}>{title}</Typography>
        {subtitle && (
          <Typography variant="body2" color="text.secondary">
            {subtitle}
          </Typography>
        )}
        <Typography variant="caption" color="text.secondary">
          {date ? formatDateTime(date) : '-'}
        </Typography>
      </Box>
    </Box>
  )
}

function OrderDetailPageSkeleton() {
  return (
    <Box sx={{ bgcolor: palette.neutral[50], minHeight: '100vh', pb: 8 }}>
      <Container maxWidth="lg" sx={{ pt: 4 }}>
        <Skeleton width={120} height={36} sx={{ mb: 3 }} />
        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 4 }}>
          <Box>
            <Skeleton width={200} height={40} />
            <Skeleton width={150} height={24} />
          </Box>
          <Skeleton width={100} height={32} sx={{ borderRadius: 4 }} />
        </Box>

        <Skeleton variant="rectangular" height={120} sx={{ mb: 4, borderRadius: 2 }} />

        <Grid container spacing={4}>
          <Grid size={{ xs: 12, md: 8 }}>
            <Skeleton variant="rectangular" height={200} sx={{ mb: 4, borderRadius: 2 }} />
            <Skeleton variant="rectangular" height={200} sx={{ mb: 4, borderRadius: 2 }} />
            <Skeleton variant="rectangular" height={300} sx={{ borderRadius: 2 }} />
          </Grid>
          <Grid size={{ xs: 12, md: 4 }}>
            <Skeleton variant="rectangular" height={200} sx={{ mb: 4, borderRadius: 2 }} />
            <Skeleton variant="rectangular" height={180} sx={{ mb: 4, borderRadius: 2 }} />
            <Skeleton variant="rectangular" height={150} sx={{ borderRadius: 2 }} />
          </Grid>
        </Grid>
      </Container>
    </Box>
  )
}
