import { TableRow, TableCell, Typography } from '@mui/material'
import { Person } from '@mui/icons-material'

export function UserTableEmptyState() {
  return (
    <TableRow>
      <TableCell colSpan={8} align="center" sx={{ py: 8 }}>
        <Person sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
        <Typography variant="h6" color="text.secondary">
          No users found
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Try adjusting your search or filters
        </Typography>
      </TableCell>
    </TableRow>
  )
}
