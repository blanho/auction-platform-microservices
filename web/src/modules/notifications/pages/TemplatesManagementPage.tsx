import { useState } from 'react'
import {
  Box,
  Container,
  Typography,
  Card,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Tooltip,
  Pagination,
} from '@mui/material'
import {
  Add,
  Edit,
  Delete,
  Email,
  Sms,
  Notifications,
  PhoneIphone,
  CheckCircle,
  Cancel,
} from '@mui/icons-material'
import { ConfirmDialog, InlineAlert, TableEmptyStateRow, TableSkeletonRows } from '@/shared/ui'
import { useTemplates, useCreateTemplate, useUpdateTemplate, useDeleteTemplate } from '../hooks'
import type {
  NotificationTemplate,
  NotificationChannel,
  CreateTemplateDto,
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
            Notification Templates
          </Typography>
          <Typography sx={{ color: '#78716C', fontFamily: '"Fira Sans", sans-serif' }}>
            Manage email, SMS, and push notification templates
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
          Create Template
        </Button>
      </Box>

      {error && (
        <InlineAlert severity="error" sx={{ mb: 3 }}>
          Failed to load templates. Please try again.
        </InlineAlert>
      )}

      <Card sx={{ borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }}>
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow sx={{ bgcolor: '#FAF5FF' }}>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95' }}>Template</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95' }}>Channel</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95' }}>Subject</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95' }}>Status</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#4C1D95' }}>Variables</TableCell>
                <TableCell align="right" sx={{ fontWeight: 600, color: '#4C1D95' }}>
                  Actions
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {isLoading && <TableSkeletonRows rows={5} columns={6} />}
              {!isLoading && templateCount === 0 && (
                <TableEmptyStateRow
                  colSpan={6}
                  title="No templates found"
                  cellSx={{ py: 8 }}
                />
              )}
              {!isLoading && templateCount > 0 && (
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
                        label={template.channel.toUpperCase()}
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
                          label="Active"
                          size="small"
                          sx={{ bgcolor: '#DCFCE7', color: '#166534', fontWeight: 600 }}
                        />
                      ) : (
                        <Chip
                          icon={<Cancel />}
                          label="Inactive"
                          size="small"
                          sx={{ bgcolor: '#FEE2E2', color: '#991B1B', fontWeight: 600 }}
                        />
                      )}
                    </TableCell>
                    <TableCell>
                      <Typography variant="caption" sx={{ color: '#78716C' }}>
                        {template.variables?.length || 0} variables
                      </Typography>
                    </TableCell>
                    <TableCell align="right">
                      <Tooltip title="Edit">
                        <IconButton
                          size="small"
                          onClick={() => handleOpenEdit(template)}
                          sx={{ color: '#7C3AED' }}
                        >
                          <Edit />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Delete">
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
                ))
              )}
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
          {editingTemplate ? 'Edit Template' : 'Create Template'}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              label="Template Key"
              value={formData.key}
              onChange={(e) => setFormData({ ...formData, key: e.target.value })}
              disabled={!!editingTemplate}
              fullWidth
              required
              helperText="Unique identifier (e.g., 'welcome-email')"
            />
            <TextField
              label="Template Name"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              fullWidth
              required
            />
            <TextField
              label="Description"
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              fullWidth
              multiline
              rows={2}
            />
            <FormControl fullWidth required>
              <InputLabel>Channel</InputLabel>
              <Select
                value={formData.channel}
                label="Channel"
                onChange={(e) =>
                  setFormData({ ...formData, channel: e.target.value as NotificationChannel })
                }
              >
                <MenuItem value="email">Email</MenuItem>
                <MenuItem value="sms">SMS</MenuItem>
                <MenuItem value="push">Push Notification</MenuItem>
                <MenuItem value="in_app">In-App</MenuItem>
              </Select>
            </FormControl>
            <TextField
              label="Subject"
              value={formData.subject}
              onChange={(e) => setFormData({ ...formData, subject: e.target.value })}
              fullWidth
              required
              helperText="Use {{variable}} for dynamic content"
            />
            <TextField
              label="Body"
              value={formData.body}
              onChange={(e) => setFormData({ ...formData, body: e.target.value })}
              fullWidth
              required
              multiline
              rows={6}
              helperText="Template content with {{variables}}"
            />
            <TextField
              label="Variables"
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
              helperText="Comma-separated list (e.g., userName, orderNumber)"
            />
          </Box>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 3 }}>
          <Button onClick={handleClose}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleSubmit}
            disabled={createTemplate.isPending || updateTemplate.isPending}
            sx={{
              bgcolor: '#7C3AED',
              '&:hover': { bgcolor: '#6D28D9' },
            }}
          >
            {editingTemplate ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={handleCloseDeleteDialog}
        onConfirm={handleConfirmDelete}
        title="Delete Template"
        message={`Are you sure you want to delete "${deleteTarget?.name ?? ''}"? This action cannot be undone.`}
        confirmLabel="Delete"
        cancelLabel="Cancel"
        variant="danger"
        loading={deleteTemplate.isPending}
      />
    </Container>
  )
}
