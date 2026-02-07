import { useState, useRef, useEffect, useMemo, useCallback } from 'react'
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
import { palette } from '@/shared/theme/tokens'
import { useCategoriesTree } from '@/modules/auctions/hooks/useCategories'
import type { Category } from '@/modules/auctions/hooks/useCategories'

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

export const MegaMenu = () => {
  const location = useLocation()
  const [activeMenu, setActiveMenu] = useState<string | null>(null)
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null)
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const { data: categoriesTree } = useCategoriesTree()

  const activeCategories = useMemo(() => {
    return (categoriesTree ?? []).filter((category) => category.isActive)
  }, [categoriesTree])

  const topCategories = useMemo(() => {
    return activeCategories.filter((category) => !category.parentId && !category.parentCategoryId)
  }, [activeCategories])

  const buildCategoryLink = useCallback(
    (categoryId: string) => `/auctions?categoryId=${categoryId}`,
    []
  )

  const flattenCategories = useMemo(() => {
    const items: Category[] = []
    topCategories.forEach((category) => {
      if (category.children && category.children.length > 0) {
        items.push(...category.children.filter((child) => child.isActive))
      } else {
        items.push(category)
      }
    })
    return items
  }, [topCategories])

  const featuredCategory = useMemo(() => {
    return activeCategories.find((category) => Boolean(category.imageUrl))
  }, [activeCategories])

  const auctionsMenuCategories: MegaMenuCategory[] = useMemo(() => {
    const byCategoryItems = flattenCategories.slice(0, 8).map((category) => ({
      name: category.name,
      path: buildCategoryLink(category.id),
    }))

    return [
      {
        title: 'By Category',
        items: byCategoryItems,
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
    ]
  }, [flattenCategories, buildCategoryLink])

  const categoriesMenuCategories: MegaMenuCategory[] = useMemo(() => {
    return topCategories.slice(0, 4).map((category) => {
      const items = (category.children ?? []).filter((child) => child.isActive)
      const mappedItems = (items.length ? items : [category]).slice(0, 6).map((item) => ({
        name: item.name,
        path: buildCategoryLink(item.id),
      }))

      return {
        title: category.name,
        items: mappedItems,
      }
    })
  }, [topCategories, buildCategoryLink])

  const navItems: NavItem[] = useMemo(() => {
    const auctionsFeatured = featuredCategory?.imageUrl
      ? {
          title: featuredCategory.name,
          image: featuredCategory.imageUrl,
          link: buildCategoryLink(featuredCategory.id),
        }
      : undefined

    return [
      {
        label: 'Auctions',
        path: '/auctions',
        megaMenu: {
          categories: auctionsMenuCategories,
          featured: auctionsFeatured,
        },
      },
      {
        label: 'Categories',
        path: '/categories',
        megaMenu: {
          categories: categoriesMenuCategories,
        },
      },
      { label: 'How It Works', path: '/how-it-works' },
      { label: 'Sell', path: '/sell' },
    ]
  }, [auctionsMenuCategories, categoriesMenuCategories, featuredCategory, buildCategoryLink])

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
              color: palette.neutral[50],
              textTransform: 'none',
              fontWeight: 500,
              px: 2,
              py: 1,
              borderRadius: 0,
              opacity: location.pathname.startsWith(item.path) ? 1 : 0.85,
              borderBottom:
                activeMenu === item.label
                  ? `2px solid ${palette.brand.primary}`
                  : '2px solid transparent',
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
        sx={{ zIndex: 1300, width: '100%', left: 0, right: 0 }}
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
                bgcolor: palette.neutral[50],
                borderTop: `1px solid rgba(68,64,60,0.1)`,
                boxShadow: '0 8px 30px rgba(0,0,0,0.08)',
              }}
            >
              <ClickAwayListener onClickAway={handleClose}>
                <Container maxWidth="xl" sx={{ py: 5 }}>
                  <Grid container spacing={4}>
                    {activeItem?.megaMenu?.categories.map((category) => (
                      <Grid
                        size={{ xs: 12, sm: 6, md: activeItem.megaMenu?.featured ? 2.4 : 3 }}
                        key={category.title}
                      >
                        <Typography
                          variant="subtitle2"
                          sx={{
                            fontWeight: 600,
                            color: palette.neutral[900],
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
                                  color: palette.neutral[700],
                                  textDecoration: 'none',
                                  fontSize: '0.9rem',
                                  transition: 'color 0.2s ease',
                                  cursor: 'pointer',
                                  '&:hover': {
                                    color: palette.brand.primary,
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
                                color: palette.neutral[50],
                                fontWeight: 500,
                              }}
                            >
                              {activeItem.megaMenu.featured.title}
                            </Typography>
                            <Typography
                              variant="caption"
                              sx={{
                                color: palette.brand.primary,
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
                        color: palette.neutral[900],
                        textDecoration: 'none',
                        fontWeight: 500,
                        fontSize: '0.85rem',
                        '&:hover': { color: palette.brand.primary },
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
