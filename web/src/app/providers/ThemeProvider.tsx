import { createAppTheme } from '@/styles/theme'
import { CssBaseline, ThemeProvider as MuiThemeProvider } from '@mui/material'
import type { ReactNode } from 'react'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ThemeContext } from '../context/ThemeContext'

type ThemeMode = 'light' | 'dark'

interface ThemeProviderProps {
  children: ReactNode
}

export function ThemeProvider({ children }: ThemeProviderProps) {
  const { i18n } = useTranslation()
  const [mode, setMode] = useState<ThemeMode>(() => {
    const saved = localStorage.getItem('theme-mode')
    return (saved as ThemeMode) || 'light'
  })

  const theme = useMemo(
    () => createAppTheme(mode, i18n.resolvedLanguage),
    [mode, i18n.resolvedLanguage]
  )

  const toggleTheme = () => {
    const newMode = mode === 'light' ? 'dark' : 'light'
    setMode(newMode)
    localStorage.setItem('theme-mode', newMode)
  }

  return (
    <ThemeContext.Provider value={{ mode, toggleTheme }}>
      <MuiThemeProvider theme={theme}>
        <CssBaseline />
        {children}
      </MuiThemeProvider>
    </ThemeContext.Provider>
  )
}
