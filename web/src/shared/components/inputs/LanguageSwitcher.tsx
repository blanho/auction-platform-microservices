import { supportedLanguages } from '@/i18n'
import { Language } from '@mui/icons-material'
import { IconButton, ListItemIcon, ListItemText, Menu, MenuItem, Typography } from '@mui/material'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'

export function LanguageSwitcher() {
  const { t, i18n } = useTranslation('common')
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

  const currentLanguage =
    supportedLanguages.find(
      (lang) => lang.code === i18n.language || i18n.language.startsWith(lang.code)
    ) || supportedLanguages[0]

  return (
    <>
      <IconButton
        onClick={handleClick}
        size="small"
        aria-label={t('language.change')}
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
            <ListItemText>{t(`language.${lang.code}`)}</ListItemText>
          </MenuItem>
        ))}
      </Menu>
    </>
  )
}
