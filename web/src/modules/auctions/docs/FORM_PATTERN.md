# Combined Create/Edit Form Pattern

> Best practices for implementing unified create/edit forms following DRY principles.

## Overview

This document describes the pattern used in `AuctionFormPage.tsx` for combining create and edit functionality into a single component, reducing code duplication and improving maintainability.

## Key Principles

### 1. URL-Based Mode Detection

```typescript
const { id } = useParams<{ id: string }>()
const isEditMode = Boolean(id)
```

- `/auctions/create` → `id` is undefined → Create mode
- `/auctions/:id/edit` → `id` exists → Edit mode

### 2. Conditional Data Fetching

```typescript
const { data: existingAuction, isLoading, error } = useAuction(id ?? '')
```

The hook internally handles the `enabled` option based on whether an ID exists.

### 3. Separate Schemas for Create/Edit

```typescript
const schema = useMemo(
  () => isEditMode ? updateAuctionSchema : createAuctionSchema,
  [isEditMode]
)
```

- **Create Schema**: All required fields (title, description, pricing, dates, etc.)
- **Edit Schema**: Only editable fields (title, description, optional prices)

### 4. Form Population in Edit Mode

```typescript
useEffect(() => {
  if (isEditMode && existingAuction) {
    reset({
      title: existingAuction.title,
      description: existingAuction.description,
      categoryId: existingAuction.categoryId,
      reservePrice: existingAuction.reservePrice,
      buyNowPrice: existingAuction.buyNowPrice,
    })
  }
}, [existingAuction, isEditMode, reset])
```

### 5. Separate Mutations

```typescript
const createMutation = useCreateAuction()
const updateMutation = useUpdateAuction()

const onSubmit = async (data) => {
  if (isEditMode && id) {
    await updateMutation.mutateAsync({ id, data })
  } else {
    await createMutation.mutateAsync(data)
  }
  navigate('/my-auctions')
}
```

## UI/UX Best Practices

### Loading States

- Show skeleton component while fetching existing data in edit mode
- Disable form while submitting
- Show loading indicator in submit button

### Error Handling

- Display error alert with retry option if data fetch fails
- Use `role="alert"` for accessibility on error messages
- Provide clear error messages for form validation

### Dynamic Content

| Element | Create Mode | Edit Mode |
|---------|-------------|-----------|
| Page Title | "Create New Auction" | "Edit Auction" |
| Submit Button | "Create Auction" | "Update Auction" |
| Loading Button | "Creating..." | "Updating..." |
| Breadcrumb | "Create" | "Edit" |

### Conditional Fields

Some fields only appear in create mode:
- Starting price
- Condition
- Start/end time
- Shipping options
- Return policy

These are immutable after auction creation.

## Accessibility

- All inputs have associated labels via `<InputLabel>` and `labelId`
- Error messages use `role="alert"` for screen readers
- Form controls use `aria-describedby` to link to error messages
- Loading skeleton provides visual feedback during data fetch

## File Structure

```
pages/
  AuctionFormPage.tsx     # Combined create/edit component
schemas/
  auction.schema.ts       # Contains both createAuctionSchema and updateAuctionSchema
hooks/
  useAuctions.ts          # Contains useAuction, useCreateAuction, useUpdateAuction
```

## Router Configuration

```typescript
{ path: '/auctions/create', element: <AuctionFormPage /> }
{ path: '/auctions/:id/edit', element: <AuctionFormPage /> }
```

Both routes use the same component - mode is determined by URL params.

## Benefits

1. **DRY Code**: Single component instead of two similar files
2. **Consistent UX**: Identical layout and behavior for create/edit
3. **Easier Testing**: One component to test
4. **Simplified Maintenance**: Changes apply to both modes
5. **Type Safety**: TypeScript ensures correct data flow

## Anti-Patterns to Avoid

- ❌ Using conditional rendering for entire form sections
- ❌ Complex nested ternaries for mode detection
- ❌ Fetching data in create mode
- ❌ Mixing create-only and edit-only mutations
- ❌ Ignoring loading/error states in edit mode
