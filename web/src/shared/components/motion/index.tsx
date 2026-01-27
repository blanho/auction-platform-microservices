import { forwardRef } from 'react'
import type { ComponentProps, ReactNode } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import type { Variants, HTMLMotionProps } from 'framer-motion'
import {
  fadeIn,
  fadeInUp,
  fadeInDown,
  fadeInLeft,
  fadeInRight,
  scaleIn,
  staggerContainer,
  staggerItem,
  pageTransition,
  useReducedMotion,
  getReducedMotionVariants,
} from '@/shared/lib/animations'

type AnimationType = 'fadeIn' | 'fadeInUp' | 'fadeInDown' | 'fadeInLeft' | 'fadeInRight' | 'scaleIn' | 'page'

const variantMap: Record<AnimationType, Variants> = {
  fadeIn,
  fadeInUp,
  fadeInDown,
  fadeInLeft,
  fadeInRight,
  scaleIn,
  page: pageTransition,
}

interface MotionBoxProps extends HTMLMotionProps<'div'> {
  animation?: AnimationType
  delay?: number
  children?: ReactNode
}

export const MotionBox = forwardRef<HTMLDivElement, MotionBoxProps>(
  ({ animation = 'fadeInUp', delay = 0, children, ...props }, ref) => {
    const prefersReducedMotion = useReducedMotion()
    const variants = prefersReducedMotion 
      ? getReducedMotionVariants()
      : variantMap[animation]

    return (
      <motion.div
        ref={ref}
        initial="initial"
        animate="animate"
        exit="exit"
        variants={variants}
        style={{ willChange: 'opacity, transform' }}
        transition={{ delay }}
        {...props}
      >
        {children}
      </motion.div>
    )
  }
)

MotionBox.displayName = 'MotionBox'

interface StaggerContainerProps extends HTMLMotionProps<'div'> {
  children?: ReactNode
  staggerDelay?: number
}

export const StaggerContainer = ({ children, staggerDelay = 0.1, ...props }: StaggerContainerProps) => {
  const prefersReducedMotion = useReducedMotion()
  
  if (prefersReducedMotion) {
    return <div {...(props as ComponentProps<'div'>)}>{children}</div>
  }

  return (
    <motion.div
      initial="initial"
      animate="animate"
      variants={{
        initial: {},
        animate: {
          transition: {
            staggerChildren: staggerDelay,
            delayChildren: 0.1,
          },
        },
      }}
      {...props}
    >
      {children}
    </motion.div>
  )
}

interface StaggerItemProps extends HTMLMotionProps<'div'> {
  children?: ReactNode
}

export const StaggerItem = ({ children, ...props }: StaggerItemProps) => {
  const prefersReducedMotion = useReducedMotion()

  if (prefersReducedMotion) {
    return <div {...(props as ComponentProps<'div'>)}>{children}</div>
  }

  return (
    <motion.div variants={staggerItem} {...props}>
      {children}
    </motion.div>
  )
}

interface PageTransitionProps {
  children: ReactNode
  className?: string
}

export const PageTransition = ({ children, className }: PageTransitionProps) => {
  const prefersReducedMotion = useReducedMotion()

  if (prefersReducedMotion) {
    return <div className={className}>{children}</div>
  }

  return (
    <motion.div
      className={className}
      initial="initial"
      animate="animate"
      exit="exit"
      variants={pageTransition}
    >
      {children}
    </motion.div>
  )
}

interface AnimatedListProps {
  children: ReactNode[]
  className?: string
}

export const AnimatedList = ({ children, className }: AnimatedListProps) => {
  const prefersReducedMotion = useReducedMotion()

  return (
    <AnimatePresence mode="popLayout">
      <motion.ul
        className={className}
        initial="initial"
        animate="animate"
        variants={prefersReducedMotion ? {} : staggerContainer}
      >
        {children}
      </motion.ul>
    </AnimatePresence>
  )
}

export { AnimatePresence } from 'framer-motion'
export { AnimatedCounter } from './AnimatedCounter'
