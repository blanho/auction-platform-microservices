import { useState, useEffect, useRef, useCallback } from 'react'
import { useSearchParams, Link } from 'react-router-dom'
import {
  Box,
  Container,
  Typography,
  TextField,
  InputAdornment,
  Card,
  Grid,
  Button,
  Chip,
  Skeleton,
  Pagination,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Slider,
  Divider,
  IconButton,
  Autocomplete,
  Paper,
} from '@mui/material'
import { Search, FilterList, Close, TrendingUp, History, Clear } from '@mui/icons-material'
import {
  useSearch,
  useSearchSuggestions,
  usePopularSearches,
  useRecentSearches,
  useClearRecentSearches,
} from '../hooks'
import type { SearchFilters, SearchResult } from '../types'
import { getResultIcon } from '../utils'
import { formatCurrency } from '@/shared/utils/formatters'
import { palette } from '@/shared/theme/tokens'

const SearchResultCard = ({ result }: { result: SearchResult }) => (
  <Card
    component={Link}
    to={result.type === 'auction' ? `/auctions/${result.id}` : `/categories/${result.id}`}
    sx={{
      display: 'flex',
      p: 2,
      textDecoration: 'none',
      borderRadius: 2,
      boxShadow: '0 2px 8px rgba(0,0,0,0.06)',
      transition: 'all 0.2s',
      '&:hover': {
        boxShadow: '0 4px 16px rgba(0,0,0,0.1)',
        transform: 'translateY(-2px)',
      },
    }}
  >
    <Box
      sx={{
        width: 100,
        height: 100,
        borderRadius: 1,
        bgcolor: palette.neutral[100],
        backgroundImage: result.imageUrl ? `url(${result.imageUrl})` : 'none',
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        flexShrink: 0,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
      }}
    >
      {!result.imageUrl && getResultIcon(result.type)}
    </Box>
    <Box sx={{ ml: 2, flex: 1, minWidth: 0 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
        <Chip
          size="small"
          label={result.type}
          sx={{
            height: 20,
            fontSize: '0.75rem',
            textTransform: 'capitalize',
            bgcolor: result.type === 'auction' ? '#FEF3C7' : '#DBEAFE',
            color: result.type === 'auction' ? '#92400E' : '#1D4ED8',
          }}
        />
      </Box>
      <Typography
        sx={{
          fontWeight: 600,
          color: palette.neutral[900],
          mb: 0.5,
          overflow: 'hidden',
          textOverflow: 'ellipsis',
          whiteSpace: 'nowrap',
        }}
      >
        {result.title}
      </Typography>
      <Typography
        sx={{
          fontSize: '0.875rem',
          color: palette.neutral[500],
          overflow: 'hidden',
          textOverflow: 'ellipsis',
          display: '-webkit-box',
          WebkitLineClamp: 2,
          WebkitBoxOrient: 'vertical',
          mb: 1,
        }}
      >
        {result.description}
      </Typography>
      {result.price !== undefined && (
        <Typography sx={{ fontWeight: 700, color: palette.brand.primary }}>
          {formatCurrency(result.price)}
        </Typography>
      )}
    </Box>
  </Card>
)

export function SearchPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const [inputValue, setInputValue] = useState(searchParams.get('q') || '')
  const inputRef = useRef<HTMLInputElement>(null)
  const debounceTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const [filters, setFilters] = useState<SearchFilters>({
    query: searchParams.get('q') || '',
    types: (searchParams.get('types')?.split(',') as SearchResultType[]) || undefined,
    categoryId: searchParams.get('category') || undefined,
    minPrice: searchParams.get('minPrice') ? Number(searchParams.get('minPrice')) : undefined,
    maxPrice: searchParams.get('maxPrice') ? Number(searchParams.get('maxPrice')) : undefined,
    sortBy: (searchParams.get('sortBy') as SearchFilters['sortBy']) || 'relevance',
    page: Number(searchParams.get('page')) || 1,
    pageSize: 20,
  })

  const [priceRange, setPriceRange] = useState<number[]>([
    filters.minPrice || 0,
    filters.maxPrice || 10000,
  ])

  const { data: searchResults, isLoading, error } = useSearch(filters, !!filters.query)
  const { data: suggestions } = useSearchSuggestions(
    inputValue,
    inputValue.length >= 2 && inputValue !== filters.query
  )
  const { data: popularSearches } = usePopularSearches()
  const { data: recentSearches } = useRecentSearches()
  const clearRecentSearches = useClearRecentSearches()

  const updateSearchParams = useCallback(
    (newFilters: SearchFilters) => {
      const params = new URLSearchParams()
      if (newFilters.query) {params.set('q', newFilters.query)}
      if (newFilters.types?.length) {params.set('types', newFilters.types.join(','))}
      if (newFilters.categoryId) {params.set('category', newFilters.categoryId)}
      if (newFilters.minPrice) {params.set('minPrice', newFilters.minPrice.toString())}
      if (newFilters.maxPrice) {params.set('maxPrice', newFilters.maxPrice.toString())}
      if (newFilters.sortBy && newFilters.sortBy !== 'relevance')
        {params.set('sortBy', newFilters.sortBy)}
      if (newFilters.page && newFilters.page > 1) {params.set('page', newFilters.page.toString())}
      setSearchParams(params)
    },
    [setSearchParams]
  )

  const debouncedSearch = useCallback(
    (query: string) => {
      if (debounceTimerRef.current) {
        clearTimeout(debounceTimerRef.current)
      }
      debounceTimerRef.current = setTimeout(() => {
        const newFilters = { ...filters, query, page: 1 }
        setFilters(newFilters)
        updateSearchParams(newFilters)
      }, 500)
    },
    [filters, updateSearchParams]
  )

  useEffect(() => {
    return () => {
      if (debounceTimerRef.current) {
        clearTimeout(debounceTimerRef.current)
      }
    }
  }, [])

  const handleSearch = (query: string) => {
    setInputValue(query)
    const newFilters = { ...filters, query, page: 1 }
    setFilters(newFilters)
    updateSearchParams(newFilters)
  }

  const handleFilterChange = (key: keyof SearchFilters, value: unknown) => {
    const newFilters = { ...filters, [key]: value, page: 1 }
    setFilters(newFilters)
    updateSearchParams(newFilters)
  }

  const handlePriceRangeCommit = () => {
    const newFilters = {
      ...filters,
      minPrice: priceRange[0] || undefined,
      maxPrice: priceRange[1] < 10000 ? priceRange[1] : undefined,
      page: 1,
    }
    setFilters(newFilters)
    updateSearchParams(newFilters)
  }

  const clearFilters = () => {
    const newFilters: SearchFilters = {
      query: filters.query,
      page: 1,
      pageSize: 20,
      sortBy: 'relevance',
    }
    setFilters(newFilters)
    setPriceRange([0, 10000])
    updateSearchParams(newFilters)
  }

  const hasActiveFilters =
    filters.types?.length ||
    filters.categoryId ||
    filters.minPrice ||
    filters.maxPrice ||
    filters.sortBy !== 'relevance'

  useEffect(() => {
    if (!filters.query) {
      inputRef.current?.focus()
    }
  }, [filters.query])

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Autocomplete
          freeSolo
          inputValue={inputValue}
          onInputChange={(_, value) => {
            setInputValue(value)
            if (value.length >= 2) {
              debouncedSearch(value)
            }
          }}
          options={suggestions?.map((s) => s.text) || []}
          renderInput={(params) => (
            <TextField
              {...params}
              inputRef={inputRef}
              placeholder="Search for auctions, categories, sellers..."
              InputProps={{
                ...params.InputProps,
                startAdornment: (
                  <InputAdornment position="start">
                    <Search sx={{ color: palette.neutral[500] }} />
                  </InputAdornment>
                ),
                endAdornment: inputValue && (
                  <InputAdornment position="end">
                    <IconButton size="small" onClick={() => handleSearch('')}>
                      <Close fontSize="small" />
                    </IconButton>
                  </InputAdornment>
                ),
              }}
              sx={{
                '& .MuiOutlinedInput-root': {
                  bgcolor: 'white',
                  borderRadius: 2,
                  '&:hover .MuiOutlinedInput-notchedOutline': {
                    borderColor: palette.brand.primary,
                  },
                  '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
                    borderColor: palette.brand.primary,
                  },
                },
              }}
            />
          )}
          onKeyDown={(e) => {
            if (e.key === 'Enter') {
              handleSearch(inputValue)
            }
          }}
        />
      </Box>

      {!filters.query && (
        <Box sx={{ mb: 4 }}>
          {recentSearches && recentSearches.length > 0 && (
            <Box sx={{ mb: 3 }}>
              <Box
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  mb: 1.5,
                }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <History sx={{ fontSize: 20, color: palette.neutral[500] }} />
                  <Typography sx={{ fontWeight: 500, color: palette.neutral[700] }}>
                    Recent Searches
                  </Typography>
                </Box>
                <Button
                  size="small"
                  startIcon={<Clear />}
                  onClick={() => clearRecentSearches.mutate()}
                  sx={{ color: palette.neutral[500], textTransform: 'none' }}
                >
                  Clear
                </Button>
              </Box>
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                {recentSearches.map((search) => (
                  <Chip
                    key={search}
                    label={search}
                    onClick={() => handleSearch(search)}
                    sx={{
                      bgcolor: palette.neutral[100],
                      '&:hover': { bgcolor: '#E5E5E5' },
                    }}
                  />
                ))}
              </Box>
            </Box>
          )}

          {popularSearches && popularSearches.length > 0 && (
            <Box>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1.5 }}>
                <TrendingUp sx={{ fontSize: 20, color: palette.brand.primary }} />
                <Typography sx={{ fontWeight: 500, color: palette.neutral[700] }}>
                  Trending Searches
                </Typography>
              </Box>
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                {popularSearches.map((search) => (
                  <Chip
                    key={search}
                    label={search}
                    onClick={() => handleSearch(search)}
                    sx={{
                      bgcolor: '#FEF3C7',
                      color: '#92400E',
                      '&:hover': { bgcolor: '#FDE68A' },
                    }}
                  />
                ))}
              </Box>
            </Box>
          )}
        </Box>
      )}

      {filters.query && (
        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 3 }}>
            <Paper
              sx={{
                p: 3,
                borderRadius: 2,
                boxShadow: '0 2px 8px rgba(0,0,0,0.06)',
                position: { md: 'sticky' },
                top: { md: 90 },
              }}
            >
              <Box
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  mb: 3,
                }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <FilterList sx={{ color: palette.brand.primary }} />
                  <Typography sx={{ fontWeight: 600, color: palette.neutral[900] }}>
                    Filters
                  </Typography>
                </Box>
                {hasActiveFilters && (
                  <Button
                    size="small"
                    onClick={clearFilters}
                    sx={{ color: palette.neutral[500], textTransform: 'none' }}
                  >
                    Clear all
                  </Button>
                )}
              </Box>

              <Box sx={{ mb: 3 }}>
                <Typography sx={{ fontWeight: 500, color: palette.neutral[700], mb: 1.5 }}>
                  Type
                </Typography>
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                  {['auction', 'category', 'seller'].map((type) => (
                    <Chip
                      key={type}
                      label={type}
                      onClick={() => {
                        const types = filters.types || []
                        const newTypes = types.includes(type as SearchResultType)
                          ? types.filter((t) => t !== type)
                          : [...types, type as SearchResultType]
                        handleFilterChange('types', newTypes.length ? newTypes : undefined)
                      }}
                      sx={{
                        textTransform: 'capitalize',
                        bgcolor: filters.types?.includes(type as SearchResultType)
                          ? palette.neutral[900]
                          : palette.neutral[100],
                        color: filters.types?.includes(type as SearchResultType)
                          ? 'white'
                          : palette.neutral[700],
                        '&:hover': {
                          bgcolor: filters.types?.includes(type as SearchResultType)
                            ? palette.neutral[700]
                            : '#E5E5E5',
                        },
                      }}
                    />
                  ))}
                </Box>
              </Box>

              <Divider sx={{ my: 2 }} />

              <Box sx={{ mb: 3 }}>
                <Typography sx={{ fontWeight: 500, color: palette.neutral[700], mb: 2 }}>
                  Price Range
                </Typography>
                <Slider
                  value={priceRange}
                  onChange={(_, value) => setPriceRange(value as number[])}
                  onChangeCommitted={handlePriceRangeCommit}
                  valueLabelDisplay="auto"
                  valueLabelFormat={(v) => formatCurrency(v)}
                  min={0}
                  max={10000}
                  step={100}
                  sx={{
                    color: palette.brand.primary,
                    '& .MuiSlider-thumb': {
                      bgcolor: 'white',
                      border: `2px solid ${palette.brand.primary}`,
                    },
                  }}
                />
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography sx={{ fontSize: '0.875rem', color: palette.neutral[500] }}>
                    {formatCurrency(priceRange[0])}
                  </Typography>
                  <Typography sx={{ fontSize: '0.875rem', color: palette.neutral[500] }}>
                    {priceRange[1] >= 10000 ? '$10,000+' : formatCurrency(priceRange[1])}
                  </Typography>
                </Box>
              </Box>

              <Divider sx={{ my: 2 }} />

              <FormControl fullWidth size="small">
                <InputLabel>Sort By</InputLabel>
                <Select
                  value={filters.sortBy || 'relevance'}
                  onChange={(e) => handleFilterChange('sortBy', e.target.value)}
                  label="Sort By"
                >
                  <MenuItem value="relevance">Relevance</MenuItem>
                  <MenuItem value="price-low">Price: Low to High</MenuItem>
                  <MenuItem value="price-high">Price: High to Low</MenuItem>
                  <MenuItem value="newest">Newest</MenuItem>
                  <MenuItem value="ending-soon">Ending Soon</MenuItem>
                </Select>
              </FormControl>
            </Paper>
          </Grid>

          <Grid size={{ xs: 12, md: 9 }}>
            {error && (
              <InlineAlert severity="error" sx={{ mb: 3 }}>
                Failed to load search results. Please try again.
              </InlineAlert>
            )}

            <Box
              sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}
            >
              <Typography sx={{ color: palette.neutral[500] }}>
                {isLoading && (
                  <Skeleton width={200} />
                )}
                {!isLoading && (
                  `${searchResults?.totalCount || 0} results for "${filters.query}"`
                )}
              </Typography>
            </Box>

            {isLoading && (
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                {Array.from({ length: 5 }).map((_, i) => (
                  <Skeleton key={i} variant="rectangular" height={120} sx={{ borderRadius: 2 }} />
                ))}
              </Box>
            )}
            {!isLoading && searchResults?.results?.length === 0 && (
              <Box sx={{ textAlign: 'center', py: 8 }}>
                <Search sx={{ fontSize: 64, color: '#D4D4D4', mb: 2 }} />
                <Typography variant="h6" sx={{ color: palette.neutral[500], mb: 1 }}>
                  No results found
                </Typography>
                <Typography sx={{ color: '#A1A1AA', mb: 3 }}>
                  Try adjusting your search or filters
                </Typography>
                <Button
                  variant="outlined"
                  onClick={clearFilters}
                  sx={{
                    borderColor: '#E5E5E5',
                    color: palette.neutral[700],
                    textTransform: 'none',
                    '&:hover': { borderColor: palette.neutral[900] },
                  }}
                >
                  Clear filters
                </Button>
              </Box>
            )}
            {!isSearching && searchResults?.results && searchResults.results.length > 0 && (
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                {searchResults?.results?.map((result) => (
                  <SearchResultCard key={result.id} result={result} />
                ))}
              </Box>
            )}

            {searchResults && searchResults.totalPages > 1 && (
              <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
                <Pagination
                  count={searchResults.totalPages}
                  page={filters.page || 1}
                  onChange={(_, page) => handleFilterChange('page', page)}
                  color="primary"
                />
              </Box>
            )}
          </Grid>
        </Grid>
      )}
    </Container>
  )
}
