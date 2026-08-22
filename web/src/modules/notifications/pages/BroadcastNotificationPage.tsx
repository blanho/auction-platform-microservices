import { InlineAlert } from '@/shared/ui'
import { Send } from '@mui/icons-material'
import {
  Box,
  Button,
  Card,
  Container,
  FormControl,
  Grid,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
} from '@mui/material'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useBroadcastNotification } from '../hooks'
import type { BroadcastNotificationDto, NotificationType } from '../types/notification.types'

const NOTIFICATION_TYPES: NotificationType[] = [
  'system',
  'promotional',
  'auction_ending',
  'auction_ended',
]

export function BroadcastNotificationPage() {
  const { t } = useTranslation('notifications')
  const [formData, setFormData] = useState<BroadcastNotificationDto>({
    type: 'system',
    title: '',
    message: '',
    targetRole: undefined,
  })
  const [success, setSuccess] = useState(false)

  const { mutate: broadcast, isPending } = useBroadcastNotification()

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    broadcast(formData, {
      onSuccess: () => {
        setSuccess(true)
        setFormData({
          type: 'system',
          title: '',
          message: '',
          targetRole: undefined,
        })
        setTimeout(() => setSuccess(false), 5000)
      },
    })
  }

  const handleChange = (field: keyof BroadcastNotificationDto, value: string) => {
    setFormData({ ...formData, [field]: value || undefined })
  }

  return (
    <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
      <Box sx={{ mb: 4 }}>
        <Typography
          variant="h4"
          sx={{
            fontFamily: '"Inter", sans-serif',
            fontWeight: 600,
            color: '#4C1D95',
            mb: 1,
          }}
        >
          {t('broadcast.title')}
        </Typography>
        <Typography sx={{ color: '#78716C', fontFamily: '"Inter", sans-serif' }}>
          {t('broadcast.description')}
        </Typography>
      </Box>

      {success && (
        <InlineAlert severity="success" sx={{ mb: 3, borderRadius: 2 }}>
          {t('broadcast.success')}
        </InlineAlert>
      )}

      <Card sx={{ p: 4, borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }}>
        <form onSubmit={handleSubmit}>
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, md: 6 }}>
              <FormControl fullWidth>
                <InputLabel>{t('broadcast.type')}</InputLabel>
                <Select
                  value={formData.type}
                  label={t('broadcast.type')}
                  onChange={(e) => handleChange('type', e.target.value)}
                  required
                >
                  {NOTIFICATION_TYPES.map((type) => (
                    <MenuItem key={type} value={type}>
                      {t(`broadcast.types.${type}`)}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                fullWidth
                label={t('broadcast.targetRole')}
                value={formData.targetRole || ''}
                onChange={(e) => handleChange('targetRole', e.target.value)}
                placeholder={t('broadcast.targetRolePlaceholder')}
                helperText={t('broadcast.targetRoleHelper')}
              />
            </Grid>

            <Grid size={{ xs: 12 }}>
              <TextField
                fullWidth
                label={t('broadcast.notificationTitle')}
                value={formData.title}
                onChange={(e) => handleChange('title', e.target.value)}
                required
                placeholder={t('broadcast.titlePlaceholder')}
              />
            </Grid>

            <Grid size={{ xs: 12 }}>
              <TextField
                fullWidth
                multiline
                rows={6}
                label={t('broadcast.message')}
                value={formData.message}
                onChange={(e) => handleChange('message', e.target.value)}
                required
                placeholder={t('broadcast.messagePlaceholder')}
              />
            </Grid>

            <Grid size={{ xs: 12 }}>
              <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
                <Button
                  type="button"
                  variant="outlined"
                  onClick={() =>
                    setFormData({
                      type: 'system',
                      title: '',
                      message: '',
                      targetRole: undefined,
                    })
                  }
                  disabled={isPending}
                  sx={{
                    borderColor: '#7C3AED',
                    color: '#7C3AED',
                    '&:hover': {
                      borderColor: '#6D28D9',
                      bgcolor: '#FAF5FF',
                    },
                  }}
                >
                  {t('broadcast.reset')}
                </Button>
                <Button
                  type="submit"
                  variant="contained"
                  disabled={isPending}
                  startIcon={<Send />}
                  sx={{
                    bgcolor: '#F97316',
                    '&:hover': { bgcolor: '#EA580C' },
                    textTransform: 'none',
                    px: 4,
                    transition: 'all 200ms',
                  }}
                >
                  {isPending ? t('broadcast.submitting') : t('broadcast.submit')}
                </Button>
              </Box>
            </Grid>
          </Grid>
        </form>
      </Card>

      <Card sx={{ mt: 3, p: 3, borderRadius: 2, bgcolor: '#FAF5FF' }}>
        <Typography variant="h6" sx={{ color: '#4C1D95', mb: 2, fontWeight: 600 }}>
          {t('broadcast.preview')}
        </Typography>
        <Box
          sx={{
            p: 2,
            bgcolor: 'white',
            borderRadius: 1,
            border: '1px solid #E9D5FF',
          }}
        >
          <Typography sx={{ fontWeight: 600, color: '#1C1917', mb: 1 }}>
            {formData.title || t('broadcast.previewTitle')}
          </Typography>
          <Typography sx={{ color: '#44403C', fontSize: '0.875rem' }}>
            {formData.message || t('broadcast.previewMessage')}
          </Typography>
        </Box>
      </Card>
    </Container>
  )
}
