import { useTranslation } from 'react-i18next'
import { Grid } from '@mui/material'
import { Person, TrendingUp, VerifiedUser, Warning } from '@mui/icons-material'
import { StatCard } from '@/shared/ui'
import { STAT_COLORS } from '../constants'
import type { UserStats } from '../types'

interface UserStatsGridProps {
  stats: UserStats | undefined
  loading: boolean
}

export function UserStatsGrid({ stats, loading }: UserStatsGridProps) {
  const { t } = useTranslation('common')
  return (
    <Grid container spacing={2} sx={{ mb: 3 }}>
      <Grid size={{ xs: 6, sm: 3 }}>
        <StatCard
          title={t('userManagement.totalUsers')}
          value={stats?.totalUsers || 0}
          icon={<Person />}
          color={STAT_COLORS.TOTAL_USERS}
          loading={loading}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <StatCard
          title={t('userManagement.activeUsers')}
          value={stats?.activeUsers || 0}
          icon={<TrendingUp />}
          color={STAT_COLORS.ACTIVE_USERS}
          loading={loading}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <StatCard
          title={t('userManagement.verified')}
          value={stats?.verifiedUsers || 0}
          icon={<VerifiedUser />}
          color={STAT_COLORS.VERIFIED_USERS}
          loading={loading}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <StatCard
          title={t('userManagement.suspended')}
          value={stats?.suspendedUsers || 0}
          icon={<Warning />}
          color={STAT_COLORS.SUSPENDED_USERS}
          loading={loading}
        />
      </Grid>
    </Grid>
  )
}
