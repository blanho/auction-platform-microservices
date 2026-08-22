import { gradients as newGradients, palette } from './tokens/colors'
import { zIndex } from './tokens/effects'
import { radius, space, spacing } from './tokens/spacing'
import {
  fontFamily,
  fontSize,
  fontWeight,
  letterSpacing,
  lineHeight,
  textStyle,
} from './tokens/typography'

export const colors = {
  background: {
    primary: palette.neutral[0],
    secondary: palette.neutral[50],
    tertiary: palette.neutral[100],
    glass: 'rgba(0, 0, 0, 0.02)',
    elevated: palette.neutral[0],
  },
  text: {
    primary: palette.neutral[900],
    secondary: palette.neutral[600],
    muted: palette.neutral[500],
    subtle: palette.neutral[400],
    disabled: palette.neutral[300],
    faint: palette.neutral[300],
    ghost: palette.neutral[200],
  },
  gold: {
    primary: palette.neutral[900],
    secondary: palette.neutral[800],
    light: palette.neutral[700],
    gradient: newGradients.brand,
    gradientHover: newGradients.brandHover,
    shimmer: newGradients.brandShimmer,
  },
  purple: palette.purple,
  success: palette.semantic.success,
  error: palette.semantic.error,
  accent: {
    green: palette.semantic.success,
    greenLight: palette.semantic.successLight,
    greenMuted: palette.semantic.successMuted,
    red: palette.semantic.error,
    purple: palette.purple.light,
    purpleMuted: palette.purple.muted,
  },
  border: {
    subtle: palette.neutral[100],
    light: palette.neutral[200],
    medium: palette.neutral[300],
    strong: palette.neutral[400],
    visible: palette.neutral[200],
  },
  glass: {
    background: 'rgba(0, 0, 0, 0.02)',
    border: palette.neutral[200],
  },
  overlay: {
    dark: 'rgba(0,0,0,0.3)',
    darker: 'rgba(0,0,0,0.5)',
    darkest: 'rgba(0,0,0,0.7)',
  },
  neutral: palette.neutral,
} as const

export const typography = {
  fontFamily: {
    display: fontFamily.display,
    body: fontFamily.body,
    serif: fontFamily.display,
    sans: fontFamily.body,
  },
  fontWeight,
  fontSize,
  lineHeight,
  letterSpacing,
  textStyle,
} as const

export { palette, radius, space, spacing, zIndex }
