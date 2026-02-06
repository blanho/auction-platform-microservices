import { useState, useCallback, useMemo } from 'react'
import type { QueryParameters, PaginatedResponse } from '../types/api'

export interface UsePaginationOptions<TFilter = Record<string, unknown>> {
  defaultPage?: number
  defaultPageSize?: number
  defaultSortBy?: string
  defaultSortOrder?: 'asc' | 'desc'
  defaultFilter?: TFilter
}

export interface UsePaginationReturn<TFilter = Record<string, unknown>> {
  page: number
  pageSize: number
  sortBy: string
  sortOrder: 'asc' | 'desc'
  filter: TFilter
  queryParams: QueryParameters & { filter?: TFilter }
  setPage: (page: number) => void
  setPageSize: (size: number) => void
  setSortBy: (field: string) => void
  setSortOrder: (order: 'asc' | 'desc') => void
  toggleSortOrder: () => void
  handleSort: (field: string) => void
  setFilter: (filter: TFilter | ((prev: TFilter) => TFilter)) => void
  updateFilter: <K extends keyof TFilter>(key: K, value: TFilter[K]) => void
  clearFilter: () => void
  reset: () => void
  getPaginationInfo: (response: PaginatedResponse<unknown> | undefined) => PaginationInfo
}

export interface PaginationInfo {
  currentPage: number
  totalPages: number
  totalCount: number
  pageSize: number
  hasNextPage: boolean
  hasPreviousPage: boolean
  startIndex: number
  endIndex: number
  isEmpty: boolean
}

const DEFAULT_PAGE = 1
const DEFAULT_PAGE_SIZE = 10

export function usePagination<TFilter = Record<string, unknown>>(
  options: UsePaginationOptions<TFilter> = {}
): UsePaginationReturn<TFilter> {
  const {
    defaultPage = DEFAULT_PAGE,
    defaultPageSize = DEFAULT_PAGE_SIZE,
    defaultSortBy = '',
    defaultSortOrder = 'desc',
    defaultFilter = {} as TFilter,
  } = options

  const [page, setPage] = useState(defaultPage)
  const [pageSize, setPageSize] = useState(defaultPageSize)
  const [sortBy, setSortBy] = useState(defaultSortBy)
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>(defaultSortOrder)
  const [filter, setFilter] = useState<TFilter>(defaultFilter)

  const setPageValue = useCallback((newPage: number) => {
    setPage(Math.max(1, newPage))
  }, [])

  const setPageSizeValue = useCallback((newSize: number) => {
    setPageSize(Math.min(Math.max(1, newSize), 100))
    setPage(1)
  }, [])

  const setSortByValue = useCallback((field: string) => {
    setSortBy(field)
    setPage(1)
  }, [])

  const setSortOrderValue = useCallback((order: 'asc' | 'desc') => {
    setSortOrder(order)
  }, [])

  const toggleSortOrder = useCallback(() => {
    setSortOrder((prev) => (prev === 'asc' ? 'desc' : 'asc'))
  }, [])

  const handleSort = useCallback(
    (field: string) => {
      if (sortBy === field) {
        toggleSortOrder()
      } else {
        setSortBy(field)
        setSortOrder('asc')
      }
      setPage(1)
    },
    [sortBy, toggleSortOrder]
  )

  const setFilterValue = useCallback((newFilter: TFilter | ((prev: TFilter) => TFilter)) => {
    setFilter(newFilter)
    setPage(1)
  }, [])

  const updateFilter = useCallback(<K extends keyof TFilter>(key: K, value: TFilter[K]) => {
    setFilter((prev) => ({ ...prev, [key]: value }))
    setPage(1)
  }, [])

  const clearFilter = useCallback(() => {
    setFilter(defaultFilter)
    setPage(1)
  }, [defaultFilter])

  const reset = useCallback(() => {
    setPage(defaultPage)
    setPageSize(defaultPageSize)
    setSortBy(defaultSortBy)
    setSortOrder(defaultSortOrder)
    setFilter(defaultFilter)
  }, [defaultPage, defaultPageSize, defaultSortBy, defaultSortOrder, defaultFilter])

  const queryParams = useMemo(
    () => ({
      page,
      pageSize,
      sortBy: sortBy || undefined,
      sortOrder: sortBy ? sortOrder : undefined,
      filter: Object.keys(filter as Record<string, unknown>).length > 0 ? filter : undefined,
    }),
    [page, pageSize, sortBy, sortOrder, filter]
  )

  const getPaginationInfo = useCallback(
    (response: PaginatedResponse<unknown> | undefined): PaginationInfo => {
      if (!response) {
        return {
          currentPage: page,
          totalPages: 0,
          totalCount: 0,
          pageSize,
          hasNextPage: false,
          hasPreviousPage: false,
          startIndex: 0,
          endIndex: 0,
          isEmpty: true,
        }
      }

      const startIndex = (response.page - 1) * response.pageSize + 1
      const endIndex = Math.min(startIndex + response.items.length - 1, response.totalCount)

      return {
        currentPage: response.page,
        totalPages: response.totalPages,
        totalCount: response.totalCount,
        pageSize: response.pageSize,
        hasNextPage: response.hasNextPage,
        hasPreviousPage: response.hasPreviousPage,
        startIndex: response.items.length > 0 ? startIndex : 0,
        endIndex: response.items.length > 0 ? endIndex : 0,
        isEmpty: response.items.length === 0,
      }
    },
    [page, pageSize]
  )

  return {
    page,
    pageSize,
    sortBy,
    sortOrder,
    filter,
    queryParams,
    setPage: setPageValue,
    setPageSize: setPageSizeValue,
    setSortBy: setSortByValue,
    setSortOrder: setSortOrderValue,
    toggleSortOrder,
    handleSort,
    setFilter: setFilterValue,
    updateFilter,
    clearFilter,
    reset,
    getPaginationInfo,
  }
}
