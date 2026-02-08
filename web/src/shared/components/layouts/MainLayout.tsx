import { useState } from 'react'
import { Link, useNavigate, useLocation, Outlet } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import {
  AppBar,
  Box,
  Toolbar,
  IconButton,
  Typography,
  Menu,
  Container,
  Avatar,
  Button,
  Tooltip,
  MenuItem,
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Divider,
  Badge,
} from '@mui/material'
import {
  Menu as MenuIcon,
  Notifications as NotificationsIcon,
  DarkMode as DarkModeIcon,
  LightMode as LightModeIcon,
  Home as HomeIcon,
  Category as CategoryIcon,
  Person as PersonIcon,
  Settings as SettingsIcon,
  Logout as LogoutIcon,
  Search as SearchIcon,
  Gavel as GavelIcon,
} from '@mui/icons-material'
import { useAuth, useThemeMode } from '@/app/providers'
import { useNotificationSummary } from '@/modules/notifications/hooks'
import { LanguageSwitcher } from '../inputs'
import { SearchAutocomplete } from '@/modules/search/components/SearchAutocomplete'
import { typography } from '@/shared/theme/tokens'

export const MainLayout = () => {
  const { t } = useTranslation()
  const { user, isAuthenticated, logout } = useAuth()
  const { mode, toggleTheme } = useThemeMode()
  const { data: notificationSummary } = useNotificationSummary()
  const navigate = useNavigate()
  const location = useLocation()

  const navItems = [
    { label: t('nav.home'), path: '/', icon: <HomeIcon /> },
    { label: t('nav.auctions'), path: '/auctions', icon: <GavelIcon /> },
    { label: 'Categories', path: '/categories', icon: <CategoryIcon /> },
  ]

  const [mobileOpen, setMobileOpen] = useState(false)
  const [anchorElUser, setAnchorElUser] = useState<null | HTMLElement>(null)

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen)
  }

  const handleOpenUserMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorElUser(event.currentTarget)
  }

  const handleCloseUserMenu = () => {
    setAnchorElUser(null)
  }

  const handleLogout = async () => {
    handleCloseUserMenu()
    await logout()
    navigate('/login')
  }

  const drawer = (
    <Box onClick={handleDrawerToggle} sx={{ textAlign: 'center' }}>
      <Typography
        sx={{
          my: 2,
          fontFamily: typography.fontFamily.display,
          fontWeight: typography.fontWeight.medium,
          fontSize: '1.125rem',
          letterSpacing: '0.08em',
          color: '#1C1917',
        }}
      >
        THE AUCTION
      </Typography>
      <Divider sx={{ borderColor: '#E7E5E4' }} />
      <List>
        {navItems.map((item) => (
          <ListItem key={item.path} disablePadding>
            <ListItemButton
              component={Link}
              to={item.path}
              selected={location.pathname === item.path}
              sx={{
                '&.Mui-selected': {
                  bgcolor: '#FAFAF9',
                  borderRight: '2px solid #1C1917',
                },
              }}
            >
              <ListItemIcon sx={{ color: '#57534E' }}>{item.icon}</ListItemIcon>
              <ListItemText
                primary={item.label}
                slotProps={{
                  primary: {
                    sx: {
                      fontSize: '0.8125rem',
                      fontWeight: typography.fontWeight.medium,
                      letterSpacing: '0.05em',
                      textTransform: 'uppercase',
                    },
                  },
                }}
              />
            </ListItemButton>
          </ListItem>
        ))}
      </List>
    </Box>
  )

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <AppBar
        position="sticky"
        elevation={0}
        sx={{
          bgcolor: '#FFFFFF',
          borderBottom: '1px solid #E7E5E4',
        }}
      >
        <Container maxWidth="xl">
          <Toolbar disableGutters sx={{ minHeight: { xs: 56, sm: 64 } }}>
            <IconButton
              aria-label="open drawer"
              edge="start"
              onClick={handleDrawerToggle}
              sx={{ mr: 2, display: { sm: 'none' }, color: '#1C1917' }}
            >
              <MenuIcon />
            </IconButton>

            <Typography
              noWrap
              component={Link}
              to="/"
              sx={{
                mr: 4,
                display: { xs: 'none', sm: 'flex' },
                fontFamily: typography.fontFamily.display,
                fontWeight: typography.fontWeight.medium,
                fontSize: '1.125rem',
                letterSpacing: '0.08em',
                color: '#1C1917',
                textDecoration: 'none',
              }}
            >
              THE AUCTION
            </Typography>

            <Box sx={{ flexGrow: 1, display: { xs: 'none', sm: 'flex' }, gap: 0.5 }}>
              {navItems.map((item) => {
                const isActive = location.pathname === item.path
                return (
                  <Button
                    key={item.path}
                    component={Link}
                    to={item.path}
                    sx={{
                      color: isActive ? '#1C1917' : '#78716C',
                      fontSize: '0.75rem',
                      fontWeight: typography.fontWeight.medium,
                      letterSpacing: '0.1em',
                      textTransform: 'uppercase',
                      px: 2,
                      borderBottom: isActive ? '2px solid #1C1917' : '2px solid transparent',
                      borderRadius: 0,
                      '&:hover': { color: '#1C1917', bgcolor: 'transparent' },
                    }}
                  >
                    {item.label}
                  </Button>
                )
              })}
            </Box>

            <Box
              sx={{
                flexGrow: 1,
                display: { xs: 'none', md: 'flex' },
                justifyContent: 'center',
                maxWidth: 360,
                mx: 2,
              }}
            >
              <SearchAutocomplete
                placeholder={t('search.placeholder') || 'Search auctions...'}
                onSearch={(query) => navigate(`/auctions?search=${encodeURIComponent(query)}`)}
              />
            </Box>

            <Box sx={{ flexGrow: 0, display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <IconButton
                sx={{ display: { xs: 'flex', md: 'none' }, color: '#1C1917' }}
                onClick={() => navigate('/search')}
              >
                <SearchIcon />
              </IconButton>
              <LanguageSwitcher />
              <IconButton onClick={toggleTheme} sx={{ color: '#57534E' }}>
                {mode === 'dark' ? <LightModeIcon /> : <DarkModeIcon />}
              </IconButton>

              {isAuthenticated ? (
                <>
                  <Tooltip title={t('nav.notifications') || 'Notifications'}>
                    <IconButton component={Link} to="/notifications" sx={{ color: '#57534E' }}>
                      <Badge badgeContent={notificationSummary?.unreadCount || 0} color="error">
                        <NotificationsIcon />
                      </Badge>
                    </IconButton>
                  </Tooltip>

                  <Tooltip title="Account settings">
                    <IconButton onClick={handleOpenUserMenu} sx={{ p: 0, ml: 1 }}>
                      <Avatar
                        alt={user?.displayName}
                        src={user?.avatarUrl}
                        sx={{
                          width: 32,
                          height: 32,
                          bgcolor: '#F5F5F4',
                          color: '#57534E',
                          fontFamily: typography.fontFamily.display,
                          fontSize: '0.875rem',
                        }}
                      />
                    </IconButton>
                  </Tooltip>
                  <Menu
                    sx={{ mt: '45px' }}
                    id="menu-appbar"
                    anchorEl={anchorElUser}
                    anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
                    keepMounted
                    transformOrigin={{ vertical: 'top', horizontal: 'right' }}
                    open={Boolean(anchorElUser)}
                    onClose={handleCloseUserMenu}
                    slotProps={{
                      paper: {
                        sx: {
                          bgcolor: '#FFFFFF',
                          border: '1px solid #E7E5E4',
                          boxShadow: '0 4px 16px rgba(0,0,0,0.08)',
                          borderRadius: 0,
                          minWidth: 180,
                        },
                      },
                    }}
                  >
                    <MenuItem
                      onClick={() => {
                        handleCloseUserMenu()
                        navigate('/profile')
                      }}
                    >
                      <ListItemIcon>
                        <PersonIcon fontSize="small" sx={{ color: '#57534E' }} />
                      </ListItemIcon>
                      <Typography sx={{ fontSize: '0.8125rem' }}>Profile</Typography>
                    </MenuItem>
                    <MenuItem
                      onClick={() => {
                        handleCloseUserMenu()
                        navigate('/settings')
                      }}
                    >
                      <ListItemIcon>
                        <SettingsIcon fontSize="small" sx={{ color: '#57534E' }} />
                      </ListItemIcon>
                      <Typography sx={{ fontSize: '0.8125rem' }}>Settings</Typography>
                    </MenuItem>
                    <Divider sx={{ borderColor: '#E7E5E4' }} />
                    <MenuItem onClick={handleLogout}>
                      <ListItemIcon>
                        <LogoutIcon fontSize="small" sx={{ color: '#57534E' }} />
                      </ListItemIcon>
                      <Typography sx={{ fontSize: '0.8125rem' }}>Logout</Typography>
                    </MenuItem>
                  </Menu>
                </>
              ) : (
                <>
                  <Button
                    component={Link}
                    to="/login"
                    sx={{
                      color: '#1C1917',
                      fontSize: '0.75rem',
                      fontWeight: typography.fontWeight.medium,
                      letterSpacing: '0.1em',
                      textTransform: 'uppercase',
                    }}
                  >
                    Login
                  </Button>
                  <Button
                    variant="contained"
                    component={Link}
                    to="/register"
                    sx={{
                      bgcolor: '#1C1917',
                      color: '#FFFFFF',
                      fontSize: '0.75rem',
                      fontWeight: typography.fontWeight.medium,
                      letterSpacing: '0.1em',
                      textTransform: 'uppercase',
                      borderRadius: 0,
                      boxShadow: 'none',
                      '&:hover': { bgcolor: '#292524', boxShadow: 'none' },
                    }}
                  >
                    Sign Up
                  </Button>
                </>
              )}
            </Box>
          </Toolbar>
        </Container>
      </AppBar>

      <Drawer
        variant="temporary"
        open={mobileOpen}
        onClose={handleDrawerToggle}
        ModalProps={{ keepMounted: true }}
        sx={{
          display: { xs: 'block', sm: 'none' },
          '& .MuiDrawer-paper': {
            boxSizing: 'border-box',
            width: 260,
            bgcolor: '#FFFFFF',
            borderRight: '1px solid #E7E5E4',
          },
        }}
      >
        {drawer}
      </Drawer>

      <Box component="main" sx={{ flexGrow: 1, bgcolor: '#FFFFFF' }}>
        <Outlet />
      </Box>

      <Box
        component="footer"
        sx={{
          py: 3,
          px: 2,
          mt: 'auto',
          bgcolor: '#FAFAF9',
          borderTop: '1px solid #E7E5E4',
        }}
      >
        <Container maxWidth="xl">
          <Typography
            align="center"
            sx={{
              color: '#A8A29E',
              fontSize: '0.75rem',
              letterSpacing: '0.05em',
            }}
          >
            © {new Date().getFullYear()} The Auction. All rights reserved.
          </Typography>
        </Container>
      </Box>
    </Box>
  )
}
