import { useState } from 'react'
import {
  Box,
  Tabs,
  Tab,
  Typography,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableRow,
  Divider,
  Skeleton,
  Chip,
  Avatar,
} from '@mui/material'
import { Description, Gavel, LocalShipping, Policy, Star } from '@mui/icons-material'
import { palette } from '@/shared/theme/tokens'
import type { BidSummary } from '../types'
import { ReviewsList } from './ReviewsList'
import type { Review, UserRatingSummary } from '@/modules/users/api/reviews.api'
import { formatTimeAgo } from '../utils'

interface TabPanelProps {
  children?: React.ReactNode
  index: number
  value: number
}

function TabPanel({ children, value, index }: TabPanelProps) {
  return (
    <Box
      role="tabpanel"
      hidden={value !== index}
      id={`auction-tabpanel-${index}`}
      aria-labelledby={`auction-tab-${index}`}
      sx={{ py: 3 }}
    >
      {value === index && children}
    </Box>
  )
}

interface ProductTabsProps {
  description: string
  bids: BidSummary[]
  reviews?: Review[]
  ratingSummary?: UserRatingSummary
  reviewsLoading?: boolean
  specifications?: Record<string, string>
  shippingInfo?: {
    method: string
    cost: number | 'free'
    estimatedDays: string
    locations: string[]
  }
  returnPolicy?: {
    accepted: boolean
    period: number
    conditions: string
  }
}

export function ProductTabs({
  description,
  bids,
  reviews = [],
  ratingSummary,
  reviewsLoading,
  specifications,
  shippingInfo,
  returnPolicy,
}: ProductTabsProps) {
  const [activeTab, setActiveTab] = useState(0)

  const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue)
  }

  return (
    <Box
      sx={{
        bgcolor: 'white',
        borderRadius: 2,
        border: `1px solid ${palette.neutral[100]}`,
        overflow: 'hidden',
      }}
    >
      <Box sx={{ borderBottom: `1px solid ${palette.neutral[100]}` }}>
        <Tabs
          value={activeTab}
          onChange={handleTabChange}
          variant="scrollable"
          scrollButtons="auto"
          sx={{
            px: 2,
            '& .MuiTab-root': {
              textTransform: 'none',
              fontWeight: 500,
              fontSize: '0.9375rem',
              color: palette.neutral[500],
              minHeight: 56,
              '&.Mui-selected': {
                color: palette.neutral[900],
              },
            },
            '& .MuiTabs-indicator': {
              bgcolor: palette.neutral[900],
              height: 2,
            },
          }}
        >
          <Tab icon={<Description fontSize="small" />} iconPosition="start" label="Description" />
          <Tab
            icon={<Gavel fontSize="small" />}
            iconPosition="start"
            label={`Bid History (${bids.length})`}
          />
          <Tab
            icon={<Star fontSize="small" />}
            iconPosition="start"
            label={`Reviews (${reviews.length})`}
          />
          <Tab icon={<LocalShipping fontSize="small" />} iconPosition="start" label="Shipping" />
          <Tab icon={<Policy fontSize="small" />} iconPosition="start" label="Returns" />
        </Tabs>
      </Box>

      <Box sx={{ px: 3 }}>
        <TabPanel value={activeTab} index={0}>
          <Typography
            variant="body1"
            sx={{
              color: palette.neutral[700],
              lineHeight: 1.8,
              whiteSpace: 'pre-line',
            }}
          >
            {description}
          </Typography>

          {specifications && Object.keys(specifications).length > 0 && (
            <>
              <Divider sx={{ my: 3 }} />
              <Typography
                variant="h6"
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontWeight: 600,
                  color: palette.neutral[900],
                  mb: 2,
                }}
              >
                Specifications
              </Typography>
              <Table size="small">
                <TableBody>
                  {Object.entries(specifications).map(([key, value]) => (
                    <TableRow key={key} sx={{ '&:last-child td': { borderBottom: 0 } }}>
                      <TableCell
                        sx={{
                          fontWeight: 500,
                          color: palette.neutral[500],
                          width: '40%',
                          borderColor: palette.neutral[100],
                          py: 1.5,
                        }}
                      >
                        {key}
                      </TableCell>
                      <TableCell
                        sx={{
                          color: palette.neutral[900],
                          borderColor: palette.neutral[100],
                          py: 1.5,
                        }}
                      >
                        {value}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </>
          )}
        </TabPanel>

        <TabPanel value={activeTab} index={1}>
          {bids.length === 0 ? (
            <Box sx={{ textAlign: 'center', py: 4 }}>
              <Typography variant="body1" sx={{ color: palette.neutral[500] }}>
                No bids have been placed yet. Be the first to bid!
              </Typography>
            </Box>
          ) : (
            <Stack spacing={0}>
              {bids.map((bid, index) => (
                <Box
                  key={bid.id}
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    py: 2,
                    borderBottom:
                      index < bids.length - 1 ? `1px solid ${palette.neutral[100]}` : 'none',
                  }}
                >
                  <Stack direction="row" alignItems="center" spacing={2}>
                    <Avatar
                      sx={{
                        width: 36,
                        height: 36,
                        bgcolor: index === 0 ? palette.brand.primary : palette.neutral[100],
                        fontSize: '0.875rem',
                      }}
                    >
                      {bid.bidderName.charAt(0).toUpperCase()}
                    </Avatar>
                    <Box>
                      <Stack direction="row" alignItems="center" spacing={1}>
                        <Typography
                          sx={{
                            fontWeight: 500,
                            color: palette.neutral[900],
                            fontSize: '0.9375rem',
                          }}
                        >
                          {bid.bidderName}
                        </Typography>
                        {index === 0 && (
                          <Chip
                            label="Highest"
                            size="small"
                            sx={{
                              height: 20,
                              fontSize: '0.6875rem',
                              bgcolor: palette.brand.muted,
                              color: palette.brand.dark,
                            }}
                          />
                        )}
                      </Stack>
                      <Typography
                        variant="body2"
                        sx={{ color: palette.neutral[500], fontSize: '0.8125rem' }}
                      >
                        {formatTimeAgo(bid.createdAt)}
                      </Typography>
                    </Box>
                  </Stack>
                  <Typography
                    sx={{
                      fontWeight: 600,
                      color: palette.neutral[900],
                      fontSize: '1rem',
                    }}
                  >
                    ${bid.amount.toLocaleString()}
                  </Typography>
                </Box>
              ))}
            </Stack>
          )}
        </TabPanel>

        <TabPanel value={activeTab} index={2}>
          <ReviewsList
            reviews={reviews}
            ratingSummary={ratingSummary}
            isLoading={reviewsLoading}
            showSummary={reviews.length > 0}
          />
        </TabPanel>

        <TabPanel value={activeTab} index={3}>
          {shippingInfo ? (
            <Stack spacing={3}>
              <Box>
                <Typography
                  variant="subtitle2"
                  sx={{ color: palette.neutral[500], mb: 0.5, fontSize: '0.8125rem' }}
                >
                  Shipping Method
                </Typography>
                <Typography sx={{ fontWeight: 500, color: palette.neutral[900] }}>
                  {shippingInfo.method}
                </Typography>
              </Box>
              <Box>
                <Typography
                  variant="subtitle2"
                  sx={{ color: palette.neutral[500], mb: 0.5, fontSize: '0.8125rem' }}
                >
                  Shipping Cost
                </Typography>
                <Typography sx={{ fontWeight: 500, color: palette.neutral[900] }}>
                  {shippingInfo.cost === 'free' ? (
                    <Chip label="Free Shipping" size="small" color="success" />
                  ) : (
                    `$${shippingInfo.cost.toFixed(2)}`
                  )}
                </Typography>
              </Box>
              <Box>
                <Typography
                  variant="subtitle2"
                  sx={{ color: palette.neutral[500], mb: 0.5, fontSize: '0.8125rem' }}
                >
                  Estimated Delivery
                </Typography>
                <Typography sx={{ fontWeight: 500, color: palette.neutral[900] }}>
                  {shippingInfo.estimatedDays}
                </Typography>
              </Box>
              <Box>
                <Typography
                  variant="subtitle2"
                  sx={{ color: palette.neutral[500], mb: 0.5, fontSize: '0.8125rem' }}
                >
                  Ships To
                </Typography>
                <Stack direction="row" flexWrap="wrap" gap={0.5}>
                  {shippingInfo.locations.map((location) => (
                    <Chip
                      key={location}
                      label={location}
                      size="small"
                      variant="outlined"
                      sx={{ borderColor: palette.neutral[100] }}
                    />
                  ))}
                </Stack>
              </Box>
            </Stack>
          ) : (
            <Typography sx={{ color: palette.neutral[500] }}>
              Shipping information not available. Please contact the seller for details.
            </Typography>
          )}
        </TabPanel>

        <TabPanel value={activeTab} index={4}>
          {returnPolicy ? (
            <Stack spacing={3}>
              <Box>
                <Typography
                  variant="subtitle2"
                  sx={{ color: palette.neutral[500], mb: 0.5, fontSize: '0.8125rem' }}
                >
                  Returns Accepted
                </Typography>
                <Chip
                  label={returnPolicy.accepted ? 'Yes' : 'No Returns'}
                  size="small"
                  color={returnPolicy.accepted ? 'success' : 'error'}
                />
              </Box>
              {returnPolicy.accepted && (
                <>
                  <Box>
                    <Typography
                      variant="subtitle2"
                      sx={{ color: palette.neutral[500], mb: 0.5, fontSize: '0.8125rem' }}
                    >
                      Return Period
                    </Typography>
                    <Typography sx={{ fontWeight: 500, color: palette.neutral[900] }}>
                      {returnPolicy.period} days from delivery
                    </Typography>
                  </Box>
                  <Box>
                    <Typography
                      variant="subtitle2"
                      sx={{ color: palette.neutral[500], mb: 0.5, fontSize: '0.8125rem' }}
                    >
                      Conditions
                    </Typography>
                    <Typography sx={{ color: palette.neutral[700], lineHeight: 1.6 }}>
                      {returnPolicy.conditions}
                    </Typography>
                  </Box>
                </>
              )}
            </Stack>
          ) : (
            <Typography sx={{ color: palette.neutral[500] }}>
              Return policy not specified. Please contact the seller for details.
            </Typography>
          )}
        </TabPanel>
      </Box>
    </Box>
  )
}

export function ProductTabsSkeleton() {
  return (
    <Box
      sx={{
        bgcolor: 'white',
        borderRadius: 2,
        border: '1px solid #E5E5E5',
        overflow: 'hidden',
      }}
    >
      <Box sx={{ borderBottom: '1px solid #E5E5E5', px: 2, py: 1 }}>
        <Stack direction="row" spacing={3}>
          {[1, 2, 3, 4].map((i) => (
            <Skeleton key={i} width={100} height={40} />
          ))}
        </Stack>
      </Box>
      <Box sx={{ p: 3 }}>
        <Skeleton width="100%" height={20} sx={{ mb: 1 }} />
        <Skeleton width="100%" height={20} sx={{ mb: 1 }} />
        <Skeleton width="80%" height={20} sx={{ mb: 1 }} />
        <Skeleton width="60%" height={20} />
      </Box>
    </Box>
  )
}
