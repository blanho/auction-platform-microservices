import { useCallback, useMemo } from 'react'
import {
  Pagination as MuiPagination,
  Select,
  MenuItem,
  Typography,
  Stack,
  FormControl,
  InputLabel,
} from '@mui/material'
import type { SelectChangeEvent, SxProps, Theme } from '@mui/material'

export interface TablePaginationProps {
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  onPageChange: (page: number) => void
  onPageSizeChange?: (size: number) => void
  pageSizeOptions?: number[]
  showPageSizeSelector?: boolean
  showItemCount?: boolean
  showPageInfo?: boolean
  position?: 'left' | 'center' | 'right' | 'space-between'
  sx?: SxProps<Theme>
}

export function TablePagination({
  page,
  pageSize,
  totalCount,
  totalPages,
  onPageChange,
  onPageSizeChange,
  pageSizeOptions = [10, 25, 50, 100],
  showPageSizeSelector = true,
  showItemCount = true,
  showPageInfo = true,
  position = 'space-between',
  sx,
}: TablePaginationProps) {
  const handlePageChange = useCallback(
    (_event: React.ChangeEvent<unknown>, newPage: number) => {
      onPageChange(newPage)
    },
    [onPageChange]
  )

  const handlePageSizeChange = useCallback(
    (event: SelectChangeEvent<number>) => {
      onPageSizeChange?.(Number(event.target.value))
    },
    [onPageSizeChange]
  )

  const { startItem, endItem } = useMemo(() => {
    const start = totalCount > 0 ? (page - 1) * pageSize + 1 : 0
    const end = Math.min(page * pageSize, totalCount)
    return { startItem: start, endItem: end }
  }, [page, pageSize, totalCount])

  const justifyContent = useMemo(() => {
    switch (position) {
      case 'left':
        return 'flex-start'
      case 'center':
        return 'center'
      case 'right':
        return 'flex-end'
      case 'space-between':
      default:
        return 'space-between'
    }
  }, [position])

  if (totalCount === 0) {
    return null
  }

  return (
    <Stack
      direction={{ xs: 'column', sm: 'row' }}
      spacing={2}
      alignItems={{ xs: 'stretch', sm: 'center' }}
      justifyContent={justifyContent}
      sx={{
        py: 2,
        px: 2,
        borderTop: '1px solid',
        borderColor: 'divider',
        bgcolor: 'background.paper',
        ...sx,
      }}
    >
      <Stack direction="row" spacing={2} alignItems="center" flexWrap="wrap">
        {showPageSizeSelector && onPageSizeChange && (
          <FormControl size="small" sx={{ minWidth: 80 }}>
            <InputLabel id="page-size-label">Rows</InputLabel>
            <Select
              labelId="page-size-label"
              value={pageSize}
              label="Rows"
              onChange={handlePageSizeChange}
            >
              {pageSizeOptions.map((option) => (
                <MenuItem key={option} value={option}>
                  {option}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        )}

        {showItemCount && (
          <Typography variant="body2" color="text.secondary" sx={{ whiteSpace: 'nowrap' }}>
            {startItem}-{endItem} of {totalCount.toLocaleString()}
          </Typography>
        )}
      </Stack>

      <Stack direction="row" spacing={2} alignItems="center">
        {showPageInfo && totalPages > 1 && (
          <Typography variant="body2" color="text.secondary" sx={{ whiteSpace: 'nowrap' }}>
            Page {page} of {totalPages}
          </Typography>
        )}

        <MuiPagination
          count={totalPages}
          page={page}
          onChange={handlePageChange}
          color="primary"
          size="medium"
          showFirstButton
          showLastButton
          siblingCount={1}
          boundaryCount={1}
          sx={{
            '& .MuiPaginationItem-root': {
              transition: 'all 0.2s ease',
            },
          }}
        />
      </Stack>
    </Stack>
  )
}
