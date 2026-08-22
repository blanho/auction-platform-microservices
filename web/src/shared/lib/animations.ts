import type { Transition, Variants } from 'framer-motion'

export const defaultTransition: Transition = {
  duration: 0.4,
  ease: [0.25, 0.1, 0.25, 1],
}

export const springTransition: Transition = {
  type: 'spring',
  stiffness: 300,
  damping: 30,
}

export const fadeInUp: Variants = {
  initial: { opacity: 0, y: 20 },
  animate: { opacity: 1, y: 0, transition: defaultTransition },
  exit: { opacity: 0, y: -10, transition: { duration: 0.2 } },
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
    transition: { duration: 0.2 },
  },
}
