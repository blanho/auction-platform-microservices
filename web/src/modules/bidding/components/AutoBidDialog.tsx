import { useState } from 'react'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Typography,
  Box,
  Stack,
  InputAdornment,
  Alert,
  Slider,
  Divider,
  FormControlLabel,
  Switch,
  IconButton,
} from '@mui/material'
import { Close, AutoMode, TrendingUp } from '@mui/icons-material'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { motion, AnimatePresence } from 'framer-motion'
import { http } from '@/services/http'
import { formatCurrency } from '@/shared/utils/formatters'
import { scaleIn } from '@/shared/lib/animations'

interface AutoBidDialogProps {
  open: boolean
  onClose: () => void
  auctionId: string
  auctionTitle: string
  currentBid: number
  minBidIncrement: number
  existingAutoBid?: {
    maxAmount: number
    isActive: boolean
  }
}

interface AutoBidFormData {
  maxAmount: number
  incrementAmount: number
  stopOnOutbid: boolean
}

export function AutoBidDialog({
  open,
  onClose,
  auctionId,
  auctionTitle,
  currentBid,
  minBidIncrement,
  existingAutoBid,
}: AutoBidDialogProps) {
  const queryClient = useQueryClient()
  const [maxAmount, setMaxAmount] = useState(existingAutoBid?.maxAmount || currentBid + minBidIncrement * 5)
  const [incrementAmount, setIncrementAmount] = useState(minBidIncrement)
  const [stopOnOutbid, setStopOnOutbid] = useState(false)
  const [success, setSuccess] = useState(false)

  const minMaxAmount = currentBid + minBidIncrement

  const createAutoBidMutation = useMutation({
    mutationFn: async (data: AutoBidFormData) => {
      const response = await http.post(`/bids/auto/${auctionId}`, data)
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['auction', auctionId] })
      queryClient.invalidateQueries({ queryKey: ['bids', auctionId] })
      setSuccess(true)
      setTimeout(() => {
        onClose()
        setSuccess(false)
      }, 2000)
    },
  })

  const cancelAutoBidMutation = useMutation({
    mutationFn: async () => {
      const response = await http.delete(`/bids/auto/${auctionId}`)
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['auction', auctionId] })
      onClose()
    },
  })

  const handleSubmit = () => {
    createAutoBidMutation.mutate({
      maxAmount,
      incrementAmount,
      stopOnOutbid,
    })
  }

  const potentialBids = Math.floor((maxAmount - currentBid) / incrementAmount)

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
      PaperProps={{
        component: motion.div,
        variants: scaleIn,
        initial: 'initial',
        animate: 'animate',
        exit: 'exit',
      }}
    >
      <DialogTitle>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <AutoMode sx={{ color: '#CA8A04' }} />
            <Typography variant="h6" fontWeight={600}>
              Auto-Bid Setup
            </Typography>
          </Box>
          <IconButton onClick={onClose} size="small">
            <Close />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent>
        <AnimatePresence mode="wait">
          {success ? (
            <motion.div
              key="success"
              initial={{ opacity: 0, scale: 0.9 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.9 }}
            >
              <Box sx={{ textAlign: 'center', py: 4 }}>
                <motion.div
                  initial={{ scale: 0 }}
                  animate={{ scale: 1 }}
                  transition={{ type: 'spring', stiffness: 200, damping: 15 }}
                >
                  <Box
                    sx={{
                      width: 64,
                      height: 64,
                      borderRadius: '50%',
                      bgcolor: 'success.light',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      mx: 'auto',
                      mb: 2,
                    }}
                  >
                    <TrendingUp sx={{ fontSize: 32, color: 'success.main' }} />
                  </Box>
                </motion.div>
                <Typography variant="h6" fontWeight={600} gutterBottom>
                  Auto-Bid Activated!
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  You'll automatically bid up to {formatCurrency(maxAmount)}
                </Typography>
              </Box>
            </motion.div>
          ) : (
            <motion.div
              key="form"
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
            >
              <Stack spacing={3} sx={{ pt: 1 }}>
                <Box sx={{ bgcolor: 'grey.50', p: 2, borderRadius: 1 }}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Auction
                  </Typography>
                  <Typography variant="body1" fontWeight={500} noWrap>
                    {auctionTitle}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                    Current bid: {formatCurrency(currentBid)}
                  </Typography>
                </Box>

                <Box>
                  <Typography variant="subtitle2" gutterBottom fontWeight={600}>
                    Maximum Bid Amount
                  </Typography>
                  <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1 }}>
                    We'll bid on your behalf up to this amount
                  </Typography>
                  <TextField
                    type="number"
                    value={maxAmount}
                    onChange={(e) => setMaxAmount(Math.max(minMaxAmount, Number(e.target.value)))}
                    fullWidth
                    slotProps={{
                      input: {
                        startAdornment: <InputAdornment position="start">$</InputAdornment>,
                      },
                    }}
                    error={maxAmount < minMaxAmount}
                    helperText={maxAmount < minMaxAmount ? `Minimum ${formatCurrency(minMaxAmount)}` : ''}
                  />
                  <Slider
                    value={maxAmount}
                    onChange={(_, value) => setMaxAmount(value as number)}
                    min={minMaxAmount}
                    max={minMaxAmount * 10}
                    step={minBidIncrement}
                    sx={{ mt: 2, color: '#CA8A04' }}
                  />
                </Box>

                <Box>
                  <Typography variant="subtitle2" gutterBottom fontWeight={600}>
                    Bid Increment
                  </Typography>
                  <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1 }}>
                    Amount to increase each automatic bid
                  </Typography>
                  <TextField
                    type="number"
                    value={incrementAmount}
                    onChange={(e) => setIncrementAmount(Math.max(minBidIncrement, Number(e.target.value)))}
                    fullWidth
                    slotProps={{
                      input: {
                        startAdornment: <InputAdornment position="start">$</InputAdornment>,
                      },
                    }}
                    helperText={`Minimum increment: ${formatCurrency(minBidIncrement)}`}
                  />
                </Box>

                <Divider />

                <FormControlLabel
                  control={
                    <Switch
                      checked={stopOnOutbid}
                      onChange={(e) => setStopOnOutbid(e.target.checked)}
                    />
                  }
                  label={
                    <Box>
                      <Typography variant="body2" fontWeight={500}>
                        Stop when outbid beyond max
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        Don't notify me for bids above my maximum
                      </Typography>
                    </Box>
                  }
                />

                <Box sx={{ bgcolor: 'info.lighter', p: 2, borderRadius: 1 }}>
                  <Typography variant="subtitle2" color="info.dark" gutterBottom>
                    Summary
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Auto-bid will place up to <strong>{potentialBids}</strong> bids, incrementing by {formatCurrency(incrementAmount)} each time, up to your maximum of {formatCurrency(maxAmount)}.
                  </Typography>
                </Box>

                {createAutoBidMutation.error && (
                  <Alert severity="error">
                    Failed to set up auto-bid. Please try again.
                  </Alert>
                )}
              </Stack>
            </motion.div>
          )}
        </AnimatePresence>
      </DialogContent>

      {!success && (
        <DialogActions sx={{ px: 3, pb: 3 }}>
          {existingAutoBid?.isActive && (
            <Button
              onClick={() => cancelAutoBidMutation.mutate()}
              disabled={cancelAutoBidMutation.isPending}
              color="error"
            >
              Cancel Auto-Bid
            </Button>
          )}
          <Box sx={{ flex: 1 }} />
          <Button onClick={onClose} variant="outlined">
            Cancel
          </Button>
          <Button
            onClick={handleSubmit}
            variant="contained"
            disabled={createAutoBidMutation.isPending || maxAmount < minMaxAmount}
            sx={{
              bgcolor: '#CA8A04',
              '&:hover': { bgcolor: '#A16207' },
            }}
          >
            {existingAutoBid?.isActive ? 'Update Auto-Bid' : 'Enable Auto-Bid'}
          </Button>
        </DialogActions>
      )}
    </Dialog>
  )
}
