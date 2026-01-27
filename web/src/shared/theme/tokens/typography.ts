export const fontFamily = {
  display: '"Cormorant Garamond", Georgia, serif',
  body: '"Montserrat", system-ui, sans-serif',
  mono: '"JetBrains Mono", "Fira Code", monospace',
} as const

export const fontSize = {
  xs: '0.75rem',
  sm: '0.875rem',
  base: '1rem',
  lg: '1.125rem',
  xl: '1.25rem',
  '2xl': '1.5rem',
  '3xl': '1.875rem',
  '4xl': '2.25rem',
  '5xl': '3rem',
  '6xl': '3.75rem',
  '7xl': '4.5rem',
} as const

export const fontWeight = {
  light: 300,
  regular: 400,
  medium: 500,
  semibold: 600,
  bold: 700,
} as const

export const lineHeight = {
  none: 1,
  tight: 1.2,
  snug: 1.3,
  normal: 1.5,
  relaxed: 1.625,
  loose: 1.75,
} as const

export const letterSpacing = {
  tighter: '-0.02em',
  tight: '-0.01em',
  normal: '0',
  wide: '0.02em',
  wider: '0.05em',
  widest: '0.1em',
} as const

export const textStyle = {
  displayXl: {
    fontFamily: fontFamily.display,
    fontSize: fontSize['7xl'],
    fontWeight: fontWeight.light,
    lineHeight: lineHeight.none,
    letterSpacing: letterSpacing.tighter,
  },
  displayLg: {
    fontFamily: fontFamily.display,
    fontSize: fontSize['5xl'],
    fontWeight: fontWeight.light,
    lineHeight: lineHeight.none,
    letterSpacing: letterSpacing.tighter,
  },
  displayMd: {
    fontFamily: fontFamily.display,
    fontSize: fontSize['3xl'],
    fontWeight: fontWeight.regular,
    lineHeight: lineHeight.tight,
  },
  displaySm: {
    fontFamily: fontFamily.display,
    fontSize: fontSize['2xl'],
    fontWeight: fontWeight.regular,
    lineHeight: lineHeight.snug,
  },
  headingLg: {
    fontFamily: fontFamily.body,
    fontSize: fontSize['2xl'],
    fontWeight: fontWeight.semibold,
    lineHeight: lineHeight.snug,
  },
  headingMd: {
    fontFamily: fontFamily.body,
    fontSize: fontSize.xl,
    fontWeight: fontWeight.semibold,
    lineHeight: lineHeight.snug,
  },
  headingSm: {
    fontFamily: fontFamily.body,
    fontSize: fontSize.lg,
    fontWeight: fontWeight.semibold,
    lineHeight: lineHeight.normal,
  },
  bodyLg: {
    fontFamily: fontFamily.body,
    fontSize: fontSize.lg,
    fontWeight: fontWeight.regular,
    lineHeight: lineHeight.relaxed,
  },
  bodyMd: {
    fontFamily: fontFamily.body,
    fontSize: fontSize.base,
    fontWeight: fontWeight.regular,
    lineHeight: lineHeight.normal,
  },
  bodySm: {
    fontFamily: fontFamily.body,
    fontSize: fontSize.sm,
    fontWeight: fontWeight.regular,
    lineHeight: lineHeight.normal,
  },
  bodyXs: {
    fontFamily: fontFamily.body,
    fontSize: fontSize.xs,
    fontWeight: fontWeight.regular,
    lineHeight: lineHeight.normal,
  },
  label: {
    fontFamily: fontFamily.body,
    fontSize: fontSize.sm,
    fontWeight: fontWeight.medium,
    lineHeight: lineHeight.normal,
    letterSpacing: letterSpacing.wide,
  },
  overline: {
    fontFamily: fontFamily.body,
    fontSize: fontSize.xs,
    fontWeight: fontWeight.medium,
    lineHeight: lineHeight.normal,
    letterSpacing: letterSpacing.widest,
    textTransform: 'uppercase' as const,
  },
  button: {
    fontFamily: fontFamily.body,
    fontSize: fontSize.base,
    fontWeight: fontWeight.semibold,
    lineHeight: lineHeight.normal,
    textTransform: 'none' as const,
  },
  buttonSm: {
    fontFamily: fontFamily.body,
    fontSize: fontSize.sm,
    fontWeight: fontWeight.medium,
    lineHeight: lineHeight.normal,
    textTransform: 'none' as const,
  },
  code: {
    fontFamily: fontFamily.mono,
    fontSize: fontSize.sm,
    fontWeight: fontWeight.regular,
    lineHeight: lineHeight.relaxed,
  },
} as const

export type FontFamily = typeof fontFamily
export type FontSize = typeof fontSize
export type FontWeight = typeof fontWeight
export type TextStyle = typeof textStyle
