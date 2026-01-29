import { useState } from 'react'
import { motion } from 'framer-motion'
import {
  Container,
  Card,
  Typography,
  Box,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Stack,
  Avatar,
  InputAdornment,
} from '@mui/material'
import { Add, Edit, Delete, Search, Image as ImageIcon } from '@mui/icons-material'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { palette } from '@/shared/theme/tokens'
import { useBrands, useCreateBrand, useUpdateBrand, useDeleteBrand } from '../hooks'
import { fadeInUp, staggerContainer } from '@/shared/lib/animations'
import type { Brand } from '../api/brands.api'
import { brandSchema, type BrandFormData } from '../schemas'

export function BrandsManagementPage() {
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
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

  const { data: brandsData } = useBrands({
    page: page + 1,
    pageSize: rowsPerPage,
    search: search || undefined,
  })
  const brands = brandsData?.items || []
  const totalCount = brandsData?.totalCount || 0

  const createMutation = useCreateBrand()
  const updateMutation = useUpdateBrand()
  const deleteMutation = useDeleteBrand()

  const handleOpenDialog = (brand?: Brand) => {
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
  }

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
            <Box sx={{ p: 2 }}>
              <TextField
                placeholder="Search brands..."
                size="small"
                value={search}
                onChange={(e) => {
                  setSearch(e.target.value)
                  setPage(0)
                }}
                slotProps={{
                  input: {
                    startAdornment: (
                      <InputAdornment position="start">
                        <Search />
                      </InputAdornment>
                    ),
                  },
                }}
                sx={{ width: 300 }}
              />
            </Box>

            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Brand</TableCell>
                    <TableCell>Slug</TableCell>
                    <TableCell>Auctions</TableCell>
                    <TableCell>Website</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {brands.map((brand) => (
                    <TableRow key={brand.id} hover>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                          <Avatar
                            src={brand.logoUrl}
                            variant="rounded"
                            sx={{ width: 48, height: 48, bgcolor: 'grey.100' }}
                          >
                            <ImageIcon sx={{ color: 'grey.400' }} />
                          </Avatar>
                          <Box>
                            <Typography variant="body2" fontWeight={500}>
                              {brand.name}
                            </Typography>
                            {brand.description && (
                              <Typography
                                variant="caption"
                                color="text.secondary"
                                noWrap
                                sx={{ maxWidth: 200, display: 'block' }}
                              >
                                {brand.description}
                              </Typography>
                            )}
                          </Box>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" color="text.secondary">
                          /{brand.slug}
                        </Typography>
                      </TableCell>
                      <TableCell>{brand.auctionCount ?? 0}</TableCell>
                      <TableCell>
                        {brand.websiteUrl ? (
                          <Typography
                            variant="body2"
                            component="a"
                            href={brand.websiteUrl}
                            target="_blank"
                            rel="noopener noreferrer"
                            sx={{ color: 'primary.main', textDecoration: 'none' }}
                          >
                            {brand.websiteUrl.replace(/^https?:\/\//, '')}
                          </Typography>
                        ) : (
                          '-'
                        )}
                      </TableCell>
                      <TableCell align="right">
                        <IconButton size="small" onClick={() => handleOpenDialog(brand)}>
                          <Edit fontSize="small" />
                        </IconButton>
                        <IconButton
                          size="small"
                          onClick={() => setDeleteDialog(brand)}
                          disabled={(brand.auctionCount ?? 0) > 0}
                        >
                          <Delete fontSize="small" />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>

            <TablePagination
              component="div"
              count={totalCount}
              page={page}
              onPageChange={(_, p) => setPage(p)}
              rowsPerPage={rowsPerPage}
              onRowsPerPageChange={(e) => {
                setRowsPerPage(parseInt(e.target.value))
                setPage(0)
              }}
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
