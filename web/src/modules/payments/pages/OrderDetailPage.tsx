import { useState } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import {
  Container,
  Typography,
  Box,
  Card,
  Grid,
  Button,
  Chip,
  Divider,
  Skeleton,
  Alert,
  Snackbar,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  CircularProgress,
  Stack,
  Avatar,
  Stepper,
  Step,
  StepLabel,
  StepConnector,
  stepConnectorClasses,
  styled,
} from '@mui/material'
import {
  ArrowBack,
  LocalShipping,
  Payment,
  CheckCircle,
  ShoppingBag,
  Inventory,
  Cancel,
  ContentCopy,
  OpenInNew,
  Person,
  LocationOn,
  Pending,
} from '@mui/icons-material'
import { useOrderById, useShipOrder, useCancelOrder, useMarkDelivered } from '../hooks'
import type { OrderStatus } from '../types'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'

const ColorlibConnector = styled(StepConnector)(({ theme }) => ({
  [`&.${stepConnectorClasses.alternativeLabel}`]: {
    top: 22,
  },
  [`&.${stepConnectorClasses.active}`]: {
    [`& .${stepConnectorClasses.line}`]: {
      backgroundImage: 'linear-gradient(95deg, #CA8A04 0%, #A16207 100%)',
    },
  },
  [`&.${stepConnectorClasses.completed}`]: {
    [`& .${stepConnectorClasses.line}`]: {
      backgroundImage: 'linear-gradient(95deg, #22C55E 0%, #16A34A 100%)',
    },
  },
  [`& .${stepConnectorClasses.line}`]: {
    height: 3,
    border: 0,
    backgroundColor: theme.palette.mode === 'dark' ? theme.palette.grey[800] : '#E7E5E4',
    borderRadius: 1,
  },
}))

const ColorlibStepIconRoot = styled('div')<{
  ownerState: { completed?: boolean; active?: boolean; error?: boolean }
}>(({ ownerState }) => ({
  backgroundColor: '#E7E5E4',
  zIndex: 1,
  color: '#78716C',
  width: 50,
  height: 50,
  display: 'flex',
  borderRadius: '50%',
  justifyContent: 'center',
  alignItems: 'center',
  ...(ownerState.active && {
    backgroundImage: 'linear-gradient(136deg, #CA8A04 0%, #A16207 100%)',
    boxShadow: '0 4px 10px 0 rgba(202, 138, 4, .25)',
    color: 'white',
  }),
  ...(ownerState.completed && {
    backgroundImage: 'linear-gradient(136deg, #22C55E 0%, #16A34A 100%)',
    color: 'white',
  }),
  ...(ownerState.error && {
    backgroundImage: 'linear-gradient(136deg, #EF4444 0%, #DC2626 100%)',
    color: 'white',
  }),
}))

function ColorlibStepIcon(props: {
  active: boolean
  completed: boolean
  error?: boolean
  icon: React.ReactNode
  className?: string
}) {
  const { active, completed, error, className, icon } = props

  return (
    <ColorlibStepIconRoot ownerState={{ completed, active, error }} className={className}>
      {icon}
    </ColorlibStepIconRoot>
  )
}

const getStatusConfig = (status: OrderStatus) => {
  const config: Record<
    OrderStatus,
    { label: string; color: 'default' | 'primary' | 'success' | 'warning' | 'error' | 'info' }
  > = {
    pending: { label: 'Pending', color: 'warning' },
    payment_pending: { label: 'Awaiting Payment', color: 'warning' },
    paid: { label: 'Paid', color: 'info' },
    shipped: { label: 'Shipped', color: 'primary' },
    delivered: { label: 'Delivered', color: 'success' },
    completed: { label: 'Completed', color: 'success' },
    cancelled: { label: 'Cancelled', color: 'error' },
    refunded: { label: 'Refunded', color: 'default' },
  }
  return config[status]
}

const getActiveStep = (status: OrderStatus) => {
  const stepMap: Record<OrderStatus, number> = {
    pending: 0,
    payment_pending: 0,
    paid: 1,
    shipped: 2,
    delivered: 3,
    completed: 4,
    cancelled: -1,
    refunded: -1,
  }
  return stepMap[status]
}

const formatDate = (dateString?: string) => {
  if (!dateString) return '-'
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(amount)
}

export function OrderDetailPage() {
  const { orderId } = useParams<{ orderId: string }>()
  const navigate = useNavigate()
  const [showShipDialog, setShowShipDialog] = useState(false)
  const [showCancelDialog, setShowCancelDialog] = useState(false)
  const [trackingNumber, setTrackingNumber] = useState('')
  const [shippingCarrier, setShippingCarrier] = useState('')
  const [cancelReason, setCancelReason] = useState('')
  const [snackbar, setSnackbar] = useState<{
    open: boolean
    message: string
    severity: 'success' | 'error' | 'info'
  }>({ open: false, message: '', severity: 'info' })

  const { data: order, isLoading, error } = useOrderById(orderId!)
  const shipOrder = useShipOrder()
  const cancelOrder = useCancelOrder()
  const markDelivered = useMarkDelivered()

  const handleCopyOrderId = () => {
    navigator.clipboard.writeText(orderId!)
    setSnackbar({ open: true, message: 'Order ID copied to clipboard', severity: 'success' })
  }

  const handleShipOrder = async () => {
    if (!trackingNumber || !shippingCarrier) return
    try {
      await shipOrder.mutateAsync({
        id: orderId!,
        data: { trackingNumber, shippingCarrier },
      })
      setShowShipDialog(false)
      setTrackingNumber('')
      setShippingCarrier('')
      setSnackbar({ open: true, message: 'Order marked as shipped', severity: 'success' })
    } catch {
      setSnackbar({ open: true, message: 'Failed to update order', severity: 'error' })
    }
  }

  const handleCancelOrder = async () => {
    try {
      await cancelOrder.mutateAsync({
        id: orderId!,
        data: { reason: cancelReason },
      })
      setShowCancelDialog(false)
      setCancelReason('')
      setSnackbar({ open: true, message: 'Order cancelled', severity: 'success' })
    } catch {
      setSnackbar({ open: true, message: 'Failed to cancel order', severity: 'error' })
    }
  }

  const handleMarkDelivered = async () => {
    try {
      await markDelivered.mutateAsync(orderId!)
      setSnackbar({ open: true, message: 'Order marked as delivered', severity: 'success' })
    } catch {
      setSnackbar({ open: true, message: 'Failed to update order', severity: 'error' })
    }
  }

  if (isLoading) {
    return <OrderDetailPageSkeleton />
  }

  if (error || !order) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert severity="error" sx={{ mb: 3 }}>
          Order not found or you don't have permission to view it.
        </Alert>
        <Button startIcon={<ArrowBack />} onClick={() => navigate('/orders')}>
          Back to Orders
        </Button>
      </Container>
    )
  }

  const steps = [
    { label: 'Order Placed', icon: <ShoppingBag fontSize="small" />, date: order.createdAt },
    { label: 'Payment', icon: <Payment fontSize="small" />, date: order.paidAt },
    { label: 'Shipped', icon: <LocalShipping fontSize="small" />, date: order.shippedAt },
    { label: 'Delivered', icon: <Inventory fontSize="small" />, date: order.deliveredAt },
    { label: 'Completed', icon: <CheckCircle fontSize="small" />, date: order.completedAt },
  ]

  const activeStep = getActiveStep(order.status)
  const isCancelled = order.status === 'cancelled' || order.status === 'refunded'
  const canShip = order.status === 'paid'
  const canMarkDelivered = order.status === 'shipped'
  const canCancel =
    order.status === 'pending' || order.status === 'payment_pending' || order.status === 'paid'

  const shippingAddress =
    typeof order.shippingAddress === 'string'
      ? JSON.parse(order.shippingAddress || '{}')
      : order.shippingAddress

  return (
    <Box sx={{ bgcolor: '#FAFAF9', minHeight: '100vh', pb: 8 }}>
      <Container maxWidth="lg" sx={{ pt: 4 }}>
        <motion.div variants={staggerContainer} initial="initial" animate="animate">
          <motion.div variants={fadeInUp}>
            <Button
              startIcon={<ArrowBack />}
              component={Link}
              to="/orders"
              sx={{ mb: 3, color: '#78716C', '&:hover': { bgcolor: '#F5F5F4' } }}
            >
              Back to Orders
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
                    color: '#1C1917',
                    mb: 1,
                  }}
                >
                  Order Details
                </Typography>
                <Stack direction="row" alignItems="center" spacing={1}>
                  <Typography variant="body2" color="text.secondary">
                    Order #{orderId?.slice(0, 8).toUpperCase()}
                  </Typography>
                  <Button
                    size="small"
                    startIcon={<ContentCopy sx={{ fontSize: 16 }} />}
                    onClick={handleCopyOrderId}
                    sx={{
                      minWidth: 'auto',
                      px: 1,
                      color: '#78716C',
                      textTransform: 'none',
                      fontSize: '0.75rem',
                    }}
                  >
                    Copy
                  </Button>
                </Stack>
              </Box>
              <Chip
                label={getStatusConfig(order.status).label}
                color={getStatusConfig(order.status).color}
                sx={{ fontWeight: 600 }}
              />
            </Box>
          </motion.div>

          {!isCancelled && (
            <motion.div variants={staggerItem}>
              <Card sx={{ p: 4, mb: 4, borderRadius: 2 }}>
                <Stepper
                  alternativeLabel
                  activeStep={activeStep}
                  connector={<ColorlibConnector />}
                >
                  {steps.map((step, index) => (
                    <Step key={step.label}>
                      <StepLabel
                        StepIconComponent={(props) => (
                          <ColorlibStepIcon
                            {...props}
                            active={index === activeStep}
                            completed={index < activeStep}
                            icon={step.icon}
                          />
                        )}
                      >
                        <Typography
                          sx={{
                            fontWeight: index <= activeStep ? 600 : 400,
                            color: index <= activeStep ? '#1C1917' : '#78716C',
                          }}
                        >
                          {step.label}
                        </Typography>
                        {step.date && (
                          <Typography variant="caption" color="text.secondary">
                            {new Date(step.date).toLocaleDateString()}
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
              <Alert
                severity="error"
                icon={<Cancel />}
                sx={{ mb: 4, borderRadius: 2 }}
              >
                This order has been{' '}
                {order.status === 'refunded' ? 'refunded' : 'cancelled'}.
              </Alert>
            </motion.div>
          )}

          <Grid container spacing={4}>
            <Grid size={{ xs: 12, md: 8 }}>
              <motion.div variants={staggerItem}>
                <Card sx={{ mb: 4, borderRadius: 2, overflow: 'hidden' }}>
                  <Box sx={{ p: 3, bgcolor: '#FAFAF9', borderBottom: '1px solid #E7E5E4' }}>
                    <Typography variant="h6" fontWeight={600}>
                      Item Details
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
                            color: '#1C1917',
                            textDecoration: 'none',
                            '&:hover': { color: '#CA8A04' },
                            display: 'flex',
                            alignItems: 'center',
                            gap: 0.5,
                          }}
                        >
                          {order.auctionTitle || order.itemTitle}
                          <OpenInNew sx={{ fontSize: 16 }} />
                        </Typography>
                        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                          Auction ID: {order.auctionId.slice(0, 8)}
                        </Typography>
                        <Typography
                          variant="h5"
                          sx={{ fontWeight: 700, color: '#CA8A04', mt: 2 }}
                        >
                          {formatCurrency(order.winningBid || order.winningBidAmount || 0)}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          Winning Bid
                        </Typography>
                      </Box>
                    </Box>
                  </Box>
                </Card>
              </motion.div>

              <motion.div variants={staggerItem}>
                <Card sx={{ mb: 4, borderRadius: 2, overflow: 'hidden' }}>
                  <Box sx={{ p: 3, bgcolor: '#FAFAF9', borderBottom: '1px solid #E7E5E4' }}>
                    <Typography variant="h6" fontWeight={600}>
                      Shipping Information
                    </Typography>
                  </Box>
                  <Box sx={{ p: 3 }}>
                    <Grid container spacing={3}>
                      <Grid size={{ xs: 12, sm: 6 }}>
                        <Stack spacing={2}>
                          <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1.5 }}>
                            <LocationOn sx={{ color: '#78716C', mt: 0.3 }} />
                            <Box>
                              <Typography variant="subtitle2" fontWeight={600}>
                                Delivery Address
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
                                  No shipping address provided
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
                              <LocalShipping sx={{ color: '#78716C', mt: 0.3 }} />
                              <Box>
                                <Typography variant="subtitle2" fontWeight={600}>
                                  Tracking Information
                                </Typography>
                                <Typography variant="body2">
                                  Carrier: {order.shippingCarrier}
                                </Typography>
                                <Typography
                                  variant="body2"
                                  sx={{
                                    color: '#CA8A04',
                                    fontWeight: 500,
                                    display: 'flex',
                                    alignItems: 'center',
                                    gap: 0.5,
                                  }}
                                >
                                  {order.trackingNumber}
                                  {order.trackingUrl && (
                                    <Button
                                      size="small"
                                      component="a"
                                      href={order.trackingUrl}
                                      target="_blank"
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
                            <Pending sx={{ color: '#78716C', mt: 0.3 }} />
                            <Box>
                              <Typography variant="subtitle2" fontWeight={600}>
                                Tracking Information
                              </Typography>
                              <Typography variant="body2" color="text.secondary">
                                Tracking info will be available once shipped
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
                  <Box sx={{ p: 3, bgcolor: '#FAFAF9', borderBottom: '1px solid #E7E5E4' }}>
                    <Typography variant="h6" fontWeight={600}>
                      Order Timeline
                    </Typography>
                  </Box>
                  <Box sx={{ p: 3 }}>
                    <Stack spacing={2}>
                      {order.completedAt && (
                        <TimelineItem
                          icon={<CheckCircle sx={{ color: '#22C55E' }} />}
                          title="Order Completed"
                          date={order.completedAt}
                        />
                      )}
                      {order.deliveredAt && (
                        <TimelineItem
                          icon={<Inventory sx={{ color: '#22C55E' }} />}
                          title="Delivered"
                          date={order.deliveredAt}
                        />
                      )}
                      {order.shippedAt && (
                        <TimelineItem
                          icon={<LocalShipping sx={{ color: '#3B82F6' }} />}
                          title={`Shipped via ${order.shippingCarrier || 'Carrier'}`}
                          date={order.shippedAt}
                          subtitle={order.trackingNumber && `Tracking: ${order.trackingNumber}`}
                        />
                      )}
                      {order.paidAt && (
                        <TimelineItem
                          icon={<Payment sx={{ color: '#22C55E' }} />}
                          title="Payment Confirmed"
                          date={order.paidAt}
                        />
                      )}
                      <TimelineItem
                        icon={<ShoppingBag sx={{ color: '#CA8A04' }} />}
                        title="Order Created"
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
                  <Box sx={{ p: 3, bgcolor: '#FAFAF9', borderBottom: '1px solid #E7E5E4' }}>
                    <Typography variant="h6" fontWeight={600}>
                      Order Summary
                    </Typography>
                  </Box>
                  <Box sx={{ p: 3 }}>
                    <Stack spacing={2}>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography color="text.secondary">Winning Bid</Typography>
                        <Typography fontWeight={500}>
                          {formatCurrency(order.winningBid || order.winningBidAmount || 0)}
                        </Typography>
                      </Box>
                      {(order.shippingCost ?? 0) > 0 && (
                        <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                          <Typography color="text.secondary">Shipping</Typography>
                          <Typography fontWeight={500}>
                            {formatCurrency(order.shippingCost || 0)}
                          </Typography>
                        </Box>
                      )}
                      {(order.platformFee ?? 0) > 0 && (
                        <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                          <Typography color="text.secondary">Platform Fee</Typography>
                          <Typography fontWeight={500}>
                            {formatCurrency(order.platformFee || 0)}
                          </Typography>
                        </Box>
                      )}
                      <Divider />
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography fontWeight={600}>Total</Typography>
                        <Typography fontWeight={700} color="#CA8A04" variant="h6">
                          {formatCurrency(order.totalAmount)}
                        </Typography>
                      </Box>
                    </Stack>
                  </Box>
                </Card>
              </motion.div>

              <motion.div variants={staggerItem}>
                <Card sx={{ mb: 4, borderRadius: 2, overflow: 'hidden' }}>
                  <Box sx={{ p: 3, bgcolor: '#FAFAF9', borderBottom: '1px solid #E7E5E4' }}>
                    <Typography variant="h6" fontWeight={600}>
                      Parties
                    </Typography>
                  </Box>
                  <Box sx={{ p: 3 }}>
                    <Stack spacing={3}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                        <Avatar sx={{ bgcolor: '#E7E5E4' }}>
                          <Person sx={{ color: '#78716C' }} />
                        </Avatar>
                        <Box>
                          <Typography variant="caption" color="text.secondary">
                            Buyer
                          </Typography>
                          <Typography fontWeight={500}>
                            {order.buyerName || order.buyerUsername}
                          </Typography>
                        </Box>
                      </Box>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                        <Avatar sx={{ bgcolor: '#FEF3C7' }}>
                          <Person sx={{ color: '#CA8A04' }} />
                        </Avatar>
                        <Box>
                          <Typography variant="caption" color="text.secondary">
                            Seller
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

              {!isCancelled && (canShip || canMarkDelivered || canCancel) && (
                <motion.div variants={staggerItem}>
                  <Card sx={{ borderRadius: 2, overflow: 'hidden' }}>
                    <Box sx={{ p: 3, bgcolor: '#FAFAF9', borderBottom: '1px solid #E7E5E4' }}>
                      <Typography variant="h6" fontWeight={600}>
                        Actions
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
                              bgcolor: '#CA8A04',
                              textTransform: 'none',
                              fontWeight: 600,
                              '&:hover': { bgcolor: '#A16207' },
                            }}
                          >
                            Mark as Shipped
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
                              bgcolor: '#22C55E',
                              textTransform: 'none',
                              fontWeight: 600,
                              '&:hover': { bgcolor: '#16A34A' },
                            }}
                          >
                            {markDelivered.isPending ? (
                              <CircularProgress size={20} color="inherit" />
                            ) : (
                              'Mark as Delivered'
                            )}
                          </Button>
                        )}
                        {canCancel && (
                          <Button
                            variant="outlined"
                            fullWidth
                            startIcon={<Cancel />}
                            onClick={() => setShowCancelDialog(true)}
                            sx={{
                              borderColor: '#EF4444',
                              color: '#EF4444',
                              textTransform: 'none',
                              fontWeight: 600,
                              '&:hover': { borderColor: '#DC2626', bgcolor: '#FEF2F2' },
                            }}
                          >
                            Cancel Order
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

      <Dialog open={showShipDialog} onClose={() => setShowShipDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 600 }}>Ship Order</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
            Enter the tracking information for this shipment.
          </Typography>
          <TextField
            fullWidth
            label="Shipping Carrier"
            value={shippingCarrier}
            onChange={(e) => setShippingCarrier(e.target.value)}
            placeholder="e.g., UPS, FedEx, USPS"
            sx={{ mb: 2 }}
          />
          <TextField
            fullWidth
            label="Tracking Number"
            value={trackingNumber}
            onChange={(e) => setTrackingNumber(e.target.value)}
            placeholder="Enter tracking number"
          />
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button
            onClick={() => setShowShipDialog(false)}
            sx={{ color: '#78716C', textTransform: 'none' }}
          >
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={handleShipOrder}
            disabled={!trackingNumber || !shippingCarrier || shipOrder.isPending}
            sx={{
              bgcolor: '#CA8A04',
              textTransform: 'none',
              '&:hover': { bgcolor: '#A16207' },
            }}
          >
            {shipOrder.isPending ? <CircularProgress size={20} color="inherit" /> : 'Confirm Shipment'}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={showCancelDialog} onClose={() => setShowCancelDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 600, color: '#EF4444' }}>Cancel Order</DialogTitle>
        <DialogContent>
          <Alert severity="warning" sx={{ mb: 3 }}>
            Are you sure you want to cancel this order? This action cannot be undone.
          </Alert>
          <TextField
            fullWidth
            label="Reason for Cancellation (Optional)"
            value={cancelReason}
            onChange={(e) => setCancelReason(e.target.value)}
            multiline
            rows={3}
            placeholder="Provide a reason for cancellation..."
          />
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button
            onClick={() => setShowCancelDialog(false)}
            sx={{ color: '#78716C', textTransform: 'none' }}
          >
            Keep Order
          </Button>
          <Button
            variant="contained"
            onClick={handleCancelOrder}
            disabled={cancelOrder.isPending}
            sx={{
              bgcolor: '#EF4444',
              textTransform: 'none',
              '&:hover': { bgcolor: '#DC2626' },
            }}
          >
            {cancelOrder.isPending ? (
              <CircularProgress size={20} color="inherit" />
            ) : (
              'Cancel Order'
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
}: {
  icon: React.ReactNode
  title: string
  date: string
  subtitle?: string
}) {
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
          {formatDate(date)}
        </Typography>
      </Box>
    </Box>
  )
}

function OrderDetailPageSkeleton() {
  return (
    <Box sx={{ bgcolor: '#FAFAF9', minHeight: '100vh', pb: 8 }}>
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
