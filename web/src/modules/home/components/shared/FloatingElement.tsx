import { motion } from 'framer-motion'
import { transitions } from '@/shared/theme/tokens'

interface FloatingElementProps {
  delay?: number
  children: React.ReactNode
}

export const FloatingElement = ({ delay = 0, children }: FloatingElementProps) => (
  <motion.div
    initial={{ y: 0 }}
    animate={{ y: [-10, 10, -10] }}
    transition={{
      duration: transitions.duration.floating,
      repeat: Infinity,
      ease: 'easeInOut',
      delay,
    }}
  >
    {children}
  </motion.div>
)
