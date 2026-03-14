import { useState, useCallback } from 'react'
import {
  Box,
  TextField,
  InputAdornment,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  IconButton,
  Button,
  Stack,
  Tooltip,
} from '@mui/material'
import type { SelectChangeEvent, SxProps, Theme } from '@mui/material'
import { Search, Close, Refresh } from '@mui/icons-material'

export interface FilterOption {
  value: string
  label: string
}

export interface FilterConfig {
  key: string
  label: string
  options: FilterOption[]
  minWidth?: number
}

export interface TableToolbarProps {
  searchValue?: string
  searchPlaceholder?: string
  onSearchChange?: (value: string) => void
  filters?: FilterConfig[]
  filterValues?: Record<string, string>
  onFilterChange?: (key: string, value: string) => void
  onClearFilters?: () => void
  onRefresh?: () => void
  showClearButton?: boolean
  showRefreshButton?: boolean
  direction?: 'row' | 'column'
  spacing?: number
  sx?: SxProps<Theme>
  children?: React.ReactNode
}

export function TableToolbar({
  searchValue = '',
  searchPlaceholder = 'Search...',
  onSearchChange,
  filters = [],
  filterValues = {},
  onFilterChange,
  onClearFilters,
  onRefresh,
  showClearButton = true,
  showRefreshButton = true,
  direction = 'row',
  spacing = 2,
  sx,
  children,
}: TableToolbarProps) {
  const [localSearch, setLocalSearch] = useState(searchValue)

  const handleSearchChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const value = e.target.value
      setLocalSearch(value)
      onSearchChange?.(value)
    },
    [onSearchChange]
  )

  const handleFilterChange = useCallback(
    (key: string) => (e: SelectChangeEvent<string>) => {
      onFilterChange?.(key, e.target.value)
    },
    [onFilterChange]
  )

  const hasActiveFilters =
    localSearch ||
    Object.values(filterValues).some((v) => v !== '' && v !== undefined)

  return (
    <Stack
      direction={{ xs: 'column', md: direction }}
      spacing={spacing}
      alignItems={{ xs: 'stretch', md: 'center' }}
      sx={sx}
    >
      {onSearchChange && (
        <TextField
          placeholder={searchPlaceholder}
          value={localSearch}
          onChange={handleSearchChange}
          size="small"
          sx={{ minWidth: 280 }}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <Search sx={{ color: 'text.secondary' }} />
              </InputAdornment>
            ),
          }}
        />
      )}

      {filters.map((filter) => (
        <FormControl key={filter.key} size="small" sx={{ minWidth: filter.minWidth || 150 }}>
          <InputLabel>{filter.label}</InputLabel>
          <Select
            value={filterValues[filter.key] || ''}
            onChange={handleFilterChange(filter.key)}
            label={filter.label}
          >
            <MenuItem value="">All</MenuItem>
            {filter.options.map((option) => (
              <MenuItem key={option.value} value={option.value}>
                {option.label}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      ))}

      {children}

      {(showClearButton || showRefreshButton) && (
        <Box sx={{ display: 'flex', gap: 1, ml: { md: 'auto' } }}>
          {showClearButton && hasActiveFilters && onClearFilters && (
            <Button
              variant="outlined"
              size="small"
              startIcon={<Close />}
              onClick={onClearFilters}
            >
              Clear
            </Button>
          )}
          {showRefreshButton && onRefresh && (
            <Tooltip title="Refresh">
              <IconButton onClick={onRefresh} color="primary" size="small">
                <Refresh />
              </IconButton>
            </Tooltip>
          )}
        </Box>
      )}
    </Stack>
  )
}
