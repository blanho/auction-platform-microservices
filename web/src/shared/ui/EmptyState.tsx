import { Box, Typography, Button } from '@mui/material'
import { useTranslation } from 'react-i18next'
import {
  Add,
  Search,
  Gavel,
  Inbox,
  ShoppingBag,
  Notifications,
  FavoriteBorder,
} from '@mui/icons-material'
import type { ReactNode } from 'react'
import { palette } from '@/shared/theme/tokens'

type EmptyStateVariant =
  | 'default'
  | 'auctions'
  | 'bids'
  | 'notifications'
  | 'search'
  | 'favorites'
  | 'orders'

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

const variantIcons: Record<EmptyStateVariant, ReactNode> = {
  default: <Inbox sx={{ fontSize: 64 }} />,
  auctions: <Gavel sx={{ fontSize: 64 }} />,
  bids: <Gavel sx={{ fontSize: 64 }} />,
  notifications: <Notifications sx={{ fontSize: 64 }} />,
  search: <Search sx={{ fontSize: 64 }} />,
  favorites: <FavoriteBorder sx={{ fontSize: 64 }} />,
  orders: <ShoppingBag sx={{ fontSize: 64 }} />,
}

export function EmptyState({
  variant = 'default',
  title,
  description,
  action,
  icon,
}: EmptyStateProps) {
  const { t } = useTranslation()
  const displayIcon = icon || variantIcons[variant]
  const displayTitle = title || t(`emptyState.${variant}.title`)
  const displayDescription = description || t(`emptyState.${variant}.description`)

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
