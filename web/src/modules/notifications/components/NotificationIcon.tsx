import {
  Notifications,
  Gavel,
  EmojiEvents,
  TrendingDown,
  Payment,
  AccessTime,
  Campaign,
} from '@mui/icons-material'
import { Box } from '@mui/material'
import type { NotificationType } from '../types'
import { getNotificationColor } from '../utils'

interface NotificationIconProps {
  type: NotificationType
  size?: number
  showBackground?: boolean
}

export function NotificationIcon({ type, size = 24, showBackground = false }: NotificationIconProps) {
  const color = getNotificationColor(type)
  const iconStyle = { fontSize: size, color }

  const getIcon = () => {
    switch (type) {
      case 'bid_placed':
        return <Gavel sx={iconStyle} />
      case 'bid_outbid':
        return <TrendingDown sx={iconStyle} />
      case 'auction_won':
      case 'auction_lost':
        return <EmojiEvents sx={iconStyle} />
      case 'auction_ending':
      case 'auction_ended':
        return <AccessTime sx={iconStyle} />
      case 'payment_received':
      case 'payment_failed':
        return <Payment sx={iconStyle} />
      case 'promotional':
        return <Campaign sx={iconStyle} />
      default:
        return <Notifications sx={iconStyle} />
    }
  }

  if (showBackground) {
    return (
      <Box
        sx={{
          width: size * 1.67,
          height: size * 1.67,
          borderRadius: '50%',
          bgcolor: '#F5F5F5',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        {getIcon()}
      </Box>
    )
  }

  return getIcon()
}
