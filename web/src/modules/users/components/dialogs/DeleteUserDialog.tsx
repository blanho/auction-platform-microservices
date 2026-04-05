import { useTranslation } from 'react-i18next'
import { ConfirmDialog } from '@/shared/ui'
import { getAdminUserDisplayName } from '../../utils'
import type { AdminUser } from '../../types'

interface DeleteUserDialogProps {
  open: boolean
  user: AdminUser | null
  loading: boolean
  onClose: () => void
  onConfirm: () => void
}

export function DeleteUserDialog({
  open,
  user,
  loading,
  onClose,
  onConfirm,
}: DeleteUserDialogProps) {
  const { t } = useTranslation('common')
  return (
    <ConfirmDialog
      open={open}
      onClose={onClose}
      onConfirm={onConfirm}
      title={t('userManagement.deleteTitle')}
      message={t('userManagement.deleteMessage', { name: getAdminUserDisplayName(user) })}
      confirmLabel={t('userManagement.deleteButton')}
      cancelLabel={t('cancel')}
      variant="danger"
      loading={loading}
    />
  )
}
