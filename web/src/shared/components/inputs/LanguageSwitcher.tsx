import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { IconButton, Menu, MenuItem, ListItemIcon, ListItemText, Typography } from '@mui/material'
import { Language } from '@mui/icons-material'
import { supportedLanguages } from '@/i18n'

export function LanguageSwitcher() {
  const { i18n } = useTranslation()
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const open = Boolean(anchorEl)

  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const handleLanguageChange = (langCode: string) => {
    i18n.changeLanguage(langCode)
    handleClose()
  }

  const currentLanguage = supportedLanguages.find(
    (lang) => lang.code === i18n.language || i18n.language.startsWith(lang.code)
  ) || supportedLanguages[0]

  return (
    <>
      <IconButton
        onClick={handleClick}
        size="small"
        aria-label="Change language"
        aria-controls={open ? 'language-menu' : undefined}
        aria-haspopup="true"
        aria-expanded={open ? 'true' : undefined}
        sx={{ color: 'inherit' }}
      >
        <Language />
      </IconButton>
      <Menu
        id="language-menu"
        anchorEl={anchorEl}
        open={open}
        onClose={handleClose}
        anchorOrigin={{
          vertical: 'bottom',
          horizontal: 'right',
        }}
        transformOrigin={{
          vertical: 'top',
          horizontal: 'right',
        }}
      >
        {supportedLanguages.map((lang) => (
          <MenuItem
            key={lang.code}
            onClick={() => handleLanguageChange(lang.code)}
            selected={currentLanguage.code === lang.code}
          >
            <ListItemIcon>
              <Typography variant="body1">{lang.flag}</Typography>
            </ListItemIcon>
            <ListItemText>{lang.label}</ListItemText>
          </MenuItem>
        ))}
      </Menu>
    </>
  )
}
