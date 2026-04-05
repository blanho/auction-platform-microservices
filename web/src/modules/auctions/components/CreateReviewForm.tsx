import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import {
  Box,
  Typography,
  TextField,
  Rating,
  Button,
  Stack,
  CircularProgress,
} from '@mui/material'
import { InlineAlert } from '@/shared/ui'
import { Star } from '@mui/icons-material'
import { palette } from '@/shared/theme/tokens'
import { useCreateReview } from '@/modules/users/hooks'
import type { CreateReviewRequest } from '@/modules/users/api/reviews.api'

interface CreateReviewFormProps {
  auctionId: string
  reviewedUserId: string
  reviewedUsername: string
  orderId?: string
  onSuccess?: () => void
  onCancel?: () => void
}

  const getRatingLabel = (value: number) => {
    const labels: Record<number, string> = {
      1: t('review.ratingPoor'),
      2: t('review.ratingFair'),
      3: t('review.ratingGood'),
      4: t('review.ratingVeryGood'),
      5: t('review.ratingExcellent'),
    }
    return labels[value] ?? ''
  }

export function CreateReviewForm({
  auctionId,
  reviewedUserId,
  reviewedUsername,
  orderId,
  onSuccess,
  onCancel,
}: Readonly<CreateReviewFormProps>) {
  const { t } = useTranslation('common')
  const [rating, setRating] = useState<number | null>(null)
  const [hover, setHover] = useState(-1)
  const [title, setTitle] = useState('')
  const [comment, setComment] = useState('')
  const [error, setError] = useState<string | null>(null)

  const createReview = useCreateReview()

  const handleSubmit = async () => {
    if (!rating) {
      setError(t('review.selectRatingError'))
      return
    }
    if (!comment.trim()) {
      setError(t('review.writeReviewError'))
      return
    }

    setError(null)

    const reviewData: CreateReviewRequest = {
      auctionId,
      reviewedUserId,
      reviewedUsername,
      rating,
      comment: comment.trim(),
      ...(title.trim() && { title: title.trim() }),
      ...(orderId && { orderId }),
    }

    try {
      await createReview.mutateAsync(reviewData)
      onSuccess?.()
    } catch {
      setError(t('review.submitFailed'))
    }
  }

  return (
    <Box
      sx={{
        p: 3,
        bgcolor: palette.neutral[50],
        borderRadius: 2,
        border: `1px solid ${palette.neutral[100]}`,
      }}
    >
      <Typography
        sx={{
          fontWeight: 600,
          color: palette.neutral[900],
          mb: 3,
          fontSize: '1.125rem',
        }}
      >
        {t('review.writeReview')}
      </Typography>

      {error && (
        <InlineAlert severity="error" sx={{ mb: 3 }}>
          {error}
        </InlineAlert>
      )}

      <Box sx={{ mb: 3 }}>
        <Typography sx={{ color: palette.neutral[500], mb: 1, fontSize: '0.875rem' }}>
          {t('review.ratingQuestion')}
        </Typography>
        <Stack direction="row" alignItems="center" spacing={2}>
          <Rating
            value={rating}
            onChange={(_, value) => setRating(value)}
            onChangeActive={(_, value) => setHover(value)}
            size="large"
            sx={{
              color: palette.brand.primary,
              '& .MuiRating-iconEmpty': { color: palette.neutral[100] },
            }}
            emptyIcon={<Star style={{ opacity: 0.55 }} fontSize="inherit" />}
          />
          {(rating !== null || hover >= 0) && (
            <Typography sx={{ color: palette.brand.primary, fontWeight: 500 }}>
              {getRatingLabel(hover >= 0 ? hover : rating ?? 0)}
            </Typography>
          )}
        </Stack>
      </Box>

      <TextField
        fullWidth
        label={t('review.reviewTitle')}
        placeholder={t('review.reviewTitlePlaceholder')}
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        sx={{ mb: 2 }}
        slotProps={{ htmlInput: { maxLength: 100 } }}
      />

      <TextField
        fullWidth
        label={t('review.yourReview')}
        placeholder={t('review.yourReviewPlaceholder')}
        value={comment}
        onChange={(e) => setComment(e.target.value)}
        multiline
        rows={4}
        sx={{ mb: 3 }}
        slotProps={{ htmlInput: { maxLength: 1000 } }}
        helperText={t('review.characters', { count: comment.length, max: 1000 })}
      />

      <Stack direction="row" spacing={2} justifyContent="flex-end">
        {onCancel && (
          <Button
            onClick={onCancel}
            sx={{
              color: palette.neutral[500],
              textTransform: 'none',
            }}
          >
            Cancel
          </Button>
        )}
        <Button
          variant="contained"
          onClick={handleSubmit}
          disabled={createReview.isPending}
          sx={{
            bgcolor: palette.brand.primary,
            textTransform: 'none',
            fontWeight: 600,
            px: 4,
            '&:hover': { bgcolor: palette.brand.hover },
          }}
        >
          {createReview.isPending ? (
            <CircularProgress size={20} sx={{ color: 'white' }} />
          ) : (
            t('review.submitReview')
          )}
        </Button>
      </Stack>
    </Box>
  )
}
