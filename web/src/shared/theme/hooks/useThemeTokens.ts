import { useMemo } from 'react'
import { useTheme } from '@mui/material/styles'
import { colors, gradients, palette } from '../tokens/colors'
import { spacing, space, radius } from '../tokens/spacing'
import { fontFamily, fontSize, fontWeight, textStyle } from '../tokens/typography'
import { shadows, blur, transitions, duration, easing, zIndex } from '../tokens/effects'
import type { ThemeMode } from '../tokens/colors'

export function useThemeTokens() {
  const theme = useTheme()
  const mode = (theme.palette.mode || 'light') as ThemeMode

  return useMemo(
    () => ({
      mode,
      colors: colors[mode],
      palette,
      gradients,
      spacing,
      space,
      radius,
      fontFamily,
      fontSize,
      fontWeight,
      textStyle,
      shadows: {
        ...shadows,
        card: mode === 'dark' ? shadows.cardDark : shadows.card,
      },
      blur,
      transitions,
      duration,
      easing,
      zIndex,
    }),
    [mode]
  )
}

export function useColors() {
  const theme = useTheme()
  const mode = (theme.palette.mode || 'light') as ThemeMode
  return colors[mode]
}

export function useColorMode(): ThemeMode {
  const theme = useTheme()
  return (theme.palette.mode || 'light') as ThemeMode
}
