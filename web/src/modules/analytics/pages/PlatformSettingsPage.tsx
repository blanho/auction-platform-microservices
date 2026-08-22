import { fadeInUp, staggerContainer } from '@/shared/lib/animations'
import { InlineAlert, TableEmptyStateRow } from '@/shared/ui'
import {
  Add,
  Delete,
  Edit,
  Email,
  Gavel,
  Lock,
  Notifications,
  Refresh,
  Security,
  Settings,
} from '@mui/icons-material'
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
  FormControl,
  FormControlLabel,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  Skeleton,
  Stack,
  Switch,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tabs,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material'
import { motion } from 'framer-motion'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { SETTING_CATEGORY, SETTING_DATA_TYPES } from '../constants'
import { useSettings } from '../hooks/useAnalytics'
import { useCreateSetting, useDeleteSetting, useUpdateSetting } from '../hooks/useSettingsMutations'
import type { CreateSettingRequest, PlatformSetting, SettingCategory } from '../types'
import {
  formatSettingTimestamp,
  formatSettingValue,
  getSettingCategoryLabel,
  getSettingDataTypeLabel,
  validateSettingValue,
} from '../utils'

const CATEGORY_ICONS: Record<SettingCategory, React.ReactElement> = {
  Platform: <Settings />,
  Auction: <Gavel />,
  Notification: <Notifications />,
  Security: <Security />,
  Email: <Email />,
}

const CATEGORIES = Object.values(SETTING_CATEGORY) as SettingCategory[]

export function PlatformSettingsPage() {
  const { t } = useTranslation('analytics')
  const [selectedCategory, setSelectedCategory] = useState<SettingCategory>('Platform')
  const [editDialogOpen, setEditDialogOpen] = useState(false)
  const [createDialogOpen, setCreateDialogOpen] = useState(false)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [selectedSetting, setSelectedSetting] = useState<PlatformSetting | null>(null)
  const [editValue, setEditValue] = useState('')
  const [editError, setEditError] = useState<string | null>(null)

  const [newSetting, setNewSetting] = useState<CreateSettingRequest>({
    key: '',
    value: '',
    description: '',
    category: 'Platform',
    dataType: 'string',
  })

  const { data: settings, isLoading, isError, refetch } = useSettings(selectedCategory)

  const createMutation = useCreateSetting()
  const updateMutation = useUpdateSetting()
  const deleteMutation = useDeleteSetting()

  const groupedSettings = useMemo(() => {
    if (!settings) {
      return { system: [], custom: [] }
    }
    return {
      system: settings.filter((s) => s.isSystem),
      custom: settings.filter((s) => !s.isSystem),
    }
  }, [settings])

  const handleEditClick = (setting: PlatformSetting) => {
    setSelectedSetting(setting)
    setEditValue(setting.value)
    setEditError(null)
    setEditDialogOpen(true)
  }

  const handleEditSave = () => {
    if (!selectedSetting) {
      return
    }

    const error = validateSettingValue(editValue, selectedSetting.dataType)
    if (error) {
      setEditError(error)
      return
    }

    updateMutation.mutate(
      { id: selectedSetting.id, data: { value: editValue } },
      {
        onSuccess: () => {
          setEditDialogOpen(false)
          setSelectedSetting(null)
          setEditValue('')
        },
      }
    )
  }

  const handleDeleteClick = (setting: PlatformSetting) => {
    setSelectedSetting(setting)
    setDeleteDialogOpen(true)
  }

  const handleDeleteConfirm = () => {
    if (!selectedSetting) {
      return
    }

    deleteMutation.mutate(selectedSetting.id, {
      onSuccess: () => {
        setDeleteDialogOpen(false)
        setSelectedSetting(null)
      },
    })
  }

  const handleCreateSave = () => {
    createMutation.mutate(newSetting, {
      onSuccess: () => {
        setCreateDialogOpen(false)
        setNewSetting({
          key: '',
          value: '',
          description: '',
          category: selectedCategory,
          dataType: 'string',
        })
      },
    })
  }

  const handleCategoryChange = (_: React.SyntheticEvent, newValue: number) => {
    setSelectedCategory(CATEGORIES[newValue])
  }

  const renderSettingValue = (setting: PlatformSetting) => {
    if (setting.dataType === 'boolean') {
      return <Switch checked={setting.value === 'true'} disabled size="small" />
    }
    return (
      <Typography
        variant="body2"
        sx={{
          fontFamily: setting.dataType === 'json' ? 'monospace' : 'inherit',
          maxWidth: 300,
          overflow: 'hidden',
          textOverflow: 'ellipsis',
          whiteSpace: 'nowrap',
        }}
      >
        {formatSettingValue(setting.value, setting.dataType)}
      </Typography>
    )
  }

  const renderEditField = () => {
    if (!selectedSetting) {
      return null
    }

    if (selectedSetting.dataType === 'boolean') {
      return (
        <FormControlLabel
          control={
            <Switch
              checked={editValue === 'true'}
              onChange={(e) => setEditValue(e.target.checked ? 'true' : 'false')}
            />
          }
          label={t(editValue === 'true' ? 'settings.enabled' : 'settings.disabled')}
        />
      )
    }

    if (selectedSetting.dataType === 'json') {
      return (
        <TextField
          fullWidth
          multiline
          rows={6}
          value={editValue}
          onChange={(e) => {
            setEditValue(e.target.value)
            setEditError(null)
          }}
          error={!!editError}
          helperText={editError}
          sx={{ fontFamily: 'monospace' }}
        />
      )
    }

    return (
      <TextField
        fullWidth
        value={editValue}
        onChange={(e) => {
          setEditValue(e.target.value)
          setEditError(null)
        }}
        error={!!editError}
        helperText={editError}
        type={
          selectedSetting.dataType === 'number' ||
          selectedSetting.dataType === 'percentage' ||
          selectedSetting.dataType === 'currency'
            ? 'number'
            : 'text'
        }
      />
    )
  }

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
                {t('settings.title')}
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                {t('settings.description')}
              </Typography>
            </Box>
            <Box sx={{ display: 'flex', gap: 1 }}>
              <Button
                variant="contained"
                startIcon={<Add />}
                onClick={() => {
                  setNewSetting((prev) => ({ ...prev, category: selectedCategory }))
                  setCreateDialogOpen(true)
                }}
              >
                {t('settings.addSetting')}
              </Button>
              <IconButton onClick={() => refetch()} color="primary">
                <Refresh />
              </IconButton>
            </Box>
          </Box>
        </motion.div>

        <motion.div variants={fadeInUp}>
          <Card sx={{ mb: 3 }}>
            <Tabs
              value={CATEGORIES.indexOf(selectedCategory)}
              onChange={handleCategoryChange}
              variant="scrollable"
              scrollButtons="auto"
              sx={{ borderBottom: 1, borderColor: 'divider' }}
            >
              {CATEGORIES.map((cat) => (
                <Tab
                  key={cat}
                  icon={CATEGORY_ICONS[cat]}
                  label={getSettingCategoryLabel(cat)}
                  iconPosition="start"
                  sx={{ minHeight: 64 }}
                />
              ))}
            </Tabs>
          </Card>
        </motion.div>

        <motion.div variants={fadeInUp}>
          {isError && (
            <InlineAlert severity="error" sx={{ mb: 3 }}>
              {t('settings.loadFailed')}
            </InlineAlert>
          )}

          {groupedSettings.system.length > 0 && (
            <Card sx={{ mb: 3 }}>
              <CardContent sx={{ pb: 0 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  <Lock sx={{ fontSize: 20, color: 'text.secondary' }} />
                  <Typography variant="subtitle1" fontWeight={600}>
                    {t('settings.systemSettings')}
                  </Typography>
                  <Chip label={t('settings.protected')} size="small" color="warning" />
                </Box>
              </CardContent>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>{t('settings.key')}</TableCell>
                      <TableCell>{t('settings.value')}</TableCell>
                      <TableCell>{t('settings.type')}</TableCell>
                      <TableCell>{t('settings.updated')}</TableCell>
                      <TableCell align="right">{t('settings.actions')}</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {isLoading
                      ? Array.from({ length: 3 }).map((_, i) => (
                          <TableRow key={i}>
                            <TableCell>
                              <Skeleton variant="text" width={150} />
                            </TableCell>
                            <TableCell>
                              <Skeleton variant="text" width={200} />
                            </TableCell>
                            <TableCell>
                              <Skeleton variant="text" width={80} />
                            </TableCell>
                            <TableCell>
                              <Skeleton variant="text" width={120} />
                            </TableCell>
                            <TableCell>
                              <Skeleton variant="circular" width={32} height={32} />
                            </TableCell>
                          </TableRow>
                        ))
                      : groupedSettings.system.map((setting) => (
                          <TableRow key={setting.id} hover sx={{ cursor: 'pointer' }}>
                            <TableCell>
                              <Tooltip title={setting.description || ''}>
                                <Typography
                                  variant="body2"
                                  fontWeight={500}
                                  sx={{ fontFamily: 'monospace' }}
                                >
                                  {setting.key}
                                </Typography>
                              </Tooltip>
                            </TableCell>
                            <TableCell>{renderSettingValue(setting)}</TableCell>
                            <TableCell>
                              <Chip
                                label={getSettingDataTypeLabel(setting.dataType || 'string')}
                                size="small"
                                variant="outlined"
                              />
                            </TableCell>
                            <TableCell>
                              <Typography variant="caption" color="text.secondary">
                                {formatSettingTimestamp(setting.updatedAt)}
                              </Typography>
                            </TableCell>
                            <TableCell align="right">
                              <Tooltip title={t('common:edit')}>
                                <IconButton size="small" onClick={() => handleEditClick(setting)}>
                                  <Edit fontSize="small" />
                                </IconButton>
                              </Tooltip>
                            </TableCell>
                          </TableRow>
                        ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Card>
          )}

          <Card>
            <CardContent sx={{ pb: 0 }}>
              <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 2 }}>
                {t('settings.customSettings')}
              </Typography>
            </CardContent>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>{t('settings.key')}</TableCell>
                    <TableCell>{t('settings.value')}</TableCell>
                    <TableCell>{t('settings.type')}</TableCell>
                    <TableCell>{t('settings.descriptionLabel')}</TableCell>
                    <TableCell>{t('settings.updated')}</TableCell>
                    <TableCell align="right">{t('settings.actions')}</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {isLoading &&
                    Array.from({ length: 5 }).map((_, i) => (
                      <TableRow key={i}>
                        <TableCell>
                          <Skeleton variant="text" width={150} />
                        </TableCell>
                        <TableCell>
                          <Skeleton variant="text" width={200} />
                        </TableCell>
                        <TableCell>
                          <Skeleton variant="text" width={80} />
                        </TableCell>
                        <TableCell>
                          <Skeleton variant="text" width={150} />
                        </TableCell>
                        <TableCell>
                          <Skeleton variant="text" width={120} />
                        </TableCell>
                        <TableCell>
                          <Skeleton variant="circular" width={32} height={32} />
                        </TableCell>
                      </TableRow>
                    ))}
                  {!isLoading && groupedSettings.custom.length === 0 && (
                    <TableEmptyStateRow
                      colSpan={6}
                      title={t('settings.noCustomSettings')}
                      description={t('settings.clickToCreate')}
                      icon={<Settings sx={{ fontSize: 48, color: 'text.secondary' }} />}
                      cellSx={{ py: 8 }}
                    />
                  )}
                  {!isLoading &&
                    groupedSettings.custom.length > 0 &&
                    groupedSettings.custom.map((setting) => (
                      <TableRow key={setting.id} hover sx={{ cursor: 'pointer' }}>
                        <TableCell>
                          <Typography
                            variant="body2"
                            fontWeight={500}
                            sx={{ fontFamily: 'monospace' }}
                          >
                            {setting.key}
                          </Typography>
                        </TableCell>
                        <TableCell>{renderSettingValue(setting)}</TableCell>
                        <TableCell>
                          <Chip
                            label={getSettingDataTypeLabel(setting.dataType || 'string')}
                            size="small"
                            variant="outlined"
                          />
                        </TableCell>
                        <TableCell>
                          <Typography
                            variant="body2"
                            color="text.secondary"
                            sx={{ maxWidth: 200 }}
                            noWrap
                          >
                            {setting.description || '-'}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="caption" color="text.secondary">
                            {formatSettingTimestamp(setting.updatedAt)}
                          </Typography>
                        </TableCell>
                        <TableCell align="right">
                          <Tooltip title={t('common:edit')}>
                            <IconButton size="small" onClick={() => handleEditClick(setting)}>
                              <Edit fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title={t('common:delete')}>
                            <IconButton
                              size="small"
                              onClick={() => handleDeleteClick(setting)}
                              color="error"
                            >
                              <Delete fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    ))}
                </TableBody>
              </Table>
            </TableContainer>
          </Card>
        </motion.div>
      </motion.div>

      <Dialog
        open={editDialogOpen}
        onClose={() => setEditDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>{t('settings.editSetting')}</DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ pt: 1 }}>
            <Box>
              <Typography variant="caption" color="text.secondary">
                {t('settings.key')}
              </Typography>
              <Typography variant="body1" sx={{ fontFamily: 'monospace' }}>
                {selectedSetting?.key}
              </Typography>
            </Box>
            {selectedSetting?.description && (
              <Box>
                <Typography variant="caption" color="text.secondary">
                  {t('settings.descriptionLabel')}
                </Typography>
                <Typography variant="body2">{selectedSetting.description}</Typography>
              </Box>
            )}
            <Box>
              <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1 }}>
                {t('settings.value')}
              </Typography>
              {renderEditField()}
            </Box>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditDialogOpen(false)}>{t('common:cancel')}</Button>
          <Button variant="contained" onClick={handleEditSave} disabled={updateMutation.isPending}>
            {updateMutation.isPending ? t('settings.saving') : t('common:save')}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={createDialogOpen}
        onClose={() => setCreateDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>{t('settings.createSetting')}</DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ pt: 1 }}>
            <TextField
              label={t('settings.key')}
              fullWidth
              value={newSetting.key}
              onChange={(e) => setNewSetting((prev) => ({ ...prev, key: e.target.value }))}
              placeholder={t('settings.keyPlaceholder')}
              helperText={t('settings.keyHelperText')}
            />
            <FormControl fullWidth>
              <InputLabel>{t('settings.dataType')}</InputLabel>
              <Select
                value={newSetting.dataType}
                onChange={(e) => setNewSetting((prev) => ({ ...prev, dataType: e.target.value }))}
                label={t('settings.dataType')}
              >
                {SETTING_DATA_TYPES.map((type) => (
                  <MenuItem key={type} value={type}>
                    {getSettingDataTypeLabel(type)}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <TextField
              label={t('settings.value')}
              fullWidth
              value={newSetting.value}
              onChange={(e) => setNewSetting((prev) => ({ ...prev, value: e.target.value }))}
              multiline={newSetting.dataType === 'json'}
              rows={newSetting.dataType === 'json' ? 4 : 1}
            />
            <TextField
              label={t('settings.descriptionLabel')}
              fullWidth
              value={newSetting.description}
              onChange={(e) => setNewSetting((prev) => ({ ...prev, description: e.target.value }))}
              multiline
              rows={2}
            />
            <FormControl fullWidth>
              <InputLabel>{t('seller.category')}</InputLabel>
              <Select
                value={newSetting.category}
                onChange={(e) =>
                  setNewSetting((prev) => ({
                    ...prev,
                    category: e.target.value as SettingCategory,
                  }))
                }
                label={t('seller.category')}
              >
                {CATEGORIES.map((cat) => (
                  <MenuItem key={cat} value={cat}>
                    {getSettingCategoryLabel(cat)}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCreateDialogOpen(false)}>{t('common:cancel')}</Button>
          <Button
            variant="contained"
            onClick={handleCreateSave}
            disabled={createMutation.isPending || !newSetting.key || !newSetting.value}
          >
            {createMutation.isPending ? t('settings.creating') : t('common:create')}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>{t('settings.deleteSetting')}</DialogTitle>
        <DialogContent>
          <InlineAlert severity="warning" sx={{ mb: 2 }}>
            {t('settings.deleteWarning')}
          </InlineAlert>
          <Typography>
            {t('settings.deleteConfirm')} <strong>{selectedSetting?.key}</strong>?
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>{t('common:cancel')}</Button>
          <Button
            variant="contained"
            color="error"
            onClick={handleDeleteConfirm}
            disabled={deleteMutation.isPending}
          >
            {deleteMutation.isPending ? t('settings.deleting') : t('common:delete')}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}
