import { useTranslation } from 'react-i18next'
import { ConfirmDialog } from '@/shared/ui'
import { getAdminUserDisplayName } from '../../utils'
import type { AdminUser } from '../../types'

interface ActivateUserDialogProps {
  open: boolean
  user: AdminUser | null
  loading: boolean
  onClose: () => void
  onConfirm: () => void
}

export function ActivateUserDialog({
  open,
  user,
  loading,
  onClose,
  onConfirm,
}: ActivateUserDialogProps) {
  const { t } = useTranslation('common')
  return (
    <ConfirmDialog
      open={open}
      onClose={onClose}
      onConfirm={onConfirm}
      title={t('userManagement.activateTitle')}
      message={t('userManagement.activateMessage', { name: getAdminUserDisplayName(user) })}
      confirmLabel={t('userManagement.activateButton')}
      cancelLabel={t('cancel')}
      variant="info"
      loading={loading}
    />
  )
}
