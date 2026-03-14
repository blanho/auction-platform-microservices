export const palette = {
  brand: {
    primary: '#1C1917',
    secondary: '#292524',
    accent: '#CA8A04',
    muted: 'rgba(28, 25, 23, 0.08)',
    hover: '#44403C',
    dark: '#0C0A09',
  },

  purple: {
    primary: '#78716C',
    light: 'rgba(120, 113, 108, 0.1)',
    muted: 'rgba(120, 113, 108, 0.08)',
  },

  semantic: {
    success: '#16A34A',
    successHover: '#15803D',
    successLight: 'rgba(22, 163, 74, 0.08)',
    successMuted: 'rgba(22, 163, 74, 0.12)',
    error: '#DC2626',
    errorHover: '#B91C1C',
    errorLight: 'rgba(220, 38, 38, 0.08)',
    warning: '#D97706',
    warningHover: '#B45309',
    warningLight: 'rgba(217, 119, 6, 0.08)',
    info: '#2563EB',
    infoHover: '#1D4ED8',
    infoLight: 'rgba(37, 99, 235, 0.08)',
  },

  neutral: {
    0: '#FFFFFF',
    50: '#FAFAF9',
    100: '#F5F5F4',
    200: '#E7E5E4',
    300: '#D6D3D1',
    400: '#A8A29E',
    500: '#78716C',
    600: '#57534E',
    700: '#44403C',
    800: '#292524',
    900: '#1C1917',
    950: '#0C0A09',
  },
} as const

export const colors = {
  light: {
    bg: {
      primary: palette.neutral[0],
      secondary: palette.neutral[50],
      tertiary: palette.neutral[100],
      elevated: palette.neutral[0],
      glass: 'rgba(0, 0, 0, 0.02)',
      overlay: 'rgba(0, 0, 0, 0.5)',
    },
    text: {
      primary: palette.neutral[900],
      secondary: palette.neutral[600],
      muted: palette.neutral[500],
      disabled: palette.neutral[400],
      inverse: palette.neutral[50],
    },
    border: {
      subtle: palette.neutral[100],
      default: palette.neutral[200],
      strong: palette.neutral[300],
      focus: palette.neutral[900],
    },
    brand: {
      primary: palette.brand.primary,
      secondary: palette.brand.secondary,
      accent: palette.brand.accent,
      muted: palette.brand.muted,
    },
  },
  dark: {
    bg: {
      primary: palette.neutral[950],
      secondary: palette.neutral[900],
      tertiary: palette.neutral[800],
      elevated: palette.neutral[800],
      glass: 'rgba(255, 255, 255, 0.05)',
      overlay: 'rgba(0, 0, 0, 0.7)',
    },
    text: {
      primary: palette.neutral[50],
      secondary: palette.neutral[400],
      muted: palette.neutral[500],
      disabled: palette.neutral[600],
      inverse: palette.neutral[900],
    },
    border: {
      subtle: 'rgba(255, 255, 255, 0.05)',
      default: 'rgba(255, 255, 255, 0.1)',
      strong: 'rgba(255, 255, 255, 0.2)',
      focus: palette.brand.accent,
    },
    brand: {
      primary: palette.brand.primary,
      secondary: palette.brand.secondary,
      accent: palette.brand.accent,
      muted: palette.brand.muted,
    },
  },
} as const

export const gradients = {
  brand: `linear-gradient(135deg, ${palette.neutral[900]} 0%, ${palette.neutral[700]} 100%)`,
  brandHover: `linear-gradient(135deg, ${palette.neutral[800]} 0%, ${palette.neutral[600]} 100%)`,
  brandShimmer: `linear-gradient(135deg, ${palette.neutral[900]} 0%, ${palette.neutral[700]} 50%, ${palette.neutral[900]} 100%)`,
  heroGlow: `radial-gradient(ellipse 80% 50% at 50% -20%, rgba(28, 25, 23, 0.03), transparent)`,
  purpleGlow: `radial-gradient(ellipse 60% 40% at 100% 100%, rgba(120, 113, 108, 0.05), transparent)`,
  cardOverlay: 'linear-gradient(to top, rgba(0,0,0,0.6) 0%, rgba(0,0,0,0.1) 50%, transparent 100%)',
  cardOverlayHover:
    'linear-gradient(to top, rgba(0,0,0,0.7) 0%, rgba(0,0,0,0.15) 50%, transparent 100%)',
  divider: `linear-gradient(90deg, transparent, ${palette.neutral[200]}, transparent)`,
} as const

export type ThemeMode = 'light' | 'dark'
export type ColorTokens = typeof colors.light
export type Palette = typeof palette
export type Gradients = typeof gradients
