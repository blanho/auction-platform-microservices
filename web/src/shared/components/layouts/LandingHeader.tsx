import { useState, useEffect } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import {
  AppBar,
  Box,
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
} from '@mui/material'
import {
  Menu as MenuIcon,
  Close as CloseIcon,
  Search as SearchIcon,
  FavoriteBorder,
  PersonOutline,
  ShoppingBagOutlined,
  NotificationsOutlined,
} from '@mui/icons-material'
import { useAuth } from '@/app/providers'
import { useNotificationSummary } from '@/modules/notifications/hooks'
import { useWatchlistCount } from '@/modules/auctions/hooks'
import { palette } from '@/shared/theme/tokens'
import { WishlistDrawer } from '../navigation/WishlistDrawer'
import { LanguageSwitcher } from '../inputs'

const categoryLinks = [
  { label: 'New Arrivals', path: '/auctions?sort=newest' },
  { label: 'Designers', path: '/categories/designers' },
  { label: 'Women', path: '/categories/women' },
  { label: 'Men', path: '/categories/men' },
  { label: 'Jewelry', path: '/categories/jewelry' },
  { label: 'Watches', path: '/categories/watches' },
  { label: 'Art', path: '/categories/art' },
  { label: 'Home', path: '/categories/home' },
  { label: 'Sale', path: '/auctions?sale=true' },
]

export const LandingHeader = () => {
  const { user, isAuthenticated, logout } = useAuth()
  const { data: notificationSummary } = useNotificationSummary()
  const { data: watchlistCount } = useWatchlistCount()
  const navigate = useNavigate()
  const location = useLocation()
  const [isScrolled, setIsScrolled] = useState(false)
  const [mobileOpen, setMobileOpen] = useState(false)
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const [searchValue, setSearchValue] = useState('')
  const [wishlistOpen, setWishlistOpen] = useState(false)

  const handleWishlistOpen = () => {
    if (isAuthenticated) {
      setWishlistOpen(true)
    } else {
      navigate('/login')
    }
  }

  const handleWishlistClose = () => {
    setWishlistOpen(false)
  }

  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 100)
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

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    if (searchValue.trim()) {
      navigate(`/auctions?search=${encodeURIComponent(searchValue.trim())}`)
    }
  }

  const isLandingPage = location.pathname === '/'

  const headerBg = isScrolled || !isLandingPage ? palette.neutral[0] : 'transparent'

  const drawer = (
    <Box sx={{ width: '100vw', height: '100vh', bgcolor: palette.neutral[0] }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', p: 3 }}>
        <Typography
          sx={{
            color: palette.neutral[900],
            fontFamily: '"Playfair Display", serif',
            fontWeight: 500,
            fontSize: '1.5rem',
            fontStyle: 'italic',
          }}
        >
          TheAuction
        </Typography>
        <IconButton onClick={() => setMobileOpen(false)} sx={{ color: palette.neutral[900] }}>
          <CloseIcon />
        </IconButton>
      </Box>
      <Divider sx={{ borderColor: palette.neutral[200] }} />

      <Box sx={{ px: 3, py: 2 }}>
        <Box
          component="form"
          onSubmit={handleSearch}
          sx={{
            display: 'flex',
            alignItems: 'center',
            border: `1px solid ${palette.neutral[300]}`,
            px: 2,
            py: 1,
          }}
        >
          <InputBase
            placeholder="Search for designers or items"
            value={searchValue}
            onChange={(e) => setSearchValue(e.target.value)}
            sx={{ flex: 1, fontSize: '0.875rem' }}
          />
          <SearchIcon sx={{ color: palette.neutral[500], fontSize: 20 }} />
        </Box>
      </Box>

      <List sx={{ pt: 1 }}>
        {categoryLinks.map((item) => (
          <ListItem key={item.path} disablePadding>
            <ListItemButton
              component={Link}
              to={item.path}
              onClick={() => setMobileOpen(false)}
              sx={{
                py: 1.5,
                px: 3,
                '&:hover': { bgcolor: palette.neutral[50] },
              }}
            >
              <ListItemText
                primary={item.label}
                primaryTypographyProps={{
                  sx: {
                    color: palette.neutral[900],
                    fontSize: '0.9375rem',
                    fontWeight: 400,
                  },
                }}
              />
            </ListItemButton>
          </ListItem>
        ))}
      </List>

      <Divider sx={{ borderColor: palette.neutral[200], my: 2 }} />

      <Box sx={{ px: 3 }}>
        <Button
          fullWidth
          variant="contained"
          component={Link}
          to="/sell"
          onClick={() => setMobileOpen(false)}
          sx={{
            bgcolor: palette.neutral[900],
            color: palette.neutral[0],
            py: 1.5,
            textTransform: 'uppercase',
            letterSpacing: '0.15em',
            fontSize: '0.75rem',
            fontWeight: 500,
            borderRadius: 0,
            '&:hover': { bgcolor: palette.neutral[800] },
          }}
        >
          Sell With Us
        </Button>
      </Box>

      <Box sx={{ position: 'absolute', bottom: 0, left: 0, right: 0, p: 3 }}>
        {isAuthenticated ? (
          <Button
            fullWidth
            variant="outlined"
            onClick={handleLogout}
            sx={{
              borderColor: palette.neutral[900],
              color: palette.neutral[900],
              py: 1.5,
              textTransform: 'uppercase',
              letterSpacing: '0.1em',
              fontSize: '0.75rem',
              borderRadius: 0,
              '&:hover': { bgcolor: palette.neutral[900], color: palette.neutral[0] },
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
                bgcolor: palette.neutral[900],
                color: palette.neutral[0],
                py: 1.5,
                textTransform: 'uppercase',
                letterSpacing: '0.1em',
                fontSize: '0.75rem',
                borderRadius: 0,
                '&:hover': { bgcolor: palette.neutral[800] },
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
                borderColor: palette.neutral[900],
                color: palette.neutral[900],
                py: 1.5,
                textTransform: 'uppercase',
                letterSpacing: '0.1em',
                fontSize: '0.75rem',
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

  const showSolidHeader = isScrolled || !isLandingPage

  const mainHeaderHeight = { xs: 56, md: 64 }
  const categoryNavHeight = { xs: 0, md: 44 }
  const greetingHeight = isAuthenticated ? { xs: 0, md: 32 } : { xs: 0, md: 0 }

  return (
    <>
      <AppBar
        position="fixed"
        elevation={0}
        sx={{
          bgcolor: headerBg,
          transition: 'all 0.3s ease',
          top: 0,
        }}
      >
        {isAuthenticated && showSolidHeader && (
          <Box
            sx={{
              bgcolor: palette.neutral[0],
              borderBottom: `1px solid ${palette.neutral[100]}`,
              py: 0.75,
              display: { xs: 'none', md: 'block' },
            }}
          >
            <Container maxWidth="xl">
              <Typography
                sx={{
                  fontSize: '0.8125rem',
                  color: palette.neutral[900],
                }}
              >
                Hi {user?.displayName || user?.email?.split('@')[0]},{' '}
                <Typography
                  component={Link}
                  to="/auctions?sort=newest"
                  sx={{
                    fontSize: '0.8125rem',
                    color: palette.neutral[900],
                    textDecoration: 'underline',
                    textUnderlineOffset: 2,
                    '&:hover': { color: palette.neutral[600] },
                  }}
                >
                  shop new items 24 hours in advance
                </Typography>
                !
              </Typography>
            </Container>
          </Box>
        )}

        <Box
          sx={{
            bgcolor: showSolidHeader ? palette.neutral[0] : 'transparent',
            borderBottom: showSolidHeader ? `1px solid ${palette.neutral[200]}` : 'none',
            py: { xs: 1.5, md: 2 },
          }}
        >
          <Container maxWidth="xl">
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                gap: { xs: 2, md: 4 },
              }}
            >
              <IconButton
                onClick={() => setMobileOpen(true)}
                sx={{
                  display: { md: 'none' },
                  color: showSolidHeader ? palette.neutral[900] : palette.neutral[0],
                }}
              >
                <MenuIcon />
              </IconButton>

              <Box
                component={Link}
                to="/"
                sx={{
                  textDecoration: 'none',
                  flexShrink: 0,
                }}
              >
                <Typography
                  sx={{
                    color: showSolidHeader ? palette.neutral[900] : palette.neutral[0],
                    fontFamily: '"Playfair Display", serif',
                    fontWeight: 500,
                    fontSize: { xs: '1.25rem', md: '1.75rem' },
                    fontStyle: 'italic',
                  }}
                >
                  TheAuction
                </Typography>
              </Box>

              <Box
                component="form"
                onSubmit={handleSearch}
                sx={{
                  flex: 1,
                  maxWidth: 500,
                  display: { xs: 'none', md: 'flex' },
                  alignItems: 'center',
                  border: `1px solid ${showSolidHeader ? palette.neutral[300] : 'rgba(255,255,255,0.3)'}`,
                  bgcolor: showSolidHeader ? palette.neutral[0] : 'rgba(255,255,255,0.1)',
                  px: 2,
                  py: 1,
                  mx: 'auto',
                  transition: 'all 0.2s ease',
                  '&:hover': {
                    borderColor: showSolidHeader ? palette.neutral[400] : 'rgba(255,255,255,0.5)',
                  },
                  '&:focus-within': {
                    borderColor: palette.neutral[900],
                  },
                }}
              >
                <InputBase
                  placeholder="Search for designers or items"
                  value={searchValue}
                  onChange={(e) => setSearchValue(e.target.value)}
                  sx={{
                    flex: 1,
                    fontSize: '0.875rem',
                    color: showSolidHeader ? palette.neutral[900] : palette.neutral[0],
                    '& input::placeholder': {
                      color: showSolidHeader ? palette.neutral[500] : 'rgba(255,255,255,0.6)',
                      opacity: 1,
                    },
                  }}
                />
                <IconButton type="submit" sx={{ p: 0.5 }}>
                  <SearchIcon
                    sx={{
                      color: showSolidHeader ? palette.neutral[900] : palette.neutral[0],
                      fontSize: 24,
                    }}
                  />
                </IconButton>
              </Box>

              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, ml: 'auto' }}>
                <Button
                  component={Link}
                  to="/sell"
                  variant="contained"
                  sx={{
                    display: { xs: 'none', lg: 'flex' },
                    bgcolor: palette.neutral[900],
                    color: palette.neutral[0],
                    textTransform: 'uppercase',
                    fontSize: '0.6875rem',
                    fontWeight: 500,
                    letterSpacing: '0.15em',
                    px: 3,
                    py: 1.25,
                    height: 40,
                    borderRadius: 0,
                    boxShadow: 'none',
                    whiteSpace: 'nowrap',
                    '&:hover': {
                      bgcolor: palette.neutral[800],
                      boxShadow: 'none',
                    },
                  }}
                >
                  Sell With Us
                </Button>

                <Box
                  sx={{
                    display: { xs: 'none', sm: 'flex' },
                    alignItems: 'center',
                    gap: 1,
                    color: showSolidHeader ? palette.neutral[900] : palette.neutral[0],
                  }}
                >
                  <LanguageSwitcher />

                  {isAuthenticated && (
                    <IconButton
                      component={Link}
                      to="/notifications"
                      sx={{
                        color: showSolidHeader ? palette.neutral[900] : palette.neutral[0],
                        p: 1,
                      }}
                    >
                      <Badge
                        badgeContent={notificationSummary?.unreadCount || 0}
                        color="error"
                        sx={{
                          '& .MuiBadge-badge': {
                            fontSize: '0.625rem',
                            minWidth: 16,
                            height: 16,
                          },
                        }}
                      >
                        <NotificationsOutlined sx={{ fontSize: 24 }} />
                      </Badge>
                    </IconButton>
                  )}
                </Box>

                {isAuthenticated ? (
                  <>
                    <IconButton
                      onClick={handleUserMenuOpen}
                      sx={{
                        color: showSolidHeader ? palette.neutral[900] : palette.neutral[0],
                        p: 1,
                      }}
                    >
                      {user?.avatarUrl ? (
                        <Avatar
                          src={user.avatarUrl}
                          sx={{
                            width: 32,
                            height: 32,
                            border: `2px solid ${showSolidHeader ? palette.neutral[300] : 'rgba(255,255,255,0.4)'}`,
                          }}
                        />
                      ) : (
                        <PersonOutline sx={{ fontSize: 24 }} />
                      )}
                    </IconButton>
                    <Menu
                      anchorEl={anchorEl}
                      open={Boolean(anchorEl)}
                      onClose={handleUserMenuClose}
                      anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
                      transformOrigin={{ vertical: 'top', horizontal: 'right' }}
                      slotProps={{
                        paper: {
                          sx: {
                            bgcolor: palette.neutral[0],
                            color: palette.neutral[900],
                            border: `1px solid ${palette.neutral[200]}`,
                            borderRadius: 0,
                            mt: 1,
                            minWidth: 200,
                            boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
                          },
                        },
                      }}
                    >
                      <MenuItem
                        component={Link}
                        to="/profile"
                        onClick={handleUserMenuClose}
                        sx={{
                          py: 1.5,
                          fontSize: '0.875rem',
                          '&:hover': { bgcolor: palette.neutral[50] },
                        }}
                      >
                        My Profile
                      </MenuItem>
                      <MenuItem
                        component={Link}
                        to="/my-bids"
                        onClick={handleUserMenuClose}
                        sx={{
                          py: 1.5,
                          fontSize: '0.875rem',
                          '&:hover': { bgcolor: palette.neutral[50] },
                        }}
                      >
                        My Bids
                      </MenuItem>
                      <MenuItem
                        component={Link}
                        to="/my-auctions"
                        onClick={handleUserMenuClose}
                        sx={{
                          py: 1.5,
                          fontSize: '0.875rem',
                          '&:hover': { bgcolor: palette.neutral[50] },
                        }}
                      >
                        My Auctions
                      </MenuItem>
                      <MenuItem
                        component={Link}
                        to="/wallet"
                        onClick={handleUserMenuClose}
                        sx={{
                          py: 1.5,
                          fontSize: '0.875rem',
                          '&:hover': { bgcolor: palette.neutral[50] },
                        }}
                      >
                        Wallet
                      </MenuItem>
                      <Divider sx={{ borderColor: palette.neutral[200] }} />
                      <MenuItem
                        component={Link}
                        to="/settings"
                        onClick={handleUserMenuClose}
                        sx={{
                          py: 1.5,
                          fontSize: '0.875rem',
                          '&:hover': { bgcolor: palette.neutral[50] },
                        }}
                      >
                        Settings
                      </MenuItem>
                      <MenuItem
                        onClick={handleLogout}
                        sx={{
                          py: 1.5,
                          fontSize: '0.875rem',
                          '&:hover': { bgcolor: palette.neutral[50] },
                        }}
                      >
                        Sign Out
                      </MenuItem>
                    </Menu>
                  </>
                ) : (
                  <IconButton
                    component={Link}
                    to="/login"
                    sx={{
                      color: showSolidHeader ? palette.neutral[900] : palette.neutral[0],
                      p: 1,
                    }}
                  >
                    <PersonOutline sx={{ fontSize: 24 }} />
                  </IconButton>
                )}

                <IconButton
                  onClick={handleWishlistOpen}
                  sx={{
                    color: showSolidHeader ? palette.neutral[900] : palette.neutral[0],
                    p: 1,
                  }}
                >
                  <Badge
                    badgeContent={watchlistCount || 0}
                    color="error"
                    sx={{
                      '& .MuiBadge-badge': {
                        fontSize: '0.625rem',
                        minWidth: 16,
                        height: 16,
                      },
                    }}
                  >
                    <FavoriteBorder sx={{ fontSize: 24 }} />
                  </Badge>
                </IconButton>

                <IconButton
                  component={Link}
                  to={isAuthenticated ? '/my-bids' : '/login'}
                  sx={{
                    color: showSolidHeader ? palette.neutral[900] : palette.neutral[0],
                    p: 1,
                  }}
                >
                  <ShoppingBagOutlined sx={{ fontSize: 24 }} />
                </IconButton>
              </Box>
            </Box>
          </Container>
        </Box>

        <Box
          sx={{
            bgcolor: showSolidHeader ? palette.neutral[0] : 'transparent',
            borderBottom: showSolidHeader ? `1px solid ${palette.neutral[200]}` : 'none',
            display: { xs: 'none', md: 'block' },
            py: 1.5,
          }}
        >
          <Container maxWidth="xl">
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                gap: { md: 3, lg: 4 },
              }}
            >
              {categoryLinks.map((item) => (
                <Typography
                  key={item.path}
                  component={Link}
                  to={item.path}
                  sx={{
                    color: showSolidHeader ? palette.neutral[900] : palette.neutral[0],
                    fontSize: '0.875rem',
                    fontWeight: 400,
                    textDecoration: 'none',
                    whiteSpace: 'nowrap',
                    transition: 'opacity 0.2s ease',
                    '&:hover': {
                      opacity: 0.7,
                    },
                  }}
                >
                  {item.label}
                </Typography>
              ))}
            </Box>
          </Container>
        </Box>
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

      <WishlistDrawer open={wishlistOpen} onClose={handleWishlistClose} />

      {!isLandingPage && (
        <Box
          sx={{
            height: {
              xs: mainHeaderHeight.xs,
              md: mainHeaderHeight.md + categoryNavHeight.md + greetingHeight.md,
            },
          }}
        />
      )}
    </>
  )
}
