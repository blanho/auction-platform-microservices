import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Box,
  Container,
  Typography,
  Card,
  Grid,
  Button,
  Tabs,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  CircularProgress,
  Alert,
  Skeleton,
  IconButton,
  Menu,
  MenuItem,
  InputAdornment,
  Select,
  FormControl,
  InputLabel,
  Pagination,
} from '@mui/material'
import {
  AccountBalanceWallet,
  ArrowUpward,
  ArrowDownward,
  Add,
  CreditCard,
  AccountBalance,
  TrendingUp,
  History,
  FilterList,
} from '@mui/icons-material'
import { useWallet, useTransactions, useDeposit, useWithdraw, usePaymentMethods } from '../hooks'
import type { TransactionFilters, TransactionType, TransactionStatus } from '../types'

const formatCurrency = (amount: number, currency = 'USD') => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency,
  }).format(amount)
}

const getTransactionIcon = (type: TransactionType) => {
  switch (type) {
    case 'deposit':
      return <ArrowDownward sx={{ color: '#22C55E' }} />
    case 'withdrawal':
      return <ArrowUpward sx={{ color: '#EF4444' }} />
    case 'payment':
      return <ArrowUpward sx={{ color: '#EF4444' }} />
    case 'refund':
      return <ArrowDownward sx={{ color: '#22C55E' }} />
    default:
      return <History sx={{ color: '#78716C' }} />
  }
}

const getStatusChip = (status: TransactionStatus) => {
  const config: Record<TransactionStatus, { color: 'success' | 'warning' | 'error' | 'default'; label: string }> = {
    completed: { color: 'success', label: 'Completed' },
    pending: { color: 'warning', label: 'Pending' },
    failed: { color: 'error', label: 'Failed' },
    cancelled: { color: 'default', label: 'Cancelled' },
  }
  const { color, label } = config[status] || { color: 'default', label: status }
  return <Chip size="small" color={color} label={label} />
}

export function WalletPage() {
  const navigate = useNavigate()
  const [activeTab, setActiveTab] = useState(0)
  const [showDepositDialog, setShowDepositDialog] = useState(false)
  const [showWithdrawDialog, setShowWithdrawDialog] = useState(false)
  const [depositAmount, setDepositAmount] = useState('')
  const [withdrawAmount, setWithdrawAmount] = useState('')
  const [selectedPaymentMethod, setSelectedPaymentMethod] = useState('')
  const [filters, setFilters] = useState<TransactionFilters>({ page: 1, pageSize: 10 })
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)

  const { data: wallet, isLoading: walletLoading } = useWallet()
  const { data: transactions, isLoading: transactionsLoading } = useTransactions(filters)
  const { data: paymentMethods } = usePaymentMethods()
  const deposit = useDeposit()
  const withdraw = useWithdraw()

  const handleDeposit = async () => {
    if (!depositAmount || !selectedPaymentMethod) return
    try {
      await deposit.mutateAsync({
        amount: parseFloat(depositAmount),
        paymentMethod: selectedPaymentMethod,
      })
      setShowDepositDialog(false)
      setDepositAmount('')
      setSelectedPaymentMethod('')
    } catch {
      /* Error handled by mutation */
    }
  }

  const handleWithdraw = async () => {
    if (!withdrawAmount || !selectedPaymentMethod) return
    try {
      await withdraw.mutateAsync({
        amount: parseFloat(withdrawAmount),
        paymentMethod: selectedPaymentMethod,
      })
      setShowWithdrawDialog(false)
      setWithdrawAmount('')
      setSelectedPaymentMethod('')
    } catch {
      /* Error handled by mutation */
    }
  }

  if (walletLoading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
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
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography
          variant="h4"
          sx={{
            fontFamily: '"Playfair Display", serif',
            fontWeight: 600,
            color: '#1C1917',
          }}
        >
          My Wallet
        </Typography>
        <Typography sx={{ color: '#78716C' }}>
          Manage your balance and transactions
        </Typography>
      </Box>

      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid size={{ xs: 12, md: 4 }}>
          <Card
            sx={{
              p: 3,
              borderRadius: 2,
              boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
              background: 'linear-gradient(135deg, #1C1917 0%, #44403C 100%)',
              color: 'white',
            }}
          >
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <AccountBalanceWallet />
              <Typography sx={{ fontSize: '0.875rem', opacity: 0.8 }}>
                Available Balance
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
              <TrendingUp sx={{ color: '#CA8A04' }} />
              <Typography sx={{ fontSize: '0.875rem', color: '#78716C' }}>
                Total Balance
              </Typography>
            </Box>
            <Typography variant="h4" sx={{ fontWeight: 700, color: '#1C1917' }}>
              {formatCurrency(wallet?.balance || 0, wallet?.currency)}
            </Typography>
            <Typography sx={{ fontSize: '0.875rem', color: '#78716C', mt: 1 }}>
              Includes held funds
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
              <History sx={{ color: '#CA8A04' }} />
              <Typography sx={{ fontSize: '0.875rem', color: '#78716C' }}>
                Held Amount
              </Typography>
            </Box>
            <Typography variant="h4" sx={{ fontWeight: 700, color: '#1C1917' }}>
              {formatCurrency(wallet?.heldAmount || 0, wallet?.currency)}
            </Typography>
            <Typography sx={{ fontSize: '0.875rem', color: '#78716C', mt: 1 }}>
              Pending transactions
            </Typography>
          </Card>
        </Grid>
      </Grid>

      <Box sx={{ display: 'flex', gap: 2, mb: 4, flexWrap: 'wrap' }}>
        <Button
          variant="contained"
          startIcon={<Add />}
          onClick={() => setShowDepositDialog(true)}
          sx={{
            bgcolor: '#CA8A04',
            textTransform: 'none',
            fontWeight: 600,
            px: 3,
            '&:hover': { bgcolor: '#A16207' },
          }}
        >
          Add Funds
        </Button>
        <Button
          variant="outlined"
          startIcon={<ArrowUpward />}
          onClick={() => setShowWithdrawDialog(true)}
          disabled={(wallet?.availableBalance || 0) <= 0}
          sx={{
            borderColor: '#1C1917',
            color: '#1C1917',
            textTransform: 'none',
            fontWeight: 600,
            px: 3,
            '&:hover': { borderColor: '#44403C', bgcolor: '#FAFAF9' },
          }}
        >
          Withdraw
        </Button>
        <Button
          variant="outlined"
          startIcon={<CreditCard />}
          onClick={() => navigate('/wallet/payment-methods')}
          sx={{
            borderColor: '#78716C',
            color: '#78716C',
            textTransform: 'none',
            fontWeight: 600,
            px: 3,
            '&:hover': { borderColor: '#44403C', bgcolor: '#FAFAF9' },
          }}
        >
          Payment Methods
        </Button>
      </Box>

      <Card
        sx={{
          borderRadius: 2,
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
        }}
      >
        <Box sx={{ borderBottom: '1px solid #F5F5F5' }}>
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
                color: '#CA8A04',
              },
              '& .MuiTabs-indicator': {
                bgcolor: '#CA8A04',
              },
            }}
          >
            <Tab label="All Transactions" />
            <Tab label="Deposits" />
            <Tab label="Withdrawals" />
          </Tabs>
        </Box>

        <Box sx={{ p: 2, display: 'flex', justifyContent: 'flex-end' }}>
          <IconButton onClick={(e) => setAnchorEl(e.currentTarget)}>
            <FilterList />
          </IconButton>
          <Menu
            anchorEl={anchorEl}
            open={Boolean(anchorEl)}
            onClose={() => setAnchorEl(null)}
          >
            <MenuItem onClick={() => { setFilters({ ...filters, status: undefined }); setAnchorEl(null) }}>
              All Status
            </MenuItem>
            <MenuItem onClick={() => { setFilters({ ...filters, status: 'completed' }); setAnchorEl(null) }}>
              Completed
            </MenuItem>
            <MenuItem onClick={() => { setFilters({ ...filters, status: 'pending' }); setAnchorEl(null) }}>
              Pending
            </MenuItem>
          </Menu>
        </Box>

        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600 }}>Transaction</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Date</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Amount</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Balance</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {transactionsLoading ? (
                Array.from({ length: 5 }).map((_, i) => (
                  <TableRow key={i}>
                    <TableCell><Skeleton /></TableCell>
                    <TableCell><Skeleton /></TableCell>
                    <TableCell><Skeleton /></TableCell>
                    <TableCell><Skeleton /></TableCell>
                    <TableCell><Skeleton /></TableCell>
                  </TableRow>
                ))
              ) : transactions?.items?.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={5} align="center" sx={{ py: 6 }}>
                    <Typography sx={{ color: '#78716C' }}>No transactions yet</Typography>
                  </TableCell>
                </TableRow>
              ) : (
                transactions?.items?.filter((t) => {
                  if (activeTab === 1) return t.type === 'deposit'
                  if (activeTab === 2) return t.type === 'withdrawal'
                  return true
                }).map((transaction) => (
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
                          <Typography sx={{ fontWeight: 500, color: '#1C1917', textTransform: 'capitalize' }}>
                            {transaction.type.replace('_', ' ')}
                          </Typography>
                          <Typography sx={{ fontSize: '0.8125rem', color: '#78716C' }}>
                            {transaction.description || `${transaction.type.replace('_', ' ')} transaction`}
                          </Typography>
                        </Box>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Typography sx={{ color: '#44403C' }}>
                        {new Date(transaction.createdAt).toLocaleDateString()}
                      </Typography>
                      <Typography sx={{ fontSize: '0.8125rem', color: '#78716C' }}>
                        {new Date(transaction.createdAt).toLocaleTimeString()}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography
                        sx={{
                          fontWeight: 600,
                          color: ['deposit', 'refund', 'release', 'escrow_release'].includes(transaction.type) ? '#22C55E' : '#EF4444',
                        }}
                      >
                        {['deposit', 'refund', 'release', 'escrow_release'].includes(transaction.type) ? '+' : '-'}
                        {formatCurrency(transaction.amount, wallet?.currency)}
                      </Typography>
                    </TableCell>
                    <TableCell>{getStatusChip(transaction.status)}</TableCell>
                    <TableCell>
                      <Typography sx={{ color: '#44403C' }}>
                        {formatCurrency(transaction.balance, wallet?.currency)}
                      </Typography>
                    </TableCell>
                  </TableRow>
                ))
              )}
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

      <Dialog open={showDepositDialog} onClose={() => setShowDepositDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 600 }}>Add Funds</DialogTitle>
        <DialogContent>
          {deposit.isError && (
            <Alert severity="error" sx={{ mb: 2 }}>
              Failed to process deposit. Please try again.
            </Alert>
          )}

          <TextField
            fullWidth
            label="Amount"
            type="number"
            value={depositAmount}
            onChange={(e) => setDepositAmount(e.target.value)}
            InputProps={{
              startAdornment: <InputAdornment position="start">$</InputAdornment>,
            }}
            sx={{ mb: 2.5, mt: 1 }}
          />

          <FormControl fullWidth>
            <InputLabel>Payment Method</InputLabel>
            <Select
              value={selectedPaymentMethod}
              onChange={(e) => setSelectedPaymentMethod(e.target.value)}
              label="Payment Method"
            >
              {paymentMethods?.map((method) => (
                <MenuItem key={method.id} value={method.id}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    {method.type === 'card' ? <CreditCard /> : <AccountBalance />}
                    {method.type === 'card'
                      ? `${method.brand} •••• ${method.last4}`
                      : `Bank •••• ${method.last4}`}
                  </Box>
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button onClick={() => setShowDepositDialog(false)} sx={{ color: '#78716C', textTransform: 'none' }}>
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={handleDeposit}
            disabled={!depositAmount || !selectedPaymentMethod || deposit.isPending}
            sx={{
              bgcolor: '#CA8A04',
              textTransform: 'none',
              '&:hover': { bgcolor: '#A16207' },
            }}
          >
            {deposit.isPending ? <CircularProgress size={20} color="inherit" /> : 'Add Funds'}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={showWithdrawDialog} onClose={() => setShowWithdrawDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 600 }}>Withdraw Funds</DialogTitle>
        <DialogContent>
          {withdraw.isError && (
            <Alert severity="error" sx={{ mb: 2 }}>
              Failed to process withdrawal. Please try again.
            </Alert>
          )}

          <Alert severity="info" sx={{ mb: 2, mt: 1 }}>
            Available balance: {formatCurrency(wallet?.availableBalance || 0, wallet?.currency)}
          </Alert>

          <TextField
            fullWidth
            label="Amount"
            type="number"
            value={withdrawAmount}
            onChange={(e) => setWithdrawAmount(e.target.value)}
            InputProps={{
              startAdornment: <InputAdornment position="start">$</InputAdornment>,
            }}
            sx={{ mb: 2.5 }}
          />

          <FormControl fullWidth>
            <InputLabel>Withdraw To</InputLabel>
            <Select
              value={selectedPaymentMethod}
              onChange={(e) => setSelectedPaymentMethod(e.target.value)}
              label="Withdraw To"
            >
              {paymentMethods?.filter((m) => m.type === 'bank_account').map((method) => (
                <MenuItem key={method.id} value={method.id}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <AccountBalance />
                    Bank •••• {method.last4}
                  </Box>
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button onClick={() => setShowWithdrawDialog(false)} sx={{ color: '#78716C', textTransform: 'none' }}>
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={handleWithdraw}
            disabled={
              !withdrawAmount ||
              !selectedPaymentMethod ||
              withdraw.isPending ||
              parseFloat(withdrawAmount) > (wallet?.availableBalance || 0)
            }
            sx={{
              bgcolor: '#1C1917',
              textTransform: 'none',
              '&:hover': { bgcolor: '#44403C' },
            }}
          >
            {withdraw.isPending ? <CircularProgress size={20} color="inherit" /> : 'Withdraw'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}
