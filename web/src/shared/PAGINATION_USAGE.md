# Pagination, Filtering & Sorting Components Usage Guide

This guide demonstrates how to use the shared pagination, filtering, and sorting components that are consistent with the backend API patterns.

## Overview

The frontend components mirror the backend patterns:
- `usePagination` hook → matches `QueryParameters<TFilter>` 
- `DataTable` component → works with `PaginatedResponse<T>`
- `FilterPanel` component → matches `IFilter<T>` patterns
- `SortableTableHeader` → works with backend sort maps

## Quick Start

### 1. Basic Usage with usePagination Hook

```tsx
import { usePagination } from '@/shared/hooks'
import { DataTable } from '@/shared/ui'
import type { ColumnConfig, OrderFilter } from '@/shared/types'

function OrdersListPage() {
  const pagination = usePagination<OrderFilter>({
    defaultPageSize: 10,
    defaultSortBy: 'createdAt',
    defaultSortOrder: 'desc',
  })

  const { data, isLoading, error } = useQuery({
    queryKey: ['orders', pagination.queryParams],
    queryFn: () => ordersApi.getOrders(pagination.queryParams),
  })

  const columns: ColumnConfig<Order>[] = [
    { key: 'id', header: 'Order ID', sortable: true },
    { key: 'totalAmount', header: 'Amount', sortable: true, align: 'right' },
    { key: 'status', header: 'Status', sortable: true },
    { key: 'createdAt', header: 'Date', sortable: true },
  ]

  return (
    <DataTable
      columns={columns}
      data={data}
      isLoading={isLoading}
      error={error}
      sortBy={pagination.sortBy}
      sortOrder={pagination.sortOrder}
      onSort={pagination.handleSort}
      page={pagination.page}
      pageSize={pagination.pageSize}
      onPageChange={pagination.setPage}
      onPageSizeChange={pagination.setPageSize}
    />
  )
}
```

### 2. With Filtering

```tsx
import { usePagination } from '@/shared/hooks'
import { DataTable, FilterPanel } from '@/shared/ui'
import type { FilterPanelConfig, OrderFilter } from '@/shared/types'

const filterConfig: FilterPanelConfig = {
  fields: [
    {
      key: 'search',
      type: 'text',
      label: 'Search',
      placeholder: 'Search orders...',
      gridSize: { xs: 12, sm: 6, md: 4 },
    },
    {
      key: 'status',
      type: 'select',
      label: 'Status',
      options: [
        { value: 'pending', label: 'Pending' },
        { value: 'completed', label: 'Completed' },
        { value: 'cancelled', label: 'Cancelled' },
      ],
      gridSize: { xs: 12, sm: 6, md: 3 },
    },
    {
      key: 'dateRange',
      type: 'dateRange',
      label: 'Date',
      startKey: 'dateFrom',
      endKey: 'dateTo',
      gridSize: { xs: 12, sm: 12, md: 5 },
    },
    {
      key: 'amount',
      type: 'numberRange',
      label: 'Amount',
      minKey: 'minAmount',
      maxKey: 'maxAmount',
      min: 0,
      step: 100,
    },
  ],
  collapsible: true,
  defaultExpanded: true,
  showClearButton: true,
}

function OrdersWithFilters() {
  const pagination = usePagination<OrderFilter>({
    defaultPageSize: 20,
    defaultSortBy: 'createdAt',
    defaultSortOrder: 'desc',
  })

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['orders', pagination.queryParams],
    queryFn: () => ordersApi.getOrders({
      page: pagination.page,
      pageSize: pagination.pageSize,
      sortBy: pagination.sortBy,
      sortOrder: pagination.sortOrder,
      ...pagination.filter,
    }),
  })

  return (
    <Stack spacing={2}>
      <FilterPanel
        config={filterConfig}
        value={pagination.filter}
        onChange={pagination.setFilter}
        onClear={pagination.clearFilter}
        onRefresh={refetch}
      />

      <DataTable
        columns={columns}
        data={data}
        isLoading={isLoading}
        sortBy={pagination.sortBy}
        sortOrder={pagination.sortOrder}
        onSort={pagination.handleSort}
        page={pagination.page}
        pageSize={pagination.pageSize}
        onPageChange={pagination.setPage}
        onPageSizeChange={pagination.setPageSize}
      />
    </Stack>
  )
}
```

### 3. Custom Column Rendering

```tsx
const columns: ColumnConfig<Order>[] = [
  {
    key: 'auctionTitle',
    header: 'Item',
    sortable: true,
    sortKey: 'title',
    render: (_value, order) => (
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
        <Avatar src={order.imageUrl} />
        <Box>
          <Typography variant="subtitle2">{order.auctionTitle}</Typography>
          <Typography variant="caption" color="text.secondary">
            #{order.id.slice(0, 8)}
          </Typography>
        </Box>
      </Box>
    ),
  },
  {
    key: 'totalAmount',
    header: 'Amount',
    sortable: true,
    align: 'right',
    render: (value) => (
      <Typography fontWeight={600}>
        ${Number(value).toLocaleString()}
      </Typography>
    ),
  },
  {
    key: 'status',
    header: 'Status',
    sortable: true,
    render: (value) => (
      <Chip
        label={String(value)}
        color={getStatusColor(value as OrderStatus)}
        size="small"
      />
    ),
  },
  {
    key: 'createdAt',
    header: 'Date',
    sortable: true,
    render: (value) => formatDate(String(value)),
  },
  {
    key: 'id',
    header: '',
    align: 'right',
    render: (value) => (
      <IconButton component={Link} to={`/orders/${value}`}>
        <Visibility />
      </IconButton>
    ),
  },
]
```

### 4. With Row Selection

```tsx
function SelectableTable() {
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const pagination = usePagination()

  return (
    <>
      {selectedIds.length > 0 && (
        <Box sx={{ mb: 2, p: 2, bgcolor: 'primary.50', borderRadius: 1 }}>
          <Typography>{selectedIds.length} items selected</Typography>
          <Button onClick={() => handleBulkAction(selectedIds)}>
            Bulk Action
          </Button>
        </Box>
      )}

      <DataTable
        columns={columns}
        data={data}
        selectable
        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}
        // ... other props
      />
    </>
  )
}
```

### 5. Clickable Rows with Navigation

```tsx
function ClickableRowsTable() {
  const navigate = useNavigate()

  return (
    <DataTable
      columns={columns}
      data={data}
      onRowClick={(row) => navigate(`/orders/${row.id}`)}
      rowHover
      // ... other props
    />
  )
}
```

## Filter Field Types Reference

### Text Field
```tsx
{
  key: 'search',
  type: 'text',
  label: 'Search',
  placeholder: 'Enter search term...',
  debounceMs: 300,
}
```

### Select Field
```tsx
{
  key: 'status',
  type: 'select',
  label: 'Status',
  options: [
    { value: 'active', label: 'Active' },
    { value: 'inactive', label: 'Inactive', disabled: true },
  ],
  multiple: false,
  clearable: true,
}
```

### Date Field
```tsx
{
  key: 'createdAt',
  type: 'date',
  label: 'Created Date',
  minDate: new Date('2020-01-01'),
  maxDate: new Date(),
}
```

### Date Range Field
```tsx
{
  key: 'dateRange',
  type: 'dateRange',
  label: 'Date Range',
  startKey: 'startDate',
  endKey: 'endDate',
}
```

### Number Range Field
```tsx
{
  key: 'price',
  type: 'numberRange',
  label: 'Price',
  minKey: 'minPrice',
  maxKey: 'maxPrice',
  min: 0,
  max: 100000,
  step: 100,
}
```

### Boolean Field
```tsx
{
  key: 'isActive',
  type: 'boolean',
  label: 'Active',
  trueLabel: 'Active Only',
  falseLabel: 'Inactive Only',
}
```

## Backend Consistency

### Sort Fields
The `sortKey` in column config should match the backend sort map keys:

Backend (C#):
```csharp
private static readonly Dictionary<string, Expression<Func<Order, object>>> OrderSortMap = new()
{
    ["createdAt"] = o => o.CreatedAt,
    ["totalAmount"] = o => o.TotalAmount,
    ["status"] = o => o.Status,
};
```

Frontend:
```tsx
const columns: ColumnConfig<Order>[] = [
  { key: 'createdAt', header: 'Date', sortable: true, sortKey: 'createdAt' },
  { key: 'totalAmount', header: 'Amount', sortable: true, sortKey: 'totalAmount' },
]
```

### Filter Parameters
The filter object should match the backend filter class:

Backend (C#):
```csharp
public class OrderFilter : IFilter<Order>
{
    public string? Search { get; set; }
    public OrderStatus? Status { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
```

Frontend:
```tsx
interface OrderFilter {
  search?: string
  status?: string
  minAmount?: number
  maxAmount?: number
  dateFrom?: string
  dateTo?: string
}
```

## Pagination Info Helper

The `usePagination` hook provides a helper to get pagination display info:

```tsx
const pagination = usePagination()
const paginationInfo = pagination.getPaginationInfo(data)

// Returns:
// {
//   currentPage: 1,
//   totalPages: 10,
//   totalCount: 100,
//   pageSize: 10,
//   hasNextPage: true,
//   hasPreviousPage: false,
//   startIndex: 1,
//   endIndex: 10,
//   isEmpty: false,
// }

// Use in UI:
<Typography>
  Showing {paginationInfo.startIndex}-{paginationInfo.endIndex} of {paginationInfo.totalCount}
</Typography>
```

## Available Filter Types (Frontend → Backend)

| Frontend Type | Frontend Interface | Backend Pattern |
|---------------|-------------------|-----------------|
| `OrderFilter` | search, status, minAmount, maxAmount, dateFrom, dateTo | `OrderFilter : IFilter<Order>` |
| `BidFilter` | auctionId, status, minAmount, maxAmount, dateFrom, dateTo | `BidFilter : IFilter<Bid>` |
| `WalletTransactionFilter` | transactionType, status, minAmount, maxAmount, dateFrom, dateTo | `WalletTransactionFilter : IFilter<WalletTransaction>` |
| `NotificationRecordFilter` | channel, status, templateKey, dateFrom, dateTo | `NotificationRecordFilterCriteria : IFilter<NotificationRecord>` |
| `UserFilter` | search, role, status, isEmailVerified, createdFrom, createdTo | Backend user filters |
| `AuctionFilter` | search, status, categoryId, minPrice, maxPrice, startDateFrom, endDateFrom | Auction filters |
| `ReviewFilter` | auctionId, reviewerId, minRating, maxRating, dateFrom, dateTo | Review filters |
