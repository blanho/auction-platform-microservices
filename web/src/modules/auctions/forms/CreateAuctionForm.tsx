import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Box,
  TextField,
  Button,
  Grid,
  InputAdornment,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormHelperText,
} from '@mui/material'
import { createAuctionSchema, type CreateAuctionFormData } from '../schemas'
import { useActiveCategories } from '../hooks'
import { ITEM_CONDITIONS, CURRENCIES } from '../constants'

interface CreateAuctionFormProps {
  onSubmit: (data: CreateAuctionFormData) => void
  isLoading?: boolean
}

export const CreateAuctionForm = ({ onSubmit, isLoading }: CreateAuctionFormProps) => {
  const { data: categories = [] } = useActiveCategories()
  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<CreateAuctionFormData>({
    resolver: zodResolver(createAuctionSchema),
    defaultValues: {
      currency: 'USD',
      isFeatured: false,
    },
  })

  return (
    <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={3}>
        <Grid size={{ xs: 12 }}>
          <TextField
            fullWidth
            label="Title"
            {...register('title')}
            error={!!errors.title}
            helperText={errors.title?.message}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            fullWidth
            label="Description"
            multiline
            rows={4}
            {...register('description')}
            error={!!errors.description}
            helperText={errors.description?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="categoryId"
            control={control}
            render={({ field }) => (
              <FormControl fullWidth error={!!errors.categoryId}>
                <InputLabel>Category</InputLabel>
                <Select {...field} label="Category">
                  {categories.map((cat) => (
                    <MenuItem key={cat.id} value={cat.id}>
                      {cat.name}
                    </MenuItem>
                  ))}
                </Select>
                {errors.categoryId && (
                  <FormHelperText>{errors.categoryId.message}</FormHelperText>
                )}
              </FormControl>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="condition"
            control={control}
            render={({ field }) => (
              <FormControl fullWidth>
                <InputLabel>Condition</InputLabel>
                <Select {...field} label="Condition">
                  {ITEM_CONDITIONS.map((cond) => (
                    <MenuItem key={cond.value} value={cond.value}>
                      {cond.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            fullWidth
            label="Starting Price (Reserve)"
            type="number"
            slotProps={{
              input: {
                startAdornment: <InputAdornment position="start">$</InputAdornment>,
              },
            }}
            {...register('reservePrice', { valueAsNumber: true })}
            error={!!errors.reservePrice}
            helperText={errors.reservePrice?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            fullWidth
            label="Buy Now Price (Optional)"
            type="number"
            slotProps={{
              input: {
                startAdornment: <InputAdornment position="start">$</InputAdornment>,
              },
            }}
            {...register('buyNowPrice', { valueAsNumber: true })}
            error={!!errors.buyNowPrice}
            helperText={errors.buyNowPrice?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="currency"
            control={control}
            render={({ field }) => (
              <FormControl fullWidth>
                <InputLabel>Currency</InputLabel>
                <Select {...field} label="Currency">
                  {CURRENCIES.map((cur) => (
                    <MenuItem key={cur.value} value={cur.value}>
                      {cur.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            fullWidth
            label="Auction End"
            type="datetime-local"
            slotProps={{ inputLabel: { shrink: true } }}
            {...register('auctionEnd')}
            error={!!errors.auctionEnd}
            helperText={errors.auctionEnd?.message}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <Button
            type="submit"
            variant="contained"
            size="large"
            fullWidth
            disabled={isLoading}
          >
            {isLoading ? 'Creating...' : 'Create Auction'}
          </Button>
        </Grid>
      </Grid>
    </Box>
  )
}
