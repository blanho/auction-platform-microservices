import { fadeInUp, staggerContainer } from '@/shared/lib/animations'
import type { FilterConfig } from '@/shared/ui'
import { InlineAlert, TableEmptyStateRow, TableSkeletonRows, TableToolbar } from '@/shared/ui'
import { History, Visibility } from '@mui/icons-material'
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Container,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  Tooltip,
  Typography,
} from '@mui/material'
import { motion } from 'framer-motion'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { AUDIT_ACTION } from '../constants'
import { useAuditLogs } from '../hooks/useAnalytics'
import type { AuditAction, AuditLog, AuditLogQueryParams } from '../types'
import {
  formatAuditTimestamp,
  formatChangedProperties,
  getAuditActionColor,
  getAuditActionLabel,
  getEntityTypeLabel,
  getServiceNameLabel,
  parseJsonSafely,
} from '../utils'

const ENTITY_TYPES = [
  'Auction',
  'Bid',
  'User',
  'Order',
  'Payment',
  'Category',
  'Brand',
  'Report',
  'Notification',
]

const SERVICE_NAMES = [
  'AuctionService',
  'BidService',
  'IdentityService',
  'PaymentService',
  'NotificationService',
  'AnalyticsService',
]

const ACTIONS = Object.values(AUDIT_ACTION)

export function AuditLogsPage() {
  const { t } = useTranslation('analytics')
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(20)
  const [search, setSearch] = useState('')
  const [entityType, setEntityType] = useState<string>('')
  const [serviceName, setServiceName] = useState<string>('')
  const [action, setAction] = useState<AuditAction | ''>('')
  const [selectedLog, setSelectedLog] = useState<AuditLog | null>(null)
  const [detailsOpen, setDetailsOpen] = useState(false)

  const filters: FilterConfig[] = useMemo(
    () => [
      {
        key: 'entityType',
        label: t('auditLogs.entityType'),
        options: ENTITY_TYPES.map((value) => ({ value, label: getEntityTypeLabel(value) })),
        minWidth: 160,
      },
      {
        key: 'serviceName',
        label: t('auditLogs.service'),
        options: SERVICE_NAMES.map((value) => ({ value, label: getServiceNameLabel(value) })),
        minWidth: 160,
      },
      {
        key: 'action',
        label: t('auditLogs.action'),
        options: ACTIONS.map((value) => ({ value, label: getAuditActionLabel(value) })),
        minWidth: 160,
      },
    ],
    [t]
  )

  const filterValues = useMemo(
    () => ({
      entityType,
      serviceName,
      action,
    }),
    [entityType, serviceName, action]
  )

  const handleFilterChange = (key: string, value: string) => {
    switch (key) {
      case 'entityType':
        setEntityType(value)
        break
      case 'serviceName':
        setServiceName(value)
        break
      case 'action':
        setAction(value as AuditAction | '')
        break
    }
    setPage(0)
  }

  const queryParams = useMemo<AuditLogQueryParams>(
    () => ({
      entityType: entityType || undefined,
      serviceName: serviceName || undefined,
      action: action || undefined,
      page: page + 1,
      pageSize: rowsPerPage,
    }),
    [entityType, serviceName, action, page, rowsPerPage]
  )

  const { data, isLoading, isError, refetch } = useAuditLogs(queryParams)

  const items = data?.items
  const filteredItems = useMemo(() => {
    if (!items) {
      return []
    }
    if (!search) {
      return items
    }
    const searchLower = search.toLowerCase()
    return items.filter(
      (log) =>
        log.entityType.toLowerCase().includes(searchLower) ||
        log.username?.toLowerCase().includes(searchLower) ||
        log.serviceName.toLowerCase().includes(searchLower) ||
        log.actionName.toLowerCase().includes(searchLower)
    )
  }, [items, search])

  const handleViewDetails = (log: AuditLog) => {
    setSelectedLog(log)
    setDetailsOpen(true)
  }

  const handleCloseDetails = () => {
    setDetailsOpen(false)
    setSelectedLog(null)
  }

  const handleClearFilters = () => {
    setSearch('')
    setEntityType('')
    setServiceName('')
    setAction('')
    setPage(0)
  }

  const hasFilters = search || entityType || serviceName || action

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <motion.div variants={staggerContainer} initial="initial" animate="animate">
        <motion.div variants={fadeInUp}>
          <Box
            sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}
          >
            <Box>
              <Typography
                variant="h4"
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontWeight: 700,
                  color: 'text.primary',
                }}
              >
                {t('auditLogs.title')}
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                {t('auditLogs.description')}
              </Typography>
            </Box>
          </Box>
        </motion.div>

        <motion.div variants={fadeInUp}>
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <TableToolbar
                searchValue={search}
                searchPlaceholder={t('auditLogs.searchPlaceholder')}
                onSearchChange={setSearch}
                filters={filters}
                filterValues={filterValues}
                onFilterChange={handleFilterChange}
                onClearFilters={handleClearFilters}
                onRefresh={() => refetch()}
              />
            </CardContent>
          </Card>
        </motion.div>

        <motion.div variants={fadeInUp}>
          <Card>
            {isError && (
              <InlineAlert severity="error" sx={{ m: 2 }}>
                {t('auditLogs.loadFailed')}
              </InlineAlert>
            )}

            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>{t('auditLogs.timestamp')}</TableCell>
                    <TableCell>{t('auditLogs.action')}</TableCell>
                    <TableCell>{t('auditLogs.entity')}</TableCell>
                    <TableCell>{t('auditLogs.user')}</TableCell>
                    <TableCell>{t('auditLogs.service')}</TableCell>
                    <TableCell>{t('auditLogs.changes')}</TableCell>
                    <TableCell align="right">{t('auditLogs.actions')}</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {isLoading && <TableSkeletonRows rows={10} columns={7} />}
                  {!isLoading && filteredItems.length === 0 && (
                    <TableEmptyStateRow
                      colSpan={7}
                      title={t('auditLogs.noLogs')}
                      description={
                        hasFilters ? t('auditLogs.adjustFilters') : t('auditLogs.logsWillAppear')
                      }
                      icon={<History sx={{ fontSize: 48, color: 'text.secondary' }} />}
                      cellSx={{ py: 8 }}
                    />
                  )}
                  {!isLoading &&
                    filteredItems.length > 0 &&
                    filteredItems.map((log) => (
                      <TableRow
                        key={log.id}
                        hover
                        sx={{ cursor: 'pointer' }}
                        onClick={() => handleViewDetails(log)}
                      >
                        <TableCell>
                          <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                            {formatAuditTimestamp(log.timestamp)}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={getAuditActionLabel(log.action)}
                            size="small"
                            color={getAuditActionColor(log.action)}
                          />
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2" fontWeight={500}>
                            {getEntityTypeLabel(log.entityType)}
                          </Typography>
                          <Typography
                            variant="caption"
                            color="text.secondary"
                            sx={{ fontFamily: 'monospace' }}
                          >
                            {log.entityId.substring(0, 8)}...
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2">{log.username || '-'}</Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2">
                            {getServiceNameLabel(log.serviceName)}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2" color="text.secondary">
                            {formatChangedProperties(log.changedProperties)}
                          </Typography>
                        </TableCell>
                        <TableCell align="right">
                          <Tooltip title={t('auditLogs.viewDetails')}>
                            <IconButton
                              size="small"
                              onClick={(e) => {
                                e.stopPropagation()
                                handleViewDetails(log)
                              }}
                            >
                              <Visibility fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    ))}
                </TableBody>
              </Table>
            </TableContainer>

            <TablePagination
              component="div"
              count={data?.totalCount || 0}
              page={page}
              onPageChange={(_, newPage) => setPage(newPage)}
              rowsPerPage={rowsPerPage}
              onRowsPerPageChange={(e) => {
                setRowsPerPage(Number.parseInt(e.target.value, 10))
                setPage(0)
              }}
              rowsPerPageOptions={[10, 20, 50, 100]}
            />
          </Card>
        </motion.div>
      </motion.div>

      <Dialog open={detailsOpen} onClose={handleCloseDetails} maxWidth="md" fullWidth>
        <DialogTitle
          sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}
        >
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <History />
            {t('auditLogs.details')}
          </Box>
          <Chip
            label={selectedLog ? getAuditActionLabel(selectedLog.action) : ''}
            color={selectedLog ? getAuditActionColor(selectedLog.action) : 'default'}
            size="small"
          />
        </DialogTitle>
        <DialogContent dividers>
          {selectedLog && (
            <Stack spacing={3}>
              <Box
                sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, gap: 2 }}
              >
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    {t('auditLogs.timestamp')}
                  </Typography>
                  <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                    {formatAuditTimestamp(selectedLog.timestamp)}
                  </Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    {t('auditLogs.entityType')}
                  </Typography>
                  <Typography variant="body2">
                    {getEntityTypeLabel(selectedLog.entityType)}
                  </Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    {t('auditLogs.entityId')}
                  </Typography>
                  <Typography
                    variant="body2"
                    sx={{ fontFamily: 'monospace', wordBreak: 'break-all' }}
                  >
                    {selectedLog.entityId}
                  </Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    {t('auditLogs.service')}
                  </Typography>
                  <Typography variant="body2">
                    {getServiceNameLabel(selectedLog.serviceName)}
                  </Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    {t('auditLogs.user')}
                  </Typography>
                  <Typography variant="body2">{selectedLog.username || '-'}</Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">
                    {t('auditLogs.userId')}
                  </Typography>
                  <Typography
                    variant="body2"
                    sx={{ fontFamily: 'monospace', wordBreak: 'break-all' }}
                  >
                    {selectedLog.userId}
                  </Typography>
                </Box>
                {selectedLog.ipAddress && (
                  <Box>
                    <Typography variant="caption" color="text.secondary">
                      {t('auditLogs.ipAddress')}
                    </Typography>
                    <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                      {selectedLog.ipAddress}
                    </Typography>
                  </Box>
                )}
                {selectedLog.correlationId && (
                  <Box>
                    <Typography variant="caption" color="text.secondary">
                      {t('auditLogs.correlationId')}
                    </Typography>
                    <Typography
                      variant="body2"
                      sx={{ fontFamily: 'monospace', wordBreak: 'break-all' }}
                    >
                      {selectedLog.correlationId}
                    </Typography>
                  </Box>
                )}
              </Box>

              {selectedLog.changedProperties && selectedLog.changedProperties.length > 0 && (
                <Box>
                  <Typography
                    variant="caption"
                    color="text.secondary"
                    sx={{ display: 'block', mb: 1 }}
                  >
                    {t('auditLogs.changedProperties')}
                  </Typography>
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                    {selectedLog.changedProperties.map((prop) => (
                      <Chip key={prop} label={prop} size="small" variant="outlined" />
                    ))}
                  </Box>
                </Box>
              )}

              {selectedLog.oldValues && (
                <Box>
                  <Typography
                    variant="caption"
                    color="text.secondary"
                    sx={{ display: 'block', mb: 1 }}
                  >
                    {t('auditLogs.oldValues')}
                  </Typography>
                  <Paper
                    variant="outlined"
                    sx={{ p: 2, bgcolor: 'error.50', maxHeight: 200, overflow: 'auto' }}
                  >
                    <Typography
                      component="pre"
                      variant="body2"
                      sx={{
                        fontFamily: 'monospace',
                        fontSize: '0.75rem',
                        m: 0,
                        whiteSpace: 'pre-wrap',
                      }}
                    >
                      {JSON.stringify(parseJsonSafely(selectedLog.oldValues), null, 2)}
                    </Typography>
                  </Paper>
                </Box>
              )}

              {selectedLog.newValues && (
                <Box>
                  <Typography
                    variant="caption"
                    color="text.secondary"
                    sx={{ display: 'block', mb: 1 }}
                  >
                    {t('auditLogs.newValues')}
                  </Typography>
                  <Paper
                    variant="outlined"
                    sx={{ p: 2, bgcolor: 'success.50', maxHeight: 200, overflow: 'auto' }}
                  >
                    <Typography
                      component="pre"
                      variant="body2"
                      sx={{
                        fontFamily: 'monospace',
                        fontSize: '0.75rem',
                        m: 0,
                        whiteSpace: 'pre-wrap',
                      }}
                    >
                      {JSON.stringify(parseJsonSafely(selectedLog.newValues), null, 2)}
                    </Typography>
                  </Paper>
                </Box>
              )}
            </Stack>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDetails}>{t('auditLogs.close')}</Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}
