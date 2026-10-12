import { useCancelAuction, useDeleteAuction, useExtendAuction } from '@/modules/auctions/hooks'
import { palette } from '@/shared/theme/tokens'
import { formatDateTime } from '@/shared/utils/formatters'
import {
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
  Typography,
} from '@mui/material'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'

interface AuctionManagementDialogsProps {
  deleteAuctionId: string | null
  cancelAuctionId: string | null
  auctionToExtend: { id: string; currentEndTime: string } | null
  onDeleteClose: () => void
  onCancelClose: () => void
  onExtendClose: () => void
  onFeedback: (message: string, severity: 'success' | 'error') => void
}

export function AuctionManagementDialogs({
  deleteAuctionId,
  cancelAuctionId,
  auctionToExtend,
  onDeleteClose,
  onCancelClose,
  onExtendClose,
  onFeedback,
}: Readonly<AuctionManagementDialogsProps>) {
  const { t } = useTranslation('users')
  const [cancelReason, setCancelReason] = useState('')
  const [newEndTime, setNewEndTime] = useState('')
  const deleteAuctionMutation = useDeleteAuction()
  const cancelAuctionMutation = useCancelAuction()
  const extendAuctionMutation = useExtendAuction()

  const handleCancelClose = () => {
    onCancelClose()
    setCancelReason('')
  }

  const handleExtendClose = () => {
    onExtendClose()
    setNewEndTime('')
  }

  const handleDeleteAuction = () => {
    if (!deleteAuctionId) {
      return
    }

    deleteAuctionMutation.mutate(deleteAuctionId, {
      onSuccess: () => {
        onFeedback(t('myAuctions.messages.deleted'), 'success')
        onDeleteClose()
      },
      onError: () => {
        onFeedback(t('myAuctions.messages.deleteError'), 'error')
      },
    })
  }

  const handleCancelAuction = () => {
    if (!cancelAuctionId) {
      return
    }

    cancelAuctionMutation.mutate(
      { id: cancelAuctionId, reason: cancelReason || undefined },
      {
        onSuccess: () => {
          onFeedback(t('myAuctions.messages.cancelled'), 'success')
          handleCancelClose()
        },
        onError: () => {
          onFeedback(t('myAuctions.messages.cancelError'), 'error')
        },
      }
    )
  }

  const handleExtendAuction = () => {
    if (!auctionToExtend || !newEndTime) {
      return
    }

    extendAuctionMutation.mutate(
      { id: auctionToExtend.id, newEndTime },
      {
        onSuccess: () => {
          onFeedback(t('myAuctions.messages.extended'), 'success')
          handleExtendClose()
        },
        onError: () => {
          onFeedback(t('myAuctions.messages.extendError'), 'error')
        },
      }
    )
  }

  return (
    <>
      <Dialog open={Boolean(deleteAuctionId)} onClose={onDeleteClose} maxWidth="xs" fullWidth>
        <DialogTitle sx={{ fontWeight: 600 }}>{t('myAuctions.deleteTitle')}</DialogTitle>
        <DialogContent>
          <Typography sx={{ color: palette.neutral[500] }}>
            {t('myAuctions.deleteDescription')}
          </Typography>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button
            onClick={onDeleteClose}
            sx={{ color: palette.neutral[500], textTransform: 'none' }}
          >
            {t('profile.cancel')}
          </Button>
          <Button
            variant="contained"
            onClick={handleDeleteAuction}
            disabled={deleteAuctionMutation.isPending}
            sx={{
              bgcolor: palette.semantic.error,
              textTransform: 'none',
              '&:hover': { bgcolor: palette.semantic.errorHover },
            }}
          >
            {deleteAuctionMutation.isPending ? (
              <CircularProgress size={20} sx={{ color: 'white' }} />
            ) : (
              t('myAuctions.delete')
            )}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={Boolean(cancelAuctionId)} onClose={handleCancelClose} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 600 }}>{t('myAuctions.cancelAuction')}</DialogTitle>
        <DialogContent>
          <Typography sx={{ color: palette.neutral[500], mb: 2 }}>
            {t('myAuctions.cancelDescription')}
          </Typography>
          <TextField
            fullWidth
            label={t('myAuctions.cancelReason')}
            multiline
            rows={3}
            value={cancelReason}
            onChange={(e) => setCancelReason(e.target.value)}
            placeholder={t('myAuctions.cancelReasonPlaceholder')}
            sx={{ mt: 1 }}
          />
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button
            onClick={handleCancelClose}
            sx={{ color: palette.neutral[500], textTransform: 'none' }}
          >
            {t('myAuctions.keepAuction')}
          </Button>
          <Button
            variant="contained"
            onClick={handleCancelAuction}
            disabled={cancelAuctionMutation.isPending}
            sx={{
              bgcolor: palette.semantic.warning,
              color: palette.neutral[900],
              textTransform: 'none',
              fontWeight: 600,
              '&:hover': { bgcolor: '#D97706' },
            }}
          >
            {cancelAuctionMutation.isPending ? (
              <CircularProgress size={20} />
            ) : (
              t('myAuctions.cancelAuction')
            )}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={Boolean(auctionToExtend)} onClose={handleExtendClose} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 600 }}>{t('myAuctions.extendAuction')}</DialogTitle>
        <DialogContent>
          <Typography sx={{ color: palette.neutral[500], mb: 2 }}>
            {t('myAuctions.extendDescription')}
          </Typography>
          {auctionToExtend && (
            <Typography sx={{ fontSize: '0.875rem', color: palette.neutral[600], mb: 2 }}>
              {t('myAuctions.currentEndTime', {
                time: formatDateTime(auctionToExtend.currentEndTime),
              })}
            </Typography>
          )}
          <TextField
            fullWidth
            label={t('myAuctions.newEndTime')}
            type="datetime-local"
            value={newEndTime}
            onChange={(e) => setNewEndTime(e.target.value)}
            slotProps={{
              inputLabel: { shrink: true },
              htmlInput: {
                min: auctionToExtend
                  ? new Date(auctionToExtend.currentEndTime).toISOString().slice(0, 16)
                  : undefined,
              },
            }}
            sx={{ mt: 1 }}
          />
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button
            onClick={handleExtendClose}
            sx={{ color: palette.neutral[500], textTransform: 'none' }}
          >
            {t('profile.cancel')}
          </Button>
          <Button
            variant="contained"
            onClick={handleExtendAuction}
            disabled={extendAuctionMutation.isPending || !newEndTime}
            sx={{
              bgcolor: palette.semantic.info,
              textTransform: 'none',
              fontWeight: 600,
              '&:hover': { bgcolor: '#0284C7' },
            }}
          >
            {extendAuctionMutation.isPending ? (
              <CircularProgress size={20} sx={{ color: 'white' }} />
            ) : (
              t('myAuctions.extendAuction')
            )}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  )
}
