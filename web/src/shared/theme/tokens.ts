import {
  palette,
  gradients as newGradients,
} from './tokens/colors'
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
    primary: palette.neutral[950],
    secondary: palette.neutral[900],
    tertiary: palette.neutral[950],
    glass: 'rgba(255,255,255,0.05)',
    elevated: 'rgba(255,255,255,0.1)',
  },
  text: {
    primary: palette.neutral[50],
    secondary: 'rgba(255,255,255,0.7)',
    muted: 'rgba(255,255,255,0.8)',
    subtle: 'rgba(255,255,255,0.6)',
    disabled: 'rgba(255,255,255,0.5)',
    faint: 'rgba(255,255,255,0.4)',
    ghost: 'rgba(255,255,255,0.3)',
  },
  gold: {
    primary: palette.brand.primary,
    secondary: palette.brand.secondary,
    light: palette.brand.accent,
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
    subtle: 'rgba(255,255,255,0.05)',
    light: 'rgba(255,255,255,0.2)',
    medium: 'rgba(255,255,255,0.3)',
    strong: 'rgba(255,255,255,0.5)',
    visible: 'rgba(255,255,255,0.12)',
  },
  glass: {
    background: 'rgba(255, 255, 255, 0.08)',
    border: 'rgba(255, 255, 255, 0.12)',
  },
  overlay: {
    dark: 'rgba(0,0,0,0.5)',
    darker: 'rgba(0,0,0,0.7)',
    darkest: 'rgba(0,0,0,0.75)',
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
  ctaBackground: 'linear-gradient(135deg, rgba(202, 138, 4, 0.1) 0%, rgba(147, 51, 234, 0.1) 100%)',
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
