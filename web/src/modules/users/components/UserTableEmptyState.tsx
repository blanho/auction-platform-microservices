import { useTranslation } from 'react-i18next'
import { TableEmptyStateRow } from '@/shared/ui'
import { Person } from '@mui/icons-material'

export function UserTableEmptyState() {
  const { t } = useTranslation('common')
  return (
    <TableEmptyStateRow
      colSpan={8}
      title={t('userManagement.noUsersTitle')}
      description={t('userManagement.noUsersDescription')}
      icon={<Person sx={{ fontSize: 48, color: 'text.secondary' }} />}
      cellSx={{ py: 8 }}
    />
  )
}
