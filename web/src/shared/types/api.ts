/**
 * Common pagination defaults matching backend BuildingBlocks.Application.Constants.PaginationDefaults
 */
export const PaginationDefaults = {
  DefaultPage: 1,
  DefaultPageSize: 10,
  MaxPageSize: 100,
} as const

/**
 * Base query parameters for pagination - matches backend QueryParameters
 */
export interface QueryParameters {
  page?: number
  pageSize?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

/**
 * Generic query parameters with typed filter - matches backend QueryParameters<TFilter>
 */
export interface QueryParametersWithFilter<TFilter> extends QueryParameters {
  filter?: TFilter
}

export interface PaginatedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

/**
 * Raw paginated envelope returned by the backend API.
 * Use mapPaginatedResponse to convert it to PaginatedResponse<TFrontend>.
 */
export interface BackendPaginatedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

/**
 * Maps a raw backend paginated response to the frontend PaginatedResponse shape,
 * applying a type-level mapper to each item.
 *
 * Eliminates copy-paste mapping blocks across all API files.
 */
export function mapPaginatedResponse<TBackend, TFrontend>(
  data: BackendPaginatedResponse<TBackend>,
  mapper: (items: TBackend[]) => TFrontend[]
): PaginatedResponse<TFrontend> {
  return {
    items: mapper(data.items),
    page: data.page,
    pageSize: data.pageSize,
    totalCount: data.totalCount,
    totalPages: data.totalPages,
    hasNextPage: data.hasNextPage,
    hasPreviousPage: data.hasPreviousPage,
  }
}

export interface ApiError {
  message: string
  code?: string
  errors?: Record<string, string[]>
}
