import { useState } from 'react'
import { Link } from 'react-router-dom'
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
  Chip,
  Pagination,
  InputAdornment,
  Button,
  CircularProgress,
} from '@mui/material'
import { Search, Timer } from '@mui/icons-material'
import { formatTimeLeft, getStatusColor } from '../utils'
import { useAuctions } from '../hooks'

export const AuctionsListPage = () => {
  const [searchQuery, setSearchQuery] = useState('')
  const [category, setCategory] = useState('all')
  const [sortBy, setSortBy] = useState('ending-soon')
  const [page, setPage] = useState(1)

  const { data, isLoading } = useAuctions({
    search: searchQuery || undefined,
    categoryId: category !== 'all' ? category : undefined,
    page,
    pageSize: 12,
  })

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
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
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <Search />
              </InputAdornment>
            ),
          }}
        />

        <FormControl sx={{ minWidth: 150 }}>
          <InputLabel>Category</InputLabel>
          <Select
            value={category}
            label="Category"
            onChange={(e) => setCategory(e.target.value)}
          >
            <MenuItem value="all">All Categories</MenuItem>
            <MenuItem value="electronics">Electronics</MenuItem>
            <MenuItem value="art">Art</MenuItem>
            <MenuItem value="collectibles">Collectibles</MenuItem>
            <MenuItem value="fashion">Fashion</MenuItem>
          </Select>
        </FormControl>

        <FormControl sx={{ minWidth: 150 }}>
          <InputLabel>Sort By</InputLabel>
          <Select
            value={sortBy}
            label="Sort By"
            onChange={(e) => setSortBy(e.target.value)}
          >
            <MenuItem value="ending-soon">Ending Soon</MenuItem>
            <MenuItem value="newest">Newest</MenuItem>
            <MenuItem value="price-low">Price: Low to High</MenuItem>
            <MenuItem value="price-high">Price: High to Low</MenuItem>
            <MenuItem value="popular">Most Bids</MenuItem>
          </Select>
        </FormControl>
      </Box>

      {isLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      ) : (
        <>
          <Grid container spacing={3}>
            {data?.items.map((auction) => (
              <Grid size={{ xs: 12, sm: 6, md: 4, lg: 3 }} key={auction.id}>
                <Card
                  component={Link}
                  to={`/auctions/${auction.id}`}
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
                    <Chip
                      label={auction.status.replace('-', ' ')}
                      color={getStatusColor(auction.status)}
                      size="small"
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
              count={data?.totalPages || 1}
              page={page}
              onChange={(_, value) => setPage(value)}
              color="primary"
              size="large"
            />
          </Box>
        </>
      )}
    </Container>
  )
}
