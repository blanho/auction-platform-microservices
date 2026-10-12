import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useSnackbar } from '@/shared/hooks/useSnackbar'
import { useMarkDelivered, useShipOrder } from './useOrders'

export function useOrderDetailActions(orderId: string | undefined) {
  const { t } = useTranslation('payments')
  const snackbar = useSnackbar()
  const [showShipDialog, setShowShipDialog] = useState(false)
  const [trackingNumber, setTrackingNumber] = useState('')
  const [shippingCarrier, setShippingCarrier] = useState('')
  const shipOrder = useShipOrder()
  const markDelivered = useMarkDelivered()

  const handleCopyOrderId = async () => {
    if (!orderId) {
      return
    }

    try {
      await navigator.clipboard.writeText(orderId)
      snackbar.show(t('orderDetail.copied'), 'success')
    } catch {
      snackbar.show(t('orderDetail.copyFailed'), 'error')
    }
  }

  const handleShipOrder = async () => {
    if (!trackingNumber || !shippingCarrier || !orderId) {
      return
    }

    try {
      await shipOrder.mutateAsync({
        id: orderId,
        data: { trackingNumber, shippingCarrier },
      })
      setShowShipDialog(false)
      setTrackingNumber('')
      setShippingCarrier('')
      snackbar.show(t('orderDetail.markedShipped'), 'success')
    } catch {
      snackbar.show(t('orderDetail.updateFailed'), 'error')
    }
  }

  const handleMarkDelivered = async () => {
    if (!orderId) {
      return
    }

    try {
      await markDelivered.mutateAsync(orderId)
      snackbar.show(t('orderDetail.markedDelivered'), 'success')
    } catch {
      snackbar.show(t('orderDetail.updateFailed'), 'error')
    }
  }

  return {
    showShipDialog,
    setShowShipDialog,
    trackingNumber,
    setTrackingNumber,
    shippingCarrier,
    setShippingCarrier,
    isShipping: shipOrder.isPending,
    isMarkingDelivered: markDelivered.isPending,
    snackbar,
    handleCopyOrderId,
    handleShipOrder,
    handleMarkDelivered,
  }
}
