import { getCurrentLocale } from '@/i18n'
import { palette } from '@/shared/theme/tokens'
import { InlineAlert, StatusBadge, TableEmptyStateRow, TableSkeletonRows } from '@/shared/ui'
import { formatCurrency } from '@/shared/utils/formatters'
import { AccountBalanceWallet, FilterList, History, TrendingUp } from '@mui/icons-material'
import {
  Box,
  Card,
  Container,
  Grid,
  IconButton,
  Menu,
  MenuItem,
  Pagination,
  Skeleton,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tabs,
  Typography,
} from '@mui/material'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { useTransactions, useWallet } from '../hooks'
import type { TransactionFilters } from '../types'
import { getTransactionIcon } from '../utils'

export function WalletPage() {
  const { t } = useTranslation('payments')
  const navigate = useNavigate()
  const [activeTab, setActiveTab] = useState(0)
  const [filters, setFilters] = useState<TransactionFilters>({ page: 1, pageSize: 10 })
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)

  const { data: wallet, isLoading: walletLoading } = useWallet()
  const { data: transactions, isLoading: transactionsLoading } = useTransactions(filters)

  if (walletLoading) {
    return (
      <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 4 }}>
            <Skeleton variant="rectangular" height={200} sx={{ borderRadius: 2 }} />
          </Grid>
          <Grid size={{ xs: 12, md: 4 }}>
            <Skeleton variant="rectangular" height={200} sx={{ borderRadius: 2 }} />
          </Grid>
          <Grid size={{ xs: 12, md: 4 }}>
            <Skeleton variant="rectangular" height={200} sx={{ borderRadius: 2 }} />
          </Grid>
        </Grid>
      </Container>
    )
  }

  return (
    <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
      <Box sx={{ mb: 4 }}>
        <Typography
          variant="h4"
          sx={{
            fontFamily: '"Playfair Display", serif',
            fontWeight: 600,
            color: palette.neutral[900],
          }}
        >
          {t('wallet.title')}
        </Typography>
        <Typography sx={{ color: palette.neutral[500] }}>{t('wallet.description')}</Typography>
      </Box>

      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid size={{ xs: 12, md: 4 }}>
          <Card
            sx={{
              p: 3,
              borderRadius: 2,
              boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
              background: `linear-gradient(135deg, ${palette.neutral[900]} 0%, ${palette.neutral[700]} 100%)`,
              color: 'white',
            }}
          >
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <AccountBalanceWallet />
              <Typography sx={{ fontSize: '0.875rem', opacity: 0.8 }}>
                {t('wallet.availableBalance')}
              </Typography>
            </Box>
            <Typography variant="h3" sx={{ fontWeight: 700, mb: 1 }}>
              {formatCurrency(wallet?.availableBalance || 0, wallet?.currency)}
            </Typography>
            <Typography sx={{ fontSize: '0.875rem', opacity: 0.7 }}>
              {wallet?.currency || 'USD'}
            </Typography>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          <Card
            sx={{
              p: 3,
              borderRadius: 2,
              boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
            }}
          >
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <TrendingUp sx={{ color: palette.brand.primary }} />
              <Typography sx={{ fontSize: '0.875rem', color: palette.neutral[500] }}>
                {t('wallet.totalBalance')}
              </Typography>
            </Box>
            <Typography variant="h4" sx={{ fontWeight: 700, color: palette.neutral[900] }}>
              {formatCurrency(wallet?.balance || 0, wallet?.currency)}
            </Typography>
            <Typography sx={{ fontSize: '0.875rem', color: palette.neutral[500], mt: 1 }}>
              {t('wallet.includesHeldFunds')}
            </Typography>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          <Card
            sx={{
              p: 3,
              borderRadius: 2,
              boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
            }}
          >
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <History sx={{ color: palette.brand.primary }} />
              <Typography sx={{ fontSize: '0.875rem', color: palette.neutral[500] }}>
                {t('wallet.heldAmount')}
              </Typography>
            </Box>
            <Typography variant="h4" sx={{ fontWeight: 700, color: palette.neutral[900] }}>
              {formatCurrency(wallet?.heldAmount || 0, wallet?.currency)}
            </Typography>
            <Typography sx={{ fontSize: '0.875rem', color: palette.neutral[500], mt: 1 }}>
              {t('wallet.pendingTransactions')}
            </Typography>
          </Card>
        </Grid>
      </Grid>

      <InlineAlert severity="info" sx={{ mb: 4 }}>
        {t('wallet.fundingUnavailable')}
      </InlineAlert>

      <Card
        sx={{
          borderRadius: 2,
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
        }}
      >
        <Box sx={{ borderBottom: `1px solid ${palette.neutral[100]}` }}>
          <Tabs
            value={activeTab}
            onChange={(_, v) => setActiveTab(v)}
            sx={{
              px: 2,
              '& .MuiTab-root': {
                textTransform: 'none',
                fontWeight: 500,
              },
              '& .Mui-selected': {
                color: palette.brand.primary,
              },
              '& .MuiTabs-indicator': {
                bgcolor: palette.brand.primary,
              },
            }}
          >
            <Tab label={t('wallet.allTransactions')} />
            <Tab label={t('wallet.deposits')} />
            <Tab label={t('wallet.withdrawals')} />
          </Tabs>
        </Box>

        <Box sx={{ p: 2, display: 'flex', justifyContent: 'flex-end' }}>
          <IconButton onClick={(e) => setAnchorEl(e.currentTarget)}>
            <FilterList />
          </IconButton>
          <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={() => setAnchorEl(null)}>
            <MenuItem
              onClick={() => {
                setFilters({ ...filters, status: undefined })
                setAnchorEl(null)
              }}
            >
              {t('wallet.allStatuses')}
            </MenuItem>
            <MenuItem
              onClick={() => {
                setFilters({ ...filters, status: 'completed' })
                setAnchorEl(null)
              }}
            >
              {t('status.completed')}
            </MenuItem>
            <MenuItem
              onClick={() => {
                setFilters({ ...filters, status: 'pending' })
                setAnchorEl(null)
              }}
            >
              {t('status.pending')}
            </MenuItem>
          </Menu>
        </Box>

        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600 }}>{t('wallet.transaction')}</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>{t('wallet.date')}</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>{t('wallet.amount')}</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>{t('wallet.status')}</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>{t('wallet.balance')}</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {transactionsLoading && <TableSkeletonRows rows={5} columns={5} />}
              {!transactionsLoading && transactions?.items?.length === 0 && (
                <TableEmptyStateRow
                  colSpan={5}
                  title={t('wallet.noTransactions')}
                  cellSx={{ py: 6 }}
                />
              )}
              {!transactionsLoading &&
                transactions?.items?.length !== 0 &&
                transactions?.items
                  ?.filter((t) => {
                    if (activeTab === 1) {
                      return t.type === 'deposit'
                    }
                    if (activeTab === 2) {
                      return t.type === 'withdrawal'
                    }
                    return true
                  })
                  .map((transaction) => (
                    <TableRow
                      key={transaction.id}
                      hover
                      onClick={() => navigate(`/wallet/transactions/${transaction.id}`)}
                      sx={{ cursor: 'pointer' }}
                    >
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                          {getTransactionIcon(transaction.type)}
                          <Box>
                            <Typography
                              sx={{
                                fontWeight: 500,
                                color: palette.neutral[900],
                                textTransform: 'capitalize',
                              }}
                            >
                              {t(`transactionTypes.${transaction.type}`)}
                            </Typography>
                            <Typography sx={{ fontSize: '0.8125rem', color: palette.neutral[500] }}>
                              {transaction.description ||
                                t('wallet.transactionDescription', {
                                  type: t(`transactionTypes.${transaction.type}`),
                                })}
                            </Typography>
                          </Box>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Typography sx={{ color: palette.neutral[700] }}>
                          {new Date(transaction.createdAt).toLocaleDateString(getCurrentLocale())}
                        </Typography>
                        <Typography sx={{ fontSize: '0.8125rem', color: palette.neutral[500] }}>
                          {new Date(transaction.createdAt).toLocaleTimeString(getCurrentLocale())}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography
                          sx={{
                            fontWeight: 600,
                            color: ['deposit', 'refund', 'release', 'escrow_release'].includes(
                              transaction.type
                            )
                              ? palette.semantic.success
                              : palette.semantic.error,
                          }}
                        >
                          {['deposit', 'refund', 'release', 'escrow_release'].includes(
                            transaction.type
                          )
                            ? '+'
                            : '-'}
                          {formatCurrency(transaction.amount, wallet?.currency)}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <StatusBadge
                          status={transaction.status}
                          label={t(`status.${transaction.status}`)}
                        />
                      </TableCell>
                      <TableCell>
                        <Typography sx={{ color: palette.neutral[700] }}>
                          {formatCurrency(transaction.balance, wallet?.currency)}
                        </Typography>
                      </TableCell>
                    </TableRow>
                  ))}
            </TableBody>
          </Table>
        </TableContainer>

        {transactions && transactions.totalPages > 1 && (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
            <Pagination
              count={transactions.totalPages}
              page={filters.page || 1}
              onChange={(_, page) => setFilters({ ...filters, page })}
              color="primary"
            />
          </Box>
        )}
      </Card>
    </Container>
  )
}
