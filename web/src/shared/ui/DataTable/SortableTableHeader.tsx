import { useCallback } from 'react'
import { TableCell, TableSortLabel, Box, Typography } from '@mui/material'
import { visuallyHidden } from '@mui/utils'
import type { ColumnConfig } from '@/shared/types/filter.types'

export interface SortableTableHeaderProps<T> {
  column: ColumnConfig<T>
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
  onSort?: (field: string) => void
}

export function SortableTableHeader<T>({
  column,
  sortBy,
  sortOrder = 'asc',
  onSort,
}: SortableTableHeaderProps<T>) {
  const sortKey = column.sortKey ?? String(column.key)
  const isSorted = sortBy === sortKey
  const isSortable = column.sortable && onSort

  const handleSort = useCallback(() => {
    if (isSortable) {
      onSort(sortKey)
    }
  }, [isSortable, onSort, sortKey])

  return (
    <TableCell
      align={column.align}
      sx={{
        width: column.width,
        minWidth: column.minWidth,
        fontWeight: 600,
        color: 'text.primary',
        bgcolor: 'background.paper',
        borderBottom: '2px solid',
        borderColor: 'divider',
        whiteSpace: 'nowrap',
      }}
    >
      {isSortable ? (
        <TableSortLabel
          active={isSorted}
          direction={isSorted ? sortOrder : 'asc'}
          onClick={handleSort}
          sx={{
            '&:hover': {
              color: 'primary.main',
            },
            '&.Mui-active': {
              color: 'primary.main',
              '& .MuiTableSortLabel-icon': {
                color: 'primary.main',
              },
            },
          }}
        >
          <Typography variant="subtitle2" component="span" fontWeight={600}>
            {column.header}
          </Typography>
          {isSorted && (
            <Box component="span" sx={visuallyHidden}>
              {sortOrder === 'desc' ? 'sorted descending' : 'sorted ascending'}
            </Box>
          )}
        </TableSortLabel>
      ) : (
        <Typography variant="subtitle2" component="span" fontWeight={600}>
          {column.header}
        </Typography>
      )}
    </TableCell>
  )
}
