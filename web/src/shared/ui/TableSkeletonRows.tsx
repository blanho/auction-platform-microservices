import { Skeleton, TableCell, TableRow } from '@mui/material'

interface TableSkeletonRowsProps {
  rows?: number
  columns?: number
}

export function TableSkeletonRows({ rows = 5, columns = 5 }: TableSkeletonRowsProps) {
  return (
    <>
      {Array.from({ length: rows }).map((_, rowIndex) => (
        <TableRow key={rowIndex}>
          {Array.from({ length: columns }).map((_, colIndex) => (
            <TableCell key={colIndex}>
              <Skeleton />
            </TableCell>
          ))}
        </TableRow>
      ))}
    </>
  )
}
