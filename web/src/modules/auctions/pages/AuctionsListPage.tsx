import { useState, useEffect, useMemo } from 'react'
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

export const AuctionsListPage = () => {
  const [searchParams] = useSearchParams()
  const [searchQuery, setSearchQuery] = useState('')
  const [category, setCategory] = useState(() => searchParams.get('categoryId') ?? 'all')
  const [sortBy, setSortBy] = useState('ending-soon')
  const [page, setPage] = useState(1)
  const { data: categoriesData } = useActiveCategories()
  const categories = useMemo(() => categoriesData ?? [], [categoriesData])

  useEffect(() => {
    const categoryId = searchParams.get('categoryId')
    const nextCategory = categoryId ?? 'all'
    if (nextCategory !== category) {
      setCategory(nextCategory)
    }
  }, [searchParams, category])

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
                  transition: 'transform 0.2s, box-shadow 0.2s',
                  '&:hover': {
                    transform: 'translateY(-4px)',
                    boxShadow: 4,
                  },
                }}
              >
                <Box sx={{ position: 'relative' }}>
                  <CardMedia
                    component="img"
                    image={auction.primaryImageUrl || '/placeholder.jpg'}
                    alt={auction.title}
                    sx={{ height: 180, objectFit: 'cover' }}
                  />
                  <StatusBadge
                    status={auction.status.replace('-', ' ')}
                    sx={{
                      position: 'absolute',
                      top: 8,
                      right: 8,
                      textTransform: 'capitalize',
                    }}
                  />
                </Box>
                <CardContent sx={{ flexGrow: 1 }}>
                  <Typography variant="subtitle1" fontWeight={600} noWrap gutterBottom>
                    {auction.title}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    {auction.categoryName}
                  </Typography>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mb: 1 }}>
                    <Timer fontSize="small" color="action" />
                    <Typography variant="body2" color="text.secondary">
                      {formatTimeLeft(auction.endTime)}
                    </Typography>
                  </Box>
                  <Typography variant="h6" fontWeight={700} color="primary">
                    ${auction.currentBid.toLocaleString()}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {auction.bidCount} bids
                  </Typography>
                </CardContent>
                <Box sx={{ p: 2, pt: 0 }}>
                  <Button variant="contained" fullWidth>
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
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" fontWeight={700} gutterBottom>
          Browse Auctions
        </Typography>
        <Typography variant="body1" color="text.secondary">
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
          <Select value={category} label="Category" onChange={(e) => setCategory(e.target.value)}>
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
