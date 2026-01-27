import { forwardRef } from 'react'
import type { ReactNode } from 'react'
import { Button, CircularProgress, Box } from '@mui/material'
import type { ButtonProps } from '@mui/material'
import { motion, AnimatePresence } from 'framer-motion'
import { useReducedMotion } from '@/shared/lib/animations'

interface LoadingButtonProps extends Omit<ButtonProps, 'startIcon' | 'endIcon'> {
  loading?: boolean
  loadingText?: string
  loadingPosition?: 'start' | 'end' | 'center'
  startIcon?: ReactNode
  endIcon?: ReactNode
}

export const LoadingButton = forwardRef<HTMLButtonElement, LoadingButtonProps>(
  function LoadingButton(
    {
      loading = false,
      loadingText,
      loadingPosition = 'center',
      startIcon,
      endIcon,
      children,
      disabled,
      sx,
      ...props
    },
    ref
  ) {
    const prefersReducedMotion = useReducedMotion()
    const isDisabled = disabled || loading

    const spinner = (
      <CircularProgress
        size={16}
        color="inherit"
        sx={{ ml: loadingPosition === 'end' ? 1 : 0, mr: loadingPosition === 'start' ? 1 : 0 }}
      />
    )

    const renderContent = () => {
      if (loading) {
        if (loadingPosition === 'center') {
          return (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              {spinner}
              {loadingText || children}
            </Box>
          )
        }
        return (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            {loadingPosition === 'start' && spinner}
            {loadingText || children}
            {loadingPosition === 'end' && spinner}
          </Box>
        )
      }

      return (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          {startIcon}
          {children}
          {endIcon}
        </Box>
      )
    }

    const buttonContent = prefersReducedMotion ? (
      renderContent()
    ) : (
      <AnimatePresence mode="wait">
        <motion.span
          key={loading ? 'loading' : 'idle'}
          initial={{ opacity: 0, y: 5 }}
          animate={{ opacity: 1, y: 0 }}
          exit={{ opacity: 0, y: -5 }}
          transition={{ duration: 0.15 }}
          style={{ display: 'flex', alignItems: 'center' }}
        >
          {renderContent()}
        </motion.span>
      </AnimatePresence>
    )

    return (
      <Button
        ref={ref}
        disabled={isDisabled}
        sx={{
          cursor: isDisabled ? 'not-allowed' : 'pointer',
          minWidth: 100,
          position: 'relative',
          ...sx,
        }}
        {...props}
      >
        {buttonContent}
      </Button>
    )
  }
)
