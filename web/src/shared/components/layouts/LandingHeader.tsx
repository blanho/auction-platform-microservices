import { useState, useEffect } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import {
  AppBar,
  Box,
  Toolbar,
  IconButton,
  Typography,
  Button,
  Container,
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  Divider,
  Avatar,
  Menu,
  MenuItem,
  Badge,
  InputBase,
  Collapse,
} from '@mui/material'
import {
  Menu as MenuIcon,
  Close as CloseIcon,
  Search as SearchIcon,
  Gavel,
  NotificationsOutlined,
  FavoriteBorder,
} from '@mui/icons-material'
import { useAuth } from '@/app/providers'
import { useNotificationSummary } from '@/modules/notifications/hooks'
import { palette } from '@/shared/theme/tokens'

const navLinks = [
  { label: 'Auctions', path: '/auctions' },
  { label: 'Categories', path: '/categories' },
  { label: 'How It Works', path: '/how-it-works' },
  { label: 'Sell', path: '/sell' },
]

export const LandingHeader = () => {
  const { user, isAuthenticated, logout } = useAuth()
  const { data: notificationSummary } = useNotificationSummary()
  const navigate = useNavigate()
  const location = useLocation()
  const [isScrolled, setIsScrolled] = useState(false)
  const [mobileOpen, setMobileOpen] = useState(false)
  const [searchOpen, setSearchOpen] = useState(false)
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)

  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 50)
    }
    window.addEventListener('scroll', handleScroll)
    return () => window.removeEventListener('scroll', handleScroll)
  }, [])

  const handleUserMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleUserMenuClose = () => {
    setAnchorEl(null)
  }

  const handleLogout = async () => {
    handleUserMenuClose()
    await logout()
    navigate('/login')
  }

  const isLandingPage = location.pathname === '/'

  const headerBg = isLandingPage
    ? isScrolled
      ? 'rgba(28, 25, 23, 0.98)'
      : 'transparent'
    : palette.neutral[900]

  const drawer = (
    <Box sx={{ width: '100vw', height: '100vh', bgcolor: palette.neutral[900] }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', p: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Gavel sx={{ color: palette.brand.primary, fontSize: 28 }} />
          <Typography variant="h6" sx={{ color: palette.neutral[50], fontWeight: 600, letterSpacing: 1 }}>
            AUCTION
          </Typography>
        </Box>
        <IconButton onClick={() => setMobileOpen(false)} sx={{ color: palette.neutral[50] }}>
          <CloseIcon />
        </IconButton>
      </Box>
      <Divider sx={{ borderColor: 'rgba(250,250,249,0.1)' }} />
      <List sx={{ pt: 4 }}>
        {navLinks.map((item) => (
          <ListItem key={item.path} disablePadding>
            <ListItemButton
              component={Link}
              to={item.path}
              onClick={() => setMobileOpen(false)}
              sx={{
                py: 2,
                px: 4,
                '&:hover': { bgcolor: 'rgba(250,250,249,0.05)' },
              }}
            >
              <ListItemText
                primary={item.label}
                primaryTypographyProps={{
                  sx: {
                    color: palette.neutral[50],
                    fontSize: '1.25rem',
                    fontWeight: 400,
                  },
                }}
              />
            </ListItemButton>
          </ListItem>
        ))}
      </List>
      <Box sx={{ position: 'absolute', bottom: 0, left: 0, right: 0, p: 4 }}>
        {isAuthenticated ? (
          <Button
            fullWidth
            variant="outlined"
            onClick={handleLogout}
            sx={{
              borderColor: 'rgba(250,250,249,0.3)',
              color: palette.neutral[50],
              py: 1.5,
              textTransform: 'none',
              borderRadius: 0,
            }}
          >
            Sign Out
          </Button>
        ) : (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <Button
              fullWidth
              variant="contained"
              component={Link}
              to="/register"
              onClick={() => setMobileOpen(false)}
              sx={{
                bgcolor: palette.brand.primary,
                color: palette.neutral[50],
                py: 1.5,
                textTransform: 'none',
                borderRadius: 0,
                '&:hover': { bgcolor: palette.brand.secondary },
              }}
            >
              Create Account
            </Button>
            <Button
              fullWidth
              variant="outlined"
              component={Link}
              to="/login"
              onClick={() => setMobileOpen(false)}
              sx={{
                borderColor: 'rgba(250,250,249,0.3)',
                color: palette.neutral[50],
                py: 1.5,
                textTransform: 'none',
                borderRadius: 0,
              }}
            >
              Sign In
            </Button>
          </Box>
        )}
      </Box>
    </Box>
  )

  return (
    <>
      <AppBar
        position="fixed"
        elevation={0}
        sx={{
          bgcolor: headerBg,
          borderBottom: isScrolled || !isLandingPage ? '1px solid rgba(250,250,249,0.1)' : 'none',
          transition: 'all 0.3s ease',
          backdropFilter: isScrolled ? 'blur(12px)' : 'none',
        }}
      >
        <Container maxWidth="xl">
          <Toolbar disableGutters sx={{ height: 72 }}>
            <IconButton
              onClick={() => setMobileOpen(true)}
              sx={{ display: { md: 'none' }, color: palette.neutral[50], mr: 1 }}
            >
              <MenuIcon />
            </IconButton>

            <Box
              component={Link}
              to="/"
              sx={{
                display: 'flex',
                alignItems: 'center',
                gap: 1,
                textDecoration: 'none',
                mr: { xs: 'auto', md: 4 },
              }}
            >
              <Gavel sx={{ color: palette.brand.primary, fontSize: 28 }} />
              <Typography
                variant="h6"
                sx={{
                  color: palette.neutral[50],
                  fontWeight: 600,
                  letterSpacing: 1,
                  display: { xs: 'none', sm: 'block' },
                }}
              >
                AUCTION
              </Typography>
            </Box>

            <Box sx={{ display: { xs: 'none', md: 'flex' }, gap: 1, flex: 1 }}>
              {navLinks.map((item) => (
                <Button
                  key={item.path}
                  component={Link}
                  to={item.path}
                  sx={{
                    color: palette.neutral[50],
                    textTransform: 'none',
                    fontWeight: 500,
                    px: 2,
                    opacity: location.pathname === item.path ? 1 : 0.8,
                    '&:hover': {
                      bgcolor: 'transparent',
                      opacity: 1,
                    },
                  }}
                >
                  {item.label}
                </Button>
              ))}
            </Box>

            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Collapse in={searchOpen} orientation="horizontal">
                <InputBase
                  placeholder="Search auctions..."
                  sx={{
                    color: palette.neutral[50],
                    bgcolor: 'rgba(250,250,249,0.1)',
                    px: 2,
                    py: 0.5,
                    borderRadius: 1,
                    width: 200,
                    '& input::placeholder': { color: 'rgba(250,250,249,0.5)' },
                  }}
                  onBlur={() => setSearchOpen(false)}
                  autoFocus={searchOpen}
                />
              </Collapse>

              <IconButton
                onClick={() => setSearchOpen(!searchOpen)}
                sx={{ color: palette.neutral[50], display: { xs: 'none', sm: 'flex' } }}
              >
                <SearchIcon />
              </IconButton>

              {isAuthenticated && (
                <>
                  <IconButton
                    component={Link}
                    to="/favorites"
                    sx={{ color: palette.neutral[50], display: { xs: 'none', sm: 'flex' } }}
                  >
                    <FavoriteBorder />
                  </IconButton>
                  <IconButton
                    component={Link}
                    to="/notifications"
                    sx={{ color: palette.neutral[50], display: { xs: 'none', sm: 'flex' } }}
                  >
                    <Badge badgeContent={notificationSummary?.unreadCount || 0} color="error">
                      <NotificationsOutlined />
                    </Badge>
                  </IconButton>
                </>
              )}

              {isAuthenticated ? (
                <>
                  <IconButton onClick={handleUserMenuOpen} sx={{ ml: 1 }}>
                    <Avatar
                      src={user?.avatarUrl}
                      sx={{
                        width: 36,
                        height: 36,
                        bgcolor: palette.brand.primary,
                        fontSize: '0.9rem',
                      }}
                    >
                      {user?.displayName?.[0] || user?.email?.[0]?.toUpperCase()}
                    </Avatar>
                  </IconButton>
                  <Menu
                    anchorEl={anchorEl}
                    open={Boolean(anchorEl)}
                    onClose={handleUserMenuClose}
                    anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
                    transformOrigin={{ vertical: 'top', horizontal: 'right' }}
                    PaperProps={{
                      sx: {
                        bgcolor: palette.neutral[900],
                        color: palette.neutral[50],
                        border: '1px solid rgba(250,250,249,0.1)',
                        mt: 1,
                        minWidth: 200,
                      },
                    }}
                  >
                    <MenuItem
                      component={Link}
                      to="/profile"
                      onClick={handleUserMenuClose}
                      sx={{ py: 1.5, '&:hover': { bgcolor: 'rgba(250,250,249,0.05)' } }}
                    >
                      My Profile
                    </MenuItem>
                    <MenuItem
                      component={Link}
                      to="/my-bids"
                      onClick={handleUserMenuClose}
                      sx={{ py: 1.5, '&:hover': { bgcolor: 'rgba(250,250,249,0.05)' } }}
                    >
                      My Bids
                    </MenuItem>
                    <MenuItem
                      component={Link}
                      to="/my-auctions"
                      onClick={handleUserMenuClose}
                      sx={{ py: 1.5, '&:hover': { bgcolor: 'rgba(250,250,249,0.05)' } }}
                    >
                      My Auctions
                    </MenuItem>
                    <MenuItem
                      component={Link}
                      to="/wallet"
                      onClick={handleUserMenuClose}
                      sx={{ py: 1.5, '&:hover': { bgcolor: 'rgba(250,250,249,0.05)' } }}
                    >
                      Wallet
                    </MenuItem>
                    <Divider sx={{ borderColor: 'rgba(250,250,249,0.1)' }} />
                    <MenuItem
                      component={Link}
                      to="/settings"
                      onClick={handleUserMenuClose}
                      sx={{ py: 1.5, '&:hover': { bgcolor: 'rgba(250,250,249,0.05)' } }}
                    >
                      Settings
                    </MenuItem>
                    <MenuItem
                      onClick={handleLogout}
                      sx={{ py: 1.5, '&:hover': { bgcolor: 'rgba(250,250,249,0.05)' } }}
                    >
                      Sign Out
                    </MenuItem>
                  </Menu>
                </>
              ) : (
                <Box sx={{ display: { xs: 'none', md: 'flex' }, gap: 1, ml: 2 }}>
                  <Button
                    component={Link}
                    to="/login"
                    sx={{
                      color: palette.neutral[50],
                      textTransform: 'none',
                      fontWeight: 500,
                    }}
                  >
                    Sign In
                  </Button>
                  <Button
                    component={Link}
                    to="/register"
                    variant="contained"
                    sx={{
                      bgcolor: palette.brand.primary,
                      color: palette.neutral[50],
                      textTransform: 'none',
                      fontWeight: 500,
                      borderRadius: 0,
                      px: 3,
                      '&:hover': { bgcolor: palette.brand.secondary },
                    }}
                  >
                    Create Account
                  </Button>
                </Box>
              )}
            </Box>
          </Toolbar>
        </Container>
      </AppBar>

      <Drawer
        anchor="left"
        open={mobileOpen}
        onClose={() => setMobileOpen(false)}
        PaperProps={{
          sx: { bgcolor: 'transparent', boxShadow: 'none' },
        }}
      >
        {drawer}
      </Drawer>

      <Toolbar sx={{ height: isLandingPage ? 0 : 72 }} />
    </>
  )
}
