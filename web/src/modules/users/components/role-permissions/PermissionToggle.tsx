import { Box, Switch, Typography, Stack, Tooltip, CircularProgress } from '@mui/material'
import { Check, Close } from '@mui/icons-material'
import type { PermissionDefinition } from '../../types'

interface PermissionToggleProps {
  readonly permission: PermissionDefinition
  readonly enabled: boolean
  readonly loading: boolean
  readonly onToggle: (permission: string, enabled: boolean) => void
}

export function PermissionToggle({
  permission,
  enabled,
  loading,
  onToggle,
}: PermissionToggleProps) {
  const handleChange = () => {
    onToggle(permission.code, !enabled)
  }

  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        py: 1.5,
        px: 2,
        borderRadius: 1,
        bgcolor: enabled ? 'success.main' : 'background.default',
        transition: 'background-color 0.2s ease-in-out',
        '&:hover': {
          bgcolor: enabled ? 'success.dark' : 'action.hover',
        },
      }}
    >
      <Stack spacing={0.25} sx={{ flex: 1, mr: 2 }}>
        <Typography
          variant="body2"
          fontWeight={500}
          sx={{ color: enabled ? 'success.contrastText' : 'text.primary' }}
        >
          {permission.name}
        </Typography>
        <Typography
          variant="caption"
          sx={{
            color: enabled ? 'rgba(255,255,255,0.7)' : 'text.secondary',
          }}
        >
          {permission.description}
        </Typography>
        <Typography
          variant="caption"
          sx={{
            color: enabled ? 'rgba(255,255,255,0.5)' : 'text.disabled',
            fontFamily: 'monospace',
            fontSize: '0.7rem',
          }}
        >
          {permission.code}
        </Typography>
      </Stack>
      <Tooltip title={enabled ? 'Revoke permission' : 'Grant permission'}>
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          {loading ? (
            <CircularProgress
              size={24}
              sx={{ color: enabled ? 'success.contrastText' : 'primary.main' }}
            />
          ) : (
            <Switch
              checked={enabled}
              onChange={handleChange}
              icon={<Close sx={{ fontSize: 16 }} />}
              checkedIcon={<Check sx={{ fontSize: 16 }} />}
              sx={{
                '& .MuiSwitch-switchBase.Mui-checked': {
                  color: 'white',
                },
                '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': {
                  bgcolor: 'rgba(255,255,255,0.3)',
                },
              }}
            />
          )}
        </Box>
      </Tooltip>
    </Box>
  )
}
