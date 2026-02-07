import { useState, useMemo } from 'react'
import {
  Box,
  Container,
  Typography,
  Grid,
  Button,
  Drawer,
  IconButton,
  FormControl,
  FormGroup,
  FormControlLabel,
  Checkbox,
  Slider,
  Divider,
  Select,
  MenuItem,
  Chip,
  Breadcrumbs,
  Link as MuiLink,
  useMediaQuery,
  useTheme,
} from '@mui/material'
import { FilterList, Close, KeyboardArrowDown, GridView, ViewList } from '@mui/icons-material'
import { Link } from 'react-router-dom'
import { palette } from '@/shared/theme/tokens'
import { AuctionProductCard, AuctionProductCardSkeleton } from '../components/AuctionProductCard'
import { useAuctions, useActiveCategories } from '../hooks'

const conditions = [
  { value: 'new', label: 'New' },
  { value: 'like-new', label: 'Like New' },
  { value: 'excellent', label: 'Excellent' },
  { value: 'good', label: 'Good' },
  { value: 'fair', label: 'Fair' },
]

const sortOptions = [
  { value: 'ending-soon', label: 'Ending Soon' },
  { value: 'newly-listed', label: 'Newly Listed' },
  { value: 'price-low', label: 'Price: Low to High' },
  { value: 'price-high', label: 'Price: High to Low' },
  { value: 'most-bids', label: 'Most Bids' },
]

export const AuctionListingPage = () => {
  const theme = useTheme()
  const isMobile = useMediaQuery(theme.breakpoints.down('md'))

  const [filterDrawerOpen, setFilterDrawerOpen] = useState(false)
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid')
  const [favorites, setFavorites] = useState<Set<string>>(new Set())

  const [selectedCategories, setSelectedCategories] = useState<string[]>([])
  const [selectedConditions, setSelectedConditions] = useState<string[]>([])
  const [priceRange, setPriceRange] = useState<[number, number]>([0, 20000])
  const [sortBy, setSortBy] = useState('ending-soon')
  const [verifiedOnly, setVerifiedOnly] = useState(false)

  const { data: categoriesData } = useActiveCategories()
  const categories = useMemo(() => categoriesData ?? [], [categoriesData])

  const skeletonKeys = useMemo(
    () => Array.from({ length: 12 }, () => crypto.randomUUID()),
    []
  )

  const { data, isLoading } = useAuctions({
    categoryId: selectedCategories[0],
    minPrice: priceRange[0],
    maxPrice: priceRange[1],
    page: 1,
    pageSize: 24,
  })

  const activeFiltersCount = useMemo(() => {
    let count = 0
    if (selectedCategories.length) {count++}
    if (selectedConditions.length) {count++}
    if (priceRange[0] > 0 || priceRange[1] < 20000) {count++}
    if (verifiedOnly) {count++}
    return count
  }, [selectedCategories, selectedConditions, priceRange, verifiedOnly])

  const toggleFavorite = (id: string) => {
    setFavorites((prev) => {
      const next = new Set(prev)
      if (next.has(id)) {next.delete(id)}
      else {next.add(id)}
      return next
    })
  }

  const clearAllFilters = () => {
    setSelectedCategories([])
    setSelectedConditions([])
    setPriceRange([0, 20000])
    setVerifiedOnly(false)
  }

  const filterContent = (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h6" sx={{ fontWeight: 500, color: palette.neutral[900] }}>
          Filters
        </Typography>
        {isMobile && (
          <IconButton onClick={() => setFilterDrawerOpen(false)}>
            <Close />
          </IconButton>
        )}
      </Box>

      {activeFiltersCount > 0 && (
        <Button
          size="small"
          onClick={clearAllFilters}
          sx={{
            color: palette.brand.primary,
            textTransform: 'none',
            mb: 2,
            p: 0,
            '&:hover': { bgcolor: 'transparent', textDecoration: 'underline' },
          }}
        >
          Clear All Filters
        </Button>
      )}

      <Box sx={{ mb: 4 }}>
        <Typography
          variant="subtitle2"
          sx={{
            fontWeight: 600,
            color: palette.neutral[900],
            mb: 2,
            textTransform: 'uppercase',
            letterSpacing: 1,
            fontSize: '0.7rem',
          }}
        >
          Category
        </Typography>
        <FormGroup>
          {categories.map((category) => (
            <FormControlLabel
              key={category.id}
              control={
                <Checkbox
                  size="small"
                  checked={selectedCategories.includes(category.id)}
                  onChange={(e) => {
                    if (e.target.checked) {
                      setSelectedCategories([...selectedCategories, category.id])
                    } else {
                      setSelectedCategories(selectedCategories.filter((c) => c !== category.id))
                    }
                  }}
                  sx={{
                    color: palette.neutral[500],
                    '&.Mui-checked': { color: palette.brand.primary },
                  }}
                />
              }
              label={
                <Box sx={{ display: 'flex', justifyContent: 'space-between', width: '100%' }}>
                  <Typography variant="body2" sx={{ color: palette.neutral[700] }}>
                    {category.name}
                  </Typography>
                  <Typography variant="body2" sx={{ color: palette.neutral[500] }}>
                    ({category.count})
                  </Typography>
                </Box>
              }
              sx={{ width: '100%', mr: 0, '& .MuiFormControlLabel-label': { flex: 1 } }}
            />
          ))}
        </FormGroup>
      </Box>

      <Divider sx={{ mb: 4, borderColor: 'rgba(68,64,60,0.1)' }} />

      <Box sx={{ mb: 4 }}>
        <Typography
          variant="subtitle2"
          sx={{
            fontWeight: 600,
            color: palette.neutral[900],
            mb: 2,
            textTransform: 'uppercase',
            letterSpacing: 1,
            fontSize: '0.7rem',
          }}
        >
          Price Range
        </Typography>
        <Slider
          value={priceRange}
          onChange={(_, value) => setPriceRange(value as [number, number])}
          min={0}
          max={20000}
          step={100}
          valueLabelDisplay="auto"
          valueLabelFormat={(value) => `$${value.toLocaleString()}`}
          sx={{
            color: palette.brand.primary,
            '& .MuiSlider-thumb': {
              bgcolor: palette.neutral[50],
              border: `2px solid ${palette.brand.primary}`,
            },
          }}
        />
        <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 1 }}>
          <Typography variant="caption" sx={{ color: palette.neutral[500] }}>
            ${priceRange[0].toLocaleString()}
          </Typography>
          <Typography variant="caption" sx={{ color: palette.neutral[500] }}>
            ${priceRange[1].toLocaleString()}
          </Typography>
        </Box>
      </Box>

      <Divider sx={{ mb: 4, borderColor: 'rgba(68,64,60,0.1)' }} />

      <Box sx={{ mb: 4 }}>
        <Typography
          variant="subtitle2"
          sx={{
            fontWeight: 600,
            color: palette.neutral[900],
            mb: 2,
            textTransform: 'uppercase',
            letterSpacing: 1,
            fontSize: '0.7rem',
          }}
        >
          Condition
        </Typography>
        <FormGroup>
          {conditions.map((condition) => (
            <FormControlLabel
              key={condition.value}
              control={
                <Checkbox
                  size="small"
                  checked={selectedConditions.includes(condition.value)}
                  onChange={(e) => {
                    if (e.target.checked) {
                      setSelectedConditions([...selectedConditions, condition.value])
                    } else {
                      setSelectedConditions(selectedConditions.filter((c) => c !== condition.value))
                    }
                  }}
                  sx={{
                    color: palette.neutral[500],
                    '&.Mui-checked': { color: palette.brand.primary },
                  }}
                />
              }
              label={
                <Typography variant="body2" sx={{ color: palette.neutral[700] }}>
                  {condition.label}
                </Typography>
              }
            />
          ))}
        </FormGroup>
      </Box>

      <Divider sx={{ mb: 4, borderColor: 'rgba(68,64,60,0.1)' }} />

      <FormControlLabel
        control={
          <Checkbox
            size="small"
            checked={verifiedOnly}
            onChange={(e) => setVerifiedOnly(e.target.checked)}
            sx={{
              color: palette.neutral[500],
              '&.Mui-checked': { color: palette.brand.primary },
            }}
          />
        }
        label={
          <Typography variant="body2" sx={{ color: palette.neutral[700] }}>
            Verified Sellers Only
          </Typography>
        }
      />
    </Box>
  )

  return (
    <Box sx={{ bgcolor: palette.neutral[50], minHeight: '100vh', pt: 4, pb: 10 }}>
      <Container maxWidth="xl">
        <Breadcrumbs
          sx={{ mb: 3 }}
          separator={<Typography sx={{ color: palette.neutral[500], mx: 1 }}>/</Typography>}
        >
          <MuiLink
            component={Link}
            to="/"
            sx={{
              color: palette.neutral[500],
              textDecoration: 'none',
              '&:hover': { color: palette.neutral[900] },
            }}
          >
            Home
          </MuiLink>
          <Typography sx={{ color: palette.neutral[900] }}>All Auctions</Typography>
        </Breadcrumbs>

        <Box sx={{ mb: 6 }}>
          <Typography
            variant="h2"
            sx={{
              color: palette.neutral[900],
              fontWeight: 300,
              fontSize: { xs: '2rem', md: '2.75rem' },
              fontFamily: '"Playfair Display", serif',
              mb: 1,
            }}
          >
            All Auctions
          </Typography>
          <Typography variant="body1" sx={{ color: palette.neutral[500] }}>
            {data?.totalCount || 0} items available
          </Typography>
        </Box>

        <Grid container spacing={4}>
          <Grid
            size={{ xs: 12, md: 3 }}
            sx={{
              display: { xs: 'none', md: 'block' },
            }}
          >
            <Box
              sx={{
                position: 'sticky',
                top: 100,
                bgcolor: palette.neutral[0],
                border: '1px solid rgba(68,64,60,0.1)',
              }}
            >
              {filterContent}
            </Box>
          </Grid>

          <Grid size={{ xs: 12, md: 9 }}>
            <Box
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                mb: 4,
                pb: 3,
                borderBottom: '1px solid rgba(68,64,60,0.1)',
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Button
                  startIcon={<FilterList />}
                  onClick={() => setFilterDrawerOpen(true)}
                  sx={{
                    display: { md: 'none' },
                    color: palette.neutral[900],
                    borderColor: 'rgba(68,64,60,0.2)',
                    textTransform: 'none',
                  }}
                  variant="outlined"
                >
                  Filters {activeFiltersCount > 0 && `(${activeFiltersCount})`}
                </Button>

                {activeFiltersCount > 0 && (
                  <Box sx={{ display: { xs: 'none', md: 'flex' }, gap: 1, flexWrap: 'wrap' }}>
                    {selectedCategories.map((catId) => {
                      const cat = categories.find((c) => c.id === catId)
                      return (
                        <Chip
                          key={catId}
                          label={cat?.name}
                          size="small"
                          onDelete={() =>
                            setSelectedCategories(selectedCategories.filter((c) => c !== catId))
                          }
                          sx={{
                            bgcolor: palette.brand.muted,
                            color: palette.brand.primary,
                            '& .MuiChip-deleteIcon': { color: palette.brand.primary },
                          }}
                        />
                      )
                    })}
                    {verifiedOnly && (
                      <Chip
                        label="Verified Only"
                        size="small"
                        onDelete={() => setVerifiedOnly(false)}
                        sx={{
                          bgcolor: palette.brand.muted,
                          color: palette.brand.primary,
                          '& .MuiChip-deleteIcon': { color: palette.brand.primary },
                        }}
                      />
                    )}
                  </Box>
                )}
              </Box>

              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Box sx={{ display: { xs: 'none', sm: 'flex' }, gap: 0.5 }}>
                  <IconButton
                    size="small"
                    onClick={() => setViewMode('grid')}
                    sx={{
                      color: viewMode === 'grid' ? palette.neutral[900] : palette.neutral[500],
                    }}
                  >
                    <GridView fontSize="small" />
                  </IconButton>
                  <IconButton
                    size="small"
                    onClick={() => setViewMode('list')}
                    sx={{
                      color: viewMode === 'list' ? palette.neutral[900] : palette.neutral[500],
                    }}
                  >
                    <ViewList fontSize="small" />
                  </IconButton>
                </Box>

                <FormControl size="small" sx={{ minWidth: 160 }}>
                  <Select
                    value={sortBy}
                    onChange={(e) => setSortBy(e.target.value)}
                    IconComponent={KeyboardArrowDown}
                    sx={{
                      color: palette.neutral[900],
                      '& .MuiOutlinedInput-notchedOutline': {
                        borderColor: 'rgba(68,64,60,0.2)',
                      },
                      '&:hover .MuiOutlinedInput-notchedOutline': {
                        borderColor: 'rgba(68,64,60,0.4)',
                      },
                    }}
                  >
                    {sortOptions.map((option) => (
                      <MenuItem key={option.value} value={option.value}>
                        {option.label}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Box>
            </Box>

            <Grid container spacing={3}>
              {isLoading
                ? skeletonKeys.map((key) => (
                    <Grid size={{ xs: 6, sm: 4, lg: 3 }} key={key}>
                      <AuctionProductCardSkeleton />
                    </Grid>
                  ))
                : data?.items.map((auction) => (
                    <Grid
                      size={{
                        xs: 6,
                        sm: viewMode === 'grid' ? 4 : 12,
                        lg: viewMode === 'grid' ? 3 : 12,
                      }}
                      key={auction.id}
                    >
                      <AuctionProductCard
                        id={auction.id}
                        title={auction.title}
                        currentBid={auction.currentBid}
                        startingPrice={auction.startingPrice}
                        images={[auction.primaryImageUrl || '/placeholder.jpg']}
                        endTime={auction.endTime}
                        bidCount={auction.bidCount}
                        seller={{ name: auction.sellerName, verified: true }}
                        isFavorited={favorites.has(auction.id)}
                        onFavoriteToggle={toggleFavorite}
                      />
                    </Grid>
                  ))}
            </Grid>

            <Box sx={{ textAlign: 'center', mt: 8 }}>
              <Button
                variant="outlined"
                size="large"
                sx={{
                  borderColor: palette.neutral[900],
                  color: palette.neutral[900],
                  px: 6,
                  py: 1.5,
                  textTransform: 'none',
                  borderRadius: 0,
                  fontWeight: 500,
                  '&:hover': {
                    borderColor: palette.neutral[900],
                    bgcolor: 'rgba(28,25,23,0.05)',
                  },
                }}
              >
                Load More
              </Button>
            </Box>
          </Grid>
        </Grid>
      </Container>

      <Drawer
        anchor="left"
        open={filterDrawerOpen}
        onClose={() => setFilterDrawerOpen(false)}
        slotProps={{
          paper: { sx: { width: 320, bgcolor: palette.neutral[50] } },
        }}
      >
        {filterContent}
        <Box sx={{ p: 3, borderTop: '1px solid rgba(68,64,60,0.1)' }}>
          <Button
            fullWidth
            variant="contained"
            onClick={() => setFilterDrawerOpen(false)}
            sx={{
              bgcolor: palette.neutral[900],
              color: palette.neutral[50],
              py: 1.5,
              textTransform: 'none',
              borderRadius: 0,
              '&:hover': { bgcolor: palette.neutral[700] },
            }}
          >
            Apply Filters
          </Button>
        </Box>
      </Drawer>
    </Box>
  )
}
