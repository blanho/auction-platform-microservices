import { Box, Card, CardContent, Typography, Chip, Stack, Skeleton } from '@mui/material'
import { Shield, Person, Store, Settings } from '@mui/icons-material'
import type { RoleDto } from '../../types'

interface RoleCardProps {
  readonly role: RoleDto
  readonly selected: boolean
  readonly onClick: () => void
}

const ROLE_ICONS: Record<string, React.ReactNode> = {
  admin: <Shield />,
  seller: <Store />,
  user: <Person />,
}

const ROLE_COLORS: Record<string, string> = {
  admin: '#EF4444',
  seller: '#F59E0B',
  user: '#3B82F6',
}

export function RoleCard({ role, selected, onClick }: RoleCardProps) {
  const roleColor = ROLE_COLORS[role.name.toLowerCase()] || '#6B7280'
  const roleIcon = ROLE_ICONS[role.name.toLowerCase()] || <Settings />

  return (
    <Card
      onClick={onClick}
      sx={{
        cursor: 'pointer',
        transition: 'all 0.2s ease-in-out',
        border: '2px solid',
        borderColor: selected ? roleColor : 'divider',
        bgcolor: selected ? `${roleColor}08` : 'background.paper',
        '&:hover': {
          borderColor: roleColor,
          transform: 'translateY(-2px)',
          boxShadow: 2,
        },
      }}
    >
      <CardContent>
        <Stack direction="row" alignItems="center" spacing={2}>
          <Box
            sx={{
              width: 48,
              height: 48,
              borderRadius: 2,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              bgcolor: `${roleColor}15`,
              color: roleColor,
            }}
          >
            {roleIcon}
          </Box>
          <Box sx={{ flex: 1 }}>
            <Typography variant="h6" fontWeight={600} sx={{ textTransform: 'capitalize' }}>
              {role.name}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {role.description || 'No description'}
            </Typography>
          </Box>
          <Chip
            label={`${role.permissions.length} permissions`}
            size="small"
            sx={{
              bgcolor: `${roleColor}15`,
              color: roleColor,
              fontWeight: 500,
            }}
          />
        </Stack>
        {role.isSystemRole && (
          <Chip
            label="System Role"
            size="small"
            variant="outlined"
            sx={{ mt: 1.5 }}
          />
        )}
      </CardContent>
    </Card>
  )
}

export function RoleCardSkeleton() {
  return (
    <Card>
      <CardContent>
        <Stack direction="row" alignItems="center" spacing={2}>
          <Skeleton variant="rounded" width={48} height={48} />
          <Box sx={{ flex: 1 }}>
            <Skeleton variant="text" width="60%" height={28} />
            <Skeleton variant="text" width="80%" height={20} />
          </Box>
          <Skeleton variant="rounded" width={100} height={24} />
        </Stack>
      </CardContent>
    </Card>
  )
}
