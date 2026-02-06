import { useMemo, useCallback } from 'react'
import { motion } from 'framer-motion'
import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Checkbox,
  Paper,
} from '@mui/material'
import type { SxProps, Theme } from '@mui/material'
import { staggerContainer, staggerItem } from '@/shared/lib/animations'
import { TableSkeletonRows, TableEmptyStateRow } from '@/shared/ui'
import { SortableTableHeader } from './SortableTableHeader'
import { TablePagination } from './TablePagination'
import type { ColumnConfig } from '@/shared/types/filter.types'
import type { PaginatedResponse } from '@/shared/types/api'

export interface DataTableProps<T extends { id?: string }> {
  columns: ColumnConfig<T>[]
  data: PaginatedResponse<T> | undefined
  isLoading?: boolean
  error?: Error | null
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
  onSort?: (field: string) => void
  page?: number
  pageSize?: number
  onPageChange?: (page: number) => void
  onPageSizeChange?: (size: number) => void
  pageSizeOptions?: number[]
  selectable?: boolean
  selectedIds?: string[]
  onSelectionChange?: (ids: string[]) => void
  emptyMessage?: string
  emptyIcon?: React.ReactNode
  skeletonRows?: number
  stickyHeader?: boolean
  maxHeight?: number | string
  onRowClick?: (row: T, index: number) => void
  rowHover?: boolean
  getRowId?: (row: T) => string
  sx?: SxProps<Theme>
  tableContainerSx?: SxProps<Theme>
  animated?: boolean
}

export function DataTable<T extends { id?: string }>({
  columns,
  data,
  isLoading = false,
  error = null,
  sortBy,
  sortOrder = 'asc',
  onSort,
  page = 1,
  pageSize = 10,
  onPageChange,
  onPageSizeChange,
  pageSizeOptions = [10, 25, 50, 100],
  selectable = false,
  selectedIds = [],
  onSelectionChange,
  emptyMessage = 'No data available',
  emptyIcon,
  skeletonRows = 5,
  stickyHeader = false,
  maxHeight,
  onRowClick,
  rowHover = true,
  getRowId,
  sx,
  tableContainerSx,
  animated = true,
}: Readonly<DataTableProps<T>>) {
  const visibleColumns = useMemo(
    () => columns.filter((col) => !col.hidden),
    [columns]
  )

  const items = useMemo(() => data?.items ?? [], [data?.items])
  const totalCount = data?.totalCount ?? 0
  const totalPages = data?.totalPages ?? 0

  const getId = useCallback(
    (row: T): string => {
      if (getRowId) {
        return getRowId(row)
      }
      return row.id ?? ''
    },
    [getRowId]
  )

  const allSelected = useMemo(
    () => items.length > 0 && items.every((row) => selectedIds.includes(getId(row))),
    [items, selectedIds, getId]
  )

  const someSelected = useMemo(
    () => items.some((row) => selectedIds.includes(getId(row))) && !allSelected,
    [items, selectedIds, allSelected, getId]
  )

  const handleSelectAll = useCallback(() => {
    if (!onSelectionChange) {
      return
    }

    if (allSelected) {
      const idsToRemove = new Set(items.map(getId))
      onSelectionChange(selectedIds.filter((id) => !idsToRemove.has(id)))
    } else {
      const newIds = items.map(getId)
      const existingSet = new Set(selectedIds)
      onSelectionChange([...selectedIds, ...newIds.filter((id) => !existingSet.has(id))])
    }
  }, [allSelected, items, selectedIds, onSelectionChange, getId])

  const handleSelectRow = useCallback(
    (row: T) => {
      if (!onSelectionChange) {
        return
      }

      const rowId = getId(row)
      if (selectedIds.includes(rowId)) {
        onSelectionChange(selectedIds.filter((id) => id !== rowId))
      } else {
        onSelectionChange([...selectedIds, rowId])
      }
    },
    [selectedIds, onSelectionChange, getId]
  )

  const handleRowClick = useCallback(
    (row: T, index: number, event: React.MouseEvent) => {
      const target = event.target as HTMLElement
      if (target.closest('input[type="checkbox"]') || target.closest('button')) {
        return
      }
      onRowClick?.(row, index)
    },
    [onRowClick]
  )

  const getCellValue = useCallback((row: T, key: string): unknown => {
    const keys = key.split('.')
    let value: unknown = row
    for (const k of keys) {
      if (value === null || value === undefined) {
        return undefined
      }
      value = (value as Record<string, unknown>)[k]
    }
    return value
  }, [])

  const renderContent = () => {
    if (error) {
      return (
        <TableEmptyStateRow
          colSpan={visibleColumns.length + (selectable ? 1 : 0)}
          message={error.message || 'An error occurred'}
        />
      )
    }

    if (isLoading) {
      return (
        <TableSkeletonRows
          rows={skeletonRows}
          columns={visibleColumns.length + (selectable ? 1 : 0)}
        />
      )
    }

    if (items.length === 0) {
      return (
        <TableEmptyStateRow
          colSpan={visibleColumns.length + (selectable ? 1 : 0)}
          message={emptyMessage}
          icon={emptyIcon}
        />
      )
    }

    const RowWrapper = animated ? motion.tr : 'tr'

    return items.map((row, rowIndex) => (
      <TableRow
        key={getId(row) || rowIndex}
        component={RowWrapper}
        {...(animated && { variants: staggerItem })}
        hover={rowHover}
        selected={selectable && selectedIds.includes(getId(row))}
        onClick={(e: React.MouseEvent) => handleRowClick(row, rowIndex, e)}
        sx={{
          cursor: onRowClick ? 'pointer' : 'default',
          '&:last-child td, &:last-child th': { border: 0 },
        }}
      >
        {selectable && (
          <TableCell padding="checkbox">
            <Checkbox
              checked={selectedIds.includes(getId(row))}
              onChange={() => handleSelectRow(row)}
              onClick={(e) => e.stopPropagation()}
            />
          </TableCell>
        )}
        {visibleColumns.map((column) => {
          const value = getCellValue(row, String(column.key))
          return (
            <TableCell
              key={String(column.key)}
              align={column.align}
              sx={{
                width: column.width,
                minWidth: column.minWidth,
              }}
            >
              {column.render ? column.render(value, row, rowIndex) : (value as React.ReactNode)}
            </TableCell>
          )
        })}
      </TableRow>
    ))
  }

  const tableBodyProps = animated
    ? { component: motion.tbody, variants: staggerContainer, initial: 'initial', animate: 'animate' }
    : {}

  return (
    <Box sx={sx}>
      <TableContainer
        component={Paper}
        sx={{
          maxHeight: maxHeight,
          boxShadow: 'none',
          border: '1px solid',
          borderColor: 'divider',
          borderRadius: 2,
          ...tableContainerSx,
        }}
      >
        <Table stickyHeader={stickyHeader} size="medium">
          <TableHead>
            <TableRow>
              {selectable && (
                <TableCell padding="checkbox">
                  <Checkbox
                    indeterminate={someSelected}
                    checked={allSelected}
                    onChange={handleSelectAll}
                  />
                </TableCell>
              )}
              {visibleColumns.map((column) => (
                <SortableTableHeader
                  key={String(column.key)}
                  column={column}
                  sortBy={sortBy}
                  sortOrder={sortOrder}
                  onSort={onSort}
                />
              ))}
            </TableRow>
          </TableHead>
          <TableBody {...tableBodyProps}>{renderContent()}</TableBody>
        </Table>
      </TableContainer>

      {onPageChange && totalCount > 0 && (
        <TablePagination
          page={page}
          pageSize={pageSize}
          totalCount={totalCount}
          totalPages={totalPages}
          onPageChange={onPageChange}
          onPageSizeChange={onPageSizeChange}
          pageSizeOptions={pageSizeOptions}
        />
      )}
    </Box>
  )
}
