import { useState, useMemo } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import {
  Container,
  Typography,
  Box,
  Grid,
  Card,
  CardContent,
  CardMedia,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Pagination,
  InputAdornment,
  Button,
  CircularProgress,
} from '@mui/material'
import { Search, Timer } from '@mui/icons-material'
import { formatTimeLeft } from '../utils'
import { useAuctions, useActiveCategories } from '../hooks'
import { ErrorState, EmptyState, StatusBadge } from '@/shared/ui'
import { palette, typography } from '@/shared/theme/tokens'

export const AuctionsListPage = () => {
  const [searchParams, setSearchParams] = useSearchParams()
  const [searchQuery, setSearchQuery] = useState('')
  const category = useMemo(() => searchParams.get('categoryId') ?? 'all', [searchParams])
  const [sortBy, setSortBy] = useState('ending-soon')
  const [page, setPage] = useState(1)
  const { data: categoriesData } = useActiveCategories()
  const categories = useMemo(() => categoriesData ?? [], [categoriesData])

  const { data, isLoading, isError, refetch } = useAuctions({
    search: searchQuery || undefined,
    categoryId: category === 'all' ? undefined : category,
    page,
    pageSize: 12,
  })

  const renderContent = () => {
    if (isLoading) {
      return (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      )
    }

    if (isError) {
      return (
        <ErrorState
          title="Failed to load auctions"
          message="We couldn't load the auctions. Please try again."
          onRetry={() => refetch()}
        />
      )
    }

    if (!data?.items || data.items.length === 0) {
      return (
        <EmptyState
          variant="auctions"
          action={{
            label: 'Create Auction',
            href: '/auctions/create',
          }}
        />
      )
    }

    return (
      <>
        <Grid container spacing={3}>
          {data.items.map((auction) => (
            <Grid size={{ xs: 12, sm: 6, md: 4, lg: 3 }} key={auction.id}>
              <Card
                component={Link}
                to={`/auctions/${auction.id}`}
                aria-label={`View auction: ${auction.title}`}
                sx={{
                  height: '100%',
                  display: 'flex',
                  flexDirection: 'column',
                  textDecoration: 'none',
                  cursor: 'pointer',
                  border: `1px solid ${palette.neutral[200]}`,
                  boxShadow: 'none',
                  borderRadius: 0,
                  transition: 'border-color 0.3s ease',
                  '&:hover': {
                    borderColor: palette.neutral[900],
                  },
                  '&:hover .auction-card-image': {
                    transform: 'scale(1.03)',
                  },
                }}
              >
                <Box sx={{ position: 'relative', overflow: 'hidden' }}>
                  <CardMedia
                    className="auction-card-image"
                    component="img"
                    image={auction.primaryImageUrl || '/placeholder.jpg'}
                    alt={auction.title}
                    sx={{
                      height: 200,
                      objectFit: 'cover',
                      transition: 'transform 0.6s ease',
                    }}
                  />
                  <StatusBadge
                    status={auction.status.replace('-', ' ')}
                    sx={{
                      position: 'absolute',
                      top: 12,
                      right: 12,
                      textTransform: 'capitalize',
                    }}
                  />
                </Box>
                <CardContent sx={{ flexGrow: 1, p: 2.5 }}>
                  <Typography
                    sx={{
                      color: palette.neutral[500],
                      fontSize: '0.625rem',
                      letterSpacing: '0.1em',
                      textTransform: 'uppercase',
                      fontWeight: typography.fontWeight.medium,
                      mb: 0.5,
                    }}
                  >
                    {auction.categoryName}
                  </Typography>
                  <Typography
                    sx={{
                      fontWeight: typography.fontWeight.medium,
                      fontSize: '0.9rem',
                      color: palette.neutral[900],
                      lineHeight: 1.4,
                      mb: 1.5,
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap',
                    }}
                  >
                    {auction.title}
                  </Typography>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mb: 1.5 }}>
                    <Timer sx={{ fontSize: 14, color: palette.neutral[400] }} />
                    <Typography sx={{ fontSize: '0.75rem', color: palette.neutral[500] }}>
                      {formatTimeLeft(auction.endTime)}
                    </Typography>
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline' }}>
                    <Typography
                      sx={{
                        fontWeight: typography.fontWeight.semibold,
                        fontSize: '1rem',
                        color: palette.neutral[900],
                      }}
                    >
                      ${auction.currentBid.toLocaleString()}
                    </Typography>
                    <Typography sx={{ fontSize: '0.75rem', color: palette.neutral[400] }}>
                      {auction.bidCount} bids
                    </Typography>
                  </Box>
                </CardContent>
                <Box sx={{ px: 2.5, pb: 2.5, pt: 0 }}>
                  <Button
                    variant="outlined"
                    fullWidth
                    sx={{
                      borderColor: palette.neutral[900],
                      color: palette.neutral[900],
                      fontSize: '0.75rem',
                      fontWeight: typography.fontWeight.medium,
                      letterSpacing: '0.1em',
                      textTransform: 'uppercase',
                      borderRadius: 0,
                      py: 1,
                      '&:hover': {
                        bgcolor: palette.neutral[900],
                        color: palette.neutral[0],
                        borderColor: palette.neutral[900],
                      },
                    }}
                  >
                    View Details
                  </Button>
                </Box>
              </Card>
            </Grid>
          ))}
        </Grid>

        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
          <Pagination
            count={data.totalPages || 1}
            page={page}
            onChange={(_, value) => setPage(value)}
            color="primary"
            size="large"
          />
        </Box>
      </>
    )
  }

  return (
    <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
      <Box sx={{ mb: 5 }}>
        <Typography
          sx={{
            color: palette.neutral[500],
            letterSpacing: '0.2em',
            fontSize: '0.6875rem',
            fontWeight: typography.fontWeight.medium,
            textTransform: 'uppercase',
            mb: 1.5,
          }}
        >
          Explore
        </Typography>
        <Typography
          variant="h3"
          sx={{
            fontFamily: typography.fontFamily.display,
            fontWeight: typography.fontWeight.regular,
            color: palette.neutral[900],
            fontSize: { xs: '1.75rem', md: '2.5rem' },
            mb: 1,
          }}
        >
          Browse Auctions
        </Typography>
        <Typography sx={{ color: palette.neutral[500], fontSize: '1rem' }}>
          Find unique items and place your bids
        </Typography>
      </Box>

      <Box
        sx={{
          display: 'flex',
          gap: 2,
          mb: 4,
          flexWrap: 'wrap',
        }}
      >
        <TextField
          placeholder="Search auctions..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          sx={{ flexGrow: 1, minWidth: 200 }}
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <Search />
                </InputAdornment>
              ),
            },
          }}
        />

        <FormControl sx={{ minWidth: 150 }}>
          <InputLabel>Category</InputLabel>
          <Select value={category} label="Category" onChange={(e) => {
            const value = e.target.value
            if (value === 'all') {
              searchParams.delete('categoryId')
            } else {
              searchParams.set('categoryId', value)
            }
            setSearchParams(searchParams)
          }}>
            <MenuItem value="all">All Categories</MenuItem>
            {categories.map((cat) => (
              <MenuItem key={cat.id} value={cat.id}>
                {cat.name}
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        <FormControl sx={{ minWidth: 150 }}>
          <InputLabel>Sort By</InputLabel>
          <Select value={sortBy} label="Sort By" onChange={(e) => setSortBy(e.target.value)}>
            <MenuItem value="ending-soon">Ending Soon</MenuItem>
            <MenuItem value="newest">Newest</MenuItem>
            <MenuItem value="price-low">Price: Low to High</MenuItem>
            <MenuItem value="price-high">Price: High to Low</MenuItem>
            <MenuItem value="popular">Most Bids</MenuItem>
          </Select>
        </FormControl>
      </Box>

      {renderContent()}
    </Container>
  )
}
