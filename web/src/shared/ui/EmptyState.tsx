import { Box, Typography, Button } from '@mui/material'
import { Add, Search, Gavel, Inbox, ShoppingBag, Notifications, FavoriteBorder } from '@mui/icons-material'
import type { ReactNode } from 'react'
import { palette } from '@/shared/theme/tokens'

type EmptyStateVariant = 'default' | 'auctions' | 'bids' | 'notifications' | 'search' | 'favorites' | 'orders'

interface EmptyStateProps {
  variant?: EmptyStateVariant
  title?: string
  description?: string
  action?: {
    label: string
    onClick?: () => void
    href?: string
  }
  icon?: ReactNode
}

const variantConfig: Record<EmptyStateVariant, { icon: ReactNode; title: string; description: string }> = {
  default: {
    icon: <Inbox sx={{ fontSize: 64 }} />,
    title: 'No data yet',
    description: 'There is nothing here at the moment.',
  },
  auctions: {
    icon: <Gavel sx={{ fontSize: 64 }} />,
    title: 'No auctions found',
    description: 'Create your first auction and start selling to collectors worldwide.',
  },
  bids: {
    icon: <Gavel sx={{ fontSize: 64 }} />,
    title: 'No bids yet',
    description: 'You haven\'t placed any bids. Start exploring auctions to find unique items.',
  },
  notifications: {
    icon: <Notifications sx={{ fontSize: 64 }} />,
    title: 'All caught up!',
    description: 'You have no new notifications at the moment.',
  },
  search: {
    icon: <Search sx={{ fontSize: 64 }} />,
    title: 'No results found',
    description: 'Try adjusting your search or filters to find what you\'re looking for.',
  },
  favorites: {
    icon: <FavoriteBorder sx={{ fontSize: 64 }} />,
    title: 'No favorites yet',
    description: 'Save items you love to keep track of them and get notified of updates.',
  },
  orders: {
    icon: <ShoppingBag sx={{ fontSize: 64 }} />,
    title: 'No orders yet',
    description: 'Win an auction to see your orders here.',
  },
}

export function EmptyState({ variant = 'default', title, description, action, icon }: EmptyStateProps) {
  const config = variantConfig[variant]
  const displayIcon = icon || config.icon
  const displayTitle = title || config.title
  const displayDescription = description || config.description

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        textAlign: 'center',
        py: 8,
        px: 4,
      }}
    >
      <Box
        sx={{
          width: 120,
          height: 120,
          borderRadius: '50%',
          bgcolor: palette.neutral[100],
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          mb: 3,
          color: palette.neutral[400],
        }}
      >
        {displayIcon}
      </Box>

      <Typography
        variant="h6"
        sx={{
          fontWeight: 600,
          color: palette.neutral[900],
          mb: 1,
        }}
      >
        {displayTitle}
      </Typography>

      <Typography
        sx={{
          color: palette.neutral[500],
          maxWidth: 320,
          mb: action ? 3 : 0,
        }}
      >
        {displayDescription}
      </Typography>

      {action && (
        <Button
          variant="outlined"
          href={action.href}
          onClick={action.onClick}
          startIcon={<Add />}
          sx={{
            borderColor: palette.brand.primary,
            color: palette.brand.primary,
            textTransform: 'none',
            fontWeight: 600,
            '&:hover': {
              borderColor: palette.brand.secondary,
              bgcolor: palette.brand.muted,
            },
          }}
        >
          {action.label}
        </Button>
      )}
    </Box>
  )
}
