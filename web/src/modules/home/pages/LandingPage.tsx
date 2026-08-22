import { Box } from '@mui/material'
import {
  CategoriesSection,
  CTASection,
  EditorialSection,
  ExclusiveOffersBar,
  FeaturedAuctionsSection,
  FeaturesSection,
  FreshArrivalsSection,
  GiftGuideSection,
  HeroSection,
  TestimonialsSection,
} from '../components'

export const LandingPage = () => {
  return (
    <Box sx={{ bgcolor: '#FFFFFF', minHeight: '100vh', overflow: 'hidden' }}>
      <HeroSection />
      <GiftGuideSection />
      <FreshArrivalsSection />
      <EditorialSection />
      <FeaturedAuctionsSection />
      <CategoriesSection />
      <FeaturesSection />
      <TestimonialsSection />
      <CTASection />
      <ExclusiveOffersBar />
    </Box>
  )
}
