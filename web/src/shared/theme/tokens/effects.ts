export const shadows = {
  none: 'none',
  xs: '0 1px 2px rgba(0, 0, 0, 0.05)',
  sm: '0 1px 3px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06)',
  md: '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
  lg: '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
  xl: '0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)',
  '2xl': '0 25px 50px -12px rgba(0, 0, 0, 0.25)',
  inner: 'inset 0 2px 4px rgba(0, 0, 0, 0.06)',
  brand: '0 8px 32px rgba(202, 138, 4, 0.3)',
  brandHover: '0 12px 40px rgba(202, 138, 4, 0.4)',
  card: '0 24px 48px rgba(0, 0, 0, 0.12)',
  cardDark: '0 24px 48px rgba(0, 0, 0, 0.3)',
} as const

export const blur = {
  none: 'blur(0)',
  sm: 'blur(4px)',
  md: 'blur(8px)',
  lg: 'blur(16px)',
  xl: 'blur(24px)',
  '2xl': 'blur(40px)',
} as const

export const transitions = {
  fast: '150ms ease',
  normal: '200ms ease',
  slow: '300ms ease',
  slower: '500ms ease',
} as const

export const duration = {
  instant: 0,
  fast: 0.15,
  normal: 0.2,
  slow: 0.3,
  slower: 0.5,
  slowest: 0.8,
  floating: 6,
} as const

export const easing = {
  linear: 'linear',
  ease: 'ease',
  easeIn: 'ease-in',
  easeOut: 'ease-out',
  easeInOut: 'ease-in-out',
  spring: [0.16, 1, 0.3, 1],
} as const

export const zIndex = {
  hide: -1,
  base: 0,
  dropdown: 1000,
  sticky: 1100,
  fixed: 1200,
  overlay: 1300,
  modal: 1400,
  popover: 1500,
  toast: 1600,
  tooltip: 1700,
} as const

export type Shadows = typeof shadows
export type Blur = typeof blur
export type Transitions = typeof transitions
export type Duration = typeof duration
export type ZIndex = typeof zIndex
