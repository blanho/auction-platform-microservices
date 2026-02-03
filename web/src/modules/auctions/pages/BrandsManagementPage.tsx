import { useState, useMemo, useCallback } from 'react'
import { motion } from 'framer-motion'
import {
  Container,
  Card,
  Typography,
  Box,
  Button,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Stack,
  Avatar,
} from '@mui/material'
import { Add, Edit, Delete, Image as ImageIcon } from '@mui/icons-material'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { palette } from '@/shared/theme/tokens'
import { useBrands, useCreateBrand, useUpdateBrand, useDeleteBrand } from '../hooks'
import { fadeInUp, staggerContainer } from '@/shared/lib/animations'
import type { Brand } from '../api/brands.api'
import { brandSchema, type BrandFormData } from '../schemas'
import { DataTable, FilterPanel } from '@/shared/ui'
import { usePagination } from '@/shared/hooks'
import type { ColumnConfig, FilterPanelConfig } from '@/shared/types'

interface BrandFilter {
  search?: string
}

const FILTER_CONFIG: FilterPanelConfig = {
  fields: [
    {
      key: 'search',
      label: 'Search',
      type: 'text',
      placeholder: 'Search brands...',
      gridSize: { xs: 12, sm: 6, md: 4 },
    },
  ],
  collapsible: false,
  showClearButton: true,
}

export function BrandsManagementPage() {
  const pagination = usePagination<BrandFilter>({ pageSize: 10 })
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingBrand, setEditingBrand] = useState<Brand | null>(null)
  const [deleteDialog, setDeleteDialog] = useState<Brand | null>(null)

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<BrandFormData>({
    resolver: zodResolver(brandSchema),
    defaultValues: {
      name: '',
      slug: '',
      description: '',
      websiteUrl: '',
    },
  })

  const { data: brandsData, refetch } = useBrands({
    page: pagination.page,
    pageSize: pagination.pageSize,
    sortBy: pagination.sortBy,
    sortOrder: pagination.sortOrder,
    search: pagination.filter.search,
  })

  const items = useMemo(() => brandsData?.items ?? [], [brandsData?.items])

  const createMutation = useCreateBrand()
  const updateMutation = useUpdateBrand()
  const deleteMutation = useDeleteBrand()

  const handleOpenDialog = useCallback((brand?: Brand) => {
    if (brand) {
      setEditingBrand(brand)
      reset({
        name: brand.name,
        slug: brand.slug,
        description: brand.description || '',
        websiteUrl: brand.websiteUrl || '',
      })
    } else {
      setEditingBrand(null)
      reset({ name: '', slug: '', description: '', websiteUrl: '' })
    }
    setDialogOpen(true)
  }, [reset])

  const columns: ColumnConfig<Brand>[] = useMemo(
    () => [
      {
        key: 'name',
        header: 'Brand',
        sortable: true,
        sortKey: 'name',
        render: (_, row) => (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Avatar
              src={row.logoUrl}
              variant="rounded"
              sx={{ width: 48, height: 48, bgcolor: 'grey.100' }}
            >
              <ImageIcon sx={{ color: 'grey.400' }} />
            </Avatar>
            <Box>
              <Typography variant="body2" fontWeight={500}>
                {row.name}
              </Typography>
              {row.description && (
                <Typography
                  variant="caption"
                  color="text.secondary"
                  noWrap
                  sx={{ maxWidth: 200, display: 'block' }}
                >
                  {row.description}
                </Typography>
              )}
            </Box>
          </Box>
        ),
      },
      {
        key: 'slug',
        header: 'Slug',
        sortable: true,
        sortKey: 'slug',
        render: (_, row) => (
          <Typography variant="body2" color="text.secondary">
            /{row.slug}
          </Typography>
        ),
      },
      {
        key: 'auctionCount',
        header: 'Auctions',
        sortable: true,
        sortKey: 'auctionCount',
        render: (_, row) => row.auctionCount ?? 0,
      },
      {
        key: 'websiteUrl',
        header: 'Website',
        render: (_, row) =>
          row.websiteUrl ? (
            <Typography
              variant="body2"
              component="a"
              href={row.websiteUrl}
              target="_blank"
              rel="noopener noreferrer"
              sx={{ color: 'primary.main', textDecoration: 'none' }}
              onClick={(e) => e.stopPropagation()}
            >
              {row.websiteUrl.replace(/^https?:\/\//, '')}
            </Typography>
          ) : (
            '-'
          ),
      },
      {
        key: 'actions',
        header: 'Actions',
        align: 'right',
        render: (_, row) => (
          <Box>
            <IconButton
              size="small"
              onClick={(e) => {
                e.stopPropagation()
                handleOpenDialog(row)
              }}
            >
              <Edit fontSize="small" />
            </IconButton>
            <IconButton
              size="small"
              onClick={(e) => {
                e.stopPropagation()
                setDeleteDialog(row)
              }}
              disabled={(row.auctionCount ?? 0) > 0}
            >
              <Delete fontSize="small" />
            </IconButton>
          </Box>
        ),
      },
    ],
    [handleOpenDialog]
  )

  const onSubmit = (data: BrandFormData) => {
    if (editingBrand) {
      updateMutation.mutate(
        { id: editingBrand.id, data },
        {
          onSuccess: () => {
            setDialogOpen(false)
            setEditingBrand(null)
            reset()
          },
        }
      )
    } else {
      createMutation.mutate(data, {
        onSuccess: () => {
          setDialogOpen(false)
          reset()
        },
      })
    }
  }

  const handleDelete = () => {
    if (deleteDialog) {
      deleteMutation.mutate(deleteDialog.id, {
        onSuccess: () => {
          setDeleteDialog(null)
        },
      })
    }
  }

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <motion.div variants={staggerContainer} initial="initial" animate="animate">
        <motion.div variants={fadeInUp}>
          <Box
            sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}
          >
            <Typography
              variant="h4"
              sx={{
                fontFamily: '"Playfair Display", serif',
                fontWeight: 700,
                color: 'primary.main',
              }}
            >
              Brands
            </Typography>
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => handleOpenDialog()}
              sx={{
                bgcolor: palette.brand.primary,
                '&:hover': { bgcolor: palette.brand.hover },
              }}
            >
              Add Brand
            </Button>
          </Box>
        </motion.div>

        <motion.div variants={fadeInUp}>
          <Card>
            <FilterPanel
              config={FILTER_CONFIG}
              value={pagination.filter}
              onChange={pagination.setFilter}
              onClear={pagination.clearFilter}
              onRefresh={refetch}
            />

            <DataTable
              columns={columns}
              data={items}
              sortBy={pagination.sortBy}
              sortOrder={pagination.sortOrder}
              onSort={pagination.handleSort}
              page={pagination.page}
              pageSize={pagination.pageSize}
              totalCount={brandsData?.totalCount ?? 0}
              totalPages={brandsData?.totalPages ?? 0}
              onPageChange={pagination.setPage}
              onPageSizeChange={pagination.setPageSize}
              onRowClick={(row) => handleOpenDialog(row)}
              emptyMessage="No brands found"
            />
          </Card>
        </motion.div>
      </motion.div>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingBrand ? 'Edit Brand' : 'Add Brand'}</DialogTitle>
        <form onSubmit={handleSubmit(onSubmit)}>
          <DialogContent>
            <Stack spacing={3} sx={{ pt: 1 }}>
              <Controller
                name="name"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label="Brand Name"
                    fullWidth
                    error={!!errors.name}
                    helperText={errors.name?.message}
                  />
                )}
              />

              <Controller
                name="slug"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label="Slug"
                    fullWidth
                    error={!!errors.slug}
                    helperText={errors.slug?.message || 'URL-friendly identifier'}
                  />
                )}
              />

              <Controller
                name="description"
                control={control}
                render={({ field }) => (
                  <TextField {...field} label="Description" fullWidth multiline rows={3} />
                )}
              />

              <Controller
                name="websiteUrl"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label="Website URL"
                    fullWidth
                    error={!!errors.websiteUrl}
                    helperText={errors.websiteUrl?.message}
                    placeholder="https://example.com"
                  />
                )}
              />
            </Stack>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
            <Button
              type="submit"
              variant="contained"
              disabled={createMutation.isPending || updateMutation.isPending}
              sx={{
                bgcolor: palette.brand.primary,
                '&:hover': { bgcolor: palette.brand.hover },
              }}
            >
              {editingBrand ? 'Update' : 'Create'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>

      <Dialog open={!!deleteDialog} onClose={() => setDeleteDialog(null)}>
        <DialogTitle>Delete Brand</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete "{deleteDialog?.name}"? This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialog(null)}>Cancel</Button>
          <Button
            variant="contained"
            color="error"
            onClick={handleDelete}
            disabled={deleteMutation.isPending}
          >
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}
