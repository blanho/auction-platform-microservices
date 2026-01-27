import { Box, Skeleton, Card, CardContent, Grid, Stack } from '@mui/material'
import { motion } from 'framer-motion'
import { shimmer, useReducedMotion } from '@/shared/lib/animations'

interface SkeletonCardProps {
  hasImage?: boolean
  hasActions?: boolean
  height?: number
}

export function SkeletonCard({ hasImage = true, hasActions = false, height = 200 }: SkeletonCardProps) {
  const prefersReducedMotion = useReducedMotion()
  
  const content = (
    <Card>
      {hasImage && (
        <Skeleton 
          variant="rectangular" 
          height={height} 
          animation={prefersReducedMotion ? false : 'wave'}
        />
      )}
      <CardContent>
        <Skeleton variant="text" width="80%" height={28} />
        <Skeleton variant="text" width="60%" height={20} sx={{ mt: 1 }} />
        <Skeleton variant="text" width="40%" height={20} sx={{ mt: 0.5 }} />
        {hasActions && (
          <Box sx={{ display: 'flex', gap: 1, mt: 2 }}>
            <Skeleton variant="rounded" width={80} height={36} />
            <Skeleton variant="rounded" width={80} height={36} />
          </Box>
        )}
      </CardContent>
    </Card>
  )

  if (prefersReducedMotion) {
    return content
  }

  return (
    <motion.div
      initial="initial"
      animate="animate"
      variants={shimmer}
    >
      {content}
    </motion.div>
  )
}

interface SkeletonGridProps {
  count?: number
  columns?: { xs?: number; sm?: number; md?: number; lg?: number }
  cardHeight?: number
}

export function SkeletonGrid({ 
  count = 8, 
  columns = { xs: 12, sm: 6, md: 4, lg: 3 },
  cardHeight = 200 
}: SkeletonGridProps) {
  return (
    <Grid container spacing={3}>
      {Array.from({ length: count }).map((_, index) => (
        <Grid key={index} size={columns}>
          <SkeletonCard height={cardHeight} />
        </Grid>
      ))}
    </Grid>
  )
}

export function SkeletonList({ count = 5 }: { count?: number }) {
  const prefersReducedMotion = useReducedMotion()
  
  return (
    <Stack spacing={2}>
      {Array.from({ length: count }).map((_, index) => (
        <Card key={index}>
          <CardContent>
            <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
              <Skeleton 
                variant="circular" 
                width={48} 
                height={48} 
                animation={prefersReducedMotion ? false : 'wave'}
              />
              <Box sx={{ flex: 1 }}>
                <Skeleton variant="text" width="60%" height={24} />
                <Skeleton variant="text" width="40%" height={18} />
              </Box>
              <Skeleton variant="rounded" width={60} height={32} />
            </Box>
          </CardContent>
        </Card>
      ))}
    </Stack>
  )
}

interface SkeletonTableProps {
  rows?: number
  columns?: number
}

export function SkeletonTable({ rows = 5, columns = 5 }: SkeletonTableProps) {
  const prefersReducedMotion = useReducedMotion()
  
  return (
    <Box>
      <Box sx={{ display: 'flex', gap: 2, py: 2, borderBottom: 1, borderColor: 'divider' }}>
        {Array.from({ length: columns }).map((_, i) => (
          <Skeleton 
            key={i} 
            variant="text" 
            width={`${100 / columns}%`} 
            height={24}
            animation={prefersReducedMotion ? false : 'wave'}
          />
        ))}
      </Box>
      {Array.from({ length: rows }).map((_, rowIndex) => (
        <Box key={rowIndex} sx={{ display: 'flex', gap: 2, py: 2, borderBottom: 1, borderColor: 'divider' }}>
          {Array.from({ length: columns }).map((_, colIndex) => (
            <Skeleton 
              key={colIndex} 
              variant="text" 
              width={`${100 / columns}%`} 
              height={20}
              animation={prefersReducedMotion ? false : 'wave'}
            />
          ))}
        </Box>
      ))}
    </Box>
  )
}

export function SkeletonDetail() {
  const prefersReducedMotion = useReducedMotion()
  
  return (
    <Grid container spacing={4}>
      <Grid size={{ xs: 12, md: 6 }}>
        <Skeleton 
          variant="rectangular" 
          height={400} 
          sx={{ borderRadius: 2 }}
          animation={prefersReducedMotion ? false : 'wave'}
        />
        <Box sx={{ display: 'flex', gap: 1, mt: 2 }}>
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} variant="rectangular" width={80} height={80} sx={{ borderRadius: 1 }} />
          ))}
        </Box>
      </Grid>
      <Grid size={{ xs: 12, md: 6 }}>
        <Skeleton variant="text" width="80%" height={40} />
        <Skeleton variant="text" width="40%" height={24} sx={{ mt: 1 }} />
        <Box sx={{ mt: 3 }}>
          <Skeleton variant="text" width="30%" height={48} />
          <Box sx={{ display: 'flex', gap: 2, mt: 3 }}>
            <Skeleton variant="rounded" width={120} height={48} />
            <Skeleton variant="rounded" width={120} height={48} />
          </Box>
        </Box>
        <Box sx={{ mt: 4 }}>
          <Skeleton variant="text" width="100%" height={20} />
          <Skeleton variant="text" width="100%" height={20} />
          <Skeleton variant="text" width="60%" height={20} />
        </Box>
      </Grid>
    </Grid>
  )
}

export function SkeletonProfile() {
  const prefersReducedMotion = useReducedMotion()
  
  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 3, mb: 4 }}>
        <Skeleton 
          variant="circular" 
          width={120} 
          height={120}
          animation={prefersReducedMotion ? false : 'wave'}
        />
        <Box sx={{ flex: 1 }}>
          <Skeleton variant="text" width="40%" height={36} />
          <Skeleton variant="text" width="30%" height={24} sx={{ mt: 1 }} />
          <Box sx={{ display: 'flex', gap: 2, mt: 2 }}>
            <Skeleton variant="rounded" width={100} height={36} />
            <Skeleton variant="rounded" width={100} height={36} />
          </Box>
        </Box>
      </Box>
      <Grid container spacing={2}>
        {Array.from({ length: 4 }).map((_, i) => (
          <Grid key={i} size={{ xs: 6, md: 3 }}>
            <Card>
              <CardContent sx={{ textAlign: 'center' }}>
                <Skeleton variant="text" width="60%" height={32} sx={{ mx: 'auto' }} />
                <Skeleton variant="text" width="80%" height={20} sx={{ mx: 'auto', mt: 1 }} />
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Box>
  )
}
