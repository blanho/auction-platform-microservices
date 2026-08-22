import { ConfirmDialog, InlineAlert, TableEmptyStateRow, TableSkeletonRows } from '@/shared/ui'
import {
  Add,
  Cancel,
  CheckCircle,
  Delete,
  Edit,
  Email,
  Notifications,
  PhoneIphone,
  Sms,
} from '@mui/icons-material'
import {
  Box,
  Button,
  Card,
  Chip,
  Container,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Pagination,
  Select,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useCreateTemplate, useDeleteTemplate, useTemplates, useUpdateTemplate } from '../hooks'
import type {
  CreateTemplateDto,
  NotificationChannel,
  NotificationTemplate,
  UpdateTemplateDto,
} from '../types/template.types'

const CHANNEL_ICONS: Record<NotificationChannel, React.ReactElement> = {
  email: <Email />,
  sms: <Sms />,
  push: <PhoneIphone />,
  in_app: <Notifications />,
}

const CHANNEL_COLORS: Record<NotificationChannel, string> = {
  email: '#3B82F6',
  sms: '#10B981',
  push: '#F59E0B',
  in_app: '#8B5CF6',
}

export function TemplatesManagementPage() {
  const { t } = useTranslation('notifications')
  const [page, setPage] = useState(1)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<NotificationTemplate | null>(null)
  const [editingTemplate, setEditingTemplate] = useState<NotificationTemplate | null>(null)
  const [formData, setFormData] = useState<CreateTemplateDto>({
    key: '',
    name: '',
    description: '',
    subject: '',
    body: '',
    channel: 'email',
    variables: [],
  })

  const { data: templatesData, isLoading, error } = useTemplates(page, 20)
  const createTemplate = useCreateTemplate()
  const updateTemplate = useUpdateTemplate()
  const deleteTemplate = useDeleteTemplate()
  const templateCount = templatesData?.items?.length ?? 0

  const handleOpenCreate = () => {
    setEditingTemplate(null)
    setFormData({
      key: '',
      name: '',
      description: '',
      subject: '',
      body: '',
      channel: 'email',
      variables: [],
    })
    setDialogOpen(true)
  }

  const handleOpenEdit = (template: NotificationTemplate) => {
    setEditingTemplate(template)
    setFormData({
      key: template.key,
      name: template.name,
      description: template.description || '',
      subject: template.subject,
      body: template.body,
      channel: template.channel,
      variables: template.variables || [],
    })
    setDialogOpen(true)
  }

  const handleClose = () => {
    setDialogOpen(false)
    setEditingTemplate(null)
  }

  const handleSubmit = async () => {
    if (editingTemplate) {
      const updateDto: UpdateTemplateDto = {
        name: formData.name,
        description: formData.description,
        subject: formData.subject,
        body: formData.body,
        channel: formData.channel,
        variables: formData.variables,
      }
      await updateTemplate.mutateAsync({ id: editingTemplate.id, dto: updateDto })
    } else {
      await createTemplate.mutateAsync(formData)
    }
    handleClose()
  }

  const handleDelete = (template: NotificationTemplate) => {
    setDeleteTarget(template)
  }

  const handleConfirmDelete = async () => {
    if (!deleteTarget) {
      return
    }
    await deleteTemplate.mutateAsync(deleteTarget.id)
    setDeleteTarget(null)
  }

  const handleCloseDeleteDialog = () => {
    setDeleteTarget(null)
  }

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box
        sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 4 }}
      >
        <Box>
          <Typography
            variant="h4"
            sx={{
              fontFamily: '"Fira Code", monospace',
              fontWeight: 600,
              color: '#4C1D95',
            }}
          >
            {t('templates.title')}
          </Typography>
          <Typography sx={{ color: '#78716C', fontFamily: '"Fira Sans", sans-serif' }}>
            {t('templates.description')}
          </Typography>
        </Box>

        <Button
          variant="contained"
          startIcon={<Add />}
          onClick={handleOpenCreate}
          sx={{
            bgcolor: '#F97316',
            '&:hover': { bgcolor: '#EA580C' },
            textTransform: 'none',
            fontWeight: 600,
          }}
        >
          {t('templates.create')}
        </Button>
      </Box>

      {error && (
        <InlineAlert severity="error" sx={{ mb: 3 }}>
          {t('templates.loadFailed')}
        </InlineAlert>
      )}

      <Card sx={{ borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }}>
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow sx={{ bgcolor: '#FAF5FF' }}>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95' }}>
                  {t('templates.template')}
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95' }}>
                  {t('templates.channel')}
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95' }}>
                  {t('templates.subject')}
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95' }}>
                  {t('templates.status')}
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95' }}>
                  {t('templates.variables')}
                </TableCell>
                <TableCell align="right" sx={{ fontWeight: 600, color: '#4C1D95' }}>
                  {t('templates.actions')}
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {isLoading && <TableSkeletonRows rows={5} columns={6} />}
              {!isLoading && templateCount === 0 && (
                <TableEmptyStateRow
                  colSpan={6}
                  title={t('templates.noTemplates')}
                  cellSx={{ py: 8 }}
                />
              )}
              {!isLoading &&
                templateCount > 0 &&
                templatesData?.items.map((template) => (
                  <TableRow
                    key={template.id}
                    sx={{
                      '&:hover': { bgcolor: '#FAFAF9' },
                      cursor: 'pointer',
                    }}
                  >
                    <TableCell>
                      <Box>
                        <Typography sx={{ fontWeight: 600, color: '#1C1917' }}>
                          {template.name}
                        </Typography>
                        <Typography
                          variant="caption"
                          sx={{ color: '#78716C', fontFamily: '"Fira Code", monospace' }}
                        >
                          {template.key}
                        </Typography>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Chip
                        icon={CHANNEL_ICONS[template.channel]}
                        label={t(`templates.channels.${template.channel}`)}
                        size="small"
                        sx={{
                          bgcolor: `${CHANNEL_COLORS[template.channel]}15`,
                          color: CHANNEL_COLORS[template.channel],
                          fontWeight: 600,
                        }}
                      />
                    </TableCell>
                    <TableCell>
                      <Typography sx={{ color: '#44403C', fontSize: '0.875rem' }}>
                        {template.subject || '-'}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      {template.isActive ? (
                        <Chip
                          icon={<CheckCircle />}
                          label={t('templates.active')}
                          size="small"
                          sx={{ bgcolor: '#DCFCE7', color: '#166534', fontWeight: 600 }}
                        />
                      ) : (
                        <Chip
                          icon={<Cancel />}
                          label={t('templates.inactive')}
                          size="small"
                          sx={{ bgcolor: '#FEE2E2', color: '#991B1B', fontWeight: 600 }}
                        />
                      )}
                    </TableCell>
                    <TableCell>
                      <Typography variant="caption" sx={{ color: '#78716C' }}>
                        {t('templates.variableCount', {
                          count: template.variables?.length || 0,
                        })}
                      </Typography>
                    </TableCell>
                    <TableCell align="right">
                      <Tooltip title={t('common:edit')}>
                        <IconButton
                          size="small"
                          onClick={() => handleOpenEdit(template)}
                          sx={{ color: '#7C3AED' }}
                        >
                          <Edit />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title={t('common:delete')}>
                        <IconButton
                          size="small"
                          onClick={() => handleDelete(template)}
                          sx={{ color: '#EF4444' }}
                        >
                          <Delete />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))}
            </TableBody>
          </Table>
        </TableContainer>

        {templatesData && templatesData.totalPages > 1 && (
          <Box
            sx={{ display: 'flex', justifyContent: 'center', p: 3, borderTop: '1px solid #F5F5F5' }}
          >
            <Pagination
              count={templatesData.totalPages}
              page={page}
              onChange={(_, p) => setPage(p)}
              color="primary"
            />
          </Box>
        )}
      </Card>

      <Dialog open={dialogOpen} onClose={handleClose} maxWidth="md" fullWidth>
        <DialogTitle sx={{ fontFamily: '"Fira Code", monospace', fontWeight: 600 }}>
          {editingTemplate ? t('templates.edit') : t('templates.create')}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              label={t('templates.key')}
              value={formData.key}
              onChange={(e) => setFormData({ ...formData, key: e.target.value })}
              disabled={!!editingTemplate}
              fullWidth
              required
              helperText={t('templates.keyHelper')}
            />
            <TextField
              label={t('templates.name')}
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              fullWidth
              required
            />
            <TextField
              label={t('templates.descriptionLabel')}
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              fullWidth
              multiline
              rows={2}
            />
            <FormControl fullWidth required>
              <InputLabel>{t('templates.channel')}</InputLabel>
              <Select
                value={formData.channel}
                label={t('templates.channel')}
                onChange={(e) =>
                  setFormData({ ...formData, channel: e.target.value as NotificationChannel })
                }
              >
                {(['email', 'sms', 'push', 'in_app'] as NotificationChannel[]).map((channel) => (
                  <MenuItem key={channel} value={channel}>
                    {t(`templates.channels.${channel}`)}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <TextField
              label={t('templates.subject')}
              value={formData.subject}
              onChange={(e) => setFormData({ ...formData, subject: e.target.value })}
              fullWidth
              required
              helperText={t('templates.subjectHelper')}
            />
            <TextField
              label={t('templates.body')}
              value={formData.body}
              onChange={(e) => setFormData({ ...formData, body: e.target.value })}
              fullWidth
              required
              multiline
              rows={6}
              helperText={t('templates.bodyHelper')}
            />
            <TextField
              label={t('templates.variables')}
              value={formData.variables?.join(', ')}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  variables: e.target.value
                    .split(',')
                    .map((v) => v.trim())
                    .filter(Boolean),
                })
              }
              fullWidth
              helperText={t('templates.variablesHelper')}
            />
          </Box>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 3 }}>
          <Button onClick={handleClose}>{t('common:cancel')}</Button>
          <Button
            variant="contained"
            onClick={handleSubmit}
            disabled={createTemplate.isPending || updateTemplate.isPending}
            sx={{
              bgcolor: '#7C3AED',
              '&:hover': { bgcolor: '#6D28D9' },
            }}
          >
            {editingTemplate ? t('templates.update') : t('common:create')}
          </Button>
        </DialogActions>
      </Dialog>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={handleCloseDeleteDialog}
        onConfirm={handleConfirmDelete}
        title={t('templates.deleteTitle')}
        message={t('templates.deleteMessage', { name: deleteTarget?.name ?? '' })}
        confirmLabel={t('common:delete')}
        cancelLabel={t('common:cancel')}
        variant="danger"
        loading={deleteTemplate.isPending}
      />
    </Container>
  )
}
