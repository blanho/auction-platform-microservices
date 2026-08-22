import { Close, Refresh, Search } from '@mui/icons-material'
import type { SelectChangeEvent, SxProps, Theme } from '@mui/material'
import {
  Box,
  Button,
  FormControl,
  IconButton,
  InputAdornment,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
  Tooltip,
} from '@mui/material'
import { useCallback } from 'react'
import { useTranslation } from 'react-i18next'

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
  const { t } = useTranslation('common')

  const handleSearchChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      onSearchChange?.(e.target.value)
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
    searchValue || Object.values(filterValues).some((v) => v !== '' && v !== undefined)

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
          value={searchValue}
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
            <MenuItem value="">{t('table.all')}</MenuItem>
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
            <Button variant="outlined" size="small" startIcon={<Close />} onClick={onClearFilters}>
              {t('table.clear')}
            </Button>
          )}
          {showRefreshButton && onRefresh && (
            <Tooltip title={t('table.refresh')}>
              <IconButton
                onClick={onRefresh}
                color="primary"
                size="small"
                aria-label={t('table.refresh')}
              >
                <Refresh />
              </IconButton>
            </Tooltip>
          )}
        </Box>
      )}
    </Stack>
  )
}
