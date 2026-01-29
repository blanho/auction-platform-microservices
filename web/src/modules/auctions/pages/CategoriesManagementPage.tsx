import React, { useState } from 'react'
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
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Stack,
  Chip,
  Avatar,
} from '@mui/material'
import { Add, Edit, Delete, DragIndicator, ExpandMore, ExpandLess } from '@mui/icons-material'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { palette } from '@/shared/theme/tokens'
import { useCategories, useCreateCategory, useUpdateCategory, useDeleteCategory } from '../hooks'
import { fadeInUp, staggerContainer } from '@/shared/lib/animations'
import type { Category } from '../api/categories.api'
import { categorySchema, type CategoryFormData } from '../schemas'

export function CategoriesManagementPage() {
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingCategory, setEditingCategory] = useState<Category | null>(null)
  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set())

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CategoryFormData>({
    resolver: zodResolver(categorySchema),
    defaultValues: {
      name: '',
      slug: '',
      description: '',
    },
  })

  const { data: categories = [] } = useCategories({})

  const createMutation = useCreateCategory()
  const updateMutation = useUpdateCategory()
  const deleteMutation = useDeleteCategory()

  const handleOpenDialog = (category?: Category) => {
    if (category) {
      setEditingCategory(category)
      reset({
        name: category.name,
        slug: category.slug,
        description: category.description || '',
        parentId: category.parentId,
      })
    } else {
      setEditingCategory(null)
      reset({ name: '', slug: '', description: '' })
    }
    setDialogOpen(true)
  }

  const onSubmit = (data: CategoryFormData) => {
    if (editingCategory) {
      updateMutation.mutate(
        { id: editingCategory.id, data },
        {
          onSuccess: () => {
            setDialogOpen(false)
            setEditingCategory(null)
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

  const toggleExpand = (id: string) => {
    const newExpanded = new Set(expandedIds)
    if (newExpanded.has(id)) {
      newExpanded.delete(id)
    } else {
      newExpanded.add(id)
    }
    setExpandedIds(newExpanded)
  }

  const renderCategoryRow = (category: Category, depth = 0): React.ReactNode => {
    const hasChildren = category.children && category.children.length > 0
    const isExpanded = expandedIds.has(category.id)

    return (
      <>
        <TableRow key={category.id} hover>
          <TableCell>
            <Box sx={{ display: 'flex', alignItems: 'center', pl: depth * 4 }}>
              <IconButton size="small" sx={{ mr: 1, cursor: 'grab' }}>
                <DragIndicator fontSize="small" />
              </IconButton>
              {hasChildren && (
                <IconButton size="small" onClick={() => toggleExpand(category.id)}>
                  {isExpanded ? <ExpandLess /> : <ExpandMore />}
                </IconButton>
              )}
              {!hasChildren && <Box sx={{ width: 34 }} />}
              <Avatar
                src={category.imageUrl}
                variant="rounded"
                sx={{ width: 40, height: 40, mr: 2, bgcolor: 'grey.200' }}
              >
                {category.name[0]}
              </Avatar>
              <Box>
                <Typography variant="body2" fontWeight={500}>
                  {category.name}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  /{category.slug}
                </Typography>
              </Box>
            </Box>
          </TableCell>
          <TableCell>
            <Chip
              label={`${category.auctionCount ?? 0} auctions`}
              size="small"
              variant="outlined"
            />
          </TableCell>
          <TableCell>
            <Typography variant="body2" color="text.secondary" noWrap sx={{ maxWidth: 300 }}>
              {category.description || '-'}
            </Typography>
          </TableCell>
          <TableCell align="right">
            <IconButton size="small" onClick={() => handleOpenDialog(category)}>
              <Edit fontSize="small" />
            </IconButton>
            <IconButton
              size="small"
              onClick={() => deleteMutation.mutate(category.id)}
              disabled={(category.auctionCount ?? 0) > 0}
            >
              <Delete fontSize="small" />
            </IconButton>
          </TableCell>
        </TableRow>
        {hasChildren &&
          isExpanded &&
          category.children?.map((child) => renderCategoryRow(child, depth + 1))}
      </>
    )
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
              Categories
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
              Add Category
            </Button>
          </Box>
        </motion.div>

        <motion.div variants={fadeInUp}>
          <Card>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Category</TableCell>
                    <TableCell>Auctions</TableCell>
                    <TableCell>Description</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {categories
                    .filter((c: Category) => !c.parentId)
                    .map((category: Category) => renderCategoryRow(category))}
                </TableBody>
              </Table>
            </TableContainer>
          </Card>
        </motion.div>
      </motion.div>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingCategory ? 'Edit Category' : 'Add Category'}</DialogTitle>
        <form onSubmit={handleSubmit(onSubmit)}>
          <DialogContent>
            <Stack spacing={3} sx={{ pt: 1 }}>
              <Controller
                name="name"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label="Category Name"
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
              {editingCategory ? 'Update' : 'Create'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </Container>
  )
}
