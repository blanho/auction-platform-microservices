import { useContext } from 'react'
import { ThemeContext, type ThemeContextType } from '../context/ThemeContext'

export function useThemeMode(): ThemeContextType {
  const context = useContext(ThemeContext)
  if (context === undefined) {
    throw new Error('useThemeMode must be used within a ThemeProvider')
  }
  return context
}
