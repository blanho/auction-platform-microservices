import { ConfirmDialog } from '@/shared/ui'
import { useTranslation } from 'react-i18next'
import type { AdminUser } from '../../types'
import { getAdminUserDisplayName } from '../../utils'

interface DeactivateUserDialogProps {
  open: boolean
  user: AdminUser | null
  loading: boolean
  onClose: () => void
  onConfirm: () => void
}

export function DeactivateUserDialog({
  open,
  user,
  loading,
  onClose,
  onConfirm,
}: DeactivateUserDialogProps) {
  const { t } = useTranslation('common')
  return (
    <ConfirmDialog
      open={open}
      onClose={onClose}
      onConfirm={onConfirm}
      title={t('userManagement.deactivateTitle')}
      message={t('userManagement.deactivateMessage', { name: getAdminUserDisplayName(user) })}
      confirmLabel={t('userManagement.deactivateButton')}
      cancelLabel={t('cancel')}
      variant="warning"
      loading={loading}
    />
  )
}
