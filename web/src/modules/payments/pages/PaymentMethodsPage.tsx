import { useState } from 'react'
import { motion } from 'framer-motion'
import { Link } from 'react-router-dom'
import {
  Container,
  Typography,
  Box,
  Card,
  Button,
  IconButton,
  Chip,
  Skeleton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Stack,
  Divider,
  Grid,
} from '@mui/material'
import { InlineAlert } from '@/shared/ui'
import {
  ArrowBack,
  CreditCard,
  AccountBalance,
  Add,
  Delete,
  Star,
  StarBorder,
  CheckCircle,
  Lock,
} from '@mui/icons-material'
import { palette } from '@/shared/theme/tokens'
import {
  usePaymentMethods,
  useAddPaymentMethod,
  useRemovePaymentMethod,
  useSetDefaultPaymentMethod,
} from '../hooks'
import type { PaymentMethod } from '../types'
import { getCardBrandIcon, formatCardExpiry } from '../utils'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'

function PaymentMethodCard({
  method,
  onSetDefault,
  onDelete,
  isDeleting,
}: {
  method: PaymentMethod
  onSetDefault: () => void
  onDelete: () => void
  isDeleting: boolean
}) {
  return (
    <Card
      component={motion.div}
      variants={staggerItem}
      sx={{
        p: 3,
        borderRadius: 2,
        border: method.isDefault
          ? `2px solid ${palette.brand.primary}`
          : `1px solid ${palette.neutral[200]}`,
        position: 'relative',
        '&:hover': { boxShadow: '0 4px 20px rgba(0,0,0,0.08)' },
      }}
    >
      {method.isDefault && (
        <Chip
          label="Default"
          size="small"
          icon={<CheckCircle sx={{ fontSize: 16 }} />}
          sx={{
            position: 'absolute',
            top: 12,
            right: 12,
            bgcolor: palette.semantic.warningLight,
            color: '#92400E',
            fontWeight: 600,
            '& .MuiChip-icon': { color: palette.brand.primary },
          }}
        />
      )}

      <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 2 }}>
        <Box
          sx={{
            width: 56,
            height: 56,
            borderRadius: 2,
            bgcolor: method.type === 'card' ? palette.neutral[100] : palette.semantic.infoLight,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
          }}
        >
          {method.type === 'card' ? (
            getCardBrandIcon(method.brand)
          ) : (
            <AccountBalance sx={{ fontSize: 32, color: palette.semantic.info }} />
          )}
        </Box>

        <Box sx={{ flex: 1 }}>
          <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 0.5 }}>
            {method.type === 'card'
              ? `${method.brand || 'Card'} •••• ${method.last4}`
              : `Bank Account •••• ${method.last4}`}
          </Typography>
          {method.type === 'card' && method.expiryMonth && method.expiryYear && (
            <Typography variant="body2" color="text.secondary">
              Expires {formatCardExpiry(method.expiryMonth, method.expiryYear)}
            </Typography>
          )}
        </Box>
      </Box>

      <Divider sx={{ my: 2 }} />

      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        {!method.isDefault ? (
          <Button
            size="small"
            startIcon={<StarBorder />}
            onClick={onSetDefault}
            sx={{ color: palette.neutral[500], textTransform: 'none' }}
          >
            Set as default
          </Button>
        ) : (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <Star sx={{ fontSize: 16, color: palette.brand.primary }} />
            <Typography variant="body2" color="text.secondary">
              Default payment method
            </Typography>
          </Box>
        )}

        <IconButton
          onClick={onDelete}
          disabled={isDeleting}
          sx={{
            color: palette.semantic.error,
            '&:hover': { bgcolor: palette.semantic.errorLight },
          }}
        >
          <Delete fontSize="small" />
        </IconButton>
      </Box>
    </Card>
  )
}

function AddPaymentMethodDialog({
  open,
  onClose,
  onSubmit,
  isLoading,
}: {
  open: boolean
  onClose: () => void
  onSubmit: (token: string) => void
  isLoading: boolean
}) {
  const [cardNumber, setCardNumber] = useState('')
  const [expiry, setExpiry] = useState('')
  const [cvc, setCvc] = useState('')
  const [name, setName] = useState('')

  const handleSubmit = () => {
    onSubmit(`card_${Date.now()}`)
  }

  const formatCardNumber = (value: string) => {
    const v = value.replace(/\s+/g, '').replace(/[^0-9]/gi, '')
    const matches = v.match(/\d{4,16}/g)
    const match = (matches && matches[0]) || ''
    const parts = []
    for (let i = 0, len = match.length; i < len; i += 4) {
      parts.push(match.substring(i, i + 4))
    }
    return parts.length ? parts.join(' ') : v
  }

  const formatExpiryDate = (value: string) => {
    const v = value.replace(/\s+/g, '').replace(/[^0-9]/gi, '')
    if (v.length >= 2) {
      return `${v.substring(0, 2)  }/${  v.substring(2, 4)}`
    }
    return v
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle sx={{ fontWeight: 600 }}>Add Payment Method</DialogTitle>
      <DialogContent>
        <Box sx={{ pt: 1 }}>
          <InlineAlert severity="info" sx={{ mb: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Lock fontSize="small" />
              Your payment information is securely encrypted
            </Box>
          </InlineAlert>

          <Stack spacing={2.5}>
            <TextField
              label="Cardholder Name"
              fullWidth
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="John Doe"
            />

            <TextField
              label="Card Number"
              fullWidth
              value={cardNumber}
              onChange={(e) => setCardNumber(formatCardNumber(e.target.value))}
              placeholder="4242 4242 4242 4242"
              inputProps={{ maxLength: 19 }}
              InputProps={{
                endAdornment: <CreditCard sx={{ color: palette.neutral[500] }} />,
              }}
            />

            <Grid container spacing={2}>
              <Grid size={{ xs: 6 }}>
                <TextField
                  label="Expiry Date"
                  fullWidth
                  value={expiry}
                  onChange={(e) => setExpiry(formatExpiryDate(e.target.value))}
                  placeholder="MM/YY"
                  inputProps={{ maxLength: 5 }}
                />
              </Grid>
              <Grid size={{ xs: 6 }}>
                <TextField
                  label="CVC"
                  fullWidth
                  value={cvc}
                  onChange={(e) => setCvc(e.target.value.replace(/[^0-9]/g, ''))}
                  placeholder="123"
                  inputProps={{ maxLength: 4 }}
                />
              </Grid>
            </Grid>
          </Stack>
        </Box>
      </DialogContent>
      <DialogActions sx={{ p: 3, pt: 0 }}>
        <Button onClick={onClose} sx={{ color: palette.neutral[500], textTransform: 'none' }}>
          Cancel
        </Button>
        <Button
          variant="contained"
          onClick={handleSubmit}
          disabled={isLoading || !cardNumber || !expiry || !cvc || !name}
          sx={{
            bgcolor: palette.brand.primary,
            textTransform: 'none',
            '&:hover': { bgcolor: '#A16207' },
          }}
        >
          {isLoading ? 'Adding...' : 'Add Card'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}

function PaymentMethodsSkeleton() {
  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Skeleton width={120} height={36} sx={{ mb: 3 }} />
      <Skeleton width={200} height={40} sx={{ mb: 1 }} />
      <Skeleton width={300} sx={{ mb: 4 }} />
      <Grid container spacing={3}>
        {[1, 2].map((i) => (
          <Grid key={i} size={{ xs: 12, sm: 6 }}>
            <Skeleton variant="rectangular" height={180} sx={{ borderRadius: 2 }} />
          </Grid>
        ))}
      </Grid>
    </Container>
  )
}

export function PaymentMethodsPage() {
  const [showAddDialog, setShowAddDialog] = useState(false)
  const [deletingId, setDeletingId] = useState<string | null>(null)

  const { data: paymentMethods, isLoading } = usePaymentMethods()
  const addPaymentMethod = useAddPaymentMethod()
  const removePaymentMethod = useRemovePaymentMethod()
  const setDefaultPaymentMethod = useSetDefaultPaymentMethod()

  const handleAddPaymentMethod = async (token: string) => {
    try {
      await addPaymentMethod.mutateAsync(token)
      setShowAddDialog(false)
    } catch {
      // Error handled by mutation
    }
  }

  const handleDeletePaymentMethod = async (id: string) => {
    setDeletingId(id)
    try {
      await removePaymentMethod.mutateAsync(id)
    } catch {
      // Error handled by mutation
    } finally {
      setDeletingId(null)
    }
  }

  const handleSetDefault = async (id: string) => {
    try {
      await setDefaultPaymentMethod.mutateAsync(id)
    } catch {
      // Error handled by mutation
    }
  }

  if (isLoading) {
    return <PaymentMethodsSkeleton />
  }

  return (
    <Box sx={{ bgcolor: palette.neutral[50], minHeight: '100vh', pb: 8 }}>
      <Container maxWidth="md" sx={{ pt: 4 }}>
        <motion.div variants={staggerContainer} initial="initial" animate="animate">
          <motion.div variants={fadeInUp}>
            <Button
              startIcon={<ArrowBack />}
              component={Link}
              to="/wallet"
              sx={{
                mb: 3,
                color: palette.neutral[500],
                '&:hover': { bgcolor: palette.neutral[100] },
              }}
            >
              Back to Wallet
            </Button>
          </motion.div>

          <motion.div variants={staggerItem}>
            <Box sx={{ mb: 4 }}>
              <Typography
                variant="h4"
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontWeight: 700,
                  color: palette.neutral[900],
                  mb: 1,
                }}
              >
                Payment Methods
              </Typography>
              <Typography color="text.secondary">
                Manage your saved cards and bank accounts
              </Typography>
            </Box>
          </motion.div>

          <motion.div variants={staggerItem}>
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => setShowAddDialog(true)}
              sx={{
                mb: 4,
                bgcolor: palette.brand.primary,
                textTransform: 'none',
                fontWeight: 600,
                '&:hover': { bgcolor: '#A16207' },
              }}
            >
              Add Payment Method
            </Button>
          </motion.div>

          {!paymentMethods || paymentMethods.length === 0 ? (
            <motion.div variants={staggerItem}>
              <Card sx={{ p: 6, textAlign: 'center', borderRadius: 2 }}>
                <CreditCard sx={{ fontSize: 64, color: palette.neutral[200], mb: 2 }} />
                <Typography variant="h6" gutterBottom>
                  No payment methods saved
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                  Add a card or bank account to make payments faster
                </Typography>
                <Button
                  variant="outlined"
                  startIcon={<Add />}
                  onClick={() => setShowAddDialog(true)}
                  sx={{
                    borderColor: palette.brand.primary,
                    color: palette.brand.primary,
                    textTransform: 'none',
                    '&:hover': { borderColor: '#A16207', bgcolor: palette.semantic.warningLight },
                  }}
                >
                  Add Payment Method
                </Button>
              </Card>
            </motion.div>
          ) : (
            <Grid container spacing={3}>
              {paymentMethods.map((method) => (
                <Grid key={method.id} size={{ xs: 12, sm: 6 }}>
                  <PaymentMethodCard
                    method={method}
                    onSetDefault={() => handleSetDefault(method.id)}
                    onDelete={() => handleDeletePaymentMethod(method.id)}
                    isDeleting={deletingId === method.id}
                  />
                </Grid>
              ))}
            </Grid>
          )}

          <motion.div variants={staggerItem}>
            <Card sx={{ p: 3, mt: 4, borderRadius: 2, bgcolor: palette.neutral[100] }}>
              <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 2 }}>
                <Lock sx={{ color: palette.neutral[500], mt: 0.5 }} />
                <Box>
                  <Typography variant="subtitle2" fontWeight={600} gutterBottom>
                    Your payment information is secure
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    We use industry-standard encryption to protect your data. Your card details are
                    never stored on our servers and are processed securely through Stripe.
                  </Typography>
                </Box>
              </Box>
            </Card>
          </motion.div>
        </motion.div>
      </Container>

      <AddPaymentMethodDialog
        open={showAddDialog}
        onClose={() => setShowAddDialog(false)}
        onSubmit={handleAddPaymentMethod}
        isLoading={addPaymentMethod.isPending}
      />
    </Box>
  )
}
