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
