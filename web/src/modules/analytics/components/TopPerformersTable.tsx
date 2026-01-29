import {
  Box,
  Typography,
  Stack,
  Skeleton,
  Avatar,
  Chip,
  useTheme,
  alpha,
  Tab,
  Tabs,
} from '@mui/material'
import { useState } from 'react'
import { EmojiEvents, TrendingUp, Gavel, ShoppingCart } from '@mui/icons-material'
import { motion } from 'framer-motion'
import { useTopPerformers } from '../hooks/useAnalytics'
import { formatCurrency, formatNumber } from '@/shared/utils/formatters'
import { palette } from '@/shared/theme/tokens'

interface TabPanelProps {
  children?: React.ReactNode
  index: number
  value: number
}

function TabPanel({ children, value, index }: TabPanelProps) {
  return (
    <Box role="tabpanel" hidden={value !== index} sx={{ pt: 2 }}>
      {value === index && children}
    </Box>
  )
}

interface PerformerRowProps {
  rank: number
  name: string
  avatar?: string
  primaryValue: string
  secondaryValue?: string
  badge?: string
}

function PerformerRow({
  rank,
  name,
  avatar,
  primaryValue,
  secondaryValue,
  badge,
}: PerformerRowProps) {
  const theme = useTheme()
  const getRankColor = () => {
    switch (rank) {
      case 1:
        return palette.brand.secondary
      case 2:
        return palette.neutral[400]
      case 3:
        return '#CD7F32'
      default:
        return theme.palette.text.secondary
    }
  }

  return (
    <Box
      component={motion.div}
      initial={{ opacity: 0, x: -20 }}
      animate={{ opacity: 1, x: 0 }}
      transition={{ delay: rank * 0.05 }}
      sx={{
        display: 'flex',
        alignItems: 'center',
        gap: 2,
        p: 1.5,
        borderRadius: 1.5,
        bgcolor: rank <= 3 ? alpha(getRankColor(), 0.08) : 'transparent',
        border: rank <= 3 ? '1px solid' : 'none',
        borderColor: alpha(getRankColor(), 0.2),
        transition: 'all 0.2s',
        '&:hover': {
          bgcolor: theme.palette.action.hover,
        },
      }}
    >
      <Box
        sx={{
          width: 28,
          height: 28,
          borderRadius: '50%',
          bgcolor:
            rank <= 3 ? alpha(getRankColor(), 0.2) : alpha(theme.palette.text.secondary, 0.1),
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          color: getRankColor(),
          fontWeight: 700,
          fontSize: '0.875rem',
        }}
      >
        {rank <= 3 ? <EmojiEvents sx={{ fontSize: 16 }} /> : rank}
      </Box>
      <Avatar
        src={avatar}
        sx={{
          width: 36,
          height: 36,
          fontSize: '0.875rem',
          bgcolor: alpha(theme.palette.primary.main, 0.2),
          color: theme.palette.primary.main,
        }}
      >
        {name.charAt(0).toUpperCase()}
      </Avatar>
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <Stack direction="row" alignItems="center" spacing={1}>
          <Typography variant="body2" fontWeight={600} noWrap sx={{ maxWidth: 150 }}>
            {name}
          </Typography>
          {badge && (
            <Chip
              label={badge}
              size="small"
              sx={{
                height: 20,
                fontSize: '0.625rem',
                bgcolor: alpha(theme.palette.primary.main, 0.1),
                color: theme.palette.primary.main,
              }}
            />
          )}
        </Stack>
        {secondaryValue && (
          <Typography variant="caption" color="text.secondary">
            {secondaryValue}
          </Typography>
        )}
      </Box>
      <Typography variant="body2" fontWeight={600} sx={{ color: theme.palette.success.main }}>
        {primaryValue}
      </Typography>
    </Box>
  )
}

export function TopPerformersTable() {
  const [tabValue, setTabValue] = useState(0)
  const { data: performers, isLoading } = useTopPerformers()

  if (isLoading) {
    return (
      <Box
        sx={{
          p: 3,
          borderRadius: 2,
          bgcolor: 'background.paper',
          boxShadow: '0 1px 3px rgba(0,0,0,0.08)',
          border: '1px solid',
          borderColor: 'divider',
        }}
      >
        <Skeleton variant="text" width={180} height={32} sx={{ mb: 2 }} />
        <Skeleton variant="rectangular" height={40} sx={{ mb: 2 }} />
        <Stack spacing={1}>
          {[1, 2, 3, 4, 5].map((i) => (
            <Skeleton key={i} variant="rounded" height={56} />
          ))}
        </Stack>
      </Box>
    )
  }

  const topSellers = performers?.topSellers ?? []
  const topBuyers = performers?.topBuyers ?? []
  const topAuctions = performers?.topAuctions ?? []

  return (
    <Box
      sx={{
        p: 3,
        borderRadius: 2,
        bgcolor: 'background.paper',
        boxShadow: '0 1px 3px rgba(0,0,0,0.08)',
        border: '1px solid',
        borderColor: 'divider',
      }}
    >
      <Typography
        variant="h6"
        fontWeight={600}
        sx={{ mb: 2, fontFamily: '"Fira Sans", sans-serif' }}
      >
        Top Performers
      </Typography>

      <Tabs
        value={tabValue}
        onChange={(_, newValue) => setTabValue(newValue)}
        sx={{
          minHeight: 40,
          '& .MuiTab-root': {
            minHeight: 40,
            textTransform: 'none',
            fontSize: '0.875rem',
          },
        }}
      >
        <Tab icon={<TrendingUp sx={{ fontSize: 18 }} />} iconPosition="start" label="Top Sellers" />
        <Tab
          icon={<ShoppingCart sx={{ fontSize: 18 }} />}
          iconPosition="start"
          label="Top Buyers"
        />
        <Tab icon={<Gavel sx={{ fontSize: 18 }} />} iconPosition="start" label="Hot Auctions" />
      </Tabs>

      <TabPanel value={tabValue} index={0}>
        <Stack spacing={0.5}>
          {topSellers.length === 0 ? (
            <Typography variant="body2" color="text.secondary" sx={{ py: 3, textAlign: 'center' }}>
              No seller data available
            </Typography>
          ) : (
            topSellers.map((seller, index) => (
              <PerformerRow
                key={seller.sellerId ?? index}
                rank={index + 1}
                name={seller.username ?? `Seller ${index + 1}`}
                primaryValue={formatCurrency(seller.totalSales ?? 0)}
                secondaryValue={`${formatNumber(seller.orderCount ?? 0)} orders`}
                badge={index === 0 ? 'Top Seller' : undefined}
              />
            ))
          )}
        </Stack>
      </TabPanel>

      <TabPanel value={tabValue} index={1}>
        <Stack spacing={0.5}>
          {topBuyers.length === 0 ? (
            <Typography variant="body2" color="text.secondary" sx={{ py: 3, textAlign: 'center' }}>
              No buyer data available
            </Typography>
          ) : (
            topBuyers.map((buyer, index) => (
              <PerformerRow
                key={buyer.buyerId ?? index}
                rank={index + 1}
                name={buyer.username ?? `Buyer ${index + 1}`}
                primaryValue={formatCurrency(buyer.totalSpent ?? 0)}
                secondaryValue={`${formatNumber(buyer.auctionsWon ?? 0)} auctions won`}
                badge={index === 0 ? 'Top Buyer' : undefined}
              />
            ))
          )}
        </Stack>
      </TabPanel>

      <TabPanel value={tabValue} index={2}>
        <Stack spacing={0.5}>
          {topAuctions.length === 0 ? (
            <Typography variant="body2" color="text.secondary" sx={{ py: 3, textAlign: 'center' }}>
              No auction data available
            </Typography>
          ) : (
            topAuctions.map((auction, index) => (
              <PerformerRow
                key={auction.auctionId ?? index}
                rank={index + 1}
                name={auction.title ?? `Auction ${index + 1}`}
                primaryValue={formatCurrency(auction.finalPrice ?? 0)}
                secondaryValue={`${formatNumber(auction.bidCount ?? 0)} bids`}
                badge={index === 0 ? 'Most Popular' : undefined}
              />
            ))
          )}
        </Stack>
      </TabPanel>
    </Box>
  )
}
