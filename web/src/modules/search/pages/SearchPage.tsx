import { palette } from '@/shared/theme/tokens'
import { InlineAlert } from '@/shared/ui'
import { formatCurrency } from '@/shared/utils/formatters'
import { Clear, Close, FilterList, History, Search, TrendingUp } from '@mui/icons-material'
import {
  Autocomplete,
  Box,
  Button,
  Card,
  Chip,
  Container,
  Divider,
  FormControl,
  Grid,
  IconButton,
  InputAdornment,
  InputLabel,
  MenuItem,
  Pagination,
  Paper,
  Select,
  Skeleton,
  Slider,
  TextField,
  Typography,
} from '@mui/material'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useSearchParams } from 'react-router-dom'
import {
  useClearRecentSearches,
  usePopularSearches,
  useRecentSearches,
  useSearch,
  useSearchSuggestions,
} from '../hooks'
import type { SearchFilters, SearchResult, SearchResultType } from '../types'
import { getResultIcon } from '../utils'

const SEARCH_RESULT_TYPES: readonly SearchResultType[] = ['auction', 'category', 'seller']
const SEARCH_SORT_OPTIONS = [
  'relevance',
  'price-low',
  'price-high',
  'newest',
  'ending-soon',
] as const

function isSearchResultType(value: string): value is SearchResultType {
  return SEARCH_RESULT_TYPES.some((type) => type === value)
}

function parseResultTypes(value: string | null): SearchResultType[] | undefined {
  const resultTypes = value?.split(',').filter(isSearchResultType)
  return resultTypes?.length ? resultTypes : undefined
}

function parseOptionalNumber(value: string | null): number | undefined {
  if (!value) {
    return undefined
  }

  const parsedValue = Number(value)
  return Number.isFinite(parsedValue) ? parsedValue : undefined
}

function parseSortOption(value: string | null): string {
  return SEARCH_SORT_OPTIONS.find((option) => option === value) ?? 'relevance'
}

function getSearchResultPath(result: SearchResult): string {
  if (result.type === 'auction') {
    return `/auctions/${result.id}`
  }
  if (result.type === 'seller') {
    return `/sellers/${result.id}`
  }
  return `/categories/${result.id}`
}

const SearchResultCard = ({ result }: { result: SearchResult }) => {
  const { t } = useTranslation('search')

  return (
    <Card
      component={Link}
      to={getSearchResultPath(result)}
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
            label={t(`types.${result.type}`)}
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
}

export function SearchPage() {
  const { t } = useTranslation('search')
  const [searchParams, setSearchParams] = useSearchParams()
  const [inputValue, setInputValue] = useState(searchParams.get('q') || '')
  const inputRef = useRef<HTMLInputElement>(null)
  const debounceTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const [filters, setFilters] = useState<SearchFilters>({
    query: searchParams.get('q') || '',
    types: parseResultTypes(searchParams.get('types')),
    categoryId: searchParams.get('category') || undefined,
    minPrice: parseOptionalNumber(searchParams.get('minPrice')),
    maxPrice: parseOptionalNumber(searchParams.get('maxPrice')),
    sortBy: parseSortOption(searchParams.get('sortBy')),
    page: parseOptionalNumber(searchParams.get('page')) || 1,
    pageSize: 20,
  })

  const [priceRange, setPriceRange] = useState<number[]>([
    filters.minPrice || 0,
    filters.maxPrice || 10000,
  ])
  const skeletonKeys = useMemo(() => Array.from({ length: 5 }, () => crypto.randomUUID()), [])

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
      if (newFilters.query) {
        params.set('q', newFilters.query)
      }
      if (newFilters.types?.length) {
        params.set('types', newFilters.types.join(','))
      }
      if (newFilters.categoryId) {
        params.set('category', newFilters.categoryId)
      }
      if (newFilters.minPrice) {
        params.set('minPrice', newFilters.minPrice.toString())
      }
      if (newFilters.maxPrice) {
        params.set('maxPrice', newFilters.maxPrice.toString())
      }
      if (newFilters.sortBy && newFilters.sortBy !== 'relevance') {
        params.set('sortBy', newFilters.sortBy)
      }
      if (newFilters.page && newFilters.page > 1) {
        params.set('page', newFilters.page.toString())
      }
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
    <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
      <Box sx={{ mb: 4 }}>
        <Autocomplete
          freeSolo
          inputValue={inputValue}
          onInputChange={(_, value) => {
            setInputValue(value)
            if (value.length >= 2) {
              debouncedSearch(value)
            } else if (debounceTimerRef.current) {
              clearTimeout(debounceTimerRef.current)
              debounceTimerRef.current = null
            }
          }}
          options={suggestions?.map((s) => s.text) || []}
          renderInput={({ InputProps: autocompleteInputProps, ...params }) => (
            <TextField
              {...params}
              inputRef={inputRef}
              placeholder={t('placeholder')}
              slotProps={{
                input: {
                  ...autocompleteInputProps,
                  startAdornment: (
                    <InputAdornment position="start">
                      <Search sx={{ color: palette.neutral[500] }} />
                    </InputAdornment>
                  ),
                  endAdornment: inputValue && (
                    <InputAdornment position="end">
                      <IconButton
                        size="small"
                        onClick={() => handleSearch('')}
                        aria-label={t('common:actions.close')}
                      >
                        <Close fontSize="small" />
                      </IconButton>
                    </InputAdornment>
                  ),
                },
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
                    {t('autocomplete.recent')}
                  </Typography>
                </Box>
                <Button
                  size="small"
                  startIcon={<Clear />}
                  onClick={() => clearRecentSearches.mutate()}
                  sx={{ color: palette.neutral[500], textTransform: 'none' }}
                >
                  {t('autocomplete.clearRecent')}
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
                  {t('trendingSearches')}
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
                    {t('filters.title')}
                  </Typography>
                </Box>
                {hasActiveFilters && (
                  <Button
                    size="small"
                    onClick={clearFilters}
                    sx={{ color: palette.neutral[500], textTransform: 'none' }}
                  >
                    {t('filters.clearAll')}
                  </Button>
                )}
              </Box>

              <Box sx={{ mb: 3 }}>
                <Typography sx={{ fontWeight: 500, color: palette.neutral[700], mb: 1.5 }}>
                  {t('filters.type')}
                </Typography>
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                  {SEARCH_RESULT_TYPES.map((type) => (
                    <Chip
                      key={type}
                      label={t(`types.${type}`)}
                      onClick={() => {
                        const types = filters.types || []
                        const newTypes = types.includes(type)
                          ? types.filter((t) => t !== type)
                          : [...types, type]
                        handleFilterChange('types', newTypes.length ? newTypes : undefined)
                      }}
                      sx={{
                        textTransform: 'capitalize',
                        bgcolor: filters.types?.includes(type)
                          ? palette.neutral[900]
                          : palette.neutral[100],
                        color: filters.types?.includes(type) ? 'white' : palette.neutral[700],
                        '&:hover': {
                          bgcolor: filters.types?.includes(type) ? palette.neutral[700] : '#E5E5E5',
                        },
                      }}
                    />
                  ))}
                </Box>
              </Box>

              <Divider sx={{ my: 2 }} />

              <Box sx={{ mb: 3 }}>
                <Typography sx={{ fontWeight: 500, color: palette.neutral[700], mb: 2 }}>
                  {t('filters.priceRange')}
                </Typography>
                <Slider
                  value={priceRange}
                  onChange={(_, value) => setPriceRange(value)}
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
                    {priceRange[1] >= 10000
                      ? `${formatCurrency(10000)}+`
                      : formatCurrency(priceRange[1])}
                  </Typography>
                </Box>
              </Box>

              <Divider sx={{ my: 2 }} />

              <FormControl fullWidth size="small">
                <InputLabel>{t('filters.sortBy')}</InputLabel>
                <Select
                  value={filters.sortBy || 'relevance'}
                  onChange={(e) => handleFilterChange('sortBy', e.target.value)}
                  label={t('filters.sortBy')}
                >
                  <MenuItem value="relevance">{t('sort.relevance')}</MenuItem>
                  <MenuItem value="price-low">{t('sort.priceLowToHigh')}</MenuItem>
                  <MenuItem value="price-high">{t('sort.priceHighToLow')}</MenuItem>
                  <MenuItem value="newest">{t('sort.newest')}</MenuItem>
                  <MenuItem value="ending-soon">{t('sort.endingSoon')}</MenuItem>
                </Select>
              </FormControl>
            </Paper>
          </Grid>

          <Grid size={{ xs: 12, md: 9 }}>
            {error && (
              <InlineAlert severity="error" sx={{ mb: 3 }}>
                {t('loadFailed')}
              </InlineAlert>
            )}

            <Box
              sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}
            >
              <Typography sx={{ color: palette.neutral[500] }}>
                {isLoading && <Skeleton width={200} />}
                {!isLoading &&
                  t('resultsFor', {
                    count: searchResults?.totalCount || 0,
                    query: filters.query,
                  })}
              </Typography>
            </Box>

            {isLoading && (
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                {skeletonKeys.map((key) => (
                  <Skeleton key={key} variant="rectangular" height={120} sx={{ borderRadius: 2 }} />
                ))}
              </Box>
            )}
            {!isLoading && searchResults?.results?.length === 0 && (
              <Box sx={{ textAlign: 'center', py: 8 }}>
                <Search sx={{ fontSize: 64, color: '#D4D4D4', mb: 2 }} />
                <Typography variant="h6" sx={{ color: palette.neutral[500], mb: 1 }}>
                  {t('noResults')}
                </Typography>
                <Typography sx={{ color: '#A1A1AA', mb: 3 }}>
                  {t('noResultsDescription')}
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
                  {t('filters.clearAll')}
                </Button>
              </Box>
            )}
            {!isLoading && searchResults?.results && searchResults.results.length > 0 && (
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
