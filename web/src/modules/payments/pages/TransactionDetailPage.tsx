import { useParams, useNavigate, Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import {
  Container,
  Card,
  Typography,
  Box,
  Button,
  Chip,
  Skeleton,
  Alert,
  Stack,
  Divider,
  Grid,
} from '@mui/material'
import {
  ArrowBack,
  ArrowUpward,
  ArrowDownward,
  Receipt,
  AccessTime,
  AccountBalanceWallet,
  ContentCopy,
  CheckCircle,
  Pending,
  Cancel,
} from '@mui/icons-material'
import { useQuery } from '@tanstack/react-query'
import { walletsApi } from '../api'
import type { TransactionType, TransactionStatus } from '../types'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'

const formatCurrency = (amount: number, currency = 'USD') => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency,
  }).format(amount)
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  })
}

const getTransactionTypeConfig = (type: TransactionType) => {
  const configs: Record<TransactionType, { label: string; icon: React.ReactNode; color: string; bgColor: string }> = {
    deposit: {
      label: 'Deposit',
      icon: <ArrowDownward />,
      color: '#22C55E',
      bgColor: '#DCFCE7',
    },
    withdrawal: {
      label: 'Withdrawal',
      icon: <ArrowUpward />,
      color: '#EF4444',
      bgColor: '#FEE2E2',
    },
    payment: {
      label: 'Payment',
      icon: <ArrowUpward />,
      color: '#EF4444',
      bgColor: '#FEE2E2',
    },
    refund: {
      label: 'Refund',
      icon: <ArrowDownward />,
      color: '#22C55E',
      bgColor: '#DCFCE7',
    },
    hold: {
      label: 'Hold',
      icon: <Pending />,
      color: '#F59E0B',
      bgColor: '#FEF3C7',
    },
    release: {
      label: 'Release',
      icon: <CheckCircle />,
      color: '#22C55E',
      bgColor: '#DCFCE7',
    },
    escrow_hold: {
      label: 'Escrow Hold',
      icon: <Pending />,
      color: '#F59E0B',
      bgColor: '#FEF3C7',
    },
    escrow_release: {
      label: 'Escrow Release',
      icon: <CheckCircle />,
      color: '#22C55E',
      bgColor: '#DCFCE7',
    },
    fee: {
      label: 'Platform Fee',
      icon: <Receipt />,
      color: '#78716C',
      bgColor: '#F5F5F4',
    },
  }
  return configs[type] || { label: type, icon: <Receipt />, color: '#78716C', bgColor: '#F5F5F4' }
}

const getStatusConfig = (status: TransactionStatus) => {
  const configs: Record<TransactionStatus, { label: string; color: 'success' | 'warning' | 'error' | 'default'; icon: React.ReactElement }> = {
    completed: { label: 'Completed', color: 'success', icon: <CheckCircle fontSize="small" /> },
    pending: { label: 'Pending', color: 'warning', icon: <Pending fontSize="small" /> },
    failed: { label: 'Failed', color: 'error', icon: <Cancel fontSize="small" /> },
    cancelled: { label: 'Cancelled', color: 'default', icon: <Cancel fontSize="small" /> },
  }
  return configs[status]
}

export function TransactionDetailPage() {
  const { transactionId } = useParams<{ transactionId: string }>()
  const navigate = useNavigate()

  const { data: transaction, isLoading, error } = useQuery({
    queryKey: ['transaction', transactionId],
    queryFn: () => walletsApi.getTransactionById(transactionId!),
    enabled: !!transactionId,
  })

  const handleCopyId = () => {
    navigator.clipboard.writeText(transactionId!)
  }

  if (isLoading) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Skeleton width={120} height={36} sx={{ mb: 3 }} />
        <Card sx={{ p: 4 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 3, mb: 4 }}>
            <Skeleton variant="circular" width={64} height={64} />
            <Box>
              <Skeleton width={200} height={32} />
              <Skeleton width={150} />
            </Box>
          </Box>
          <Skeleton variant="rectangular" height={200} sx={{ borderRadius: 2 }} />
        </Card>
      </Container>
    )
  }

  if (error || !transaction) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="error" sx={{ mb: 3 }}>
          Transaction not found or you don't have permission to view it.
        </Alert>
        <Button startIcon={<ArrowBack />} onClick={() => navigate('/wallet')}>
          Back to Wallet
        </Button>
      </Container>
    )
  }

  const typeConfig = getTransactionTypeConfig(transaction.type)
  const statusConfig = getStatusConfig(transaction.status)
  const isPositive = ['deposit', 'refund', 'escrow_release'].includes(transaction.type)

  return (
    <Box sx={{ bgcolor: '#FAFAF9', minHeight: '100vh', pb: 8 }}>
      <Container maxWidth="md" sx={{ pt: 4 }}>
        <motion.div variants={staggerContainer} initial="initial" animate="animate">
          <motion.div variants={fadeInUp}>
            <Button
              startIcon={<ArrowBack />}
              component={Link}
              to="/wallet"
              sx={{ mb: 3, color: '#78716C', '&:hover': { bgcolor: '#F5F5F4' } }}
            >
              Back to Wallet
            </Button>
          </motion.div>

          <motion.div variants={staggerItem}>
            <Card sx={{ borderRadius: 3, overflow: 'hidden' }}>
              <Box
                sx={{
                  p: 4,
                  background: `linear-gradient(135deg, ${typeConfig.bgColor} 0%, #FFFFFF 100%)`,
                  borderBottom: '1px solid #E7E5E4',
                }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 3 }}>
                  <Box
                    sx={{
                      width: 64,
                      height: 64,
                      borderRadius: '50%',
                      bgcolor: 'white',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      boxShadow: '0 4px 12px rgba(0,0,0,0.08)',
                      color: typeConfig.color,
                    }}
                  >
                    {typeConfig.icon}
                  </Box>
                  <Box sx={{ flex: 1 }}>
                    <Typography variant="h5" fontWeight={700} color="#1C1917">
                      {typeConfig.label}
                    </Typography>
                    <Stack direction="row" alignItems="center" spacing={1} sx={{ mt: 0.5 }}>
                      <Typography variant="body2" color="text.secondary">
                        ID: {transactionId?.slice(0, 12)}...
                      </Typography>
                      <Button
                        size="small"
                        onClick={handleCopyId}
                        sx={{ minWidth: 'auto', p: 0.5, color: '#78716C' }}
                      >
                        <ContentCopy sx={{ fontSize: 16 }} />
                      </Button>
                    </Stack>
                  </Box>
                  <Chip
                    icon={statusConfig.icon}
                    label={statusConfig.label}
                    color={statusConfig.color}
                    sx={{ fontWeight: 600 }}
                  />
                </Box>
              </Box>

              <Box sx={{ p: 4 }}>
                <Box sx={{ textAlign: 'center', mb: 4 }}>
                  <Typography
                    variant="h3"
                    sx={{
                      fontWeight: 700,
                      color: isPositive ? '#22C55E' : '#EF4444',
                    }}
                  >
                    {isPositive ? '+' : '-'}
                    {formatCurrency(transaction.amount)}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                    Balance after: {formatCurrency(transaction.balance)}
                  </Typography>
                </Box>

                <Divider sx={{ my: 3 }} />

                <Grid container spacing={3}>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <DetailItem
                      icon={<AccessTime sx={{ color: '#78716C' }} />}
                      label="Date & Time"
                      value={formatDate(transaction.createdAt)}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <DetailItem
                      icon={<AccountBalanceWallet sx={{ color: '#78716C' }} />}
                      label="Wallet Balance"
                      value={formatCurrency(transaction.balance)}
                    />
                  </Grid>
                  {transaction.description && (
                    <Grid size={{ xs: 12 }}>
                      <DetailItem
                        icon={<Receipt sx={{ color: '#78716C' }} />}
                        label="Description"
                        value={transaction.description}
                      />
                    </Grid>
                  )}
                  {transaction.reference && (
                    <Grid size={{ xs: 12 }}>
                      <DetailItem
                        icon={<Receipt sx={{ color: '#78716C' }} />}
                        label="Reference"
                        value={transaction.reference}
                      />
                    </Grid>
                  )}
                </Grid>
              </Box>
            </Card>
          </motion.div>

          {transaction.relatedOrderId && (
            <motion.div variants={staggerItem}>
              <Card sx={{ mt: 3, p: 3, borderRadius: 2 }}>
                <Typography variant="subtitle2" fontWeight={600} gutterBottom>
                  Related Order
                </Typography>
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    p: 2,
                    bgcolor: '#F5F5F4',
                    borderRadius: 1,
                  }}
                >
                  <Typography variant="body2">
                    Order #{transaction.relatedOrderId.slice(0, 8).toUpperCase()}
                  </Typography>
                  <Button
                    component={Link}
                    to={`/orders/${transaction.relatedOrderId}`}
                    size="small"
                    sx={{ color: '#CA8A04', fontWeight: 600 }}
                  >
                    View Order
                  </Button>
                </Box>
              </Card>
            </motion.div>
          )}
        </motion.div>
      </Container>
    </Box>
  )
}

function DetailItem({
  icon,
  label,
  value,
}: {
  icon: React.ReactNode
  label: string
  value: string
}) {
  return (
    <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 2 }}>
      <Box sx={{ mt: 0.3 }}>{icon}</Box>
      <Box>
        <Typography variant="caption" color="text.secondary">
          {label}
        </Typography>
        <Typography variant="body1" fontWeight={500}>
          {value}
        </Typography>
      </Box>
    </Box>
  )
}
