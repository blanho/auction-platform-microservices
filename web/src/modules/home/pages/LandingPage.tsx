import { useState } from 'react'
import { Link } from 'react-router-dom'
import {
  Box,
  Container,
  Typography,
  Button,
  Grid,
  Card,
  CardContent,
  CardMedia,
  IconButton,
  Chip,
  Stack,
  Divider,
  Skeleton,
} from '@mui/material'
import {
  ArrowForward,
  Gavel,
  Timer,
  TrendingUp,
  Shield,
  LocalShipping,
  Favorite,
  FavoriteBorder,
  KeyboardArrowRight,
} from '@mui/icons-material'
import { useFeaturedAuctions } from '../../auctions/hooks'
import { formatTimeLeft, formatCurrency } from '../../auctions/utils'

const featuredCategories = [
  { id: '1', name: 'Fine Art', image: '/images/categories/fine-art.jpg', count: 234 },
  { id: '2', name: 'Antiques', image: '/images/categories/antiques.jpg', count: 156 },
  { id: '3', name: 'Jewelry', image: '/images/categories/jewelry.jpg', count: 89 },
  { id: '4', name: 'Collectibles', image: '/images/categories/collectibles.jpg', count: 312 },
  { id: '5', name: 'Watches', image: '/images/categories/watches.jpg', count: 67 },
  { id: '6', name: 'Wine & Spirits', image: '/images/categories/wine.jpg', count: 45 },
]

const endingSoonAuctions = [
  {
    id: '5',
    title: 'Rare First Edition Hemingway',
    currentBid: 1850,
    endTime: new Date(Date.now() + 2 * 60 * 60 * 1000).toISOString(),
    image: '/images/auctions/book.jpg',
  },
  {
    id: '6',
    title: 'Georgian Silver Tea Service',
    currentBid: 3400,
    endTime: new Date(Date.now() + 4 * 60 * 60 * 1000).toISOString(),
    image: '/images/auctions/tea-service.jpg',
  },
  {
    id: '7',
    title: 'Signed Sports Memorabilia Collection',
    currentBid: 950,
    endTime: new Date(Date.now() + 1 * 60 * 60 * 1000).toISOString(),
    image: '/images/auctions/memorabilia.jpg',
  },
]

export const LandingPage = () => {
  const [favorites, setFavorites] = useState<Set<string>>(new Set())
  const { data: featuredData, isLoading: featuredLoading } = useFeaturedAuctions(4)
  const featuredAuctions = featuredData?.items ?? []

  const toggleFavorite = (id: string) => {
    setFavorites((prev) => {
      const next = new Set(prev)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })
  }

  return (
    <Box sx={{ bgcolor: '#FAFAF9', minHeight: '100vh' }}>
      <Box
        sx={{
          position: 'relative',
          height: { xs: '90vh', md: '85vh' },
          overflow: 'hidden',
          bgcolor: '#1C1917',
        }}
      >
        <Box
          sx={{
            position: 'absolute',
            inset: 0,
            backgroundImage: 'url(/images/hero-bg.jpg)',
            backgroundSize: 'cover',
            backgroundPosition: 'center',
            opacity: 0.4,
          }}
        />
        <Box
          sx={{
            position: 'absolute',
            inset: 0,
            background: 'linear-gradient(to right, rgba(28,25,23,0.95) 0%, rgba(28,25,23,0.7) 50%, rgba(28,25,23,0.4) 100%)',
          }}
        />

        <Container
          maxWidth="xl"
          sx={{
            position: 'relative',
            height: '100%',
            display: 'flex',
            flexDirection: 'column',
            justifyContent: 'center',
            pt: { xs: 8, md: 0 },
          }}
        >
          <Grid container spacing={4} alignItems="center">
            <Grid size={{ xs: 12, md: 7, lg: 6 }}>
              <Typography
                variant="overline"
                sx={{
                  color: '#CA8A04',
                  letterSpacing: 4,
                  fontWeight: 500,
                  mb: 2,
                  display: 'block',
                }}
              >
                CURATED AUCTIONS
              </Typography>
              <Typography
                variant="h1"
                sx={{
                  color: '#FAFAF9',
                  fontSize: { xs: '2.5rem', sm: '3.5rem', md: '4rem', lg: '4.5rem' },
                  fontWeight: 300,
                  lineHeight: 1.1,
                  mb: 3,
                  letterSpacing: '-0.02em',
                }}
              >
                Discover
                <Box component="span" sx={{ display: 'block', fontWeight: 500 }}>
                  Extraordinary Finds
                </Box>
              </Typography>
              <Typography
                variant="h6"
                sx={{
                  color: 'rgba(250,250,249,0.7)',
                  fontWeight: 400,
                  lineHeight: 1.6,
                  mb: 4,
                  maxWidth: 500,
                }}
              >
                From rare antiques to contemporary art, explore our expertly curated
                auctions featuring authenticated pieces from trusted sellers worldwide.
              </Typography>

              <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', mb: 4 }}>
                <Button
                  variant="contained"
                  size="large"
                  endIcon={<ArrowForward />}
                  component={Link}
                  to="/auctions"
                  sx={{
                    bgcolor: '#CA8A04',
                    color: '#FAFAF9',
                    px: 4,
                    py: 1.5,
                    fontSize: '1rem',
                    fontWeight: 500,
                    textTransform: 'none',
                    borderRadius: 0,
                    '&:hover': {
                      bgcolor: '#A16207',
                    },
                  }}
                >
                  Explore Auctions
                </Button>
                <Button
                  variant="outlined"
                  size="large"
                  component={Link}
                  to="/register"
                  sx={{
                    borderColor: 'rgba(250,250,249,0.3)',
                    color: '#FAFAF9',
                    px: 4,
                    py: 1.5,
                    fontSize: '1rem',
                    fontWeight: 500,
                    textTransform: 'none',
                    borderRadius: 0,
                    '&:hover': {
                      borderColor: '#FAFAF9',
                      bgcolor: 'rgba(250,250,249,0.05)',
                    },
                  }}
                >
                  Start Selling
                </Button>
              </Box>

              <Box sx={{ display: 'flex', gap: 6, flexWrap: 'wrap' }}>
                {[
                  { value: '50K+', label: 'Items Sold' },
                  { value: '$12M+', label: 'Total Sales' },
                  { value: '15K+', label: 'Happy Bidders' },
                ].map((stat) => (
                  <Box key={stat.label}>
                    <Typography
                      variant="h4"
                      sx={{ color: '#CA8A04', fontWeight: 600, mb: 0.5 }}
                    >
                      {stat.value}
                    </Typography>
                    <Typography
                      variant="body2"
                      sx={{ color: 'rgba(250,250,249,0.6)', textTransform: 'uppercase', letterSpacing: 1 }}
                    >
                      {stat.label}
                    </Typography>
                  </Box>
                ))}
              </Box>
            </Grid>
          </Grid>
        </Container>

        <Box
          sx={{
            position: 'absolute',
            bottom: 40,
            left: '50%',
            transform: 'translateX(-50%)',
            display: { xs: 'none', md: 'flex' },
            flexDirection: 'column',
            alignItems: 'center',
            color: 'rgba(250,250,249,0.5)',
            cursor: 'pointer',
            transition: 'color 0.2s',
            '&:hover': { color: '#FAFAF9' },
          }}
          onClick={() => window.scrollTo({ top: window.innerHeight * 0.85, behavior: 'smooth' })}
        >
          <Typography variant="caption" sx={{ mb: 1, letterSpacing: 2, textTransform: 'uppercase' }}>
            Scroll to explore
          </Typography>
          <Box
            sx={{
              width: 24,
              height: 40,
              border: '2px solid currentColor',
              borderRadius: 12,
              display: 'flex',
              justifyContent: 'center',
              pt: 1,
            }}
          >
            <Box
              sx={{
                width: 4,
                height: 8,
                bgcolor: 'currentColor',
                borderRadius: 2,
                animation: 'scroll 1.5s infinite',
                '@keyframes scroll': {
                  '0%': { opacity: 1, transform: 'translateY(0)' },
                  '100%': { opacity: 0, transform: 'translateY(12px)' },
                },
              }}
            />
          </Box>
        </Box>
      </Box>

      <Box sx={{ py: { xs: 8, md: 12 }, bgcolor: '#FAFAF9' }}>
        <Container maxWidth="xl">
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-end', mb: 6 }}>
            <Box>
              <Typography
                variant="overline"
                sx={{ color: '#44403C', letterSpacing: 3, display: 'block', mb: 1 }}
              >
                BROWSE BY
              </Typography>
              <Typography variant="h3" sx={{ color: '#1C1917', fontWeight: 400 }}>
                Categories
              </Typography>
            </Box>
            <Button
              endIcon={<KeyboardArrowRight />}
              component={Link}
              to="/categories"
              sx={{
                color: '#1C1917',
                textTransform: 'none',
                fontWeight: 500,
                '&:hover': { bgcolor: 'transparent', textDecoration: 'underline' },
              }}
            >
              View All
            </Button>
          </Box>

          <Grid container spacing={2}>
            {featuredCategories.map((category, index) => (
              <Grid size={{ xs: 6, sm: 4, md: 2 }} key={category.id}>
                <Box
                  component={Link}
                  to={`/auctions?category=${category.id}`}
                  sx={{
                    display: 'block',
                    position: 'relative',
                    aspectRatio: '1',
                    overflow: 'hidden',
                    cursor: 'pointer',
                    textDecoration: 'none',
                    '&:hover img': { transform: 'scale(1.05)' },
                    '&:hover .overlay': { bgcolor: 'rgba(28,25,23,0.4)' },
                  }}
                >
                  <Box
                    component="img"
                    src={category.image || `https://picsum.photos/400/400?random=${index}`}
                    alt={category.name}
                    sx={{
                      width: '100%',
                      height: '100%',
                      objectFit: 'cover',
                      transition: 'transform 0.4s ease',
                    }}
                  />
                  <Box
                    className="overlay"
                    sx={{
                      position: 'absolute',
                      inset: 0,
                      bgcolor: 'rgba(28,25,23,0.3)',
                      transition: 'background-color 0.3s ease',
                      display: 'flex',
                      flexDirection: 'column',
                      justifyContent: 'flex-end',
                      p: 2,
                    }}
                  >
                    <Typography variant="subtitle1" sx={{ color: '#FAFAF9', fontWeight: 500 }}>
                      {category.name}
                    </Typography>
                    <Typography variant="caption" sx={{ color: 'rgba(250,250,249,0.7)' }}>
                      {category.count} items
                    </Typography>
                  </Box>
                </Box>
              </Grid>
            ))}
          </Grid>
        </Container>
      </Box>

      <Box sx={{ py: { xs: 8, md: 12 }, bgcolor: '#FFFFFF' }}>
        <Container maxWidth="xl">
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-end', mb: 6 }}>
            <Box>
              <Typography
                variant="overline"
                sx={{ color: '#44403C', letterSpacing: 3, display: 'block', mb: 1 }}
              >
                FEATURED
              </Typography>
              <Typography variant="h3" sx={{ color: '#1C1917', fontWeight: 400 }}>
                Live Auctions
              </Typography>
            </Box>
            <Button
              endIcon={<KeyboardArrowRight />}
              component={Link}
              to="/auctions"
              sx={{
                color: '#1C1917',
                textTransform: 'none',
                fontWeight: 500,
                '&:hover': { bgcolor: 'transparent', textDecoration: 'underline' },
              }}
            >
              View All Auctions
            </Button>
          </Box>

          <Grid container spacing={3}>
            {featuredLoading ? (
              Array.from({ length: 4 }).map((_, index) => (
                <Grid size={{ xs: 12, sm: 6, lg: 3 }} key={index}>
                  <Card elevation={0} sx={{ bgcolor: 'transparent' }}>
                    <Skeleton variant="rectangular" sx={{ aspectRatio: '1', mb: 2 }} />
                    <Skeleton variant="text" width="40%" />
                    <Skeleton variant="text" />
                    <Skeleton variant="text" width="60%" />
                  </Card>
                </Grid>
              ))
            ) : featuredAuctions.length === 0 ? (
              <Grid size={{ xs: 12 }}>
                <Typography color="text.secondary" align="center" sx={{ py: 8 }}>
                  No featured auctions available
                </Typography>
              </Grid>
            ) : (
              featuredAuctions.map((auction, index) => (
              <Grid size={{ xs: 12, sm: 6, lg: 3 }} key={auction.id}>
                <Card
                  elevation={0}
                  sx={{
                    bgcolor: 'transparent',
                    cursor: 'pointer',
                    '&:hover .auction-image': { transform: 'scale(1.02)' },
                  }}
                >
                  <Box sx={{ position: 'relative', overflow: 'hidden', mb: 2 }}>
                    <CardMedia
                      component="img"
                      className="auction-image"
                      image={auction.primaryImageUrl || `https://picsum.photos/600/600?random=${index + 10}`}
                      alt={auction.title}
                      sx={{
                        aspectRatio: '1',
                        objectFit: 'cover',
                        transition: 'transform 0.4s ease',
                      }}
                    />
                    <IconButton
                      onClick={(e) => {
                        e.preventDefault()
                        toggleFavorite(auction.id)
                      }}
                      sx={{
                        position: 'absolute',
                        top: 12,
                        right: 12,
                        bgcolor: 'rgba(255,255,255,0.9)',
                        '&:hover': { bgcolor: '#FFFFFF' },
                      }}
                    >
                      {favorites.has(auction.id) ? (
                        <Favorite sx={{ color: '#DC2626' }} />
                      ) : (
                        <FavoriteBorder sx={{ color: '#44403C' }} />
                      )}
                    </IconButton>
                    <Chip
                      icon={<Timer sx={{ fontSize: 16 }} />}
                      label={formatTimeLeft(auction.endTime)}
                      size="small"
                      sx={{
                        position: 'absolute',
                        bottom: 12,
                        left: 12,
                        bgcolor: 'rgba(28,25,23,0.85)',
                        color: '#FAFAF9',
                        fontWeight: 500,
                        '& .MuiChip-icon': { color: '#CA8A04' },
                      }}
                    />
                  </Box>
                  <CardContent sx={{ p: 0 }}>
                    <Typography variant="caption" sx={{ color: '#44403C', mb: 0.5, display: 'block' }}>
                      {auction.categoryName}
                    </Typography>
                    <Typography
                      variant="subtitle1"
                      component={Link}
                      to={`/auctions/${auction.id}`}
                      sx={{
                        color: '#1C1917',
                        fontWeight: 500,
                        textDecoration: 'none',
                        display: 'block',
                        mb: 1.5,
                        lineHeight: 1.4,
                        '&:hover': { color: '#CA8A04' },
                      }}
                    >
                      {auction.title}
                    </Typography>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline' }}>
                      <Box>
                        <Typography variant="caption" sx={{ color: '#44403C', display: 'block' }}>
                          Current Bid
                        </Typography>
                        <Typography variant="h6" sx={{ color: '#1C1917', fontWeight: 600 }}>
                          {formatCurrency(auction.currentBid || auction.startingPrice)}
                        </Typography>
                      </Box>
                      <Typography variant="caption" sx={{ color: '#44403C' }}>
                        {auction.bidCount} bids
                      </Typography>
                    </Box>
                  </CardContent>
                </Card>
              </Grid>
            )))}
          </Grid>
        </Container>
      </Box>

      <Box sx={{ py: { xs: 8, md: 12 }, bgcolor: '#1C1917' }}>
        <Container maxWidth="xl">
          <Grid container spacing={6} alignItems="center">
            <Grid size={{ xs: 12, md: 6 }}>
              <Typography
                variant="overline"
                sx={{ color: '#CA8A04', letterSpacing: 3, display: 'block', mb: 2 }}
              >
                ENDING SOON
              </Typography>
              <Typography variant="h3" sx={{ color: '#FAFAF9', fontWeight: 400, mb: 3 }}>
                Don't Miss These
              </Typography>
              <Typography variant="body1" sx={{ color: 'rgba(250,250,249,0.7)', mb: 4, maxWidth: 400 }}>
                These auctions are closing soon. Place your bid now before time runs out.
              </Typography>
              <Button
                variant="outlined"
                endIcon={<ArrowForward />}
                component={Link}
                to="/auctions?sort=ending-soon"
                sx={{
                  borderColor: '#CA8A04',
                  color: '#CA8A04',
                  px: 4,
                  py: 1.5,
                  textTransform: 'none',
                  borderRadius: 0,
                  '&:hover': {
                    borderColor: '#CA8A04',
                    bgcolor: 'rgba(202,138,4,0.1)',
                  },
                }}
              >
                View All Ending Soon
              </Button>
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <Stack spacing={2}>
                {endingSoonAuctions.map((auction, index) => (
                  <Box
                    key={auction.id}
                    component={Link}
                    to={`/auctions/${auction.id}`}
                    sx={{
                      display: 'flex',
                      gap: 3,
                      p: 2,
                      bgcolor: 'rgba(250,250,249,0.05)',
                      textDecoration: 'none',
                      cursor: 'pointer',
                      transition: 'background-color 0.2s',
                      '&:hover': { bgcolor: 'rgba(250,250,249,0.1)' },
                    }}
                  >
                    <Box
                      component="img"
                      src={auction.image || `https://picsum.photos/120/120?random=${index + 20}`}
                      alt={auction.title}
                      sx={{ width: 80, height: 80, objectFit: 'cover' }}
                    />
                    <Box sx={{ flex: 1, minWidth: 0 }}>
                      <Typography
                        variant="subtitle2"
                        sx={{
                          color: '#FAFAF9',
                          fontWeight: 500,
                          mb: 0.5,
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                          whiteSpace: 'nowrap',
                        }}
                      >
                        {auction.title}
                      </Typography>
                      <Typography variant="body2" sx={{ color: '#CA8A04', fontWeight: 600 }}>
                        {formatCurrency(auction.currentBid)}
                      </Typography>
                    </Box>
                    <Box sx={{ textAlign: 'right' }}>
                      <Chip
                        label={formatTimeLeft(auction.endTime)}
                        size="small"
                        sx={{
                          bgcolor: '#DC2626',
                          color: '#FAFAF9',
                          fontWeight: 600,
                          animation: 'pulse 2s infinite',
                          '@keyframes pulse': {
                            '0%, 100%': { opacity: 1 },
                            '50%': { opacity: 0.7 },
                          },
                        }}
                      />
                    </Box>
                  </Box>
                ))}
              </Stack>
            </Grid>
          </Grid>
        </Container>
      </Box>

      <Box sx={{ py: { xs: 8, md: 12 }, bgcolor: '#FAFAF9' }}>
        <Container maxWidth="xl">
          <Box sx={{ textAlign: 'center', mb: 8 }}>
            <Typography
              variant="overline"
              sx={{ color: '#44403C', letterSpacing: 3, display: 'block', mb: 1 }}
            >
              WHY CHOOSE US
            </Typography>
            <Typography variant="h3" sx={{ color: '#1C1917', fontWeight: 400 }}>
              The Trusted Auction Experience
            </Typography>
          </Box>

          <Grid container spacing={4}>
            {[
              {
                icon: <Shield sx={{ fontSize: 40 }} />,
                title: 'Authenticated Items',
                description: 'Every item is verified by our expert team before listing. Shop with confidence.',
              },
              {
                icon: <Gavel sx={{ fontSize: 40 }} />,
                title: 'Fair Bidding',
                description: 'Transparent auction process with real-time updates. No hidden fees or last-minute surprises.',
              },
              {
                icon: <LocalShipping sx={{ fontSize: 40 }} />,
                title: 'Secure Shipping',
                description: 'Insured delivery worldwide. Professional packaging for delicate items.',
              },
              {
                icon: <TrendingUp sx={{ fontSize: 40 }} />,
                title: 'Seller Success',
                description: 'Reach millions of collectors. Low commission rates with premium seller support.',
              },
            ].map((feature, index) => (
              <Grid size={{ xs: 12, sm: 6, md: 3 }} key={index}>
                <Box sx={{ textAlign: 'center' }}>
                  <Box sx={{ color: '#CA8A04', mb: 2 }}>{feature.icon}</Box>
                  <Typography variant="h6" sx={{ color: '#1C1917', fontWeight: 500, mb: 1 }}>
                    {feature.title}
                  </Typography>
                  <Typography variant="body2" sx={{ color: '#44403C', lineHeight: 1.7 }}>
                    {feature.description}
                  </Typography>
                </Box>
              </Grid>
            ))}
          </Grid>
        </Container>
      </Box>

      <Box sx={{ py: { xs: 8, md: 10 }, bgcolor: '#FFFFFF' }}>
        <Container maxWidth="md">
          <Box
            sx={{
              textAlign: 'center',
              py: 8,
              px: 4,
              bgcolor: '#1C1917',
            }}
          >
            <Typography variant="h4" sx={{ color: '#FAFAF9', fontWeight: 400, mb: 2 }}>
              Ready to start bidding?
            </Typography>
            <Typography variant="body1" sx={{ color: 'rgba(250,250,249,0.7)', mb: 4 }}>
              Join thousands of collectors and discover your next treasure.
            </Typography>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center', flexWrap: 'wrap' }}>
              <Button
                variant="contained"
                size="large"
                component={Link}
                to="/register"
                sx={{
                  bgcolor: '#CA8A04',
                  color: '#FAFAF9',
                  px: 5,
                  py: 1.5,
                  textTransform: 'none',
                  borderRadius: 0,
                  fontWeight: 500,
                  '&:hover': { bgcolor: '#A16207' },
                }}
              >
                Create Account
              </Button>
              <Button
                variant="outlined"
                size="large"
                component={Link}
                to="/auctions"
                sx={{
                  borderColor: 'rgba(250,250,249,0.3)',
                  color: '#FAFAF9',
                  px: 5,
                  py: 1.5,
                  textTransform: 'none',
                  borderRadius: 0,
                  fontWeight: 500,
                  '&:hover': {
                    borderColor: '#FAFAF9',
                    bgcolor: 'rgba(250,250,249,0.05)',
                  },
                }}
              >
                Browse Auctions
              </Button>
            </Box>
          </Box>
        </Container>
      </Box>

      <Box
        component="footer"
        sx={{
          py: 8,
          bgcolor: '#0C0A09',
          color: '#FAFAF9',
        }}
      >
        <Container maxWidth="xl">
          <Grid container spacing={6}>
            <Grid size={{ xs: 12, md: 4 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 3 }}>
                <Gavel sx={{ fontSize: 28, color: '#CA8A04' }} />
                <Typography variant="h6" sx={{ fontWeight: 600, letterSpacing: 1 }}>
                  AUCTION
                </Typography>
              </Box>
              <Typography variant="body2" sx={{ color: 'rgba(250,250,249,0.6)', mb: 3, maxWidth: 300 }}>
                The premier destination for collectors and connoisseurs. Discover authenticated
                treasures from trusted sellers worldwide.
              </Typography>
            </Grid>
            <Grid size={{ xs: 6, sm: 3, md: 2 }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 2, color: '#FAFAF9' }}>
                Explore
              </Typography>
              <Stack spacing={1.5}>
                {['All Auctions', 'Categories', 'Ending Soon', 'New Arrivals'].map((item) => (
                  <Typography
                    key={item}
                    component={Link}
                    to="/auctions"
                    variant="body2"
                    sx={{
                      color: 'rgba(250,250,249,0.6)',
                      textDecoration: 'none',
                      cursor: 'pointer',
                      transition: 'color 0.2s',
                      '&:hover': { color: '#CA8A04' },
                    }}
                  >
                    {item}
                  </Typography>
                ))}
              </Stack>
            </Grid>
            <Grid size={{ xs: 6, sm: 3, md: 2 }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 2, color: '#FAFAF9' }}>
                Sell
              </Typography>
              <Stack spacing={1.5}>
                {['Start Selling', 'Seller Guide', 'Fees & Pricing', 'Success Stories'].map((item) => (
                  <Typography
                    key={item}
                    component={Link}
                    to="/sell"
                    variant="body2"
                    sx={{
                      color: 'rgba(250,250,249,0.6)',
                      textDecoration: 'none',
                      cursor: 'pointer',
                      transition: 'color 0.2s',
                      '&:hover': { color: '#CA8A04' },
                    }}
                  >
                    {item}
                  </Typography>
                ))}
              </Stack>
            </Grid>
            <Grid size={{ xs: 6, sm: 3, md: 2 }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 2, color: '#FAFAF9' }}>
                Support
              </Typography>
              <Stack spacing={1.5}>
                {['Help Center', 'Contact Us', 'Shipping Info', 'Returns'].map((item) => (
                  <Typography
                    key={item}
                    component={Link}
                    to="/help"
                    variant="body2"
                    sx={{
                      color: 'rgba(250,250,249,0.6)',
                      textDecoration: 'none',
                      cursor: 'pointer',
                      transition: 'color 0.2s',
                      '&:hover': { color: '#CA8A04' },
                    }}
                  >
                    {item}
                  </Typography>
                ))}
              </Stack>
            </Grid>
            <Grid size={{ xs: 6, sm: 3, md: 2 }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 2, color: '#FAFAF9' }}>
                Company
              </Typography>
              <Stack spacing={1.5}>
                {['About Us', 'Careers', 'Press', 'Blog'].map((item) => (
                  <Typography
                    key={item}
                    component={Link}
                    to="/about"
                    variant="body2"
                    sx={{
                      color: 'rgba(250,250,249,0.6)',
                      textDecoration: 'none',
                      cursor: 'pointer',
                      transition: 'color 0.2s',
                      '&:hover': { color: '#CA8A04' },
                    }}
                  >
                    {item}
                  </Typography>
                ))}
              </Stack>
            </Grid>
          </Grid>

          <Divider sx={{ borderColor: 'rgba(250,250,249,0.1)', my: 6 }} />

          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}>
            <Typography variant="caption" sx={{ color: 'rgba(250,250,249,0.4)' }}>
              © {new Date().getFullYear()} Auction Platform. All rights reserved.
            </Typography>
            <Box sx={{ display: 'flex', gap: 3 }}>
              {['Privacy Policy', 'Terms of Service', 'Cookie Policy'].map((item) => (
                <Typography
                  key={item}
                  variant="caption"
                  sx={{
                    color: 'rgba(250,250,249,0.4)',
                    cursor: 'pointer',
                    '&:hover': { color: '#FAFAF9' },
                  }}
                >
                  {item}
                </Typography>
              ))}
            </Box>
          </Box>
        </Container>
      </Box>
    </Box>
  )
}
