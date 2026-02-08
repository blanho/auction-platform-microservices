import { createTheme } from '@mui/material/styles'
import type { ThemeOptions } from '@mui/material/styles'

const baseTheme: ThemeOptions = {
  typography: {
    fontFamily: '"Inter", "Helvetica Neue", "Arial", sans-serif',
    h1: {
      fontFamily: '"Playfair Display", Georgia, serif',
      fontSize: '2.75rem',
      fontWeight: 400,
      lineHeight: 1.15,
      letterSpacing: '-0.02em',
    },
    h2: {
      fontFamily: '"Playfair Display", Georgia, serif',
      fontSize: '2.25rem',
      fontWeight: 400,
      lineHeight: 1.2,
      letterSpacing: '-0.01em',
    },
    h3: {
      fontFamily: '"Playfair Display", Georgia, serif',
      fontSize: '1.75rem',
      fontWeight: 500,
      lineHeight: 1.25,
    },
    h4: {
      fontSize: '1.375rem',
      fontWeight: 600,
      lineHeight: 1.35,
      letterSpacing: '-0.01em',
    },
    h5: {
      fontSize: '1.125rem',
      fontWeight: 600,
      lineHeight: 1.4,
    },
    h6: {
      fontSize: '1rem',
      fontWeight: 600,
      lineHeight: 1.5,
    },
    body1: {
      fontSize: '0.9375rem',
      lineHeight: 1.6,
      letterSpacing: '0.01em',
    },
    body2: {
      fontSize: '0.8125rem',
      lineHeight: 1.6,
      letterSpacing: '0.01em',
    },
    button: {
      textTransform: 'none',
      fontWeight: 500,
      letterSpacing: '0.03em',
    },
    overline: {
      fontSize: '0.6875rem',
      fontWeight: 500,
      letterSpacing: '0.12em',
      textTransform: 'uppercase',
      lineHeight: 1.5,
    },
  },
  shape: {
    borderRadius: 2,
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 0,
          padding: '10px 24px',
          fontSize: '0.8125rem',
          letterSpacing: '0.08em',
          fontWeight: 500,
        },
        sizeLarge: {
          padding: '14px 32px',
          fontSize: '0.875rem',
        },
        contained: {
          boxShadow: 'none',
          '&:hover': {
            boxShadow: 'none',
          },
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 0,
          boxShadow: 'none',
          border: '1px solid #E7E5E4',
        },
      },
    },
    MuiTextField: {
      defaultProps: {
        variant: 'outlined',
        size: 'small',
      },
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-root': {
            borderRadius: 0,
          },
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        rounded: {
          borderRadius: 2,
        },
      },
      defaultProps: {
        elevation: 0,
      },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          boxShadow: 'none',
        },
      },
    },
    MuiDivider: {
      styleOverrides: {
        root: {
          borderColor: '#E7E5E4',
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          borderRadius: 0,
          fontWeight: 500,
          letterSpacing: '0.04em',
        },
      },
    },
  },
}

const lightPalette = {
  mode: 'light' as const,
  primary: {
    main: '#1C1917',
    light: '#44403C',
    dark: '#0C0A09',
    contrastText: '#FFFFFF',
  },
  secondary: {
    main: '#78716C',
    light: '#A8A29E',
    dark: '#57534E',
    contrastText: '#FFFFFF',
  },
  error: {
    main: '#DC2626',
    light: '#EF4444',
    dark: '#B91C1C',
  },
  warning: {
    main: '#D97706',
    light: '#F59E0B',
    dark: '#B45309',
  },
  success: {
    main: '#16A34A',
    light: '#22C55E',
    dark: '#15803D',
  },
  info: {
    main: '#2563EB',
    light: '#3B82F6',
    dark: '#1D4ED8',
  },
  background: {
    default: '#FFFFFF',
    paper: '#FFFFFF',
  },
  text: {
    primary: '#1C1917',
    secondary: '#57534E',
    disabled: '#A8A29E',
  },
  divider: '#E7E5E4',
}

const darkPalette = {
  mode: 'dark' as const,
  primary: {
    main: '#FAFAF9',
    light: '#FFFFFF',
    dark: '#E7E5E4',
    contrastText: '#1C1917',
  },
  secondary: {
    main: '#A8A29E',
    light: '#D6D3D1',
    dark: '#78716C',
    contrastText: '#1C1917',
  },
  error: {
    main: '#EF4444',
    light: '#F87171',
    dark: '#DC2626',
  },
  warning: {
    main: '#F59E0B',
    light: '#FBBF24',
    dark: '#D97706',
  },
  success: {
    main: '#22C55E',
    light: '#34D399',
    dark: '#16A34A',
  },
  info: {
    main: '#3B82F6',
    light: '#60A5FA',
    dark: '#2563EB',
  },
  background: {
    default: '#0C0A09',
    paper: '#1C1917',
  },
  text: {
    primary: '#FAFAF9',
    secondary: '#A8A29E',
    disabled: '#57534E',
  },
  divider: '#292524',
}

export const createAppTheme = (mode: 'light' | 'dark') => {
  return createTheme({
    ...baseTheme,
    palette: mode === 'light' ? lightPalette : darkPalette,
  })
}
