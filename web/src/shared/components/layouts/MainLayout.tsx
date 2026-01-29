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
  Gavel as GavelIcon,
  Notifications as NotificationsIcon,
  DarkMode as DarkModeIcon,
  LightMode as LightModeIcon,
  Home as HomeIcon,
  Category as CategoryIcon,
  Person as PersonIcon,
  Settings as SettingsIcon,
  Logout as LogoutIcon,
  Search as SearchIcon,
} from '@mui/icons-material'
import { useAuth, useThemeMode } from '@/app/providers'
import { useNotificationSummary } from '@/modules/notifications/hooks'
import { LanguageSwitcher } from '../inputs'
import { SearchAutocomplete } from '@/modules/search/components/SearchAutocomplete'

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
      <Typography variant="h6" sx={{ my: 2 }}>
        Auction Platform
      </Typography>
      <Divider />
      <List>
        {navItems.map((item) => (
          <ListItem key={item.path} disablePadding>
            <ListItemButton
              component={Link}
              to={item.path}
              selected={location.pathname === item.path}
            >
              <ListItemIcon>{item.icon}</ListItemIcon>
              <ListItemText primary={item.label} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>
    </Box>
  )

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <AppBar position="sticky" elevation={1}>
        <Container maxWidth="xl">
          <Toolbar disableGutters>
            <IconButton
              color="inherit"
              aria-label="open drawer"
              edge="start"
              onClick={handleDrawerToggle}
              sx={{ mr: 2, display: { sm: 'none' } }}
            >
              <MenuIcon />
            </IconButton>

            <GavelIcon sx={{ display: { xs: 'none', sm: 'flex' }, mr: 1 }} />
            <Typography
              variant="h6"
              noWrap
              component={Link}
              to="/"
              sx={{
                mr: 2,
                display: { xs: 'none', sm: 'flex' },
                fontWeight: 700,
                color: 'inherit',
                textDecoration: 'none',
              }}
            >
              Auction
            </Typography>

            <Box sx={{ flexGrow: 1, display: { xs: 'none', sm: 'flex' }, gap: 1 }}>
              {navItems.map((item) => (
                <Button
                  key={item.path}
                  component={Link}
                  to={item.path}
                  sx={{
                    color: 'white',
                    opacity: location.pathname === item.path ? 1 : 0.8,
                  }}
                >
                  {item.label}
                </Button>
              ))}
            </Box>

            <Box
              sx={{
                flexGrow: 1,
                display: { xs: 'none', md: 'flex' },
                justifyContent: 'center',
                maxWidth: 400,
                mx: 2,
              }}
            >
              <SearchAutocomplete
                placeholder={t('search.placeholder') || 'Search auctions...'}
                onSearch={(query) => navigate(`/auctions?search=${encodeURIComponent(query)}`)}
              />
            </Box>

            <Box sx={{ flexGrow: 0, display: 'flex', alignItems: 'center', gap: 1 }}>
              <IconButton
                color="inherit"
                sx={{ display: { xs: 'flex', md: 'none' } }}
                onClick={() => navigate('/search')}
              >
                <SearchIcon />
              </IconButton>
              <LanguageSwitcher />
              <IconButton color="inherit" onClick={toggleTheme}>
                {mode === 'dark' ? <LightModeIcon /> : <DarkModeIcon />}
              </IconButton>

              {isAuthenticated ? (
                <>
                  <Tooltip title={t('nav.notifications') || 'Notifications'}>
                    <IconButton color="inherit" component={Link} to="/notifications">
                      <Badge badgeContent={notificationSummary?.unreadCount || 0} color="error">
                        <NotificationsIcon />
                      </Badge>
                    </IconButton>
                  </Tooltip>

                  <Tooltip title="Account settings">
                    <IconButton onClick={handleOpenUserMenu} sx={{ p: 0 }}>
                      <Avatar
                        alt={user?.displayName}
                        src={user?.avatarUrl}
                        sx={{ width: 32, height: 32 }}
                      />
                    </IconButton>
                  </Tooltip>
                  <Menu
                    sx={{ mt: '45px' }}
                    id="menu-appbar"
                    anchorEl={anchorElUser}
                    anchorOrigin={{
                      vertical: 'top',
                      horizontal: 'right',
                    }}
                    keepMounted
                    transformOrigin={{
                      vertical: 'top',
                      horizontal: 'right',
                    }}
                    open={Boolean(anchorElUser)}
                    onClose={handleCloseUserMenu}
                  >
                    <MenuItem
                      onClick={() => {
                        handleCloseUserMenu()
                        navigate('/profile')
                      }}
                    >
                      <ListItemIcon>
                        <PersonIcon fontSize="small" />
                      </ListItemIcon>
                      Profile
                    </MenuItem>
                    <MenuItem
                      onClick={() => {
                        handleCloseUserMenu()
                        navigate('/settings')
                      }}
                    >
                      <ListItemIcon>
                        <SettingsIcon fontSize="small" />
                      </ListItemIcon>
                      Settings
                    </MenuItem>
                    <Divider />
                    <MenuItem onClick={handleLogout}>
                      <ListItemIcon>
                        <LogoutIcon fontSize="small" />
                      </ListItemIcon>
                      Logout
                    </MenuItem>
                  </Menu>
                </>
              ) : (
                <>
                  <Button color="inherit" component={Link} to="/login">
                    Login
                  </Button>
                  <Button variant="contained" color="secondary" component={Link} to="/register">
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
        ModalProps={{
          keepMounted: true,
        }}
        sx={{
          display: { xs: 'block', sm: 'none' },
          '& .MuiDrawer-paper': { boxSizing: 'border-box', width: 240 },
        }}
      >
        {drawer}
      </Drawer>

      <Box component="main" sx={{ flexGrow: 1 }}>
        <Outlet />
      </Box>

      <Box
        component="footer"
        sx={{
          py: 3,
          px: 2,
          mt: 'auto',
          backgroundColor: (theme) =>
            theme.palette.mode === 'light' ? theme.palette.grey[100] : theme.palette.grey[900],
        }}
      >
        <Container maxWidth="xl">
          <Typography variant="body2" color="text.secondary" align="center">
            © {new Date().getFullYear()} Auction Platform. All rights reserved.
          </Typography>
        </Container>
      </Box>
    </Box>
  )
}
