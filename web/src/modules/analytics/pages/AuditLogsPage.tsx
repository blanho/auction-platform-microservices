import { useState, useMemo } from 'react'
import { motion } from 'framer-motion'
import {
  Container,
  Card,
  CardContent,
  Typography,
  Box,
  TextField,
  InputAdornment,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Chip,
  IconButton,
  MenuItem,
  Select,
  FormControl,
  InputLabel,
  Stack,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Skeleton,
  Alert,
  Tooltip,
  Paper,
} from '@mui/material'
import {
  Search,
  Visibility,
  Refresh,
  History,
  Close,
} from '@mui/icons-material'
import { useAuditLogs } from '../hooks/useAnalytics'
import { fadeInUp, staggerContainer } from '@/shared/lib/animations'
import type { AuditLog, AuditAction, AuditLogQueryParams } from '../types'
import {
  getAuditActionLabel,
  getAuditActionColor,
  getEntityTypeLabel,
  getServiceNameLabel,
  formatAuditTimestamp,
  parseJsonSafely,
  formatChangedProperties,
} from '../utils'
import { AUDIT_ACTION } from '../constants'

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

export function AuditLogsPage() {
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(20)
  const [search, setSearch] = useState('')
  const [entityType, setEntityType] = useState<string>('')
  const [serviceName, setServiceName] = useState<string>('')
  const [action, setAction] = useState<AuditAction | ''>('')
  const [selectedLog, setSelectedLog] = useState<AuditLog | null>(null)
  const [detailsOpen, setDetailsOpen] = useState(false)

  const queryParams = useMemo<AuditLogQueryParams>(() => ({
    entityType: entityType || undefined,
    serviceName: serviceName || undefined,
    action: action || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
  }), [entityType, serviceName, action, page, rowsPerPage])

  const { data, isLoading, isError, refetch } = useAuditLogs(queryParams)

  const items = data?.items
  const filteredItems = useMemo(() => {
    if (!items) return []
    if (!search) return items
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
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
            <Box>
              <Typography
                variant="h4"
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontWeight: 700,
                  color: 'text.primary',
                }}
              >
                Audit Logs
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                Track all system activities and changes
              </Typography>
            </Box>
            <Box sx={{ display: 'flex', gap: 1 }}>
              {hasFilters && (
                <Button
                  variant="outlined"
                  startIcon={<Close />}
                  onClick={handleClearFilters}
                  size="small"
                >
                  Clear Filters
                </Button>
              )}
              <IconButton onClick={() => refetch()} color="primary">
                <Refresh />
              </IconButton>
            </Box>
          </Box>
        </motion.div>

        <motion.div variants={fadeInUp}>
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
                <TextField
                  placeholder="Search by entity, user, service..."
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  size="small"
                  sx={{ minWidth: 280 }}
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <Search sx={{ color: 'text.secondary' }} />
                      </InputAdornment>
                    ),
                  }}
                />

                <FormControl size="small" sx={{ minWidth: 160 }}>
                  <InputLabel>Entity Type</InputLabel>
                  <Select
                    value={entityType}
                    onChange={(e) => setEntityType(e.target.value)}
                    label="Entity Type"
                  >
                    <MenuItem value="">All</MenuItem>
                    {ENTITY_TYPES.map((type) => (
                      <MenuItem key={type} value={type}>
                        {getEntityTypeLabel(type)}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>

                <FormControl size="small" sx={{ minWidth: 160 }}>
                  <InputLabel>Service</InputLabel>
                  <Select
                    value={serviceName}
                    onChange={(e) => setServiceName(e.target.value)}
                    label="Service"
                  >
                    <MenuItem value="">All</MenuItem>
                    {SERVICE_NAMES.map((name) => (
                      <MenuItem key={name} value={name}>
                        {getServiceNameLabel(name)}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>

                <FormControl size="small" sx={{ minWidth: 160 }}>
                  <InputLabel>Action</InputLabel>
                  <Select
                    value={action}
                    onChange={(e) => setAction(e.target.value as AuditAction | '')}
                    label="Action"
                  >
                    <MenuItem value="">All</MenuItem>
                    {Object.values(AUDIT_ACTION).map((act) => (
                      <MenuItem key={act} value={act}>
                        {getAuditActionLabel(act as AuditAction)}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Stack>
            </CardContent>
          </Card>
        </motion.div>

        <motion.div variants={fadeInUp}>
          <Card>
            {isError && (
              <Alert severity="error" sx={{ m: 2 }}>
                Failed to load audit logs. Please try again.
              </Alert>
            )}

            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Timestamp</TableCell>
                    <TableCell>Action</TableCell>
                    <TableCell>Entity</TableCell>
                    <TableCell>User</TableCell>
                    <TableCell>Service</TableCell>
                    <TableCell>Changes</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {isLoading ? (
                    Array.from({ length: 10 }).map((_, index) => (
                      <TableRow key={index}>
                        <TableCell><Skeleton variant="text" width={140} /></TableCell>
                        <TableCell><Skeleton variant="rounded" width={80} height={24} /></TableCell>
                        <TableCell><Skeleton variant="text" width={100} /></TableCell>
                        <TableCell><Skeleton variant="text" width={100} /></TableCell>
                        <TableCell><Skeleton variant="text" width={120} /></TableCell>
                        <TableCell><Skeleton variant="text" width={80} /></TableCell>
                        <TableCell><Skeleton variant="circular" width={32} height={32} /></TableCell>
                      </TableRow>
                    ))
                  ) : filteredItems.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={7} align="center" sx={{ py: 8 }}>
                        <History sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
                        <Typography variant="h6" color="text.secondary">
                          No audit logs found
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          {hasFilters ? 'Try adjusting your filters' : 'Audit logs will appear here'}
                        </Typography>
                      </TableCell>
                    </TableRow>
                  ) : (
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
                          <Typography variant="caption" color="text.secondary" sx={{ fontFamily: 'monospace' }}>
                            {log.entityId.substring(0, 8)}...
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2">
                            {log.username || '-'}
                          </Typography>
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
                          <Tooltip title="View Details">
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
                    ))
                  )}
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
                setRowsPerPage(parseInt(e.target.value, 10))
                setPage(0)
              }}
              rowsPerPageOptions={[10, 20, 50, 100]}
            />
          </Card>
        </motion.div>
      </motion.div>

      <Dialog
        open={detailsOpen}
        onClose={handleCloseDetails}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <History />
            Audit Log Details
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
              <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, gap: 2 }}>
                <Box>
                  <Typography variant="caption" color="text.secondary">Timestamp</Typography>
                  <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                    {formatAuditTimestamp(selectedLog.timestamp)}
                  </Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">Entity Type</Typography>
                  <Typography variant="body2">{getEntityTypeLabel(selectedLog.entityType)}</Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">Entity ID</Typography>
                  <Typography variant="body2" sx={{ fontFamily: 'monospace', wordBreak: 'break-all' }}>
                    {selectedLog.entityId}
                  </Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">Service</Typography>
                  <Typography variant="body2">{getServiceNameLabel(selectedLog.serviceName)}</Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">User</Typography>
                  <Typography variant="body2">{selectedLog.username || '-'}</Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">User ID</Typography>
                  <Typography variant="body2" sx={{ fontFamily: 'monospace', wordBreak: 'break-all' }}>
                    {selectedLog.userId}
                  </Typography>
                </Box>
                {selectedLog.ipAddress && (
                  <Box>
                    <Typography variant="caption" color="text.secondary">IP Address</Typography>
                    <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                      {selectedLog.ipAddress}
                    </Typography>
                  </Box>
                )}
                {selectedLog.correlationId && (
                  <Box>
                    <Typography variant="caption" color="text.secondary">Correlation ID</Typography>
                    <Typography variant="body2" sx={{ fontFamily: 'monospace', wordBreak: 'break-all' }}>
                      {selectedLog.correlationId}
                    </Typography>
                  </Box>
                )}
              </Box>

              {selectedLog.changedProperties && selectedLog.changedProperties.length > 0 && (
                <Box>
                  <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1 }}>
                    Changed Properties
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
                  <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1 }}>
                    Old Values
                  </Typography>
                  <Paper variant="outlined" sx={{ p: 2, bgcolor: 'error.50', maxHeight: 200, overflow: 'auto' }}>
                    <Typography
                      component="pre"
                      variant="body2"
                      sx={{ fontFamily: 'monospace', fontSize: '0.75rem', m: 0, whiteSpace: 'pre-wrap' }}
                    >
                      {JSON.stringify(parseJsonSafely(selectedLog.oldValues), null, 2)}
                    </Typography>
                  </Paper>
                </Box>
              )}

              {selectedLog.newValues && (
                <Box>
                  <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1 }}>
                    New Values
                  </Typography>
                  <Paper variant="outlined" sx={{ p: 2, bgcolor: 'success.50', maxHeight: 200, overflow: 'auto' }}>
                    <Typography
                      component="pre"
                      variant="body2"
                      sx={{ fontFamily: 'monospace', fontSize: '0.75rem', m: 0, whiteSpace: 'pre-wrap' }}
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
          <Button onClick={handleCloseDetails}>Close</Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}
