import { useState, useMemo } from 'react'
import { motion } from 'framer-motion'
import {
  Container,
  Card,
  CardContent,
  Typography,
  Box,
  TextField,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
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
  Tabs,
  Tab,
  Switch,
  FormControlLabel,
} from '@mui/material'
import {
  Edit,
  Add,
  Delete,
  Refresh,
  Settings,
  Gavel,
  Notifications,
  Security,
  Email,
  Lock,
} from '@mui/icons-material'
import { useSettings } from '../hooks/useAnalytics'
import { useCreateSetting, useUpdateSetting, useDeleteSetting } from '../hooks/useSettingsMutations'
import { fadeInUp, staggerContainer } from '@/shared/lib/animations'
import type { PlatformSetting, SettingCategory, CreateSettingRequest } from '../types'
import {
  getSettingCategoryLabel,
  getSettingDataTypeLabel,
  formatSettingValue,
  formatSettingTimestamp,
  validateSettingValue,
} from '../utils'
import { SETTING_CATEGORY, SETTING_DATA_TYPES } from '../constants'

const CATEGORY_ICONS: Record<SettingCategory, React.ReactElement> = {
  Platform: <Settings />,
  Auction: <Gavel />,
  Notification: <Notifications />,
  Security: <Security />,
  Email: <Email />,
}

const CATEGORIES = Object.values(SETTING_CATEGORY) as SettingCategory[]

export function PlatformSettingsPage() {
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
    if (!settings) return { system: [], custom: [] }
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
    if (!selectedSetting) return

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
    if (!selectedSetting) return

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
      return (
        <Switch
          checked={setting.value === 'true'}
          disabled
          size="small"
        />
      )
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
    if (!selectedSetting) return null

    if (selectedSetting.dataType === 'boolean') {
      return (
        <FormControlLabel
          control={
            <Switch
              checked={editValue === 'true'}
              onChange={(e) => setEditValue(e.target.checked ? 'true' : 'false')}
            />
          }
          label={editValue === 'true' ? 'Enabled' : 'Disabled'}
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
        type={selectedSetting.dataType === 'number' || selectedSetting.dataType === 'percentage' || selectedSetting.dataType === 'currency' ? 'number' : 'text'}
      />
    )
  }

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
                Platform Settings
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                Manage platform configuration and preferences
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
                Add Setting
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
            <Alert severity="error" sx={{ mb: 3 }}>
              Failed to load settings. Please try again.
            </Alert>
          )}

          {groupedSettings.system.length > 0 && (
            <Card sx={{ mb: 3 }}>
              <CardContent sx={{ pb: 0 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  <Lock sx={{ fontSize: 20, color: 'text.secondary' }} />
                  <Typography variant="subtitle1" fontWeight={600}>
                    System Settings
                  </Typography>
                  <Chip label="Protected" size="small" color="warning" />
                </Box>
              </CardContent>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Key</TableCell>
                      <TableCell>Value</TableCell>
                      <TableCell>Type</TableCell>
                      <TableCell>Updated</TableCell>
                      <TableCell align="right">Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {isLoading ? (
                      Array.from({ length: 3 }).map((_, i) => (
                        <TableRow key={i}>
                          <TableCell><Skeleton variant="text" width={150} /></TableCell>
                          <TableCell><Skeleton variant="text" width={200} /></TableCell>
                          <TableCell><Skeleton variant="text" width={80} /></TableCell>
                          <TableCell><Skeleton variant="text" width={120} /></TableCell>
                          <TableCell><Skeleton variant="circular" width={32} height={32} /></TableCell>
                        </TableRow>
                      ))
                    ) : (
                      groupedSettings.system.map((setting) => (
                        <TableRow key={setting.id} hover sx={{ cursor: 'pointer' }}>
                          <TableCell>
                            <Tooltip title={setting.description || ''}>
                              <Typography variant="body2" fontWeight={500} sx={{ fontFamily: 'monospace' }}>
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
                            <Tooltip title="Edit">
                              <IconButton size="small" onClick={() => handleEditClick(setting)}>
                                <Edit fontSize="small" />
                              </IconButton>
                            </Tooltip>
                          </TableCell>
                        </TableRow>
                      ))
                    )}
                  </TableBody>
                </Table>
              </TableContainer>
            </Card>
          )}

          <Card>
            <CardContent sx={{ pb: 0 }}>
              <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 2 }}>
                Custom Settings
              </Typography>
            </CardContent>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Key</TableCell>
                    <TableCell>Value</TableCell>
                    <TableCell>Type</TableCell>
                    <TableCell>Description</TableCell>
                    <TableCell>Updated</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {isLoading ? (
                    Array.from({ length: 5 }).map((_, i) => (
                      <TableRow key={i}>
                        <TableCell><Skeleton variant="text" width={150} /></TableCell>
                        <TableCell><Skeleton variant="text" width={200} /></TableCell>
                        <TableCell><Skeleton variant="text" width={80} /></TableCell>
                        <TableCell><Skeleton variant="text" width={150} /></TableCell>
                        <TableCell><Skeleton variant="text" width={120} /></TableCell>
                        <TableCell><Skeleton variant="circular" width={32} height={32} /></TableCell>
                      </TableRow>
                    ))
                  ) : groupedSettings.custom.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={6} align="center" sx={{ py: 8 }}>
                        <Settings sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
                        <Typography variant="h6" color="text.secondary">
                          No custom settings
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          Click "Add Setting" to create a new configuration
                        </Typography>
                      </TableCell>
                    </TableRow>
                  ) : (
                    groupedSettings.custom.map((setting) => (
                      <TableRow key={setting.id} hover sx={{ cursor: 'pointer' }}>
                        <TableCell>
                          <Typography variant="body2" fontWeight={500} sx={{ fontFamily: 'monospace' }}>
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
                          <Typography variant="body2" color="text.secondary" sx={{ maxWidth: 200 }} noWrap>
                            {setting.description || '-'}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="caption" color="text.secondary">
                            {formatSettingTimestamp(setting.updatedAt)}
                          </Typography>
                        </TableCell>
                        <TableCell align="right">
                          <Tooltip title="Edit">
                            <IconButton size="small" onClick={() => handleEditClick(setting)}>
                              <Edit fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete">
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
                    ))
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </Card>
        </motion.div>
      </motion.div>

      <Dialog open={editDialogOpen} onClose={() => setEditDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Edit Setting</DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ pt: 1 }}>
            <Box>
              <Typography variant="caption" color="text.secondary">Key</Typography>
              <Typography variant="body1" sx={{ fontFamily: 'monospace' }}>
                {selectedSetting?.key}
              </Typography>
            </Box>
            {selectedSetting?.description && (
              <Box>
                <Typography variant="caption" color="text.secondary">Description</Typography>
                <Typography variant="body2">{selectedSetting.description}</Typography>
              </Box>
            )}
            <Box>
              <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1 }}>
                Value
              </Typography>
              {renderEditField()}
            </Box>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleEditSave}
            disabled={updateMutation.isPending}
          >
            {updateMutation.isPending ? 'Saving...' : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={createDialogOpen} onClose={() => setCreateDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Create New Setting</DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ pt: 1 }}>
            <TextField
              label="Key"
              fullWidth
              value={newSetting.key}
              onChange={(e) => setNewSetting((prev) => ({ ...prev, key: e.target.value }))}
              placeholder="e.g., auction.max_duration_days"
              helperText="Use lowercase with dots for namespacing"
            />
            <FormControl fullWidth>
              <InputLabel>Data Type</InputLabel>
              <Select
                value={newSetting.dataType}
                onChange={(e) => setNewSetting((prev) => ({ ...prev, dataType: e.target.value }))}
                label="Data Type"
              >
                {SETTING_DATA_TYPES.map((type) => (
                  <MenuItem key={type} value={type}>
                    {getSettingDataTypeLabel(type)}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <TextField
              label="Value"
              fullWidth
              value={newSetting.value}
              onChange={(e) => setNewSetting((prev) => ({ ...prev, value: e.target.value }))}
              multiline={newSetting.dataType === 'json'}
              rows={newSetting.dataType === 'json' ? 4 : 1}
            />
            <TextField
              label="Description"
              fullWidth
              value={newSetting.description}
              onChange={(e) => setNewSetting((prev) => ({ ...prev, description: e.target.value }))}
              multiline
              rows={2}
            />
            <FormControl fullWidth>
              <InputLabel>Category</InputLabel>
              <Select
                value={newSetting.category}
                onChange={(e) => setNewSetting((prev) => ({ ...prev, category: e.target.value as SettingCategory }))}
                label="Category"
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
          <Button onClick={() => setCreateDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleCreateSave}
            disabled={createMutation.isPending || !newSetting.key || !newSetting.value}
          >
            {createMutation.isPending ? 'Creating...' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Delete Setting</DialogTitle>
        <DialogContent>
          <Alert severity="warning" sx={{ mb: 2 }}>
            This action cannot be undone.
          </Alert>
          <Typography>
            Are you sure you want to delete the setting <strong>{selectedSetting?.key}</strong>?
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            color="error"
            onClick={handleDeleteConfirm}
            disabled={deleteMutation.isPending}
          >
            {deleteMutation.isPending ? 'Deleting...' : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}
