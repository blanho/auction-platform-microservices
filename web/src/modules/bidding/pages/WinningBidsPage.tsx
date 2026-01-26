import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  Chip,
  Button,
  Skeleton,
  Alert,
  Pagination,
  Stack,
  Container,
} from '@mui/material'
import { EmojiEvents, AccessTime, AttachMoney, Whatshot } from '@mui/icons-material'
import { useWinningBids } from '../hooks/useBids'
import { BID_CONSTANTS } from '../constants'
import { formatCurrency, formatRelativeTime } from '@/shared/utils'

export const WinningBidsPage = () => {
  const navigate = useNavigate()
  const [page, setPage] = useState(1)
  const pageSize = BID_CONSTANTS.PAGE_SIZE
  const { data, isLoading, error } = useWinningBids(page, pageSize)

  if (error) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4 }}>
        <Alert severity="error">Failed to load winning bids. Please try again.</Alert>
      </Container>
    )
  }

  const totalPages = Math.ceil((data?.totalCount || 0) / pageSize)

  return (
    <Box
      sx={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #F8FAFC 0%, #E0E7FF 100%)',
        py: 6,
      }}
    >
      <Container maxWidth="xl">
        <Box
          sx={{
            mb: 6,
            p: 4,
            background: 'rgba(255, 255, 255, 0.7)',
            backdropFilter: 'blur(12px)',
            borderRadius: 3,
            border: '1px solid rgba(255, 255, 255, 0.3)',
          }}
        >
          <Stack direction="row" alignItems="center" spacing={2} mb={1}>
            <EmojiEvents sx={{ width: 40, height: 40, color: '#2563EB' }} />
            <Typography variant="h3" sx={{ fontFamily: 'Russo One', fontWeight: 700, color: '#1E293B' }}>
              Winning Bids
            </Typography>
          </Stack>
          <Typography variant="body1" color="text.secondary">
            Auctions where you currently have the highest bid
          </Typography>
        </Box>

        {isLoading ? (
          <Grid container spacing={3}>
            {[...Array(6)].map((_, index) => (
              <Grid key={index} size={{ xs: 12, sm: 6, md: 4 }}>
                <Skeleton variant="rounded" height={280} sx={{ borderRadius: 3 }} />
              </Grid>
            ))}
          </Grid>
        ) : data?.items && data.items.length > 0 ? (
          <>
            <Grid container spacing={3} mb={4}>
              {data.items.map((bid) => (
                <Grid key={bid.id} size={{ xs: 12, sm: 6, md: 4 }}>
                  <Card
                    sx={{
                      height: '100%',
                      background: 'rgba(255, 255, 255, 0.85)',
                      backdropFilter: 'blur(16px)',
                      border: '1px solid rgba(37, 99, 235, 0.2)',
                      borderRadius: 3,
                      transition: 'all 0.3s ease',
                      cursor: 'pointer',
                      position: 'relative',
                      overflow: 'visible',
                      '&:hover': {
                        transform: 'translateY(-8px)',
                        boxShadow: '0 20px 40px rgba(37, 99, 235, 0.2)',
                        border: '1px solid rgba(37, 99, 235, 0.4)',
                      },
                    }}
                    onClick={() => navigate(`/auctions/${bid.auctionId}`)}
                  >
                    <CardContent sx={{ p: 3 }}>
                      <Box
                        sx={{
                          position: 'absolute',
                          top: -12,
                          right: 16,
                          px: 2,
                          py: 0.5,
                          background: 'linear-gradient(135deg, #F97316 0%, #FB923C 100%)',
                          borderRadius: 2,
                          border: '2px solid rgba(255, 255, 255, 0.9)',
                        }}
                      >
                        <Stack direction="row" alignItems="center" spacing={0.5}>
                          <Whatshot sx={{ width: 16, height: 16, color: '#FFF' }} />
                          <Typography
                            variant="caption"
                            sx={{
                              color: '#FFF',
                              fontFamily: 'Chakra Petch',
                              fontWeight: 700,
                              fontSize: '0.75rem',
                            }}
                          >
                            WINNING
                          </Typography>
                        </Stack>
                      </Box>

                      <Typography
                        variant="h6"
                        sx={{
                          fontFamily: 'Russo One',
                          color: '#1E293B',
                          mb: 3,
                          mt: 1,
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                          display: '-webkit-box',
                          WebkitLineClamp: 2,
                          WebkitBoxOrient: 'vertical',
                        }}
                      >
                        {bid.auctionTitle}
                      </Typography>

                      <Stack spacing={2.5}>
                        <Box>
                          <Stack direction="row" alignItems="center" spacing={1} mb={0.5}>
                            <AttachMoney sx={{ width: 18, height: 18, color: '#2563EB' }} />
                            <Typography
                              variant="caption"
                              sx={{ color: '#64748B', fontFamily: 'Chakra Petch', fontWeight: 500 }}
                            >
                              Your Bid
                            </Typography>
                          </Stack>
                          <Typography
                            variant="h5"
                            sx={{
                              color: '#2563EB',
                              fontFamily: 'Russo One',
                              fontWeight: 700,
                            }}
                          >
                            {formatCurrency(bid.currentBid)}
                          </Typography>
                          <Typography variant="caption" sx={{ color: '#94A3B8' }}>
                            Next min: {formatCurrency(bid.minimumNextBid)}
                          </Typography>
                        </Box>

                        <Stack direction="row" alignItems="center" spacing={1}>
                          <AccessTime sx={{ width: 18, height: 18, color: '#94A3B8' }} />
                          <Typography variant="body2" sx={{ color: '#64748B', fontFamily: 'Chakra Petch' }}>
                            Ends {formatRelativeTime(bid.auctionEndTime)}
                          </Typography>
                        </Stack>

                        <Stack direction="row" spacing={1} flexWrap="wrap">
                          {bid.isActive && (
                            <Chip
                              label="ACTIVE"
                              size="small"
                              sx={{
                                background: 'rgba(34, 197, 94, 0.1)',
                                color: '#16A34A',
                                fontFamily: 'Chakra Petch',
                                fontWeight: 600,
                                fontSize: '0.7rem',
                              }}
                            />
                          )}
                          <Chip
                            label={`${bid.bidCount} BIDS`}
                            size="small"
                            sx={{
                              background: 'rgba(37, 99, 235, 0.1)',
                              color: '#2563EB',
                              fontFamily: 'Chakra Petch',
                              fontWeight: 600,
                              fontSize: '0.7rem',
                            }}
                          />
                        </Stack>
                      </Stack>
                    </CardContent>
                  </Card>
                </Grid>
              ))}
            </Grid>

            {totalPages > 1 && (
              <Box display="flex" justifyContent="center">
                <Pagination
                  count={totalPages}
                  page={page}
                  onChange={(_, value) => setPage(value)}
                  color="primary"
                  size="large"
                  sx={{
                    '& .MuiPaginationItem-root': {
                      fontFamily: 'Chakra Petch',
                      fontWeight: 600,
                      background: 'rgba(255, 255, 255, 0.7)',
                      backdropFilter: 'blur(8px)',
                    },
                    '& .Mui-selected': {
                      background: '#2563EB !important',
                      color: '#FFF',
                    },
                  }}
                />
              </Box>
            )}
          </>
        ) : (
          <Card
            sx={{
              p: 6,
              textAlign: 'center',
              background: 'rgba(255, 255, 255, 0.7)',
              backdropFilter: 'blur(12px)',
              border: '1px solid rgba(255, 255, 255, 0.3)',
              borderRadius: 3,
            }}
          >
            <EmojiEvents sx={{ width: 80, height: 80, color: '#CBD5E1', margin: '0 auto 16px' }} />
            <Typography variant="h6" sx={{ fontFamily: 'Russo One', color: '#64748B', mb: 2 }}>
              No Winning Bids Yet
            </Typography>
            <Typography variant="body2" color="text.secondary" mb={3}>
              Start bidding on auctions to see your winning positions here
            </Typography>
            <Button
              variant="contained"
              onClick={() => navigate('/auctions')}
              sx={{
                background: 'linear-gradient(135deg, #F97316 0%, #FB923C 100%)',
                fontFamily: 'Chakra Petch',
                fontWeight: 700,
                px: 4,
                py: 1.5,
                '&:hover': {
                  background: 'linear-gradient(135deg, #EA580C 0%, #F97316 100%)',
                },
              }}
            >
              Browse Auctions
            </Button>
          </Card>
        )}
      </Container>
    </Box>
  )
}
