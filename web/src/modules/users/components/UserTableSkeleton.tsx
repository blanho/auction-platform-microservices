import { TableRow, TableCell, Skeleton, Box } from '@mui/material'

interface UserTableSkeletonProps {
  rows?: number
}

export function UserTableSkeleton({ rows = 5 }: UserTableSkeletonProps) {
  return (
    <>
      {Array.from({ length: rows }).map((_, i) => (
        <TableRow key={i}>
          <TableCell>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <Skeleton variant="circular" width={40} height={40} />
              <Box>
                <Skeleton width={120} />
                <Skeleton width={180} />
              </Box>
            </Box>
          </TableCell>
          <TableCell>
            <Skeleton width={60} />
          </TableCell>
          <TableCell>
            <Skeleton width={70} />
          </TableCell>
          <TableCell>
            <Skeleton width={40} />
          </TableCell>
          <TableCell>
            <Skeleton width={40} />
          </TableCell>
          <TableCell>
            <Skeleton width={80} />
          </TableCell>
          <TableCell>
            <Skeleton width={80} />
          </TableCell>
          <TableCell>
            <Skeleton width={32} />
          </TableCell>
        </TableRow>
      ))}
    </>
  )
}
