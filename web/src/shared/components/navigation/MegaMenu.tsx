import { useState, useRef, useEffect } from 'react'
import { Link, useLocation } from 'react-router-dom'
import {
  Box,
  Container,
  Typography,
  Button,
  Popper,
  Paper,
  Grow,
  Grid,
  ClickAwayListener,
} from '@mui/material'
import { KeyboardArrowDown } from '@mui/icons-material'

interface CategoryItem {
  name: string
  path: string
  image?: string
}

interface MegaMenuCategory {
  title: string
  items: CategoryItem[]
}

interface NavItem {
  label: string
  path: string
  megaMenu?: {
    categories: MegaMenuCategory[]
    featured?: {
      title: string
      image: string
      link: string
    }
  }
}

const navItems: NavItem[] = [
  {
    label: 'Auctions',
    path: '/auctions',
    megaMenu: {
      categories: [
        {
          title: 'By Category',
          items: [
            { name: 'Fine Art', path: '/auctions?category=fine-art' },
            { name: 'Antiques', path: '/auctions?category=antiques' },
            { name: 'Jewelry & Watches', path: '/auctions?category=jewelry' },
            { name: 'Collectibles', path: '/auctions?category=collectibles' },
            { name: 'Furniture', path: '/auctions?category=furniture' },
            { name: 'Wine & Spirits', path: '/auctions?category=wine' },
            { name: 'Fashion & Accessories', path: '/auctions?category=fashion' },
            { name: 'Books & Manuscripts', path: '/auctions?category=books' },
          ],
        },
        {
          title: 'By Status',
          items: [
            { name: 'Ending Today', path: '/auctions?ending=today' },
            { name: 'Ending This Week', path: '/auctions?ending=week' },
            { name: 'Newly Listed', path: '/auctions?sort=new' },
            { name: 'No Reserve', path: '/auctions?reserve=none' },
            { name: 'Buy Now Available', path: '/auctions?buyNow=true' },
          ],
        },
        {
          title: 'Price Range',
          items: [
            { name: 'Under $500', path: '/auctions?maxPrice=500' },
            { name: '$500 - $2,000', path: '/auctions?minPrice=500&maxPrice=2000' },
            { name: '$2,000 - $10,000', path: '/auctions?minPrice=2000&maxPrice=10000' },
            { name: 'Over $10,000', path: '/auctions?minPrice=10000' },
          ],
        },
      ],
      featured: {
        title: 'Featured Collection',
        image: 'https://picsum.photos/400/500?random=mega1',
        link: '/collections/featured',
      },
    },
  },
  {
    label: 'Categories',
    path: '/categories',
    megaMenu: {
      categories: [
        {
          title: 'Art & Paintings',
          items: [
            { name: 'Contemporary Art', path: '/categories/contemporary-art' },
            { name: 'Modern Art', path: '/categories/modern-art' },
            { name: 'Old Masters', path: '/categories/old-masters' },
            { name: 'Photography', path: '/categories/photography' },
            { name: 'Prints & Multiples', path: '/categories/prints' },
            { name: 'Sculptures', path: '/categories/sculptures' },
          ],
        },
        {
          title: 'Luxury & Fashion',
          items: [
            { name: 'Designer Handbags', path: '/categories/handbags' },
            { name: 'Fine Jewelry', path: '/categories/jewelry' },
            { name: 'Luxury Watches', path: '/categories/watches' },
            { name: 'Vintage Clothing', path: '/categories/vintage-clothing' },
            { name: 'Shoes & Accessories', path: '/categories/shoes' },
          ],
        },
        {
          title: 'Home & Décor',
          items: [
            { name: 'Furniture', path: '/categories/furniture' },
            { name: 'Rugs & Carpets', path: '/categories/rugs' },
            { name: 'Lighting', path: '/categories/lighting' },
            { name: 'Decorative Objects', path: '/categories/decorative' },
            { name: 'Silver & Tableware', path: '/categories/silver' },
          ],
        },
        {
          title: 'Collectibles',
          items: [
            { name: 'Coins & Stamps', path: '/categories/coins' },
            { name: 'Sports Memorabilia', path: '/categories/sports' },
            { name: 'Rare Books', path: '/categories/books' },
            { name: 'Toys & Models', path: '/categories/toys' },
            { name: 'Wine & Whisky', path: '/categories/wine' },
          ],
        },
      ],
    },
  },
  { label: 'How It Works', path: '/how-it-works' },
  { label: 'Sell', path: '/sell' },
]

export const MegaMenu = () => {
  const location = useLocation()
  const [activeMenu, setActiveMenu] = useState<string | null>(null)
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null)
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const handleMouseEnter = (event: React.MouseEvent<HTMLElement>, label: string) => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current)
    }
    setAnchorEl(event.currentTarget)
    setActiveMenu(label)
  }

  const handleMouseLeave = () => {
    timeoutRef.current = setTimeout(() => {
      setActiveMenu(null)
      setAnchorEl(null)
    }, 150)
  }

  const handleMenuMouseEnter = () => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current)
    }
  }

  const handleClose = () => {
    setActiveMenu(null)
    setAnchorEl(null)
  }

  useEffect(() => {
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current)
      }
    }
  }, [])

  const activeItem = navItems.find((item) => item.label === activeMenu)

  return (
    <Box sx={{ display: { xs: 'none', md: 'flex' }, alignItems: 'center', gap: 1 }}>
      {navItems.map((item) => (
        <Box
          key={item.label}
          onMouseEnter={(e) => item.megaMenu && handleMouseEnter(e, item.label)}
          onMouseLeave={handleMouseLeave}
        >
          <Button
            component={Link}
            to={item.path}
            endIcon={item.megaMenu ? <KeyboardArrowDown sx={{ fontSize: 18 }} /> : undefined}
            sx={{
              color: '#FAFAF9',
              textTransform: 'none',
              fontWeight: 500,
              px: 2,
              py: 1,
              borderRadius: 0,
              opacity: location.pathname.startsWith(item.path) ? 1 : 0.85,
              borderBottom: activeMenu === item.label ? '2px solid #CA8A04' : '2px solid transparent',
              '&:hover': {
                bgcolor: 'transparent',
                opacity: 1,
              },
            }}
          >
            {item.label}
          </Button>
        </Box>
      ))}

      <Popper
        open={Boolean(activeMenu && activeItem?.megaMenu)}
        anchorEl={anchorEl}
        placement="bottom-start"
        transition
        disablePortal
        sx={{ zIndex: 1300, width: '100%', left: '0 !important', right: 0 }}
        modifiers={[
          {
            name: 'offset',
            options: {
              offset: [0, 0],
            },
          },
        ]}
      >
        {({ TransitionProps }) => (
          <Grow {...TransitionProps} style={{ transformOrigin: 'top center' }}>
            <Paper
              elevation={0}
              onMouseEnter={handleMenuMouseEnter}
              onMouseLeave={handleMouseLeave}
              sx={{
                width: '100vw',
                maxWidth: '100vw',
                ml: '-50vw',
                left: '50%',
                position: 'relative',
                borderRadius: 0,
                bgcolor: '#FAFAF9',
                borderTop: '1px solid rgba(68,64,60,0.1)',
                boxShadow: '0 8px 30px rgba(0,0,0,0.08)',
              }}
            >
              <ClickAwayListener onClickAway={handleClose}>
                <Container maxWidth="xl" sx={{ py: 5 }}>
                  <Grid container spacing={4}>
                    {activeItem?.megaMenu?.categories.map((category, idx) => (
                      <Grid size={{ xs: 12, sm: 6, md: activeItem.megaMenu?.featured ? 2.4 : 3 }} key={idx}>
                        <Typography
                          variant="subtitle2"
                          sx={{
                            fontWeight: 600,
                            color: '#1C1917',
                            textTransform: 'uppercase',
                            letterSpacing: 1,
                            fontSize: '0.7rem',
                            mb: 2,
                          }}
                        >
                          {category.title}
                        </Typography>
                        <Box component="ul" sx={{ listStyle: 'none', p: 0, m: 0 }}>
                          {category.items.map((subItem) => (
                            <Box component="li" key={subItem.path} sx={{ mb: 1.5 }}>
                              <Typography
                                component={Link}
                                to={subItem.path}
                                onClick={handleClose}
                                sx={{
                                  color: '#44403C',
                                  textDecoration: 'none',
                                  fontSize: '0.9rem',
                                  transition: 'color 0.2s ease',
                                  cursor: 'pointer',
                                  '&:hover': {
                                    color: '#CA8A04',
                                  },
                                }}
                              >
                                {subItem.name}
                              </Typography>
                            </Box>
                          ))}
                        </Box>
                      </Grid>
                    ))}

                    {activeItem?.megaMenu?.featured && (
                      <Grid size={{ xs: 12, md: 3 }}>
                        <Box
                          component={Link}
                          to={activeItem.megaMenu.featured.link}
                          onClick={handleClose}
                          sx={{
                            display: 'block',
                            position: 'relative',
                            overflow: 'hidden',
                            textDecoration: 'none',
                            '&:hover img': {
                              transform: 'scale(1.03)',
                            },
                          }}
                        >
                          <Box
                            component="img"
                            src={activeItem.megaMenu.featured.image}
                            alt={activeItem.megaMenu.featured.title}
                            sx={{
                              width: '100%',
                              height: 280,
                              objectFit: 'cover',
                              transition: 'transform 0.4s ease',
                            }}
                          />
                          <Box
                            sx={{
                              position: 'absolute',
                              bottom: 0,
                              left: 0,
                              right: 0,
                              p: 2.5,
                              background: 'linear-gradient(transparent, rgba(28,25,23,0.8))',
                            }}
                          >
                            <Typography
                              variant="subtitle1"
                              sx={{
                                color: '#FAFAF9',
                                fontWeight: 500,
                              }}
                            >
                              {activeItem.megaMenu.featured.title}
                            </Typography>
                            <Typography
                              variant="caption"
                              sx={{
                                color: '#CA8A04',
                                textTransform: 'uppercase',
                                letterSpacing: 1,
                              }}
                            >
                              Shop Now →
                            </Typography>
                          </Box>
                        </Box>
                      </Grid>
                    )}
                  </Grid>

                  <Box
                    sx={{
                      mt: 4,
                      pt: 3,
                      borderTop: '1px solid rgba(68,64,60,0.1)',
                      display: 'flex',
                      gap: 4,
                    }}
                  >
                    <Typography
                      component={Link}
                      to={activeItem?.path || '/'}
                      onClick={handleClose}
                      sx={{
                        color: '#1C1917',
                        textDecoration: 'none',
                        fontWeight: 500,
                        fontSize: '0.85rem',
                        '&:hover': { color: '#CA8A04' },
                      }}
                    >
                      View All {activeItem?.label} →
                    </Typography>
                  </Box>
                </Container>
              </ClickAwayListener>
            </Paper>
          </Grow>
        )}
      </Popper>
    </Box>
  )
}
