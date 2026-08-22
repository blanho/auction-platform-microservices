import { useToast } from '@/app/providers'
import { fadeInUp, scaleIn } from '@/shared/lib/animations'
import { palette } from '@/shared/theme/tokens'
import { InlineAlert } from '@/shared/ui'
import { CheckCircle, ContentCopy, QrCode2, Refresh, Security } from '@mui/icons-material'
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  InputAdornment,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Paper,
  Stack,
  Step,
  StepLabel,
  Stepper,
  TextField,
  Typography,
} from '@mui/material'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { AnimatePresence, motion } from 'framer-motion'
import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { authApi } from '../api'

interface TwoFactorSetupProps {
  isEnabled: boolean
  onComplete?: () => void
}

export function TwoFactorSetup({ isEnabled, onComplete }: TwoFactorSetupProps) {
  const { t } = useTranslation('common')
  const steps = [
    t('twoFactor.stepInstallApp'),
    t('twoFactor.stepScanQr'),
    t('twoFactor.stepVerify'),
  ]
  const queryClient = useQueryClient()
  const toast = useToast()
  const [activeStep, setActiveStep] = useState(0)
  const [verificationCode, setVerificationCode] = useState('')
  const [password, setPassword] = useState('')
  const [showRecoveryCodes, setShowRecoveryCodes] = useState(false)
  const [recoveryCodes, setRecoveryCodes] = useState<string[]>([])
  const [copied, setCopied] = useState(false)
  const copiedTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  useEffect(
    () => () => {
      if (copiedTimerRef.current) {
        clearTimeout(copiedTimerRef.current)
      }
    },
    []
  )

  const setupMutation = useMutation({
    mutationFn: () => authApi.setup2FA(),
    onSuccess: () => setActiveStep(1),
  })

  const verifyMutation = useMutation({
    mutationFn: (code: string) => authApi.enable2FA(code),
    onSuccess: (codes) => {
      setRecoveryCodes(codes)
      setShowRecoveryCodes(true)
      queryClient.invalidateQueries({ queryKey: ['user'] })
      queryClient.invalidateQueries({ queryKey: ['users', 'profile'] })
    },
  })

  const disableMutation = useMutation({
    mutationFn: (currentPassword: string) => authApi.disable2FA(currentPassword),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['user'] })
      queryClient.invalidateQueries({ queryKey: ['users', 'profile'] })
      onComplete?.()
    },
  })

  const handleVerify = () => {
    if (verificationCode.length === 6) {
      verifyMutation.mutate(verificationCode)
    }
  }

  const handleDisable = () => {
    if (password) {
      disableMutation.mutate(password)
    }
  }

  const handleCopyKey = async () => {
    if (!setupMutation.data?.sharedKey) {
      return
    }

    try {
      await navigator.clipboard.writeText(setupMutation.data.sharedKey)
      showCopiedFeedback()
    } catch {
      toast.error(t('twoFactor.copyFailed'))
    }
  }

  const handleCopyRecoveryCodes = async () => {
    try {
      await navigator.clipboard.writeText(recoveryCodes.join('\n'))
      showCopiedFeedback()
    } catch {
      toast.error(t('twoFactor.copyFailed'))
    }
  }

  const showCopiedFeedback = () => {
    if (copiedTimerRef.current) {
      clearTimeout(copiedTimerRef.current)
    }
    setCopied(true)
    copiedTimerRef.current = setTimeout(() => {
      setCopied(false)
      copiedTimerRef.current = null
    }, 2000)
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
            {t('twoFactor.title')} {t('twoFactor.enabled')}
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
            {t('twoFactor.recoveryInstructions')}
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
              {copied ? t('twoFactor.copied') : t('twoFactor.copy')}
            </Button>
            <Button
              variant="contained"
              onClick={onComplete}
              sx={{
                bgcolor: palette.brand.primary,
                '&:hover': { bgcolor: '#A16207' },
              }}
            >
              {t('twoFactor.done')}
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
                {t('twoFactor.title')}
              </Typography>
              <Typography variant="body2" color="success.main">
                {t('twoFactor.enabled')}
              </Typography>
            </Box>
          </Box>

          <InlineAlert severity="warning" sx={{ mb: 3 }}>
            {t('twoFactor.disableWarning')}
          </InlineAlert>

          <Stack spacing={2}>
            <TextField
              label={t('twoFactor.enterPassword')}
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              slotProps={{
                input: {
                  startAdornment: (
                    <InputAdornment position="start">
                      <Security />
                    </InputAdornment>
                  ),
                },
              }}
            />

            {disableMutation.error && (
              <InlineAlert severity="error">{t('twoFactor.invalidPassword')}</InlineAlert>
            )}

            <Button
              variant="outlined"
              color="error"
              onClick={handleDisable}
              disabled={!password || disableMutation.isPending}
            >
              {disableMutation.isPending ? <CircularProgress size={24} /> : t('twoFactor.disable')}
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
          {t('twoFactor.setupTitle')}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
          {t('twoFactor.setupSubtitle')}
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
                    primary={t('twoFactor.downloadApp')}
                    secondary={t('twoFactor.downloadAppDesc')}
                  />
                </ListItem>
                <ListItem>
                  <ListItemIcon>
                    <CheckCircle color="primary" />
                  </ListItemIcon>
                  <ListItemText
                    primary={t('twoFactor.scanQrOrKey')}
                    secondary={t('twoFactor.scanQrOrKeyDesc')}
                  />
                </ListItem>
                <ListItem>
                  <ListItemIcon>
                    <CheckCircle color="primary" />
                  </ListItemIcon>
                  <ListItemText
                    primary={t('twoFactor.enterVerificationCode')}
                    secondary={t('twoFactor.enterVerificationCodeDesc')}
                  />
                </ListItem>
              </List>

              <Button
                variant="contained"
                onClick={() => setupMutation.mutate()}
                disabled={setupMutation.isPending}
                sx={{
                  mt: 2,
                  bgcolor: palette.brand.primary,
                  '&:hover': { bgcolor: '#A16207' },
                }}
              >
                {setupMutation.isPending ? <CircularProgress size={24} /> : t('twoFactor.continue')}
              </Button>
            </motion.div>
          )}

          {activeStep === 1 && setupMutation.data && (
            <motion.div
              key="step-1"
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -20 }}
            >
              {setupMutation.data.qrCodeBase64 ? (
                <Box sx={{ textAlign: 'center', mb: 3 }}>
                  <Paper
                    variant="outlined"
                    sx={{ p: 2, display: 'inline-block', bgcolor: 'white' }}
                  >
                    <Box
                      component="img"
                      src={
                        setupMutation.data.qrCodeBase64.startsWith('data:')
                          ? setupMutation.data.qrCodeBase64
                          : `data:image/png;base64,${setupMutation.data.qrCodeBase64}`
                      }
                      alt={t('twoFactor.qrCodeAlt')}
                      sx={{ width: 200, height: 200 }}
                    />
                  </Paper>
                </Box>
              ) : (
                <InlineAlert severity="info" sx={{ mb: 3 }}>
                  {t('twoFactor.manualSetupOnly')}
                </InlineAlert>
              )}

              <Typography variant="body2" color="text.secondary" textAlign="center" gutterBottom>
                {t('twoFactor.cantScan')}
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
                <Typography variant="body1" fontFamily="monospace" sx={{ letterSpacing: 2 }}>
                  {setupMutation.data.sharedKey}
                </Typography>
                <Button size="small" startIcon={<ContentCopy />} onClick={handleCopyKey}>
                  {copied ? t('twoFactor.copied') : t('twoFactor.copy')}
                </Button>
              </Paper>

              <Stack direction="row" spacing={2} justifyContent="center">
                <Button variant="outlined" onClick={() => setActiveStep(0)}>
                  {t('back')}
                </Button>
                <Button
                  variant="contained"
                  onClick={() => setActiveStep(2)}
                  sx={{
                    bgcolor: palette.brand.primary,
                    '&:hover': { bgcolor: '#A16207' },
                  }}
                >
                  {t('twoFactor.scannedCode')}
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
                  {t('twoFactor.enterSixDigitCode')}
                </Typography>
              </Box>

              <TextField
                fullWidth
                label={t('twoFactor.verificationCode')}
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
                <InlineAlert severity="error" sx={{ mb: 3 }}>
                  {t('twoFactor.invalidCode')}
                </InlineAlert>
              )}

              <Stack direction="row" spacing={2} justifyContent="center">
                <Button variant="outlined" onClick={() => setActiveStep(1)}>
                  {t('back')}
                </Button>
                <Button
                  variant="contained"
                  onClick={handleVerify}
                  disabled={verificationCode.length !== 6 || verifyMutation.isPending}
                  sx={{
                    bgcolor: palette.brand.primary,
                    '&:hover': { bgcolor: '#A16207' },
                  }}
                >
                  {verifyMutation.isPending ? (
                    <CircularProgress size={24} />
                  ) : (
                    t('twoFactor.verifyEnable')
                  )}
                </Button>
              </Stack>
            </motion.div>
          )}
        </AnimatePresence>

        {setupMutation.isError && (
          <Alert
            severity="error"
            action={
              <Button
                color="inherit"
                size="small"
                startIcon={<Refresh />}
                onClick={() => setupMutation.mutate()}
              >
                {t('retry')}
              </Button>
            }
            sx={{ mt: 3 }}
          >
            {t('twoFactor.setupFailed')}
          </Alert>
        )}
      </Paper>
    </motion.div>
  )
}
