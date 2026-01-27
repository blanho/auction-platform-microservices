import type { Variants, Transition } from 'framer-motion'

export const defaultTransition: Transition = {
  duration: 0.4,
  ease: [0.25, 0.1, 0.25, 1],
}

export const springTransition: Transition = {
  type: 'spring',
  stiffness: 300,
  damping: 30,
}

export const fadeIn: Variants = {
  initial: { opacity: 0 },
  animate: { opacity: 1, transition: defaultTransition },
  exit: { opacity: 0, transition: { duration: 0.2 } },
}

export const fadeInUp: Variants = {
  initial: { opacity: 0, y: 20 },
  animate: { opacity: 1, y: 0, transition: defaultTransition },
  exit: { opacity: 0, y: -10, transition: { duration: 0.2 } },
}

export const fadeInDown: Variants = {
  initial: { opacity: 0, y: -20 },
  animate: { opacity: 1, y: 0, transition: defaultTransition },
  exit: { opacity: 0, y: 10, transition: { duration: 0.2 } },
}

export const fadeInLeft: Variants = {
  initial: { opacity: 0, x: -20 },
  animate: { opacity: 1, x: 0, transition: defaultTransition },
  exit: { opacity: 0, x: 20, transition: { duration: 0.2 } },
}

export const fadeInRight: Variants = {
  initial: { opacity: 0, x: 20 },
  animate: { opacity: 1, x: 0, transition: defaultTransition },
  exit: { opacity: 0, x: -20, transition: { duration: 0.2 } },
}

export const scaleIn: Variants = {
  initial: { opacity: 0, scale: 0.95 },
  animate: { opacity: 1, scale: 1, transition: springTransition },
  exit: { opacity: 0, scale: 0.95, transition: { duration: 0.2 } },
}

export const staggerContainer: Variants = {
  initial: {},
  animate: {
    transition: {
      staggerChildren: 0.1,
      delayChildren: 0.1,
    },
  },
}

export const staggerItem: Variants = {
  initial: { opacity: 0, y: 20 },
  animate: { opacity: 1, y: 0, transition: defaultTransition },
}

export const cardHover = {
  rest: { scale: 1, boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' },
  hover: { 
    scale: 1.02, 
    boxShadow: '0 20px 25px -5px rgb(0 0 0 / 0.1)',
    transition: { duration: 0.2 }
  },
}

export const buttonTap = {
  tap: { scale: 0.98 },
}

export const slideInFromBottom: Variants = {
  initial: { y: '100%', opacity: 0 },
  animate: { y: 0, opacity: 1, transition: { ...defaultTransition, duration: 0.5 } },
  exit: { y: '100%', opacity: 0, transition: { duration: 0.3 } },
}

export const slideInFromTop: Variants = {
  initial: { y: '-100%', opacity: 0 },
  animate: { y: 0, opacity: 1, transition: { ...defaultTransition, duration: 0.5 } },
  exit: { y: '-100%', opacity: 0, transition: { duration: 0.3 } },
}

export const pageTransition: Variants = {
  initial: { opacity: 0, y: 10 },
  animate: { opacity: 1, y: 0, transition: { duration: 0.3, ease: 'easeOut' } },
  exit: { opacity: 0, y: -10, transition: { duration: 0.2 } },
}

export const listItem: Variants = {
  initial: { opacity: 0, x: -10 },
  animate: { opacity: 1, x: 0 },
  exit: { opacity: 0, x: 10 },
}

export const modalOverlay: Variants = {
  initial: { opacity: 0 },
  animate: { opacity: 1, transition: { duration: 0.2 } },
  exit: { opacity: 0, transition: { duration: 0.15 } },
}

export const modalContent: Variants = {
  initial: { opacity: 0, scale: 0.95, y: 20 },
  animate: { opacity: 1, scale: 1, y: 0, transition: springTransition },
  exit: { opacity: 0, scale: 0.95, y: 20, transition: { duration: 0.2 } },
}

export const notification: Variants = {
  initial: { opacity: 0, x: 50, scale: 0.95 },
  animate: { opacity: 1, x: 0, scale: 1, transition: springTransition },
  exit: { opacity: 0, x: 50, scale: 0.95, transition: { duration: 0.2 } },
}

export const pulseAnimation = {
  scale: [1, 1.05, 1],
  transition: { duration: 2, repeat: Infinity, ease: 'easeInOut' },
}

export const shimmer: Variants = {
  initial: { backgroundPosition: '-200% 0' },
  animate: { 
    backgroundPosition: '200% 0',
    transition: { duration: 1.5, repeat: Infinity, ease: 'linear' }
  },
}

export const getReducedMotionVariants = (): Variants => ({
  initial: { opacity: 0 },
  animate: { opacity: 1, transition: { duration: 0.01 } },
  exit: { opacity: 0, transition: { duration: 0.01 } },
})

export const useReducedMotion = (): boolean => {
  if (typeof window === 'undefined') return false
  return window.matchMedia('(prefers-reduced-motion: reduce)').matches
}
