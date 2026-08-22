import { componentStyles } from '@/shared/theme/component-styles'
import { palette } from '@/shared/theme/tokens'
import { ErrorState } from '@/shared/ui'
import { FilterList, GridView, KeyboardArrowDown, ViewList } from '@mui/icons-material'
import {
  Box,
  Breadcrumbs,
  Button,
  Chip,
  Container,
  Drawer,
  FormControl,
  Grid,
  IconButton,
  MenuItem,
  Link as MuiLink,
  Pagination,
  Select,
  Typography,
} from '@mui/material'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { AuctionBrowseFilters } from '../components/AuctionBrowseFilters'
import { AuctionProductCard, AuctionProductCardSkeleton } from '../components/AuctionProductCard'
import { AUCTION_SORT_CONFIG, type AuctionSortOption } from '../constants'
import { useActiveCategories, useAuctions, useToggleWatchlist, useWatchlist } from '../hooks'

const PAGE_SIZE = 24
const SKELETON_COUNT = 12

export const AuctionListingPage = () => {
  const { t } = useTranslation('auctions')
  const [filterDrawerOpen, setFilterDrawerOpen] = useState(false)
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid')
  const [selectedCategoryId, setSelectedCategoryId] = useState<string>()
  const [sortBy, setSortBy] = useState<AuctionSortOption>('ending-soon')
  const [page, setPage] = useState(1)

  const { data: categories = [] } = useActiveCategories()
  const selectedCategory = categories.find((category) => category.id === selectedCategoryId)
  const sort = AUCTION_SORT_CONFIG[sortBy]

  const { data, isLoading, isError, refetch } = useAuctions({
    category: selectedCategory?.name,
    orderBy: sort.orderBy,
    descending: sort.descending,
    page,
    pageSize: PAGE_SIZE,
  })

  const { data: watchlist = [] } = useWatchlist({ pageSize: 100, status: 'all' })
  const watchedAuctionIds = useMemo(
    () => new Set(watchlist.map((item) => item.auctionId)),
    [watchlist]
  )
  const toggleWatchlistMutation = useToggleWatchlist()

  const sortOptions: { value: AuctionSortOption; label: string }[] = [
    { value: 'ending-soon', label: t('sort.endingSoon') },
    { value: 'newly-listed', label: t('sort.newest') },
    { value: 'price-low', label: t('sort.priceLowToHigh') },
    { value: 'price-high', label: t('sort.priceHighToLow') },
  ]

  const handleCategoryChange = (categoryId?: string) => {
    setSelectedCategoryId(categoryId)
    setPage(1)
  }

  const filters = (
    <AuctionBrowseFilters
      categories={categories}
      selectedCategoryId={selectedCategoryId}
      onCategoryChange={handleCategoryChange}
    />
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
            {t('common:nav.home')}
          </MuiLink>
          <Typography sx={{ color: palette.neutral[900] }}>{t('browse')}</Typography>
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
            {t('browse')}
          </Typography>
          <Typography variant="body1" sx={{ color: palette.neutral[500] }}>
            {t('itemsAvailable', { count: data?.totalCount ?? 0 })}
          </Typography>
        </Box>

        <Grid container spacing={4}>
          <Grid size={{ xs: 12, md: 3 }} sx={{ display: { xs: 'none', md: 'block' } }}>
            <Box
              sx={{
                position: 'sticky',
                top: 100,
                bgcolor: palette.neutral[0],
                border: '1px solid rgba(68,64,60,0.1)',
              }}
            >
              {filters}
            </Box>
          </Grid>

          <Grid size={{ xs: 12, md: 9 }}>
            <Box
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                gap: 2,
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
                  {t('filter.title')}
                </Button>
                {selectedCategory && (
                  <Chip
                    label={selectedCategory.name}
                    size="small"
                    onDelete={() => handleCategoryChange(undefined)}
                    sx={componentStyles.brandChip}
                  />
                )}
              </Box>

              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Box sx={{ display: { xs: 'none', sm: 'flex' }, gap: 0.5 }}>
                  <IconButton
                    size="small"
                    onClick={() => setViewMode('grid')}
                    aria-label={t('filter.gridView')}
                    aria-pressed={viewMode === 'grid'}
                    sx={{
                      color: viewMode === 'grid' ? palette.neutral[900] : palette.neutral[500],
                    }}
                  >
                    <GridView fontSize="small" />
                  </IconButton>
                  <IconButton
                    size="small"
                    onClick={() => setViewMode('list')}
                    aria-label={t('filter.listView')}
                    aria-pressed={viewMode === 'list'}
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
                    onChange={(event) => {
                      setSortBy(event.target.value as AuctionSortOption)
                      setPage(1)
                    }}
                    IconComponent={KeyboardArrowDown}
                    inputProps={{ 'aria-label': t('filter.sortBy') }}
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

            {isError ? (
              <ErrorState onRetry={() => void refetch()} />
            ) : (
              <Grid container spacing={3}>
                {isLoading
                  ? Array.from({ length: SKELETON_COUNT }, (_, index) => (
                      <Grid size={{ xs: 6, sm: 4, lg: 3 }} key={index}>
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
                          isFavorited={watchedAuctionIds.has(auction.id)}
                          onFavoriteToggle={() =>
                            toggleWatchlistMutation.mutate({
                              auctionId: auction.id,
                              isInWatchlist: watchedAuctionIds.has(auction.id),
                            })
                          }
                        />
                      </Grid>
                    ))}
              </Grid>
            )}

            {(data?.totalPages ?? 0) > 1 && (
              <Pagination
                page={page}
                count={data?.totalPages ?? 1}
                onChange={(_, nextPage) => setPage(nextPage)}
                sx={{ display: 'flex', justifyContent: 'center', mt: 6 }}
              />
            )}
          </Grid>
        </Grid>
      </Container>

      <Drawer
        anchor="left"
        open={filterDrawerOpen}
        onClose={() => setFilterDrawerOpen(false)}
        slotProps={{ paper: { sx: { width: 320, bgcolor: palette.neutral[50] } } }}
      >
        <AuctionBrowseFilters
          categories={categories}
          selectedCategoryId={selectedCategoryId}
          onCategoryChange={(categoryId) => {
            handleCategoryChange(categoryId)
            setFilterDrawerOpen(false)
          }}
          onClose={() => setFilterDrawerOpen(false)}
        />
      </Drawer>
    </Box>
  )
}
