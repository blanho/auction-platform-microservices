import { useToast } from '@/app/providers'
import { getCurrentLocale } from '@/i18n'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import { palette } from '@/shared/theme/tokens'
import { InlineAlert } from '@/shared/ui'
import { formatCurrency } from '@/shared/utils/formatters'
import {
  AccessTime,
  AccountBalanceWallet,
  ArrowBack,
  ContentCopy,
  Receipt,
} from '@mui/icons-material'
import {
  Box,
  Button,
  Card,
  Chip,
  Container,
  Divider,
  Grid,
  Skeleton,
  Stack,
  Typography,
} from '@mui/material'
import { useQuery } from '@tanstack/react-query'
import { motion } from 'framer-motion'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { walletsApi } from '../api'
import { getTransactionStatusConfig, getTransactionTypeConfig } from '../utils'

const formatDateLong = (dateString: string) => {
  return new Date(dateString).toLocaleDateString(getCurrentLocale(), {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  })
}

export function TransactionDetailPage() {
  const { t } = useTranslation('payments')
  const { transactionId } = useParams<{ transactionId: string }>()
  const navigate = useNavigate()
  const toast = useToast()

  const {
    data: transaction,
    isLoading,
    error,
  } = useQuery({
    queryKey: ['transaction', transactionId],
    queryFn: () => {
      if (!transactionId) {
        throw new Error(t('transaction.idRequired'))
      }
      return walletsApi.getTransactionById(transactionId)
    },
    enabled: !!transactionId,
  })

  const handleCopyId = async () => {
    if (!transactionId) {
      return
    }

    try {
      await navigator.clipboard.writeText(transactionId)
      toast.success(t('transaction.copied'))
    } catch {
      toast.error(t('transaction.copyFailed'))
    }
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
        <InlineAlert severity="error" sx={{ mb: 3 }}>
          {t('transaction.notFound')}
        </InlineAlert>
        <Button startIcon={<ArrowBack />} onClick={() => navigate('/wallet')}>
          {t('transaction.backToWallet')}
        </Button>
      </Container>
    )
  }

  const typeConfig = getTransactionTypeConfig(transaction.type)
  const statusConfig = getTransactionStatusConfig(transaction.status)
  const isPositive = ['deposit', 'refund', 'escrow_release'].includes(transaction.type)

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
              {t('transaction.backToWallet')}
            </Button>
          </motion.div>

          <motion.div variants={staggerItem}>
            <Card sx={{ borderRadius: 3, overflow: 'hidden' }}>
              <Box
                sx={{
                  p: 4,
                  background: `linear-gradient(135deg, ${typeConfig.bgColor} 0%, ${palette.neutral[0]} 100%)`,
                  borderBottom: `1px solid ${palette.neutral[200]}`,
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
                    <Typography variant="h5" fontWeight={700} color={palette.neutral[900]}>
                      {typeConfig.label}
                    </Typography>
                    <Stack direction="row" alignItems="center" spacing={1} sx={{ mt: 0.5 }}>
                      <Typography variant="body2" color="text.secondary">
                        {t('transaction.id', { id: transactionId?.slice(0, 12) })}
                      </Typography>
                      <Button
                        size="small"
                        onClick={handleCopyId}
                        aria-label={t('transaction.copy')}
                        sx={{ minWidth: 'auto', p: 0.5, color: palette.neutral[500] }}
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
                      color: isPositive ? palette.semantic.success : palette.semantic.error,
                    }}
                  >
                    {isPositive ? '+' : '-'}
                    {formatCurrency(transaction.amount)}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                    {t('transaction.balanceAfter', {
                      amount: formatCurrency(transaction.balance),
                    })}
                  </Typography>
                </Box>

                <Divider sx={{ my: 3 }} />

                <Grid container spacing={3}>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <DetailItem
                      icon={<AccessTime sx={{ color: palette.neutral[500] }} />}
                      label={t('transaction.dateTime')}
                      value={formatDateLong(transaction.createdAt)}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <DetailItem
                      icon={<AccountBalanceWallet sx={{ color: palette.neutral[500] }} />}
                      label={t('transaction.walletBalance')}
                      value={formatCurrency(transaction.balance)}
                    />
                  </Grid>
                  {transaction.description && (
                    <Grid size={{ xs: 12 }}>
                      <DetailItem
                        icon={<Receipt sx={{ color: palette.neutral[500] }} />}
                        label={t('transaction.description')}
                        value={transaction.description}
                      />
                    </Grid>
                  )}
                  {transaction.reference && (
                    <Grid size={{ xs: 12 }}>
                      <DetailItem
                        icon={<Receipt sx={{ color: palette.neutral[500] }} />}
                        label={t('transaction.reference')}
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
                  {t('transaction.relatedOrder')}
                </Typography>
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    p: 2,
                    bgcolor: palette.neutral[100],
                    borderRadius: 1,
                  }}
                >
                  <Typography variant="body2">
                    {t('transaction.orderNumber', {
                      number: transaction.relatedOrderId.slice(0, 8).toUpperCase(),
                    })}
                  </Typography>
                  <Button
                    component={Link}
                    to={`/orders/${transaction.relatedOrderId}`}
                    size="small"
                    sx={{ color: palette.brand.primary, fontWeight: 600 }}
                  >
                    {t('transaction.viewOrder')}
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
}: Readonly<{
  icon: React.ReactNode
  label: string
  value: string
}>) {
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
