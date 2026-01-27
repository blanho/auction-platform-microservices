import { useState, useCallback, useRef, useEffect } from 'react'
import {
  Autocomplete,
  TextField,
  Box,
  Typography,
  Paper,
  Chip,
  InputAdornment,
  IconButton,
  CircularProgress,
  Divider,
  useTheme,
  alpha,
} from '@mui/material'
import {
  Search as SearchIcon,
  Clear as ClearIcon,
  TrendingUp as TrendingIcon,
  History as HistoryIcon,
  Category as CategoryIcon,
  Gavel as AuctionIcon,
} from '@mui/icons-material'
import { useQuery } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { AnimatePresence, motion } from 'framer-motion'
import { searchApi } from '@/modules/search/api/search.api'
import type { SearchSuggestion } from '@/modules/search/types'

interface SearchAutocompleteProps {
  placeholder?: string
  size?: 'small' | 'medium'
  variant?: 'standard' | 'outlined' | 'filled'
  fullWidth?: boolean
  onSearch?: (query: string) => void
  defaultValue?: string
}

interface GroupedOption {
  type: 'recent' | 'trending' | 'suggestion'
  label: string
  suggestion?: SearchSuggestion
}

const DEBOUNCE_DELAY = 300

export function SearchAutocomplete({
  placeholder = 'Search auctions, categories, sellers...',
  size = 'medium',
  variant = 'outlined',
  fullWidth = true,
  onSearch,
  defaultValue = '',
}: SearchAutocompleteProps) {
  const theme = useTheme()
  const navigate = useNavigate()
  const [inputValue, setInputValue] = useState(defaultValue)
  const [debouncedQuery, setDebouncedQuery] = useState('')
  const [isOpen, setIsOpen] = useState(false)
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  useEffect(() => {
    if (debounceRef.current) {
      clearTimeout(debounceRef.current)
    }

    if (inputValue.trim()) {
      debounceRef.current = setTimeout(() => {
        setDebouncedQuery(inputValue.trim())
      }, DEBOUNCE_DELAY)
    }

    return () => {
      if (debounceRef.current) {
        clearTimeout(debounceRef.current)
      }
    }
  }, [inputValue])

  const { data: suggestions = [], isLoading: suggestionsLoading } = useQuery({
    queryKey: ['search', 'suggestions', debouncedQuery],
    queryFn: (): Promise<SearchSuggestion[]> => searchApi.getSuggestions(debouncedQuery),
    enabled: debouncedQuery.length >= 2,
    staleTime: 30 * 1000,
  })

  const { data: recentSearches = [] } = useQuery({
    queryKey: ['search', 'recent'],
    queryFn: (): Promise<string[]> => searchApi.getRecentSearches(),
    staleTime: 5 * 60 * 1000,
  })

  const { data: popularSearches = [] } = useQuery({
    queryKey: ['search', 'popular'],
    queryFn: (): Promise<string[]> => searchApi.getPopularSearches(),
    staleTime: 10 * 60 * 1000,
  })

  const getOptions = useCallback((): GroupedOption[] => {
    if (debouncedQuery.length >= 2 && suggestions.length > 0) {
      return suggestions.map(s => ({
        type: 'suggestion' as const,
        label: s.text,
        suggestion: s,
      }))
    }

    const options: GroupedOption[] = []

    if (recentSearches.length > 0) {
      recentSearches.slice(0, 3).forEach(search => {
        options.push({ type: 'recent', label: search })
      })
    }

    if (popularSearches.length > 0) {
      popularSearches.slice(0, 5).forEach(search => {
        options.push({ type: 'trending', label: search })
      })
    }

    return options
  }, [debouncedQuery, suggestions, recentSearches, popularSearches])

  const handleSearch = useCallback((query: string) => {
    if (!query.trim()) return
    
    if (onSearch) {
      onSearch(query)
    } else {
      navigate(`/search?q=${encodeURIComponent(query)}`)
    }
    setIsOpen(false)
  }, [navigate, onSearch])

  const handleOptionSelect = useCallback((_: unknown, option: GroupedOption | string | null) => {
    if (!option) return
    
    const query = typeof option === 'string' ? option : option.label
    handleSearch(query)
  }, [handleSearch])

  const handleKeyDown = useCallback((event: React.KeyboardEvent) => {
    if (event.key === 'Enter' && inputValue.trim()) {
      handleSearch(inputValue)
    }
  }, [inputValue, handleSearch])

  const getOptionIcon = (option: GroupedOption) => {
    if (option.type === 'recent') {
      return <HistoryIcon fontSize="small" sx={{ color: 'text.secondary' }} />
    }
    if (option.type === 'trending') {
      return <TrendingIcon fontSize="small" sx={{ color: 'warning.main' }} />
    }
    if (option.suggestion?.type === 'category') {
      return <CategoryIcon fontSize="small" sx={{ color: 'primary.main' }} />
    }
    if (option.suggestion?.type === 'auction') {
      return <AuctionIcon fontSize="small" sx={{ color: 'secondary.main' }} />
    }
    return <SearchIcon fontSize="small" sx={{ color: 'text.secondary' }} />
  }

  const groupBy = (option: GroupedOption) => {
    if (option.type === 'recent') return 'Recent Searches'
    if (option.type === 'trending') return 'Trending'
    return 'Suggestions'
  }

  return (
    <Autocomplete<GroupedOption, false, false, true>
      freeSolo
      open={isOpen}
      onOpen={() => setIsOpen(true)}
      onClose={() => setIsOpen(false)}
      options={getOptions()}
      getOptionLabel={(option) => typeof option === 'string' ? option : option.label}
      groupBy={groupBy}
      onChange={handleOptionSelect}
      inputValue={inputValue}
      onInputChange={(_, value) => setInputValue(value)}
      fullWidth={fullWidth}
      loading={suggestionsLoading}
      filterOptions={(x) => x}
      isOptionEqualToValue={(option, value) => option.label === value.label}
      PaperComponent={(props) => (
        <Paper
          {...props}
          elevation={8}
          sx={{
            borderRadius: 2,
            mt: 0.5,
            overflow: 'hidden',
          }}
        />
      )}
      renderGroup={(params) => (
        <Box key={params.key}>
          <Typography
            variant="overline"
            sx={{
              px: 2,
              py: 1,
              display: 'block',
              color: 'text.secondary',
              backgroundColor: alpha(theme.palette.primary.main, 0.04),
            }}
          >
            {params.group}
          </Typography>
          {params.children}
          <Divider />
        </Box>
      )}
      renderOption={({ key, ...props }, option) => (
        <Box
          component="li"
          key={key}
          {...props}
          sx={{
            display: 'flex',
            alignItems: 'center',
            gap: 1.5,
            px: 2,
            py: 1,
            cursor: 'pointer',
            '&:hover': {
              backgroundColor: alpha(theme.palette.primary.main, 0.08),
            },
          }}
        >
          <Box sx={{ color: 'text.secondary', display: 'flex' }}>
            {getOptionIcon(option)}
          </Box>
          <Box sx={{ flex: 1 }}>
            <Typography variant="body2">{option.label}</Typography>
            {option.suggestion?.type === 'category' && (
              <Typography variant="caption" color="text.secondary">
                Category
              </Typography>
            )}
          </Box>
          {option.suggestion?.metadata?.count && (
            <Chip
              size="small"
              label={option.suggestion.metadata.count}
              sx={{ height: 20, fontSize: '0.7rem' }}
            />
          )}
        </Box>
      )}
      renderInput={(params) => (
        <TextField
          {...params}
          placeholder={placeholder}
          size={size}
          variant={variant}
          onKeyDown={handleKeyDown}
          slotProps={{
            input: {
              ...params.InputProps,
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon sx={{ color: 'text.secondary' }} />
                </InputAdornment>
              ),
              endAdornment: (
                <>
                  <AnimatePresence>
                    {suggestionsLoading && (
                      <motion.div
                        initial={{ opacity: 0, scale: 0.8 }}
                        animate={{ opacity: 1, scale: 1 }}
                        exit={{ opacity: 0, scale: 0.8 }}
                      >
                        <CircularProgress size={20} sx={{ mr: 1 }} />
                      </motion.div>
                    )}
                  </AnimatePresence>
                  {inputValue && (
                    <InputAdornment position="end">
                      <IconButton
                        size="small"
                        onClick={() => setInputValue('')}
                        sx={{ 
                          cursor: 'pointer',
                          '&:hover': { color: 'error.main' },
                          transition: 'color 0.15s ease-out'
                        }}
                      >
                        <ClearIcon fontSize="small" />
                      </IconButton>
                    </InputAdornment>
                  )}
                  {params.InputProps.endAdornment}
                </>
              ),
            },
          }}
          sx={{
            '& .MuiOutlinedInput-root': {
              borderRadius: 2,
              transition: 'box-shadow 0.2s ease-out',
              '&:hover': {
                boxShadow: `0 0 0 2px ${alpha(theme.palette.primary.main, 0.1)}`,
              },
              '&.Mui-focused': {
                boxShadow: `0 0 0 3px ${alpha(theme.palette.primary.main, 0.2)}`,
              },
            },
          }}
        />
      )}
    />
  )
}
