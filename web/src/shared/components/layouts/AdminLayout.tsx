import { useState, type ReactNode } from 'react'
import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { motion, AnimatePresence } from 'framer-motion'
import {
  Box,
  Drawer,
  AppBar,
  Toolbar,
  Typography,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  IconButton,
  Avatar,
  Menu,
  MenuItem,
  Divider,
  Badge,
  useTheme,
  useMediaQuery,
} from '@mui/material'
import {
  Dashboard,
  People,
  Gavel,
  Category,
  Settings,
  BarChart,
  Menu as MenuIcon,
  Notifications,
  Store,
  Flag,
  Receipt,
  AdminPanelSettings,
  Logout,
} from '@mui/icons-material'
import { useAuth } from '@/app/hooks/useAuth'
import { palette } from '@/shared/theme/tokens'

const DRAWER_WIDTH = 280

interface NavItem {
  path: string
  label: string
  icon: ReactNode
  children?: NavItem[]
}

const navItems: NavItem[] = [
  { path: '/admin', label: 'Dashboard', icon: <Dashboard /> },
  { path: '/admin/users', label: 'Users', icon: <People /> },
  { path: '/admin/auctions', label: 'Auctions', icon: <Gavel /> },
  { path: '/admin/categories', label: 'Categories', icon: <Category /> },
  { path: '/admin/brands', label: 'Brands', icon: <Store /> },
  { path: '/admin/orders', label: 'Orders', icon: <Receipt /> },
  { path: '/admin/reports', label: 'Reports', icon: <Flag /> },
  { path: '/admin/analytics', label: 'Analytics', icon: <BarChart /> },
  { path: '/admin/settings', label: 'Settings', icon: <Settings /> },
]

export function AdminLayout() {
  const theme = useTheme()
  const isMobile = useMediaQuery(theme.breakpoints.down('md'))
  const [mobileOpen, setMobileOpen] = useState(false)
  const [userMenuAnchor, setUserMenuAnchor] = useState<null | HTMLElement>(null)
  const location = useLocation()
  const navigate = useNavigate()
  const { user, logout } = useAuth()

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen)
  }

  const handleLogout = () => {
    setUserMenuAnchor(null)
    logout()
    navigate('/login')
  }

  const drawer = (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <Box
        sx={{
          p: 3,
          display: 'flex',
          alignItems: 'center',
          gap: 2,
          borderBottom: 1,
          borderColor: 'divider',
        }}
      >
        <AdminPanelSettings sx={{ fontSize: 32, color: palette.brand.primary }} />
        <Typography
          variant="h6"
          sx={{
            fontFamily: '"Playfair Display", serif',
            fontWeight: 700,
            color: 'primary.main',
          }}
        >
          Admin Panel
        </Typography>
      </Box>

      <List component="nav" sx={{ flex: 1, px: 2, py: 1 }}>
        {navItems.map((item) => {
          const isActive = location.pathname === item.path || 
            (item.path !== '/admin' && location.pathname.startsWith(item.path))
          
          return (
            <ListItem key={item.path} disablePadding sx={{ mb: 0.5 }}>
              <ListItemButton
                component={Link}
                to={item.path}
                selected={isActive}
                onClick={() => isMobile && setMobileOpen(false)}
                sx={{
                  borderRadius: 2,
                  '&.Mui-selected': {
                    bgcolor: palette.brand.muted,
                    color: palette.brand.primary,
                    '& .MuiListItemIcon-root': {
                      color: palette.brand.primary,
                    },
                    '&:hover': {
                      bgcolor: palette.brand.muted,
                    },
                  },
                }}
              >
                <ListItemIcon sx={{ minWidth: 44 }}>
                  {item.icon}
                </ListItemIcon>
                <ListItemText primary={item.label} />
              </ListItemButton>
            </ListItem>
          )
        })}
      </List>

      <Divider />

      <Box sx={{ p: 2 }}>
        <ListItemButton
          component={Link}
          to="/"
          sx={{ borderRadius: 2 }}
        >
          <ListItemIcon sx={{ minWidth: 44 }}>
            <Gavel />
          </ListItemIcon>
          <ListItemText primary="Back to Site" />
        </ListItemButton>
      </Box>
    </Box>
  )

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <AppBar
        position="fixed"
        elevation={0}
        sx={{
          width: { md: `calc(100% - ${DRAWER_WIDTH}px)` },
          ml: { md: `${DRAWER_WIDTH}px` },
          bgcolor: 'background.paper',
          borderBottom: 1,
          borderColor: 'divider',
        }}
      >
        <Toolbar>
          <IconButton
            color="inherit"
            edge="start"
            onClick={handleDrawerToggle}
            sx={{ mr: 2, display: { md: 'none' }, color: 'text.primary' }}
          >
            <MenuIcon />
          </IconButton>

          <Box sx={{ flex: 1 }} />

          <IconButton sx={{ mr: 1 }}>
            <Badge badgeContent={4} color="error">
              <Notifications />
            </Badge>
          </IconButton>

          <IconButton onClick={(e) => setUserMenuAnchor(e.currentTarget)}>
            <Avatar
              src={user?.avatarUrl}
              sx={{ width: 36, height: 36, bgcolor: 'primary.main' }}
            >
              {user?.displayName?.[0]}
            </Avatar>
          </IconButton>

          <Menu
            anchorEl={userMenuAnchor}
            open={Boolean(userMenuAnchor)}
            onClose={() => setUserMenuAnchor(null)}
            anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
            transformOrigin={{ vertical: 'top', horizontal: 'right' }}
          >
            <MenuItem onClick={() => { setUserMenuAnchor(null); navigate('/profile') }}>
              Profile
            </MenuItem>
            <MenuItem onClick={() => { setUserMenuAnchor(null); navigate('/settings') }}>
              Settings
            </MenuItem>
            <Divider />
            <MenuItem onClick={handleLogout}>
              <Logout sx={{ mr: 1, fontSize: 18 }} />
              Logout
            </MenuItem>
          </Menu>
        </Toolbar>
      </AppBar>

      <Box
        component="nav"
        sx={{ width: { md: DRAWER_WIDTH }, flexShrink: { md: 0 } }}
      >
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={handleDrawerToggle}
          ModalProps={{ keepMounted: true }}
          sx={{
            display: { xs: 'block', md: 'none' },
            '& .MuiDrawer-paper': {
              boxSizing: 'border-box',
              width: DRAWER_WIDTH,
            },
          }}
        >
          {drawer}
        </Drawer>

        <Drawer
          variant="permanent"
          sx={{
            display: { xs: 'none', md: 'block' },
            '& .MuiDrawer-paper': {
              boxSizing: 'border-box',
              width: DRAWER_WIDTH,
              borderRight: 1,
              borderColor: 'divider',
            },
          }}
          open
        >
          {drawer}
        </Drawer>
      </Box>

      <Box
        component="main"
        sx={{
          flexGrow: 1,
          width: { md: `calc(100% - ${DRAWER_WIDTH}px)` },
          minHeight: '100vh',
          bgcolor: 'grey.50',
        }}
      >
        <Toolbar />
        <AnimatePresence mode="wait">
          <motion.div
            key={location.pathname}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -20 }}
            transition={{ duration: 0.2 }}
          >
            <Outlet />
          </motion.div>
        </AnimatePresence>
      </Box>
    </Box>
  )
}
