export const spacing = {
  0: '0',
  0.5: '2px',
  1: '4px',
  1.5: '6px',
  2: '8px',
  2.5: '10px',
  3: '12px',
  3.5: '14px',
  4: '16px',
  5: '20px',
  6: '24px',
  7: '28px',
  8: '32px',
  9: '36px',
  10: '40px',
  11: '44px',
  12: '48px',
  14: '56px',
  16: '64px',
  20: '80px',
  24: '96px',
  28: '112px',
  32: '128px',
} as const

export const space = {
  page: {
    x: { xs: spacing[4], sm: spacing[6], md: spacing[8] },
    y: { xs: spacing[10], md: spacing[16] },
  },
  section: {
    gap: { xs: spacing[10], md: spacing[16] },
    padding: { xs: spacing[10], md: spacing[16] },
  },
  card: {
    padding: { xs: spacing[4], md: spacing[6] },
    gap: spacing[4],
  },
  stack: {
    xs: spacing[1],
    sm: spacing[2],
    md: spacing[4],
    lg: spacing[6],
    xl: spacing[8],
  },
  inline: {
    xs: spacing[1],
    sm: spacing[2],
    md: spacing[3],
    lg: spacing[4],
  },
} as const

export const radius = {
  none: '0',
  sm: '4px',
  md: '8px',
  lg: '12px',
  xl: '16px',
  '2xl': '24px',
  full: '9999px',
} as const

export type Spacing = typeof spacing
export type Space = typeof space
export type Radius = typeof radius
