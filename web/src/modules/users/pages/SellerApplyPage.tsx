import { useState } from 'react'
import { motion } from 'framer-motion'
import { Link } from 'react-router-dom'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation } from '@tanstack/react-query'
import { palette } from '@/shared/theme/tokens'
import {
  Container,
  Typography,
  Box,
  Card,
  Grid,
  Button,
  TextField,
  Divider,
  CircularProgress,
  Checkbox,
  FormControlLabel,
  Stack,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
} from '@mui/material'
import {
  Store,
  CheckCircle,
  TrendingUp,
  Security,
  Support,
  Gavel,
  ArrowBack,
} from '@mui/icons-material'
import { http } from '@/services/http'
import { fadeInUp, staggerContainer, staggerItem, scaleIn } from '@/shared/lib/animations'

const sellerSchema = z.object({
  businessName: z.string().min(2, 'Business name is required'),
  businessType: z.string().min(1, 'Business type is required'),
  taxId: z.string().optional(),
  phoneNumber: z.string().min(10, 'Valid phone number is required'),
  address: z.string().min(10, 'Address is required'),
  description: z.string().min(50, 'Tell us more about your business (min 50 characters)'),
  agreedToTerms: z.boolean().refine((val) => val === true, {
    message: 'You must agree to the terms',
  }),
})

type SellerFormData = z.infer<typeof sellerSchema>

const benefits = [
  {
    icon: <TrendingUp />,
    title: 'Grow Your Business',
    description: 'Reach thousands of collectors and buyers',
  },
  {
    icon: <Security />,
    title: 'Secure Transactions',
    description: 'Protected payments and buyer verification',
  },
  { icon: <Support />, title: 'Dedicated Support', description: '24/7 seller support team' },
  {
    icon: <Gavel />,
    title: 'Professional Tools',
    description: 'Analytics, inventory management, and more',
  },
]

export function SellerApplyPage() {
  const [success, setSuccess] = useState(false)

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<SellerFormData>({
    resolver: zodResolver(sellerSchema),
    defaultValues: {
      businessName: '',
      businessType: '',
      taxId: '',
      phoneNumber: '',
      address: '',
      description: '',
    },
  })

  const applyMutation = useMutation({
    mutationFn: async (data: Omit<SellerFormData, 'agreedToTerms'>) => {
      const response = await http.post('/users/seller/apply', data)
      return response.data
    },
    onSuccess: () => {
      setSuccess(true)
    },
  })

  const onSubmit = (data: SellerFormData) => {
    const { agreedToTerms: _, ...submitData } = data
    applyMutation.mutate(submitData)
  }

  if (success) {
    return (
      <Container maxWidth="sm" sx={{ py: { xs: 6, md: 8 }, minHeight: '60vh' }}>
        <motion.div variants={scaleIn} initial="initial" animate="animate">
          <Card sx={{ p: 6, textAlign: 'center' }}>
            <motion.div
              initial={{ scale: 0 }}
              animate={{ scale: 1 }}
              transition={{ type: 'spring', stiffness: 200, damping: 15 }}
            >
              <Box
                sx={{
                  width: 80,
                  height: 80,
                  borderRadius: '50%',
                  bgcolor: 'success.light',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  mx: 'auto',
                  mb: 3,
                }}
              >
                <CheckCircle sx={{ fontSize: 48, color: 'success.main' }} />
              </Box>
            </motion.div>

            <Typography
              variant="h4"
              sx={{
                fontFamily: '"Playfair Display", serif',
                fontWeight: 700,
                color: 'primary.main',
                mb: 2,
              }}
            >
              Application Submitted!
            </Typography>
            <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
              Thank you for applying to become a seller. We'll review your application and get back
              to you within 2-3 business days.
            </Typography>

            <Stack direction="row" spacing={2} justifyContent="center">
              <Button variant="outlined" component={Link} to="/dashboard">
                Go to Dashboard
              </Button>
              <Button variant="contained" component={Link} to="/auctions">
                Browse Auctions
              </Button>
            </Stack>
          </Card>
        </motion.div>
      </Container>
    )
  }

  return (
    <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
      <motion.div variants={staggerContainer} initial="initial" animate="animate">
        <motion.div variants={fadeInUp}>
          <Button
            startIcon={<ArrowBack />}
            component={Link}
            to="/dashboard"
            sx={{ mb: 3, color: 'text.secondary' }}
          >
            Back to Dashboard
          </Button>
        </motion.div>

        <Grid container spacing={4}>
          <Grid size={{ xs: 12, md: 5 }}>
            <motion.div variants={staggerItem}>
              <Box sx={{ position: 'sticky', top: 100 }}>
                <Box sx={{ textAlign: 'center', mb: 4 }}>
                  <Store sx={{ fontSize: 64, color: 'primary.main', mb: 2 }} />
                  <Typography
                    variant="h3"
                    sx={{
                      fontFamily: '"Playfair Display", serif',
                      fontWeight: 700,
                      color: 'primary.main',
                      mb: 2,
                    }}
                  >
                    Become a Seller
                  </Typography>
                  <Typography variant="body1" color="text.secondary">
                    Join our marketplace and start selling your unique items to collectors worldwide
                  </Typography>
                </Box>

                <Card sx={{ p: 3 }}>
                  <Typography variant="h6" fontWeight={600} gutterBottom>
                    Seller Benefits
                  </Typography>
                  <List>
                    {benefits.map((benefit) => (
                      <ListItem key={benefit.title} sx={{ px: 0 }}>
                        <ListItemIcon sx={{ color: 'primary.main', minWidth: 44 }}>
                          {benefit.icon}
                        </ListItemIcon>
                        <ListItemText
                          primary={benefit.title}
                          secondary={benefit.description}
                          slotProps={{ primary: { fontWeight: 600, variant: 'subtitle2' } }}
                        />
                      </ListItem>
                    ))}
                  </List>
                </Card>
              </Box>
            </motion.div>
          </Grid>

          <Grid size={{ xs: 12, md: 7 }}>
            <motion.div variants={staggerItem}>
              <Card sx={{ p: 4 }}>
                <Typography variant="h5" fontWeight={600} gutterBottom>
                  Seller Application
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                  Complete the form below to apply for a seller account
                </Typography>

                <form onSubmit={handleSubmit(onSubmit)}>
                  <Stack spacing={3}>
                    <Controller
                      name="businessName"
                      control={control}
                      render={({ field }) => (
                        <TextField
                          {...field}
                          label="Business / Store Name"
                          fullWidth
                          error={!!errors.businessName}
                          helperText={errors.businessName?.message}
                        />
                      )}
                    />

                    <Controller
                      name="businessType"
                      control={control}
                      render={({ field }) => (
                        <TextField
                          {...field}
                          label="Business Type"
                          fullWidth
                          error={!!errors.businessType}
                          helperText={
                            errors.businessType?.message ||
                            'e.g., Antique Dealer, Art Gallery, Individual Collector'
                          }
                        />
                      )}
                    />

                    <Controller
                      name="taxId"
                      control={control}
                      render={({ field }) => (
                        <TextField
                          {...field}
                          label="Tax ID / Business Registration (Optional)"
                          fullWidth
                          helperText="Required for business accounts"
                        />
                      )}
                    />

                    <Divider />

                    <Controller
                      name="phoneNumber"
                      control={control}
                      render={({ field }) => (
                        <TextField
                          {...field}
                          label="Phone Number"
                          fullWidth
                          error={!!errors.phoneNumber}
                          helperText={errors.phoneNumber?.message}
                        />
                      )}
                    />

                    <Controller
                      name="address"
                      control={control}
                      render={({ field }) => (
                        <TextField
                          {...field}
                          label="Business Address"
                          fullWidth
                          multiline
                          rows={2}
                          error={!!errors.address}
                          helperText={errors.address?.message}
                        />
                      )}
                    />

                    <Controller
                      name="description"
                      control={control}
                      render={({ field }) => (
                        <TextField
                          {...field}
                          label="Tell us about your business"
                          fullWidth
                          multiline
                          rows={4}
                          error={!!errors.description}
                          helperText={
                            errors.description?.message ||
                            'Describe what you sell, your expertise, and why you want to join our marketplace'
                          }
                        />
                      )}
                    />

                    <Divider />

                    <Controller
                      name="agreedToTerms"
                      control={control}
                      render={({ field }) => (
                        <FormControlLabel
                          control={
                            <Checkbox checked={Boolean(field.value)} onChange={field.onChange} />
                          }
                          label={
                            <Typography variant="body2">
                              I agree to the{' '}
                              <Button
                                component="a"
                                href="/terms"
                                sx={{ p: 0, minWidth: 'auto', textTransform: 'none' }}
                              >
                                Seller Terms of Service
                              </Button>{' '}
                              and{' '}
                              <Button
                                component="a"
                                href="/fees"
                                sx={{ p: 0, minWidth: 'auto', textTransform: 'none' }}
                              >
                                Fee Schedule
                              </Button>
                            </Typography>
                          }
                        />
                      )}
                    />
                    {errors.agreedToTerms && (
                      <InlineAlert severity="error" sx={{ mt: -2 }}>
                        {errors.agreedToTerms.message}
                      </InlineAlert>
                    )}

                    {applyMutation.error && (
                      <InlineAlert severity="error">
                        Failed to submit application. Please try again.
                      </InlineAlert>
                    )}

                    <Button
                      type="submit"
                      variant="contained"
                      size="large"
                      disabled={applyMutation.isPending}
                      sx={{
                        py: 1.5,
                        bgcolor: palette.brand.primary,
                        '&:hover': { bgcolor: '#A16207' },
                      }}
                    >
                      {applyMutation.isPending ? (
                        <CircularProgress size={24} />
                      ) : (
                        'Submit Application'
                      )}
                    </Button>
                  </Stack>
                </form>
              </Card>
            </motion.div>
          </Grid>
        </Grid>
      </motion.div>
    </Container>
  )
}
