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
  return (
    <ConfirmDialog
      open={open}
      onClose={onClose}
      onConfirm={onConfirm}
      title="Activate User"
      message={`Are you sure you want to activate ${getAdminUserDisplayName(user)}? This will allow the user to access the platform again.`}
      confirmLabel="Activate User"
      cancelLabel="Cancel"
      variant="info"
      loading={loading}
    />
  )
}
