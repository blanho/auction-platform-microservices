import { useState, useCallback, useMemo } from 'react'
import {
  Box,
  Paper,
  Stack,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Button,
  IconButton,
  Typography,
  InputAdornment,
  Grid,
  Chip,
} from '@mui/material'
import type { SelectChangeEvent, SxProps, Theme } from '@mui/material'
import {
  Search,
  Close,
  FilterList,
  ExpandMore,
  ExpandLess,
  Refresh,
} from '@mui/icons-material'
import { motion, AnimatePresence } from 'framer-motion'
import type { FilterField, FilterPanelConfig } from '@/shared/types/filter.types'

export interface FilterPanelProps<TFilter extends Record<string, unknown>> {
  config: FilterPanelConfig
  value: TFilter
  onChange: (filter: TFilter) => void
  onClear?: () => void
  onRefresh?: () => void
  showRefreshButton?: boolean
  sx?: SxProps<Theme>
}

export function FilterPanel<TFilter extends Record<string, unknown>>({
  config,
  value,
  onChange,
  onClear,
  onRefresh,
  showRefreshButton = true,
  sx,
}: Readonly<FilterPanelProps<TFilter>>) {
  const [isExpanded, setIsExpanded] = useState(config.defaultExpanded ?? true)

  const activeFilterCount = useMemo(() => {
    return Object.entries(value).filter(
      ([_, v]) => v !== undefined && v !== null && v !== ''
    ).length
  }, [value])

  const handleFieldChange = useCallback(
    (key: string, fieldValue: unknown) => {
      onChange({ ...value, [key]: fieldValue } as TFilter)
    },
    [value, onChange]
  )

  const handleClearField = useCallback(
    (key: string) => {
      const newValue = { ...value }
      delete newValue[key]
      onChange(newValue)
    },
    [value, onChange]
  )

  const handleClearAll = useCallback(() => {
    onClear?.()
    onChange({} as TFilter)
  }, [onClear, onChange])

  const renderField = useCallback(
    (field: FilterField) => {
      const fieldValue = value[field.key]
      const gridSize = field.gridSize ?? { xs: 12, sm: 6, md: 4, lg: 3 }

      switch (field.type) {
        case 'text':
          return (
            <Grid size={gridSize} key={field.key}>
              <TextField
                fullWidth
                size="small"
                label={field.label}
                placeholder={field.placeholder}
                value={(fieldValue as string) ?? ''}
                onChange={(e) => handleFieldChange(field.key, e.target.value || undefined)}
                slotProps={{
                  input: {
                    startAdornment: (
                      <InputAdornment position="start">
                        <Search sx={{ color: 'text.secondary', fontSize: 20 }} />
                      </InputAdornment>
                    ),
                    endAdornment: fieldValue ? (
                      <InputAdornment position="end">
                        <IconButton
                          size="small"
                          onClick={() => handleClearField(field.key)}
                          edge="end"
                        >
                          <Close sx={{ fontSize: 18 }} />
                        </IconButton>
                      </InputAdornment>
                    ) : undefined,
                  },
                }}
              />
            </Grid>
          )

        case 'select':
          return (
            <Grid size={gridSize} key={field.key}>
              <FormControl fullWidth size="small">
                <InputLabel>{field.label}</InputLabel>
                <Select
                  value={(fieldValue as string) ?? ''}
                  label={field.label}
                  onChange={(e: SelectChangeEvent<string>) =>
                    handleFieldChange(field.key, e.target.value || undefined)
                  }
                  multiple={field.multiple}
                >
                  {field.clearable !== false && (
                    <MenuItem value="">
                      <em>All</em>
                    </MenuItem>
                  )}
                  {field.options.map((option) => (
                    <MenuItem
                      key={option.value}
                      value={option.value}
                      disabled={option.disabled}
                    >
                      {option.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
          )

        case 'date':
          return (
            <Grid size={gridSize} key={field.key}>
              <TextField
                fullWidth
                size="small"
                type="date"
                label={field.label}
                value={(fieldValue as string) ?? ''}
                onChange={(e) => handleFieldChange(field.key, e.target.value || undefined)}
                slotProps={{
                  inputLabel: { shrink: true },
                }}
              />
            </Grid>
          )

        case 'dateRange':
          return (
            <Grid size={{ xs: 12, sm: 6, md: 6, lg: 4 }} key={field.key}>
              <Stack direction="row" spacing={1} alignItems="center">
                <TextField
                  fullWidth
                  size="small"
                  type="date"
                  label={`${field.label} From`}
                  value={(value[field.startKey] as string) ?? ''}
                  onChange={(e) => handleFieldChange(field.startKey, e.target.value || undefined)}
                  slotProps={{
                    inputLabel: { shrink: true },
                  }}
                />
                <Typography variant="body2" color="text.secondary">
                  to
                </Typography>
                <TextField
                  fullWidth
                  size="small"
                  type="date"
                  label={`${field.label} To`}
                  value={(value[field.endKey] as string) ?? ''}
                  onChange={(e) => handleFieldChange(field.endKey, e.target.value || undefined)}
                  slotProps={{
                    inputLabel: { shrink: true },
                  }}
                />
              </Stack>
            </Grid>
          )

        case 'numberRange':
          return (
            <Grid size={{ xs: 12, sm: 6, md: 6, lg: 4 }} key={field.key}>
              <Stack direction="row" spacing={1} alignItems="center">
                <TextField
                  fullWidth
                  size="small"
                  type="number"
                  label={`Min ${field.label}`}
                  value={(value[field.minKey] as number) ?? ''}
                  onChange={(e) =>
                    handleFieldChange(
                      field.minKey,
                      e.target.value ? Number(e.target.value) : undefined
                    )
                  }
                  slotProps={{ htmlInput: { min: field.min, max: field.max, step: field.step } }}
                />
                <Typography variant="body2" color="text.secondary">
                  -
                </Typography>
                <TextField
                  fullWidth
                  size="small"
                  type="number"
                  label={`Max ${field.label}`}
                  value={(value[field.maxKey] as number) ?? ''}
                  onChange={(e) =>
                    handleFieldChange(
                      field.maxKey,
                      e.target.value ? Number(e.target.value) : undefined
                    )
                  }
                  slotProps={{ htmlInput: { min: field.min, max: field.max, step: field.step } }}
                />
              </Stack>
            </Grid>
          )

        case 'boolean': {
          let booleanValue = ''

          if (fieldValue === true) {
            booleanValue = 'true'
          } else if (fieldValue === false) {
            booleanValue = 'false'
          }

          return (
            <Grid size={gridSize} key={field.key}>
              <FormControl fullWidth size="small">
                <InputLabel>{field.label}</InputLabel>
                <Select
                  value={booleanValue}
                  label={field.label}
                  onChange={(e: SelectChangeEvent<string>) => {
                    const newValue = e.target.value
                    if (newValue === '') {
                      handleFieldChange(field.key, undefined)
                    } else {
                      handleFieldChange(field.key, newValue === 'true')
                    }
                  }}
                >
                  <MenuItem value="">
                    <em>All</em>
                  </MenuItem>
                  <MenuItem value="true">{field.trueLabel ?? 'Yes'}</MenuItem>
                  <MenuItem value="false">{field.falseLabel ?? 'No'}</MenuItem>
                </Select>
              </FormControl>
            </Grid>
          )
        }

        default:
          return null
      }
    },
    [value, handleFieldChange, handleClearField]
  )

  const filterContent = (
    <Grid container spacing={2}>
      {config.fields.map(renderField)}

      <Grid size={{ xs: 12, sm: 'auto' }}>
        <Stack direction="row" spacing={1} alignItems="center" height="100%">
          {(config.showClearButton ?? true) && activeFilterCount > 0 && (
            <Button
              variant="outlined"
              size="small"
              startIcon={<Close />}
              onClick={handleClearAll}
            >
              Clear ({activeFilterCount})
            </Button>
          )}
          {showRefreshButton && onRefresh && (
            <IconButton onClick={onRefresh} color="primary" size="small">
              <Refresh />
            </IconButton>
          )}
        </Stack>
      </Grid>
    </Grid>
  )

  if (!config.collapsible) {
    return (
      <Paper
        elevation={0}
        sx={{
          p: 2,
          border: '1px solid',
          borderColor: 'divider',
          borderRadius: 2,
          ...sx,
        }}
      >
        {filterContent}
      </Paper>
    )
  }

  return (
    <Paper
      elevation={0}
      sx={{
        border: '1px solid',
        borderColor: 'divider',
        borderRadius: 2,
        overflow: 'hidden',
        ...sx,
      }}
    >
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          px: 2,
          py: 1.5,
          cursor: 'pointer',
          bgcolor: 'background.default',
          '&:hover': {
            bgcolor: 'action.hover',
          },
          transition: 'background-color 0.2s',
        }}
        onClick={() => setIsExpanded(!isExpanded)}
      >
        <Stack direction="row" spacing={1} alignItems="center">
          <FilterList color="action" />
          <Typography variant="subtitle2">Filters</Typography>
          {activeFilterCount > 0 && (
            <Chip
              size="small"
              label={activeFilterCount}
              color="primary"
              sx={{ height: 20, fontSize: '0.75rem' }}
            />
          )}
        </Stack>
        <IconButton size="small" onClick={(e) => e.stopPropagation()}>
          {isExpanded ? <ExpandLess /> : <ExpandMore />}
        </IconButton>
      </Box>

      <AnimatePresence>
        {isExpanded && (
          <motion.div
            initial={{ height: 0, opacity: 0 }}
            animate={{ height: 'auto', opacity: 1 }}
            exit={{ height: 0, opacity: 0 }}
            transition={{ duration: 0.2 }}
          >
            <Box sx={{ p: 2, borderTop: '1px solid', borderColor: 'divider' }}>
              {filterContent}
            </Box>
          </motion.div>
        )}
      </AnimatePresence>
    </Paper>
  )
}
