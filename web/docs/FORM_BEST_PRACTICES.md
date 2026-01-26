# Form Best Practices: Combined Create/Edit Pattern

## 📚 Research Summary

### Industry Best Practices

#### 1. **Single Page for Create & Edit** ✅
**Why:** DRY principle, consistent UX, easier maintenance

**Pattern:**
```
/resource/create  → New resource
/resource/:id/edit → Edit existing resource
```

**Implementation Strategy:**
- Same component handles both modes
- URL param determines mode (`id` present = edit mode)
- Shared form logic with mode-specific behavior
- Dynamic page titles and submit buttons

#### 2. **Form State Management** 

**React Hook Form + Zod:**
```typescript
// ✅ DO: Single schema, mode-specific defaults
const form = useForm({
  resolver: zodResolver(schema),
  defaultValues: isEditMode ? existingData : emptyDefaults
})

// ❌ DON'T: Separate forms for create/edit
```

**Key Principles:**
- Initialize form with data in edit mode
- Show loading skeleton while fetching
- Handle form reset after successful submission
- Validate on blur for better UX

#### 3. **Loading States** (From UX Guidelines)

**Best Practice:**
```
Loading → Success/Error → Feedback
```

**Implementation:**
```typescript
// Show loading skeleton
if (isLoading) return <FormSkeleton />

// Show error state with retry
if (error) return <ErrorState onRetry={refetch} />

// Show form
return <Form data={data} mode={mode} />
```

#### 4. **Error Handling** (From UX Guidelines)

**Severity: HIGH**
- Use `aria-live` or `role="alert"` for errors
- Provide clear next steps
- Show field-level errors inline
- Show form-level errors at top

```typescript
// ✅ DO: Accessible error messages
<FormHelperText error role="alert">
  {error.message}
</FormHelperText>

// ❌ DON'T: Visual-only errors
<div className="text-red">{error}</div>
```

#### 5. **Submit Feedback** (From UX Guidelines)

**Severity: HIGH**
```
1. Button → Loading state (disabled + spinner)
2. API call → Show progress
3. Success → Show message + redirect/reset
4. Error → Show error + keep form state
```

**Implementation:**
```typescript
const onSubmit = async (data) => {
  setIsSubmitting(true)
  try {
    await mutateAsync(data)
    toast.success('Auction created successfully!')
    navigate('/my-auctions')
  } catch (error) {
    toast.error(error.message)
  } finally {
    setIsSubmitting(false)
  }
}
```

#### 6. **Form Labels & Accessibility** (From React Guidelines)

**Severity: HIGH**
```typescript
// ✅ DO: Label with htmlFor
<label htmlFor="title">Title</label>
<input id="title" {...field} />

// ❌ DON'T: Placeholder only
<input placeholder="Title" />
```

#### 7. **Controlled Components** (From React Guidelines)

**Severity: MEDIUM**
```typescript
// ✅ DO: React Hook Form handles this
<Controller
  control={control}
  name="title"
  render={({ field }) => <TextField {...field} />}
/>

// ❌ DON'T: Uncontrolled with refs
<input ref={titleRef} />
```

---

## 🎯 Implementation Pattern

### File Structure
```
pages/
├── AuctionFormPage.tsx       # Combined create/edit page
└── AuctionFormSkeleton.tsx   # Loading skeleton

components/
└── AuctionForm.tsx           # Reusable form component (optional)
```

### Route Configuration
```typescript
{
  path: '/auctions/create',
  element: <AuctionFormPage mode="create" />
},
{
  path: '/auctions/:id/edit',
  element: <AuctionFormPage mode="edit" />
}
```

### Component Structure
```typescript
interface AuctionFormPageProps {
  mode?: 'create' | 'edit'
}

export function AuctionFormPage({ mode: modeProp }: AuctionFormPageProps) {
  const { id } = useParams()
  const mode = modeProp || (id ? 'edit' : 'create')
  const isEditMode = mode === 'edit'
  
  // Fetch data only in edit mode
  const { data, isLoading } = useAuction(id!, { enabled: isEditMode })
  
  // Mutations
  const createMutation = useCreateAuction()
  const updateMutation = useUpdateAuction()
  
  // Form setup
  const form = useForm({
    resolver: zodResolver(isEditMode ? updateSchema : createSchema),
    defaultValues: isEditMode ? undefined : getDefaultValues(),
  })
  
  // Populate form in edit mode
  useEffect(() => {
    if (isEditMode && data) {
      form.reset(data)
    }
  }, [data, isEditMode])
  
  // Submit handler
  const onSubmit = async (formData) => {
    if (isEditMode) {
      await updateMutation.mutateAsync({ id: id!, data: formData })
      toast.success('Auction updated successfully!')
    } else {
      await createMutation.mutateAsync(formData)
      toast.success('Auction created successfully!')
      navigate('/my-auctions')
    }
  }
  
  // Loading state
  if (isEditMode && isLoading) return <FormSkeleton />
  
  // Error state
  if (isEditMode && !data) return <ErrorState />
  
  return (
    <Container>
      <Typography variant="h4">
        {isEditMode ? 'Edit Auction' : 'Create New Auction'}
      </Typography>
      
      <form onSubmit={form.handleSubmit(onSubmit)}>
        {/* Form fields */}
        
        <Button 
          type="submit"
          disabled={isSubmitting}
          startIcon={isSubmitting && <CircularProgress size={20} />}
        >
          {isSubmitting 
            ? 'Saving...' 
            : isEditMode ? 'Update Auction' : 'Create Auction'
          }
        </Button>
      </form>
    </Container>
  )
}
```

---

## 📋 Checklist for Implementation

### Form Setup
- [ ] Single component handles create & edit
- [ ] URL-based mode detection (`/create` vs `/:id/edit`)
- [ ] React Hook Form + Zod validation
- [ ] TypeScript interfaces for form data

### Data Loading (Edit Mode)
- [ ] Fetch existing data with `useAuction(id)`
- [ ] Show loading skeleton while fetching
- [ ] Populate form with `form.reset(data)` after fetch
- [ ] Handle error state if fetch fails

### Submit Logic
- [ ] Separate mutations for create/update
- [ ] Loading state during submission
- [ ] Success feedback (toast + redirect)
- [ ] Error handling with recovery path
- [ ] Button disabled during submission

### Accessibility
- [ ] All inputs have labels with `htmlFor`
- [ ] Error messages use `role="alert"`
- [ ] Form has `aria-label` or heading
- [ ] Focus management after errors

### UX Polish
- [ ] Dynamic page title (Create vs Edit)
- [ ] Dynamic submit button text
- [ ] Breadcrumbs show current context
- [ ] Unsaved changes warning (optional)
- [ ] Auto-save draft (optional)

---

## 🎨 UI/UX Recommendations

### Page Title
```tsx
<Breadcrumbs>
  <Link to="/auctions">Auctions</Link>
  {isEditMode && <Link to={`/auctions/${id}`}>{data.title}</Link>}
  <Typography>{isEditMode ? 'Edit' : 'Create'}</Typography>
</Breadcrumbs>
```

### Submit Button States
```tsx
<Button
  disabled={isSubmitting || !isValid}
  startIcon={isSubmitting && <CircularProgress size={20} />}
>
  {isSubmitting 
    ? (isEditMode ? 'Updating...' : 'Creating...')
    : (isEditMode ? 'Update Auction' : 'Create Auction')
  }
</Button>
```

### Error Display
```tsx
{errors.root && (
  <Alert severity="error" role="alert" sx={{ mb: 3 }}>
    <AlertTitle>Error</AlertTitle>
    {errors.root.message}
    <Button onClick={handleRetry}>Try Again</Button>
  </Alert>
)}
```

---

## 🚀 Benefits of This Pattern

✅ **DRY Code** - No duplication between create/edit  
✅ **Consistent UX** - Same interface for both modes  
✅ **Easier Testing** - Test one component, two modes  
✅ **Maintainability** - Single source of truth  
✅ **Type Safety** - Shared types and validation  
✅ **Accessibility** - Proper ARIA labels and roles  
✅ **Performance** - Conditional data fetching only when needed  

---

## 📚 References

- [React Hook Form Best Practices](https://react-hook-form.com/get-started)
- [UX Guidelines: Form Submission](https://uxplanet.org/form-design-best-practices)
- [Accessibility: Form Labels](https://www.w3.org/WAI/tutorials/forms/labels/)
- [React: Controlled Components](https://react.dev/reference/react-dom/components/input)
