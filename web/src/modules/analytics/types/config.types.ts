import type { AdminDashboardStats } from './dashboard.types'

export interface StatCardConfig {
  key: keyof AdminDashboardStats
  label: string
  icon: React.ReactNode
  color: string
  format: 'currency' | 'number'
  changeKey: keyof AdminDashboardStats
}
