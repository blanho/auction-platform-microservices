import { http } from '@/services/http'
import { fadeInUp, scaleIn, staggerContainer, staggerItem } from '@/shared/lib/animations'
import { palette } from '@/shared/theme/tokens'
import { InlineAlert } from '@/shared/ui'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  ArrowBack,
  CheckCircle,
  Gavel,
  Security,
  Store,
  Support,
  TrendingUp,
} from '@mui/icons-material'
import {
  Box,
  Button,
  Card,
  Checkbox,
  CircularProgress,
  Container,
  Divider,
  FormControlLabel,
  Grid,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { useMutation } from '@tanstack/react-query'
import { motion } from 'framer-motion'
import type { TFunction } from 'i18next'
import { useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { z } from 'zod'

const createSellerSchema = (t: TFunction<'users'>) =>
  z.object({
    businessName: z.string().min(2, t('sellerApply.validation.businessName')),
    businessType: z.string().min(1, t('sellerApply.validation.businessType')),
    taxId: z.string().optional(),
    phoneNumber: z.string().min(10, t('sellerApply.validation.phoneNumber')),
    address: z.string().min(10, t('sellerApply.validation.address')),
    description: z.string().min(50, t('sellerApply.validation.description')),
    agreedToTerms: z.boolean().refine((val) => val === true, {
      message: t('sellerApply.validation.terms'),
    }),
  })

type SellerFormData = z.infer<ReturnType<typeof createSellerSchema>>

export function SellerApplyPage() {
  const { t } = useTranslation('users')
  const [success, setSuccess] = useState(false)
  const sellerSchema = createSellerSchema(t)
  const benefits = [
    {
      icon: <TrendingUp />,
      title: t('sellerApply.benefits.growth.title'),
      description: t('sellerApply.benefits.growth.description'),
    },
    {
      icon: <Security />,
      title: t('sellerApply.benefits.security.title'),
      description: t('sellerApply.benefits.security.description'),
    },
    {
      icon: <Support />,
      title: t('sellerApply.benefits.support.title'),
      description: t('sellerApply.benefits.support.description'),
    },
    {
      icon: <Gavel />,
      title: t('sellerApply.benefits.tools.title'),
      description: t('sellerApply.benefits.tools.description'),
    },
  ]

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
              {t('sellerApply.successTitle')}
            </Typography>
            <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
              {t('sellerApply.successDescription')}
            </Typography>

            <Stack direction="row" spacing={2} justifyContent="center">
              <Button variant="outlined" component={Link} to="/dashboard">
                {t('sellerApply.goToDashboard')}
              </Button>
              <Button variant="contained" component={Link} to="/auctions">
                {t('sellerApply.browseAuctions')}
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
            {t('sellerApply.backToDashboard')}
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
                    {t('profile.becomeSeller')}
                  </Typography>
                  <Typography variant="body1" color="text.secondary">
                    {t('sellerApply.heroDescription')}
                  </Typography>
                </Box>

                <Card sx={{ p: 3 }}>
                  <Typography variant="h6" fontWeight={600} gutterBottom>
                    {t('sellerApply.benefitsTitle')}
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
                  {t('sellerApply.formTitle')}
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                  {t('sellerApply.formDescription')}
                </Typography>

                <form onSubmit={handleSubmit(onSubmit)}>
                  <Stack spacing={3}>
                    <Controller
                      name="businessName"
                      control={control}
                      render={({ field }) => (
                        <TextField
                          {...field}
                          label={t('sellerApply.businessName')}
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
                          label={t('sellerApply.businessType')}
                          fullWidth
                          error={!!errors.businessType}
                          helperText={
                            errors.businessType?.message || t('sellerApply.businessTypeHelp')
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
                          label={t('sellerApply.taxId')}
                          fullWidth
                          helperText={t('sellerApply.taxIdHelp')}
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
                          label={t('profile.phone')}
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
                          label={t('sellerApply.businessAddress')}
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
                          label={t('sellerApply.businessDescription')}
                          fullWidth
                          multiline
                          rows={4}
                          error={!!errors.description}
                          helperText={
                            errors.description?.message || t('sellerApply.businessDescriptionHelp')
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
                              {t('sellerApply.agreePrefix')}{' '}
                              <Button
                                component="a"
                                href="/terms"
                                sx={{ p: 0, minWidth: 'auto', textTransform: 'none' }}
                              >
                                {t('sellerApply.sellerTerms')}
                              </Button>{' '}
                              {t('sellerApply.and')}{' '}
                              <Button
                                component="a"
                                href="/fees"
                                sx={{ p: 0, minWidth: 'auto', textTransform: 'none' }}
                              >
                                {t('sellerApply.feeSchedule')}
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
                      <InlineAlert severity="error">{t('sellerApply.submitError')}</InlineAlert>
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
                        t('sellerApply.submit')
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
