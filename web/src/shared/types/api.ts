export interface QueryParameters {
  page?: number
  pageSize?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

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

export interface BackendPaginatedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

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
