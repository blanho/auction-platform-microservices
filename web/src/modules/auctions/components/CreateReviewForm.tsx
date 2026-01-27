import { useState } from 'react'
import {
  Box,
  Typography,
  TextField,
  Rating,
  Button,
  Stack,
  Alert,
  CircularProgress,
} from '@mui/material'
import { Star } from '@mui/icons-material'
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

const ratingLabels: { [index: number]: string } = {
  1: 'Poor',
  2: 'Fair',
  3: 'Good',
  4: 'Very Good',
  5: 'Excellent',
}

export function CreateReviewForm({
  auctionId,
  reviewedUserId,
  reviewedUsername,
  orderId,
  onSuccess,
  onCancel,
}: CreateReviewFormProps) {
  const [rating, setRating] = useState<number | null>(null)
  const [hover, setHover] = useState(-1)
  const [title, setTitle] = useState('')
  const [comment, setComment] = useState('')
  const [error, setError] = useState<string | null>(null)

  const createReview = useCreateReview()

  const handleSubmit = async () => {
    if (!rating) {
      setError('Please select a rating')
      return
    }
    if (!comment.trim()) {
      setError('Please write a review')
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
      setError('Failed to submit review. Please try again.')
    }
  }

  return (
    <Box
      sx={{
        p: 3,
        bgcolor: '#FAFAF9',
        borderRadius: 2,
        border: '1px solid #E5E5E5',
      }}
    >
      <Typography
        sx={{
          fontWeight: 600,
          color: '#1C1917',
          mb: 3,
          fontSize: '1.125rem',
        }}
      >
        Write a Review
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Box sx={{ mb: 3 }}>
        <Typography sx={{ color: '#78716C', mb: 1, fontSize: '0.875rem' }}>
          How would you rate your experience?
        </Typography>
        <Stack direction="row" alignItems="center" spacing={2}>
          <Rating
            value={rating}
            onChange={(_, value) => setRating(value)}
            onChangeActive={(_, value) => setHover(value)}
            size="large"
            sx={{
              color: '#CA8A04',
              '& .MuiRating-iconEmpty': { color: '#D6D3D1' },
            }}
            emptyIcon={<Star style={{ opacity: 0.55 }} fontSize="inherit" />}
          />
          {(rating !== null || hover !== -1) && (
            <Typography sx={{ color: '#CA8A04', fontWeight: 500 }}>
              {ratingLabels[hover !== -1 ? hover : rating!]}
            </Typography>
          )}
        </Stack>
      </Box>

      <TextField
        fullWidth
        label="Review Title (Optional)"
        placeholder="Summarize your experience"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        sx={{ mb: 2 }}
        inputProps={{ maxLength: 100 }}
      />

      <TextField
        fullWidth
        label="Your Review"
        placeholder="Share details of your experience with this seller"
        value={comment}
        onChange={(e) => setComment(e.target.value)}
        multiline
        rows={4}
        sx={{ mb: 3 }}
        inputProps={{ maxLength: 1000 }}
        helperText={`${comment.length}/1000 characters`}
      />

      <Stack direction="row" spacing={2} justifyContent="flex-end">
        {onCancel && (
          <Button
            onClick={onCancel}
            sx={{
              color: '#78716C',
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
            bgcolor: '#CA8A04',
            textTransform: 'none',
            fontWeight: 600,
            px: 4,
            '&:hover': { bgcolor: '#A16207' },
          }}
        >
          {createReview.isPending ? (
            <CircularProgress size={20} sx={{ color: 'white' }} />
          ) : (
            'Submit Review'
          )}
        </Button>
      </Stack>
    </Box>
  )
}
