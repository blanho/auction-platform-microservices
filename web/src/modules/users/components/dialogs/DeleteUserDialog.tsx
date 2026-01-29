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
  return (
    <ConfirmDialog
      open={open}
      onClose={onClose}
      onConfirm={onConfirm}
      title="Delete User"
      message={`Are you sure you want to delete ${getAdminUserDisplayName(user)}? This action is permanent and cannot be undone. All user data, auctions, bids, and transaction history will be deleted.`}
      confirmLabel="Delete User"
      cancelLabel="Cancel"
      variant="danger"
      loading={loading}
    />
  )
}
