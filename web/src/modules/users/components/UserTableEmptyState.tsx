import { TableEmptyStateRow } from '@/shared/ui'
import { Person } from '@mui/icons-material'

export function UserTableEmptyState() {
  return (
    <TableEmptyStateRow
      colSpan={8}
      title="No users found"
      description="Try adjusting your search or filters"
      icon={<Person sx={{ fontSize: 48, color: 'text.secondary' }} />}
      cellSx={{ py: 8 }}
    />
  )
}
