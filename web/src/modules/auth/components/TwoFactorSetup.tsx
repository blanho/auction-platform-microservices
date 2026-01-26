import { useState } from 'react'
import {
  Box,
  Typography,
  Button,
  TextField,
  Stack,
  Alert,
  CircularProgress,
  Paper,
  Stepper,
  Step,
  StepLabel,
  InputAdornment,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
} from '@mui/material'
import {
  Security,
  QrCode2,
  CheckCircle,
  ContentCopy,
  Refresh,
} from '@mui/icons-material'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { motion, AnimatePresence } from 'framer-motion'
import { http } from '@/services/http'
import { fadeInUp, scaleIn } from '@/shared/lib/animations'

interface TwoFactorSetupProps {
  isEnabled: boolean
  onComplete?: () => void
}

interface SetupResponse {
  qrCodeUrl: string
  manualEntryKey: string
  recoveryCodes: string[]
}

const steps = ['Generate Secret', 'Scan QR Code', 'Verify Code']

export function TwoFactorSetup({ isEnabled, onComplete }: TwoFactorSetupProps) {
  const queryClient = useQueryClient()
  const [activeStep, setActiveStep] = useState(0)
  const [verificationCode, setVerificationCode] = useState('')
  const [showRecoveryCodes, setShowRecoveryCodes] = useState(false)
  const [recoveryCodes, setRecoveryCodes] = useState<string[]>([])
  const [copied, setCopied] = useState(false)

  const setupQuery = useQuery<SetupResponse>({
    queryKey: ['2fa-setup'],
    queryFn: async (): Promise<SetupResponse> => {
      const response = await http.post('/auth/2fa/setup')
      return response.data as SetupResponse
    },
    enabled: !isEnabled && activeStep >= 0,
    staleTime: 5 * 60 * 1000,
  })

  const verifyMutation = useMutation({
    mutationFn: async (code: string) => {
      const response = await http.post('/auth/2fa/verify', { code })
      return response.data as { recoveryCodes?: string[] }
    },
    onSuccess: (data) => {
      setRecoveryCodes(data.recoveryCodes || setupQuery.data?.recoveryCodes || [])
      setShowRecoveryCodes(true)
      queryClient.invalidateQueries({ queryKey: ['user'] })
    },
  })

  const disableMutation = useMutation({
    mutationFn: async (code: string) => {
      const response = await http.post('/auth/2fa/disable', { code })
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['user'] })
      onComplete?.()
    },
  })

  const handleVerify = () => {
    if (verificationCode.length === 6) {
      verifyMutation.mutate(verificationCode)
    }
  }

  const handleDisable = () => {
    if (verificationCode.length === 6) {
      disableMutation.mutate(verificationCode)
    }
  }

  const handleCopyKey = async () => {
    if (setupQuery.data?.manualEntryKey) {
      await navigator.clipboard.writeText(setupQuery.data.manualEntryKey)
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    }
  }

  const handleCopyRecoveryCodes = async () => {
    await navigator.clipboard.writeText(recoveryCodes.join('\n'))
    setCopied(true)
    setTimeout(() => setCopied(false), 2000)
  }

  if (showRecoveryCodes) {
    return (
      <motion.div variants={scaleIn} initial="initial" animate="animate">
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Box
            sx={{
              width: 64,
              height: 64,
              borderRadius: '50%',
              bgcolor: 'success.light',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              mx: 'auto',
              mb: 2,
            }}
          >
            <CheckCircle sx={{ fontSize: 36, color: 'success.main' }} />
          </Box>
          
          <Typography variant="h5" fontWeight={600} gutterBottom>
            Two-Factor Authentication Enabled
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
            Save these recovery codes in a secure place. You can use them to access your account if you lose your device.
          </Typography>

          <Paper
            variant="outlined"
            sx={{
              p: 2,
              bgcolor: 'grey.50',
              fontFamily: 'monospace',
              textAlign: 'left',
              mb: 3,
            }}
          >
            <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 1 }}>
              {recoveryCodes.map((code, index) => (
                <Typography key={index} variant="body2" fontFamily="monospace">
                  {code}
                </Typography>
              ))}
            </Box>
          </Paper>

          <Stack direction="row" spacing={2} justifyContent="center">
            <Button
              startIcon={<ContentCopy />}
              onClick={handleCopyRecoveryCodes}
              variant="outlined"
            >
              {copied ? 'Copied!' : 'Copy Codes'}
            </Button>
            <Button
              variant="contained"
              onClick={onComplete}
              sx={{
                bgcolor: '#CA8A04',
                '&:hover': { bgcolor: '#A16207' },
              }}
            >
              Done
            </Button>
          </Stack>
        </Paper>
      </motion.div>
    )
  }

  if (isEnabled) {
    return (
      <motion.div variants={fadeInUp} initial="initial" animate="animate">
        <Paper sx={{ p: 4 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
            <Box
              sx={{
                width: 48,
                height: 48,
                borderRadius: '50%',
                bgcolor: 'success.light',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              <Security sx={{ color: 'success.main' }} />
            </Box>
            <Box>
              <Typography variant="h6" fontWeight={600}>
                Two-Factor Authentication
              </Typography>
              <Typography variant="body2" color="success.main">
                Enabled
              </Typography>
            </Box>
          </Box>

          <Alert severity="warning" sx={{ mb: 3 }}>
            Disabling 2FA will make your account less secure. Make sure you have other security measures in place.
          </Alert>

          <Stack spacing={2}>
            <TextField
              label="Enter code from authenticator app"
              value={verificationCode}
              onChange={(e) => setVerificationCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
              slotProps={{
                input: {
                  startAdornment: (
                    <InputAdornment position="start">
                      <Security />
                    </InputAdornment>
                  ),
                },
              }}
              placeholder="000000"
            />

            {disableMutation.error && (
              <Alert severity="error">
                Invalid verification code. Please try again.
              </Alert>
            )}

            <Button
              variant="outlined"
              color="error"
              onClick={handleDisable}
              disabled={verificationCode.length !== 6 || disableMutation.isPending}
            >
              {disableMutation.isPending ? <CircularProgress size={24} /> : 'Disable 2FA'}
            </Button>
          </Stack>
        </Paper>
      </motion.div>
    )
  }

  return (
    <motion.div variants={fadeInUp} initial="initial" animate="animate">
      <Paper sx={{ p: 4 }}>
        <Typography variant="h6" fontWeight={600} gutterBottom>
          Set Up Two-Factor Authentication
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
          Add an extra layer of security to your account
        </Typography>

        <Stepper activeStep={activeStep} sx={{ mb: 4 }}>
          {steps.map((label) => (
            <Step key={label}>
              <StepLabel>{label}</StepLabel>
            </Step>
          ))}
        </Stepper>

        <AnimatePresence mode="wait">
          {activeStep === 0 && (
            <motion.div
              key="step-0"
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -20 }}
            >
              <List>
                <ListItem>
                  <ListItemIcon>
                    <CheckCircle color="primary" />
                  </ListItemIcon>
                  <ListItemText
                    primary="Download an authenticator app"
                    secondary="Google Authenticator, Authy, or Microsoft Authenticator"
                  />
                </ListItem>
                <ListItem>
                  <ListItemIcon>
                    <CheckCircle color="primary" />
                  </ListItemIcon>
                  <ListItemText
                    primary="Scan the QR code or enter the key manually"
                    secondary="This links your account to your authenticator"
                  />
                </ListItem>
                <ListItem>
                  <ListItemIcon>
                    <CheckCircle color="primary" />
                  </ListItemIcon>
                  <ListItemText
                    primary="Enter the verification code"
                    secondary="Confirm setup with a code from your app"
                  />
                </ListItem>
              </List>

              <Button
                variant="contained"
                onClick={() => setActiveStep(1)}
                disabled={setupQuery.isLoading}
                sx={{
                  mt: 2,
                  bgcolor: '#CA8A04',
                  '&:hover': { bgcolor: '#A16207' },
                }}
              >
                {setupQuery.isLoading ? <CircularProgress size={24} /> : 'Continue'}
              </Button>
            </motion.div>
          )}

          {activeStep === 1 && setupQuery.data && (
            <motion.div
              key="step-1"
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -20 }}
            >
              <Box sx={{ textAlign: 'center', mb: 3 }}>
                <Paper
                  variant="outlined"
                  sx={{
                    p: 2,
                    display: 'inline-block',
                    bgcolor: 'white',
                  }}
                >
                  <Box
                    component="img"
                    src={setupQuery.data.qrCodeUrl}
                    alt="QR Code"
                    sx={{ width: 200, height: 200 }}
                  />
                </Paper>
              </Box>

              <Typography variant="body2" color="text.secondary" textAlign="center" gutterBottom>
                Can't scan? Enter this key manually:
              </Typography>
              
              <Paper
                variant="outlined"
                sx={{
                  p: 2,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  gap: 2,
                  mb: 3,
                }}
              >
                <Typography
                  variant="body1"
                  fontFamily="monospace"
                  sx={{ letterSpacing: 2 }}
                >
                  {setupQuery.data.manualEntryKey}
                </Typography>
                <Button
                  size="small"
                  startIcon={<ContentCopy />}
                  onClick={handleCopyKey}
                >
                  {copied ? 'Copied!' : 'Copy'}
                </Button>
              </Paper>

              <Stack direction="row" spacing={2} justifyContent="center">
                <Button variant="outlined" onClick={() => setActiveStep(0)}>
                  Back
                </Button>
                <Button
                  variant="contained"
                  onClick={() => setActiveStep(2)}
                  sx={{
                    bgcolor: '#CA8A04',
                    '&:hover': { bgcolor: '#A16207' },
                  }}
                >
                  I've Scanned the Code
                </Button>
              </Stack>
            </motion.div>
          )}

          {activeStep === 2 && (
            <motion.div
              key="step-2"
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -20 }}
            >
              <Box sx={{ textAlign: 'center', mb: 3 }}>
                <QrCode2 sx={{ fontSize: 64, color: 'primary.main', mb: 2 }} />
                <Typography variant="body1" gutterBottom>
                  Enter the 6-digit code from your authenticator app
                </Typography>
              </Box>

              <TextField
                fullWidth
                label="Verification Code"
                value={verificationCode}
                onChange={(e) => setVerificationCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                slotProps={{
                  input: {
                    sx: {
                      textAlign: 'center',
                      fontSize: '1.5rem',
                      letterSpacing: 8,
                    },
                  },
                }}
                placeholder="000000"
                sx={{ mb: 3 }}
              />

              {verifyMutation.error && (
                <Alert severity="error" sx={{ mb: 3 }}>
                  Invalid verification code. Please try again.
                </Alert>
              )}

              <Stack direction="row" spacing={2} justifyContent="center">
                <Button variant="outlined" onClick={() => setActiveStep(1)}>
                  Back
                </Button>
                <Button
                  variant="contained"
                  onClick={handleVerify}
                  disabled={verificationCode.length !== 6 || verifyMutation.isPending}
                  sx={{
                    bgcolor: '#CA8A04',
                    '&:hover': { bgcolor: '#A16207' },
                  }}
                >
                  {verifyMutation.isPending ? <CircularProgress size={24} /> : 'Verify & Enable'}
                </Button>
              </Stack>
            </motion.div>
          )}
        </AnimatePresence>

        {setupQuery.isError && (
          <Alert
            severity="error"
            action={
              <Button
                color="inherit"
                size="small"
                startIcon={<Refresh />}
                onClick={() => setupQuery.refetch()}
              >
                Retry
              </Button>
            }
            sx={{ mt: 3 }}
          >
            Failed to generate 2FA setup. Please try again.
          </Alert>
        )}
      </Paper>
    </motion.div>
  )
}
