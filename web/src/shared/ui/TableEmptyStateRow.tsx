import { Box, Stack, TableCell, TableRow, Typography } from '@mui/material'
import type { ReactNode } from 'react'
import type { SxProps, Theme } from '@mui/material/styles'

interface TableEmptyStateRowProps {
  colSpan: number
  title: string
  description?: string
  icon?: ReactNode
  cellSx?: SxProps<Theme>
  actions?: ReactNode
}

export function TableEmptyStateRow({
  colSpan,
  title,
  description,
  icon,
  cellSx,
  actions,
}: TableEmptyStateRowProps) {
  return (
    <TableRow>
      <TableCell colSpan={colSpan} align="center" sx={[{ py: 6 }, cellSx]}>
        <Stack spacing={1} alignItems="center">
          {icon && <Box>{icon}</Box>}
          <Typography variant="h6" color="text.secondary">
            {title}
          </Typography>
          {description && (
            <Typography variant="body2" color="text.secondary">
              {description}
            </Typography>
          )}
          {actions && <Box>{actions}</Box>}
        </Stack>
      </TableCell>
    </TableRow>
  )
}
