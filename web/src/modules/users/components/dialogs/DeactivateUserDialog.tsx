import { ConfirmDialog } from '@/shared/ui'
import { getAdminUserDisplayName } from '../../utils'
import type { AdminUser } from '../../types'

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
  return (
    <ConfirmDialog
      open={open}
      onClose={onClose}
      onConfirm={onConfirm}
      title="Deactivate User"
      message={`Are you sure you want to deactivate ${getAdminUserDisplayName(user)}? This will disable the user's account and they will not be able to log in.`}
      confirmLabel="Deactivate User"
      cancelLabel="Cancel"
      variant="warning"
      loading={loading}
    />
  )
}
