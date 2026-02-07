import { useState } from 'react'
import {
  Box,
  Container,
  Typography,
  Card,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Button,
  Grid,
} from '@mui/material'
import { Send } from '@mui/icons-material'
import { InlineAlert } from '@/shared/ui'
import { useBroadcastNotification } from '../hooks'
import type { NotificationType, BroadcastNotificationDto } from '../types/notification.types'

const NOTIFICATION_TYPES: { value: NotificationType; label: string }[] = [
  { value: 'system', label: 'System' },
  { value: 'promotional', label: 'Promotional' },
  { value: 'auction_ending', label: 'Auction Ending' },
  { value: 'auction_ended', label: 'Auction Ended' },
]

export function BroadcastNotificationPage() {
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
    <Container maxWidth="lg" sx={{ py: 4 }}>
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
          Broadcast Notification
        </Typography>
        <Typography sx={{ color: '#78716C', fontFamily: '"Inter", sans-serif' }}>
          Send a notification to all users or a specific role
        </Typography>
      </Box>

      {success && (
        <InlineAlert severity="success" sx={{ mb: 3, borderRadius: 2 }}>
          Notification broadcast successfully!
        </InlineAlert>
      )}

      <Card sx={{ p: 4, borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }}>
        <form onSubmit={handleSubmit}>
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, md: 6 }}>
              <FormControl fullWidth>
                <InputLabel>Notification Type</InputLabel>
                <Select
                  value={formData.type}
                  label="Notification Type"
                  onChange={(e) => handleChange('type', e.target.value)}
                  required
                >
                  {NOTIFICATION_TYPES.map((type) => (
                    <MenuItem key={type.value} value={type.value}>
                      {type.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                fullWidth
                label="Target Role (Optional)"
                value={formData.targetRole || ''}
                onChange={(e) => handleChange('targetRole', e.target.value)}
                placeholder="e.g., admin, seller, buyer"
                helperText="Leave empty to broadcast to all users"
              />
            </Grid>

            <Grid size={{ xs: 12 }}>
              <TextField
                fullWidth
                label="Title"
                value={formData.title}
                onChange={(e) => handleChange('title', e.target.value)}
                required
                placeholder="Enter notification title"
              />
            </Grid>

            <Grid size={{ xs: 12 }}>
              <TextField
                fullWidth
                multiline
                rows={6}
                label="Message"
                value={formData.message}
                onChange={(e) => handleChange('message', e.target.value)}
                required
                placeholder="Enter notification message"
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
                  Reset
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
                  {isPending ? 'Broadcasting...' : 'Broadcast Notification'}
                </Button>
              </Box>
            </Grid>
          </Grid>
        </form>
      </Card>

      <Card sx={{ mt: 3, p: 3, borderRadius: 2, bgcolor: '#FAF5FF' }}>
        <Typography variant="h6" sx={{ color: '#4C1D95', mb: 2, fontWeight: 600 }}>
          Preview
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
            {formData.title || 'Notification title will appear here'}
          </Typography>
          <Typography sx={{ color: '#44403C', fontSize: '0.875rem' }}>
            {formData.message || 'Notification message will appear here'}
          </Typography>
        </Box>
      </Card>
    </Container>
  )
}
