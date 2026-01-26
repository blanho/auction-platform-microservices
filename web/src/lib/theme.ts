import { createTheme } from '@mui/material/styles'
import type { ThemeOptions } from '@mui/material/styles'

const baseTheme: ThemeOptions = {
  typography: {
    fontFamily: '"DM Sans", "Inter", "Roboto", "Helvetica", "Arial", sans-serif',
    h1: {
      fontFamily: '"Playfair Display", Georgia, serif',
      fontSize: '3rem',
      fontWeight: 700,
      lineHeight: 1.2,
      letterSpacing: '-0.02em',
    },
    h2: {
      fontFamily: '"Playfair Display", Georgia, serif',
      fontSize: '2.25rem',
      fontWeight: 600,
      lineHeight: 1.3,
      letterSpacing: '-0.01em',
    },
    h3: {
      fontFamily: '"Playfair Display", Georgia, serif',
      fontSize: '1.875rem',
      fontWeight: 600,
      lineHeight: 1.3,
    },
    h4: {
      fontFamily: '"DM Sans", sans-serif',
      fontSize: '1.5rem',
      fontWeight: 600,
      lineHeight: 1.4,
    },
    h5: {
      fontFamily: '"DM Sans", sans-serif',
      fontSize: '1.25rem',
      fontWeight: 600,
      lineHeight: 1.4,
    },
    h6: {
      fontFamily: '"DM Sans", sans-serif',
      fontSize: '1rem',
      fontWeight: 600,
      lineHeight: 1.5,
    },
    body1: {
      fontFamily: '"DM Sans", sans-serif',
      fontSize: '1rem',
      lineHeight: 1.6,
    },
    body2: {
      fontFamily: '"DM Sans", sans-serif',
      fontSize: '0.875rem',
      lineHeight: 1.5,
    },
    button: {
      fontFamily: '"DM Sans", sans-serif',
      textTransform: 'none',
      fontWeight: 600,
    },
    caption: {
      fontFamily: '"DM Sans", sans-serif',
      fontSize: '0.75rem',
      lineHeight: 1.5,
    },
    overline: {
      fontFamily: '"DM Sans", sans-serif',
      fontSize: '0.75rem',
      fontWeight: 600,
      letterSpacing: '0.1em',
      textTransform: 'uppercase',
    },
  },
  shape: {
    borderRadius: 8,
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 4,
          padding: '10px 24px',
          fontWeight: 600,
        },
        sizeLarge: {
          padding: '14px 32px',
          fontSize: '1rem',
        },
        sizeSmall: {
          padding: '6px 16px',
          fontSize: '0.875rem',
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          boxShadow: '0 1px 3px rgba(0,0,0,0.08)',
          border: '1px solid #E5E5E5',
        },
      },
    },
    MuiTextField: {
      defaultProps: {
        variant: 'outlined',
      },
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-root': {
            borderRadius: 4,
          },
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          borderRadius: 4,
          fontWeight: 500,
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        rounded: {
          borderRadius: 8,
        },
      },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          boxShadow: 'none',
          borderBottom: '1px solid #E5E5E5',
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
    main: '#44403C',
    light: '#78716C',
    dark: '#1C1917',
    contrastText: '#FFFFFF',
  },
  warning: {
    main: '#CA8A04',
    light: '#EAB308',
    dark: '#A16207',
    contrastText: '#FFFFFF',
  },
  error: {
    main: '#DC2626',
    light: '#EF4444',
    dark: '#B91C1C',
    contrastText: '#FFFFFF',
  },
  success: {
    main: '#16A34A',
    light: '#22C55E',
    dark: '#15803D',
    contrastText: '#FFFFFF',
  },
  info: {
    main: '#0284C7',
    light: '#0EA5E9',
    dark: '#0369A1',
    contrastText: '#FFFFFF',
  },
  background: {
    default: '#FAFAF9',
    paper: '#FFFFFF',
  },
  text: {
    primary: '#0C0A09',
    secondary: '#44403C',
    disabled: '#A8A29E',
  },
  divider: '#E5E5E5',
  action: {
    active: '#1C1917',
    hover: 'rgba(28, 25, 23, 0.04)',
    selected: 'rgba(28, 25, 23, 0.08)',
    disabled: 'rgba(28, 25, 23, 0.26)',
    disabledBackground: 'rgba(28, 25, 23, 0.12)',
  },
}

const darkPalette = {
  mode: 'dark' as const,
  primary: {
    main: '#FAFAF9',
    light: '#FFFFFF',
    dark: '#E5E5E5',
    contrastText: '#1C1917',
  },
  secondary: {
    main: '#A8A29E',
    light: '#D6D3D1',
    dark: '#78716C',
    contrastText: '#1C1917',
  },
  warning: {
    main: '#EAB308',
    light: '#FACC15',
    dark: '#CA8A04',
    contrastText: '#1C1917',
  },
  error: {
    main: '#EF4444',
    light: '#F87171',
    dark: '#DC2626',
    contrastText: '#FFFFFF',
  },
  success: {
    main: '#22C55E',
    light: '#4ADE80',
    dark: '#16A34A',
    contrastText: '#1C1917',
  },
  info: {
    main: '#0EA5E9',
    light: '#38BDF8',
    dark: '#0284C7',
    contrastText: '#1C1917',
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
  action: {
    active: '#FAFAF9',
    hover: 'rgba(250, 250, 249, 0.08)',
    selected: 'rgba(250, 250, 249, 0.12)',
    disabled: 'rgba(250, 250, 249, 0.26)',
    disabledBackground: 'rgba(250, 250, 249, 0.12)',
  },
}

export const createAppTheme = (mode: 'light' | 'dark') => {
  return createTheme({
    ...baseTheme,
    palette: mode === 'light' ? lightPalette : darkPalette,
  })
}

export const theme = createAppTheme('light')
