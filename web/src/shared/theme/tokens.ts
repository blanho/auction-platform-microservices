import { palette, gradients as newGradients } from './tokens/colors'
import { spacing, space, radius } from './tokens/spacing'
import {
  fontFamily,
  fontSize,
  fontWeight,
  lineHeight,
  letterSpacing,
  textStyle,
} from './tokens/typography'
import {
  shadows as newShadows,
  blur as newBlur,
  transitions as newTransitions,
  duration,
  easing,
  zIndex,
} from './tokens/effects'

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

export const gradients = {
  gold: newGradients.brand,
  goldHover: newGradients.brandHover,
  goldShimmer: newGradients.brandShimmer,
  goldButton: newGradients.brand,
  goldButtonHover: newGradients.brandHover,
  heroGlow: newGradients.heroGlow,
  purpleGlow: newGradients.purpleGlow,
  cardOverlay: newGradients.cardOverlay,
  cardOverlayHover: newGradients.cardOverlayHover,
  horizontalDivider: newGradients.divider,
  topLine: newGradients.divider,
  ctaBackground: `linear-gradient(135deg, ${palette.neutral[50]} 0%, ${palette.neutral[100]} 100%)`,
  radial: {
    gold: newGradients.heroGlow,
    purple: newGradients.purpleGlow,
  },
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

export const shadows = {
  ...newShadows,
  card: newShadows.cardDark,
  button: newShadows.brand,
  buttonHover: newShadows.brandHover,
  gold: newShadows.brand,
  goldHover: newShadows.brandHover,
} as const

export const transitions = {
  fast: newTransitions.fast,
  normal: newTransitions.normal,
  slow: newTransitions.slow,
  slower: newTransitions.slower,
  duration,
  easing,
} as const

export const blur = newBlur

export { palette, spacing, space, radius, zIndex }

export const tokens = {
  colors,
  gradients,
  typography,
  shadows,
  transitions,
  blur,
  palette,
  spacing,
  space,
  radius,
  zIndex,
} as const

export type ThemeColors = typeof colors
export type ThemeGradients = typeof gradients
export type ThemeTypography = typeof typography
