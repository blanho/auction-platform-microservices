import { Box } from '@mui/material'
import {
  HeroSection,
  FeaturedAuctionsSection,
  GiftGuideSection,
  EditorialSection,
  FreshArrivalsSection,
  CategoriesSection,
  FeaturesSection,
  TestimonialsSection,
  CTASection,
  ExclusiveOffersBar,
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