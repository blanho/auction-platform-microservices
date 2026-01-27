import type { ReactNode } from 'react'
import { Box, Typography, Button, Paper, useTheme, alpha } from '@mui/material'
import { motion } from 'framer-motion'
import {
  Inbox as InboxIcon,
  SearchOff as SearchOffIcon,
  ShoppingCart as CartIcon,
  Gavel as AuctionIcon,
} from '@mui/icons-material'
import { fadeInUp, useReducedMotion } from '@/shared/lib/animations'

type EmptyStateVariant = 'default' | 'search' | 'cart' | 'auctions'

interface EmptyStateProps {
  variant?: EmptyStateVariant
  title?: string
  description?: string
  icon?: ReactNode
  action?: {
    label: string
    onClick: () => void
  }
  secondaryAction?: {
    label: string
    onClick: () => void
  }
}

const variantConfig: Record<EmptyStateVariant, { icon: ReactNode; title: string; description: string }> = {
  default: {
    icon: <InboxIcon sx={{ fontSize: 64 }} />,
    title: 'Nothing here yet',
    description: 'Check back later or try a different action.',
  },
  search: {
    icon: <SearchOffIcon sx={{ fontSize: 64 }} />,
    title: 'No results found',
    description: 'Try adjusting your search or filters to find what you\'re looking for.',
  },
  cart: {
    icon: <CartIcon sx={{ fontSize: 64 }} />,
    title: 'Your cart is empty',
    description: 'Browse our auctions and add items to your cart.',
  },
  auctions: {
    icon: <AuctionIcon sx={{ fontSize: 64 }} />,
    title: 'No auctions available',
    description: 'There are no auctions matching your criteria at this time.',
  },
}

export function EmptyState({
  variant = 'default',
  title,
  description,
  icon,
  action,
  secondaryAction,
}: EmptyStateProps) {
  const theme = useTheme()
  const prefersReducedMotion = useReducedMotion()
  const config = variantConfig[variant]

  const content = (
    <Paper
      elevation={0}
      sx={{
        p: 6,
        textAlign: 'center',
        backgroundColor: alpha(theme.palette.primary.main, 0.02),
        borderRadius: 3,
        border: `1px dashed ${alpha(theme.palette.divider, 0.5)}`,
      }}
    >
      <Box
        sx={{
          color: alpha(theme.palette.text.secondary, 0.5),
          mb: 2,
        }}
      >
        {icon || config.icon}
      </Box>
      <Typography variant="h6" fontWeight={600} gutterBottom>
        {title || config.title}
      </Typography>
      <Typography
        variant="body2"
        color="text.secondary"
        sx={{ maxWidth: 400, mx: 'auto', mb: action ? 3 : 0 }}
      >
        {description || config.description}
      </Typography>
      {(action || secondaryAction) && (
        <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center', mt: 3 }}>
          {action && (
            <Button
              variant="contained"
              onClick={action.onClick}
              sx={{ cursor: 'pointer' }}
            >
              {action.label}
            </Button>
          )}
          {secondaryAction && (
            <Button
              variant="outlined"
              onClick={secondaryAction.onClick}
              sx={{ cursor: 'pointer' }}
            >
              {secondaryAction.label}
            </Button>
          )}
        </Box>
      )}
    </Paper>
  )

  if (prefersReducedMotion) {
    return content
  }

  return (
    <motion.div
      initial="initial"
      animate="animate"
      variants={fadeInUp}
    >
      {content}
    </motion.div>
  )
}
